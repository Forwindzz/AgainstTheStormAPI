using System;
using System.Collections.Generic;
using ATS_API.Helpers;
using Cysharp.Threading.Tasks;
using Eremite;

namespace ATS_API.SaveLoading;

/// <summary>
/// The class manages the save and load data for all mods.
/// You can add extra data in different phases.
/// To use this class, you need to add various listeners through this class.
/// If the mod register listeners here, the API will regard the mod need save and load states.
/// The listener accepts the parameters:<see cref="ModSaveData"/> <see cref="SaveFileState"/>.
/// The listener will recieve the corresponding mod data and save file state.
/// For <see cref="ModSaveData"/>, you can put any data you want here.
/// For <see cref="SaveFileState"/>, it indicates the save file state, 
/// if it is newly created file <see cref="SaveFileState.NewFile"/>, you need to initialize and setup basic data.
/// Otherwise the existing data is loaded from the exsiting file (<see cref="SaveFileState.LoadedFile"/>)
/// Note: The <see cref="SaveFileState.NewFile"/> state will appear once both for loaded listeners and pre-save listeners,
/// if the save data is newly created.
/// 
/// There are two kinds of listeners: LoadedXXXX and PreSaveXXXX,
/// LoadedXXX will be invoked after loading the save file (or create a new one),
/// PreSaveXXX will be invoked before saving the save file.
/// </summary>
public static partial class ModdedSaveManager
{
    internal static Dictionary<string, ModSaveData> ModGuidToDataLookup => m_ModSaveData;

    // %userprofile%\AppData\LocalLow\Eremite Games\Against the Storm\
    public static string PathToSaveFile => GameMB.ProfilesService.GetDefaultFolderPath();

    private static readonly Dictionary<string, ModSaveData> m_ModSaveData = new Dictionary<string, ModSaveData>();

    /// <summary>
    /// Get the save data based on the mod guid.
    /// </summary>
    /// <param name="guid">mod guid</param>
    /// <returns>Mod save data, it will not return null</returns>
    /// <exception cref="Exception">If you invoke this method before the API load the file, it will throw an exception.</exception>
    public static ModSaveData GetSaveData(string guid)
    {
        if (ModdedSaveManagerService.Instance == null || !ModdedSaveManagerService.Instance.Loaded)
        {
            throw new Exception("ModdedSaveManagerService not loaded yet. Use ModdedSaveManager.ListenForLoadedSaveData to wait for it to load instead!");
        }
        
        if (m_ModSaveData.TryGetValue(guid, out var data))
        {
            return data;
        }
        
        m_ModSaveData[guid] = new ModSaveData(guid);
        return m_ModSaveData[guid];
    }

    /// <summary>
    /// Check if the mod save data exists
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public static bool ContainsSaveData(string guid)
    {
        return m_ModSaveData.ContainsKey(guid);
    }

    /// <summary>
    /// Save all mod data.
    /// </summary>
    public static void SaveAllModdedData()
    {
        ModdedSaveManagerService.Instance.SaveAllModdedData();
    }

    /// <summary>
    /// Save the specific type of data manually.
    /// Normally, API will manage save and load automatically.
    /// </summary>
    /// <param name="saveDataType"></param>
    public static void SaveAllTypeModdedData(SaveDataType saveDataType)
    {
        ModdedSaveManagerService.Instance.SaveAllTypeModdedData(saveDataType);
    }

    /// <summary>
    /// Save the specific type of mod data manually.
    /// The Save path is <see cref="GetSaveFilePath"/>
    /// </summary>
    /// <param name="saveDataType"></param>
    /// <param name="guid"></param>
    public static void SaveData(SaveDataType saveDataType, string guid)
    {
        ModdedSaveManagerService.Instance.SaveData(saveDataType.GetCurrentProfilePath(), guid, saveDataType);
    }

    /// <summary>
    /// The save file path.
    /// </summary>
    /// <param name="modGUID"></param>
    /// <param name="saveDataType"></param>
    /// <returns></returns>
    public static string GetSaveFilePath(string modGUID, SaveDataType saveDataType)
    {
        return ModdedSaveManagerService.Instance.GetSaveFilePath(saveDataType.GetCurrentProfilePath(), modGUID, saveDataType);
    }

    /// <summary>
    /// Add a listener. It will be invoked when API loads any saved data.
    /// This will be invoked when API loads the General, Meta, CurrentCycle, CurrentSettlement saved data.
    /// For the callback,<see cref="ModSaveData"/> will return the current mod data.
    ///<see cref="SaveFileState"/> will return 
    ///<see cref="SaveFileState.NewFile"/> if the data is newly created, you may want to initialize data.
    ///<see cref="SaveFileState.LoadedFile"/> if the data is loaded from existing save, you may want to apply the data to the program. 
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="callback"></param>
    public static void ListenForLoadedSaveData(string guid, Action<ModSaveData, SaveFileState> callback)
    {
        if (!ModdedSaveManagerService.LoadedSaveDataListeners.TryGetValue(guid, out SafeAction<ModSaveData, SaveFileState> listeners))
        {
            listeners = new SafeAction<ModSaveData, SaveFileState>();
            ModdedSaveManagerService.LoadedSaveDataListeners[guid] = listeners;
        }

        listeners.AddListener(callback);
    }

    /// <summary>
    /// Remove the listener, which can be invoked when API loads any saved data.
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="callback"></param>
    public static void StopListeningForLoadedSaveData(string guid, Action<ModSaveData, SaveFileState> callback)
    {
        if (ModdedSaveManagerService.LoadedSaveDataListeners.TryGetValue(guid, out SafeAction<ModSaveData, SaveFileState> listeners))
        {
            listeners.RemoveListener(callback);
        }
    }

    /// <summary>
    /// Add a listener. It will be invoked before API save any data.
    /// This will be invoked before API save the General, Meta, CurrentCycle, CurrentSettlement saved data.
    /// For the callback,<see cref="ModSaveData"/> will return the current mod data.
    ///<see cref="SaveFileState"/> will return 
    ///<see cref="SaveFileState.NewFile"/> if the data have not been saved since its newly created.
    ///<see cref="SaveFileState.LoadedFile"/> if the data has been saved at least once, or loaded from existing save files. 
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="callback"></param>
    public static void ListenForPreSaveSaveData(string guid, Action<ModSaveData, SaveFileState> callback)
    {
        if (!ModdedSaveManagerService.PreSaveSaveDataListeners.TryGetValue(guid, out SafeAction<ModSaveData, SaveFileState> listeners))
        {
            listeners = new SafeAction<ModSaveData, SaveFileState>();
            ModdedSaveManagerService.PreSaveSaveDataListeners[guid] = listeners;
        }

        listeners.AddListener(callback);
    }

    /// <summary>
    /// Remove the listener, which can be invoked before API save any data.
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="callback"></param>
    public static void StopListeningForPreSaveSaveData(string guid, Action<ModSaveData, SaveFileState> callback)
    {
        if (ModdedSaveManagerService.PreSaveSaveDataListeners.TryGetValue(guid, out SafeAction<ModSaveData, SaveFileState> listeners))
        {
            listeners.RemoveListener(callback);
        }
    }

    /// <summary>
    /// Add a listener. The listener will be invoked when saveDataType data is loaded.
    /// For the callback,<see cref="ModSaveData"/> will return the current mod data.
    ///<see cref="SaveFileState"/> will return 
    ///<see cref="SaveFileState.NewFile"/> if the data is newly created, you may want to initialize data.
    ///<see cref="SaveFileState.LoadedFile"/> if the data is loaded from existing save, you may want to apply the data to the program. 
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="saveDataType">The data type (SaveDataType) you want to monitor</param>
    /// <param name="callback"></param>
    public static void ListenForLoadedSpecificData(
        string guid,
        SaveDataType saveDataType,
        Action<ModSaveData, SaveFileState> callback)
    {
        ModdedSaveManagerService.AddDataTypeListener(
            ModdedSaveManagerService.LoadedTypeListeners,
            saveDataType, guid, callback);
    }

    /// <summary>
    /// Remove the listener, which will be invoked when saveDataType data is loaded.
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="saveDataType">The data type (SaveDataType) you want to monitor</param>
    /// <param name="callback"></param>
    public static void StopListeningForLoadedSpecificData(
        string guid,
        SaveDataType saveDataType,
        Action<ModSaveData, SaveFileState> callback)
    {
        ModdedSaveManagerService.RemoveDataTypeListener(
            ModdedSaveManagerService.LoadedTypeListeners,
            saveDataType, guid, callback);
    }

    /// <summary>
    /// Add a listener. The listener will be invoked before saving saveDataType data.
    /// For the callback,<see cref="ModSaveData"/> will return the current mod data.
    ///<see cref="SaveFileState"/> will return 
    ///<see cref="SaveFileState.NewFile"/> if the data have not been saved since its newly created.
    ///<see cref="SaveFileState.LoadedFile"/> if the data has been saved at least once, or loaded from existing save files. 
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="saveDataType">The data type (SaveDataType) you want to monitor</param>
    /// <param name="callback"></param>
    public static void ListenForPreSaveSpecificData(
        string guid,
        SaveDataType saveDataType,
        Action<ModSaveData, SaveFileState> callback)
    {
        ModdedSaveManagerService.AddDataTypeListener(
            ModdedSaveManagerService.PreSaveTypeListeners,
            saveDataType, guid, callback);
    }

    /// <summary>
    /// Remove the listener, which will be invoked before saving saveDataType data.
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="saveDataType">The data type (SaveDataType) you want to monitor</param>
    /// <param name="callback"></param>
    public static void StopListeningForPreSaveSpecificData(
        string guid,
        SaveDataType saveDataType,
        Action<ModSaveData, SaveFileState> callback)
    {
        ModdedSaveManagerService.RemoveDataTypeListener(
            ModdedSaveManagerService.PreSaveTypeListeners,
            saveDataType, guid, callback);
    }

    /// <summary>
    /// Handle the error if you want to deal with it by yourself.
    /// In the handler, you need to recover the data (or replace with a default data) for your ModSaveData.
    /// For error information: see <see cref="ErrorData"/>.
    /// If the mod data does not have any error handler, the API will handle it by self 
    /// (let player quit game or create a brand new data).
    /// </summary>
    /// <param name="guid">The GUID of your mod</param>
    /// <param name="handler"></param>
    public static void AddErrorHandler(string guid, Func<ErrorData, UniTask<ModSaveData>> handler)
    {
        if (!ModdedSaveManagerService.ErrorHandlers.TryGetValue(guid, out SafeFunc<ErrorData, UniTask<ModSaveData>> handlers))
        {
            handlers = new SafeFunc<ErrorData, UniTask<ModSaveData>>();
            ModdedSaveManagerService.ErrorHandlers[guid] = handlers;
        }
        
        handlers.AddListener(handler);
    }
    
    /// <summary>
    /// Remove the error handler
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="handler"></param>
    public static void RemoveErrorhandler(string guid, Func<ErrorData, UniTask<ModSaveData>> handler)
    {
        if (ModdedSaveManagerService.ErrorHandlers.TryGetValue(guid, out SafeFunc<ErrorData, UniTask<ModSaveData>> handlers))
        {
            handlers.RemoveListener(handler);
        }
    }
}