using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableRICO
{

	public class PloppableCommercial : CommercialBuildingAI
	{
		public int m_workplaceCount = 1;
		public int m_constructionCost = 1;
		public string m_subtype = "low";

		public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			minWidth = 1;
			maxWidth = 32;
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

        public override string GenerateName(ushort buildingID, InstanceID caller)
        {
            return base.m_info.GetUncheckedLocalizedTitle();
        }

        public override int GetConstructionCost()
		{
			int result = (m_constructionCost * 100);
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return result;
		}

		public override void CalculateWorkplaceCount (Randomizer r, int width, int length, out int level1,out int level2,out int level3, out int level4)
		{
			int widths = 1;

			if (m_subtype == "low") {
				widths = (m_workplaceCount + (m_workplaceCount / 3));
			}
			if (m_subtype == "high") {
				widths = (m_workplaceCount + m_workplaceCount);
			}

			base.CalculateWorkplaceCount (r, widths, 1 ,out level1,out level2,out level3, out level4);
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
			expr_6A_cp_0[(int)expr_6A_cp_1].m_servicePoliciesEffect = (expr_6A_cp_0[(int)expr_6A_cp_1].m_servicePoliciesEffect | (servicePolicies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.RecreationalUse)));
			ItemClass.SubService subService = buildingData.Info.m_class.m_subService;
			if (subService != ItemClass.SubService.CommercialLow)
			{
				if (subService != ItemClass.SubService.CommercialHigh)
				{
					if (subService != ItemClass.SubService.CommercialLeisure)
					{
						if (subService != ItemClass.SubService.CommercialTourist)
						{
						}
					}
					else
					{
						District[] expr_17D_cp_0 = instance.m_districts.m_buffer;
						byte expr_17D_cp_1 = district;
						expr_17D_cp_0[(int)expr_17D_cp_1].m_taxationPoliciesEffect = (expr_17D_cp_0[(int)expr_17D_cp_1].m_taxationPoliciesEffect | (taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure));
						District[] expr_1A1_cp_0 = instance.m_districts.m_buffer;
						byte expr_1A1_cp_1 = district;
						expr_1A1_cp_0[(int)expr_1A1_cp_1].m_cityPlanningPoliciesEffect = (expr_1A1_cp_0[(int)expr_1A1_cp_1].m_cityPlanningPoliciesEffect | (cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises));
					}
				}
				else
				{
					if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh)) != (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh))
					{
						District[] expr_133_cp_0 = instance.m_districts.m_buffer;
						byte expr_133_cp_1 = district;
						expr_133_cp_0[(int)expr_133_cp_1].m_taxationPoliciesEffect = (expr_133_cp_0[(int)expr_133_cp_1].m_taxationPoliciesEffect | (taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh)));
					}
					District[] expr_157_cp_0 = instance.m_districts.m_buffer;
					byte expr_157_cp_1 = district;
					expr_157_cp_0[(int)expr_157_cp_1].m_cityPlanningPoliciesEffect = (expr_157_cp_0[(int)expr_157_cp_1].m_cityPlanningPoliciesEffect | (cityPlanningPolicies & DistrictPolicies.CityPlanning.BigBusiness));
				}
			}
			else
			{
				if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow)) != (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow))
				{
					District[] expr_D8_cp_0 = instance.m_districts.m_buffer;
					byte expr_D8_cp_1 = district;
					expr_D8_cp_0[(int)expr_D8_cp_1].m_taxationPoliciesEffect = (expr_D8_cp_0[(int)expr_D8_cp_1].m_taxationPoliciesEffect | (taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow)));
				}
				District[] expr_FC_cp_0 = instance.m_districts.m_buffer;
				byte expr_FC_cp_1 = district;
				expr_FC_cp_0[(int)expr_FC_cp_1].m_cityPlanningPoliciesEffect = (expr_FC_cp_0[(int)expr_FC_cp_1].m_cityPlanningPoliciesEffect | (cityPlanningPolicies & DistrictPolicies.CityPlanning.SmallBusiness));
			}
			Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = HandleWorkers(buildingID, ref buildingData, ref behaviourData, ref num, ref num2, ref num3);
			int width = buildingData.Width;
			int length = buildingData.Length;
			int num5 = 4000;
			int num6 = 0;
			int num7 = 0;
			base.GetVisitBehaviour(buildingID, ref buildingData, ref behaviourData, ref num6, ref num7);
			int num8 = CalculateVisitplaceCount(new Randomizer((int)buildingID), width, length);
			int num9 = Mathf.Max(0, num8 - num7);
			int num10 = num8 * 500;
			int num11 = Mathf.Max(num10, num5 * 4);
			TransferManager.TransferReason incomingTransferReason = GetIncomingTransferReason2();
			TransferManager.TransferReason outgoingTransferReason = GetOutgoingTransferReason2(buildingID);
			if (num4 != 0)
			{
				int num12 = num11;
				if (incomingTransferReason != TransferManager.TransferReason.None)
				{
					num12 = Mathf.Min(num12, (int)buildingData.m_customBuffer1);
				}
				if (outgoingTransferReason != TransferManager.TransferReason.None)
				{
					num12 = Mathf.Min(num12, num11 - (int)buildingData.m_customBuffer2);
				}
				num4 = Mathf.Max(0, Mathf.Min(num4, (num12 * 200 + num11 - 1) / num11));
				int num13 = (num8 * num4 + 9) / 10;
				if (Singleton<SimulationManager>.instance.m_isNightTime)
				{
					num13 = num13 + 1 >> 1;
				}
				num13 = Mathf.Max(0, Mathf.Min(num13, num12));
				if (incomingTransferReason != TransferManager.TransferReason.None)
				{
					buildingData.m_customBuffer1 -= (ushort)num13;
				}
				if (outgoingTransferReason != TransferManager.TransferReason.None)
				{
					buildingData.m_customBuffer2 += (ushort)num13;
				}
				num4 = (num13 + 9) / 10;
			}
			int num14;
			int num15;
			int num16;
			int num17;
			int num18;
			GetConsumptionRates(new Randomizer((int)buildingID), num4, out num14, out num15, out num16, out num17, out num18);
			if (num17 != 0 && (servicePolicies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
			{
				num17 = Mathf.Max(1, num17 * 85 / 100);
				num18 = num18 * 95 / 100;
			}
			subService = buildingData.Info.m_class.m_subService;
			int num19;
			if (subService != ItemClass.SubService.CommercialLeisure)
			{
				if (subService != ItemClass.SubService.CommercialTourist)
				{
					num19 = Singleton<EconomyManager>.instance.GetTaxRate(buildingData.Info.m_class, taxationPolicies);
				}
				else if ((buildingData.m_flags & Building.Flags.HighDensity) != Building.Flags.None)
				{
					num19 = Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialHigh, buildingData.Info.m_class.m_level, taxationPolicies);
				}
				else
				{
					num19 = Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow, buildingData.Info.m_class.m_level, taxationPolicies);
				}
			}
			else
			{
				if ((buildingData.m_flags & Building.Flags.HighDensity) != Building.Flags.None)
				{
					num19 = Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialHigh, buildingData.Info.m_class.m_level, taxationPolicies);
				}
				else
				{
					num19 = Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow, buildingData.Info.m_class.m_level, taxationPolicies);
				}
				if ((taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure) != DistrictPolicies.Taxation.None)
				{
					num19 = 0;
				}
				if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != DistrictPolicies.CityPlanning.None && Singleton<SimulationManager>.instance.m_isNightTime)
				{
					num14 = num14 + 1 >> 1;
					num15 = num15 + 1 >> 1;
					num16 = num16 + 1 >> 1;
					num17 = num17 + 1 >> 1;
					num18 = 0;
				}
			}
			if (num4 != 0)
			{
				int num20 = HandleCommonConsumption(buildingID, ref buildingData, ref num14, ref num15, ref num16, ref num17, servicePolicies);
				num4 = (num4 * num20 + 99) / 100;
				if (num4 != 0)
				{
					int num21 = num18;
					if (num21 != 0)
					{
						if (buildingData.Info.m_class.m_subService == ItemClass.SubService.CommercialLow)
						{
							if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.SmallBusiness) != DistrictPolicies.CityPlanning.None)
							{
								Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 12, buildingData.Info.m_class);
								num21 *= 2;
							}
						}
						else if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.BigBusiness) != DistrictPolicies.CityPlanning.None)
						{
							Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 25, buildingData.Info.m_class);
							num21 *= 3;
						}
						if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
						{
							num21 = (num21 * 105 + 99) / 100;
						}
						num21 = Singleton<EconomyManager>.instance.AddPrivateIncome(num21, ItemClass.Service.Commercial, buildingData.Info.m_class.m_subService, buildingData.Info.m_class.m_level, num19);
						int num22 = (behaviourData.m_touristCount * num21 + (num6 >> 1)) / Mathf.Max(1, num6);
						int num23 = Mathf.Max(0, num21 - num22);
						if (num23 != 0)
						{
							Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.CitizenIncome, num23, buildingData.Info.m_class);
						}
						if (num22 != 0)
						{
							Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.TourismIncome, num22, buildingData.Info.m_class);
						}
					}
					int num24;
					int num25;
					GetPollutionRates(num4, cityPlanningPolicies, out num24, out num25);
					if (num24 != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(3u) == 0)
					{
						Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num24, num24, buildingData.m_position, 60f);
					}
					if (num25 != 0)
					{
						Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num25, buildingData.m_position, 60f);
					}
					if (num20 < 100)
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
			int num26 = 0;
			int wellbeing = 0;
			float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
			if (behaviourData.m_healthAccumulation != 0)
			{
				if (num + num6 != 0)
				{
					num26 = (behaviourData.m_healthAccumulation + (num + num6 >> 1)) / (num + num6);
				}
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Health, behaviourData.m_healthAccumulation, buildingData.m_position, radius);
			}
			if (behaviourData.m_wellbeingAccumulation != 0)
			{
				if (num + num6 != 0)
				{
					wellbeing = (behaviourData.m_wellbeingAccumulation + (num + num6 >> 1)) / (num + num6);
				}
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Wellbeing, behaviourData.m_wellbeingAccumulation, buildingData.m_position, radius);
			}
			int num27 = Citizen.GetHappiness(num26, wellbeing) * 15 / 100;
			int num28 = num * 20 / num3;
			if ((buildingData.m_problems & Notification.Problem.MajorProblem) == Notification.Problem.None)
			{
				num27 += 20;
			}
			if (buildingData.m_problems == Notification.Problem.None)
			{
				num27 += 25;
			}
			num27 += Mathf.Min(num28, (int)buildingData.m_customBuffer1 * num28 / num11);
			num27 += num28 - Mathf.Min(num28, (int)buildingData.m_customBuffer2 * num28 / num11);
			int num29 = (int)((ItemClass.Level)8 - buildingData.Info.m_class.m_level);
			int num30 = (int)((ItemClass.Level)11 - buildingData.Info.m_class.m_level);
			if (buildingData.Info.m_class.m_subService == ItemClass.SubService.CommercialHigh)
			{
				num29++;
				num30++;
			}
			if (num19 < num29)
			{
				num27 += num29 - num19;
			}
			if (num19 > num30)
			{
				num27 -= num19 - num30;
			}
			if (num19 >= num30 + 4)
			{
				if (buildingData.m_taxProblemTimer != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(32u) == 0)
				{
					int num31 = num19 - num30 >> 2;
					buildingData.m_taxProblemTimer = (byte)Mathf.Min(255, (int)buildingData.m_taxProblemTimer + num31);
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
			int num32;
			int num33;
			GetAccumulation2(new Randomizer((int)buildingID), num4, num19, cityPlanningPolicies, taxationPolicies, out num32, out num33);
			if (num32 != 0)
			{
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Entertainment, num32, buildingData.m_position, radius);
			}
			if (num33 != 0)
			{
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, num33);
			}
			num27 = Mathf.Clamp(num27, 0, 100);
			buildingData.m_health = (byte)num26;
			buildingData.m_happiness = (byte)num27;
			buildingData.m_citizenCount = (byte)(num + num6);
			HandleDead(buildingID, ref buildingData, ref behaviourData, num2 + num7);
			int num34 = behaviourData.m_crimeAccumulation / 10;
			if (this.m_info.m_class.m_subService == ItemClass.SubService.CommercialLeisure)
			{
				num34 = num34 * 5 + 3 >> 2;
			}
			if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
			{
				num34 = num34 * 3 + 3 >> 2;
			}
			HandleCrime(buildingID, ref buildingData, num34, (int)buildingData.m_citizenCount);
			int num35 = (int)buildingData.m_crimeBuffer;
			if (num != 0)
			{
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, num, buildingData.m_position, radius);
				int num36 = behaviourData.m_educated0Count * 100 + behaviourData.m_educated1Count * 50 + behaviourData.m_educated2Count * 30;
				num36 = num36 / num + 50;
				buildingData.m_fireHazard = (byte)num36;
			}
			else
			{
				buildingData.m_fireHazard = 0;
			}
			if (buildingData.m_citizenCount != 0)
			{
				num35 = (num35 + (buildingData.m_citizenCount >> 1)) / (int)buildingData.m_citizenCount;
			}
			else
			{
				num35 = 0;
			}
			int num37 = 0;
			int num38 = 0;
			int num39 = 0;
			int value = 0;
			if (incomingTransferReason != TransferManager.TransferReason.None)
			{
				CalculateGuestVehicles(buildingID, ref buildingData, incomingTransferReason, ref num37, ref num38, ref num39, ref value);
				buildingData.m_tempImport = (byte)Mathf.Clamp(value, (int)buildingData.m_tempImport, 255);
			}
			buildingData.m_tempExport = (byte)Mathf.Clamp(behaviourData.m_touristCount, (int)buildingData.m_tempExport, 255);
			SimulationManager instance2 = Singleton<SimulationManager>.instance;
			uint num40 = (instance2.m_currentFrameIndex & 3840u) >> 8;
			if ((ulong)num40 == (ulong)((long)(buildingID & 15)) && (buildingData.Info.m_class.m_subService == ItemClass.SubService.CommercialLow || buildingData.Info.m_class.m_subService == ItemClass.SubService.CommercialHigh) && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance2.m_currentBuildIndex && (buildingData.m_flags & Building.Flags.Upgrading) == Building.Flags.None)
			{


				/////////////////////////

				//this.CheckBuildingLevel(buildingID, ref buildingData, ref frameData, ref behaviourData, num6);
			}

			//////////////////



			if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
			{
				Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoCustomers | Notification.Problem.NoGoods);
				if ((int)buildingData.m_customBuffer2 > num11 - (num10 >> 1) && num6 <= num8 >> 1)
				{
					buildingData.m_outgoingProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_outgoingProblemTimer + 1));
					if (buildingData.m_outgoingProblemTimer >= 192)
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoCustomers | Notification.Problem.MajorProblem);
					}
					else if (buildingData.m_outgoingProblemTimer >= 128)
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoCustomers);
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
						problem = Notification.AddProblems(problem, Notification.Problem.NoGoods);
					}
					else
					{
						problem = Notification.AddProblems(problem, Notification.Problem.NoGoods | Notification.Problem.MajorProblem);
					}
				}
				else
				{
					buildingData.m_incomingProblemTimer = 0;
				}
				buildingData.m_problems = problem;
				if (buildingData.m_fireIntensity == 0 && incomingTransferReason != TransferManager.TransferReason.None)
				{
					int num41 = num11 - (int)buildingData.m_customBuffer1 - num39;
					num41 -= num5 >> 1;
					if (num41 >= 0)
					{
						TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
						offer.Priority = num41 * 8 / num5;
						offer.Building = buildingID;
						offer.Position = buildingData.m_position;
						offer.Amount = 1;
						offer.Active = false;
						Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
					}
				}
				if (buildingData.m_fireIntensity == 0 && outgoingTransferReason != TransferManager.TransferReason.None)
				{
					int num42 = (int)buildingData.m_customBuffer2 - num6 * 100;
					if (num42 >= 100 && num9 > 0)
					{
						TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
						offer2.Priority = Mathf.Max(1, num42 * 8 / num11);
						offer2.Building = buildingID;
						offer2.Position = buildingData.m_position;
						offer2.Amount = Mathf.Min(num42 / 100, num9);
						offer2.Active = false;
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

		public override void SimulationStep (ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			buildingData.m_garbageBuffer = 30;
			buildingData.m_fireHazard = 0;
			buildingData.m_fireIntensity = 0;
			buildingData.m_majorProblemTimer = 0;

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

		private TransferManager.TransferReason GetOutgoingTransferReason2(ushort buildingID)
		{
			int num = 0;
			ItemClass.SubService subService = this.m_info.m_class.m_subService;
			if (subService != ItemClass.SubService.CommercialLow)
			{
				if (subService != ItemClass.SubService.CommercialHigh)
				{
					if (subService != ItemClass.SubService.CommercialLeisure)
					{
						if (subService == ItemClass.SubService.CommercialTourist)
						{
							num = 80;
						}
					}
					else
					{
						num = 20;
					}
				}
				else
				{
					num = 4;
				}
			}
			else
			{
				num = 2;
			}
			Randomizer randomizer = new Randomizer((int)buildingID);
			if (randomizer.Int32(100u) < num)
			{
				switch (randomizer.Int32(4u))
				{
				case 0:
					return TransferManager.TransferReason.Entertainment;
				case 1:
					return TransferManager.TransferReason.EntertainmentB;
				case 2:
					return TransferManager.TransferReason.EntertainmentC;
				case 3:
					return TransferManager.TransferReason.EntertainmentD;
				default:
					return TransferManager.TransferReason.Entertainment;
				}
			}
			else
			{
				switch (randomizer.Int32(8u))
				{
				case 0:
					return TransferManager.TransferReason.Shopping;
				case 1:
					return TransferManager.TransferReason.ShoppingB;
				case 2:
					return TransferManager.TransferReason.ShoppingC;
				case 3:
					return TransferManager.TransferReason.ShoppingD;
				case 4:
					return TransferManager.TransferReason.ShoppingE;
				case 5:
					return TransferManager.TransferReason.ShoppingF;
				case 6:
					return TransferManager.TransferReason.ShoppingG;
				case 7:
					return TransferManager.TransferReason.ShoppingH;
				default:
					return TransferManager.TransferReason.Shopping;
				}
			}
		}

		private TransferManager.TransferReason GetIncomingTransferReason2()
		{
			return this.m_incomingResource;
		}

		private void GetAccumulation2(Randomizer r, int productionRate, int taxRate, DistrictPolicies.CityPlanning cityPlanningPolicies, DistrictPolicies.Taxation taxationPolicies, out int entertainment, out int attractiveness)
		{
			ItemClass @class = this.m_info.m_class;
			entertainment = 0;
			attractiveness = 0;
			ItemClass.SubService subService = @class.m_subService;
			if (subService == ItemClass.SubService.CommercialLeisure)
			{
				if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != DistrictPolicies.CityPlanning.None)
				{
					entertainment = 25;
					attractiveness = 2;
				}
				else
				{
					entertainment = 50;
					attractiveness = 4;
				}
				if ((taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure) != DistrictPolicies.Taxation.None)
				{
					entertainment += 50;
					attractiveness += 4;
				}
				productionRate += productionRate / (taxRate + 1);
			}
			if (entertainment != 0)
			{
				entertainment = (productionRate * entertainment + r.Int32(100u)) / 100;
			}
			if (attractiveness != 0)
			{
				attractiveness = (productionRate * attractiveness + r.Int32(100u)) / 100;
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