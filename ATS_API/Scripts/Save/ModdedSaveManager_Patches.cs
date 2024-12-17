using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eremite.Controller.Generator;
using Eremite.Model.State;
using Eremite.Services;
using Eremite.Services.World;
using Eremite.WorldMap;
using HarmonyLib;

namespace ATS_API.SaveLoading;

[HarmonyPatch]
public static partial class ModdedSaveManager
{
    //
    // Auto saving patches
    //
    [HarmonyPatch(typeof(WorldStateService), nameof(WorldStateService.SaveState))]
    [HarmonyPostfix]
    private static void Post_WorldState_Save()
    {
        Plugin.Log.LogInfo("WorldStateService saves CurrentCycle modded data");
        SaveAllTypeModdedData(SaveDataType.CurrentCycle);
    }

    [HarmonyPatch(typeof(WorldStateService), nameof(WorldStateService.LoadState))]
    [HarmonyPostfix]
    private static async UniTask<WorldState> Post_WorldState_LoadAsync(UniTask<WorldState> __result)
    {
        Plugin.Log.LogInfo("WorldStateService loads CurrentCycle modded data");
        await ModdedSaveManagerService.Instance.LoadAllTypeModdedData(SaveDataType.CurrentCycle);
        return await __result;
    }

    [HarmonyPatch(typeof(GameSaveService), nameof(GameSaveService.SaveState))]
    [HarmonyPostfix]
    private static async UniTask Post_GameSaveService_Save(UniTask enumerator)
    {
        Plugin.Log.LogInfo("GameSaveService saves CurrentSettlement modded data");
        await enumerator;
        SaveAllTypeModdedData(SaveDataType.CurrentSettlement);
    }

    [HarmonyPatch(typeof(GameSaveService), nameof(GameSaveService.LoadState))]
    [HarmonyPostfix]
    private static async UniTask<GameState> Post_GameSaveService_Load(UniTask<GameState> __result)
    {
        Plugin.Log.LogInfo("GameSaveService loads CurrentSettlement modded data");
        await ModdedSaveManagerService.Instance.LoadAllTypeModdedData(SaveDataType.CurrentSettlement);
        return await __result;
    }

    [HarmonyPatch(typeof(MetaStateService), nameof(MetaStateService.SaveState))]
    [HarmonyPostfix]
    private static void Post_MetaStateService_Save()
    {
        Plugin.Log.LogInfo("MetaStateService saves Meta modded data");
        SaveAllTypeModdedData(SaveDataType.Meta);
    }

    [HarmonyPatch(typeof(MetaStateService), nameof(MetaStateService.LoadState))]
    [HarmonyPostfix]
    private static async UniTask<MetaState> Post_MetaStateService_Load(UniTask<MetaState> __result)
    {
        Plugin.Log.LogInfo("MetaStateService loads Meta modded data");
        await ModdedSaveManagerService.Instance.LoadAllTypeModdedData(SaveDataType.Meta);
        return await __result;
    }

    // When quit the game, or the players vote, all profile information will be saved
    // We also save the current profile information here.
    // The general data is also saved here
    [HarmonyPatch(typeof(ProfilesService), nameof(ProfilesService.SaveProfiles))]
    [HarmonyPostfix]
    private static void Post_ProfilesService_Save()
    {
        Plugin.Log.LogInfo("ProfilesService saves General modded data");
        SaveAllTypeModdedData(SaveDataType.General);
    }

    [HarmonyPatch(typeof(ProfilesService), nameof(ProfilesService.LoadManifest))]
    [HarmonyPostfix]
    private static async UniTask<List<ProfileData>> Post_ProfilesService_LoadManifest_Load(
        UniTask<List<ProfileData>> __result)
    {
        Plugin.Log.LogInfo("ProfilesService loads General modded data");
        await ModdedSaveManagerService.Instance.LoadAllTypeModdedData(SaveDataType.General);
        return await __result;
    }

    //
    // Clearing data patches
    //

    [HarmonyPatch(typeof(WorldCalendarService), nameof(WorldCalendarService.OnCycleEnded))]
    [HarmonyPostfix]
    private static void OnCycleEnded()
    {
        Plugin.Log.LogInfo("OnCycleEnded. Clearing CurrentCycle data from all mods");
        ModdedSaveManagerService.Instance.CleanTypeData(SaveDataType.CurrentCycle);
    }
    
    [HarmonyPatch(typeof(GameLoader), nameof(GameLoader.StartNewGame))]
    [HarmonyPostfix]
    private static void StartNewGame()
    {
        Plugin.Log.LogInfo("StartNewGame. Clearing CurrentSettlement data from all mods");
        ModdedSaveManagerService.Instance.CleanTypeData(SaveDataType.CurrentSettlement);
    }
}