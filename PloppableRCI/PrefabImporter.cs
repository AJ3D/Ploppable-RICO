using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace PloppableRICO
{
	/// <summary>
	///This class reads the XML settings, and applies RICO settings to prefabs. Its based on boformers Sub-Building Enabler mod. Many thanks to him for his work. 
	/// </summary>
	/// 
	public class PrefabImporter
	{
		public Dictionary<BuildingInfo, string> Run ()
		{
            var prefabToCategoryMap = new Dictionary<BuildingInfo, string>();

            var ricoDefParseErrors = new HashSet<string> ();
			var checkedPaths = new List<string> ();

			for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount (); i++)
            {
				var prefab = PrefabCollection<BuildingInfo>.GetLoaded (i);
				if (prefab == null) continue;

                // search for PloppableRICODefinition.xml
                var asset = PackageManager.FindAssetByName (prefab.name);
				if (asset == null || asset.package == null)
					continue;

				var crpPath = asset.package.packagePath;
				if (crpPath == null)
					continue;

				var ricoDefPath = Path.Combine (Path.GetDirectoryName (crpPath), "PloppableRICODefinition.xml");

				// skip files which were already parsed
				if (checkedPaths.Contains (ricoDefPath))
					continue;
				checkedPaths.Add (ricoDefPath);

				if (!File.Exists (ricoDefPath))
					continue;

				PloppableRICODefinition ricoDef = null;

				var xmlSerializer = new XmlSerializer (typeof(PloppableRICODefinition));
				try {
					using (StreamReader streamReader = new System.IO.StreamReader (ricoDefPath)) {
						ricoDef = xmlSerializer.Deserialize (streamReader) as PloppableRICODefinition;
					}
				} catch (Exception e) {
					Debug.LogException (e);
					ricoDefParseErrors.Add (asset.package.packageName + " - " + e.Message);
					continue;
				}

				if (ricoDef == null || ricoDef.Buildings == null || ricoDef.Buildings.Count == 0) {
					ricoDefParseErrors.Add (asset.package.packageName + " - ricoDef is null or empty.");
					continue;
				}

				foreach (var buildingDef in ricoDef.Buildings)
                {
					if (buildingDef == null || buildingDef.Name == null)
                    {
						ricoDefParseErrors.Add (asset.package.packageName + " - Building name missing.");
						continue;
					}

					var parentBuildingPrefab = FindPrefab (buildingDef.Name, asset.package.packageName);

					if (parentBuildingPrefab == null)
                    {
						ricoDefParseErrors.Add (asset.package.packageName + " - Building with name " + buildingDef.Name + " not loaded.");
						continue;
					}

                    prefabToCategoryMap.Add (parentBuildingPrefab, buildingDef.UICategory);

                    // INIT RICO SETTINGS FOR BUILDING
                    try {
                        InitRICO(buildingDef.Service, buildingDef.SubService, parentBuildingPrefab, buildingDef.LevelMax,
                            buildingDef.LevelMin, buildingDef.HomeCount, buildingDef.WorkplaceCount, buildingDef.ConstructionCost);
                    }
                    catch (Exception e)
                    {
                        ricoDefParseErrors.Add(asset.package.packageName + " - " + e.Message);
                    }
				}
			}

			if (ricoDefParseErrors.Count > 0) {
				var errorMessage = "Error while parsing Ploppable RICO definition file(s). Contact the author of the assets. \n"
				                   + "List of errors:\n";
				foreach (var error in ricoDefParseErrors)
					errorMessage += error + '\n';

				UIView.library.ShowModal<ExceptionPanel> ("ExceptionPanel").SetMessage ("Ploppable RICO", errorMessage, true);
			}

			return prefabToCategoryMap;
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

        public void InitRICO(string service, string subService, BuildingInfo prefab, int levelMax, int levelMin, int homeCount, int workplaceCount, int constructionCost)
        {
            if (service == "Residential")
            {
                var ai = prefab.gameObject.AddComponent<PloppableResidential>();
                prefab.m_buildingAI = ai;

                ai.m_homeCount = homeCount;
                ai.m_constructionCost = constructionCost;
                ai.m_constructionTime = 0;

                if (subService == "Low")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Low Residential - Level" + levelMin);
                }
                else if (subService == "High")
                {
                    prefab.m_class = ItemClassCollection.FindClass("High Residential - Level" + levelMin);
                }
                else
                {
                    throw new Exception("Invalid SubService " + subService + "!");
                }
            }

            else if (service == "Office")
            {
                var ai = prefab.gameObject.AddComponent<PloppableOffice>();
                prefab.m_buildingAI = ai;

                ai.m_workplaceCount = workplaceCount;
                ai.m_constructionCost = constructionCost;
                ai.m_constructionTime = 0;

                prefab.m_class = ItemClassCollection.FindClass("Office - Level" + levelMin);
            }

            else if (service == "Industrial")
            {
                var ai = prefab.gameObject.AddComponent<PloppableIndustrial>();
                prefab.m_buildingAI = ai;

                ai.m_workplaceCount = workplaceCount;
                ai.m_constructionCost = constructionCost;
                ai.m_constructionTime = 0;

                if (subService == "Farming")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Farming - Processing");
                }
                else if (subService == "Forest")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Forest - Processing");
                }
                else if (subService == "Oil")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Oil - Processing");
                }
                else if (subService == "Ore")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Ore - Processing");
                }
                else if (subService == "Generic")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Industrial - Level" + levelMin);
                }
                else
                {
                    throw new Exception("Invalid SubService " + subService + "!");
                }
            }

            else if (service == "Extractor")
            {
                var ai = prefab.gameObject.AddComponent<PloppableExtractor>();
                prefab.m_buildingAI = ai;

                ai.m_workplaceCount = workplaceCount;
                ai.m_constructionCost = constructionCost;
                ai.m_constructionTime = 0;

                if (subService == "Farming")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Farming - Extractor");
                }
                else if (subService == "Forest")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Forest - Extractor");
                }
                else if (subService == "Oil")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Oil - Extractor");
                }
                else if (subService == "Ore")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Ore - Extractor");
                }
                else
                {
                    throw new Exception("Invalid SubService " + subService + "!");
                }
            }

            else if (service == "Commercial")
            {
                var ai = prefab.gameObject.AddComponent<PloppableCommercial>();
                prefab.m_buildingAI = ai;

                ai.m_workplaceCount = workplaceCount;
                ai.m_constructionCost = constructionCost;
                ai.m_constructionTime = 0;

                if (subService == "Low")
                {
                    prefab.m_class = ItemClassCollection.FindClass("Low Commercial - Level" + levelMin);
                }
                else if (subService == "High")
                {
                    prefab.m_class = ItemClassCollection.FindClass("High Commercial - Level" + levelMin);
                }
                else
                {
                    throw new Exception("Invalid SubService " + subService + "!");
                }
            }

            else
            {
                throw new Exception("Invalid Service " + service + "!");
            }

            prefab.m_buildingAI.m_info = prefab;
            prefab.InitializePrefab();
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
			public string Name { get; set; }

			[XmlAttribute ("service"), DefaultValue ("dummy")]
			public string Service { get; set; }

			[XmlAttribute ("sub-service"), DefaultValue ("dummy")]
			public string SubService { get; set; }

			[XmlAttribute ("construction-cost"), DefaultValue (1)]
			public int ConstructionCost { get; set; }

            [XmlAttribute("homes"), DefaultValue(0)]
            public int HomeCount { get; set; }

            [XmlAttribute("workplaces"), DefaultValue(0)]
            public int WorkplaceCount { get; set; }

            [XmlAttribute ("level-min"), DefaultValue (1)]
			public int LevelMin { get; set; }

			[XmlAttribute ("level-max"), DefaultValue (1)]
			public int LevelMax { get; set; }

			[XmlAttribute ("ui-category"), DefaultValue ("reshigh")]
			public string UICategory { get; set; }
		}
	}
}



