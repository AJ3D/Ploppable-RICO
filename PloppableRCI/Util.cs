using System;
using System.Linq;
using ColossalFramework.Plugins;
using System.Collections.Generic;
using System.IO;

namespace PloppableRICO
{
#if DEBUG
    //[ProfilerAspect()]
#endif
    public static class Util
    {
        public static FileInfo crpFileIn( DirectoryInfo d )
        {
            try
            {
                var f = d.GetFiles("*.crp");
                if ( f != null && f.Count() == 1 )
                    return f[0];
            }
            catch
            {
            }
            return null;
        }

        public static FileInfo ricoFileIn( DirectoryInfo d, FileInfo crpFile )
        {
            var p = Path.Combine(d.FullName, "PloppableRICODefinition.xml");
            if ( File.Exists( p ) )
                return new FileInfo( p );

            var n = Path.Combine(d.FullName, Path.GetFileNameWithoutExtension(crpFile.Name) + ".rico");
            if ( File.Exists( n ) ) return new FileInfo( n );

            return null;
        }

        public static int[] WorkplaceDistributionOf(string service, string subservice, string level)
        {
            Dictionary<String, int[]>distributions = new Dictionary<String, int[]>()
            {
                { "IndustrialIndustrialFarming", new int[] { 100, 100, 0, 0, 0 } },
                { "IndustrialIndustrialForestry", new int[] { 100, 100, 0, 0, 0 } },
                { "IndustrialIndustrialOre", new int[] { 100, 20, 60, 20, 0 } },
                { "IndustrialIndustrialOil", new int[] { 100, 20, 60, 20, 0 } },
                { "IndustrialIndustrialGenericLevel1", new int[] { 100, 100, 0, 0, 0 } },
                { "IndustrialIndustrialGenericLevel2", new int[] { 100, 20, 50, 20, 0 } },
                { "IndustrialIndustrialGenericLevel3", new int[] { 100, 15, 55, 25, 5 } },
                { "OfficeNoneLevel1", new int[] { 100, 0, 40, 50, 10 } },
                { "OfficeNoneLevel2", new int[] { 100, 0, 20, 50, 30 } },
                { "OfficeNoneLevel3", new int[] { 100, 0, 0, 40, 60 } },
                { "ExtractorIndustrialFarming", new int[] { 100, 100, 0, 0, 0 } },
                { "ExtractorIndustrialForestry", new int[] { 100, 100, 0, 0, 0 } },
                { "ExtractorIndustrialOre", new int[] { 100, 20, 60, 20, 0 } },
                { "ExtractorIndustrialOil", new int[] { 100, 20, 60, 20, 0 } },
                { "CommercialCommercialTourist", new int[] { 100, 20, 20, 30, 30 } },
                { "CommercialCommercialLeisure", new int[] { 100, 30, 30, 20, 20 } },
                { "CommercialCommercialLowLevel1", new int[] { 100, 100, 0, 0, 0 } },
                { "CommercialCommercialLowLevel2", new int[] { 100, 20, 60, 20, 0 } },
                { "CommercialCommercialLowLevel3", new int[] { 100, 5, 15, 30, 50 } },
                { "CommercialCommercialHighLevel1", new int[] { 100, 0, 40, 50, 10 } },
                { "CommercialCommercialHighLevel2", new int[] { 100, 0, 20, 50, 30 } },
                { "CommercialCommercialHighLevel3", new int[] { 100, 0, 0, 40, 60 } },
            };


            distributions.Add( "industrialfarming", distributions["IndustrialIndustrialFarming"] );
            distributions.Add( "industrialforestry", distributions["IndustrialIndustrialForestry"] );
            distributions.Add( "industrialore", distributions["IndustrialIndustrialOre"] );
            distributions.Add( "industrialoil", distributions["IndustrialIndustrialOil"] );
            distributions.Add( "industrialgenericLevel1", distributions[ "IndustrialIndustrialGenericLevel1"] );
            distributions.Add( "industrialgenericLevel2", distributions[ "IndustrialIndustrialGenericLevel2"] );
            distributions.Add( "industrialgenericLevel3", distributions[ "IndustrialIndustrialGenericLevel3"] );
            distributions.Add( "officenoneLevel1", distributions[ "OfficeNoneLevel1"] );
            distributions.Add( "officenoneLevel2", distributions[ "OfficeNoneLevel2"] );
            distributions.Add( "officenoneLevel3", distributions[ "OfficeNoneLevel3"] );
            distributions.Add( "extractorfarming", distributions["ExtractorIndustrialFarming"] );
            distributions.Add( "extractorforestry", distributions["ExtractorIndustrialForestry"] );
            distributions.Add( "extractorore", distributions["ExtractorIndustrialOre"] );
            distributions.Add("extractoroil", distributions["ExtractorIndustrialOil"] );
            distributions.Add("commercialtourist", distributions["CommercialCommercialTourist"] );
            distributions.Add( "commercialleisure", distributions["CommercialCommercialLeisure"] );
            distributions.Add( "commerciallowLevel1", distributions[ "CommercialCommercialLowLevel1"] );
            distributions.Add( "commerciallowLevel2", distributions[ "CommercialCommercialLowLevel2"] );
            distributions.Add( "commerciallowLevel3", distributions[ "CommercialCommercialLowLevel3"] );
            distributions.Add( "commercialhighLevel1", distributions[ "CommercialCommercialHighLevel1"] );
            distributions.Add( "commercialhighLevel2", distributions[ "CommercialCommercialHighLevel2"] );
            distributions.Add( "commercialhighLevel3", distributions[ "CommercialCommercialHighLevel3"] );

            int[] workplaceDistribution = null;

            if ( distributions.ContainsKey( service + subservice ) )
                workplaceDistribution = distributions[service + subservice];

            if ( distributions.ContainsKey( service + subservice + level ) )
                workplaceDistribution = distributions[service + subservice + level];

            if ( workplaceDistribution != null )
                return workplaceDistribution;
            else
                return new int[] { 100, 25, 25, 25, 25 };

        }


        public static int MaxLevelOf( string service, string subservice )
        {
            return service == "residential" ? 5 :
                   service == "office" ? 3 :
                   service == "commercial" ? 3 :
                   service == "industrial" && subservice == "generic" ? 3 :
                   1;
        }
        
        public static string UICategoryOf(string service, string subservice)
        {
            var category = "";
            if ( service == "" || subservice == "" )
                return "";

            switch (service)
            {
                case "residential": category = subservice == "high" ? "reshigh" : "reslow"; break;
                case "commercial": category = subservice == "high" ? "comhigh" : "comlow"; break;
                case "office": category = "office"; break;
                case "industrial": category = subservice == "generic" ? "industrial" : subservice; break;
                case "extractor": category = subservice; break;
            }
            return category;
        }

        public static List<String> industryServices = new List<String>() { "farming", "forest", "oil", "ore" };
        public static List<String> vanillaCommercialServices = new List<String>() { "low", "high" };
        public static List<String> afterDarkCommercialServices = new List<String>() { "low", "high", "tourist", "leisure" };

        // returns s but with first character upper cased
        public static string ucFirst(String s)
        {
            if (s == "forest"){
                return "Forestry";
            }else
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        public static BuildingInfo FindPrefab(string prefabName, string packageName)
        {
            //Profiler.Info( String.Format( "Find prefab: prefabName = {0}, packageName {1}", prefabName, packageName ) );

            var prefab = PrefabCollection<BuildingInfo>.FindLoaded(prefabName);
            if (prefab == null)
                prefab = PrefabCollection<BuildingInfo>.FindLoaded(prefabName + "_Data");
            if (prefab == null)
                prefab = PrefabCollection<BuildingInfo>.FindLoaded(ColossalFramework.IO.PathEscaper.Escape(prefabName) + "_Data");
            if (prefab == null)
                prefab = PrefabCollection<BuildingInfo>.FindLoaded(packageName + "." + prefabName + "_Data");
            if (prefab == null)
                prefab = PrefabCollection<BuildingInfo>.FindLoaded(packageName + "." + ColossalFramework.IO.PathEscaper.Escape(prefabName) + "_Data");

            //Profiler.Info( String.Format( "Find Asset: found = {0}", (prefab != null).ToString() ) );
            return prefab;
        }


        //This is run in the SimulationStep of all the ploppable AI's. 
        public static void buildingFlags(ref Building buildingData) {

            //A set of flags to apply to RICO buildings before/after each sim step. Sloppy, but it avoids having to mess with simstep code. 
            buildingData.m_garbageBuffer = 100;
            buildingData.m_majorProblemTimer = 0;
            buildingData.m_levelUpProgress = 0;
			buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
			buildingData.m_flags &= ~Building.Flags.Abandoned;
			buildingData.m_flags &= ~Building.Flags.Demolishing;
            //This will solve the "Turned Off" error. 
            buildingData.m_problems &= ~Notification.Problem.TurnedOff;
        }

        public static void AssignServiceClass() {

            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                if (PrefabCollection<BuildingInfo>.GetLoaded(i) != null)
                {
                    var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);

                    //f (prefab.m_isCustomContent)

                    if (prefab.m_buildingAI is PloppableRICO.PloppableExtractor || prefab.m_buildingAI is PloppableRICO.PloppableResidential
                        || prefab.m_buildingAI is PloppableRICO.PloppableOffice || prefab.m_buildingAI is PloppableRICO.PloppableCommercial ||
                        prefab.m_buildingAI is PloppableRICO.PloppableIndustrial)
                    {
                        // Just assign any RICO prefab a ploppable ItemClass so it will reload. It gets set back once the mod loads. 
                        //ConvertPrefabs.InitializePrefab(prefab, ResidentialBuildingAI, ItemClass.ser)
                        prefab.m_class = ItemClassCollection.FindClass("Low Residential - Level1");
                        prefab.m_placementStyle = ItemClass.Placement.Manual;
                        prefab.InitializePrefab();

                    }
                }
            }  
        }


        public static bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }

        public static string SettingsModPath(string name)
        {
            var modList = PluginManager.instance.GetPluginsInfo();
            var modPath = "null";

            foreach (var modInfo in modList)
            {
                if (modInfo.name == name)
                {
                    modPath = modInfo.modPath;
                }
            }
            return modPath;
        }

        public static bool isADinstalled() {

            return SteamHelper.IsDLCOwned(SteamHelper.DLC.AfterDarkDLC);
        }

        public static bool isSFinstalled()
        {
            return SteamHelper.IsDLCOwned(SteamHelper.DLC.SnowFallDLC);
        }

        public static bool isGCinstalled()
        {
            return SteamHelper.IsDLCOwned(SteamHelper.DLC.GreenCitiesDLC);
        }


    }

}