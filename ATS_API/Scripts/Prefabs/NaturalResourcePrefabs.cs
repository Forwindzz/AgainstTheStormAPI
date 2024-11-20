using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Pool;
using Eremite;

namespace ATS_API.Helpers;

// Generated using Version 1.4.17R
public enum NaturalResourcePrefabs
{
	Unknown = -1,
	None,
	Bay_Tree_1,           // Bay Tree 1 - Kelpwood
	Bay_Tree_2,           // Bay Tree 2 - Kelpwood
	Bay_Tree_3,           // Bay Tree 3 - Kelpwood,Kelpwood
	Bay_Tree_4,           // Bay Tree 4 - Kelpwood
	Bay_Tree_5,           // Bay Tree 5 - Kelpwood
	Bay_Tree_6,           // Bay Tree 6 - Kelpwood,Kelpwood
	Coral_Forest_Tree_3,  // Coral Forest Tree 3 - Crimsonreach Tree
	Coral_Forest_Tree_4,  // Coral Forest Tree 4 - Plateleaf Tree
	Coral_Forest_Tree_5,  // Coral Forest Tree 5 - Plateleaf Tree
	Coral_Forest_Tree_A1, // Coral Forest Tree A1 - Musselsprout Tree
	Coral_Forest_Tree_A2, // Coral Forest Tree A2 - Musselsprout Tree
	Coral_Forest_Tree_A3, // Coral Forest Tree A3 - Musselsprout Tree
	Coral_Forest_Tree_A4, // Coral Forest Tree A4 - Musselsprout Tree
	Coral_Forest_Tree_A5, // Coral Forest Tree A5 - Musselsprout Tree
	Cursed_Tree_3,        // Cursed Tree 3 - Dying Tree,Abyssal Tree
	Cursed_Tree_6,        // Cursed Tree 6 - Dying Tree,Abyssal Tree
	Last_Biome_Tree_1,    // Last Biome Tree 1 - Abyssal Tree,Abyssal Tree
	Last_Biome_Tree_2,    // Last Biome Tree 2 - Abyssal Tree,Abyssal Tree,Abyssal Tree
	Last_Biome_Tree_3,    // Last Biome Tree 3 - Abyssal Tree
	Last_Biome_Tree_4,    // Last Biome Tree 4 - Abyssal Tree
	Last_Biome_Tree_5,    // Last Biome Tree 5 - Abyssal Tree
	Last_Biome_Tree_6,    // Last Biome Tree 6 - Overgrown Abyssal Tree
	Last_Biome_Tree_7,    // Last Biome Tree 7 - Overgrown Abyssal Tree
	Marshlands_Tree_01,   // Marshlands_Tree_01 - Mushwood,Mushwood
	Marshlands_Tree_02,   // Marshlands_Tree_02 - Mushwood
	Marshlands_Tree_03,   // Marshlands_Tree_03 - Mushwood
	Marshlands_Tree_04,   // Marshlands_Tree_04 - Mushwood
	Marshlands_Tree_05,   // Marshlands_Tree_05 - Mushwood,Mushwood
	Marshlands_Tree_06,   // Marshlands_Tree_06 - Mushwood,Mushwood
	Marshlands_Tree_07,   // Marshlands_Tree_07 - Mushwood,Mushwood
	Moorlands_Tree_1,     // Moorlands Tree 1 - Coppervein Tree
	Moorlands_Tree_2,     // Moorlands Tree 2 - Coppervein Tree
	Moorlands_Tree_3,     // Moorlands Tree 3 - Coppervein Tree
	Moorlands_Tree_4,     // Moorlands Tree 4 - Coppervein Tree
	Moorlands_Tree_5,     // Moorlands Tree 5 - Coppervein Tree
	Moorlands_Tree_6,     // Moorlands Tree 6 - Coppervein Tree
	Tree_1,               // Tree 1 - Lush Tree
	Tree_2,               // Tree 2 - Lush Tree
	Tree_3,               // Tree 3 - Lush Tree
	Tree_4,               // Tree 4 - Lush Tree
	Tree_5,               // Tree 5 - Lush Tree
	Tree_6,               // Tree 6 - Lush Tree
	Tree_7,               // Tree 7 - Lush Tree


	MAX = 43
}

public static class NaturalResourcePrefabsExtensions
{
	private static NaturalResourcePrefabs[] s_All = null;
	public static NaturalResourcePrefabs[] All()
	{
		if (s_All == null)
		{
			s_All = new NaturalResourcePrefabs[43];
			for (int i = 0; i < 43; i++)
			{
				s_All[i] = (NaturalResourcePrefabs)(i+1);
			}
		}
		return s_All;
	}
	
	/// <summary>
	/// Returns the name or internal ID of the model that will be used in the game.
	/// Every NaturalResourcePrefabs should have a unique name as to distinguish it from others.
	/// If no name is found, it will return NaturalResourcePrefabs.Bay_Tree_1 in the enum and log an error.
	/// </summary>
	public static string ToName(this NaturalResourcePrefabs type)
	{
		if (TypeToInternalName.TryGetValue(type, out var name))
		{
			return name;
		}

		Plugin.Log.LogError($"Cannot find name of NaturalResourcePrefabs: " + type + "\n" + Environment.StackTrace);
		return TypeToInternalName[NaturalResourcePrefabs.Bay_Tree_1];
	}
	
	/// <summary>
	/// Returns a NaturalResourcePrefabs associated with the given name.
	/// Every NaturalResourcePrefabs should have a unique name as to distinguish it from others.
	/// If no NaturalResourcePrefabs is found, it will return NaturalResourcePrefabs.Unknown and log a warning.
	/// </summary>
	public static NaturalResourcePrefabs ToNaturalResourcePrefabs(this string name)
	{
		foreach (KeyValuePair<NaturalResourcePrefabs,string> pair in TypeToInternalName)
		{
			if (pair.Value == name)
			{
				return pair.Key;
			}
		}

		Plugin.Log.LogWarning("Cannot find NaturalResourcePrefabs with name: " + name + "\n" + Environment.StackTrace);
		return NaturalResourcePrefabs.Unknown;
	}
	
	/// <summary>
	/// Returns a NaturalResource associated with the given name.
	/// NaturalResource contain all the data that will be used in the game.
	/// Every NaturalResource should have a unique name as to distinguish it from others.
	/// If no NaturalResource is found, it will return null and log an error.
	/// </summary>
	public static Eremite.MapObjects.NaturalResource ToNaturalResource(this string name)
	{
		Eremite.MapObjects.NaturalResource model = SO.Settings.NaturalResources.Select(a=>a.prefabs).SelectMany(a=>a).FirstOrDefault(a=>a.name == name);
		if (model != null)
		{
			return model;
		}
	
		Plugin.Log.LogError("Cannot find NaturalResource for NaturalResourcePrefabs with name: " + name + "\n" + Environment.StackTrace);
		return null;
	}

    /// <summary>
    /// Returns a NaturalResource associated with the given NaturalResourcePrefabs.
    /// NaturalResource contain all the data that will be used in the game.
    /// Every NaturalResource should have a unique name as to distinguish it from others.
    /// If no NaturalResource is found, it will return null and log an error.
    /// </summary>
	public static Eremite.MapObjects.NaturalResource ToNaturalResource(this NaturalResourcePrefabs types)
	{
		return types.ToName().ToNaturalResource();
	}
	
	/// <summary>
	/// Returns an array of NaturalResource associated with the given NaturalResourcePrefabs.
	/// NaturalResource contain all the data that will be used in the game.
	/// Every NaturalResource should have a unique name as to distinguish it from others.
	/// If a NaturalResource is not found, the element will be replaced with null and an error will be logged.
	/// </summary>
	public static Eremite.MapObjects.NaturalResource[] ToNaturalResourceArray(this IEnumerable<NaturalResourcePrefabs> collection)
	{
		int count = collection.Count();
		Eremite.MapObjects.NaturalResource[] array = new Eremite.MapObjects.NaturalResource[count];
		int i = 0;
		foreach (NaturalResourcePrefabs element in collection)
		{
			array[i++] = element.ToNaturalResource();
		}

		return array;
	}
	
	/// <summary>
	/// Returns an array of NaturalResource associated with the given NaturalResourcePrefabs.
	/// NaturalResource contain all the data that will be used in the game.
	/// Every NaturalResource should have a unique name as to distinguish it from others.
	/// If a NaturalResource is not found, the element will be replaced with null and an error will be logged.
	/// </summary>
	public static Eremite.MapObjects.NaturalResource[] ToNaturalResourceArray(this IEnumerable<string> collection)
	{
		int count = collection.Count();
		Eremite.MapObjects.NaturalResource[] array = new Eremite.MapObjects.NaturalResource[count];
		int i = 0;
		foreach (string element in collection)
		{
			array[i++] = element.ToNaturalResource();
		}

		return array;
	}
	
	/// <summary>
	/// Returns an array of NaturalResource associated with the given NaturalResourcePrefabs.
	/// NaturalResource contain all the data that will be used in the game.
	/// Every NaturalResource should have a unique name as to distinguish it from others.
	/// If a NaturalResource is not found, it will not be included in the array.
	/// </summary>
	public static Eremite.MapObjects.NaturalResource[] ToNaturalResourceArrayNoNulls(this IEnumerable<string> collection)
	{
		using(ListPool<Eremite.MapObjects.NaturalResource>.Get(out List<Eremite.MapObjects.NaturalResource> list))
		{
			foreach (string element in collection)
			{
				Eremite.MapObjects.NaturalResource model = element.ToNaturalResource();
				if (model != null)
				{
					list.Add(model);
				}
			}
			return list.ToArray();
		}
	}
	
	/// <summary>
	/// Returns an array of NaturalResource associated with the given NaturalResourcePrefabs.
	/// NaturalResource contain all the data that will be used in the game.
	/// Every NaturalResource should have a unique name as to distinguish it from others.
	/// If a NaturalResource is not found, it will not be included in the array.
	/// </summary>
	public static Eremite.MapObjects.NaturalResource[] ToNaturalResourceArrayNoNulls(this IEnumerable<NaturalResourcePrefabs> collection)
	{
		using(ListPool<Eremite.MapObjects.NaturalResource>.Get(out List<Eremite.MapObjects.NaturalResource> list))
		{
			foreach (NaturalResourcePrefabs element in collection)
			{
				Eremite.MapObjects.NaturalResource model = element.ToNaturalResource();
				if (model != null)
				{
					list.Add(model);
				}
			}
			return list.ToArray();
		}
	}
	
	internal static readonly Dictionary<NaturalResourcePrefabs, string> TypeToInternalName = new()
	{
		{ NaturalResourcePrefabs.Bay_Tree_1, "Bay Tree 1" },                     // Bay Tree 1 - Kelpwood
		{ NaturalResourcePrefabs.Bay_Tree_2, "Bay Tree 2" },                     // Bay Tree 2 - Kelpwood
		{ NaturalResourcePrefabs.Bay_Tree_3, "Bay Tree 3" },                     // Bay Tree 3 - Kelpwood,Kelpwood
		{ NaturalResourcePrefabs.Bay_Tree_4, "Bay Tree 4" },                     // Bay Tree 4 - Kelpwood
		{ NaturalResourcePrefabs.Bay_Tree_5, "Bay Tree 5" },                     // Bay Tree 5 - Kelpwood
		{ NaturalResourcePrefabs.Bay_Tree_6, "Bay Tree 6" },                     // Bay Tree 6 - Kelpwood,Kelpwood
		{ NaturalResourcePrefabs.Coral_Forest_Tree_3, "Coral Forest Tree 3" },   // Coral Forest Tree 3 - Crimsonreach Tree
		{ NaturalResourcePrefabs.Coral_Forest_Tree_4, "Coral Forest Tree 4" },   // Coral Forest Tree 4 - Plateleaf Tree
		{ NaturalResourcePrefabs.Coral_Forest_Tree_5, "Coral Forest Tree 5" },   // Coral Forest Tree 5 - Plateleaf Tree
		{ NaturalResourcePrefabs.Coral_Forest_Tree_A1, "Coral Forest Tree A1" }, // Coral Forest Tree A1 - Musselsprout Tree
		{ NaturalResourcePrefabs.Coral_Forest_Tree_A2, "Coral Forest Tree A2" }, // Coral Forest Tree A2 - Musselsprout Tree
		{ NaturalResourcePrefabs.Coral_Forest_Tree_A3, "Coral Forest Tree A3" }, // Coral Forest Tree A3 - Musselsprout Tree
		{ NaturalResourcePrefabs.Coral_Forest_Tree_A4, "Coral Forest Tree A4" }, // Coral Forest Tree A4 - Musselsprout Tree
		{ NaturalResourcePrefabs.Coral_Forest_Tree_A5, "Coral Forest Tree A5" }, // Coral Forest Tree A5 - Musselsprout Tree
		{ NaturalResourcePrefabs.Cursed_Tree_3, "Cursed Tree 3" },               // Cursed Tree 3 - Dying Tree,Abyssal Tree
		{ NaturalResourcePrefabs.Cursed_Tree_6, "Cursed Tree 6" },               // Cursed Tree 6 - Dying Tree,Abyssal Tree
		{ NaturalResourcePrefabs.Last_Biome_Tree_1, "Last Biome Tree 1" },       // Last Biome Tree 1 - Abyssal Tree,Abyssal Tree
		{ NaturalResourcePrefabs.Last_Biome_Tree_2, "Last Biome Tree 2" },       // Last Biome Tree 2 - Abyssal Tree,Abyssal Tree,Abyssal Tree
		{ NaturalResourcePrefabs.Last_Biome_Tree_3, "Last Biome Tree 3" },       // Last Biome Tree 3 - Abyssal Tree
		{ NaturalResourcePrefabs.Last_Biome_Tree_4, "Last Biome Tree 4" },       // Last Biome Tree 4 - Abyssal Tree
		{ NaturalResourcePrefabs.Last_Biome_Tree_5, "Last Biome Tree 5" },       // Last Biome Tree 5 - Abyssal Tree
		{ NaturalResourcePrefabs.Last_Biome_Tree_6, "Last Biome Tree 6" },       // Last Biome Tree 6 - Overgrown Abyssal Tree
		{ NaturalResourcePrefabs.Last_Biome_Tree_7, "Last Biome Tree 7" },       // Last Biome Tree 7 - Overgrown Abyssal Tree
		{ NaturalResourcePrefabs.Marshlands_Tree_01, "Marshlands_Tree_01" },     // Marshlands_Tree_01 - Mushwood,Mushwood
		{ NaturalResourcePrefabs.Marshlands_Tree_02, "Marshlands_Tree_02" },     // Marshlands_Tree_02 - Mushwood
		{ NaturalResourcePrefabs.Marshlands_Tree_03, "Marshlands_Tree_03" },     // Marshlands_Tree_03 - Mushwood
		{ NaturalResourcePrefabs.Marshlands_Tree_04, "Marshlands_Tree_04" },     // Marshlands_Tree_04 - Mushwood
		{ NaturalResourcePrefabs.Marshlands_Tree_05, "Marshlands_Tree_05" },     // Marshlands_Tree_05 - Mushwood,Mushwood
		{ NaturalResourcePrefabs.Marshlands_Tree_06, "Marshlands_Tree_06" },     // Marshlands_Tree_06 - Mushwood,Mushwood
		{ NaturalResourcePrefabs.Marshlands_Tree_07, "Marshlands_Tree_07" },     // Marshlands_Tree_07 - Mushwood,Mushwood
		{ NaturalResourcePrefabs.Moorlands_Tree_1, "Moorlands Tree 1" },         // Moorlands Tree 1 - Coppervein Tree
		{ NaturalResourcePrefabs.Moorlands_Tree_2, "Moorlands Tree 2" },         // Moorlands Tree 2 - Coppervein Tree
		{ NaturalResourcePrefabs.Moorlands_Tree_3, "Moorlands Tree 3" },         // Moorlands Tree 3 - Coppervein Tree
		{ NaturalResourcePrefabs.Moorlands_Tree_4, "Moorlands Tree 4" },         // Moorlands Tree 4 - Coppervein Tree
		{ NaturalResourcePrefabs.Moorlands_Tree_5, "Moorlands Tree 5" },         // Moorlands Tree 5 - Coppervein Tree
		{ NaturalResourcePrefabs.Moorlands_Tree_6, "Moorlands Tree 6" },         // Moorlands Tree 6 - Coppervein Tree
		{ NaturalResourcePrefabs.Tree_1, "Tree 1" },                             // Tree 1 - Lush Tree
		{ NaturalResourcePrefabs.Tree_2, "Tree 2" },                             // Tree 2 - Lush Tree
		{ NaturalResourcePrefabs.Tree_3, "Tree 3" },                             // Tree 3 - Lush Tree
		{ NaturalResourcePrefabs.Tree_4, "Tree 4" },                             // Tree 4 - Lush Tree
		{ NaturalResourcePrefabs.Tree_5, "Tree 5" },                             // Tree 5 - Lush Tree
		{ NaturalResourcePrefabs.Tree_6, "Tree 6" },                             // Tree 6 - Lush Tree
		{ NaturalResourcePrefabs.Tree_7, "Tree 7" },                             // Tree 7 - Lush Tree

	};
}
