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
	public class Sub_BuildingsEnabler
	{
		public PloppableResidential thread;
		
		public void Run ()
		{
			var subBuildingsDefParseErrors = new HashSet<string>();
			var checkedPaths = new List<string>();

			for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
			{
				var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);

				if (prefab == null) continue;

				// search for SubBuildingsEnabler.xml
				var asset = PackageManager.FindAssetByName(prefab.name);
				if (asset == null || asset.package == null) continue;

				var crpPath = asset.package.packagePath;
				if (crpPath == null) continue;

				var subBuildingsDefPath = Path.Combine(Path.GetDirectoryName(crpPath), "PloppableRICODefinition.xml");

				// skip files which were already parsed
				if (checkedPaths.Contains(subBuildingsDefPath)) continue;
				checkedPaths.Add(subBuildingsDefPath);

				if (!File.Exists(subBuildingsDefPath)) continue;

				SubBuildingsDefinition subBuildingsDef = null;

				var xmlSerializer = new XmlSerializer(typeof(SubBuildingsDefinition));
				try
				{
					using (StreamReader streamReader = new System.IO.StreamReader(subBuildingsDefPath))
					{
						subBuildingsDef = xmlSerializer.Deserialize(streamReader) as SubBuildingsDefinition;
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					subBuildingsDefParseErrors.Add(asset.package.packageName + " - " + e.Message);
					continue;
				}

				if (subBuildingsDef == null || subBuildingsDef.Buildings == null || subBuildingsDef.Buildings.Count == 0)
				{
					subBuildingsDefParseErrors.Add(asset.package.packageName + " - subBuildingsDef is null or empty.");
					continue;
				}

				foreach (var parentBuildingDef in subBuildingsDef.Buildings)
				{
					if (parentBuildingDef == null || parentBuildingDef.Name == null)
					{
						subBuildingsDefParseErrors.Add(asset.package.packageName + " - Building name missing.");
						continue;
					}

					var parentBuildingPrefab = FindPrefab(parentBuildingDef.Name, asset.package.packageName);

					if (parentBuildingPrefab == null)
					{
						subBuildingsDefParseErrors.Add(asset.package.packageName + " - Building with name " + parentBuildingDef.Name + " not loaded.");
						continue;
					}

					if (parentBuildingDef.SubBuildings == null || parentBuildingDef.SubBuildings.Count == 0)
					{
						subBuildingsDefParseErrors.Add(asset.package.packageName + " - No sub buildings specified for " + parentBuildingDef.Name + ".");
						continue;
					}

					var subBuildings = new List<BuildingInfo.SubInfo>();

					foreach (var subBuildingDef in parentBuildingDef.SubBuildings)
					{
						if (subBuildingDef == null || subBuildingDef.Name == null)
						{
							subBuildingsDefParseErrors.Add(parentBuildingDef.Name + " - Sub-building name missing.");
							continue;
						}

						var subBuildingPrefab = FindPrefab(subBuildingDef.Name, asset.package.packageName);

						//////////////////////////////////ADDED



						if (subBuildingDef.type == "Residential"){
							subBuildingPrefab.gameObject.AddComponent<PloppableResidential> ();
							//GameObject.Destroy(subBuildingPrefab.gameObject.GetComponent<DummyBuildingAI>());
							subBuildingPrefab.m_buildingAI = subBuildingPrefab.GetComponent<PloppableResidential> ();
							PloppableResidential prefabai = subBuildingPrefab.m_buildingAI as PloppableResidential;
							prefabai.m_housemulti = subBuildingDef.multi;
							prefabai.m_constructionTime = 0;
							subBuildingPrefab.m_buildingAI.m_info = subBuildingPrefab;
							subBuildingPrefab.m_buildingAI.InitializePrefab ();
							subBuildingPrefab.InitializePrefab ();
							Debug.Log ("Residential Worked");
						}

						if (subBuildingDef.type == "Office"){

	
							subBuildingPrefab.gameObject.AddComponent<PloppableOffice> ();
							//GameObject.Destroy(subBuildingPrefab.gameObject.GetComponent<DummyBuildingAI>());
							subBuildingPrefab.m_buildingAI = subBuildingPrefab.GetComponent<PloppableOffice> ();
							PloppableOffice prefabai = subBuildingPrefab.m_buildingAI as PloppableOffice;
							prefabai.m_housemulti = subBuildingDef.multi;
							prefabai.m_constructionTime = 0;
							subBuildingPrefab.m_buildingAI.m_info = subBuildingPrefab;
							subBuildingPrefab.m_buildingAI.InitializePrefab ();
							subBuildingPrefab.InitializePrefab ();
							Debug.Log ("Office Worked");
						}
							
						/////////////////////////////ADDED

						if (subBuildingPrefab == null)
						{
							subBuildingsDefParseErrors.Add(parentBuildingDef.Name + " - Sub-building with name " + subBuildingDef.Name + " not loaded.");
							continue;
						}

						var subBuilding = new BuildingInfo.SubInfo
						{
							m_buildingInfo = subBuildingPrefab,
							m_position = new Vector3(subBuildingDef.PosX, subBuildingDef.PosY, subBuildingDef.PosZ),
							m_angle = subBuildingDef.Angle,
							m_fixedHeight = subBuildingDef.FixedHeight,
						};

						subBuildings.Add(subBuilding);

						// this is usually done in the InitializePrefab method
						if (subBuildingDef.FixedHeight && !parentBuildingPrefab.m_fixedHeight) parentBuildingPrefab.m_fixedHeight = true;
					}

					if (subBuildings.Count == 0)
					{
						subBuildingsDefParseErrors.Add("No sub buildings specified for " + parentBuildingDef.Name + ".");
						continue;
					}

					parentBuildingPrefab.m_subBuildings = subBuildings.ToArray();
				}
			}

			if (subBuildingsDefParseErrors.Count > 0)
			{
				var errorMessage = "Error while parsing sub-building definition file(s). Contact the author of the assets. \n"
					+ "List of errors:\n";
				foreach (var error in subBuildingsDefParseErrors) errorMessage += error + '\n';

				UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Sub-Buildings Enabler", errorMessage, true);
			}
		}

		public void SetThings(BuildingInfo original, BuildingInfo newone){

			//This changes some settings in the newly instanceated BuildingInfos. 

			newone.m_buildingAI = original.m_buildingAI;
			newone.m_AssetEditorTemplate = false;
			newone.m_prefabInitialized = false;
			newone.m_instanceChanged = true;
			newone.m_autoRemove = false;
		}

		public void AssignAI(){

			Debug.Log ("Assign");

			for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount (); i++) {

				BuildingInfo prefabi = PrefabCollection<BuildingInfo>.GetLoaded (i);

				if (prefabi.m_buildingAI is DummyBuildingAI) {

					prefabi.gameObject.AddComponent<PloppableResidential> ();

					GameObject.Destroy(prefabi.gameObject.GetComponent<DummyBuildingAI>());

					prefabi.m_buildingAI = prefabi.GetComponent<PloppableResidential> ();
					prefabi.m_buildingAI.m_info = prefabi;

					PloppableResidential prefabai = prefabi.m_buildingAI as PloppableResidential;
					prefabai.m_housemulti = 4;
					prefabai.m_constructionTime = 0;

				
					prefabi.m_buildingAI.InitializePrefab ();
					prefabi.InitializePrefab ();

					Debug.Log (prefabi.m_buildingAI.ToString() + prefabi.name.ToString());

				}
			}
		}
	

		private BuildingInfo FindPrefab(string prefabName, string packageName)
		{
			var prefab = PrefabCollection<BuildingInfo>.FindLoaded(prefabName);
			if (prefab == null) prefab = PrefabCollection<BuildingInfo>.FindLoaded(prefabName + "_Data");
			if (prefab == null) prefab = PrefabCollection<BuildingInfo>.FindLoaded(PathEscaper.Escape(prefabName) + "_Data");
			if (prefab == null) prefab = PrefabCollection<BuildingInfo>.FindLoaded(packageName + "." + prefabName + "_Data");
			if (prefab == null) prefab = PrefabCollection<BuildingInfo>.FindLoaded(packageName + "." + PathEscaper.Escape(prefabName) + "_Data");

			return prefab;
		}
	}

	public class SubBuildingsDefinition
	{
		public List<Building> Buildings { get; set; }

		public SubBuildingsDefinition()
		{
			Buildings = new List<Building>();
		}

		public class Building
		{
			[XmlAttribute("name"), DefaultValue(null)]
			public string Name { get; set; }

			public List<SubBuilding> SubBuildings { get; set; }

			public Building()
			{
				SubBuildings = new List<SubBuilding>();
			}
		}

		public class SubBuilding
		{
			[XmlAttribute("name"), DefaultValue(null)]
			public string Name { get; set; }

			[XmlAttribute("pos-x"), DefaultValue(0f)]
			public float PosX { get; set; }

			[XmlAttribute("pos-y"), DefaultValue(0f)]
			public float PosY { get; set; }

			[XmlAttribute("pos-z"), DefaultValue(0f)]
			public float PosZ { get; set; }

			[XmlAttribute("angle"), DefaultValue(0f)]
			public float Angle { get; set; }

			[XmlAttribute("fixed-height"), DefaultValue(true)]
			public bool FixedHeight { get; set; }

			[XmlAttribute("type"), DefaultValue("dummy")]
			public string type { get; set; }

			[XmlAttribute("multi"), DefaultValue(0)]
			public int multi { get; set; }

			public SubBuilding()
			{
				FixedHeight = true;
			}
		}
	}
	
}



