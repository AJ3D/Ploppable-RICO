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
using System.Reflection;

namespace PloppableRICO
{
	public class ExtendedLoading : LoadingExtensionBase
	{
		public PloppableTool PloppableTool;

		System.Collections.Generic.List<string[]> BNames = new System.Collections.Generic.List<string[]>{}; //This passes a list of prefab names from subbuildings enabler to ploppable tool. 

		static GameObject buildingWindowGameObject;
		BuildingInfoWindow5 buildingWindow;
		ServiceInfoWindow serviceWindow;

		LoadRICO LoadXML = new LoadRICO ();

		private LoadMode _mode;



		public override void OnLevelLoaded (LoadMode mode)
		{
			base.OnLevelLoaded (mode);

			if (mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
				return;

			/////////////////////////////// Load XML settings and apply them to prefabs. Based on the Sub-Buildings Enabler mod. 

			LoadXML.Run (BNames);

			/////////////////////////////Load info panel tabs. This based on the Extended Building Info mod. 
	
			if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
				return;
			_mode = mode;

			buildingWindowGameObject = new GameObject ("buildingWindowObject");

			//Zoned Panel
			var buildingInfo = UIView.Find<UIPanel> ("(Library) ZonedBuildingWorldInfoPanel");
			this.buildingWindow = buildingWindowGameObject.AddComponent<BuildingInfoWindow5> ();
			this.buildingWindow.transform.parent = buildingInfo.transform;
			this.buildingWindow.size = new Vector3 (buildingInfo.size.x, buildingInfo.size.y);
			this.buildingWindow.baseBuildingWindow = buildingInfo.gameObject.transform.GetComponentInChildren<ZonedBuildingWorldInfoPanel> ();
			this.buildingWindow.position = new Vector3 (0, 12);
			buildingInfo.eventVisibilityChanged += buildingInfo_eventVisibilityChanged;

			//Service panel
			var serviceBuildingInfo = UIView.Find<UIPanel> ("(Library) CityServiceWorldInfoPanel");
			serviceWindow = buildingWindowGameObject.AddComponent<ServiceInfoWindow> (); 
			serviceWindow.servicePanel = serviceBuildingInfo.gameObject.transform.GetComponentInChildren<CityServiceWorldInfoPanel> ();
	
			serviceBuildingInfo.eventVisibilityChanged += serviceBuildingInfo_eventVisibilityChanged;


			///////////////Load the RICO panel. This is based on the Terraform Tool mod. 
		
			try {
				if (PloppableTool == null) {
					GameObject gameController = GameObject.FindWithTag ("GameController");
					PloppableTool = gameController.AddComponent<PloppableTool> ();
					PloppableTool.name = "PloppableTool";
					PloppableTool.InitGui (BNames);
					PloppableTool.enabled = false;
					GameObject.FindObjectOfType<ToolController> ().Tools [0].enabled = true;
				}
					
			} catch (Exception e) {
				Debug.Log (e.ToString ());
			}


			/////////////////Deploy BuildingTool Detour. This is based on the Larger Footprints mod. 
			/// 
			Detour.BuildingToolDetour.Deploy ();


		}
			
		public override void OnReleased()
		{
			Detour.BuildingToolDetour.Revert();
		}
	
		private void serviceBuildingInfo_eventVisibilityChanged (UIComponent component, bool value)  //Service panel
		{
			serviceWindow.Update ();
		}

		void buildingInfo_eventVisibilityChanged (UIComponent component, bool value) //Zoned panel
		{
			this.buildingWindow.isEnabled = value;
			if (value) {
				this.buildingWindow.Show ();
			} else {
				this.buildingWindow.Hide ();
			}
		}

		public override void OnLevelUnloading ()
		{
			if (_mode != LoadMode.LoadGame && _mode != LoadMode.NewGame)
				return;

			GameObject.Destroy (buildingWindow);
			GameObject.Destroy (serviceWindow);

			if (buildingWindowGameObject != null) {

				GameObject.Destroy (buildingWindowGameObject);

			}
		
				
		//RICO ploppables need a non private item class assigned to pass though the game reload. 
			for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount (); i++) {
				
				var prefab = PrefabCollection<BuildingInfo>.GetLoaded (i);



				if (prefab.m_buildingAI is PloppableRICO.PloppableExtractor || prefab.m_buildingAI  is PloppableResidential || prefab.m_buildingAI  is PloppableOffice || prefab.m_buildingAI is PloppableCommercial || prefab.m_buildingAI  is PloppableIndustrial) {


					prefab.m_class = ItemClassCollection.FindClass ("Beautification Item"); //Just assign any RICO prefab a ploppable Itemclass so it will load. It gets set back once the mod loads. 

					prefab.InitializePrefab ();
				}
			}
		}
	}
}
