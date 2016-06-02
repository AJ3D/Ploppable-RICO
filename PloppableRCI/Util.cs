using System;
using System.Linq;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;


namespace PloppableRICO
{
    internal static class Util
    {
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