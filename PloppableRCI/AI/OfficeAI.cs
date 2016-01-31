using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableRICO
{

	public class PloppableOffice : OfficeBuildingAI

	{
		public int m_workplaceCount = 1;
		public int m_constructionCost = 1;

        public override string GenerateName(ushort buildingID, InstanceID caller)
        {
            return base.m_info.GetUncheckedLocalizedTitle();

        }

        public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			
			minWidth = 1;
			maxWidth = 16;
		}

		public override void GetLengthRange (out int minLength, out int maxLength)
		{
			
			minLength = 1;
			maxLength = 16;
		}

		public override int GetConstructionCost()
		{
			int result = (m_constructionCost * 100);
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return 1;
		}

		public override void CalculateWorkplaceCount (Randomizer r, int width, int length, out int level1,out int level2,out int level3, out int level4)
		{
			int widths = (m_workplaceCount * 2);

			base.CalculateWorkplaceCount (r, widths , 1 ,out level1,out level2,out level3, out level4);
		}

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}

		public override void SimulationStep (ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			buildingData.m_garbageBuffer = 0;
			buildingData.m_fireHazard = 0;
			buildingData.m_fireIntensity = 0;
			buildingData.m_majorProblemTimer = 0;



			SimulationStepP(buildingID, ref buildingData, ref frameData);
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte district = instance.GetDistrict(buildingData.m_position);
			DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
			if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
			{
				District[] expr_5A_cp_0_cp_0 = instance.m_districts.m_buffer;
				byte expr_5A_cp_0_cp_1 = district;
				expr_5A_cp_0_cp_0[(int)expr_5A_cp_0_cp_1].m_officeData.m_tempBuildingCount = (ushort)(expr_5A_cp_0_cp_0[(int)expr_5A_cp_0_cp_1].m_officeData.m_tempBuildingCount + 1);
				District[] expr_7E_cp_0_cp_0 = instance.m_districts.m_buffer;
				byte expr_7E_cp_0_cp_1 = district;
				expr_7E_cp_0_cp_0[(int)expr_7E_cp_0_cp_1].m_officeData.m_tempBuildingArea = expr_7E_cp_0_cp_0[(int)expr_7E_cp_0_cp_1].m_officeData.m_tempBuildingArea + (uint)(buildingData.Width * buildingData.Length);
				if ((buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None)
				{
					District[] expr_BE_cp_0_cp_0 = instance.m_districts.m_buffer;
					byte expr_BE_cp_0_cp_1 = district;
					expr_BE_cp_0_cp_0[(int)expr_BE_cp_0_cp_1].m_officeData.m_tempAbandonedCount = (ushort)(expr_BE_cp_0_cp_0[(int)expr_BE_cp_0_cp_1].m_officeData.m_tempAbandonedCount + 1);
				}
				else if ((buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
				{
					District[] expr_F8_cp_0_cp_0 = instance.m_districts.m_buffer;
					byte expr_F8_cp_0_cp_1 = district;
					expr_F8_cp_0_cp_0[(int)expr_F8_cp_0_cp_1].m_officeData.m_tempBurnedCount = (ushort)(expr_F8_cp_0_cp_0[(int)expr_F8_cp_0_cp_1].m_officeData.m_tempBurnedCount + 1);
				}
			}
			if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.HighriseBan) != DistrictPolicies.CityPlanning.None && this.m_info.m_class.m_level == ItemClass.Level.Level3)
			{
				SimulationManager instance2 = Singleton<SimulationManager>.instance;
				if (instance2.m_randomizer.Int32(10u) == 0 && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance2.m_currentBuildIndex)
				{
					District[] expr_163_cp_0 = instance.m_districts.m_buffer;
					byte expr_163_cp_1 = district;
					expr_163_cp_0[(int)expr_163_cp_1].m_cityPlanningPoliciesEffect = (expr_163_cp_0[(int)expr_163_cp_1].m_cityPlanningPoliciesEffect | DistrictPolicies.CityPlanning.HighriseBan);
					buildingData.m_flags |= Building.Flags.Demolishing;
					instance2.m_currentBuildIndex += 1u;
				}
			}

		}

		public void SimulationStepP(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			SimulationStepC(buildingID, ref buildingData, ref frameData);
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
						CheckNearbyBuildingZones2(buildingData.m_position);
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
						else if (this.m_info.m_class.m_service == ItemClass.Service.Commercial)
						{
							ZoneBlock.GetCommercialType(buildingData.m_position, this.m_info.m_class.GetZone(), width, num3, out subService, out level);
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

		public void SimulationStepC (ushort buildingID, ref Building buildingData, ref Building.Frame frameData){

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
				int constructionTime = GetConstructionTime ();
				if (constructionTime == 0) {
					frameData.m_constructState = 255;
				} else {
					frameData.m_constructState = (byte)Mathf.Min (255, (int)frameData.m_constructState + 1088 / constructionTime);
				}
				if (frameData.m_constructState == 255) {
					this.BuildingCompleted (buildingID, ref buildingData);
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

		protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte district = instance.GetDistrict(buildingData.m_position);
			DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
			DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[(int)district].m_taxationPolicies;
			DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
			District[] expr_6A_cp_0 = instance.m_districts.m_buffer;
			byte expr_6A_cp_1 = district;
			expr_6A_cp_0[(int)expr_6A_cp_1].m_servicePoliciesEffect = (expr_6A_cp_0[(int)expr_6A_cp_1].m_servicePoliciesEffect | (servicePolicies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.Recycling)));
			if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseOffice | DistrictPolicies.Taxation.TaxLowerOffice)) != (DistrictPolicies.Taxation.TaxRaiseOffice | DistrictPolicies.Taxation.TaxLowerOffice))
			{
				District[] expr_9C_cp_0 = instance.m_districts.m_buffer;
				byte expr_9C_cp_1 = district;
				expr_9C_cp_0[(int)expr_9C_cp_1].m_taxationPoliciesEffect = (expr_9C_cp_0[(int)expr_9C_cp_1].m_taxationPoliciesEffect | (taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseOffice | DistrictPolicies.Taxation.TaxLowerOffice)));
			}
			Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = HandleWorkers(buildingID, ref buildingData, ref behaviourData, ref num, ref num2, ref num3);
			int width = buildingData.Width;
			int length = buildingData.Length;
			int num5 = CalculateProductionCapacity(new Randomizer((int)buildingID), width, length);
			num4 = (num4 * num5 + 99) / 100;
			int electricityConsumption;
			int waterConsumption;
			int sewageAccumulation;
			int num6;
			int num7;
			GetConsumptionRates(new Randomizer((int)buildingID), num4, out electricityConsumption, out waterConsumption, out sewageAccumulation, out num6, out num7);
			if (num6 != 0 && (servicePolicies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
			{
				num6 = Mathf.Max(1, num6 * 85 / 100);
				num7 = num7 * 95 / 100;
			}
			if (num4 != 0)
			{
				int num8 = base.HandleCommonConsumption(buildingID, ref buildingData, ref electricityConsumption, ref waterConsumption, ref sewageAccumulation, ref num6, servicePolicies);
				num4 = (num4 * num8 + 99) / 100;
				if (num4 != 0)
				{
					if (num7 != 0)
					{
						Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PrivateIncome, num7, buildingData.Info.m_class, taxationPolicies);
					}
					int num9;
					int num10;
					this.GetPollutionRates(num4, cityPlanningPolicies, out num9, out num10);
					if (num9 != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(3u) == 0)
					{
						Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num9, num9, buildingData.m_position, 60f);
					}
					if (num10 != 0)
					{
						Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num10, buildingData.m_position, 60f);
					}
					if (num8 < 100)
					{
						buildingData.m_flags |= Building.Flags.RateReduced;
					}
					else
					{
						buildingData.m_flags &= ~Building.Flags.RateReduced;
					}
					buildingData.m_flags |= Building.Flags.Active;
				}
				else
				{
					buildingData.m_flags &= ~(Building.Flags.RateReduced | Building.Flags.Active);
				}
			}
			else
			{
				buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Electricity | Notification.Problem.Water | Notification.Problem.Sewage | Notification.Problem.Flood);
				buildingData.m_flags &= ~(Building.Flags.RateReduced | Building.Flags.Active);
			}
			int num11 = 0;
			int wellbeing = 0;
			float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
			if (behaviourData.m_healthAccumulation != 0)
			{
				if (num != 0)
				{
					num11 = (behaviourData.m_healthAccumulation + (num >> 1)) / num;
				}
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Health, behaviourData.m_healthAccumulation, buildingData.m_position, radius);
			}
			if (behaviourData.m_wellbeingAccumulation != 0)
			{
				if (num != 0)
				{
					wellbeing = (behaviourData.m_wellbeingAccumulation + (num >> 1)) / num;
				}
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Wellbeing, behaviourData.m_wellbeingAccumulation, buildingData.m_position, radius);
			}
			int num12 = Citizen.GetHappiness(num11, wellbeing) * 15 / 100;
			if (num3 != 0)
			{
				num12 += num * 40 / num3;
			}
			if ((buildingData.m_problems & Notification.Problem.MajorProblem) == Notification.Problem.None)
			{
				num12 += 20;
			}
			if (buildingData.m_problems == Notification.Problem.None)
			{
				num12 += 25;
			}
			int taxRate = Singleton<EconomyManager>.instance.GetTaxRate(buildingData.Info.m_class, taxationPolicies);
			int num13 = (int)((ItemClass.Level)9 - buildingData.Info.m_class.m_level);
			int num14 = (int)((ItemClass.Level)11 - buildingData.Info.m_class.m_level);
			if (taxRate < num13)
			{
				num12 += num13 - taxRate;
			}
			if (taxRate > num14)
			{
				num12 -= taxRate - num14;
			}
			if (taxRate >= num14 + 4)
			{
				if (buildingData.m_taxProblemTimer != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(32u) == 0)
				{
					int num15 = taxRate - num14 >> 2;
					buildingData.m_taxProblemTimer = (byte)Mathf.Min(255, (int)buildingData.m_taxProblemTimer + num15);
					if (buildingData.m_taxProblemTimer >= 96)
					{
						buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh | Notification.Problem.MajorProblem);
					}
					else if (buildingData.m_taxProblemTimer >= 32)
					{
						buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh);
					}
				}
			}
			else
			{
				buildingData.m_taxProblemTimer = (byte)Mathf.Max(0, (int)(buildingData.m_taxProblemTimer - 1));
				buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh);
			}
			num12 = Mathf.Clamp(num12, 0, 100);
			buildingData.m_health = (byte)num11;
			buildingData.m_happiness = (byte)num12;
			buildingData.m_citizenCount = (byte)num;
			base.HandleDead(buildingID, ref buildingData, ref behaviourData, num2);
			int num16 = behaviourData.m_crimeAccumulation / 10;
			if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
			{
				num16 = num16 * 3 + 3 >> 2;
			}
			base.HandleCrime(buildingID, ref buildingData, num16, num);
			int num17 = (int)buildingData.m_crimeBuffer;
			if (num != 0)
			{
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, num, buildingData.m_position, radius);
				int num18 = behaviourData.m_educated0Count * 100 + behaviourData.m_educated1Count * 50 + behaviourData.m_educated2Count * 30;
				num18 = num18 / num + 50;
				buildingData.m_fireHazard = (byte)num18;
				num17 = (num17 + (num >> 1)) / num;
			}
			else
			{
				buildingData.m_fireHazard = 0;
				num17 = 0;
			}
			SimulationManager instance2 = Singleton<SimulationManager>.instance;
			uint num19 = (instance2.m_currentFrameIndex & 3840u) >> 8;
			if ((ulong)num19 == (ulong)((long)(buildingID & 15)) && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance2.m_currentBuildIndex && (buildingData.m_flags & Building.Flags.Upgrading) == Building.Flags.None)
			{


				/////////////////

				//CheckBuildingLevel(buildingID, ref buildingData, ref frameData, ref behaviourData, num);
			}
			///////////////////////////////////


			if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
			{
				instance.m_districts.m_buffer[(int)district].AddOfficeData(ref behaviourData, num11, num12, num17, num3, num, Mathf.Max(0, num3 - num2), (int)this.m_info.m_class.m_level, electricityConsumption, waterConsumption, sewageAccumulation, num6, num7, Mathf.Min(100, (int)(buildingData.m_garbageBuffer / 50)), (int)(buildingData.m_waterPollution * 100 / 255));
				SimulationStepActiveP(buildingID, ref buildingData, ref frameData);
				HandleFire(buildingID, ref buildingData, ref frameData, servicePolicies);
			}
		}

		public void SimulationStepActiveP(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			SimulationStepActiveC(buildingID, ref buildingData, ref frameData);
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
						RemovePeople(buildingID, ref buildingData);
						BuildingDeactivated(buildingID, ref buildingData);
						Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
					}
				}
			}
			else
			{
				buildingData.m_majorProblemTimer = 0;
			}
		}

		public void SimulationStepActiveC(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
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

		private static void CheckNearbyBuildingZones2(Vector3 position)
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