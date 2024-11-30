using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Pool;
using Eremite;
using Eremite.Model;

namespace ATS_API.Helpers;

// Generated using Version 1.5.5R
public enum MetaCurrencyTypes
{
	Unknown = -1,
	None,
	
	/// <summary>
	/// Artifacts
	/// </summary>
	/// <name>Artifacts</name>
	Artifacts,

	/// <summary>
	/// Food Stockpiles
	/// </summary>
	/// <name>Food Stockpiles</name>
	Food_Stockpiles,

	/// <summary>
	/// Machinery
	/// </summary>
	/// <name>Machinery</name>
	Machinery,



	MAX = 3
}

public static class MetaCurrencyTypesExtensions
{
	private static MetaCurrencyTypes[] s_All = null;
	public static MetaCurrencyTypes[] All()
	{
		if (s_All == null)
		{
			s_All = new MetaCurrencyTypes[3];
			for (int i = 0; i < 3; i++)
			{
				s_All[i] = (MetaCurrencyTypes)(i+1);
			}
		}
		return s_All;
	}
	
	/// <summary>
	/// Returns the name or internal ID of the model that will be used in the game.
	/// Every MetaCurrencyTypes should have a unique name as to distinguish it from others.
	/// If no name is found, it will return MetaCurrencyTypes.Artifacts in the enum and log an error.
	/// </summary>
	public static string ToName(this MetaCurrencyTypes type)
	{
		if (TypeToInternalName.TryGetValue(type, out var name))
		{
			return name;
		}

		Plugin.Log.LogError($"Cannot find name of MetaCurrencyTypes: " + type + "\n" + Environment.StackTrace);
		return TypeToInternalName[MetaCurrencyTypes.Artifacts];
	}
	
	/// <summary>
	/// Returns a MetaCurrencyTypes associated with the given name.
	/// Every MetaCurrencyTypes should have a unique name as to distinguish it from others.
	/// If no MetaCurrencyTypes is found, it will return MetaCurrencyTypes.Unknown and log a warning.
	/// </summary>
	public static MetaCurrencyTypes ToMetaCurrencyTypes(this string name)
	{
		foreach (KeyValuePair<MetaCurrencyTypes,string> pair in TypeToInternalName)
		{
			if (pair.Value == name)
			{
				return pair.Key;
			}
		}

		Plugin.Log.LogWarning("Cannot find MetaCurrencyTypes with name: " + name + "\n" + Environment.StackTrace);
		return MetaCurrencyTypes.Unknown;
	}
	
	/// <summary>
	/// Returns a MetaCurrencyModel associated with the given name.
	/// MetaCurrencyModel contain all the data that will be used in the game.
	/// Every MetaCurrencyModel should have a unique name as to distinguish it from others.
	/// If no MetaCurrencyModel is found, it will return null and log an error.
	/// </summary>
	public static Eremite.Model.MetaCurrencyModel ToMetaCurrencyModel(this string name)
	{
		Eremite.Model.MetaCurrencyModel model = SO.Settings.metaCurrencies.FirstOrDefault(a=>a.name == name);
		if (model != null)
		{
			return model;
		}
	
		Plugin.Log.LogError("Cannot find MetaCurrencyModel for MetaCurrencyTypes with name: " + name + "\n" + Environment.StackTrace);
		return null;
	}

    /// <summary>
    /// Returns a MetaCurrencyModel associated with the given MetaCurrencyTypes.
    /// MetaCurrencyModel contain all the data that will be used in the game.
    /// Every MetaCurrencyModel should have a unique name as to distinguish it from others.
    /// If no MetaCurrencyModel is found, it will return null and log an error.
    /// </summary>
	public static Eremite.Model.MetaCurrencyModel ToMetaCurrencyModel(this MetaCurrencyTypes types)
	{
		return types.ToName().ToMetaCurrencyModel();
	}
	
	/// <summary>
	/// Returns an array of MetaCurrencyModel associated with the given MetaCurrencyTypes.
	/// MetaCurrencyModel contain all the data that will be used in the game.
	/// Every MetaCurrencyModel should have a unique name as to distinguish it from others.
	/// If a MetaCurrencyModel is not found, the element will be replaced with null and an error will be logged.
	/// </summary>
	public static Eremite.Model.MetaCurrencyModel[] ToMetaCurrencyModelArray(this IEnumerable<MetaCurrencyTypes> collection)
	{
		int count = collection.Count();
		Eremite.Model.MetaCurrencyModel[] array = new Eremite.Model.MetaCurrencyModel[count];
		int i = 0;
		foreach (MetaCurrencyTypes element in collection)
		{
			array[i++] = element.ToMetaCurrencyModel();
		}

		return array;
	}
	
	/// <summary>
	/// Returns an array of MetaCurrencyModel associated with the given MetaCurrencyTypes.
	/// MetaCurrencyModel contain all the data that will be used in the game.
	/// Every MetaCurrencyModel should have a unique name as to distinguish it from others.
	/// If a MetaCurrencyModel is not found, the element will be replaced with null and an error will be logged.
	/// </summary>
	public static Eremite.Model.MetaCurrencyModel[] ToMetaCurrencyModelArray(this IEnumerable<string> collection)
	{
		int count = collection.Count();
		Eremite.Model.MetaCurrencyModel[] array = new Eremite.Model.MetaCurrencyModel[count];
		int i = 0;
		foreach (string element in collection)
		{
			array[i++] = element.ToMetaCurrencyModel();
		}

		return array;
	}
	
	/// <summary>
	/// Returns an array of MetaCurrencyModel associated with the given MetaCurrencyTypes.
	/// MetaCurrencyModel contain all the data that will be used in the game.
	/// Every MetaCurrencyModel should have a unique name as to distinguish it from others.
	/// If a MetaCurrencyModel is not found, it will not be included in the array.
	/// </summary>
	public static Eremite.Model.MetaCurrencyModel[] ToMetaCurrencyModelArrayNoNulls(this IEnumerable<string> collection)
	{
		using(ListPool<Eremite.Model.MetaCurrencyModel>.Get(out List<Eremite.Model.MetaCurrencyModel> list))
		{
			foreach (string element in collection)
			{
				Eremite.Model.MetaCurrencyModel model = element.ToMetaCurrencyModel();
				if (model != null)
				{
					list.Add(model);
				}
			}
			return list.ToArray();
		}
	}
	
	/// <summary>
	/// Returns an array of MetaCurrencyModel associated with the given MetaCurrencyTypes.
	/// MetaCurrencyModel contain all the data that will be used in the game.
	/// Every MetaCurrencyModel should have a unique name as to distinguish it from others.
	/// If a MetaCurrencyModel is not found, it will not be included in the array.
	/// </summary>
	public static Eremite.Model.MetaCurrencyModel[] ToMetaCurrencyModelArrayNoNulls(this IEnumerable<MetaCurrencyTypes> collection)
	{
		using(ListPool<Eremite.Model.MetaCurrencyModel>.Get(out List<Eremite.Model.MetaCurrencyModel> list))
		{
			foreach (MetaCurrencyTypes element in collection)
			{
				Eremite.Model.MetaCurrencyModel model = element.ToMetaCurrencyModel();
				if (model != null)
				{
					list.Add(model);
				}
			}
			return list.ToArray();
		}
	}
	
	internal static readonly Dictionary<MetaCurrencyTypes, string> TypeToInternalName = new()
	{
		{ MetaCurrencyTypes.Artifacts, "Artifacts" },             // Artifacts
		{ MetaCurrencyTypes.Food_Stockpiles, "Food Stockpiles" }, // Food Stockpiles
		{ MetaCurrencyTypes.Machinery, "Machinery" },             // Machinery

	};
}
