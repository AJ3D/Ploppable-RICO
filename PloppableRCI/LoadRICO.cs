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
	public class LoadRICO
	{

		InitRICOPrefabs InitRICO = new InitRICOPrefabs ();

		public List<String[]> Run (List<string[]> BuildingNames)
		{
			var subBuildingsDefParseErrors = new HashSet<string> ();
			var checkedPaths = new List<string> ();

			for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount (); i++) {
				var prefab = PrefabCollection<BuildingInfo>.GetLoaded (i);

				if (prefab == null)
					continue;

				// search for SubBuildingsEnabler.xml
				var asset = PackageManager.FindAssetByName (prefab.name);
				if (asset == null || asset.package == null)
					continue;

				var crpPath = asset.package.packagePath;
				if (crpPath == null)
					continue;

				var subBuildingsDefPath = Path.Combine (Path.GetDirectoryName (crpPath), "PloppableRICODefinition.xml");

				// skip files which were already parsed
				if (checkedPaths.Contains (subBuildingsDefPath))
					continue;
				checkedPaths.Add (subBuildingsDefPath);

				if (!File.Exists (subBuildingsDefPath))
					continue;

				SubBuildingsDefinition subBuildingsDef = null;

				var xmlSerializer = new XmlSerializer (typeof(SubBuildingsDefinition));
				try {
					using (StreamReader streamReader = new System.IO.StreamReader (subBuildingsDefPath)) {
						subBuildingsDef = xmlSerializer.Deserialize (streamReader) as SubBuildingsDefinition;
					}
				} catch (Exception e) {
					Debug.LogException (e);
					subBuildingsDefParseErrors.Add (asset.package.packageName + " - " + e.Message);
					continue;
				}

				if (subBuildingsDef == null || subBuildingsDef.Buildings == null || subBuildingsDef.Buildings.Count == 0) {
					subBuildingsDefParseErrors.Add (asset.package.packageName + " - subBuildingsDef is null or empty.");
					continue;
				}

				foreach (var parentBuildingDef in subBuildingsDef.Buildings) {
					if (parentBuildingDef == null || parentBuildingDef.Name == null) {
						subBuildingsDefParseErrors.Add (asset.package.packageName + " - Building name missing.");
						continue;
					}

					var parentBuildingPrefab = FindPrefab (parentBuildingDef.Name, asset.package.packageName);

					if (parentBuildingPrefab == null) {
						subBuildingsDefParseErrors.Add (asset.package.packageName + " - Building with name " + parentBuildingDef.Name + " not loaded.");
						continue;
					}

					var subBuildings = new List<BuildingInfo.SubInfo> ();


					string[] NamesType = new string[2]; //This list gets passed off to the RICO panel, and is used to draw the buttons. 
					NamesType [0] = parentBuildingPrefab.name;
					NamesType [1] = parentBuildingDef.uitab; 
					BuildingNames.Add (NamesType);


					if (parentBuildingDef.SubBuildings != null || parentBuildingDef.SubBuildings.Count > 0) {
						foreach (var subBuildingDef in parentBuildingDef.SubBuildings) {
							if (subBuildingDef == null || subBuildingDef.Name == null) {
								subBuildingsDefParseErrors.Add (parentBuildingDef.Name + " - Sub-building name missing.");
								continue;
							}

							var subBuildingPrefab = FindPrefab (subBuildingDef.Name, asset.package.packageName);

							if (subBuildingPrefab == null) {
								subBuildingsDefParseErrors.Add (parentBuildingDef.Name + " - Sub-building with name " + subBuildingDef.Name + " not loaded.");
								continue;
							}
								
							//<<<<<<<<<<<<<<INIT RICO SETTINGS FOR SUB BUILDING

							InitRICO.InitRICO (subBuildingDef.type, subBuildingDef.subtype, subBuildingPrefab, subBuildingDef.levelmax, subBuildingDef.levelmin, 
								subBuildingDef.multi, subBuildingDef.cost);

					
							//<<<<<<<<<<<<<<INIT RICO SETTINGS


							var subBuilding = new BuildingInfo.SubInfo {
								m_buildingInfo = subBuildingPrefab,
								m_position = new Vector3 (subBuildingDef.PosX, subBuildingDef.PosY, subBuildingDef.PosZ),
								m_angle = subBuildingDef.Angle,
								m_fixedHeight = subBuildingDef.FixedHeight,
							};

							subBuildings.Add (subBuilding);

							// this is usually done in the InitializePrefab method
							if (subBuildingDef.FixedHeight && !parentBuildingPrefab.m_fixedHeight)
								parentBuildingPrefab.m_fixedHeight = true;
						}

					} /// end of sub-building loop

					if (subBuildings.Count > 0) {
						
						parentBuildingPrefab.m_subBuildings = subBuildings.ToArray (); // Add sub-building prefabs to main prefab. 
					}

					//<<<<<<<<<<<<<<INIT RICO SETTINGS FOR MAIN BUILDING

					InitRICO.InitRICO (parentBuildingDef.type, parentBuildingDef.subtype, parentBuildingPrefab, parentBuildingDef.levelmax, 
						parentBuildingDef.levelmin, parentBuildingDef.multi, parentBuildingDef.cost);

					//<<<<<<<<<<<<<<INIT RICO SETTINGS
				}
			}

			if (subBuildingsDefParseErrors.Count > 0) {
				var errorMessage = "Error while parsing sub-building definition file(s). Contact the author of the assets. \n"
				                   + "List of errors:\n";
				foreach (var error in subBuildingsDefParseErrors)
					errorMessage += error + '\n';

				UIView.library.ShowModal<ExceptionPanel> ("ExceptionPanel").SetMessage ("Sub-Buildings Enabler", errorMessage, true);
			}

			return BuildingNames;
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
	}

	public class SubBuildingsDefinition
	{
		public List<Building> Buildings { get; set; }

		public SubBuildingsDefinition ()
		{
			Buildings = new List<Building> ();
		}

		public class Building
		{
			[XmlAttribute ("name"), DefaultValue (null)]
			public string Name { get; set; }

			[XmlAttribute ("type"), DefaultValue ("dummy")]
			public string type { get; set; }

			[XmlAttribute ("subtype"), DefaultValue ("dummy")]
			public string subtype { get; set; }

			[XmlAttribute ("multi"), DefaultValue (0)]
			public int multi { get; set; }

			[XmlAttribute ("cost"), DefaultValue (1)]
			public int cost { get; set; }

			[XmlAttribute ("levelmin"), DefaultValue (1)]
			public int levelmin { get; set; }

			[XmlAttribute ("levelmax"), DefaultValue (1)]
			public int levelmax { get; set; }

			[XmlAttribute ("uitab"), DefaultValue ("reshigh")]
			public string uitab { get; set; }

			public List<SubBuilding> SubBuildings { get; set; }

			public Building ()
			{
				SubBuildings = new List<SubBuilding> ();
			}
		}

		public class SubBuilding
		{
			[XmlAttribute ("name"), DefaultValue (null)]
			public string Name { get; set; }

			[XmlAttribute ("pos-x"), DefaultValue (0f)]
			public float PosX { get; set; }

			[XmlAttribute ("pos-y"), DefaultValue (0f)]
			public float PosY { get; set; }

			[XmlAttribute ("pos-z"), DefaultValue (0f)]
			public float PosZ { get; set; }

			[XmlAttribute ("angle"), DefaultValue (0f)]
			public float Angle { get; set; }

			[XmlAttribute ("fixed-height"), DefaultValue (true)]
			public bool FixedHeight { get; set; }

			[XmlAttribute ("type"), DefaultValue ("dummy")]
			public string type { get; set; }

			[XmlAttribute ("cost"), DefaultValue (1)]
			public int cost { get; set; }

			[XmlAttribute ("subtype"), DefaultValue ("dummy")]
			public string subtype { get; set; }

			[XmlAttribute ("multi"), DefaultValue (0)]
			public int multi { get; set; }

			[XmlAttribute ("levelmin"), DefaultValue (1)]
			public int levelmin { get; set; }

			[XmlAttribute ("levelmax"), DefaultValue (1)]
			public int levelmax { get; set; }


			public SubBuilding ()
			{
				FixedHeight = true;
			}
		}
	}
	
}



