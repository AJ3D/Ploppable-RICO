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
		public int m_levelmin = 1;
		public int m_levelmax = 1;
		public int m_housemulti = 1;
		public int m_constructionCost = 1;
		public string m_subtype = "Low";
		BuildingData Bdata;

		public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			base.GetWidthRange (out minWidth, out maxWidth);
			minWidth = 1;
			maxWidth = 32;
		}

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}

		public override void GetLengthRange (out int minLength, out int maxLength)
		{
			base.GetLengthRange (out minLength, out maxLength);
			minLength = 1;
			maxLength = 16;
		}
		public override string GenerateName(ushort buildingID, InstanceID caller){

			return "Store";
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

			if (m_subtype == "Low") {
				widths = (m_housemulti + (m_housemulti / 3));
			}
			if (m_subtype == "High") {
				widths = (m_housemulti + m_housemulti);
			}

			base.CalculateWorkplaceCount (r, widths, 1 ,out level1,out level2,out level3, out level4);
		}

		public void RenderLevelUpEffect(ushort buildingID, ref Building buildingData){

			BuildingManager instance = Singleton<BuildingManager>.instance;
			instance.UpdateBuildingRenderer(buildingID, true);
			EffectInfo levelupEffect = instance.m_properties.m_levelupEffect;
			if (levelupEffect != null)
			{
				InstanceID instance2 = default(InstanceID);
				instance2.Building = buildingID;
				Vector3 pos;
				Quaternion q;
				buildingData.CalculateMeshPosition(out pos, out q);
				Matrix4x4 matrix = Matrix4x4.TRS(pos, q, Vector3.one);
				EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(matrix, this.m_info.m_lodMeshData);
				Singleton<EffectManager>.instance.DispatchEffect(levelupEffect, instance2, spawnArea, Vector3.zero, 0f, 1f, instance.m_audioGroup);
			}
			Vector3 position = buildingData.m_position;
			position.y += this.m_info.m_size.y;
			Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LevelUp, position, 1f);
			Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;

		}


		public override void SimulationStep (ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{

			BuildingData[] dataArray = BuildingDataManager.buildingData;		
			Bdata = dataArray [(int)buildingID];

			if (Bdata == null) {

				Bdata = new BuildingData ();
				dataArray [(int)buildingID] = Bdata;
				Bdata.level = m_levelmin;
				Bdata.Name = buildingData.Info.name;
				Bdata.saveflag = false;
			}


			if (Bdata.saveflag == false){

				if (Bdata.level == 2) {
					buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level2");
				}
				if (Bdata.level == 3) {

					buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level3");
				}

				Bdata.saveflag = true;
			}

			if (m_levelmax == Bdata.level) { ///If Its reached max level, then dont level up. 
				buildingData.m_levelUpProgress = 240;
			}

			if (buildingData.m_levelUpProgress >= 253) { //254 is normal

				if (Bdata.level == 1) {

					buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level2");
					Bdata.level = 2;
					RenderLevelUpEffect (buildingID, ref buildingData);

				} else if (Bdata.level == 2) {

					buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level3");
					Bdata.level = 3;
					RenderLevelUpEffect (buildingID, ref buildingData);
				}
				buildingData.m_levelUpProgress = 240; //once leveled, set the building back to normal level up progress. 
			}

			buildingData.m_garbageBuffer = 30;
			buildingData.m_fireHazard = 0;
			buildingData.m_fireIntensity = 0;
			buildingData.m_majorProblemTimer = 0;

			//data.m_problems = Notification.Problem.None;
			//data.m_flags = Building.Flags.None;
			//data.m_flags |= Building.Flags.Active;
			//data.m_flags |= Building.Flags.Created;
			//data.m_flags |= Building.Flags.Completed;


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
				DistrictManager instance8 = Singleton<DistrictManager>.instance;
				byte district = instance8.GetDistrict(buildingData.m_position);
				ushort style = instance8.m_districts.m_buffer[(int)district].m_Style;
				if (style > 0 && (int)(style - 1) < instance8.m_Styles.Length)
				{
					DistrictStyle districtStyle = instance8.m_Styles[(int)(style - 1)];
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

			//////////////////////////////COMMERCIAL AI

			SimulationManager instance9 = Singleton<SimulationManager>.instance;
			//DistrictManager instance2 = Singleton<DistrictManager>.instance;
			//byte district = instance2.GetDistrict(buildingData.m_position);
			/*
			DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
			if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
			{
				instance2.m_districts.m_buffer[(int)district].AddCommercialData(buildingData.Width * buildingData.Length, (buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None, (buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None, this.m_info.m_class.m_subService);
			}
			if (this.m_info.m_class.m_subService == ItemClass.SubService.CommercialHigh && (cityPlanningPolicies & DistrictPolicies.CityPlanning.HighriseBan) != DistrictPolicies.CityPlanning.None && this.m_info.m_class.m_level == ItemClass.Level.Level3 && instance.m_randomizer.Int32(10u) == 0 && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
			{
				District[] expr_10E_cp_0 = instance2.m_districts.m_buffer;
				byte expr_10E_cp_1 = district;
				expr_10E_cp_0[(int)expr_10E_cp_1].m_cityPlanningPoliciesEffect = (expr_10E_cp_0[(int)expr_10E_cp_1].m_cityPlanningPoliciesEffect | DistrictPolicies.CityPlanning.HighriseBan);
				buildingData.m_flags |= Building.Flags.Demolishing;
				instance.m_currentBuildIndex += 1u;
			}
			if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10u) == 0)
			{
				DistrictPolicies.Specialization specializationPolicies = instance2.m_districts.m_buffer[(int)district].m_specializationPolicies;
				DistrictPolicies.Specialization specialization = this.SpecialPolicyNeeded();
				if (specialization != DistrictPolicies.Specialization.None)
				{
					if ((specializationPolicies & specialization) == DistrictPolicies.Specialization.None)
					{
						if (Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
						{
							buildingData.m_flags |= Building.Flags.Demolishing;
							instance.m_currentBuildIndex += 1u;
						}
					}
					else
					{
						District[] expr_1CE_cp_0 = instance2.m_districts.m_buffer;
						byte expr_1CE_cp_1 = district;
						expr_1CE_cp_0[(int)expr_1CE_cp_1].m_specializationPoliciesEffect = (expr_1CE_cp_0[(int)expr_1CE_cp_1].m_specializationPoliciesEffect | specialization);
					}
				}
				else if ((specializationPolicies & (DistrictPolicies.Specialization.Leisure | DistrictPolicies.Specialization.Tourist)) != DistrictPolicies.Specialization.None && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance.m_currentBuildIndex)
				{
					buildingData.m_flags |= Building.Flags.Demolishing;
					instance.m_currentBuildIndex += 1u;
				}
			}

			*/
			uint num9 = (instance9.m_currentFrameIndex & 3840u) >> 8;
			if (num9 == 15u)
			{
				buildingData.m_finalImport = buildingData.m_tempImport;
				buildingData.m_finalExport = buildingData.m_tempExport;
				buildingData.m_tempImport = 0;
				buildingData.m_tempExport = 0;
			}
		}

	}
}