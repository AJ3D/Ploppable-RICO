using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableRICO
{

	public class PloppableIndustrial : IndustrialBuildingAI

	{
		public int m_constructionCost = 1;
		public int m_workplaceCount = 1;

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

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}

		public override int GetConstructionCost()
		{
			int result = (m_constructionCost * 100);
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return 1;
		}

		public override void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
		{
			int widths = m_workplaceCount;

			base.CalculateWorkplaceCount (r, widths, 1 ,out level0,out level1,out level2, out level3);
		}

		public override void SimulationStep (ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			//Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, 500, 500, data.m_position, this.m_pollutionRadius);

			buildingData.m_garbageBuffer = 0;
			buildingData.m_fireHazard = 0;
			buildingData.m_fireIntensity = 0;
			buildingData.m_majorProblemTimer = 0;

			SimulationStepP(buildingID, ref buildingData, ref frameData);
	
			SimulationManager instance = Singleton<SimulationManager>.instance;
			DistrictManager instance2 = Singleton<DistrictManager>.instance;
			byte district = instance2.GetDistrict(buildingData.m_position);
			if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
			{
				instance2.m_districts.m_buffer[(int)district].AddIndustrialData(buildingData.Width * buildingData.Length, (buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None, (buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None, this.m_info.m_class.m_subService);
			}

			/*
			if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10u) == 0)
			{
				DistrictPolicies.Specialization specializationPolicies = instance2.m_districts.m_buffer[(int)district].m_specializationPolicies;
				DistrictPolicies.Specialization specialization = SpecialPolicyNeeded();
				if (specialization != DistrictPolicies.Specialization.None)
				{
					if ((specializationPolicies & specialization) == DistrictPolicies.Specialization.None)
					{
						if (Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
						{
							//buildingData.m_flags |= Building.Flags.Demolishing;
							instance.m_currentBuildIndex += 1u;
						}
					}
					else
					{
						District[] expr_11A_cp_0 = instance2.m_districts.m_buffer;
						byte expr_11A_cp_1 = district;
						expr_11A_cp_0[(int)expr_11A_cp_1].m_specializationPoliciesEffect = (expr_11A_cp_0[(int)expr_11A_cp_1].m_specializationPoliciesEffect | specialization);
					}
				}
				else if ((specializationPolicies & (DistrictPolicies.Specialization.Forest | DistrictPolicies.Specialization.Farming | DistrictPolicies.Specialization.Oil | DistrictPolicies.Specialization.Ore)) != DistrictPolicies.Specialization.None && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
				{
					//buildingData.m_flags |= Building.Flags.Demolishing;
					instance.m_currentBuildIndex += 1u;
				}
			}
			*/
			uint num = (instance.m_currentFrameIndex & 3840u) >> 8;
			if (num == 15u)
			{
				buildingData.m_finalImport = buildingData.m_tempImport;
				buildingData.m_finalExport = buildingData.m_tempExport;
				buildingData.m_tempImport = 0;
				buildingData.m_tempExport = 0;
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
			DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
			District[] expr_52_cp_0 = instance.m_districts.m_buffer;
			byte expr_52_cp_1 = district;
			expr_52_cp_0[(int)expr_52_cp_1].m_servicePoliciesEffect = (expr_52_cp_0[(int)expr_52_cp_1].m_servicePoliciesEffect | (servicePolicies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.Recycling)));
			District[] expr_73_cp_0 = instance.m_districts.m_buffer;
			byte expr_73_cp_1 = district;
			expr_73_cp_0[(int)expr_73_cp_1].m_cityPlanningPoliciesEffect = (expr_73_cp_0[(int)expr_73_cp_1].m_cityPlanningPoliciesEffect | (cityPlanningPolicies & DistrictPolicies.CityPlanning.IndustrySpace));
			Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = base.HandleWorkers(buildingID, ref buildingData, ref behaviourData, ref num, ref num2, ref num3);
			if (Singleton<SimulationManager>.instance.m_isNightTime)
			{
				num4 = num4 + 1 >> 1;
			}
			TransferManager.TransferReason incomingTransferReason = GetIncomingTransferReason2(buildingID);
			TransferManager.TransferReason outgoingTransferReason = GetOutgoingTransferReason2();
			int width = buildingData.Width;
			int length = buildingData.Length;
			int num5 = 4000;
			int num6 = 8000;
			int num7 = CalculateProductionCapacity(new Randomizer((int)buildingID), width, length);
			int consumptionDivider = GetConsumptionDivider2();
			int num8 = Mathf.Max(num7 * 500 / consumptionDivider, num5 * 4);
			int num9 = num7 * 500;
			int num10 = Mathf.Max(num9, num6 * 2);
			if (num4 != 0)
			{
				int num11 = num10;
				if (incomingTransferReason != TransferManager.TransferReason.None)
				{
					num11 = Mathf.Min(num11, (int)buildingData.m_customBuffer1 * consumptionDivider);
				}
				if (outgoingTransferReason != TransferManager.TransferReason.None)
				{
					num11 = Mathf.Min(num11, num10 - (int)buildingData.m_customBuffer2);
				}
				num4 = Mathf.Max(0, Mathf.Min(num4, (num11 * 200 + num10 - 1) / num10));
				int num12 = (num7 * num4 + 9) / 10;
				num12 = Mathf.Max(0, Mathf.Min(num12, num11));
				if (incomingTransferReason != TransferManager.TransferReason.None)
				{
					buildingData.m_customBuffer1 -= (ushort)((num12 + consumptionDivider - 1) / consumptionDivider);
				}
				if (outgoingTransferReason != TransferManager.TransferReason.None)
				{
					if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.IndustrySpace) != DistrictPolicies.CityPlanning.None)
					{
						Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 38, buildingData.Info.m_class);
						buildingData.m_customBuffer2 = (ushort)Mathf.Min(num10, (int)buildingData.m_customBuffer2 + num12 * 2);
					}
					else
					{
						buildingData.m_customBuffer2 += (ushort)num12;
					}
					IndustrialBuildingAI.ProductType productType = IndustrialBuildingAI.GetProductType(outgoingTransferReason);
					if (productType != IndustrialBuildingAI.ProductType.None)
					{
						StatisticsManager instance2 = Singleton<StatisticsManager>.instance;
						StatisticBase statisticBase = instance2.Acquire<StatisticArray>(StatisticType.GoodsProduced);
						statisticBase.Acquire<StatisticInt32>((int)productType, 5).Add(num12);
					}
				}
				num4 = (num12 + 9) / 10;
			}
			int electricityConsumption;
			int waterConsumption;
			int sewageAccumulation;
			int num13;
			int num14;
			GetConsumptionRates(new Randomizer((int)buildingID), num4, out electricityConsumption, out waterConsumption, out sewageAccumulation, out num13, out num14);
			if (num13 != 0 && (servicePolicies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
			{
				num13 = Mathf.Max(1, num13 * 85 / 100);
				num14 = num14 * 95 / 100;
			}
			if (Singleton<SimulationManager>.instance.m_isNightTime)
			{
				num14 <<= 1;
			}
			if (num4 != 0)
			{
				int num15 = HandleCommonConsumption(buildingID, ref buildingData, ref electricityConsumption, ref waterConsumption, ref sewageAccumulation, ref num13, servicePolicies);
				num4 = (num4 * num15 + 99) / 100;
				if (num4 != 0)
				{
					if (num14 != 0)
					{
						Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PrivateIncome, num14, buildingData.Info.m_class);
					}
					int num16;
					int num17;
					GetPollutionRates(num4, cityPlanningPolicies, out num16, out num17);
					if (num16 != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(3u) == 0)
					{
						Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num16, num16, buildingData.m_position, 60f);
					}
					if (num17 != 0)
					{
						Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num17, buildingData.m_position, 60f);
					}
					if (num15 < 100)
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
			int num18 = 0;
			int wellbeing = 0;
			float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
			if (behaviourData.m_healthAccumulation != 0)
			{
				if (num != 0)
				{
					num18 = (behaviourData.m_healthAccumulation + (num >> 1)) / num;
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
			int num19 = Citizen.GetHappiness(num18, wellbeing) * 15 / 100;
			if (num3 != 0)
			{
				num19 += num * 40 / num3;
			}
			if ((buildingData.m_problems & Notification.Problem.MajorProblem) == Notification.Problem.None)
			{
				num19 += 20;
			}
			if (buildingData.m_problems == Notification.Problem.None)
			{
				num19 += 25;
			}
			int taxRate = Singleton<EconomyManager>.instance.GetTaxRate(buildingData.Info.m_class);
			int num20 = (int)((ItemClass.Level)9 - buildingData.Info.m_class.m_level);
			int num21 = (int)((ItemClass.Level)11 - buildingData.Info.m_class.m_level);
			if (taxRate < num20)
			{
				num19 += num20 - taxRate;
			}
			if (taxRate > num21)
			{
				num19 -= taxRate - num21;
			}
			if (taxRate >= num21 + 4)
			{
				if (buildingData.m_taxProblemTimer != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(32u) == 0)
				{
					int num22 = taxRate - num21 >> 2;
					buildingData.m_taxProblemTimer = (byte)Mathf.Min(255, (int)buildingData.m_taxProblemTimer + num22);
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
			num19 = Mathf.Clamp(num19, 0, 100);
			buildingData.m_health = (byte)num18;
			buildingData.m_happiness = (byte)num19;
			buildingData.m_citizenCount = (byte)num;
			base.HandleDead(buildingID, ref buildingData, ref behaviourData, num2);
			int num23 = behaviourData.m_crimeAccumulation / 10;
			if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
			{
				num23 = num23 * 3 + 3 >> 2;
			}
			base.HandleCrime(buildingID, ref buildingData, num23, num);
			int num24 = (int)buildingData.m_crimeBuffer;
			if (num != 0)
			{
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, num, buildingData.m_position, radius);
				int num25 = behaviourData.m_educated0Count * 100 + behaviourData.m_educated1Count * 50 + behaviourData.m_educated2Count * 30;
				num25 = num25 / num + 100;
				buildingData.m_fireHazard = (byte)num25;
				num24 = (num24 + (num >> 1)) / num;
			}
			else
			{
				buildingData.m_fireHazard = 0;
				num24 = 0;
			}
			int num26 = 0;
			int num27 = 0;
			int num28 = 0;
			int value = 0;
			if (incomingTransferReason != TransferManager.TransferReason.None)
			{
				CalculateGuestVehicles(buildingID, ref buildingData, incomingTransferReason, ref num26, ref num27, ref num28, ref value);
				buildingData.m_tempImport = (byte)Mathf.Clamp(value, (int)buildingData.m_tempImport, 255);
			}
			int num29 = 0;
			int num30 = 0;
			int num31 = 0;
			int value2 = 0;
			if (outgoingTransferReason != TransferManager.TransferReason.None)
			{
				base.CalculateOwnVehicles(buildingID, ref buildingData, outgoingTransferReason, ref num29, ref num30, ref num31, ref value2);
				buildingData.m_tempExport = (byte)Mathf.Clamp(value2, (int)buildingData.m_tempExport, 255);
			}
			SimulationManager instance3 = Singleton<SimulationManager>.instance;
			uint num32 = (instance3.m_currentFrameIndex & 3840u) >> 8;
			if ((ulong)num32 == (ulong)((long)(buildingID & 15)) && this.m_info.m_class.m_subService == ItemClass.SubService.IndustrialGeneric && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance3.m_currentBuildIndex && (buildingData.m_flags & Building.Flags.Upgrading) == Building.Flags.None)
			{
				//////////////


				//CheckBuildingLevel(buildingID, ref buildingData, ref frameData, ref behaviourData, num);
			}

			/////////////////////


			if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
			{
				Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoResources | Notification.Problem.NoPlaceforGoods);
				if ((int)buildingData.m_customBuffer2 > num10 - (num9 >> 1))
				{
					buildingData.m_outgoingProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_outgoingProblemTimer + 1));
					if (buildingData.m_outgoingProblemTimer >= 192)
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoPlaceforGoods | Notification.Problem.MajorProblem);
					}
					else if (buildingData.m_outgoingProblemTimer >= 128)
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoPlaceforGoods);
					}
				}
				else
				{
					buildingData.m_outgoingProblemTimer = 0;
				}
				if (buildingData.m_customBuffer1 == 0)
				{
					buildingData.m_incomingProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_incomingProblemTimer + 1));
					if (buildingData.m_incomingProblemTimer < 64)
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoResources);
					}
					else
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoResources | Notification.Problem.MajorProblem);
					}
				}
				else
				{
					buildingData.m_incomingProblemTimer = 0;
				}
				buildingData.m_problems = problem;
				instance.m_districts.m_buffer[(int)district].AddIndustrialData(ref behaviourData, num18, num19, num24, num3, num, Mathf.Max(0, num3 - num2), (int)buildingData.Info.m_class.m_level, electricityConsumption, waterConsumption, sewageAccumulation, num13, num14, Mathf.Min(100, (int)(buildingData.m_garbageBuffer / 50)), (int)(buildingData.m_waterPollution * 100 / 255), (int)buildingData.m_finalImport, (int)buildingData.m_finalExport, buildingData.Info.m_class.m_subService);
				if (buildingData.m_fireIntensity == 0 && incomingTransferReason != TransferManager.TransferReason.None)
				{
					int num33 = num8 - (int)buildingData.m_customBuffer1 - num28;
					num33 -= num5 >> 1;
					if (num33 >= 0)
					{
						TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
						offer.Priority = num33 * 8 / num5;
						offer.Building = buildingID;
						offer.Position = buildingData.m_position;
						offer.Amount = 1;
						offer.Active = false;
						Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
					}
				}
				if (buildingData.m_fireIntensity == 0 && outgoingTransferReason != TransferManager.TransferReason.None)
				{
					int num34 = Mathf.Max(1, num7 / 6);
					int customBuffer = (int)buildingData.m_customBuffer2;
					if (customBuffer >= num6 && num29 < num34)
					{
						TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
						offer2.Priority = customBuffer * 8 / num6;
						offer2.Building = buildingID;
						offer2.Position = buildingData.m_position;
						offer2.Amount = Mathf.Min(customBuffer / num6, num34 - num29);
						offer2.Active = true;
						Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, offer2);
					}
				}
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

		private TransferManager.TransferReason GetIncomingTransferReason2(ushort buildingID)
		{
			switch (this.m_info.m_class.m_subService)
			{
			case ItemClass.SubService.IndustrialForestry:
				return TransferManager.TransferReason.Logs;
			case ItemClass.SubService.IndustrialFarming:
				return TransferManager.TransferReason.Grain;
			case ItemClass.SubService.IndustrialOil:
				return TransferManager.TransferReason.Oil;
			case ItemClass.SubService.IndustrialOre:
				return TransferManager.TransferReason.Ore;
			default:
				{
					Randomizer randomizer = new Randomizer((int)buildingID);
					switch (randomizer.Int32(4u))
					{
					case 0:
						return TransferManager.TransferReason.Lumber;
					case 1:
						return TransferManager.TransferReason.Food;
					case 2:
						return TransferManager.TransferReason.Petrol;
					case 3:
						return TransferManager.TransferReason.Coal;
					default:
						return TransferManager.TransferReason.None;
					}
					break;
				}
			}
		}

		private TransferManager.TransferReason GetOutgoingTransferReason2()
		{
			switch (this.m_info.m_class.m_subService)
			{
			case ItemClass.SubService.IndustrialForestry:
				return TransferManager.TransferReason.Lumber;
			case ItemClass.SubService.IndustrialFarming:
				return TransferManager.TransferReason.Food;
			case ItemClass.SubService.IndustrialOil:
				return TransferManager.TransferReason.Petrol;
			case ItemClass.SubService.IndustrialOre:
				return TransferManager.TransferReason.Coal;
			default:
				return TransferManager.TransferReason.Goods;
			}
		}

		private int GetConsumptionDivider2()
		{
			switch (this.m_info.m_class.m_subService)
			{
			case ItemClass.SubService.IndustrialForestry:
				return 1;
			case ItemClass.SubService.IndustrialFarming:
				return 1;
			case ItemClass.SubService.IndustrialOil:
				return 1;
			case ItemClass.SubService.IndustrialOre:
				return 1;
			default:
				return 4;
			}
		}


		private static void CheckNearbyBuildingZones2(Vector3 position)
		{
			int num = Mathf.Max ((int)((position.x - 35f) / 64f + 135f), 0);
			int num2 = Mathf.Max ((int)((position.z - 35f) / 64f + 135f), 0);
			int num3 = Mathf.Min ((int)((position.x + 35f) / 64f + 135f), 269);
			int num4 = Mathf.Min ((int)((position.z + 35f) / 64f + 135f), 269);
			Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
			ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
			for (int i = num2; i <= num4; i++) {
				for (int j = num; j <= num3; j++) {
					ushort num5 = buildingGrid [i * 270 + j];
					int num6 = 0;
					while (num5 != 0) {
						ushort nextGridBuilding = buildings.m_buffer [(int)num5].m_nextGridBuilding;
						Building.Flags flags = buildings.m_buffer [(int)num5].m_flags;
						if ((flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Demolishing)) == Building.Flags.Created) {
							BuildingInfo info = buildings.m_buffer [(int)num5].Info;
							if (info != null && info.m_placementStyle == ItemClass.Placement.Automatic) {
								ItemClass.Zone zone = info.m_class.GetZone ();
								ItemClass.Zone secondaryZone = info.m_class.GetSecondaryZone ();
								if (zone != ItemClass.Zone.None && (buildings.m_buffer [(int)num5].m_flags & Building.Flags.ZonesUpdated) != Building.Flags.None && VectorUtils.LengthSqrXZ (buildings.m_buffer [(int)num5].m_position - position) <= 1225f) {
									Building[] expr_1A6_cp_0 = buildings.m_buffer;
									ushort expr_1A6_cp_1 = num5;
									expr_1A6_cp_0 [(int)expr_1A6_cp_1].m_flags = (expr_1A6_cp_0 [(int)expr_1A6_cp_1].m_flags & ~Building.Flags.ZonesUpdated);
									if (!buildings.m_buffer [(int)num5].CheckZoning (zone, secondaryZone)) {
										Singleton<BuildingManager>.instance.ReleaseBuilding (num5);
									}
								}
							}
						}
						num5 = nextGridBuilding;
						if (++num6 >= 49152) {
							CODebugBase<LogChannel>.Error (LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
							break;
						}
					}
				}
			}
		}
	}
}