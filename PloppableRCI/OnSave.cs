using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using System.IO;



namespace PloppableRICO
{

	public class save : SerializableDataExtensionBase
	{
		//public List<string> prefabnames = new List<string>();

		public override void OnSaveData ()
		{

			/*
			for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount (); i++) {
				var prefab = PrefabCollection<BuildingInfo>.GetLoaded (i);

				if (prefab.m_buildingAI is PloppableResidential) {

					prefabnames.Add (prefab.name);
					prefab.m_class = new ItemClass ();
					prefab.m_class.m_service = ItemClass.Service.None;
					prefab.m_class.m_subService = ItemClass.SubService.None;
					prefab.m_placementStyle = ItemClass.Placement.Manual;
					PloppableResidential holder = prefab.m_buildingAI as PloppableResidential;

					holder.timer = 0;

					prefab.gameObject.AddComponent<DummyBuildingAI> ();
					GameObject.Destroy(prefab.gameObject.GetComponent<PloppableResidential>());
					prefab.m_buildingAI = prefab.GetComponent<DummyBuildingAI> ();
					prefab.m_buildingAI.m_info = prefab;
					prefab.m_buildingAI.InitializePrefab ();
					prefab.InitializePrefab ();
				}
			}
*/

			base.OnSaveData ();


			//foreach (string name in prefabnames.ToArray()) {
				
			//}

		}

	}
}
