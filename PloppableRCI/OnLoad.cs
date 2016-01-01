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
	public class ExtendedLoading : LoadingExtensionBase
	{
		
		static GameObject buildingWindowGameObject;
		ServiceInfoWindow serviceWindow;
		Sub_BuildingsEnabler sub = new Sub_BuildingsEnabler();
		private LoadMode _mode;
	
		public override void OnLevelLoaded (LoadMode mode)
		{
			base.OnLevelLoaded (mode);

			if (mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
				return;

			sub.Run (); //Sub-Building Enabler
			//sub.AssignAI();
		

			if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
				return;
			_mode = mode;

			buildingWindowGameObject = new GameObject ("buildingWindowObject");

			var serviceBuildingInfo = UIView.Find<UIPanel> ("(Library) CityServiceWorldInfoPanel");
			serviceWindow = buildingWindowGameObject.AddComponent<ServiceInfoWindow> (); 
			serviceWindow.servicePanel = serviceBuildingInfo.gameObject.transform.GetComponentInChildren<CityServiceWorldInfoPanel> ();
	
			serviceBuildingInfo.eventVisibilityChanged += serviceBuildingInfo_eventVisibilityChanged;

		}
	
	private void serviceBuildingInfo_eventVisibilityChanged(UIComponent component, bool value)
	{
		serviceWindow.Update();
	}



		public override void OnLevelUnloading()
		{
			if (_mode != LoadMode.LoadGame && _mode != LoadMode.NewGame)
				return;

			if (buildingWindowGameObject != null)
			{
				GameObject.Destroy(buildingWindowGameObject);
			}
		}
	}}
