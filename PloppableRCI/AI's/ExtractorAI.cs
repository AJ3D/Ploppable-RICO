using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableRICO
{

	public class PloppableExtractor : IndustrialExtractorAI

	{

		public int m_levelmin = 1;
		public int m_levelmax = 1;
		public int m_maintenanceCost = 100;
		public int m_constructionCost = 1;
		public int m_housemulti = 1;

		BuildingData Bdata;


		float m_pollutionRadius = 400f;

		public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			base.GetWidthRange (out minWidth, out maxWidth);
			minWidth = 1;
			maxWidth = 32;
		}


		public override void GetLengthRange (out int minLength, out int maxLength)
		{
			base.GetLengthRange (out minLength, out maxLength);
			minLength = 1;
			maxLength = 16;
		}

		public override int GetConstructionCost()
		{
			int result = (m_constructionCost * 100);
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return result;
		}

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}

		public override void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
		{
			int widths = m_housemulti;

			base.CalculateWorkplaceCount (r, widths, 1 ,out level0,out level1,out level2, out level3);
		}

		public override void SimulationStep (ushort buildingID, ref Building buildingData, ref Building.Frame frameData)

		{
			
		
			buildingData.UpdateBuilding ((ushort)buildingID);

			BuildingData[] dataArray = BuildingDataManager.buildingData;		
			Bdata = dataArray [(int)buildingID];

			if (Bdata == null) {

				Bdata = new BuildingData ();
				dataArray [(int)buildingID] = Bdata;
				Bdata.Name = buildingData.Info.name;
				Bdata.level = 1;
				Bdata.saveflag = false;
			}

			if (Bdata.saveflag == false) {
				//buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.fieldA + "_Level1");
				Bdata.saveflag = true;
			}

			//Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, 500, 500, data.m_position, this.m_pollutionRadius);

			//buildingData.m_problems = Notification.Problem.None;
			buildingData.m_flags = Building.Flags.None;
			buildingData.m_flags |= Building.Flags.Created;
			buildingData.m_flags |= Building.Flags.Completed;
	

			buildingData.m_garbageBuffer = 0;
			buildingData.m_fireHazard = 0;
			buildingData.m_fireIntensity = 0;
			buildingData.m_majorProblemTimer = 0;


			/////////////////////////////COMMON BUILDING AI
	 
			if ((buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None)
			{
				GuideController properties = Singleton<GuideManager>.instance.m_properties;
				if (properties != null)
				{
					Singleton<BuildingManager>.instance.m_buildingAbandoned1.Activate(properties.m_buildingAbandoned1, buildingID);
					Singleton<BuildingManager>.instance.m_buildingAbandoned2.Activate(properties.m_buildingAbandoned2, buildingID);
				}
				if (buildingData.m_majorProblemTimer < 255)
				{
					buildingData.m_majorProblemTimer += 1;
				}
				float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Abandonment, 10, buildingData.m_position, radius);
			}
			else if ((buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
			{
				GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
				if (properties2 != null)
				{
					Singleton<BuildingManager>.instance.m_buildingBurned.Activate(properties2.m_buildingBurned, buildingID);
				}
				float radius2 = (float)(buildingData.Width + buildingData.Length) * 2.5f;
				Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Abandonment, 10, buildingData.m_position, radius2);
			}
			else if ((buildingData.m_flags & Building.Flags.Completed) == Building.Flags.None)
			{
				bool flag = (buildingData.m_flags & Building.Flags.Upgrading) != Building.Flags.None;
				int constructionTime = this.GetConstructionTime();
				if (constructionTime == 0)
				{
					frameData.m_constructState = 255;
				}
				else
				{
					frameData.m_constructState = (byte)Mathf.Min(255, (int)frameData.m_constructState + 1088 / constructionTime);
				}
				if (frameData.m_constructState == 255)
				{
					this.BuildingCompleted(buildingID, ref buildingData);
					GuideController properties3 = Singleton<GuideManager>.instance.m_properties;
					if (properties3 != null)
					{
						Singleton<BuildingManager>.instance.m_buildingLevelUp.Deactivate(buildingID, true);
					}
				}
				else if (flag)
				{
					GuideController properties4 = Singleton<GuideManager>.instance.m_properties;
					if (properties4 != null)
					{
						//Singleton<BuildingManager>.instance.m_buildingLevelUp.Activate(properties4.m_buildingLevelUp, buildingID);
					}
				}
				if (flag)
				{
					this.SimulationStepActive(buildingID, ref buildingData, ref frameData);
				}
			}
			else
			{
				this.SimulationStepActive(buildingID, ref buildingData, ref frameData);
			}

			/// /////////////////////////////COMMON BUILDING AI


			/////////////////////////////////////////////////////PRIVATEBUILDINGAI

			if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10u) == 0)
			{
				DistrictManager instance = Singleton<DistrictManager>.instance;
				byte district = instance.GetDistrict(buildingData.m_position);
				ushort style = instance.m_districts.m_buffer[(int)district].m_Style;
				if (style > 0 && (int)(style - 1) < instance.m_Styles.Length)
				{
					DistrictStyle districtStyle = instance.m_Styles[(int)(style - 1)];
					if (districtStyle != null && this.m_info.m_class != null && districtStyle.AffectsService(this.m_info.GetService(), this.m_info.GetSubService(), this.m_info.m_class.m_level) && !districtStyle.Contains(this.m_info) && Singleton<ZoneManager>.instance.m_lastBuildIndex == Singleton<SimulationManager>.instance.m_currentBuildIndex)
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
					if (!buildingData.CheckZoning(this.m_info.m_class.GetZone(), this.m_info.m_class.GetSecondaryZone()))
					{
						//buildingData.m_flags |= Building.Flags.Demolishing;
						//PrivateBuildingAI.CheckNearbyBuildingZones(buildingData.m_position);
						instance2.m_currentBuildIndex += 1u;
					}
				}
			}
			else if ((buildingData.m_flags & (Building.Flags.Abandoned | Building.Flags.Downgrading)) != Building.Flags.None && (buildingData.m_majorProblemTimer == 255 || (buildingData.m_flags & Building.Flags.Abandoned) == Building.Flags.None))
			{
				SimulationManager instance3 = Singleton<SimulationManager>.instance;
				ZoneManager instance4 = Singleton<ZoneManager>.instance;
				int num;
				switch (this.m_info.m_class.m_service)
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
						ItemClass.SubService subService = this.m_info.m_class.m_subService;
						ItemClass.Level level = ItemClass.Level.Level1;
						int width = buildingData.Width;
						int num3 = buildingData.Length;
						if (this.m_info.m_class.m_service == ItemClass.Service.Industrial)
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
						BuildingInfo randomBuildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, this.m_info.m_class.m_service, subService, level, width, num3, this.m_info.m_zoningMode, (int)style2);
						if (randomBuildingInfo != null)
						{
							//buildingData.m_flags |= Building.Flags.Demolishing;
							float num4 = buildingData.m_angle + 1.57079637f;
							if (this.m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft && randomBuildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerRight)
							{
								num4 -= 1.57079637f;
								num3 = width;
							}
							else if (this.m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerRight && randomBuildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
							{
								num4 += 1.57079637f;
								num3 = width;
							}
							ushort num5;

							/*
							if (Singleton<BuildingManager>.instance.CreateBuilding(out num5, ref Singleton<SimulationManager>.instance.m_randomizer, randomBuildingInfo, buildingData.m_position, buildingData.m_angle, num3, Singleton<SimulationManager>.instance.m_currentBuildIndex))
							{
								Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
								switch (this.m_info.m_class.m_service)
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
							}  */
							instance3.m_currentBuildIndex += 1u;
						}
					}
				}
			}

			////////////////////////////////////////////////////PRIVATEBUILDINGAI




			///////////////////////////////////////INDUSTRIALEXTRACTOR SIM STEP
			/*

			SimulationManager instance5 = Singleton<SimulationManager>.instance;
			DistrictManager instance4 = Singleton<DistrictManager>.instance;
			byte district = instance4.GetDistrict(buildingData.m_position);
			if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
			{
				instance4.m_districts.m_buffer[(int)district].AddIndustrialData(buildingData.Width * buildingData.Length, (buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None, (buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None, this.m_info.m_class.m_subService);
			}
			if (instance5.m_randomizer.Int32(10u) == 0)
			{
				DistrictPolicies.Specialization specializationPolicies = instance4.m_districts.m_buffer[(int)district].m_specializationPolicies;
				DistrictPolicies.Specialization specialization = this.SpecialPolicyNeeded();
				if (specialization != DistrictPolicies.Specialization.None)
				{
					if ((specializationPolicies & specialization) == DistrictPolicies.Specialization.None)
					{
						if (Singleton<ZoneManager>.instance.m_lastBuildIndex == instance5.m_currentBuildIndex)
						{
							buildingData.m_flags |= Building.Flags.Demolishing;
							instance5.m_currentBuildIndex += 1u;
						}
					}
					else
					{
						District[] expr_116_cp_0 = instance4.m_districts.m_buffer;
						byte expr_116_cp_1 = district;
						expr_116_cp_0[(int)expr_116_cp_1].m_specializationPoliciesEffect = (expr_116_cp_0[(int)expr_116_cp_1].m_specializationPoliciesEffect | specialization);
					}
				}
				else if ((specializationPolicies & (DistrictPolicies.Specialization.Forest | DistrictPolicies.Specialization.Farming | DistrictPolicies.Specialization.Oil | DistrictPolicies.Specialization.Ore)) != DistrictPolicies.Specialization.None && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
				{
					buildingData.m_flags |= Building.Flags.Demolishing;
					instance5.m_currentBuildIndex += 1u;
				}
			}
			uint num = (instance5.m_currentFrameIndex & 3840u) >> 8;
			if (num == 15u)
			{
				buildingData.m_finalExport = buildingData.m_tempExport;
				buildingData.m_tempExport = 0;
			}
*/
			//////////////////////////////////////////INDUSTRIALEXTRACTOR SIM STEP



			//base.SimulationStep(buildingID, ref data);

			//buildingData.m_problems = Notification.Problem.None;
			buildingData.m_flags = Building.Flags.None;
			buildingData.m_flags |= Building.Flags.Created;
			buildingData.m_flags |= Building.Flags.Completed;

		}
	}
}