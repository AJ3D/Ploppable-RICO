using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;
using ColossalFramework.UI;

namespace PloppableRICO
{

	public class save : SerializableDataExtensionBase
	{

		//List<BuildingInfo> Prefabs;
		//string collect;
		//string[] replace = new string[]{};
		public override void OnSaveData ()
		{

			//This loops though every building in the scene, and finds ones with the custom AI.
			//It also sorts by service. The original BuildingInfos are set to Monument service in thier ItemClass. We want to leave the originals alone.
			//It then reassigns the original BuildingInfo object. 
			//This is so when the scene is reloaded, the Building object will have a vaild BuildingInfo, since the instances would not have been created yet.
			//Since the list of BuildingInfos is generated fresh from the asset list at scene load, 
			// if you leave the Building objects InfoIndex pointed at one of the instances, it will throw a null referance error on scene load and delete the building object. 


			int count = (int)BuildingManager.instance.m_buildings.m_size;
			for (int i = 1; i < count; i++) {

				// if the building has one of the new AI's
				if (Singleton<BuildingManager>.instance.m_buildings.m_buffer [i].Info.m_buildingAI is PloppableResidential || Singleton<BuildingManager>.instance.m_buildings.m_buffer [i].Info.m_buildingAI is PloppableOffice)
				{

					//and if on those buildings, if the service is office or residential
					if (ItemClass.Service.Residential == Singleton<BuildingManager>.instance.m_buildings.m_buffer [i].Info.m_class.m_service || ItemClass.Service.Office == Singleton<BuildingManager>.instance.m_buildings.m_buffer [i].Info.m_class.m_service) {

						//then reassign the BuildingInfo back to the orginial
						string name = BuildingManager.instance.m_buildings.m_buffer [i].Info.name;

						name = name.Remove (name.Length - 7); //Since I added 7 characters to the name of each instance, I just need to subtract 7 characters to get the original name. 

						if (PrefabCollection<BuildingInfo>.FindLoaded (name) != null) {
							BuildingManager.instance.m_buildings.m_buffer [i].Info = PrefabCollection<BuildingInfo>.FindLoaded (name);
							BuildingManager.instance.m_buildings.m_buffer [i].m_infoIndex = (ushort)PrefabCollection<BuildingInfo>.FindLoaded (name).m_prefabDataIndex;
							Debug.Log (name + "Set back to orginial info");

						}else{
							Debug.Log ( name + "Original Prefab not found");
							BuildingManager.instance.m_buildings.m_buffer [i].m_infoIndex = 0;
						}
					}
				}
			}
			base.OnSaveData ();
		}

	}
}