using System;
using System.Linq;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using System.Collections.Generic;

namespace PloppableRICO
{
    public static class Util
    {
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
            int[] workplaceDistribution = null;

            if ( distributions.ContainsKey( service + subservice ) )
                workplaceDistribution = distributions[service + subservice];

            if ( distributions.ContainsKey( service + subservice + level ) )
                workplaceDistribution = distributions[service + subservice + level];

            return workplaceDistribution;

        }

        public static void TRACE( string line )
        {
            try
            {
                var fw = new System.IO.StreamWriter(@"d:\log.txt", true);
                fw.WriteLine( line );
                fw.Close();
            }
            catch { }
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
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        public static BuildingInfo FindPrefab(string prefabName, string packageName)
        {
            Util.TRACE( String.Format( "Find prefab: prefabName = {0}, packageName {1}", prefabName, packageName ) );

            var prefab = PrefabCollection<BuildingInfo>.FindLoaded(prefabName);
            if (prefab == null)
                prefab = PrefabCollection<BuildingInfo>.FindLoaded(prefabName + "_Data");
            if (prefab == null)
                prefab = PrefabCollection<BuildingInfo>.FindLoaded(ColossalFramework.IO.PathEscaper.Escape(prefabName) + "_Data");
            if (prefab == null)
                prefab = PrefabCollection<BuildingInfo>.FindLoaded(packageName + "." + prefabName + "_Data");
            if (prefab == null)
                prefab = PrefabCollection<BuildingInfo>.FindLoaded(packageName + "." + ColossalFramework.IO.PathEscaper.Escape(prefabName) + "_Data");

            Util.TRACE( String.Format( "Find Asset: found = {0}", (prefab != null).ToString() ) );
            return prefab;
        }


        //This is run in the SimulationStep of all the ploppable AI's. 
        public static void buildingFlags(ref Building buildingData) {

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

                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);

                if (prefab.m_buildingAI is PloppableRICO.PloppableExtractor || prefab.m_buildingAI is PloppableResidential
                    || prefab.m_buildingAI is PloppableOffice || prefab.m_buildingAI is PloppableCommercial ||
                    prefab.m_buildingAI is PloppableIndustrial)
                {

                    // Just assign any RICO prefab a ploppable ItemClass so it will reload. It gets set back once the mod loads. 
                    prefab.m_class = ItemClassCollection.FindClass("Beautification Item");
                    prefab.InitializePrefab();
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

            return Steam.IsDlcInstalled(369150u);
        }

        public static bool isSFinstalled()
        {
            return Steam.IsDlcInstalled(420610u);
        }

      
    }

}