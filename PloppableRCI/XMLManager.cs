using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using ColossalFramework.Globalization;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using System.ComponentModel;
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
        public static BuildingData[] xmlData;
        public static BuildingData Instance;

        public void Run()
        {
            //This is the data object that holds all of the RICO settings. Its read by the tool panel and the settings panel. 
            //It contains one entry for every ploppable building, and any RICO settings they may have applied.

            xmlData = new BuildingData[PrefabCollection<BuildingInfo>.LoadedCount()];

            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);

                if (prefab.m_class.m_service == ItemClass.Service.Monument)
                {
                    //BuildingData buildingData = xmlData?[(int)i];
                    var buildingData = new BuildingData();
                    buildingData.id = (uint)prefab.m_prefabDataIndex;
                    buildingData.name = prefab.name;
                    //buildingData.author = new PloppableRICODefinition.Building();
                    //buildingData.local = new PloppableRICODefinition.Building();
                    //buildingData.mod = new PloppableRICODefinition.Building();
                    //buildingData.author.ricoEnabled = false;
                    //buildingData.local.ricoEnabled = false;
                   // buildingData.mod.ricoEnabled = false;
                    xmlData[(int)i] = buildingData;
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
                        xmlData[buildingPrefab.m_prefabDataIndex].local = local;
                        xmlData[buildingPrefab.m_prefabDataIndex].hasAuthor = true;
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

                        if (buildingPrefab != null)
                        {
                            var author = new PloppableRICODefinition.Building();
                            author = buildingDef;
                            author.manualCountEnabled = true;
                            author.ricoEnabled = true;
                            xmlData[buildingPrefab.m_prefabDataIndex].author = author;
                            xmlData[buildingPrefab.m_prefabDataIndex].hasAuthor = true;
                            //Debug.Log(xmlData[buildingPrefab.m_prefabDataIndex].name + " AUTHOR");
                        }
                    }
                    catch (Exception e)
                    {
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
        public uint id;
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

	public class PloppableRICODefinition
	{
		public List<Building> Buildings { get; set; }

		public PloppableRICODefinition ()
		{
			Buildings = new List<Building> ();
		}

		public class Building
		{
			[XmlAttribute ("name"), DefaultValue (null)]
			public string name { get; set; }

			[XmlAttribute ("service"), DefaultValue ("none")]
			public string service { get; set; }

			[XmlAttribute ("sub-service"), DefaultValue ("none")]
			public string subService { get; set; }

			[XmlAttribute ("construction-cost"), DefaultValue (1)]
			public int constructionCost { get; set; }

            [XmlAttribute("ui-category"), DefaultValue("none")]
            public string UICategory { get; set; }

            [XmlAttribute ("homes"), DefaultValue (0)]
			public int homeCount { get; set; }

            [XmlAttribute("level"), DefaultValue(1)]
            public int level { get; set; }

            //Pollution
            [XmlAttribute("pollution-radius"), DefaultValue(0)]
            public int pollutionRadius { get; set; }

            //Workplace settings
            [XmlAttribute("workplaces"), DefaultValue(0)]
            public int workplaceCount { get; set; }

            [XmlAttribute("uneducated"), DefaultValue(1)]
            public int uneducated { get; set; }

            [XmlAttribute("educated"), DefaultValue(1)]
            public int educated { get; set; }
       
            [XmlAttribute("welleducated"), DefaultValue(1)]
            public int wellEducated { get; set; }

            //Toggles
            [XmlAttribute("enable-popbalance"), DefaultValue(true)]
            public bool popbalanceEnabled { get; set; }

            [XmlAttribute("enable-rico"), DefaultValue(true)]
            public bool ricoEnabled { get; set; }

            [XmlAttribute("enable-educationratio"), DefaultValue(true)]
            public bool educationRatioEnabled { get; set; }

            [XmlAttribute("enable-pollution"), DefaultValue(true)]
            public bool pollutionEnabled { get; set; }

            [XmlAttribute("enable-manualcount"), DefaultValue(true)]
            public bool manualCountEnabled { get; set; }

            [XmlAttribute("enable-constructioncost"), DefaultValue(true)]
            public bool constructionCostEnabled { get; set; }

        }
	}
}



