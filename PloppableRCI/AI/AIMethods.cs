using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;
namespace PloppableRICO
{
	public interface AIMethods{ }


	public static class AIUtils {
		
		public static void SimulationStepActiveP(this AIMethods input, ushort buildingID, ref Building buildingData, ref Building.Frame frameData) { 
			SimulationStepActiveC(input, buildingID, ref buildingData, ref frameData);
				if ((buildingData.m_problems & Notification.Problem.MajorProblem) != Notification.Problem.None)
				{
					if (buildingData.m_fireIntensity == 0)
					{
						buildingData.m_majorProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_majorProblemTimer + 1));
						if (buildingData.m_majorProblemTimer >= 64 && !Singleton<BuildingManager>.instance.m_abandonmentDisabled)
						{
							buildingData.m_majorProblemTimer = 192;
							buildingData.m_flags &= ~Building.Flags.Active;
							buildingData.m_flags |= Building.Flags.Abandoned;
							buildingData.m_problems = (Notification.Problem.FatalProblem | (buildingData.m_problems & ~Notification.Problem.MajorProblem));
							//RemovePeople(buildingID, ref buildingData);
						//	BuildingDeactivated(buildingID, ref buildingData);
							Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
						}
					}
				}
				else
				{
					buildingData.m_majorProblemTimer = 0;
				}
			}

		public static void SimulationStepC (this AIMethods input, ushort buildingID, ref Building buildingData, ref Building.Frame frameData){

			if ((buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None) {
				GuideController properties = Singleton<GuideManager>.instance.m_properties;
				if (properties != null) {
					Singleton<BuildingManager>.instance.m_buildingAbandoned1.Activate (properties.m_buildingAbandoned1, buildingID);
					Singleton<BuildingManager>.instance.m_buildingAbandoned2.Activate (properties.m_buildingAbandoned2, buildingID);
				}
				if (buildingData.m_majorProblemTimer < 255) {
					buildingData.m_majorProblemTimer += 1;
				}
				float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
				Singleton<ImmaterialResourceManager>.instance.AddResource (ImmaterialResourceManager.Resource.Abandonment, 10, buildingData.m_position, radius);
			} else if ((buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None) {
				GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
				if (properties2 != null) {
					Singleton<BuildingManager>.instance.m_buildingBurned.Activate (properties2.m_buildingBurned, buildingID);
				}
				float radius2 = (float)(buildingData.Width + buildingData.Length) * 2.5f;
				Singleton<ImmaterialResourceManager>.instance.AddResource (ImmaterialResourceManager.Resource.Abandonment, 10, buildingData.m_position, radius2);
			} else if ((buildingData.m_flags & Building.Flags.Completed) == Building.Flags.None) {
				bool flag = (buildingData.m_flags & Building.Flags.Upgrading) != Building.Flags.None;
				int constructionTime = 0;
				if (constructionTime == 0) {
					frameData.m_constructState = 255;
				} else {
					frameData.m_constructState = (byte)Mathf.Min (255, (int)frameData.m_constructState + 1088 / constructionTime);
				}
				if (frameData.m_constructState == 255) {
					BuildingCompleted (buildingID, ref buildingData);
					GuideController properties3 = Singleton<GuideManager>.instance.m_properties;
					if (properties3 != null) {
						Singleton<BuildingManager>.instance.m_buildingLevelUp.Deactivate (buildingID, true);
					}
				} else if (flag) {
					GuideController properties4 = Singleton<GuideManager>.instance.m_properties;
					if (properties4 != null) {
						Singleton<BuildingManager>.instance.m_buildingLevelUp.Activate (properties4.m_buildingLevelUp, buildingID);
					}
				}
				if (flag) {
					SimulationStepActive (buildingID, ref buildingData, ref frameData);
				}
			} else {
				SimulationStepActive (buildingID, ref buildingData, ref frameData);
			}
		}

		public static void SimulationStepActiveC(this AIMethods input, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Garbage);
			if (buildingData.m_garbageBuffer >= 2000)
			{
				int num = (int)(buildingData.m_garbageBuffer / 1000);
				if (Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
				{
					Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num, num, buildingData.m_position, 0f);
				}
				if (num >= 3)
				{
					if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Garbage))
					{
						if (num >= 6)
						{
							problem = Notification.AddProblems(problem, Notification.Problem.Garbage | Notification.Problem.MajorProblem);
						}
						else
						{
							problem = Notification.AddProblems(problem, Notification.Problem.Garbage);
						}
						GuideController properties = Singleton<GuideManager>.instance.m_properties;
						if (properties != null)
						{
							int publicServiceIndex = ItemClass.GetPublicServiceIndex(ItemClass.Service.Garbage);
							Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Activate(properties.m_serviceNeeded, ItemClass.Service.Garbage);
						}
					}
					else
					{
						buildingData.m_garbageBuffer = 2000;
					}
				}
			}
			buildingData.m_problems = problem;
			float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
			if (buildingData.m_crimeBuffer != 0)
			{
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.CrimeRate, (int)buildingData.m_crimeBuffer, buildingData.m_position, radius);
			}
			int num2;
			int num3;
			int num4;
			this.GetFireParameters(buildingID, ref buildingData, out num2, out num3, out num4);
			if (num2 != 0)
			{
				DistrictManager instance = Singleton<DistrictManager>.instance;
				byte district = instance.GetDistrict(buildingData.m_position);
				DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
				if ((servicePolicies & DistrictPolicies.Services.SmokeDetectors) != DistrictPolicies.Services.None)
				{
					num2 = num2 * 75 / 100;
				}
			}
			num2 = 100 - (10 + num4) * 50000 / ((100 + num2) * (100 + num3));
			if (num2 > 0)
			{
				num2 = num2 * buildingData.Width * buildingData.Length;
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.FireHazard, num2, buildingData.m_position, radius);
			}
		}

		public static void SimulationStepP(this AIMethods input, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			SimulationStepC(input, buildingID, ref buildingData, ref frameData);
			if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10u) == 0)
			{
				DistrictManager instance = Singleton<DistrictManager>.instance;
				byte district = instance.GetDistrict(buildingData.m_position);
				ushort style = instance.m_districts.m_buffer[(int)district].m_Style;
				if (style > 0 && (int)(style - 1) < instance.m_Styles.Length)
				{
					DistrictStyle districtStyle = instance.m_Styles[(int)(style - 1)];
					if (districtStyle != null && buildingData.Info.m_class != null && districtStyle.AffectsService(buildingData.Info.GetService(), buildingData.Info.GetSubService(), buildingData.Info.m_class.m_level) && !districtStyle.Contains(buildingData.Info) && Singleton<ZoneManager>.instance.m_lastBuildIndex == Singleton<SimulationManager>.instance.m_currentBuildIndex)
					{
						//buildingData.m_flags |= Building.Flags.Demolishing;
						Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
					}
				}
			}
			if ((buildingData.m_flags & Building.Flags.ZonesUpdated) != Building.Flags.None)
			{
				SimulationManager instance2 = Singleton<SimulationManager>.instance;
				if (buildingData.m_fireIntensity == 0 && instance2.m_randomizer.Int32(10u) == 0 && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance2.m_currentBuildIndex)
				{
					buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
					if (!buildingData.CheckZoning(buildingData.Info.m_class.GetZone(), buildingData.Info.m_class.GetSecondaryZone()))
					{
						//buildingData.m_flags |= Building.Flags.Demolishing;
						CheckNearbyBuildingZones(input, buildingData.m_position);
						instance2.m_currentBuildIndex += 1u;
					}
				}
			}
			else if ((buildingData.m_flags & (Building.Flags.Abandoned | Building.Flags.Downgrading)) != Building.Flags.None && (buildingData.m_majorProblemTimer == 255 || (buildingData.m_flags & Building.Flags.Abandoned) == Building.Flags.None))
			{
				SimulationManager instance3 = Singleton<SimulationManager>.instance;
				ZoneManager instance4 = Singleton<ZoneManager>.instance;
				int num;
				switch (buildingData.Info.m_class.m_service)
				{
				case ItemClass.Service.Residential:
					num = instance4.m_actualResidentialDemand;
					goto IL_280;
				case ItemClass.Service.Commercial:
					num = instance4.m_actualCommercialDemand;
					goto IL_280;
				case ItemClass.Service.Industrial:
					num = instance4.m_actualWorkplaceDemand;
					goto IL_280;
				case ItemClass.Service.Office:
					num = instance4.m_actualWorkplaceDemand;
					goto IL_280;
				}
				num = 0;
				IL_280:
				if (instance3.m_randomizer.Int32(100u) < num && instance4.m_lastBuildIndex == instance3.m_currentBuildIndex)
				{
					float num2 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(buildingData.m_position));
					if (num2 <= buildingData.m_position.y)
					{
						ItemClass.SubService subService = buildingData.Info.m_class.m_subService;
						ItemClass.Level level = ItemClass.Level.Level1;
						int width = buildingData.Width;
						int num3 = buildingData.Length;
						if (buildingData.Info.m_class.m_service == ItemClass.Service.Industrial)
						{
							ZoneBlock.GetIndustryType(buildingData.m_position, out subService, out level);
						}
						else if (buildingData.Info.m_class.m_service == ItemClass.Service.Commercial)
						{
							ZoneBlock.GetCommercialType(buildingData.m_position, buildingData.Info.m_class.GetZone(), width, num3, out subService, out level);
						}
						DistrictManager instance5 = Singleton<DistrictManager>.instance;
						byte district2 = instance5.GetDistrict(buildingData.m_position);
						ushort style2 = instance5.m_districts.m_buffer[(int)district2].m_Style;
						BuildingInfo randomBuildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, buildingData.Info.m_class.m_service, subService, level, width, num3, buildingData.Info.m_zoningMode, (int)style2);
						if (randomBuildingInfo != null)
						{
							//buildingData.m_flags |= Building.Flags.Demolishing;
							float num4 = buildingData.m_angle + 1.57079637f;
							if (buildingData.Info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft && randomBuildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerRight)
							{
								num4 -= 1.57079637f;
								num3 = width;
							}
							else if (buildingData.Info.m_zoningMode == BuildingInfo.ZoningMode.CornerRight && randomBuildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
							{
								num4 += 1.57079637f;
								num3 = width;
							}
							ushort num5;
							if (Singleton<BuildingManager>.instance.CreateBuilding(out num5, ref Singleton<SimulationManager>.instance.m_randomizer, randomBuildingInfo, buildingData.m_position, buildingData.m_angle, num3, Singleton<SimulationManager>.instance.m_currentBuildIndex))
							{
								Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
								switch (buildingData.Info.m_class.m_service)
								{
								case ItemClass.Service.Residential:
									instance4.m_actualResidentialDemand = Mathf.Max(0, instance4.m_actualResidentialDemand - 5);
									break;
								case ItemClass.Service.Commercial:
									instance4.m_actualCommercialDemand = Mathf.Max(0, instance4.m_actualCommercialDemand - 5);
									break;
								case ItemClass.Service.Industrial:
									instance4.m_actualWorkplaceDemand = Mathf.Max(0, instance4.m_actualWorkplaceDemand - 5);
									break;
								case ItemClass.Service.Office:
									instance4.m_actualWorkplaceDemand = Mathf.Max(0, instance4.m_actualWorkplaceDemand - 5);
									break;
								}
							}
							instance3.m_currentBuildIndex += 1u;
						}
					}
				}
			}
		}

		private static void CheckNearbyBuildingZones(this AIMethods input, Vector3 position)
		{
			int num = Mathf.Max((int)((position.x - 35f) / 64f + 135f), 0);
			int num2 = Mathf.Max((int)((position.z - 35f) / 64f + 135f), 0);
			int num3 = Mathf.Min((int)((position.x + 35f) / 64f + 135f), 269);
			int num4 = Mathf.Min((int)((position.z + 35f) / 64f + 135f), 269);
			Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
			ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
			for (int i = num2; i <= num4; i++)
			{
				for (int j = num; j <= num3; j++)
				{
					ushort num5 = buildingGrid[i * 270 + j];
					int num6 = 0;
					while (num5 != 0)
					{
						ushort nextGridBuilding = buildings.m_buffer[(int)num5].m_nextGridBuilding;
						Building.Flags flags = buildings.m_buffer[(int)num5].m_flags;
						if ((flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Demolishing)) == Building.Flags.Created)
						{
							BuildingInfo info = buildings.m_buffer[(int)num5].Info;
							if (info != null && info.m_placementStyle == ItemClass.Placement.Automatic)
							{
								ItemClass.Zone zone = info.m_class.GetZone();
								ItemClass.Zone secondaryZone = info.m_class.GetSecondaryZone();
								if (zone != ItemClass.Zone.None && (buildings.m_buffer[(int)num5].m_flags & Building.Flags.ZonesUpdated) != Building.Flags.None && VectorUtils.LengthSqrXZ(buildings.m_buffer[(int)num5].m_position - position) <= 1225f)
								{
									Building[] expr_1A6_cp_0 = buildings.m_buffer;
									ushort expr_1A6_cp_1 = num5;
									expr_1A6_cp_0[(int)expr_1A6_cp_1].m_flags = (expr_1A6_cp_0[(int)expr_1A6_cp_1].m_flags & ~Building.Flags.ZonesUpdated);
									if (!buildings.m_buffer[(int)num5].CheckZoning(zone, secondaryZone))
									{
										Singleton<BuildingManager>.instance.ReleaseBuilding(num5);
									}
								}
							}
						}
						num5 = nextGridBuilding;
						if (++num6 >= 49152)
						{
							CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
							break;
						}
					}
				}
			}
		}
	}




}

