using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using UnityEngine;

namespace PloppableRICO
{
	/// <summary>
	/// If a building has a cloned prefab assgined, this will reassgin the orginal prefab when the city is saved. 
	/// </summary>

	public class save : SerializableDataExtensionBase
	{

		BuildingData Bdata;
		Building CustomB;
		int count = (int)BuildingManager.instance.m_buildings.m_size;
		BuildingData[] dataArray;

		public override void OnSaveData ()
		{

			try {

				for (int i = 1; i < count; i++) {

					CustomB = Singleton<BuildingManager>.instance.m_buildings.m_buffer [(ushort)i];

					if (CustomB.Info.m_buildingAI is PloppableResidential || CustomB.Info.m_buildingAI is PloppableOffice || CustomB.Info.m_buildingAI is PloppableCommercial || CustomB.Info.m_buildingAI is PloppableIndustrial) {

						dataArray = BuildingDataManager.buildingData;

						Bdata = dataArray [(int)i];

						if (Bdata != null) {
							if (Bdata.level != 1) {

								//Debug.Log ("On Save name is " + Bdata.Name);

								BuildingManager.instance.m_buildings.m_buffer [(ushort)i].Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name);
								BuildingManager.instance.m_buildings.m_buffer [(ushort)i].m_infoIndex = (ushort)PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name).m_prefabDataIndex;
								Bdata.saveflag = false;

							}
						}
					}
				}


			} catch (Exception e) {
				Debug.Log (e.ToString ());
			}

			base.OnSaveData ();
							
		}

	}
		

}
