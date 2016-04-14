using ColossalFramework.UI;
using System;
using System.Collections.Generic;
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
            //It contains one entry for every ploppable building, and any RICO settings they may have applied.

            xmlData = new Dictionary<BuildingInfo, BuildingData>();

            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);

                if (prefab.m_class.m_service == ItemClass.Service.Monument)
                {
                    //BuildingData buildingData = xmlData?[(int)i];
                    var buildingData = new BuildingData
                    {
                        prefab = prefab,
                        name = prefab.name
                    };
                    //buildingData.author = new PloppableRICODefinition.Building();
                    //buildingData.local = new PloppableRICODefinition.Building();
                    //buildingData.mod = new PloppableRICODefinition.Building();
                    //buildingData.author.ricoEnabled = false;
                    //buildingData.local.ricoEnabled = false;
                   // buildingData.mod.ricoEnabled = false;
                    xmlData[prefab] = buildingData;
                    //Debug.Log(prefab.name);?
                    // Debug.Log(xmlData[(int)i].name + "In Array");
                }
            }

            if (File.Exists("LocalRICOSettings.xml"))
            {
                LocalSettings();
            }

            AssetSettings();

            if (Util.IsModEnabled(629850626uL))
            {
                //ModSettings();
            }

         }

        public void LocalSettings()
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
                        var local = new PloppableRICODefinition.Building();
                        local = buildingDef;
                        local.manualCountEnabled = true;
                        local.ricoEnabled = true;
                        xmlData[buildingPrefab].local = local;
                        xmlData[buildingPrefab].hasAuthor = true;
                    }
                }
            }
        }

        //Load RICO Settings from asset folders
        public void AssetSettings() {

            var ricoDefParseErrors = new HashSet<string>();
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

                var crpPath = asset.package.packagePath;
                if (crpPath == null)
                    continue;

                var ricoDefPath = Path.Combine(Path.GetDirectoryName(crpPath), "PloppableRICODefinition.xml");

                // skip files which were already parsed
                if (checkedPaths.Contains(ricoDefPath))
                    continue;
                checkedPaths.Add(ricoDefPath);

                if (!File.Exists(ricoDefPath))
                    continue;

                PloppableRICODefinition ricoDef = null;

                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));
                try
                {
                    using (StreamReader streamReader = new System.IO.StreamReader(ricoDefPath))
                    {
                        ricoDef = xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    ricoDefParseErrors.Add(asset.package.packageName + " - " + e.Message);
                    continue;
                }

                if (ricoDef == null || ricoDef.Buildings == null || ricoDef.Buildings.Count == 0)
                {
                    ricoDefParseErrors.Add(asset.package.packageName + " - ricoDef is null or empty.");
                    continue;
                }

                foreach (var buildingDef in ricoDef.Buildings)
                {
                    if (buildingDef == null || buildingDef.name == null)
                    {
                        ricoDefParseErrors.Add(asset.package.packageName + " - Building name missing.");
                        continue;
                    }

                    var buildingPrefab = FindPrefab(buildingDef.name, asset.package.packageName);

                    if (buildingPrefab == null)
                    {
                        ricoDefParseErrors.Add(asset.package.packageName + " - Building with name " + buildingDef.name + " not loaded.");
                        continue;
                    }

                    if (buildingDef.subService == null) {
                        buildingDef.subService = "none";
                    }

                    // INIT RICO SETTINGS FOR BUILDING
                    try
                    {
                        UnityEngine.Debug.Log($"data index: {buildingPrefab.m_prefabDataIndex}");

                        if (buildingPrefab != null)
                        {
                            var author = new PloppableRICODefinition.Building();
                            author = buildingDef;
                            author.manualCountEnabled = true;
                            author.ricoEnabled = true;
                            xmlData[buildingPrefab].author = author;
                            xmlData[buildingPrefab].hasAuthor = true;
                            //Debug.Log(xmlData[buildingPrefab.m_prefabDataIndex].name + " AUTHOR");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        ricoDefParseErrors.Add(asset.package.packageName + " - " + e.Message);
                    }
                }
            }

            if (ricoDefParseErrors.Count > 0)
            {
                var errorMessage = "Error while parsing Ploppable RICO definition file(s). Contact the author of the assets. \n"
                                   + "List of errors:\n";
                foreach (var error in ricoDefParseErrors)
                    errorMessage += error + '\n';

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Ploppable RICO", errorMessage, true);
            }
        }

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
                    }
                }
            }


        private BuildingInfo FindPrefab (string prefabName, string packageName)
		{
			var prefab = PrefabCollection<BuildingInfo>.FindLoaded (prefabName);
			if (prefab == null)
				prefab = PrefabCollection<BuildingInfo>.FindLoaded (prefabName + "_Data");
			if (prefab == null)
				prefab = PrefabCollection<BuildingInfo>.FindLoaded (PathEscaper.Escape (prefabName) + "_Data");
			if (prefab == null)
				prefab = PrefabCollection<BuildingInfo>.FindLoaded (packageName + "." + prefabName + "_Data");
			if (prefab == null)
				prefab = PrefabCollection<BuildingInfo>.FindLoaded (packageName + "." + PathEscaper.Escape (prefabName) + "_Data");

			return prefab;
		}

        public static void SaveLocal(BuildingData newBuildingData)
        {

            if (File.Exists("LocalRICOSettings.xml"))
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

                newBuildingData.author.name = newBuildingData.name;
                newlocalSettings.Buildings.Add(newBuildingData.local);

                using (TextWriter writer = new StreamWriter("LocalRICOSettings.xml"))
                {
                    xmlSerializer.Serialize(writer, newlocalSettings);
                }
            }
        }
    }

    
 

    public class BuildingData
    {
        private string m_displayName;
        public BuildingInfo prefab;
        public string name;

        public PloppableRICODefinition.Building local;
        public PloppableRICODefinition.Building author;
        public PloppableRICODefinition.Building mod;

        public bool hasAuthor;
        public bool hasLocal;
        public bool hasMod;

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



