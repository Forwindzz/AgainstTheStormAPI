using System;
using System.Collections.Generic;
using Eremite;
using Eremite.Buildings.UI.Ports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ATS_API.SaveLoading;

/// <summary>
/// Indicate which part of data.
/// </summary>
public enum SaveDataType
{
    /// <summary>
    /// it is global. Can be used cross profiles, worlds, and settlements.
    /// The general data loads when the game lanuch (after loading the profile data)
    /// The general data saves when the game exit, or after saving the profile data
    /// </summary>
    General = 0,
    /// <summary>
    /// The meta data stores meta data, such as game history, meta upgrades, achievements, story...
    /// which mostly about the game data in citadel (the smoldering city).
    /// The meta data loads when switch the profile or open the game.
    /// The meta data saves when the player exit the world map and return to the menu.
    /// It will also automatically save during the settlement game.
    /// </summary>
    Meta = 1,
    /// <summary>
    /// The cycle data is mainly about world map information, this includes: 
    /// embark bonus, established town, map information, years, part of game statics.
    /// The CurrentCycle data loads when switch the profile or open the game.
    /// The data will be cleaned up and loaded with flag <see cref="SaveFileState.NewFile"/> when the player start a new cycle.
    /// The data will be saved when the player exit the world map and return to the menu, it will also be saved when the player finish a settlement/
    /// It will also automatically save during the settlement game.
    /// </summary>
    CurrentCycle = 2,
    /// <summary>
    /// The settlement data stores the current game (settlement) information, this includes:
    /// buildings, villagers, orders...
    /// The data will be loaded when the player continue an unfinished game.
    /// When the player start a new settlement, the data will be cleaned up and loaded with flag <see cref="SaveFileState.NewFile"/>.
    /// The data will be saved when the player exit the current game and return to the main menu.
    /// It will also automatically save during the game.
    /// </summary>
    CurrentSettlement = 3
}

internal static class SaveDataTypeExtensions
{
    internal static string GetCurrentProfilePath(this SaveDataType dataType)
    {
        return dataType switch
        {
            SaveDataType.General => SO.ProfilesService.GetDefaultFolderPath(),
            _ => SO.ProfilesService.GetProfileFolderPath()
        };
    }
}


/// <summary>
/// This class contains all the data from the specific mod.
/// </summary>
[Serializable]
public class ModSaveData
{
    /// <summary>
    /// Your mod guid.
    /// </summary>
    public string ModGuid;
    
    /// <summary>
    /// General data can be used cross profiles, worlds, and settlements.
    /// </summary>
    public SaveData General = new SaveData();
    /// <summary>
    /// Current settlement game data. 
    /// It will be initialized when the player begins a new settlement game.
    /// It will be cleaned up when the player finishes a settlement game.
    /// </summary>
    public SaveData CurrentSettlement = new SaveData();
    /// <summary>
    /// Current world cycle game data. Ex. World map data is a kind of cycle game data.
    /// It will be initialized when the player begins a new cycle.
    /// It will be cleaned up when the player finishes a cycle.
    /// </summary>
    public SaveData CurrentCycle = new SaveData();
    /// <summary>
    /// Current Profile data. Ex. Queen hand trial (QHT) saves its data in the profile.
    /// The player might create, change and remove profiles in the main menu.
    /// </summary>
    public SaveData CurrentProfile = new SaveData();
    /// <summary>
    /// Current Meta data. Ex. The meta upgrade perks, game histories.
    /// The meta data is initialized when a new profile is created, and the player have not play this profile.
    /// </summary>
    public SaveData Meta = new SaveData();

    /// <summary>
    /// Create a new mod data
    /// </summary>
    /// <param name="guid"></param>
    public ModSaveData(string guid)
    {
        ModGuid = guid;
    }

    /// <summary>
    /// Get the <see cref="SaveData"/> based on <see cref="SaveDataType"/>
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public SaveData GetSaveData(SaveDataType dataType)
    {
        return dataType switch
        {
            SaveDataType.General => General,
            SaveDataType.Meta => Meta,
            SaveDataType.CurrentCycle => CurrentCycle,
            SaveDataType.CurrentSettlement => CurrentSettlement,
            _ => null,
        };
    }

    /// <summary>
    /// Set the <see cref="SaveData"/> based on <see cref="SaveDataType"/>
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="saveData"></param>
    public void SetSaveData(SaveDataType dataType, SaveData saveData)
    {
        switch (dataType)
        {
            case SaveDataType.General:
                General = saveData;
                break;
            case SaveDataType.Meta:
                Meta = saveData;
                break;
            case SaveDataType.CurrentCycle:
                CurrentCycle = saveData;
                break;
            case SaveDataType.CurrentSettlement:
                CurrentSettlement = saveData;
                break;
        }
    }
}

/// <summary>
/// This stores the actual data, you can use it like a dictionary.
/// </summary>
[Serializable]
public class SaveData
{
    [JsonProperty]
    private Dictionary<string, object> Data = new Dictionary<string, object>();

    // a temporary tag to mark this data is newly created or not, for initialization purpose.
    [JsonIgnore]
    internal bool isNewData = false;
    
    /// <summary>
    /// Get the object with the key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue">If the not exists, return default value</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public T GetValueAsObject<T>(string key, T defaultValue = default)
    {
        if (!Data.ContainsKey(key))
            Data.Add(key, defaultValue);

        object o = Data[key];
        try
        {
            if (o is JObject jObject)
            {
                return jObject.ToObject<T>();
            }
            return (T)Convert.ChangeType(o, typeof(T));
        }
        catch (InvalidCastException e)
        {
            throw new InvalidCastException($"Failed to cast value of key {key} to type {typeof(T)} from type {(o == null ? null : o.GetType())}");
        }
    }

    /// <summary>
    /// Get the object with the key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public object GetValue(string key, object defaultValue = null)
    {
        if (!Data.ContainsKey(key))
            Data.Add(key, defaultValue);

        return Data[key];
    }

    /// <summary>
    /// Store the object with the key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(string key, object value)
    {
        Data[key] = value;
    }

    /// <summary>
    /// Clear all data.
    /// </summary>
    public void Clear()
    {
        isNewData = true;
        Data.Clear();
    }
}
