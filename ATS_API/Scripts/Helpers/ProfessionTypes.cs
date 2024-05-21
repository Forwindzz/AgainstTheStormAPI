﻿using System.Collections.Generic;
using Eremite;
using Eremite.Model;

namespace ATS_API.Helpers;

public enum ProfessionTypes
{
    Unknown = -1,
    None,
    Alchemist,
    Apothecary,
    Artisan,
    Baker,
    BathHouseWorker,
    BeaneryWorker,
    BlightFighter,
    BreweryWorker,
    BrickyardWorker,
    Builder,
    Butcher,
    Carpenter,
    CellarWorker,
    ClanMember,
    ClayPitWorkshopWorker,
    ClayPitDigger,
    Cook,
    Cooper,
    Craftsman,
    DistilleryWorker,
    Druid,
    Explorer,
    FactoryWorker,
    Farmer,
    FineSmith,
    FireKeeper,
    Forager,
    FurnaceWorker,
    Geologist,
    GranaryWorker,
    GreenhouseWorker,
    GreenhouseWorkshopWorker,
    GrillWorker,
    GuildMember,
    Hauler,
    Harvester,
    Herbalist,
    KilnWorker,
    LeatherWorker,
    Librarian,
    LumberMillWorker,
    Manufacturer,
    MillWorker,
    Miner,
    Monk,
    Outfitter,
    OvenWorker,
    PantryWorker,
    Priest,
    PressWorker,
    Provisioner,
    RainCatcherWorker,
    RainCollectorWorker,
    Rancher,
    Scavenger,
    Scout,
    Scribe,
    Seller,
    Sewer,
    Smelter,
    Smith,
    SmokehouseWorker,
    Speaker,
    StampingMillWorker,
    StoneCutter,
    Supplier,
    TeaDoctor,
    TeaHouseWorker,
    TincturyWorker,
    Tinkerer,
    ToolShopWorker,
    Trapper,
    Waiter,
    Weaver,
    Woodcutter
}

public static class ProfessionExtension
{
    internal static readonly Dictionary<ProfessionTypes, string> TypeToInternalName = new()
    {
        { ProfessionTypes.Alchemist, "Alchemist" },
        { ProfessionTypes.Apothecary, "Apothecary" },
        { ProfessionTypes.Artisan, "Artisan" },
        { ProfessionTypes.Baker, "Baker" },
        { ProfessionTypes.BathHouseWorker, "Bath House worker" },
        { ProfessionTypes.BeaneryWorker, "Beanery Worker" },
        { ProfessionTypes.BlightFighter, "BlightFighter" },
        { ProfessionTypes.BreweryWorker, "Brewery Worker" },
        { ProfessionTypes.BrickyardWorker, "Brickyard Worker" },
        { ProfessionTypes.Builder, "Builder" },
        { ProfessionTypes.Butcher, "Butcher" },
        { ProfessionTypes.Carpenter, "Carpenter" },
        { ProfessionTypes.CellarWorker, "Cellar worker" },
        { ProfessionTypes.ClanMember, "Clan Mamber" },
        { ProfessionTypes.ClayPitWorkshopWorker, "Clay Pit Workshop Worker" },
        { ProfessionTypes.ClayPitDigger, "Claypit Digger" },
        { ProfessionTypes.Cook, "Cook" },
        { ProfessionTypes.Cooper, "Cooper" },
        { ProfessionTypes.Craftsman, "Craftsman" },
        { ProfessionTypes.DistilleryWorker, "Distillery Worker" },
        { ProfessionTypes.Druid, "Druid" },
        { ProfessionTypes.Explorer, "Explorer" },
        { ProfessionTypes.FactoryWorker, "Factory Worker" },
        { ProfessionTypes.Farmer, "Farmer" },
        { ProfessionTypes.FineSmith, "Finesmith" },
        { ProfessionTypes.FireKeeper, "FireKeeper" },
        { ProfessionTypes.Forager, "Forager" },
        { ProfessionTypes.FurnaceWorker, "Furnace worker" },
        { ProfessionTypes.Geologist, "Geologist" },
        { ProfessionTypes.GranaryWorker, "Granary worker" },
        { ProfessionTypes.GreenhouseWorker, "Greenhouse Worker" },
        { ProfessionTypes.GreenhouseWorkshopWorker, "Greenhouse Workshop Worker" },
        { ProfessionTypes.GrillWorker, "Grill Worker" },
        { ProfessionTypes.GuildMember, "Guild member" },
        { ProfessionTypes.Hauler, "Hauler" },
        { ProfessionTypes.Harvester, "Harvester" },
        { ProfessionTypes.Herbalist, "Herbalist" },
        { ProfessionTypes.KilnWorker, "Kiln worker" },
        { ProfessionTypes.LeatherWorker, "Leatherworker" },
        { ProfessionTypes.Librarian, "Librarian" },
        { ProfessionTypes.LumberMillWorker, "Lumbermill worker" },
        { ProfessionTypes.Manufacturer, "Manufacturer" },
        { ProfessionTypes.MillWorker, "Mill worker" },
        { ProfessionTypes.Miner, "Miner" },
        { ProfessionTypes.Monk, "Monk" },
        { ProfessionTypes.Outfitter, "Outfitter" },
        { ProfessionTypes.OvenWorker, "Oven worker" },
        { ProfessionTypes.PantryWorker, "Pantry worker" },
        { ProfessionTypes.Priest, "Priest" },
        { ProfessionTypes.PressWorker, "Press Worker" },
        { ProfessionTypes.Provisioner, "Provisioner" },
        { ProfessionTypes.RainCatcherWorker, "Rain Catcher Worker" },
        { ProfessionTypes.RainCollectorWorker, "Rain Collector Worker" },
        { ProfessionTypes.Rancher, "Rancher" },
        { ProfessionTypes.Scavenger, "Scavenger" },
        { ProfessionTypes.Scout, "Scout" },
        { ProfessionTypes.Scribe, "Scribe" },
        { ProfessionTypes.Seller, "Seller" },
        { ProfessionTypes.Sewer, "Sewer" },
        { ProfessionTypes.Smelter, "Smelter" },
        { ProfessionTypes.Smith, "Smith" },
        { ProfessionTypes.SmokehouseWorker, "Smokehouse worker" },
        { ProfessionTypes.Speaker, "Speaker" },
        { ProfessionTypes.StampingMillWorker, "Stamping Mill Worker" },
        { ProfessionTypes.StoneCutter, "Stonecutter" },
        { ProfessionTypes.Supplier, "Supplier" },
        { ProfessionTypes.TeaDoctor, "Teadoctor" },
        { ProfessionTypes.TeaHouseWorker, "Teahouse Worker" },
        { ProfessionTypes.TincturyWorker, "Tinctury Worker" },
        { ProfessionTypes.Tinkerer, "Tinkerer" },
        { ProfessionTypes.ToolShopWorker, "Toolshop Worker" },
        { ProfessionTypes.Trapper, "Trapper" },
        { ProfessionTypes.Waiter, "Waiter" },
        { ProfessionTypes.Weaver, "Weaver" },
        { ProfessionTypes.Woodcutter, "Woodcutter" },
    };

    public static string ToName(this ProfessionTypes type)
    {
        if (TypeToInternalName.TryGetValue(type, out var value))
        {
            return value;
        }

        Plugin.Log.LogError("Cannot find name of Profession type: " + type);
        return ProfessionTypes.Alchemist.ToString();
    }

    public static ProfessionModel ToProfessionModel(this ProfessionTypes type)
    {
        var name = type.ToName();
        if (SO.Settings.professionsCache.Contains(SO.Settings.Professions, name))
        {
            return SO.Settings.GetProfessionModel(name);
        }

        Plugin.Log.LogError("Cannot find name of Profession model for type: " + type + " with name: " + name);
        return null;
    }
}