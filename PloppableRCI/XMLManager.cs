using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using ColossalFramework.Globalization;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using System.IO;
using System.Xml.Serialization;
using ColossalFramework.Plugins;

namespace PloppableRICO
{
    /// <summary>
    ///This class reads the XML settings, and applies RICO settings to prefabs. Its based on boformers Sub-Building Enabler mod. Many thanks to him for his work. 
    /// </summary>
    /// 
    public class XMLManager
    {
        public static Dictionary<BuildingInfo, BuildingData> xmlData;
        public static BuildingData Instance;

        public void Run()
        {
            //This is the data object that holds all of the RICO settings. Its read by the tool panel and the settings panel. 
            //It contains one entry for every ploppable building, and any RICO settings they may have.

            xmlData = new Dictionary<BuildingInfo, BuildingData>();

            //Loop though all prefabs
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);

                //Add one entry for every ploppable building
                if (prefab.m_class.m_service == ItemClass.Service.Beautification ||
                   prefab.m_class.m_service == ItemClass.Service.Monument ||
                   prefab.m_class.m_service == ItemClass.Service.Electricity ||
                   prefab.m_class.m_service == ItemClass.Service.Education)
                {
                    var buildingData = new BuildingData
                    {
                        prefab = prefab,
                        name = prefab.name,
                        category = AssignCategory(prefab)
                    };

                    xmlData[prefab] = buildingData;
                }
            }

            //RICO settings can come from 3 sources. Local settings are applied first, followed by asset author settings,
            //and then finaly settings from settings mods. 

            //If local settings are present, load them. 
            if (File.Exists("LocalRICOSettings.xml"))
            {
                LocalSettings();
            }

            //Import settings from asset folders. 
            AssetSettings();

            //If settings mod is active, load its settings. (disabled for now)
            if (Util.IsModEnabled(629850626uL))
            {
                //ModSettings();
            }

        }

        //Load local RICO settings. 
        public void LocalSettings()
        {
            try
            {
                PloppableRICODefinition localSettings = null;

                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));

                using (StreamReader streamReader = new System.IO.StreamReader("LocalRICOSettings.xml"))
                {
                    localSettings = xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
                }

                foreach (var buildingDef in localSettings.Buildings)
                {
                    if (PrefabCollection<BuildingInfo>.FindLoaded(buildingDef.name) != null)
                    {
                        var buildingPrefab = PrefabCollection<BuildingInfo>.FindLoaded(buildingDef.name);
                        if (buildingPrefab != null)
                        {
                            //Add local RICO settings to dictionary
                            var local = new PloppableRICODefinition.Building();
                            local = buildingDef;
                            xmlData[buildingPrefab].local = local;
                            xmlData[buildingPrefab].hasLocal = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        //Load RICO Settings from asset folders
        public void AssetSettings()
        {

            var ricoDefParseErrors = new List<string>();
            var checkedPaths = new List<string>();

            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (prefab == null)
                    continue;

                // search for PloppableRICODefinition.xml
                var asset = PackageManager.FindAssetByName(prefab.name);
                if (asset == null || asset.package == null)
                    continue;

                var crpPath = asset.pathOnDisk;
                if (crpPath == null)
                    continue;

                var ricoDefPath = Path.Combine(Path.GetDirectoryName(crpPath), "PloppableRICODefinition.xml");
                var pn = asset.name;

                // skip files which were already parsed
                if (checkedPaths.Contains(ricoDefPath))
                    continue;
                checkedPaths.Add(ricoDefPath);

                if (!File.Exists(ricoDefPath))
                    continue;

                PloppableRICODefinition ricoDef = RICOReader.ParseRICODefinition(pn, ricoDefPath, ricoDefParseErrors);

                foreach (var buildingDef in ricoDef.Buildings)
                {
                    if (buildingDef.prefab == null)
                    {
                        ricoDefParseErrors.Add(String.Format("Error while processing RICO - file {0} at building #{1}. ({2})", pn, i, "Building has not been loaded. Either it is broken, deactivated or not subscribed to." + buildingDef.name + " not loaded."));
                    }
                    else
                    {
                        //Add asset author settings to dictionary. 
                        xmlData[buildingDef.prefab].author = buildingDef;
                        xmlData[buildingDef.prefab].hasAuthor = true;
                    }
                }
            }

            if (ricoDefParseErrors.Count > 0)
            {
                var errorMessage = new StringBuilder();
                foreach (var error in ricoDefParseErrors)
                    errorMessage.Append(error).Append('\n');

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Ploppable RICO", errorMessage.ToString(), true);
            }
        }

        //Load settings from settings mods. (Currently disabled)
        public void ModSettings()
        {
            var workshopModSettingsPath = Path.Combine(Util.SettingsModPath("629850626"), "WorkshopRICOSettings.xml");
            var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));

            PloppableRICODefinition workshopSettings = null;

            using (StreamReader streamReader = new System.IO.StreamReader(workshopModSettingsPath))
            {
                workshopSettings = xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
            }
            foreach (var buildingDef in workshopSettings.Buildings)
            {
                if (PrefabCollection<BuildingInfo>.FindLoaded(buildingDef.name) != null)
                {
                    var buildingPrefab = PrefabCollection<BuildingInfo>.FindLoaded(buildingDef.name);
                    //Add settings mod settings here

                }
            }
        }

        private Category AssignCategory(BuildingInfo prefab)
        {

            if (prefab.m_buildingAI is MonumentAI)
            {
                return Category.Monument;
            }
            else if (prefab.m_buildingAI is ParkAI)
            {
                return Category.Beautification;
            }
            else if (prefab.m_buildingAI is PowerPlantAI)
            {
                return Category.Power;
            }
            else if (prefab.m_buildingAI is WaterFacilityAI)
            {
                return Category.Water;
            }
            else if (prefab.m_buildingAI is SchoolAI)
            {
                return Category.Education;
            }
            else if (prefab.m_buildingAI is HospitalAI)
            {
                return Category.Health;
            }

            else return Category.Beautification;

        }

        //This is called by the settings panel. It will serialize any new local settings the player sets in game. 
        public static void SaveLocal(PloppableRICODefinition.Building newBuildingData)
        {
            Debug.Log("SaveLocal");
            
            if (File.Exists("LocalRICOSettings.xml") && newBuildingData != null)
            {

                PloppableRICODefinition localSettings = null;
                var newlocalSettings = new PloppableRICODefinition();

                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));

                using (StreamReader streamReader = new System.IO.StreamReader("LocalRICOSettings.xml"))
                {
                    localSettings = xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
                }

                foreach (var buildingDef in localSettings.Buildings)
                {
                    if (buildingDef.name != newBuildingData.name)
                    {
                        newlocalSettings.Buildings.Add(buildingDef);
                    }
                }

                //newBuildingData.name = newBuildingData.name;
                newlocalSettings.Buildings.Add(newBuildingData);

                using (TextWriter writer = new StreamWriter("LocalRICOSettings.xml"))
                {
                    xmlSerializer.Serialize(writer, newlocalSettings);
                }
            } 
        }
    }

    //This is the data object definition for the dictionary. It contains one entry for every ploppable building. 
    //Each entry contains up to 3 PloppableRICODef entries. 

    public class BuildingData
    {
        private string m_displayName;
        public BuildingInfo prefab;
        public string name;
        public Category category;

        //RICO settings. 
        public PloppableRICODefinition.Building local;
        public PloppableRICODefinition.Building author;
        public PloppableRICODefinition.Building mod;

        //These are used by the settings panel and tool to determine which settings to use. It will first use local, then asset, and finaly mod. 
        public bool hasAuthor;
        public bool hasLocal;
        public bool hasMod;

        //Called by the settings panel fastlist. 
        public string displayName
        {
            get
            {
                m_displayName = Locale.GetUnchecked("BUILDING_TITLE", name);
                if (m_displayName.StartsWith("BUILDING_TITLE"))
                {
                    m_displayName = name.Substring(name.IndexOf('.') + 1).Replace("_Data", "");
                }
                m_displayName = CleanName(m_displayName, !name.Contains("."));

                return m_displayName;
            }
        }
        public static string CleanName(string name, bool cleanNumbers = false)
        {
            name = Regex.Replace(name, @"^{{.*?}}\.", "");
            name = Regex.Replace(name, @"[_+\.]", " ");
            name = Regex.Replace(name, @"(\d[xX]\d)|([HL]\d)", "");
            if (cleanNumbers)
            {
                name = Regex.Replace(name, @"(\d+[\da-z])", "");
                name = Regex.Replace(name, @"\s\d+", " ");
            }
            name = Regex.Replace(name, @"\s+", " ").Trim();

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
        }
    }
}



