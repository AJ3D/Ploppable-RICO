using ColossalFramework;
using ColossalFramework.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
/*
namespace PloppableAI
{
	public struct BuildingToolDetoursHolder
	{
		private static bool deployed = false;

		private static RedirectCallsState _BuildingTool_CreateBuilding_state;
		private static MethodInfo _BuildingTool_CreateBuilding_original;
		private static MethodInfo _BuildingTool_CreateBuilding_detour;

		public static void Deploy()
		{
			if (!deployed)
			{
				_BuildingTool_CreateBuilding_original = typeof(BuildingTool).GetMethod("CreateBuilding", BindingFlags.Instance | BindingFlags.NonPublic);
				_BuildingTool_CreateBuilding_detour = typeof(BuildingToolDetoursHolder).GetMethod("CreateBuilding", BindingFlags.Instance | BindingFlags.NonPublic);
				_BuildingTool_CreateBuilding_state = RedirectionHelper.RedirectCalls(_BuildingTool_CreateBuilding_original, _BuildingTool_CreateBuilding_detour);

				deployed = true;

				Debug.Log("LargerFootprints: BuildingTool Methods detoured!");
			}
		}

		public static void Revert()
		{
			if (deployed)
			{
				RedirectionHelper.RevertRedirect(_BuildingTool_CreateBuilding_original, _BuildingTool_CreateBuilding_state);
				_BuildingTool_CreateBuilding_original = null;
				_BuildingTool_CreateBuilding_detour = null;

				deployed = false;

				Debug.Log("LargerFootprints: BuildingTool Methods restored!");
			}
		}

		private ushort CreateBuilding(BuildingInfo info, Vector3 position, float angle, int relocating)
		{
			Debug.Log("CreateBuilding called.");

			BuildingTool _this = ToolsModifierControl.GetTool<BuildingTool>();

			ushort result = 0;
			bool flag = (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) != ItemClass.Availability.None;
			ulong[] collidingSegmentBuffer;
			ulong[] collidingBuildingBuffer;

			ToolController _m_toolController = (ToolController)typeof(BuildingTool).GetField("m_toolConroller", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_this);

			_m_toolController.BeginColliding(out collidingSegmentBuffer, out collidingBuildingBuffer);

			try
			{
				float minY;
				float num;
				float num2;
				Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out minY, out num, out num2);

				ToolBase.ToolErrors errors = (ToolBase.ToolErrors)typeof(BuildingTool).GetMethod("CheckSpace", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_this, new object[] { info, relocating, position, minY, num2 + info.m_size.y, angle, info.m_cellWidth, info.m_cellLength, false, collidingSegmentBuffer, collidingBuildingBuffer });

				if (errors == ToolBase.ToolErrors.None)
				{
					if (flag)
					{
						int constructionCost = (int)typeof(BuildingTool).GetField("m_constructionCost", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_this);
						Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, constructionCost, info.m_class);
					}
					bool flag2 = false;
					if (relocating != 0)
					{
						Singleton<BuildingManager>.instance.RelocateBuilding((ushort)relocating, position, angle);
						InstanceID id = default(InstanceID);
						id.Building = (ushort)relocating;

						ThreadHelper.dispatcher.Dispatch(delegate
							{
								//_this.m_relocateCompleted(id);

							});

						result = (ushort)relocating;
						relocating = 0;
						typeof(BuildingTool).GetField("m_relocate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_this, 0);
						flag2 = true;
					}
					else if (info.m_buildingAI.WorksAsNet())
					{
						float elevation = (float)typeof(BuildingTool).GetMethod("GetElevation", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_this, new object[] { info });
						Building building = default(Building);
						building.m_buildIndex = Singleton<SimulationManager>.instance.m_currentBuildIndex;
						building.m_position = position;
						building.m_angle = angle;
						building.m_width = (byte)info.m_cellWidth;
						building.m_length = (byte)info.m_cellLength;
						BuildingDecoration.LoadPaths(info, 0, ref building, elevation);
						if (Mathf.Abs(elevation) < 1f)
						{
							BuildingDecoration.LoadProps(info, 0, ref building);
						}
						Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
						flag2 = true;
					}
					else if (Singleton<BuildingManager>.instance.CreateBuilding(out result, ref Singleton<SimulationManager>.instance.m_randomizer, info, position, angle, 0, Singleton<SimulationManager>.instance.m_currentBuildIndex))
					{
						Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
						flag2 = true;
					}
					if (flag2)
					{
						if (info.m_notUsedGuide != null)
						{
							info.m_notUsedGuide.Disable();
						}
						info.m_buildingAI.PlacementSucceeded();
						Singleton<GuideManager>.instance.m_notEnoughMoney.Deactivate();
						if (info.m_class.m_service > ItemClass.Service.Office)
						{
							int num3 = info.m_class.m_service - ItemClass.Service.Office - 1;
							Singleton<GuideManager>.instance.m_serviceNotUsed[num3].Disable();
							Singleton<GuideManager>.instance.m_serviceNeeded[num3].Deactivate();
							Singleton<CoverageManager>.instance.CoverageUpdated(info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
						}
						BuildingTool.DispatchPlacementEffect(info, position, angle, info.m_cellLength, false);
					}
				}
				else
				{
					info.m_buildingAI.PlacementFailed();
				}
			}
			finally
			{
				//this.m_toolController.EndColliding();
				_m_toolController.EndColliding();
			}
			return result;
		}
	}
}
*/