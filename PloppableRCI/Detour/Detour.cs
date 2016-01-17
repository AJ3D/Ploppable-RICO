using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;

namespace PloppableRICO.Detour
{

	/// <summary>
	///This detours the CheckCollidingBuildngs method in BuildingTool. Its based on boformers Larger Footprints mod. Many thanks to him for his work. 
	/// </summary>


	public class BuildingToolDetour : ItemClass
	{
		private static bool deployed = false;
	
		private static RedirectCallsState _BuildingTool_CheckCollidingBuildings_state;
		private static MethodInfo _BuildingTool_CheckCollidingBuildings_original;
		private static MethodInfo _BuildingTool_CheckCollidingBuildings_detour;

		public static void Deploy ()
		{
			if (!deployed) {
				_BuildingTool_CheckCollidingBuildings_original = typeof(BuildingTool).GetMethod ("CheckCollidingBuildings", BindingFlags.Static | BindingFlags.Public);
				_BuildingTool_CheckCollidingBuildings_detour = typeof(BuildingToolDetour).GetMethod ("CheckCollidingBuildings", BindingFlags.Static | BindingFlags.Public);
				_BuildingTool_CheckCollidingBuildings_state = RedirectionHelper.RedirectCalls (_BuildingTool_CheckCollidingBuildings_original, _BuildingTool_CheckCollidingBuildings_detour);
		
				deployed = true;

				//Debug.Log("BuildingTool Methods detoured");
			}
		}

		public static void Revert ()
		{
			if (deployed) {
				RedirectionHelper.RevertRedirect (_BuildingTool_CheckCollidingBuildings_original, _BuildingTool_CheckCollidingBuildings_state);
				_BuildingTool_CheckCollidingBuildings_original = null;
				_BuildingTool_CheckCollidingBuildings_detour = null;

				deployed = false;

				//Debug.Log("BuildingTool Methods restored");
			}
		}

		private static bool CheckParentNode (ushort building, ulong[] buildingMask, ulong[] segmentMask)
		{
			ushort num = Singleton<BuildingManager>.instance.m_buildings.m_buffer [(int)building].FindParentNode (building);
			NetManager instance = Singleton<NetManager>.instance;
			if (num == 0) {
				return true;
			}
			NetInfo info = instance.m_nodes.m_buffer [(int)num].Info;
			int publicServiceIndex = ItemClass.GetPublicServiceIndex (info.m_class.m_service);
			if ((publicServiceIndex != -1 && !info.m_autoRemove) || (instance.m_nodes.m_buffer [(int)num].m_flags & NetNode.Flags.Untouchable) != NetNode.Flags.None) {
				return true;
			}
			bool flag = false;
			for (int i = 0; i < 8; i++) {
				ushort segment = instance.m_nodes.m_buffer [(int)num].GetSegment (i);
				if (segment != 0 && (segmentMask [segment >> 6] & 1uL << (int)segment) == 0uL) {
					info = instance.m_segments.m_buffer [(int)segment].Info;
					publicServiceIndex = ItemClass.GetPublicServiceIndex (info.m_class.m_service);
					if ((publicServiceIndex != -1 && !info.m_autoRemove) || (instance.m_segments.m_buffer [(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None) {
						flag = true;
					} else {
						segmentMask [segment >> 6] |= 1uL << (int)segment;
					}
				}
			}
			if (!flag) {
				buildingMask [building >> 6] &= ~(1uL << (int)building);
			}
			return flag;
		}

		private static bool IsImportantBuilding (BuildingInfo info, ref Building building)
		{
			int publicServiceIndex = ItemClass.GetPublicServiceIndex (info.m_class.m_service);


			///////////////////////This exempts RICO buildings from the wrath of the BuildingTool. 

			if (info.m_buildingAI is PloppableOffice || info.m_buildingAI is PloppableExtractor || info.m_buildingAI is PloppableResidential || info.m_buildingAI is PloppableCommercial || info.m_buildingAI is PloppableIndustrial) {
				return true;
				//////////////////////////////////////
			} else {
				
				return (publicServiceIndex != -1 && !info.m_autoRemove) || (building.m_flags & Building.Flags.Untouchable) != Building.Flags.None || building.m_fireIntensity != 0;
			}
		}

		private static bool IsImportantBuilding (BuildingInfo info, ushort id)
		{
			return IsImportantBuilding (info, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer [(int)id]);
		}


		public static bool CheckCollidingBuildings (ulong[] buildingMask, ulong[] segmentMask)
		{
			BuildingManager instance = Singleton<BuildingManager>.instance;
			int num = buildingMask.Length;
			bool result = false;
			for (int i = 0; i < num; i++) {
				ulong num2 = buildingMask [i];
				if (num2 != 0uL) {
					for (int j = 0; j < 64; j++) {
						if ((num2 & 1uL << j) != 0uL) {
							int num3 = i << 6 | j;
							BuildingInfo info = instance.m_buildings.m_buffer [num3].Info;
							if ((instance.m_buildings.m_buffer [num3].m_flags & Building.Flags.Untouchable) != Building.Flags.None) {
								if (CheckParentNode ((ushort)num3, buildingMask, segmentMask)) {
									result = true;
								}
							} else if (IsImportantBuilding (info, (ushort)num3)) {


								result = true;

							}
						}
					}
				}
			}
			//Debug.Log ("Detour Worked");
			return result;
		}

	}
}
