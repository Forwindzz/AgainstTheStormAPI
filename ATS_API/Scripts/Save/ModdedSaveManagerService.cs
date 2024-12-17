using System;
using System.Collections.Generic;
using System.IO;
using ATS_API.Helpers;
using ATS_API.Localization;
using Cysharp.Threading.Tasks;
using Eremite;
using Eremite.Model.SaveSupport;
using Eremite.Services;
using Eremite.View.SaveSupport;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements.StyleSheets.Syntax;

namespace ATS_API.SaveLoading;

[HarmonyPatch]
internal class ModdedSaveManagerService : Service
{
    public static ModdedSaveManagerService Instance { get; private set; }
    // after loading listeners
    public static Dictionary<string, SafeAction<ModSaveData, SaveFileState>> LoadedSaveDataListeners = new();
    // pre-save listeners
    public static Dictionary<string, SafeAction<ModSaveData, SaveFileState>> PreSaveSaveDataListeners = new();

    public static Dictionary<string, SafeFunc<ErrorData, UniTask<ModSaveData>>> ErrorHandlers = new();

    public static List<Dictionary<string, SafeAction<ModSaveData, SaveFileState>>> LoadedTypeListeners = new List<Dictionary<string, SafeAction<ModSaveData, SaveFileState>>>()
        {new(),new(),new(),new()};

    public static List<Dictionary<string, SafeAction<ModSaveData, SaveFileState>>> PreSaveTypeListeners = new List<Dictionary<string, SafeAction<ModSaveData, SaveFileState>>>()
        {new(),new(),new(),new()};

    /// <summary>
    /// When first load, set this true.
    /// When this is true, if the user switch the profile, it will keep the previous general data. 
    /// </summary>
    private bool m_hasLoadedGeneral = false;

    internal static void TriggerEvent(
        Dictionary<string, SafeAction<ModSaveData, SaveFileState>> eventListeners,
        SaveFileState state)
    {
        foreach (var pair in eventListeners)
        {
            string guid = pair.Key;
            SafeAction<ModSaveData, SaveFileState> listeners = pair.Value;
            try
            {
                ModSaveData data = ModdedSaveManager.GetSaveData(guid);
                listeners.Invoke(data, state);
            }
            catch (Exception ex)
            {
                Log.Error($"Error when try to invoke listener from mod {guid}");
                Log.Error(ex);
            }
        }
    }

    internal static void TriggerEvent(
        Dictionary<string, SafeAction<ModSaveData, SaveFileState>> eventListeners,
        Func<ModSaveData, SaveData> getValueFunc,
        bool eraseNewTag)
    {
        foreach (var pair in eventListeners)
        {
            string guid = pair.Key;
            SafeAction<ModSaveData, SaveFileState> listeners = pair.Value;
            try
            {
                ModSaveData modSaveData = ModdedSaveManager.GetSaveData(guid);
                SaveData saveData = getValueFunc(modSaveData);
                if (saveData.isNewData)
                {
                    listeners.Invoke(modSaveData, SaveFileState.NewFile);
                    if (eraseNewTag)
                    {
                        saveData.isNewData = false;
                    }
                }
                else
                {
                    listeners.Invoke(modSaveData, SaveFileState.LoadedFile);
                }

            }
            catch (Exception ex)
            {
                Log.Error($"Error when try to invoke listener from mod {guid}");
                Log.Error(ex);
            }
        }
    }

    public override UniTask OnLoading()
    {
        return UniTask.CompletedTask;
    }

    private SafeAction<ModSaveData, SaveFileState> GetTypeModListeners(
        List<Dictionary<string, SafeAction<ModSaveData, SaveFileState>>> ListenersCollection,
        string guid,
        SaveDataType dataType)
    {
        if (GetTypeListeners(ListenersCollection, dataType).TryGetValue(guid, out var listeners))
        {
            return listeners;
        }
        return null;
    }

    private static Dictionary<string, SafeAction<ModSaveData, SaveFileState>> GetTypeListeners(
        List<Dictionary<string, SafeAction<ModSaveData, SaveFileState>>> ListenersCollection,
        SaveDataType dataType)
    {
        return LoadedTypeListeners[(int)dataType];
    }

    public static void AddDataTypeListener(
        List<Dictionary<string, SafeAction<ModSaveData, SaveFileState>>> ListenersCollection,
        SaveDataType dataType,
        string guid,
        Action<ModSaveData, SaveFileState> newListener)
    {
        Dictionary<string, SafeAction<ModSaveData, SaveFileState>> dictionaryListener = GetTypeListeners(ListenersCollection, dataType);
        if (!dictionaryListener.TryGetValue(guid, out var typeModListeners))
        {
            typeModListeners = new SafeAction<ModSaveData, SaveFileState>();
            dictionaryListener[guid] = typeModListeners;
        }
        typeModListeners.AddListener(newListener);
    }

    public static void RemoveDataTypeListener(
        List<Dictionary<string, SafeAction<ModSaveData, SaveFileState>>> ListenersCollection,
        SaveDataType dataType,
        string guid,
        Action<ModSaveData, SaveFileState> removeListener)
    {
        Dictionary<string, SafeAction<ModSaveData, SaveFileState>> dictionaryListener = GetTypeListeners(ListenersCollection, dataType);
        if (dictionaryListener.TryGetValue(guid, out var typeModListeners))
        {
            typeModListeners.RemoveListener(removeListener);
        }
    }

    public string GetSaveFilePath(string path, string guid, SaveDataType dataType)
    {
        return Path.Combine(path, dataType.ToString(), $"{CleanGuid(guid)}.moddedsave");
    }

    private async UniTask LoadAllModdedData()
    {
        foreach (SaveDataType dataType in Enum.GetValues(typeof(SaveDataType)))
        {
            if (dataType == SaveDataType.General)
            {
                await LoadAllTypeModdedData(ProfilesService.GetDefaultFolderPath(), SaveDataType.General);
                continue;
            }
            await LoadAllTypeModdedData(ProfilesService.GetProfileFolderPath(), dataType);
        }
        Plugin.Log.LogInfo($"Loaded all modded data");
    }

    public async UniTask LoadAllTypeModdedData(SaveDataType dataType)
    {
        await LoadAllTypeModdedData(dataType.GetCurrentProfilePath(),dataType);
    }

    public async UniTask LoadAllTypeModdedData(string path, SaveDataType dataType)
    {
        Dictionary<string, SafeAction<ModSaveData, SaveFileState>> listenersDictionary =
            GetTypeListeners(LoadedTypeListeners, dataType);
        foreach (var pair in listenersDictionary)
        {
            string guid = pair.Key;
            await LoadSpecificData(path, guid, dataType);
        }
        Plugin.Log.LogInfo($"Loaded all modded data for {dataType}");
    }

    public async UniTask LoadSpecificData(string path, string guid, SaveDataType dataType)
    {
        string saveFilePath = GetSaveFilePath(path, guid, dataType);
        bool dataLoaded = false;
        SafeAction<ModSaveData, SaveFileState> callback = GetTypeModListeners(LoadedTypeListeners, guid, dataType);
        SafeAction<ModSaveData, SaveFileState> globalCallback = null;
        LoadedSaveDataListeners.TryGetValue(guid, out globalCallback);

        if (File.Exists(saveFilePath))
        {
            try
            {
                // Try loading the file
                string json = File.ReadAllText(saveFilePath);
                JObject saveDataJObject = (JObject)JsonConvert.DeserializeObject(json);
                ModSaveData saveData = (ModSaveData)saveDataJObject.ToObject(typeof(ModSaveData));
                ModdedSaveManager.ModGuidToDataLookup[guid] = saveData;
                dataLoaded = true;
                Plugin.Log.LogInfo($"Loaded save file {saveFilePath}");

                // Tell any listeners (the mod) that we loaded the data
                // If the listener throws an exception handle that too
                await Callback(guid, globalCallback, saveData, SaveFileState.LoadedFile);
                await Callback(guid, callback, saveData, SaveFileState.LoadedFile);
                return;
            }
            catch (Exception e)
            {
                if (dataLoaded)
                {
                    return;
                }

                // Something went wrong when loading the file
                // As the user what they want to do (Use backup, Delete, Go to the discord)
                Plugin.Log.LogError($"Failed to load save file {saveFilePath}");
                Plugin.Log.LogError(e);

                // See if a mod wants to handle the error themselves.
                if (ErrorHandlers.TryGetValue(guid, out SafeFunc<ErrorData, UniTask<ModSaveData>> errorHandler))
                {
                    ErrorData errorData = new ErrorData
                    {
                        exception = e,
                        filePath = saveFilePath
                    };

                    ModSaveData value = await errorHandler.Invoke(errorData);
                    if (value != null)
                    {
                        Plugin.Log.LogInfo($"Manually Handled save file {saveFilePath}");
                        ModdedSaveManager.ModGuidToDataLookup[guid] = value;
                        await Callback(guid, globalCallback, value, SaveFileState.LoadedFile);
                        await Callback(guid, callback, value, SaveFileState.LoadedFile);
                        return;
                    }
                }

                string fileName = Path.GetFileName(saveFilePath);
                ModSaveData handled = await HandleCorruptionTask<ModSaveData>(CreateSaveCorruptionTask(saveFilePath, fileName));
                if (handled != null)
                {
                    ModdedSaveManager.ModGuidToDataLookup[guid] = handled;
                    await Callback(guid, globalCallback, handled, SaveFileState.LoadedFile);
                    await Callback(guid, callback, handled, SaveFileState.LoadedFile);
                    return;
                }
            }
        }

        // If the file doesn't exist, create a new one
        // If the listener throws an exception handle that too
        Plugin.Log.LogInfo($"Creating new save file for {guid}");
        SaveData data = new SaveData();
        data.isNewData = true;
        ModSaveData modSaveData = ModdedSaveManager.GetSaveData(guid);
        modSaveData.SetSaveData(dataType,data);
        await Callback(guid, globalCallback, modSaveData, SaveFileState.NewFile);
        await Callback(guid, callback, modSaveData, SaveFileState.NewFile);
    }

    private async UniTask Callback(string guid, SafeAction<ModSaveData, SaveFileState> callback, ModSaveData saveData, SaveFileState fileState)
    {
        if (callback == null)
        {
            return;
        }
        await callback.Invoke(saveData, fileState, async (Exception e) =>
        {
            await GenericPopupTask.ShowException(guid, "Tried invoking callback after loading a mods save data.", e)
                .WaitForDecisionAsync(new GenericPopupTask.ButtonInfo
                {
                    Key = Keys.Continue_Key.ToLocaText(),
                    OptionKey = Keys.GenericPopup_ContinueAtRisk_Key.ToLocaText(),
                    Type = GenericPopupTask.ButtonTypes.Normal,
                    OnPressed = () => { }
                },
                new GenericPopupTask.ButtonInfo
                {
                    Key = Keys.Quit_Key.ToLocaText(),
                    OptionKey = Keys.GenericPopup_QuitGameAndDisableMod_Key.ToLocaText(),
                    Type = GenericPopupTask.ButtonTypes.CTA,
                    OnPressed = Application.Quit
                }
                );
            return true;
        });
    }

    private SaveCorruptionTask CreateSaveCorruptionTask(string path, string fileDisplayNameKey)
    {
        return new SaveCorruptionTask
        {
            filePath = path,
            fileDisplayNameKey = fileDisplayNameKey,
            backupExist = DoesBackupExist(path),
            backupDate = GetBackupDate(path),
            completionSource = new UniTaskCompletionSource()
        };
    }

    private DateTime GetBackupDate(string path)
    {
        return File.GetLastWriteTime(path + "_backup");
    }

    private bool DoesBackupExist(string path)
    {
        return File.Exists(path + "_backup");
    }

    private async UniTask<T> HandleCorruptionTask<T>(SaveCorruptionTask task)
    {
        await WaitForDecision(task);
        if (task.decision == SaveCorruptionDecision.Backup)
            return LoadBackup<T>(task);
        if (task.decision == SaveCorruptionDecision.Delete)
            return Delete<T>(task);

        throw new NotImplementedException(task.decision.ToString());
    }

    private async UniTask WaitForDecision(SaveCorruptionTask task)
    {
        ReactiveProperty<SaveCorruptionTask> CorruptionTask = SO.Services.SavesIOService.CorruptionTask;
        CorruptionTask.Value = task;
        await task.completionSource.Task;
        CorruptionTask.Value = null;
    }

    private T LoadBackup<T>(SaveCorruptionTask task)
    {
        return JsonConvert.DeserializeObject<T>(task.filePath + "_backup");
    }

    private T Delete<T>(SaveCorruptionTask task)
    {
        File.Delete(task.filePath);
        return default(T);
    }

    public override IService[] GetDependencies()
    {
        return new IService[] { SO.Services.SavesIOService, SO.Services.TextsService, SO.Services.ProfilesService };
    }

    public void SaveAllModdedData()
    {
        foreach (SaveDataType dataType in Enum.GetValues(typeof(SaveDataType)))
        {
            if (dataType == SaveDataType.General)
            {
                SaveAllTypeModdedData(ProfilesService.GetDefaultFolderPath(), SaveDataType.General);
                continue;
            }
            SaveAllTypeModdedData(ProfilesService.GetProfileFolderPath(), dataType);
        }
        Plugin.Log.LogInfo($"Saved all modded data");
    }

    public void SaveAllTypeModdedData(SaveDataType dataType)
    {
        SaveAllTypeModdedData(dataType.GetCurrentProfilePath(), dataType);
    }

    public void SaveAllTypeModdedData(string path, SaveDataType dataType)
    {
        foreach (KeyValuePair<string, ModSaveData> kvp in ModdedSaveManager.ModGuidToDataLookup)
        {
            string guid = kvp.Key;
            SaveData(path, guid, dataType);
        }
        Plugin.Log.LogInfo($"Saved all modded data for {dataType}");
    }

    public void SaveData(string path, string guid, SaveDataType dataType)
    {
        ModSaveData modSaveData = ModdedSaveManager.GetSaveData(guid);
        SaveData data = modSaveData.GetSaveData(dataType);
        SafeAction<ModSaveData, SaveFileState> listeners = GetTypeModListeners(PreSaveTypeListeners, guid, dataType);
        listeners?.Invoke(modSaveData, data.isNewData ? SaveFileState.NewFile : SaveFileState.LoadedFile);
        data.isNewData = false;
        string fullFilePath = GetSaveFilePath(path, guid, dataType);
        SaveDataToFile(fullFilePath, data);
        Plugin.Log.LogInfo($"Saved modded data for {guid} {dataType}");
    }

    public void SaveDataToFile(string filePath, SaveData data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        SaveFile(filePath, json);
    }

    public void CleanAllModdedData()
    {
        foreach (SaveDataType dataType in Enum.GetValues(typeof(SaveDataType)))
        {
            CleanTypeData(dataType);
        }
        Plugin.Log.LogInfo($"Clean all modded data");
    }

    public void CleanTypeData(SaveDataType dataType)
    {
        foreach (KeyValuePair<string, ModSaveData> kvp in ModdedSaveManager.ModGuidToDataLookup)
        {
            string guid = kvp.Key;
            CleanData(guid, dataType);
        }
        Plugin.Log.LogInfo($"Clean all modded data for {dataType}");
    }

    public void CleanData(string guid, SaveDataType dataType)
    {
        SafeAction<ModSaveData, SaveFileState> callback = GetTypeModListeners(LoadedTypeListeners, guid, dataType);
        SafeAction<ModSaveData, SaveFileState> globalCallback = null;
        LoadedSaveDataListeners.TryGetValue(guid, out globalCallback);
        ModSaveData modSaveData = ModdedSaveManager.ModGuidToDataLookup[guid];

        modSaveData.GetSaveData(dataType).Clear();
        globalCallback?.Invoke(modSaveData, SaveFileState.NewFile);
        callback?.Invoke(modSaveData, SaveFileState.NewFile);
        Plugin.Log.LogInfo($"Clean modded data for {guid} {dataType}");
    }

    private static void EnsureDirectoryForFile(string filePath)
    {
        string folderPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    private void SaveFile(string path, string json)
    {
        Plugin.Log.LogInfo($"Save file: Len={json.Length}, Path={path}");
        EnsureDirectoryForFile(path);
        File.WriteAllText(path, json);
        File.WriteAllText(path + "_backup", json);

        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Remove any characters that would upset a file path
    /// </summary>
    private string CleanGuid(string guid)
    {
        return guid.Replace("/", "").Replace("\\", "").Replace(":", "").Replace("*", "")
            .Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "")
            .Replace("|", "").Replace(".", "_");
    }

    [HarmonyPatch(typeof(AppServices), nameof(AppServices.CreateServices))]
    [HarmonyPostfix]
    private static void AddModdedSaveManagerAsService(AppServices __instance)
    {
        Instance = new ModdedSaveManagerService();
        __instance.allServices.Add(Instance);
    }

    [HarmonyPatch(typeof(SaveCorruptionPopup), nameof(SaveCorruptionPopup.Contact))]
    [HarmonyPrefix]
    private static bool ContactUsButtonPressed(SaveCorruptionPopup __instance)
    {
        if (__instance.task.filePath.Contains("moddedsave"))
        {
            Application.OpenURL("https://tinyurl.com/2hd5xvw3");
            return false;
        }

        return true;
    }
}

/// <summary>
/// The error information
/// </summary>
public struct ErrorData
{
    /// <summary>
    /// Exception
    /// </summary>
    public Exception exception;
    /// <summary>
    /// When load this file, we encounter an exception
    /// </summary>
    public string filePath;
    /// <summary>
    /// Which type of data we are loading.
    /// </summary>
    public SaveDataType dataType;
}