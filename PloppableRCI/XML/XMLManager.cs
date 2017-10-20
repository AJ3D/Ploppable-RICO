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
using System.Linq;

namespace PloppableRICO
{
    /// <summary>
    ///This class tracks what prefabs have RICO settings applied. 
    /// </summary>
    /// 
    public class RICOPrefabManager
    {
        public static Dictionary<BuildingInfo, BuildingData> prefabHash;
        public static List<BuildingData> prefabList;
        //public static BuildingData Instance;

        public void Run()
        {

            //This is the data object that holds all of the RICO settings. Its read by the tool panel and the settings panel.
            //It contains one entry for every building, and any RICO settings they may have.

            prefabHash = new Dictionary<BuildingInfo, BuildingData>();
            prefabList = new List<BuildingData>();
            //Loop though all prefabs

            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {

                if (PrefabCollection<BuildingInfo>.GetLoaded(i) != null)
                {
                    var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    Debug.Log(prefab.name + " Loaded");
                    {
                        var buildingData = new BuildingData
                        {
                            prefab = prefab,
                            name = prefab.name,

                            density = SetPrefabDensity(prefab),

                            category = AssignCategory(prefab)
                        };

                        prefabList.Add(buildingData);
                        prefabHash[prefab] = buildingData;
                    }
                }
            }

            //RICO settings can come from 3 sources. Local settings are applied first, followed by asset author settings,
            //and then finaly settings from settings mods.

            //If local settings are present, load them.
            if (File.Exists("LocalRICOSettings.xml"))
            {
                RicoSettings("LocalRICOSettings.xml", isLocal: true);
            }

            //Import settings from asset folders.
            Debug.Log("Trying Asset");
            AssetSettings();

            //If settings mod is active, load its settings.
            if (Util.IsModEnabled(629850626uL))
            {
                var workshopModSettingsPath = Path.Combine(Util.SettingsModPath("629850626"), "WorkshopRICOSettings.xml");
                RicoSettings(workshopModSettingsPath, isMod: true);

            }
        }

        public int SetPrefabDensity(BuildingInfo prefab)
        {

            if (prefab.m_collisionHeight < 20) return 0; //under 4, assign low
            else if (prefab.m_collisionHeight >= 20 & prefab.m_collisionHeight < 45) return 1; //medium
            else if (prefab.m_collisionHeight > 45) return 2; //high
            else return 1;
        }

        //Load RICO Settings from asset folders
        public void AssetSettings()
        {
            var checkedPaths = new List<string>();
            var foo = new Dictionary<string, string>();


            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (prefab == null)
                    continue;

                // search for PloppableRICODefinition.xml
                var asset = PackageManager.FindAssetByName(prefab.name);

                if (asset == null || asset.package == null)
                    continue;

                var crpPath = asset.package.packagePath;
                if (crpPath == null)
                    continue;

                var ricoDefPath = Path.Combine(Path.GetDirectoryName(crpPath), "PloppableRICODefinition.xml");

                if (!checkedPaths.Contains(ricoDefPath))
                {
                    checkedPaths.Add(ricoDefPath);
                    foo.Add(asset.package.packageName, ricoDefPath);
                }
            }

            RicoSettings(foo, isAuthored: true);
        }

        public void RicoSettings(string ricoFileName, bool isLocal = false, bool isAuthored = false, bool isMod = false)
        {

            RicoSettings(new List<string>() { ricoFileName }, isLocal, isAuthored, isMod);
        }

        public void RicoSettings(List<string> ricoFileNames, bool isLocal = false, bool isAuthored = false, bool isMod = false)
        {
            var foo = new Dictionary<string, string>();

            foreach (var fileName in ricoFileNames)
            {
                foo.Add("", fileName);

            }

            RicoSettings(foo, isLocal, isAuthored, isMod);
        }

        void RicoSettings(Dictionary<string, string> foo, bool isLocal = false, bool isAuthored = false, bool isMod = false)
        {
            var allParseErrors = new List<string>();

            foreach (var packageId in foo.Keys)
            {
                var ricoDefPath = foo[packageId];

                if (!File.Exists(ricoDefPath))
                {
                    continue;
                }

                PloppableRICODefinition ricoDef = null;

                if (isLocal == true)
                {
                    ricoDef = RICOReader.ParseRICODefinition(packageId, ricoDefPath, insanityOK: true);
                }
                else {
                    ricoDef = RICOReader.ParseRICODefinition(packageId, ricoDefPath);
                }

                if (ricoDef != null)
                {
                    //Debug.Log("RICO Def isnt null");
                    var j = 0;
                    foreach (var buildingDef in ricoDef.Buildings)
                    {
                        j++;
                        BuildingInfo prefab;

                        prefab = Util.FindPrefab(buildingDef.name, packageId);

                        if (prefab != null)
                        {
                            if (prefabHash.ContainsKey(prefab))
                            {
                                if (isAuthored)
                                {
                                    prefabHash[prefab].author = buildingDef;
                                    prefabHash[prefab].hasAuthor = true;
                                }
                                else if (isLocal)
                                {
                                    prefabHash[prefab].local = buildingDef;
                                    prefabHash[prefab].hasLocal = true;
                                }
                                else if (isMod)
                                {
                                    //Debug.Log(prefabHash[pf].name + " Has Mod");
                                    prefabHash[prefab].mod = buildingDef;
                                    prefabHash[prefab].hasMod = true;
                                }
                            }

                            allParseErrors.AddRange(ricoDef.errors);
                        }
                    }
                }
                else
                {
                    allParseErrors.AddRange(RICOReader.LastErrors);
                }
            }

            if (allParseErrors.Count > 0)
            {
                var errorMessage = new StringBuilder();
                foreach (var error in allParseErrors)
                    errorMessage.Append(error).Append('\n');

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Ploppable RICO", errorMessage.ToString(), true);
            }
        }

        //List of categories for the settings panel. 
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
            else if (prefab.m_buildingAI is ResidentialBuildingAI)
            {
                return Category.Residential;
            }
            else if (prefab.m_buildingAI is IndustrialExtractorAI)
            {
                return Category.Industrial;
            }
            else if (prefab.m_buildingAI is IndustrialBuildingAI)
            {
                return Category.Industrial;
            }
            else if (prefab.m_buildingAI is OfficeBuildingAI)
            {
                return Category.Office;
            }
            else if (prefab.m_buildingAI is CommercialBuildingAI)
            {
                return Category.Commercial;
            }



            else return Category.Beautification;

        }


        //This is called by the settings panel. It will serialize any new local settings the player sets in game. 
        public static void SaveLocal(RICOBuilding newBuildingData)
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

        public int density;

        public Category category;

        //RICO settings. 
        public RICOBuilding local;
        public RICOBuilding author;
        public RICOBuilding mod;

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