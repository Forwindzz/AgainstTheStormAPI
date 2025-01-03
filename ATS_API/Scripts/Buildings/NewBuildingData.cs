using System;
using System.Collections.Generic;
using System.Linq;
using ATS_API.Helpers;
using ATS_API.Localization;
using ATS_API.Recipes.Builders;
using Eremite.Buildings;
using Eremite.MapObjects;
using Eremite.Model;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ATS_API.Buildings;

public class NewBuildingData : ASyncable<BuildingModel>
{
    public string Guid;
    public string Name;
    public BuildingTypes ID;
    public BuildingTagTypes Tag;
    public BuildingModel BuildingModel;
    public BuildingVisualData VisualData;
    public BuildingBehaviourTypes Behaviour;
    public object MetaData;
    public List<NameToAmount> RequiredGoods = new List<NameToAmount>();
    public NameToAmount MoveCost;
    public ProfessionTypes Profession = ProfessionTypes.Unknown;
    public BuildingCategoriesTypes Category = BuildingCategoriesTypes.Unknown;
    public List<TagTypes> UsabilityTags = new List<TagTypes>();
    public List<BuildingTagTypes> Tags = new List<BuildingTagTypes>();
    public GameObject CustomPrefab;
    public BuildingConstructionAnimationData BuildingConstructionAnimationData;

    public override bool Sync()
    {
        if (MoveCost != null)
        {
            BuildingModel.movingCost = new GoodRef()
            {
                good = MoveCost.Name.ToGoodModel(),
                amount = MoveCost.Amount
            };
        }
        
        if (RequiredGoods.Count > 0)
        {
            BuildingModel.requiredGoods = RequiredGoods.Select(a => new GoodRef()
            {
                good = a.Name.ToGoodModel(),
                amount = a.Amount
            }).ToArray();
        }
        
        if (UsabilityTags.Count > 0)
        {
            BuildingModel.usabilityTags = UsabilityTags.ToModelTagArray();
        }
        
        if (Tags.Count > 0)
        {
            BuildingModel.tags = Tags.ToBuildingTagModelArray();
        }

        if (Category != BuildingCategoriesTypes.Unknown)
        {
            BuildingModel.category = Category.ToBuildingCategoryModel();
        }

        try
        {
            return SetupPrefab();
        } 
        catch (Exception e)
        {
            Debug.LogError($"Error while setting up prefab for building {BuildingModel.name} and behaviour {Behaviour}");
            Debug.LogError(e);
            return false;
        }
    }

    public override void PostSync()
    {
        base.PostSync();
        
        if (BuildingModel is HouseModel houseModel)
        {
            HouseBuildingBuilder.MetaData metaData = (HouseBuildingBuilder.MetaData) MetaData;
            houseModel.housingRaces = metaData.HousingRaces.ToRaceModelArray();
            houseModel.servedNeeds = metaData.ServedNeeds.ToNeedModelArray();
        }
        else if(BuildingModel is DecorationModel decorationModel)
        {
            DecorationBuildingBuilder.MetaData metaData = (DecorationBuildingBuilder.MetaData) MetaData;
            decorationModel.tier = metaData.Tier.ToDecorationTier();
            
            if ((decorationModel.description == null || decorationModel.description.key == Placeholders.DescriptionKey) 
                && decorationModel.tier != null)
            {
                if (decorationModel.tier.name.ToDecorationTierTypes() == DecorationTierTypes.Comfort)
                {
                    decorationModel.description = "Building_Decoration_Comfort_Desc".ToLocaText();
                }
                else if (decorationModel.tier.name.ToDecorationTierTypes() == DecorationTierTypes.Aesthetics)
                {
                    decorationModel.description = "Building_Decoration_Aesthetics_Desc".ToLocaText();
                }
                else if (decorationModel.tier.name.ToDecorationTierTypes() == DecorationTierTypes.Harmony)
                {
                    decorationModel.description = "Building_Decoration_Harmony_Desc".ToLocaText();
                }
            }
        }
    }

    private bool SetupPrefab()
    {
        if (VisualData == null)
        {
            // No visuals to setup
            if (BuildingModel.Prefab == null)
            {
                APILogger.LogError($"Building {BuildingModel.name} has no prefab and no visuals to setup!");
                return false;
            }
            
            return true;
        }
        
        if (VisualData.Prefab != null)
        {
            // Already setup????
            return true;
        }
  
        APILogger.LogInfo($"Setting up prefab for building {BuildingModel.name} and behaviour {Behaviour}");
        GameObject prefab = CustomPrefab == null ? BuildingManager.GetDefaultVisualData(Behaviour, VisualData.Icon) : CustomPrefab;
        if (prefab == null)
        {
            APILogger.LogError($"Custom prefab for building {BuildingModel.name} is null! Has Custom prefab: {CustomPrefab != null}");
            return false;
        }
        
        GameObject root = Object.Instantiate(prefab, BuildingManager.PrefabContainer);
        if (Behaviour == BuildingBehaviourTypes.Workshop)
        {
            if (BuildingModel is not WorkshopModel workshopModel)
            {
                APILogger.LogError($"Building {BuildingModel.name} is not a WorkshopModel!");
                return false;
            }
            APILogger.LogInfo($"Setting up prefab for workshop {BuildingModel.name}");
            WorkshopBuildingBuilder.MetaData metaData = (WorkshopBuildingBuilder.MetaData) MetaData;
            
            // Visuals
            try
            {
                APILogger.LogInfo($"Initializing prefab for workshop {BuildingModel.name}");
                BuildingManager.InitializePrefab<Workshop, WorkshopView, WorkshopModel>(root, workshopModel, VisualData.Icon, BuildingConstructionAnimationData, AnimHookType.Construction);
            } 
            catch (Exception e)
            {
                Debug.LogError($"Could not set up prefab for building {BuildingModel.name} and behaviour {Behaviour}");
                Debug.LogError(e);
                return false;
            }

            APILogger.LogInfo($"Prefab initialized for {BuildingModel.name} is root null? {root == null} is VisualData null? {VisualData == null} is MetaData null? {MetaData == null}");
            Workshop workshop = root.GetComponent<Workshop>();
            VisualData.Prefab = workshop;
        
            // Data
            APILogger.LogInfo($"Setting up prefab for workshop {BuildingModel.name}");
            workshopModel.prefab = workshop;

            if (metaData.Recipes != null || metaData.Builders != null)
            {
                IEnumerable<WorkshopRecipeModel> recipes = metaData.Recipes ?? new List<WorkshopRecipeModel>();
                IEnumerable<WorkshopRecipeBuilder> builders = metaData.Builders ?? new List<WorkshopRecipeBuilder>();
                workshopModel.recipes = recipes.Concat(builders.Select(a => a.Build())).ToArray();
            }
            else if (workshopModel.recipes == null)
            {
                workshopModel.recipes = new WorkshopRecipeModel[0];
            }

            if (Profession != ProfessionTypes.Unknown)
            {
                workshopModel.profession = Profession.ToProfessionModel();
            }

            APILogger.LogInfo($"Setting up workplaces for workshop {BuildingModel.name}");
            if (metaData.WorkPlaces != null)
            {
                workshopModel.workplaces = new WorkplaceModel[metaData.WorkPlaces.Count];
                for (int i = 0; i < metaData.WorkPlaces.Count; i++)
                {
                    var allowedRaces = metaData.WorkPlaces[i];

                    WorkplaceModel workplace = new WorkplaceModel();
                    workplace.allowedRaces = new RaceModel[allowedRaces.Length];
                    for (int j = 0; j < allowedRaces.Length; j++)
                    {
                        workplace.allowedRaces[j] = allowedRaces[j].ToRaceModel();
                    }

                    workshopModel.workplaces[i] = workplace;
                }
            }
            else if (workshopModel.workplaces == null)
            {
                workshopModel.workplaces = new WorkplaceModel[0];
            }
        }
        else if (Behaviour == BuildingBehaviourTypes.House)
        {
            HouseModel houseModel = BuildingModel as HouseModel;
            
            // Visuals
            try
            {
                BuildingManager.InitializePrefab<House, HouseView, HouseModel>(root, houseModel, VisualData.Icon, BuildingConstructionAnimationData, AnimHookType.Construction);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            House house = root.GetComponent<House>();
            VisualData.Prefab = house;
         
            // Data
            houseModel.prefab = house;
        }
        else if (Behaviour == BuildingBehaviourTypes.Decoration)
        {
            DecorationModel decorationModel = BuildingModel as DecorationModel;
            DecorationBuildingBuilder.MetaData metaData = (DecorationBuildingBuilder.MetaData) MetaData;
            
            // Visuals
            try
            {
                BuildingManager.InitializePrefab<Decoration, DecorationView, DecorationModel>(root, decorationModel, VisualData.Icon, BuildingConstructionAnimationData, AnimHookType.Construction);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            Decoration decoration = root.GetComponent<Decoration>();
            VisualData.Prefab = decoration;
        
            // Data
            decorationModel.prefab = decoration;
            decoration.model = decorationModel;
        }
        else
        { 
            APILogger.LogError($"Building type {Behaviour} for buildings not implemented yet!");
            return false;
        }

        return true;
    }

    public static NewBuildingData FromModel<T>(T model) where T : BuildingModel
    {
        NewBuildingData buildingData = BuildingManager.NewBuildings.FirstOrDefault(a => a.BuildingModel == model);
        if (buildingData != null)
        {
            return buildingData;
        }
        
        buildingData = new NewBuildingData();
        buildingData.BuildingModel = model;
        buildingData.Guid = "";
        buildingData.Name = model.name;
        buildingData.ID = model.name.ToBuildingTypes();
        
        return buildingData;
    }
}