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

namespace PloppableRICO
{
	/// <summary>
	/// This class will initalize the RICO prefabs.
	/// </summary>

	public class InitRICOPrefabs
	{
		public void InitRICO (string type, string subtype, BuildingInfo BuildingPrefab, int levelmax, int levelmin, int multi, int cost)
		{

			if (type == "Residential") {

				//Assign the custom AI threads to prefab
				BuildingPrefab.gameObject.AddComponent<PloppableResidential> ();
				BuildingPrefab.m_buildingAI = BuildingPrefab.GetComponent<PloppableResidential> ();
				PloppableResidential prefabai = BuildingPrefab.m_buildingAI as PloppableResidential;
				//Pull settings from XML and assign them to custom AI
				prefabai.m_levelmax = levelmax;
				prefabai.m_levelmin = levelmin;
				prefabai.m_households = multi;
				prefabai.m_constructionCost = cost;
				prefabai.m_constructionTime = 0;

			}

			if (type == "Office") {

				BuildingPrefab.gameObject.AddComponent<PloppableOffice> ();
				BuildingPrefab.m_buildingAI = BuildingPrefab.GetComponent<PloppableOffice> ();
				PloppableOffice prefabai = BuildingPrefab.m_buildingAI as PloppableOffice;

				prefabai.m_levelmax = levelmax;
				prefabai.m_levelmin = levelmin;
				prefabai.m_housemulti = multi;
				prefabai.m_constructionCost = cost;
				prefabai.m_constructionTime = 0;
			}

			if (type == "Industrial") {
				BuildingPrefab.gameObject.AddComponent<PloppableIndustrial> ();
				BuildingPrefab.m_buildingAI = BuildingPrefab.GetComponent<PloppableIndustrial> ();
				PloppableIndustrial prefabai = BuildingPrefab.m_buildingAI as PloppableIndustrial;

				prefabai.m_levelmax = levelmax;
				prefabai.m_levelmin = levelmin;
				prefabai.m_housemulti = multi;
				prefabai.m_constructionCost = cost;
				prefabai.m_constructionTime = 0;

			}

			if (type == "Extractor") {
				BuildingPrefab.gameObject.AddComponent<PloppableExtractor> ();
				BuildingPrefab.m_buildingAI = BuildingPrefab.GetComponent<PloppableExtractor> ();
				PloppableExtractor prefabai = BuildingPrefab.m_buildingAI as PloppableExtractor;

				prefabai.m_levelmax = levelmax;
				prefabai.m_levelmin = levelmin;
				prefabai.m_housemulti = multi;
				prefabai.m_constructionCost = cost;
				prefabai.m_constructionTime = 0;


			}

			if (type == "Commercial") {
				BuildingPrefab.gameObject.AddComponent<PloppableCommercial> ();
				BuildingPrefab.m_buildingAI = BuildingPrefab.GetComponent<PloppableCommercial> ();
				PloppableCommercial prefabai = BuildingPrefab.m_buildingAI as PloppableCommercial;

				prefabai.m_levelmax = levelmax;
				prefabai.m_levelmin = levelmin;
				prefabai.m_housemulti = multi;
				prefabai.m_constructionCost = cost;
				prefabai.m_constructionTime = 0;

			}

			if (type == "Residential" || type == "Extractor" || type == "Industrial" || type == "Office" || type == "Commercial") {

				BuildingPrefab.m_buildingAI.m_info = BuildingPrefab;

				BuildingPrefab.InitializePrefab ();
				makeInfos (BuildingPrefab, type, subtype);
			}
		}

		public void makeInfos (BuildingInfo Holder, string Type, string subtype)
		{

			// Make new BuildingInfos for leveling
			if (PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_level2") == null) {

				if (Type == "Residential" || Type == "Office" || Type == "Commercial" || (Type == "Industrial" & subtype == "Generic")) { // Make 2 new levels for these types. 

					BuildingInfo Level2 = BuildingInfo.Instantiate (Holder);
					Level2.name = Holder.name + "_Level2";
					this.SetThings (Holder, Level2);

					BuildingInfo Level3 = BuildingInfo.Instantiate (Holder);
					Level3.name = Holder.name + "_Level3";
					this.SetThings (Holder, Level3);

					BuildingInfo[] bray4 = new BuildingInfo[] { Level2, Level3 };
					string[] stra4 = new string[] { Level2.name, Level3.name };

					PrefabCollection<BuildingInfo>.InitializePrefabs ("BuildingInfo", bray4, stra4);
					PrefabCollection<BuildingInfo>.BindPrefabs ();

					if (Type == "Residential") { //If its residential, make 2 more levels. 

						BuildingInfo Level4 = BuildingInfo.Instantiate (Holder);
						Level4.name = Holder.name + "_Level4";
						this.SetThings (Holder, Level4);

						BuildingInfo Level5 = BuildingInfo.Instantiate (Holder);
						Level5.name = Holder.name + "_Level5";
						this.SetThings (Holder, Level5);

						BuildingInfo[] bray = new BuildingInfo[] { Level4, Level5 };
						string[] stra = new string[] { Level4.name, Level5.name };

						PrefabCollection<BuildingInfo>.InitializePrefabs ("BuildingInfo", bray, stra); //initlaize the instances so they can be referenced by the Buliding objects. 
						PrefabCollection<BuildingInfo>.BindPrefabs ();

					}
				}
				PrefabCollection<BuildingInfo>.BindPrefabs ();
						
				Debug.Log ("Building Info Generated");
			}

			//Assign Item classes to RICO prefabs. 
			if (Type == "Residential") {

				if (subtype == "Low") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Low Residential - Level1");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level2").m_class = ItemClassCollection.FindClass ("Low Residential - Level2");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level3").m_class = ItemClassCollection.FindClass ("Low Residential - Level3");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level4").m_class = ItemClassCollection.FindClass ("Low Residential - Level4");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level5").m_class = ItemClassCollection.FindClass ("Low Residential - Level5");

				} else {

					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("High Residential - Level1");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level2").m_class = ItemClassCollection.FindClass ("High Residential - Level2");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level3").m_class = ItemClassCollection.FindClass ("High Residential - Level3");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level4").m_class = ItemClassCollection.FindClass ("High Residential - Level4");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level5").m_class = ItemClassCollection.FindClass ("High Residential - Level5");
				}
			}
			if (Type == "Commercial") {

				if (subtype == "Low") {

					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Low Commercial - Level1");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level2").m_class = ItemClassCollection.FindClass ("Low Commercial - Level2");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level3").m_class = ItemClassCollection.FindClass ("Low Commercial - Level3");

				} else {

					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("High Commercial - Level1");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level2").m_class = ItemClassCollection.FindClass ("High Commercial - Level2");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level3").m_class = ItemClassCollection.FindClass ("High Commercial - Level3");
				}
			}

			if (Type == "Office") {

				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Office - Level1");
				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level2").m_class = ItemClassCollection.FindClass ("Office - Level2");
				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level3").m_class = ItemClassCollection.FindClass ("Office - Level3");
			}
			if (Type == "Industrial") {

				if (subtype == "Farming") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Farming - Processing");

				}
				if (subtype == "Forest") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Forest - Processing");
				} 
				if (subtype == "Oil") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Oil - Processing");
				}
				if (subtype == "Ore") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Ore - Processing");
				} 
				if (subtype == "Generic") {

					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Industrial - Level1");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level2").m_class = ItemClassCollection.FindClass ("Industrial - Level2");
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level3").m_class = ItemClassCollection.FindClass ("Industrial - Level3");
				}
			}
			if (Type == "Extractor") {

				if (subtype == "Farming") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Farming - Extractor");
				}
				if (subtype == "Forest") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Forest - Extractor");
				}
				if (subtype == "Oil") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Oil - Extractor");
				}
				if (subtype == "Ore") {
					PrefabCollection<BuildingInfo>.FindLoaded (Holder.name).m_class = ItemClassCollection.FindClass ("Ore - Extractor");
				}
			}
		}

		public void SetThings (BuildingInfo original, BuildingInfo newone)
		{

			//This changes some settings in the newly instanceated BuildingInfos. 

			newone.m_buildingAI = original.m_buildingAI;
			//newone.m_AssetEditorTemplate = true;
			//newone.m_prefabInitialized = false;
			//newone.m_instanceChanged = true;
			//newone.m_autoRemove = false;
		}
	}
}


