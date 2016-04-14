using System;
using System.Linq;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;


namespace PloppableRICO
{
    internal static class Util
    {

        public static void buildingFlags(ref Building buildingData) {

            buildingData.m_garbageBuffer = 100;
            buildingData.m_majorProblemTimer = 0;
            buildingData.m_levelUpProgress = 0;
			buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
			buildingData.m_flags &= ~Building.Flags.Abandoned;
			buildingData.m_flags &= ~Building.Flags.Demolishing;
            buildingData.m_problems &= ~Notification.Problem.TurnedOff;
            //buildingData.m_flags &= ~Building.Flags.
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