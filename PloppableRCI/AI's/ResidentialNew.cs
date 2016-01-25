using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;
using ColossalFramework.Globalization;

namespace PloppableRICO
{

public class PloppableResidential2 : BuildingAI
{
	public bool m_ignoreNoPropsWarning;
	[CustomizableProperty("Construction Time")]
	public int m_constructionTime = 0;
	public int m_levelmin = 1;
	public int m_levelmax = 1;
	public int m_constructionCost = 1;
	public int m_households = 1;
	public string m_name = "Residential";
	BuildingData Bdata;
	int leveltimer = 20;
	public byte m_levelprog = 0;

	public Color GetColorC(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
	{
		InfoManager.InfoMode infoMode1 = infoMode;
		switch (infoMode1)
		{
		case InfoManager.InfoMode.None:
			Color color1 = base.GetColor(buildingID, ref data, infoMode);
			if (this.ShowConsumption(buildingID, ref data) && (int) data.m_fireIntensity == 0)
			{
				bool electricity;
				Singleton<ElectricityManager>.instance.CheckElectricity(data.m_position, out electricity);
				color1.a = !electricity ? 0.0f : 1f;
			}
			else
				color1.a = 0.0f;
			return color1;
		case InfoManager.InfoMode.Electricity:
			if (!this.ShowConsumption(buildingID, ref data))
				return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
			bool electricity1;
			Singleton<ElectricityManager>.instance.CheckElectricity(data.m_position, out electricity1);
			if (electricity1)
			{
				Color color2 = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor;
				color2.a = 0.0f;
				return color2;
			}
			Color color3 = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor;
			color3.a = 0.0f;
			return color3;
		case InfoManager.InfoMode.Water:
			if (!this.ShowConsumption(buildingID, ref data))
				return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
			bool water;
			bool sewage;
			byte waterPollution;
			Singleton<WaterManager>.instance.CheckWater(data.m_position, out water, out sewage, out waterPollution);
			if (water && sewage)
				return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor;
			if (water)
				return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, 0.5f);
			return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor;
		case InfoManager.InfoMode.CrimeRate:
			if (this.ShowConsumption(buildingID, ref data))
				return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, (float) data.m_crimeBuffer / Mathf.Max(1f, (float) data.m_citizenCount * 100f));
			return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
		case InfoManager.InfoMode.Happiness:
			if (this.ShowConsumption(buildingID, ref data))
				return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) Citizen.GetHappinessLevel((int) data.m_happiness) * 0.25f);
			return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
		default:
			switch (infoMode1 - 16)
			{
			case InfoManager.InfoMode.None:
				return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, (float) Mathf.Min(100, (int) data.m_garbageBuffer / 50) * 0.01f);
			case InfoManager.InfoMode.Water:
				int fireHazard;
				int fireSize;
				int fireTolerance;
				if (!this.ShowConsumption(buildingID, ref data) || !this.GetFireParameters(buildingID, ref data, out fireHazard, out fireSize, out fireTolerance))
					return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
				if (fireHazard != 0)
				{
					DistrictManager instance = Singleton<DistrictManager>.instance;
					byte district = instance.GetDistrict(data.m_position);
					if ((instance.m_districts.m_buffer[(int) district].m_servicePolicies & DistrictPolicies.Services.SmokeDetectors) != DistrictPolicies.Services.None)
						fireHazard = fireHazard * 75 / 100;
				}
				int num1 = (int) Singleton<CoverageManager>.instance.FindFireCoverage(data.CalculateSidewalkPosition()) * 100 / (int) byte.MaxValue;
				int num2 = Mathf.Min(100, (10 + fireTolerance) * (25 + num1) * 2000 / ((100 + fireHazard) * (100 + fireSize)));
				return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) num2 * 0.01f);
			case InfoManager.InfoMode.Health:
				if (!this.ShowConsumption(buildingID, ref data))
					return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
				int local;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.Entertainment, data.m_position, out local);
				return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, Mathf.Clamp01((float) local * 0.005f));
			default:
				return base.GetColor(buildingID, ref data, infoMode);
			}
		}
	}
	public static Color GetNoisePollutionColor(float amount)
	{
		InfoManager instance = Singleton<InfoManager>.instance;
		amount = Mathf.Clamp01(amount * 0.04f);
		Color color;
		Color color2;
		if (amount >= 0.5f)
		{
			color = instance.m_properties.m_modeProperties[7].m_activeColorB;
			color2 = instance.m_properties.m_modeProperties[7].m_activeColor;
			amount = amount * 2f - 1f;
		}
		else
		{
			color = instance.m_properties.m_neutralColor;
			color2 = instance.m_properties.m_modeProperties[7].m_activeColorB;
			amount *= 2f;
		}
		return ColorUtils.LinearLerp(color, color2, amount);
	}
	protected virtual bool ShowConsumptionC(ushort buildingID, ref Building data)
	{
		return true;
	}
	public override int GetGarbageAmount(ushort buildingID, ref Building data)
	{
		return (int)data.m_garbageBuffer;
	}
	public override string GetDebugString(ushort buildingID, ref Building data)
	{
		CitizenManager instance = Singleton<CitizenManager>.instance;
		uint num = data.m_citizenUnits;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		while (num != 0u)
		{
			uint nextUnit = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
			if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Home) != 0)
			{
				num4 += 5;
				for (int i = 0; i < 5; i++)
				{
					uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
					if (citizen != 0u)
					{
						num3++;
					}
				}
			}
			else if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Work) != 0)
			{
				num6 += 5;
				for (int j = 0; j < 5; j++)
				{
					uint citizen2 = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(j);
					if (citizen2 != 0u)
					{
						num5++;
					}
				}
			}
			else if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Student) != 0)
			{
				num8 += 5;
				for (int k = 0; k < 5; k++)
				{
					uint citizen3 = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(k);
					if (citizen3 != 0u)
					{
						num7++;
					}
				}
			}
			for (int l = 0; l < 5; l++)
			{
				uint citizen4 = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(l);
				if (citizen4 != 0u && instance.m_citizens.m_buffer[(int)((UIntPtr)citizen4)].Dead && instance.m_citizens.m_buffer[(int)((UIntPtr)citizen4)].GetBuildingByLocation() == buildingID)
				{
					num9++;
				}
			}
			num = nextUnit;
			if (++num2 > 524288)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
		return string.Format("Electricity: {0}\nWater: {1} ({2}% polluted)\nSewage: {3}\nGarbage: {4}\nCrime: {5}\nResidents: {6}/{7}\nWorkers: {8}/{9}\nStudents: {10}/{11}\nDead: {12}", new object[]
			{
				data.m_electricityBuffer,
				data.m_waterBuffer,
				(int)(data.m_waterPollution * 100 / 255),
				data.m_sewageBuffer,
				data.m_garbageBuffer,
				data.m_crimeBuffer,
				num3,
				num4,
				num5,
				num6,
				num7,
				num8,
				num9
			});
	}
	public override void RefreshInstance(RenderManager.CameraInfo cameraInfo, ushort buildingID, ref Building data, int layerMask, ref RenderManager.Instance instance)
	{
		if ((data.m_flags & (Building.Flags.Completed | Building.Flags.BurnedDown)) != Building.Flags.Completed)
		{
			if ((data.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
			{
				BuildingAI.RefreshInstance(this.m_info, cameraInfo, buildingID, ref data, layerMask, ref instance);
			}
			else
			{
				if ((data.m_flags & Building.Flags.Upgrading) != Building.Flags.None)
				{
					BuildingInfo upgradeInfo = this.GetUpgradeInfo(buildingID, ref data);
					if (upgradeInfo != null && instance.m_dataInt0 == upgradeInfo.m_prefabDataIndex)
					{
						BuildingAI.RefreshInstance(upgradeInfo, cameraInfo, buildingID, ref data, layerMask, ref instance);
						return;
					}
				}
				BuildingAI.RefreshInstance(this.m_info, cameraInfo, buildingID, ref data, layerMask, ref instance);
			}
		}
		else
		{
			base.RefreshInstance(cameraInfo, buildingID, ref data, layerMask, ref instance);
			Randomizer randomizer = new Randomizer((int)buildingID);
			PropInfo randomPropInfo = Singleton<PropManager>.instance.GetRandomPropInfo(ref randomizer, ItemClass.Service.Garbage);
			if (randomPropInfo != null)
			{
				int width = data.Width;
				int length = data.Length;
				for (int i = 0; i < 8; i++)
				{
					PropInfo variation = randomPropInfo.GetVariation(ref randomizer);
					randomizer.Int32(10000u);
					randomizer.Int32(10000u);
					variation.GetColor(ref randomizer);
					Vector3 vector;
					vector.x = ((float)randomizer.Int32(10000u) * 0.0001f - 0.5f) * (float)width * 4f;
					vector.y = 0f;
					vector.z = (float)randomizer.Int32(10000u) * 0.0001f - 0.5f + (float)length * 4f;
					vector = instance.m_dataMatrix0.MultiplyPoint(vector);
					vector.y = Singleton<TerrainManager>.instance.SampleDetailHeight(vector);
					instance.m_extraData.SetUShort(64 + i, (ushort)Mathf.Clamp(Mathf.RoundToInt(vector.y * 64f), 0, 65535));
				}
			}
		}
	}
	public override void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort buildingID, ref Building data, int layerMask, ref RenderManager.Instance instance)
	{
		if ((data.m_flags & (Building.Flags.Completed | Building.Flags.BurnedDown)) != Building.Flags.Completed)
		{
			if ((data.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
			{
				BuildingManager instance2 = Singleton<BuildingManager>.instance;
				if (instance2.m_common != null)
				{
					BuildingInfoBase burned = instance2.m_common.m_burned;
					Vector3 vector = this.m_info.m_generatedInfo.m_min;
					Vector3 vector2 = this.m_info.m_generatedInfo.m_max;
					float num = (float)data.Width * 4f;
					float num2 = (float)data.Length * 4f;
					vector = Vector3.Max(vector, new Vector3(-num, 0f, -num2));
					vector2 = Vector3.Min(vector2, new Vector3(num, 0f, num2));
					Vector3 vector3 = vector2 - vector;
					float x = (vector3.x + 1f) / Mathf.Max(1f, burned.m_generatedInfo.m_size.x);
					float z = (vector3.z + 1f) / Mathf.Max(1f, burned.m_generatedInfo.m_size.z);
					instance.m_dataVector0.y = burned.m_generatedInfo.m_size.y;
					Matrix4x4 matrix = Matrix4x4.Scale(new Vector3(x, 1f, z));
					BuildingAI.RenderMesh(cameraInfo, this.m_info, burned, matrix, ref instance);
				}
			}
			else
			{
				uint num3 = (uint)(((int)buildingID << 8) / 49152);
				uint num4 = Singleton<SimulationManager>.instance.m_referenceFrameIndex - num3;
				float t = ((num4 & 255u) + Singleton<SimulationManager>.instance.m_referenceTimer) * 0.00390625f;
				Building.Frame frameData = data.GetFrameData(num4 - 512u);
				Building.Frame frameData2 = data.GetFrameData(num4 - 256u);
				float num5 = 0f;
				BuildingInfo buildingInfo;
				BuildingInfo buildingInfo2;
				if ((data.m_flags & Building.Flags.Upgrading) != Building.Flags.None)
				{
					BuildingInfo upgradeInfo = this.GetUpgradeInfo(buildingID, ref data);
					if (upgradeInfo != null)
					{
						buildingInfo = this.m_info;
						buildingInfo2 = upgradeInfo;
					}
					else
					{
						buildingInfo = null;
						buildingInfo2 = this.m_info;
					}
				}
				else
				{
					buildingInfo = null;
					buildingInfo2 = this.m_info;
				}
				float num6 = buildingInfo2.m_size.y;
				if (buildingInfo != null)
				{
					num6 = Mathf.Max(num6, buildingInfo.m_size.y);
				}
				float num7 = (float)this.GetConstructionTime();
				num7 /= Mathf.Max(1f, num7 - 6f);
				float num8 = Mathf.Max(0.5f, num6 / 60f);
				float num9 = Mathf.Ceil(num6 / num8 / 6f) * 6f;
				float num10 = (num9 * 2f + 6f) * num7 * Mathf.Lerp((float)frameData.m_constructState, (float)frameData2.m_constructState, t) * 0.003921569f;
				float num11 = (num10 - 6f) * num8;
				if (num11 >= buildingInfo2.m_size.y && instance.m_dataInt0 != buildingInfo2.m_prefabDataIndex)
				{
					BuildingAI.RefreshInstance(buildingInfo2, cameraInfo, buildingID, ref data, layerMask, ref instance);
				}
				float num12;
				if (num10 > num9)
				{
					num12 = Mathf.Min(num9, num9 * 2f + 6f - num10);
				}
				else
				{
					num12 = num10;
				}
				if (frameData2.m_productionState < frameData.m_productionState)
				{
					instance.m_dataVector3.w = Mathf.Lerp((float)frameData.m_productionState, (float)frameData2.m_productionState + 256f, t) * 0.00390625f;
					if (instance.m_dataVector3.w >= 1f)
					{
						instance.m_dataVector3.w = instance.m_dataVector3.w - 1f;
					}
				}
				else
				{
					instance.m_dataVector3.w = Mathf.Lerp((float)frameData.m_productionState, (float)frameData2.m_productionState, t) * 0.00390625f;
				}
				if (buildingInfo != null)
				{
					instance.m_position = Building.CalculateMeshPosition(buildingInfo, data.m_position, data.m_angle, data.Length);
					instance.m_rotation = Quaternion.AngleAxis(data.m_angle * 57.29578f, Vector3.down);
					instance.m_dataMatrix1.SetTRS(instance.m_position, instance.m_rotation, Vector3.one);
					instance.m_dataColor0 = buildingInfo.m_buildingAI.GetColor(buildingID, ref data, Singleton<InfoManager>.instance.CurrentMode);
					float num13 = num10 * num8;
					float num14;
					if (num13 > buildingInfo.m_size.y)
					{
						num14 = buildingInfo.m_size.y * 2f - num13;
					}
					else
					{
						num14 = buildingInfo.m_size.y;
					}
					if (num14 > 0f)
					{
						instance.m_dataVector0.y = -num14;
						instance.m_dataVector0.x = num12 * num8;
						buildingInfo.m_buildingAI.RenderMeshes(cameraInfo, buildingID, ref data, layerMask, ref instance);
						num5 = Mathf.Max(num5, instance.m_dataVector0.y);
						if (instance.m_dataVector0.y >= buildingInfo.m_size.y && instance.m_dataInt0 == buildingInfo.m_prefabDataIndex)
						{
							layerMask &= ~(1 << Singleton<TreeManager>.instance.m_treeLayer);
							buildingInfo.m_buildingAI.RenderProps(cameraInfo, buildingID, ref data, layerMask, ref instance);
						}
					}
				}
				float num15 = data.m_angle;
				int length = data.Length;
				int num16 = 0;
				if (buildingInfo != null && buildingInfo2 != null)
				{
					if (buildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft && buildingInfo2.m_zoningMode == BuildingInfo.ZoningMode.CornerRight)
					{
						num15 -= 1.57079637f;
						num16 = -1;
						length = data.Width;
					}
					else if (buildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerRight && buildingInfo2.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft)
					{
						num15 += 1.57079637f;
						num16 = 1;
						length = data.Width;
					}
				}
				instance.m_position = Building.CalculateMeshPosition(buildingInfo2, data.m_position, num15, length);
				instance.m_rotation = Quaternion.AngleAxis(num15 * 57.29578f, Vector3.down);
				instance.m_dataMatrix1.SetTRS(instance.m_position, instance.m_rotation, Vector3.one);
				instance.m_dataColor0 = buildingInfo2.m_buildingAI.GetColor(buildingID, ref data, Singleton<InfoManager>.instance.CurrentMode);
				if (num11 > 0f)
				{
					instance.m_dataVector0.y = -num11;
					instance.m_dataVector0.x = num12 * num8;
					buildingInfo2.m_buildingAI.RenderMeshes(cameraInfo, buildingID, ref data, layerMask, ref instance);
					num5 = Mathf.Max(num5, instance.m_dataVector0.y);
					if (num11 >= buildingInfo2.m_size.y && instance.m_dataInt0 == buildingInfo2.m_prefabDataIndex)
					{
						layerMask &= ~(1 << Singleton<TreeManager>.instance.m_treeLayer);
						buildingInfo2.m_buildingAI.RenderProps(cameraInfo, buildingID, ref data, layerMask, ref instance);
					}
				}
				BuildingManager instance3 = Singleton<BuildingManager>.instance;
				if (instance3.m_common != null)
				{
					BuildingInfoBase construction = instance3.m_common.m_construction;
					Vector3 vector4 = buildingInfo2.m_generatedInfo.m_max;
					Vector3 vector5 = buildingInfo2.m_generatedInfo.m_min;
					if (buildingInfo != null)
					{
						Vector3 zero = Vector3.zero;
						zero.z = -Building.CalculateLocalMeshOffset(buildingInfo2, length);
						if (num16 == -1)
						{
							zero.x -= Building.CalculateLocalMeshOffset(buildingInfo, data.Length);
							Vector3 max = buildingInfo.m_generatedInfo.m_max;
							Vector3 min = buildingInfo.m_generatedInfo.m_min;
							vector4 = Vector3.Max(vector4, new Vector3(max.z, max.y, -min.x) - zero);
							vector5 = Vector3.Min(vector5, new Vector3(min.z, min.y, -max.x) - zero);
						}
						else if (num16 == 1)
						{
							zero.x += Building.CalculateLocalMeshOffset(buildingInfo, data.Length);
							Vector3 max2 = buildingInfo.m_generatedInfo.m_max;
							Vector3 min2 = buildingInfo.m_generatedInfo.m_min;
							vector4 = Vector3.Max(vector4, new Vector3(max2.z, max2.y, max2.x) - zero);
							vector5 = Vector3.Min(vector5, new Vector3(min2.z, min2.y, min2.x) - zero);
						}
						else
						{
							zero.z += Building.CalculateLocalMeshOffset(buildingInfo, data.Length);
							vector4 = Vector3.Max(vector4, buildingInfo.m_generatedInfo.m_max - zero);
							vector5 = Vector3.Min(vector5, buildingInfo.m_generatedInfo.m_min - zero);
						}
					}
					Vector3 vector6 = vector4 - vector5;
					float x2 = (vector6.x + 1f) / Mathf.Max(1f, construction.m_generatedInfo.m_size.x);
					float z2 = (vector6.z + 1f) / Mathf.Max(1f, construction.m_generatedInfo.m_size.z);
					Vector3 pos = new Vector3((vector4.x + vector5.x) * 0.5f, 0f, (vector4.z + vector5.z) * 0.5f);
					Vector3 s = new Vector3(x2, num8, z2);
					Matrix4x4 matrix2 = Matrix4x4.TRS(pos, Quaternion.identity, s);
					if (num12 > 0f)
					{
						instance.m_dataVector0.y = num12;
						BuildingAI.RenderMesh(cameraInfo, buildingInfo2, construction, matrix2, ref instance);
						num5 = Mathf.Max(num5, instance.m_dataVector0.y);
					}
				}
				instance.m_dataVector0.y = num5;
			}
		}
		else
		{
			if (data.m_garbageBuffer >= 1000)
			{
				Randomizer randomizer = new Randomizer((int)buildingID);
				PropInfo randomPropInfo = Singleton<PropManager>.instance.GetRandomPropInfo(ref randomizer, ItemClass.Service.Garbage);
				if (randomPropInfo != null && (layerMask & 1 << randomPropInfo.m_prefabDataLayer) != 0)
				{
					int num17 = Mathf.Min(8, (int)(data.m_garbageBuffer / 1000));
					int width = data.Width;
					int length2 = data.Length;
					for (int i = 0; i < num17; i++)
					{
						PropInfo variation = randomPropInfo.GetVariation(ref randomizer);
						float scale = variation.m_minScale + (float)randomizer.Int32(10000u) * (variation.m_maxScale - variation.m_minScale) * 0.0001f;
						float angle = (float)randomizer.Int32(10000u) * 0.0006283185f;
						Color color = variation.GetColor(ref randomizer);
						Vector3 vector7;
						vector7.x = ((float)randomizer.Int32(10000u) * 0.0001f - 0.5f) * (float)width * 4f;
						vector7.y = 0f;
						vector7.z = (float)randomizer.Int32(10000u) * 0.0001f - 0.5f + (float)length2 * 4f;
						vector7 = instance.m_dataMatrix0.MultiplyPoint(vector7);
						vector7.y = (float)instance.m_extraData.GetUShort(64 + i) * 0.015625f;
						if (cameraInfo.CheckRenderDistance(vector7, variation.m_maxRenderDistance))
						{
							Vector4 objectIndex = new Vector4(0.001953125f, 0.00260416674f, 0f, 0f);
							PropInstance.RenderInstance(cameraInfo, variation, new InstanceID
								{
									Building = buildingID
								}, vector7, scale, angle, color, objectIndex, (data.m_flags & Building.Flags.Active) != Building.Flags.None);
						}
					}
				}
			}
			uint num18 = (uint)(((int)buildingID << 8) / 49152);
			uint num19 = Singleton<SimulationManager>.instance.m_referenceFrameIndex - num18;
			float t2 = ((num19 & 255u) + Singleton<SimulationManager>.instance.m_referenceTimer) * 0.00390625f;
			Building.Frame frameData3 = data.GetFrameData(num19 - 512u);
			Building.Frame frameData4 = data.GetFrameData(num19 - 256u);
			instance.m_dataVector0.x = Mathf.Lerp((float)frameData3.m_fireDamage, (float)frameData4.m_fireDamage, t2) * 0.003921569f;
			instance.m_dataVector0.z = (((data.m_flags & Building.Flags.Abandoned) == Building.Flags.None) ? 0f : 1f);
			if (frameData4.m_productionState < frameData3.m_productionState)
			{
				instance.m_dataVector3.w = Mathf.Lerp((float)frameData3.m_productionState, (float)frameData4.m_productionState + 256f, t2) * 0.00390625f;
				if (instance.m_dataVector3.w >= 1f)
				{
					instance.m_dataVector3.w = instance.m_dataVector3.w - 1f;
				}
			}
			else
			{
				instance.m_dataVector3.w = Mathf.Lerp((float)frameData3.m_productionState, (float)frameData4.m_productionState, t2) * 0.00390625f;
			}
			base.RenderInstance(cameraInfo, buildingID, ref data, layerMask, ref instance);
			if (data.m_fireIntensity != 0 && Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None)
			{
				this.RenderFireEffect(cameraInfo, buildingID, ref data, instance.m_dataVector0.x);
			}
		}
	}
	public override void PlayAudio(AudioManager.ListenerInfo listenerInfo, ushort buildingID, ref Building data)
	{
		if (data.m_fireIntensity != 0)
		{
			uint num = (uint)(((int)buildingID << 8) / 49152);
			uint num2 = Singleton<SimulationManager>.instance.m_referenceFrameIndex - num;
			float t = ((num2 & 255u) + Singleton<SimulationManager>.instance.m_referenceTimer) * 0.00390625f;
			Building.Frame frameData = data.GetFrameData(num2 - 512u);
			Building.Frame frameData2 = data.GetFrameData(num2 - 256u);
			float fireDamage = Mathf.Lerp((float)frameData.m_fireDamage, (float)frameData2.m_fireDamage, t) * 0.003921569f;
			this.PlayFireEffect(listenerInfo, buildingID, ref data, fireDamage);
		}
		else
		{
			base.PlayAudio(listenerInfo, buildingID, ref data);
		}
	}
	public void RenderFireEffect(RenderManager.CameraInfo cameraInfo, ushort buildingID, ref Building data, float fireDamage)
	{
		float num = (0.5f - Mathf.Abs(fireDamage - 0.5f)) * (float)data.m_fireIntensity * 0.007843138f;
		float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
		Vector3 pos;
		Quaternion q;
		data.CalculateMeshPosition(out pos, out q);
		Matrix4x4 matrix = Matrix4x4.TRS(pos, q, Vector3.one);
		InstanceID id = default(InstanceID);
		id.Building = buildingID;
		num /= 1f + this.m_info.m_generatedInfo.m_triangleArea * 0.0002f;
		EffectInfo fireEffect = Singleton<BuildingManager>.instance.m_properties.m_fireEffect;
		EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(matrix, this.m_info.m_lodMeshData);
		fireEffect.RenderEffect(id, area, Vector3.zero, 0f, num, simulationTimeDelta, cameraInfo);
	}
	public void PlayFireEffect(AudioManager.ListenerInfo listenerInfo, ushort buildingID, ref Building data, float fireDamage)
	{
		float magnitude = (0.5f - Mathf.Abs(fireDamage - 0.5f)) * (float)data.m_fireIntensity * 0.007843138f;
		Vector3 pos;
		Quaternion q;
		data.CalculateMeshPosition(out pos, out q);
		Matrix4x4 matrix = Matrix4x4.TRS(pos, q, Vector3.one);
		InstanceID id = default(InstanceID);
		id.Building = buildingID;
		EffectInfo fireEffect = Singleton<BuildingManager>.instance.m_properties.m_fireEffect;
		EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(matrix, this.m_info.m_lodMeshData);
		fireEffect.PlayEffect(id, area, Vector3.zero, 0f, magnitude, listenerInfo, Singleton<BuildingManager>.instance.m_audioGroup);
	}
	public override void InitializePrefab()
	{
		base.InitializePrefab();
		if (!this.m_ignoreNoPropsWarning && (this.m_info.m_props == null || this.m_info.m_props.Length == 0))
		{
			CODebugBase<LogChannel>.Warn(LogChannel.Core, "No props placed: " + base.gameObject.name, base.gameObject);
		}
	}
	public override void DestroyPrefab()
	{
		base.DestroyPrefab();
	}
	public void CreateBuildingC(ushort buildingID, ref Building data)
	{
		base.CreateBuilding(buildingID, ref data);
		if (this.GetConstructionTime() == 0)
		{
			data.m_frame0.m_constructState = 255;
			this.BuildingCompleted(buildingID, ref data);
		}
	}
	public void ReleaseBuildingC(ushort buildingID, ref Building data)
	{
		this.ManualDeactivation(buildingID, ref data);
		this.BuildingDeactivated(buildingID, ref data);
		base.ReleaseBuilding(buildingID, ref data);
	}
	public override void BeginRelocating(ushort buildingID, ref Building data)
	{
		base.BeginRelocating(buildingID, ref data);
		this.ManualDeactivation(buildingID, ref data);
	}
	public override void EndRelocating(ushort buildingID, ref Building data)
	{
		base.EndRelocating(buildingID, ref data);
		this.ManualActivation(buildingID, ref data);
	}
	public void SimulationStepC(ushort buildingID, ref Building data)
	{
		base.SimulationStep(buildingID, ref data);
		if ((data.m_flags & Building.Flags.Demolishing) != Building.Flags.None)
		{
			Singleton<BuildingManager>.instance.ReleaseBuilding(buildingID);
		}
	}
	public void SimulationStepC(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
	{
		base.SimulationStep(buildingID, ref buildingData, ref frameData);
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
					Singleton<BuildingManager>.instance.m_buildingLevelUp.Activate(properties4.m_buildingLevelUp, buildingID);
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
	}
	protected void EmptyBuilding(ushort buildingID, ref Building data)
	{
		CitizenManager instance = Singleton<CitizenManager>.instance;
		uint num = data.m_citizenUnits;
		int num2 = 0;
		while (num != 0u)
		{
			for (int i = 0; i < 5; i++)
			{
				uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
				if (citizen != 0u)
				{
					ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
					if (instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].GetBuildingByLocation() == buildingID || (instance2 != 0 && instance.m_instances.m_buffer[(int)instance2].m_targetBuilding == buildingID))
					{
						ushort num3 = 0;
						if (instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_homeBuilding == buildingID)
						{
							num3 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_workBuilding;
						}
						else if (instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_workBuilding == buildingID)
						{
							num3 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_homeBuilding;
						}
						else if (instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_visitBuilding == buildingID)
						{
							if (instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].Arrested)
							{
								instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].Arrested = false;
								if (instance2 != 0)
								{
									instance.ReleaseCitizenInstance(instance2);
								}
							}
							instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].SetVisitplace(citizen, 0, 0u);
							num3 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_homeBuilding;
						}
						if (num3 != 0)
						{
							CitizenInfo citizenInfo = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].GetCitizenInfo(citizen);
							HumanAI humanAI = citizenInfo.m_citizenAI as HumanAI;
							if (humanAI != null)
							{
								humanAI.StartMoving(citizen, ref instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)], buildingID, num3);
							}
						}
					}
				}
			}
			num = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
			if (++num2 > 524288)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
	}
	protected virtual int GetConstructionTimeC()
	{
		return 0;
	}
	protected virtual void BuildingCompleted(ushort buildingID, ref Building buildingData)
	{
		Building.Flags flags = buildingData.m_flags;
		if ((flags & Building.Flags.Upgrading) != Building.Flags.None)
		{
			flags &= ~Building.Flags.Upgrading;
			flags |= Building.Flags.Completed;
			buildingData.m_flags = flags;
			BuildingInfo upgradeInfo = this.GetUpgradeInfo(buildingID, ref buildingData);
			if (upgradeInfo != null)
			{
				Singleton<BuildingManager>.instance.UpdateBuildingInfo(buildingID, upgradeInfo);
				upgradeInfo.m_buildingAI.BuildingUpgraded(buildingID, ref buildingData);
				this.ManualActivation(buildingID, ref buildingData);
				return;
			}
		}
		flags |= Building.Flags.Completed;
		buildingData.m_flags = flags;
		if (this.GetConstructionTime() != 0)
		{
			Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
		}
		this.ManualActivation(buildingID, ref buildingData);
	}
	public override bool CanUseGroupMesh(ushort buildingID, ref Building buildingData)
	{
		Building.Frame lastFrameData = buildingData.GetLastFrameData();
		return base.CanUseGroupMesh(buildingID, ref buildingData) && (buildingData.m_flags & Building.Flags.Completed) != Building.Flags.None && (lastFrameData.m_fireDamage == 0 || lastFrameData.m_fireDamage == 255) && buildingData.m_fireIntensity == 0;
	}
	public override Color32 GetBuildingGroupState(ushort buildingID, ref Building buildingData)
	{
		Color32 buildingGroupState = base.GetBuildingGroupState(buildingID, ref buildingData);
		Building.Frame lastFrameData = buildingData.GetLastFrameData();
		if ((buildingData.m_flags & Building.Flags.BurnedDown) == Building.Flags.None)
		{
			buildingGroupState.r = lastFrameData.m_fireDamage;
			buildingGroupState.b = ((byte)(((buildingData.m_flags & Building.Flags.Abandoned) == Building.Flags.None) ? 0 : 255));
		}
		return buildingGroupState;
	}
	public override ToolBase.ToolErrors CheckBulldozing(ushort buildingID, ref Building data)
	{
		if (data.m_fireIntensity != 0)
		{
			return ToolBase.ToolErrors.BuildingInFire;
		}
		return base.CheckBulldozing(buildingID, ref data);
	}
	public override ToolBase.ToolErrors CheckBuildPosition(ushort relocateID, ref Vector3 position, ref float angle, float waterHeight, float elevation, ref Segment3 connectionSegment, out int productionRate, out int constructionCost)
	{
		ToolBase.ToolErrors toolErrors = base.CheckBuildPosition(relocateID, ref position, ref angle, waterHeight, elevation, ref connectionSegment, out productionRate, out constructionCost);
		if (relocateID != 0)
		{
			BuildingManager instance = Singleton<BuildingManager>.instance;
			if (instance.m_buildings.m_buffer[(int)relocateID].m_fireIntensity != 0)
			{
				toolErrors |= ToolBase.ToolErrors.BuildingInFire;
			}
			else if ((instance.m_buildings.m_buffer[(int)relocateID].m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
			{
				toolErrors |= ToolBase.ToolErrors.BurnedDown;
			}
		}
		return toolErrors;
	}
	public override void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
	{
		base.StartTransfer(buildingID, ref data, material, offer);
	}
	public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
	{
		if (material == TransferManager.TransferReason.Garbage)
		{
			int garbageBuffer = (int)data.m_garbageBuffer;
			amountDelta = Mathf.Clamp(amountDelta, -garbageBuffer, 65535 - garbageBuffer);
			data.m_garbageBuffer = (ushort)(garbageBuffer + amountDelta);
		}
		else if (material == TransferManager.TransferReason.Crime)
		{
			int crimeBuffer = (int)data.m_crimeBuffer;
			amountDelta = Mathf.Clamp(amountDelta, -crimeBuffer, 65535 - crimeBuffer);
			data.m_crimeBuffer = (ushort)(crimeBuffer + amountDelta);
		}
		else
		{
			base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
		}
	}
	public override void GetMaterialAmount(ushort buildingID, ref Building data, TransferManager.TransferReason material, out int amount, out int max)
	{
		if (material == TransferManager.TransferReason.Garbage)
		{
			amount = (int)data.m_garbageBuffer;
			max = 65535;
		}
		else if (material == TransferManager.TransferReason.Crime)
		{
			amount = (int)data.m_crimeBuffer;
			max = 65535;
		}
		else
		{
			base.GetMaterialAmount(buildingID, ref data, material, out amount, out max);
		}
	}
	public override float ElectricityGridRadius()
	{
		return 32f;
	}
	public override void BuildingDeactivated(ushort buildingID, ref Building data)
	{
		TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
		offer.Building = buildingID;
		Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Garbage, offer);
		Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Crime, offer);
		Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Sick, offer);
		Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Dead, offer);
		Singleton<TransferManager>.instance.RemoveOutgoingOffer(TransferManager.TransferReason.Fire, offer);
		Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Worker0, offer);
		Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Worker1, offer);
		Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Worker2, offer);
		Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Worker3, offer);
		data.m_flags &= ~Building.Flags.Active;
		this.EmptyBuilding(buildingID, ref data);
		base.BuildingDeactivated(buildingID, ref data);
	}
	protected virtual void SimulationStepActiveC(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
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
	protected int HandleCommonConsumption(ushort buildingID, ref Building data, ref int electricityConsumption, ref int waterConsumption, ref int sewageAccumulation, ref int garbageAccumulation, DistrictPolicies.Services policies)
	{
		int num = 100;
		Notification.Problem problem = Notification.RemoveProblems(data.m_problems, Notification.Problem.Electricity | Notification.Problem.Water | Notification.Problem.Sewage | Notification.Problem.Flood);
		bool flag = data.m_electricityProblemTimer != 0;
		bool flag2 = false;
		if (electricityConsumption != 0)
		{
			if ((policies & DistrictPolicies.Services.PowerSaving) != DistrictPolicies.Services.None)
			{
				electricityConsumption = Mathf.Max(1, electricityConsumption * 90 / 100);
				Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 32, this.m_info.m_class);
			}
			int num2 = electricityConsumption * 2 - (int)data.m_electricityBuffer;
			if (num2 > 0)
			{
				int num3 = Singleton<ElectricityManager>.instance.TryFetchElectricity(data.m_position, electricityConsumption, num2);
				data.m_electricityBuffer += (ushort)num3;
			}
			if ((int)data.m_electricityBuffer < electricityConsumption)
			{
				flag2 = true;
				data.m_electricityProblemTimer = (byte)Mathf.Min(255, (int)(data.m_electricityProblemTimer + 1));
				if (data.m_electricityProblemTimer >= 65)
				{
					num = 0;
					problem = Notification.AddProblems(problem, Notification.Problem.Electricity | Notification.Problem.MajorProblem);
				}
				else if (data.m_electricityProblemTimer >= 1)
				{
					num /= 2;
					problem = Notification.AddProblems(problem, Notification.Problem.Electricity);
				}
				data.m_electricityBuffer = 0;
				if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Electricity))
				{
					GuideController properties = Singleton<GuideManager>.instance.m_properties;
					if (properties != null)
					{
						int publicServiceIndex = ItemClass.GetPublicServiceIndex(ItemClass.Service.Electricity);
						DistrictManager instance = Singleton<DistrictManager>.instance;
						int electricityCapacity = instance.m_districts.m_buffer[0].GetElectricityCapacity();
						int electricityConsumption2 = instance.m_districts.m_buffer[0].GetElectricityConsumption();
						if (electricityCapacity >= electricityConsumption2)
						{
							Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Activate(properties.m_serviceNeeded2, ItemClass.Service.Electricity);
						}
						else
						{
							Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Activate(properties.m_serviceNeeded, ItemClass.Service.Electricity);
						}
					}
				}
			}
			else
			{
				data.m_electricityBuffer -= (ushort)electricityConsumption;
			}
		}
		if (!flag2)
		{
			data.m_electricityProblemTimer = 0;
		}
		if (flag != flag2)
		{
			Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
		}
		bool flag3 = false;
		int num4 = sewageAccumulation;
		if (waterConsumption != 0)
		{
			if ((policies & DistrictPolicies.Services.WaterSaving) != DistrictPolicies.Services.None)
			{
				waterConsumption = Mathf.Max(1, waterConsumption * 85 / 100);
				if (sewageAccumulation != 0)
				{
					sewageAccumulation = Mathf.Max(1, sewageAccumulation * 85 / 100);
				}
				Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 32, this.m_info.m_class);
			}
			int num5 = waterConsumption * 2 - (int)data.m_waterBuffer;
			if (num5 > 0)
			{
				int num6 = Singleton<WaterManager>.instance.TryFetchWater(data.m_position, waterConsumption, num5, ref data.m_waterPollution);
				data.m_waterBuffer += (ushort)num6;
			}
			if ((int)data.m_waterBuffer < waterConsumption)
			{
				flag3 = true;
				data.m_waterProblemTimer = (byte)Mathf.Min(255, (int)(data.m_waterProblemTimer + 1));
				if (data.m_waterProblemTimer >= 65)
				{
					num = 0;
					problem = Notification.AddProblems(problem, Notification.Problem.Water | Notification.Problem.MajorProblem);
				}
				else if (data.m_waterProblemTimer >= 1)
				{
					num /= 2;
					problem = Notification.AddProblems(problem, Notification.Problem.Water);
				}
				num4 = sewageAccumulation * (waterConsumption + (int)data.m_waterBuffer) / (waterConsumption << 1);
				data.m_waterBuffer = 0;
				if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Water))
				{
					GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
					if (properties2 != null)
					{
						int publicServiceIndex2 = ItemClass.GetPublicServiceIndex(ItemClass.Service.Water);
						DistrictManager instance2 = Singleton<DistrictManager>.instance;
						int waterCapacity = instance2.m_districts.m_buffer[0].GetWaterCapacity();
						int waterConsumption2 = instance2.m_districts.m_buffer[0].GetWaterConsumption();
						if (waterCapacity >= waterConsumption2)
						{
							Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex2].Activate(properties2.m_serviceNeeded2, ItemClass.Service.Water);
						}
						else
						{
							Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex2].Activate(properties2.m_serviceNeeded, ItemClass.Service.Water);
						}
					}
				}
			}
			else
			{
				num4 = sewageAccumulation;
				data.m_waterBuffer -= (ushort)waterConsumption;
			}
		}
		if (num4 != 0)
		{
			int num7 = num4 * 2 - (int)data.m_sewageBuffer;
			if (num7 < num4)
			{
				if (!flag3 && (data.m_problems & Notification.Problem.Water) == Notification.Problem.None)
				{
					flag3 = true;
					data.m_waterProblemTimer = (byte)Mathf.Min(255, (int)(data.m_waterProblemTimer + 1));
					if (data.m_waterProblemTimer >= 65)
					{
						num = 0;
						problem = Notification.AddProblems(problem, Notification.Problem.Sewage | Notification.Problem.MajorProblem);
					}
					else if (data.m_waterProblemTimer >= 1)
					{
						num /= 2;
						problem = Notification.AddProblems(problem, Notification.Problem.Sewage);
					}
				}
				data.m_sewageBuffer = (ushort)(num4 * 2);
			}
			else
			{
				data.m_sewageBuffer += (ushort)num4;
			}
		}
		if (!flag3)
		{
			data.m_waterProblemTimer = 0;
		}
		if (garbageAccumulation != 0)
		{
			int num8 = (int)(65535 - data.m_garbageBuffer);
			if (num8 < garbageAccumulation)
			{
				num = 0;
				data.m_garbageBuffer = (ushort)num8;
			}
			else
			{
				data.m_garbageBuffer += (ushort)garbageAccumulation;
			}
		}
		if (num4 != 0)
		{
			int num9 = Mathf.Min(num4 * 2, (int)data.m_sewageBuffer);
			if (num9 > 0)
			{
				int num10 = Singleton<WaterManager>.instance.TryDumpSewage(data.m_position, num4 * 2, num9);
				data.m_sewageBuffer -= (ushort)num10;
			}
		}
		if (garbageAccumulation != 0)
		{
			int num11 = (int)data.m_garbageBuffer;
			if (num11 >= 200 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0 && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Garbage))
			{
				int num12 = 0;
				int num13 = 0;
				int num14 = 0;
				int num15 = 0;
				this.CalculateGuestVehicles(buildingID, ref data, TransferManager.TransferReason.Garbage, ref num12, ref num13, ref num14, ref num15);
				num11 -= num14 - num13;
				if (num11 >= 200)
				{
					TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
					offer.Priority = num11 / 1000;
					offer.Building = buildingID;
					offer.Position = data.m_position;
					offer.Amount = 1;
					Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Garbage, offer);
				}
			}
		}
		if (this.CanSufferFromFlood())
		{
			float num16 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(data.m_position));
			if (num16 > data.m_position.y)
			{
				if (num16 > data.m_position.y + 1f)
				{
					num = 0;
					problem = Notification.AddProblems(problem, Notification.Problem.Flood | Notification.Problem.MajorProblem);
				}
				else
				{
					num /= 2;
					problem = Notification.AddProblems(problem, Notification.Problem.Flood);
				}
			}
		}
		data.m_problems = problem;
		return num;
	}
	protected virtual bool CanSufferFromFlood()
	{
		return true;
	}
	protected void CalculateOwnVehicles(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
	{
		VehicleManager instance = Singleton<VehicleManager>.instance;
		ushort num = data.m_ownVehicles;
		int num2 = 0;
		while (num != 0)
		{
			if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[(int)num].m_transferType == material)
			{
				VehicleInfo info = instance.m_vehicles.m_buffer[(int)num].Info;
				int a;
				int num3;
				info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[(int)num], out a, out num3);
				cargo += Mathf.Min(a, num3);
				capacity += num3;
				count++;
				if ((instance.m_vehicles.m_buffer[(int)num].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != Vehicle.Flags.None)
				{
					outside++;
				}
			}
			num = instance.m_vehicles.m_buffer[(int)num].m_nextOwnVehicle;
			if (++num2 > 16384)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
	}
	protected void CalculateGuestVehicles(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
	{
		VehicleManager instance = Singleton<VehicleManager>.instance;
		ushort num = data.m_guestVehicles;
		int num2 = 0;
		while (num != 0)
		{
			if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[(int)num].m_transferType == material)
			{
				VehicleInfo info = instance.m_vehicles.m_buffer[(int)num].Info;
				int a;
				int num3;
				info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[(int)num], out a, out num3);
				cargo += Mathf.Min(a, num3);
				capacity += num3;
				count++;
				if ((instance.m_vehicles.m_buffer[(int)num].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != Vehicle.Flags.None)
				{
					outside++;
				}
			}
			num = instance.m_vehicles.m_buffer[(int)num].m_nextGuestVehicle;
			if (++num2 > 16384)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
	}
	protected void HandleDead(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
	{
		Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Death);
		if (behaviour.m_deadCount != 0 && Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.DeathCare))
		{
			buildingData.m_deathProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_deathProblemTimer + 1));
			if (buildingData.m_deathProblemTimer >= 128)
			{
				problem = Notification.AddProblems(problem, Notification.Problem.Death | Notification.Problem.MajorProblem);
			}
			else if (buildingData.m_deathProblemTimer >= 64)
			{
				problem = Notification.AddProblems(problem, Notification.Problem.Death);
			}
			int num = behaviour.m_deadCount;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			this.CalculateGuestVehicles(buildingID, ref buildingData, TransferManager.TransferReason.Dead, ref num2, ref num3, ref num4, ref num5);
			num -= num4;
			if (num > 0)
			{
				TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
				offer.Priority = (int)(buildingData.m_deathProblemTimer * 7 / 128);
				offer.Building = buildingID;
				offer.Position = buildingData.m_position;
				offer.Amount = 1;
				offer.Active = false;
				Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Dead, offer);
			}
		}
		else
		{
			buildingData.m_deathProblemTimer = 0;
		}
		buildingData.m_problems = problem;
	}
	protected void HandleSick(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
	{
		Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.DirtyWater | Notification.Problem.Pollution | Notification.Problem.Noise);
		if (behaviour.m_sickCount != 0 && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
		{
			byte b;
			Singleton<NaturalResourceManager>.instance.CheckPollution(buildingData.m_position, out b);
			int num;
			Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.NoisePollution, buildingData.m_position, out num);
			int num2 = (int)(buildingData.m_waterPollution * 2);
			int num3 = (int)(b * 100 / 255);
			int num4 = num * 100 / 255;
			int num5;
			if (num2 < 35)
			{
				num5 = num2;
			}
			else
			{
				num5 = num2 * 2 - 35;
			}
			if (num5 > 10 && num5 > num3 && num5 > num4)
			{
				buildingData.m_healthProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_healthProblemTimer + 1));
				if (buildingData.m_healthProblemTimer >= 96)
				{
					problem = Notification.AddProblems(problem, Notification.Problem.DirtyWater | Notification.Problem.MajorProblem);
				}
				else if (buildingData.m_healthProblemTimer >= 32)
				{
					problem = Notification.AddProblems(problem, Notification.Problem.DirtyWater);
				}
			}
			else if (num3 > 10 && num3 > num4)
			{
				buildingData.m_healthProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_healthProblemTimer + 1));
				if (buildingData.m_healthProblemTimer >= 96)
				{
					problem = Notification.AddProblems(problem, Notification.Problem.Pollution | Notification.Problem.MajorProblem);
				}
				else if (buildingData.m_healthProblemTimer >= 32)
				{
					problem = Notification.AddProblems(problem, Notification.Problem.Pollution);
				}
			}
			else if (num4 > 10)
			{
				buildingData.m_healthProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_healthProblemTimer + 1));
				if (buildingData.m_healthProblemTimer >= 96)
				{
					problem = Notification.AddProblems(problem, Notification.Problem.Noise | Notification.Problem.MajorProblem);
				}
				else if (buildingData.m_healthProblemTimer >= 32)
				{
					problem = Notification.AddProblems(problem, Notification.Problem.Noise);
				}
			}
			else
			{
				buildingData.m_healthProblemTimer = 0;
			}
		}
		else
		{
			buildingData.m_healthProblemTimer = 0;
		}
		buildingData.m_problems = problem;
	}
	protected void HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
	{
		if (crimeAccumulation != 0)
		{
			if (Singleton<SimulationManager>.instance.m_isNightTime)
			{
				crimeAccumulation = crimeAccumulation * 5 >> 2;
			}
			crimeAccumulation = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)crimeAccumulation);
			if (!Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment))
			{
				crimeAccumulation = 0;
			}
		}
		data.m_crimeBuffer = (ushort)Mathf.Min(citizenCount * 100, (int)data.m_crimeBuffer + crimeAccumulation);
		int crimeBuffer = (int)data.m_crimeBuffer;
		if (citizenCount != 0 && crimeBuffer > citizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			this.CalculateGuestVehicles(buildingID, ref data, TransferManager.TransferReason.Crime, ref num, ref num2, ref num3, ref num4);
			if (num == 0)
			{
				TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
				offer.Priority = crimeBuffer / Mathf.Max(1, citizenCount * 10);
				offer.Building = buildingID;
				offer.Position = data.m_position;
				offer.Amount = 1;
				Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Crime, offer);
			}
		}
		Notification.Problem problem = Notification.RemoveProblems(data.m_problems, Notification.Problem.Crime);
		if ((int)data.m_crimeBuffer > citizenCount * 90)
		{
			problem = Notification.AddProblems(problem, Notification.Problem.Crime | Notification.Problem.MajorProblem);
		}
		else if ((int)data.m_crimeBuffer > citizenCount * 60)
		{
			problem = Notification.AddProblems(problem, Notification.Problem.Crime);
		}
		data.m_problems = problem;
	}
	protected void RemovePeople(ushort buildingID, ref Building data)
	{
		if (data.m_citizenUnits != 0u)
		{
			Singleton<CitizenManager>.instance.ReleaseUnits(data.m_citizenUnits);
			data.m_citizenUnits = 0u;
		}
	}
	protected void HandleFire(ushort buildingID, ref Building data, ref Building.Frame frameData, DistrictPolicies.Services policies)
	{
		int num;
		int num2;
		int num3;
		this.GetFireParameters(buildingID, ref data, out num, out num2, out num3);
		if (num != 0 && data.m_fireIntensity == 0 && frameData.m_fireDamage == 0)
		{
			if ((policies & DistrictPolicies.Services.SmokeDetectors) != DistrictPolicies.Services.None)
			{
				num = num * 75 / 100;
				Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 32, this.m_info.m_class);
			}
			if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2000000u) < num && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.FireDepartment) && !Singleton<BuildingManager>.instance.m_firesDisabled)
			{
				float num4 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(data.m_position));
				if (num4 <= data.m_position.y)
				{
					data.m_fireIntensity = (byte)num2;
					this.BuildingDeactivated(buildingID, ref data);
					Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
					Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
				}
			}
		}
		if (data.m_fireIntensity != 0)
		{
			int num5;
			if (num3 != 0)
			{
				if (data.m_fireIntensity > 127)
				{
					if (frameData.m_fireDamage > 30)
					{
						num5 = (200 + num3) / num3;
					}
					else
					{
						num5 = (100 + num3) / num3;
					}
				}
				else if (frameData.m_fireDamage > 50)
				{
					num5 = (100 + num3) / num3;
				}
				else
				{
					num5 = (50 + num3) / num3;
				}
			}
			else
			{
				num5 = 255;
			}
			if (num5 != 0)
			{
				num5 = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, num5);
				frameData.m_fireDamage = (byte)Mathf.Min((int)frameData.m_fireDamage + num5, 255);
				if (frameData.m_fireDamage > 20)
				{
					this.HandleFireSpread(buildingID, ref data, num5);
				}
				if (frameData.m_fireDamage == 255)
				{
					data.SetFrameData(Singleton<SimulationManager>.instance.m_currentFrameIndex, frameData);
					data.m_fireIntensity = 0;
					data.m_garbageBuffer = 0;
					data.m_flags |= Building.Flags.BurnedDown;
					data.m_problems = (Notification.Problem.Fire | Notification.Problem.FatalProblem);
					this.RemovePeople(buildingID, ref data);
					this.BuildingDeactivated(buildingID, ref data);
					if (this.m_info.m_hasParkingSpaces)
					{
						Singleton<BuildingManager>.instance.UpdateParkingSpaces(buildingID, ref data);
					}
					Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
					Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
					GuideController properties = Singleton<GuideManager>.instance.m_properties;
					if (properties != null)
					{
						Singleton<BuildingManager>.instance.m_buildingOnFire.Deactivate(buildingID, false);
					}
				}
				else
				{
					float num6 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(data.m_position));
					if (num6 > data.m_position.y + 1f)
					{
						data.m_fireIntensity = 0;
						Building.Flags flags = data.m_flags;
						if (data.m_productionRate != 0)
						{
							data.m_flags |= Building.Flags.Active;
						}
						Building.Flags flags2 = data.m_flags;
						Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, frameData.m_fireDamage == 0);
						Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
						if (flags2 != flags)
						{
							Singleton<BuildingManager>.instance.UpdateFlags(buildingID, flags2 ^ flags);
						}
						GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
						if (properties2 != null)
						{
							Singleton<BuildingManager>.instance.m_buildingOnFire.Deactivate(buildingID, false);
						}
					}
					else
					{
						num2 = Mathf.Min(5000, (int)data.m_fireIntensity * data.Width * data.Length);
						int num7 = 0;
						int num8 = 0;
						int num9 = 0;
						int num10 = 0;
						this.CalculateGuestVehicles(buildingID, ref data, TransferManager.TransferReason.Fire, ref num7, ref num8, ref num9, ref num10);
						if (num9 * 15 < num2)
						{
							TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
							offer.Priority = Mathf.Max(8 - num7 - 1, 4);
							offer.Building = buildingID;
							offer.Position = data.m_position;
							offer.Amount = 1;
							Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Fire, offer);
						}
					}
				}
			}
			if (data.m_fireIntensity != 0)
			{
				if (frameData.m_fireDamage >= 128)
				{
					data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem.Fire | Notification.Problem.MajorProblem);
				}
				else
				{
					data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem.Fire);
				}
				Vector3 position = data.CalculateSidewalkPosition();
				PathUnit.Position position2;
				if (PathManager.FindPathPosition(position, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, false, false, 32f, out position2))
				{
					Singleton<NetManager>.instance.m_segments.m_buffer[(int)position2.m_segment].AddTraffic(65535);
				}
			}
		}
		else
		{
			if (frameData.m_fireDamage != 0 && (data.m_flags & Building.Flags.BurnedDown) == Building.Flags.None)
			{
				frameData.m_fireDamage = (byte)Mathf.Max((int)(frameData.m_fireDamage - 16), 0);
				if (frameData.m_fireDamage == 0)
				{
					data.SetFrameData(Singleton<SimulationManager>.instance.m_currentFrameIndex, frameData);
					Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
				}
			}
			data.m_problems = Notification.RemoveProblems(data.m_problems, Notification.Problem.Fire);
		}
	}
	private void HandleFireSpread(ushort buildingID, ref Building buildingData, int damageAccumulation)
	{
		int width = buildingData.Width;
		int length = buildingData.Length;
		Vector2 a = VectorUtils.XZ(buildingData.m_position);
		Vector2 a2 = new Vector2(Mathf.Cos(buildingData.m_angle), Mathf.Sin(buildingData.m_angle)) * 8f;
		Vector2 a3 = new Vector2(a2.y, -a2.x);
		Quad2 quad;
		quad.a = a - ((float)width * 0.5f + 1.5f) * a2 - ((float)length * 0.5f + 1.5f) * a3;
		quad.b = a + ((float)width * 0.5f + 1.5f) * a2 - ((float)length * 0.5f + 1.5f) * a3;
		quad.c = a + ((float)width * 0.5f + 1.5f) * a2 + ((float)length * 0.5f + 1.5f) * a3;
		quad.d = a - ((float)width * 0.5f + 1.5f) * a2 + ((float)length * 0.5f + 1.5f) * a3;
		Vector2 vector = quad.Min();
		Vector2 vector2 = quad.Max();
		vector.y -= (float)buildingData.m_baseHeight;
		vector2.y += this.m_info.m_size.y;
		int num = Mathf.Max((int)((vector.x - 72f) / 64f + 135f), 0);
		int num2 = Mathf.Max((int)((vector.y - 72f) / 64f + 135f), 0);
		int num3 = Mathf.Min((int)((vector2.x + 72f) / 64f + 135f), 269);
		int num4 = Mathf.Min((int)((vector2.y + 72f) / 64f + 135f), 269);
		BuildingManager instance = Singleton<BuildingManager>.instance;
		for (int i = num2; i <= num4; i++)
		{
			for (int j = num; j <= num3; j++)
			{
				ushort num5 = instance.m_buildingGrid[i * 270 + j];
				int num6 = 0;
				while (num5 != 0)
				{
					if (num5 != buildingID && Singleton<SimulationManager>.instance.m_randomizer.Int32(1000u) < damageAccumulation)
					{
						//CommonBuildingAI.TrySpreadFire(quad, vector.y, vector2.y, num5, ref instance.m_buildings.m_buffer[(int)num5]);
					}
					num5 = instance.m_buildings.m_buffer[(int)num5].m_nextGridBuilding;
					if (++num6 >= 49152)
					{
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
						break;
					}
				}
			}
		}
	}
	private static void TrySpreadFire(Quad2 quad, float minY, float maxY, ushort buildingID, ref Building buildingData)
	{
		BuildingInfo info = buildingData.Info;
		int num;
		int num2;
		int num3;
		info.m_buildingAI.GetFireParameters(buildingID, ref buildingData, out num, out num2, out num3);
		if (num != 0)
		{
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte district = instance.GetDistrict(buildingData.m_position);
			DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
			if ((servicePolicies & DistrictPolicies.Services.SmokeDetectors) != DistrictPolicies.Services.None)
			{
				num = num * 75 / 100;
			}
		}
		if (num != 0 && (buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Abandoned)) == Building.Flags.Completed && buildingData.m_fireIntensity == 0 && buildingData.GetLastFrameData().m_fireDamage == 0 && buildingData.OverlapQuad(buildingID, quad, minY, maxY))
		{
			float num4 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(buildingData.m_position));
			if (num4 <= buildingData.m_position.y)
			{
				Building.Flags flags = buildingData.m_flags;
				buildingData.m_fireIntensity = (byte)num2;
				info.m_buildingAI.BuildingDeactivated(buildingID, ref buildingData);
				Building.Flags flags2 = buildingData.m_flags;
				Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
				Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
				if (flags2 != flags)
				{
					Singleton<BuildingManager>.instance.UpdateFlags(buildingID, flags2 ^ flags);
				}
			}
		}
	}
	protected void GetHomeBehaviour(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount, ref int homeCount, ref int aliveHomeCount, ref int emptyHomeCount)
	{
		CitizenManager instance = Singleton<CitizenManager>.instance;
		uint num = buildingData.m_citizenUnits;
		int num2 = 0;
		while (num != 0u)
		{
			if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Home) != 0)
			{
				int num3 = 0;
				int num4 = 0;
				instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizenHomeBehaviour(ref behaviour, ref num3, ref num4);
				if (num3 != 0)
				{
					aliveHomeCount++;
					aliveCount += num3;
				}
				if (num4 != 0)
				{
					totalCount += num4;
				}
				else
				{
					emptyHomeCount++;
				}
				homeCount++;
			}
			num = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
			if (++num2 > 524288)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
	}
	protected void GetWorkBehaviour(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount)
	{
		CitizenManager instance = Singleton<CitizenManager>.instance;
		uint num = buildingData.m_citizenUnits;
		int num2 = 0;
		while (num != 0u)
		{
			if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Work) != 0)
			{
				instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizenWorkBehaviour(ref behaviour, ref aliveCount, ref totalCount);
			}
			num = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
			if (++num2 > 524288)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
	}
	protected void GetStudentBehaviour(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount)
	{
		CitizenManager instance = Singleton<CitizenManager>.instance;
		uint num = buildingData.m_citizenUnits;
		int num2 = 0;
		while (num != 0u)
		{
			if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Student) != 0)
			{
				instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizenStudentBehaviour(ref behaviour, ref aliveCount, ref totalCount);
			}
			num = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
			if (++num2 > 524288)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
	}
	protected void GetVisitBehaviour(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount)
	{
		CitizenManager instance = Singleton<CitizenManager>.instance;
		uint num = buildingData.m_citizenUnits;
		int num2 = 0;
		while (num != 0u)
		{
			if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Visit) != 0)
			{
				instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizenVisitBehaviour(ref behaviour, ref aliveCount, ref totalCount);
			}
			num = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
			if (++num2 > 524288)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
	}
	protected void HandleWorkPlaces(ushort buildingID, ref Building data, int workPlaces0, int workPlaces1, int workPlaces2, int workPlaces3, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount)
	{
		int num = workPlaces0 + workPlaces1 + workPlaces2 + workPlaces3;
		if (totalWorkerCount < num && data.m_citizenUnits != 0u)
		{
			int num2 = behaviour.m_educated0Count;
			int num3 = behaviour.m_educated1Count;
			int num4 = behaviour.m_educated2Count;
			int educated3Count = behaviour.m_educated3Count;
			int num5 = Singleton<SimulationManager>.instance.m_randomizer.Int32(5u);
			if (num5 == 0)
			{
				int num6 = num - totalWorkerCount;
				int num7 = (workPlaces3 * 300 + workPlaces2 * 200 + workPlaces1 * 100) / (num + 1);
				int num8 = (educated3Count * 300 + num4 * 200 + num3 * 100) / (aliveWorkerCount + 1);
				if (educated3Count < workPlaces3 && num6 > 0)
				{
					TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
					offer.Priority = Mathf.Max(1, (workPlaces3 - educated3Count) * 8 / workPlaces3);
					offer.Building = buildingID;
					offer.Position = data.m_position;
					int num9 = Mathf.Min(num6, workPlaces3 - educated3Count);
					offer.Amount = num9;
					num6 -= num9;
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker3, offer);
				}
				else if (educated3Count > workPlaces3)
				{
					num4 += educated3Count - workPlaces3;
				}
				if (num4 < workPlaces2 && num6 > 0)
				{
					TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
					offer2.Priority = Mathf.Max(1, (workPlaces2 - num4) * 8 / workPlaces2);
					offer2.Building = buildingID;
					offer2.Position = data.m_position;
					int num10 = Mathf.Min(num6, workPlaces2 - num4);
					offer2.Amount = num10;
					num6 -= num10;
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker2, offer2);
				}
				else if (num4 > workPlaces2)
				{
					num3 += num4 - workPlaces2;
				}
				if (num3 < workPlaces1 && num6 > 0)
				{
					TransferManager.TransferOffer offer3 = default(TransferManager.TransferOffer);
					offer3.Priority = Mathf.Max(1, (workPlaces1 - num3) * 8 / workPlaces1);
					offer3.Building = buildingID;
					offer3.Position = data.m_position;
					int num11 = Mathf.Min(num6, workPlaces1 - num3);
					offer3.Amount = num11;
					num6 -= num11;
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker1, offer3);
				}
				else if (num3 > workPlaces1)
				{
					num2 += num3 - workPlaces1;
				}
				if (num2 < workPlaces0 && num6 > 0)
				{
					TransferManager.TransferOffer offer4 = default(TransferManager.TransferOffer);
					offer4.Priority = Mathf.Max(1, (workPlaces0 - num2) * 8 / workPlaces0);
					offer4.Building = buildingID;
					offer4.Position = data.m_position;
					int num12 = Mathf.Min(num6, workPlaces0 - num2);
					offer4.Amount = num12;
					num6 -= num12;
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker0, offer4);
				}
				if (num8 < num7 - 100 && Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Education))
				{
					GuideController properties = Singleton<GuideManager>.instance.m_properties;
					if (properties != null)
					{
						int publicServiceIndex = ItemClass.GetPublicServiceIndex(ItemClass.Service.Education);
						Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Activate(properties.m_serviceNeeded, ItemClass.Service.Education);
					}
				}
			}
			else if (num5 == 1)
			{
				int num13 = 0;
				if (num2 >= workPlaces0)
				{
					num13++;
					if (num3 >= workPlaces1)
					{
						num13++;
						if (num4 >= workPlaces2)
						{
							num13++;
						}
					}
				}
				int num14 = Singleton<SimulationManager>.instance.m_randomizer.Int32(num13, 3);
				TransferManager.TransferOffer offer5 = default(TransferManager.TransferOffer);
				offer5.Priority = 0;
				offer5.Building = buildingID;
				offer5.Position = data.m_position;
				offer5.Amount = 1;
				switch (num14)
				{
				case 0:
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker0, offer5);
					break;
				case 1:
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker1, offer5);
					break;
				case 2:
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker2, offer5);
					break;
				case 3:
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Worker3, offer5);
					break;
				}
			}
		}
	}
	protected static void ExportResource(ushort buildingID, ref Building data, TransferManager.TransferReason resource, int amount)
	{
		byte district = Singleton<DistrictManager>.instance.GetDistrict(data.m_position);
		TransferManager.TransferReason transferReason = resource;
		switch (transferReason)
		{
		case TransferManager.TransferReason.Oil:
			Singleton<DistrictManager>.instance.m_districts.m_buffer[(int) district].m_exportData.m_tempOil += (uint) amount;
			break;
		case TransferManager.TransferReason.Ore:
		case TransferManager.TransferReason.Coal:
			Singleton<DistrictManager>.instance.m_districts.m_buffer[(int) district].m_exportData.m_tempOre += (uint) amount;
			break;
		case TransferManager.TransferReason.Logs:
			Singleton<DistrictManager>.instance.m_districts.m_buffer[(int) district].m_exportData.m_tempForestry += (uint) amount;
			break;
		case TransferManager.TransferReason.Grain:
			Singleton<DistrictManager>.instance.m_districts.m_buffer[(int) district].m_exportData.m_tempAgricultural += (uint) amount;
			break;
		case TransferManager.TransferReason.Goods:
			Singleton<DistrictManager>.instance.m_districts.m_buffer[(int) district].m_exportData.m_tempGoods += (uint) amount;
			break;
		default:
			if (transferReason != TransferManager.TransferReason.Petrol)
			{
				if (transferReason != TransferManager.TransferReason.Food)
				{
					if (transferReason != TransferManager.TransferReason.Lumber)
						break;
					goto case TransferManager.TransferReason.Logs;
				}
				else
					goto case TransferManager.TransferReason.Grain;
			}
			else
				goto case TransferManager.TransferReason.Oil;
		}
	}

	public override bool CalculateGroupData(ushort buildingID, ref Building buildingData, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
	{
		if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.BurnedDown)) != Building.Flags.Completed)
		{
			bool result = false;
			if ((buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
			{
				if (this.m_info.m_prefabDataLayer == layer)
				{
					result = true;
					if (this.m_info.m_buildingAI.CanUseGroupMesh(buildingID, ref buildingData))
					{
						BuildingManager instance = Singleton<BuildingManager>.instance;
						if (instance.m_common != null)
						{
							BuildingInfoBase burned = instance.m_common.m_burned;
							if (burned.m_lodMeshData != null)
							{
								vertexCount += burned.m_lodMeshData.m_vertices.Length;
								triangleCount += burned.m_lodMeshData.m_triangles.Length;
								objectCount++;
								vertexArrays |= (burned.m_lodMeshData.VertexArrayMask() | RenderGroup.VertexArrays.Colors | RenderGroup.VertexArrays.Uvs2);
								if (burned.m_lodMeshBase != null && buildingData.m_baseHeight >= 3)
								{
									vertexCount += burned.m_lodMeshBase.m_vertices.Length;
									triangleCount += burned.m_lodMeshBase.m_triangles.Length;
								}
							}
						}
					}
				}
			}
			else
			{
				if (this.m_info.m_prefabDataLayer == layer)
				{
					result = true;
				}
				if (base.CalculatePropGroupData(buildingID, ref buildingData, true, false, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays))
				{
					result = true;
				}
			}
			return result;
		}
		return base.CalculateGroupData(buildingID, ref buildingData, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
	}
	public override void PopulateGroupData(ushort buildingID, ref Building buildingData, out float height, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance)
	{
		if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.BurnedDown)) != Building.Flags.Completed)
		{
			height = this.m_info.m_size.y;
			if ((buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
			{
				if (this.m_info.m_prefabDataLayer == layer)
				{
					int width = buildingData.Width;
					int length = buildingData.Length;
					Vector3 a = new Vector3(Mathf.Cos(buildingData.m_angle), 0f, Mathf.Sin(buildingData.m_angle)) * 8f;
					Vector3 a2 = new Vector3(a.z, 0f, -a.x);
					Vector3 vector = buildingData.m_position - (float)width * 0.5f * a - (float)length * 0.5f * a2;
					Vector3 vector2 = buildingData.m_position + (float)width * 0.5f * a - (float)length * 0.5f * a2;
					Vector3 vector3 = buildingData.m_position + (float)width * 0.5f * a + (float)length * 0.5f * a2;
					Vector3 vector4 = buildingData.m_position - (float)width * 0.5f * a + (float)length * 0.5f * a2;
					float x = Mathf.Min(Mathf.Min(vector.x, vector2.x), Mathf.Min(vector3.x, vector4.x));
					float z = Mathf.Min(Mathf.Min(vector.z, vector2.z), Mathf.Min(vector3.z, vector4.z));
					float x2 = Mathf.Max(Mathf.Max(vector.x, vector2.x), Mathf.Max(vector3.x, vector4.x));
					float z2 = Mathf.Max(Mathf.Max(vector.z, vector2.z), Mathf.Max(vector3.z, vector4.z));
					min = Vector3.Min(min, new Vector3(x, buildingData.m_position.y - (float)buildingData.m_baseHeight, z));
					max = Vector3.Max(max, new Vector3(x2, buildingData.m_position.y + this.m_info.m_size.y, z2));
					maxRenderDistance = Mathf.Max(maxRenderDistance, 20000f);
					maxInstanceDistance = Mathf.Max(maxInstanceDistance, 1000f);
					if (this.m_info.m_buildingAI.CanUseGroupMesh(buildingID, ref buildingData))
					{
						BuildingManager instance = Singleton<BuildingManager>.instance;
						if (instance.m_common != null)
						{
							BuildingInfoBase burned = instance.m_common.m_burned;
							Vector3 vector5 = this.m_info.m_generatedInfo.m_min;
							Vector3 vector6 = this.m_info.m_generatedInfo.m_max;
							float num = (float)width * 4f;
							float num2 = (float)length * 4f;
							vector5 = Vector3.Max(vector5, new Vector3(-num, 0f, -num2));
							vector6 = Vector3.Min(vector6, new Vector3(num, 0f, num2));
							Vector3 vector7 = vector6 - vector5;
							float x3 = (vector7.x + 1f) / Mathf.Max(1f, burned.m_generatedInfo.m_size.x);
							float z3 = (vector7.z + 1f) / Mathf.Max(1f, burned.m_generatedInfo.m_size.z);
							height = burned.m_generatedInfo.m_size.y;
							Matrix4x4 rhs = Matrix4x4.Scale(new Vector3(x3, 1f, z3));
							if (burned.m_lodMeshData != null)
							{
								Vector3 a3;
								Quaternion q;
								buildingData.CalculateMeshPosition(out a3, out q);
								Matrix4x4 matrix4x = Matrix4x4.TRS(a3 - groupPosition, q, Vector3.one) * rhs;
								Vector4 colorLocation = RenderManager.GetColorLocation((uint)buildingID);
								Vector2 vector8 = new Vector2(colorLocation.x, colorLocation.y);
								Color32 buildingGroupState = this.m_info.m_buildingAI.GetBuildingGroupState(buildingID, ref buildingData);
								int[] triangles = burned.m_lodMeshData.m_triangles;
								int num3 = triangles.Length;
								for (int i = 0; i < num3; i++)
								{
									data.m_triangles[triangleIndex++] = triangles[i] + vertexIndex;
								}
								RenderGroup.VertexArrays vertexArrays = burned.m_lodMeshData.VertexArrayMask();
								Vector3[] vertices = burned.m_lodMeshData.m_vertices;
								Vector3[] normals = burned.m_lodMeshData.m_normals;
								Vector4[] tangents = burned.m_lodMeshData.m_tangents;
								Vector2[] uvs = burned.m_lodMeshData.m_uvs;
								Vector2[] uvs2 = burned.m_lodMeshData.m_uvs3;
								Vector2[] uvs3 = burned.m_lodMeshData.m_uvs4;
								int num4 = vertices.Length;
								for (int j = 0; j < num4; j++)
								{
									data.m_vertices[vertexIndex] = matrix4x.MultiplyPoint(vertices[j]);
									if ((vertexArrays & RenderGroup.VertexArrays.Normals) != (RenderGroup.VertexArrays)0)
									{
										data.m_normals[vertexIndex] = matrix4x.MultiplyVector(normals[j]);
									}
									else
									{
										data.m_normals[vertexIndex] = new Vector3(0f, 1f, 0f);
									}
									if ((vertexArrays & RenderGroup.VertexArrays.Tangents) != (RenderGroup.VertexArrays)0)
									{
										Vector4 vector9 = tangents[j];
										Vector3 vector10 = matrix4x.MultiplyVector(vector9);
										vector9.x = vector10.x;
										vector9.y = vector10.y;
										vector9.z = vector10.z;
										data.m_tangents[vertexIndex] = vector9;
									}
									else
									{
										data.m_tangents[vertexIndex] = new Vector4(1f, 0f, 0f, 1f);
									}
									if ((vertexArrays & RenderGroup.VertexArrays.Uvs) != (RenderGroup.VertexArrays)0)
									{
										data.m_uvs[vertexIndex] = uvs[j];
									}
									else
									{
										data.m_uvs[vertexIndex] = Vector2.zero;
									}
									data.m_colors[vertexIndex] = buildingGroupState;
									data.m_uvs2[vertexIndex] = vector8;
									data.m_uvs3[vertexIndex] = uvs2[j];
									data.m_uvs4[vertexIndex] = uvs3[j];
									vertexIndex++;
								}
								if (burned.m_lodMeshBase != null && buildingData.m_baseHeight >= 3)
								{
									triangles = burned.m_lodMeshBase.m_triangles;
									num3 = triangles.Length;
									for (int k = 0; k < num3; k++)
									{
										data.m_triangles[triangleIndex++] = triangles[k] + vertexIndex;
									}
									vertexArrays = burned.m_lodMeshBase.VertexArrayMask();
									vertices = burned.m_lodMeshBase.m_vertices;
									normals = burned.m_lodMeshBase.m_normals;
									tangents = burned.m_lodMeshBase.m_tangents;
									uvs = burned.m_lodMeshBase.m_uvs;
									uvs2 = burned.m_lodMeshBase.m_uvs3;
									uvs3 = burned.m_lodMeshBase.m_uvs4;
									num4 = vertices.Length;
									buildingGroupState.a = 255;
									for (int l = 0; l < num4; l++)
									{
										Vector3 v = vertices[l];
										v.y *= (float)buildingData.m_baseHeight;
										data.m_vertices[vertexIndex] = matrix4x.MultiplyPoint(v);
										if ((vertexArrays & RenderGroup.VertexArrays.Normals) != (RenderGroup.VertexArrays)0)
										{
											data.m_normals[vertexIndex] = matrix4x.MultiplyVector(normals[l]);
										}
										else
										{
											data.m_normals[vertexIndex] = new Vector3(0f, 1f, 0f);
										}
										if ((vertexArrays & RenderGroup.VertexArrays.Tangents) != (RenderGroup.VertexArrays)0)
										{
											Vector4 vector11 = tangents[l];
											Vector3 vector12 = matrix4x.MultiplyVector(vector11);
											vector11.x = vector12.x;
											vector11.y = vector12.y;
											vector11.z = vector12.z;
											data.m_tangents[vertexIndex] = vector11;
										}
										else
										{
											data.m_tangents[vertexIndex] = new Vector4(1f, 0f, 0f, 1f);
										}
										if ((vertexArrays & RenderGroup.VertexArrays.Uvs) != (RenderGroup.VertexArrays)0)
										{
											Vector2 vector13 = uvs[l];
											vector13.y *= (float)buildingData.m_baseHeight;
											data.m_uvs[vertexIndex] = vector13;
										}
										else
										{
											data.m_uvs[vertexIndex] = Vector2.zero;
										}
										data.m_colors[vertexIndex] = buildingGroupState;
										data.m_uvs2[vertexIndex] = vector8;
										data.m_uvs3[vertexIndex] = uvs2[l];
										data.m_uvs4[vertexIndex] = uvs3[l];
										vertexIndex++;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if (this.m_info.m_prefabDataLayer == layer)
				{
					int width2 = buildingData.Width;
					int length2 = buildingData.Length;
					Vector3 a4 = new Vector3(Mathf.Cos(buildingData.m_angle), 0f, Mathf.Sin(buildingData.m_angle)) * 8f;
					Vector3 a5 = new Vector3(a4.z, 0f, -a4.x);
					Vector3 vector14 = buildingData.m_position - (float)width2 * 0.5f * a4 - (float)length2 * 0.5f * a5;
					Vector3 vector15 = buildingData.m_position + (float)width2 * 0.5f * a4 - (float)length2 * 0.5f * a5;
					Vector3 vector16 = buildingData.m_position + (float)width2 * 0.5f * a4 + (float)length2 * 0.5f * a5;
					Vector3 vector17 = buildingData.m_position - (float)width2 * 0.5f * a4 + (float)length2 * 0.5f * a5;
					float x4 = Mathf.Min(Mathf.Min(vector14.x, vector15.x), Mathf.Min(vector16.x, vector17.x));
					float z4 = Mathf.Min(Mathf.Min(vector14.z, vector15.z), Mathf.Min(vector16.z, vector17.z));
					float x5 = Mathf.Max(Mathf.Max(vector14.x, vector15.x), Mathf.Max(vector16.x, vector17.x));
					float z5 = Mathf.Max(Mathf.Max(vector14.z, vector15.z), Mathf.Max(vector16.z, vector17.z));
					min = Vector3.Min(min, new Vector3(x4, buildingData.m_position.y - (float)buildingData.m_baseHeight, z4));
					max = Vector3.Max(max, new Vector3(x5, buildingData.m_position.y + this.m_info.m_size.y, z5));
					maxRenderDistance = Mathf.Max(maxRenderDistance, 20000f);
					maxInstanceDistance = Mathf.Max(maxInstanceDistance, 1000f);
				}
				base.PopulatePropGroupData(buildingID, ref buildingData, true, false, groupX, groupZ, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
			}
		}
		else
		{
			base.PopulateGroupData(buildingID, ref buildingData, out height, groupX, groupZ, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
		}
	}
	public void GetDecorationDirectionsC(out bool negX, out bool posX, out bool negZ, out bool posZ)
	{
		negX = false;
		posX = false;
		negZ = false;
		posZ = true;
	}

	///////////PRIVATE BUILDING

	public Color GetColorP(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
	{
		switch (infoMode)
		{
		case InfoManager.InfoMode.NoisePollution:
			{
				DistrictManager instance = Singleton<DistrictManager>.instance;
				byte district = instance.GetDistrict(data.m_position);
				DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
				int num;
				int num2;
				this.GetPollutionRates((!this.ShowConsumption(buildingID, ref data)) ? 0 : 40, cityPlanningPolicies, out num, out num2);
				if (num2 != 0)
				{
					return CommonBuildingAI.GetNoisePollutionColor((float)num2 * 0.25f);
				}
				return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
			}
		case InfoManager.InfoMode.Pollution:
			{
				DistrictManager instance2 = Singleton<DistrictManager>.instance;
				byte district2 = instance2.GetDistrict(data.m_position);
				DistrictPolicies.CityPlanning cityPlanningPolicies2 = instance2.m_districts.m_buffer[(int)district2].m_cityPlanningPolicies;
				int num3;
				int num4;
				this.GetPollutionRates((!this.ShowConsumption(buildingID, ref data)) ? 0 : 40, cityPlanningPolicies2, out num3, out num4);
				if (num3 != 0)
				{
					return ColorUtils.LinearLerp(Singleton<InfoManager>.instance.m_properties.m_neutralColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor, Mathf.Clamp01((float)num3 * 0.01f));
				}
				return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
			}
		}
		return GetColorC(buildingID, ref data, infoMode);
	}
	protected bool ShowConsumption(ushort buildingID, ref Building data)
	{
		return (data.m_flags & (Building.Flags.Completed | Building.Flags.Abandoned | Building.Flags.BurnedDown)) == Building.Flags.Completed;
	}
	public override string GetLocalizedStatus(ushort buildingID, ref Building data)
	{
		if ((data.m_flags & Building.Flags.Abandoned) != Building.Flags.None)
		{
			return Locale.Get("BUILDING_STATUS_ABANDONED");
		}
		if ((data.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
		{
			return Locale.Get("BUILDING_STATUS_BURNED");
		}
		if ((data.m_flags & Building.Flags.Upgrading) != Building.Flags.None)
		{
			return Locale.Get("BUILDING_STATUS_UPGRADING");
		}
		if ((data.m_flags & Building.Flags.Completed) == Building.Flags.None)
		{
			return Locale.Get("BUILDING_STATUS_UNDER_CONSTRUCTION");
		}
		if ((data.m_flags & Building.Flags.Active) == Building.Flags.None)
		{
			return this.GetLocalizedStatusInactive(buildingID, ref data);
		}
		return this.GetLocalizedStatusActive(buildingID, ref data);
	}
	protected virtual string GetLocalizedStatusInactiveP(ushort buildingID, ref Building data)
	{
		return Locale.Get("BUILDING_STATUS_NOT_OPERATING");
	}
	protected virtual string GetLocalizedStatusActiveP(ushort buildingID, ref Building data)
	{
		return Locale.Get("BUILDING_STATUS_DEFAULT");
	}
	public  void CreateBuildingP(ushort buildingID, ref Building data)
	{
		CreateBuildingC(buildingID, ref data);
		int num;
		int num2;
		int num3;
		int num4;
		this.CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length, out num, out num2, out num3, out num4);
		int workCount = num + num2 + num3 + num4;
		int homeCount = this.CalculateHomeCount(new Randomizer((int)buildingID), data.Width, data.Length);
		int visitCount = this.CalculateVisitplaceCount(new Randomizer((int)buildingID), data.Width, data.Length);
		Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount, workCount, visitCount, 0, 0);
	}
	public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
	{
		base.BuildingLoaded(buildingID, ref data, version);
		int num;
		int num2;
		int num3;
		int num4;
		this.CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length, out num, out num2, out num3, out num4);
		int workCount = num + num2 + num3 + num4;
		int homeCount = this.CalculateHomeCount(new Randomizer((int)buildingID), data.Width, data.Length);
		int visitCount = this.CalculateVisitplaceCount(new Randomizer((int)buildingID), data.Width, data.Length);
		base.EnsureCitizenUnits(buildingID, ref data, homeCount, workCount, visitCount, 0);
	}
	public  void ReleaseBuildingP(ushort buildingID, ref Building data)
	{
		ReleaseBuildingC(buildingID, ref data);
	}
	protected int GetConstructionTime()
	{
		return this.m_constructionTime;
	}
	public  void SimulationStepP(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
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
					CheckNearbyBuildingZones(buildingData.m_position);
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
						}
						instance3.m_currentBuildIndex += 1u;
					}
				}
			}
		}
	}
	private static void CheckNearbyBuildingZones(Vector3 position)
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
	protected  void SimulationStepActiveP(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
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
					this.BuildingDeactivated(buildingID, ref buildingData);
					Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
				}
			}
		}
		else
		{
			buildingData.m_majorProblemTimer = 0;
		}
	}
	protected int HandleWorkers(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount)
	{
		int b = 0;
		GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
		int num;
		int num2;
		int num3;
		int num4;
		this.CalculateWorkplaceCount(new Randomizer((int)buildingID), buildingData.Width, buildingData.Length, out num, out num2, out num3, out num4);
		workPlaceCount = num + num2 + num3 + num4;
		if (buildingData.m_fireIntensity == 0)
		{
			HandleWorkPlaces(buildingID, ref buildingData, num, num2, num3, num4, ref behaviour, aliveWorkerCount, totalWorkerCount);
			if (aliveWorkerCount != 0 && workPlaceCount != 0)
			{
				int num5 = (behaviour.m_efficiencyAccumulation + aliveWorkerCount - 1) / aliveWorkerCount;
				b = 2 * num5 - 200 * num5 / ((100 * aliveWorkerCount + workPlaceCount - 1) / workPlaceCount + 100);
			}
		}
		Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoWorkers | Notification.Problem.NoEducatedWorkers);
		int num6 = (num4 * 300 + num3 * 200 + num2 * 100) / (workPlaceCount + 1);
		int num7 = (behaviour.m_educated3Count * 300 + behaviour.m_educated2Count * 200 + behaviour.m_educated1Count * 100) / (aliveWorkerCount + 1);
		if (aliveWorkerCount < workPlaceCount >> 1)
		{
			buildingData.m_workerProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_workerProblemTimer + 1));
			if (buildingData.m_workerProblemTimer >= 128)
			{
				problem = Notification.AddProblems(problem, Notification.Problem.NoWorkers | Notification.Problem.MajorProblem);
			}
			else if (buildingData.m_workerProblemTimer >= 64)
			{
				problem = Notification.AddProblems(problem, Notification.Problem.NoWorkers);
			}
		}
		else if (num7 < num6 - 50)
		{
			buildingData.m_workerProblemTimer = (byte)Mathf.Min(255, (int)(buildingData.m_workerProblemTimer + 1));
			if (buildingData.m_workerProblemTimer >= 128)
			{
				problem = Notification.AddProblems(problem, Notification.Problem.NoEducatedWorkers | Notification.Problem.MajorProblem);
			}
			else if (buildingData.m_workerProblemTimer >= 64)
			{
				problem = Notification.AddProblems(problem, Notification.Problem.NoEducatedWorkers);
			}
		}
		else
		{
			buildingData.m_workerProblemTimer = 0;
		}
		buildingData.m_problems = problem;
		return Mathf.Max(1, b);
	}
	public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
	{
			/*
		Randomizer randomizer = new Randomizer((int)buildingID);
		for (int i = 0; i <= (int)this.m_info.m_class.m_level; i++)
		{
			randomizer.Int32(1000u);
		}
		ItemClass.Level level = this.m_info.m_class.m_level + 1;
		DistrictManager instance = Singleton<DistrictManager>.instance;
		byte district = instance.GetDistrict(data.m_position);
		ushort style = instance.m_districts.m_buffer[(int)district].m_Style;
		return Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref randomizer, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, level, data.Width, data.Length, this.m_info.m_zoningMode, (int)style);*/

		//	BuildingData[] dataArray = BuildingDataManager.buildingData;		
			//Bdata = dataArray [(int)buildingID];

			//BuildingInfo NewInfo = new BuildingInfo ();

			if (data.Info.m_class.m_level == ItemClass.Level.Level1) {
				
				//Bdata.level = 2;
				//this.m_info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level2");

				return PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level2");

			}else
				if (data.Info.m_class.m_level == ItemClass.Level.Level2) {

				//Bdata.level = 3;
				return PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level3");

			}else
					if (data.Info.m_class.m_level == ItemClass.Level.Level3) {

				//Bdata.level = 4;
				return PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level4");

					}else if (data.Info.m_class.m_level == ItemClass.Level.Level4) {

				//Bdata.level = 5;
				return PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level5");

			}else
			return PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name);
		
}
	public override void BuildingUpgraded(ushort buildingID, ref Building data)
	{
		int num;
		int num2;
		int num3;
		int num4;
		this.CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length, out num, out num2, out num3, out num4);
		int workCount = num + num2 + num3 + num4;
		int homeCount = this.CalculateHomeCount(new Randomizer((int)buildingID), data.Width, data.Length);
		int visitCount = this.CalculateVisitplaceCount(new Randomizer((int)buildingID), data.Width, data.Length);
		base.EnsureCitizenUnits(buildingID, ref data, homeCount, workCount, visitCount, 0);
	}
	protected void StartUpgrading(ushort buildingID, ref Building buildingData)
	{
		buildingData.m_frame0.m_constructState = 0;
		buildingData.m_frame1.m_constructState = 0;
		buildingData.m_frame2.m_constructState = 0;
		buildingData.m_frame3.m_constructState = 0;
		Building.Flags flags = buildingData.m_flags;
		flags |= Building.Flags.Upgrading;
		flags &= ~Building.Flags.Completed;
		flags &= ~Building.Flags.LevelUpEducation;
		flags &= ~Building.Flags.LevelUpLandValue;
		buildingData.m_flags = flags;
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
	public override void GetWidthRange(out int minWidth, out int maxWidth)
	{
			minWidth = 1;
			maxWidth = 32;
	}
	public override void GetLengthRange(out int minLength, out int maxLength)
	{
			minLength = 1;
			maxLength = 16;
	}
	public override void GetDecorationArea(out int width, out int length, out float offset)
	{
		width = this.m_info.m_cellWidth;
		length = ((this.m_info.m_zoningMode != BuildingInfo.ZoningMode.Straight) ? this.m_info.m_cellLength : 4);
		offset = (float)(length - this.m_info.m_cellLength) * 4f;
		if (!this.m_info.m_expandFrontYard)
		{
			offset = -offset;
		}
	}
	public override void GetDecorationDirections(out bool negX, out bool posX, out bool negZ, out bool posZ)
	{
		negX = (this.m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerRight);
		posX = (this.m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft);
		negZ = false;
		posZ = true;
	}
	public virtual int CalculateHomeCountP(Randomizer r, int width, int length)
	{
		return 0;
	}
	public virtual int CalculateVisitplaceCountP(Randomizer r, int width, int length)
	{
		return 0;
	}
	public virtual void CalculateWorkplaceCountP(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
	{
		level0 = 0;
		level1 = 0;
		level2 = 0;
		level3 = 0;
	}
	public virtual int CalculateProductionCapacityP(Randomizer r, int width, int length)
	{
		return 0;
	}
	public virtual void GetConsumptionRatesP(Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation)
	{
		electricityConsumption = 0;
		waterConsumption = 0;
		sewageAccumulation = 0;
		garbageAccumulation = 0;
		incomeAccumulation = 0;
	}
	public virtual void GetPollutionRatesP(int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
	{
		groundPollution = 0;
		noisePollution = 0;
	}

	///////RESIDENTIAL AI

	public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
	{
		InfoManager.InfoMode infoMode1 = infoMode;
		switch (infoMode1)
		{
		case InfoManager.InfoMode.Health:
			if (this.ShowConsumption(buildingID, ref data) && (int) data.m_citizenCount != 0)
				return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) Citizen.GetHealthLevel((int) data.m_health) * 0.2f);
			return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
		case InfoManager.InfoMode.Density:
			if (!this.ShowConsumption(buildingID, ref data) || (int) data.m_citizenCount == 0)
				return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
			int num1 = ((int) data.m_citizenCount - (int) data.m_youngs - (int) data.m_adults - (int) data.m_seniors) * 3;
			int num2 = (int) data.m_youngs + (int) data.m_adults;
			int num3 = (int) data.m_seniors;
			if (num1 == 0 && num2 == 0 && num3 == 0)
				return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
			if (num1 >= num2 && num1 >= num3)
				return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_activeColor;
			if (num2 >= num3)
				return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_activeColorB;
			return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor;
		default:
			switch (infoMode1 - 17)
			{
			case InfoManager.InfoMode.None:
				if (this.ShowConsumption(buildingID, ref data))
					return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_neutralColor, Color.Lerp(Singleton<ZoneManager>.instance.m_properties.m_zoneColors[2], Singleton<ZoneManager>.instance.m_properties.m_zoneColors[3], 0.5f) * 0.5f, (float) (0.200000002980232 + (double) this.m_info.m_class.m_level * 0.200000002980232));
				return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
			case InfoManager.InfoMode.Water:
				if (!this.ShowConsumption(buildingID, ref data) || (int) data.m_citizenCount == 0)
					return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
				InfoManager.SubInfoMode currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
				int num4;
				int num5;
				if (currentSubMode == InfoManager.SubInfoMode.Default)
				{
					num4 = (int) data.m_education1 * 100;
					num5 = (int) data.m_teens + (int) data.m_youngs + (int) data.m_adults + (int) data.m_seniors;
				}
				else if (currentSubMode == InfoManager.SubInfoMode.WaterPower)
				{
					num4 = (int) data.m_education2 * 100;
					num5 = (int) data.m_youngs + (int) data.m_adults + (int) data.m_seniors;
				}
				else
				{
					num4 = (int) data.m_education3 * 100;
					num5 = (int) data.m_youngs * 2 / 3 + (int) data.m_adults + (int) data.m_seniors;
				}
				if (num5 != 0)
					num4 = (num4 + (num5 >> 1)) / num5;
				int num6 = Mathf.Clamp(num4, 0, 100);
				return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) num6 * 0.01f);
			default:
				return GetColorP(buildingID, ref data, infoMode);
			}
		}
	}
	public override int GetResourceRate(ushort buildingID, ref Building data, NaturalResourceManager.Resource resource)
	{
		return base.GetResourceRate(buildingID, ref data, resource);
	}
	public override int GetResourceRate(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource)
	{
		return base.GetResourceRate(buildingID, ref data, resource);
	}
	public override int GetResourceRate(ushort buildingID, ref Building data, EconomyManager.Resource resource)
	{
		if (resource == EconomyManager.Resource.PrivateIncome)
		{
			Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			GetHomeBehaviour(buildingID, ref data, ref behaviourData, ref num, ref num2, ref num3, ref num4, ref num5);
			return behaviourData.m_incomeAccumulation;
		}
		return base.GetResourceRate(buildingID, ref data, resource);
	}
	public override int GetElectricityRate(ushort buildingID, ref Building data)
	{
		Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		GetHomeBehaviour(buildingID, ref data, ref behaviourData, ref num, ref num2, ref num3, ref num4, ref num5);
		return -((behaviourData.m_electricityConsumption + 99) / 100);
	}
	public override int GetWaterRate(ushort buildingID, ref Building data)
	{
		Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		GetHomeBehaviour(buildingID, ref data, ref behaviourData, ref num, ref num2, ref num3, ref num4, ref num5);
		return -((behaviourData.m_waterConsumption + 99) / 100);
	}
	public override int GetGarbageRate(ushort buildingID, ref Building data)
	{
		Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		GetHomeBehaviour(buildingID, ref data, ref behaviourData, ref num, ref num2, ref num3, ref num4, ref num5);
		return (behaviourData.m_garbageAccumulation + 99) / 100;
	}
	public override string GetLevelUpInfo(ushort buildingID, ref Building data, out float progress)
	{
		if ((data.m_problems & Notification.Problem.FatalProblem) != Notification.Problem.None)
		{
			progress = 0f;
			return Locale.Get("LEVELUP_IMPOSSIBLE");
		}
			if (data.Info.m_class.m_level == ItemClass.Level.Level5)
		{
			progress = 0f;
			return Locale.Get("LEVELUP_RESIDENTIAL_HAPPY");
		}
		if (data.m_problems != Notification.Problem.None)
		{
			progress = 0f;
			return Locale.Get("LEVELUP_DISTRESS");
		}
		if (data.m_levelUpProgress == 0)
		{
			return base.GetLevelUpInfo(buildingID, ref data, out progress);
		}
		if (data.m_levelUpProgress == 1)
		{
			progress = 0.933333337f;
			return Locale.Get("LEVELUP_HIGHRISE_BAN");
		}
		int num = (int)((data.m_levelUpProgress & 15) - 1);
		int num2 = (data.m_levelUpProgress >> 4) - 1;
		if (num <= num2)
		{
			progress = (float)num * 0.06666667f;
			return Locale.Get("LEVELUP_LOWTECH");
		}
		progress = (float)num2 * 0.06666667f;
		return Locale.Get("LEVELUP_LOWLANDVALUE");
	}
	protected string GetLocalizedStatusInactive(ushort buildingID, ref Building data)
	{
		Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		GetHomeBehaviour(buildingID, ref data, ref behaviourData, ref num, ref num2, ref num3, ref num4, ref num5);
		return string.Format(Locale.Get("BUILDING_STATUS_HOUSEHOLDS"), num4, num3);
	}
	protected  string GetLocalizedStatusActive(ushort buildingID, ref Building data)
	{
		Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		GetHomeBehaviour(buildingID, ref data, ref behaviourData, ref num, ref num2, ref num3, ref num4, ref num5);
		return string.Format(Locale.Get("BUILDING_STATUS_HOUSEHOLDS"), num4, num3);
	}
	public override void CreateBuilding(ushort buildingID, ref Building data)
	{
		CreateBuildingP(buildingID, ref data);
	}
	public override void ReleaseBuilding(ushort buildingID, ref Building data)
	{
		ReleaseBuildingP(buildingID, ref data);
	}
	protected override void ManualActivation(ushort buildingID, ref Building buildingData)
	{
		Vector3 position = buildingData.m_position;
		position.y += this.m_info.m_size.y;
		Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainPopulation, position, 1.5f);
	}
	protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
	{
		if ((buildingData.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None)
		{
			Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, (float)(-(float)buildingData.Width * buildingData.Length), 64f);
		}
		else
		{
			Vector3 position = buildingData.m_position;
			position.y += this.m_info.m_size.y;
			Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LosePopulation, position, 1.5f);
		}
	}
	public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
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

			Debug.Log (Bdata.Name);

			if (Bdata.saveflag == false){

				if (Bdata.level == 2) {
					buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level2");

				}
				if (Bdata.level == 3) { 

					buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level3");

				}
				if (Bdata.level == 4) {

					buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level4");

				}
				if (Bdata.level == 5) {

					buildingData.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level5");

				}
				Bdata.saveflag = true;
			}

		SimulationStepP(buildingID, ref buildingData, ref frameData);
		DistrictManager instance = Singleton<DistrictManager>.instance;
		byte district = instance.GetDistrict(buildingData.m_position);
		DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
		if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
		{
			District[] expr_5A_cp_0_cp_0 = instance.m_districts.m_buffer;
			byte expr_5A_cp_0_cp_1 = district;
			expr_5A_cp_0_cp_0[(int)expr_5A_cp_0_cp_1].m_residentialData.m_tempBuildingCount = (ushort)(expr_5A_cp_0_cp_0[(int)expr_5A_cp_0_cp_1].m_residentialData.m_tempBuildingCount + 1);
			District[] expr_7E_cp_0_cp_0 = instance.m_districts.m_buffer;
			byte expr_7E_cp_0_cp_1 = district;
			expr_7E_cp_0_cp_0[(int)expr_7E_cp_0_cp_1].m_residentialData.m_tempBuildingArea = expr_7E_cp_0_cp_0[(int)expr_7E_cp_0_cp_1].m_residentialData.m_tempBuildingArea + (uint)(buildingData.Width * buildingData.Length);
			if ((buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None)
			{
				District[] expr_BE_cp_0_cp_0 = instance.m_districts.m_buffer;
				byte expr_BE_cp_0_cp_1 = district;
				expr_BE_cp_0_cp_0[(int)expr_BE_cp_0_cp_1].m_residentialData.m_tempAbandonedCount = (ushort)(expr_BE_cp_0_cp_0[(int)expr_BE_cp_0_cp_1].m_residentialData.m_tempAbandonedCount + 1);
			}
			else if ((buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
			{
				District[] expr_F8_cp_0_cp_0 = instance.m_districts.m_buffer;
				byte expr_F8_cp_0_cp_1 = district;
				expr_F8_cp_0_cp_0[(int)expr_F8_cp_0_cp_1].m_residentialData.m_tempBurnedCount = (ushort)(expr_F8_cp_0_cp_0[(int)expr_F8_cp_0_cp_1].m_residentialData.m_tempBurnedCount + 1);
			}
		}
		if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialHigh && (cityPlanningPolicies & DistrictPolicies.CityPlanning.HighriseBan) != DistrictPolicies.CityPlanning.None && this.m_info.m_class.m_level == ItemClass.Level.Level5)
		{
			SimulationManager instance2 = Singleton<SimulationManager>.instance;
			if (instance2.m_randomizer.Int32(10u) == 0 && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance2.m_currentBuildIndex)
			{
				District[] expr_179_cp_0 = instance.m_districts.m_buffer;
				byte expr_179_cp_1 = district;
				expr_179_cp_0[(int)expr_179_cp_1].m_cityPlanningPoliciesEffect = (expr_179_cp_0[(int)expr_179_cp_1].m_cityPlanningPoliciesEffect | DistrictPolicies.CityPlanning.HighriseBan);
				//buildingData.m_flags |= Building.Flags.Demolishing;
				instance2.m_currentBuildIndex += 1u;
			}
		}
	}
	protected void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
	{

			//////////////




			/////////
		Citizen.BehaviourData behaviourData = default(Citizen.BehaviourData);
		int num = 0;
		int citizenCount = 0;
		int num2 = 0;
		int aliveHomeCount = 0;
		int num3 = 0;
		GetHomeBehaviour(buildingID, ref buildingData, ref behaviourData, ref num, ref citizenCount, ref num2, ref aliveHomeCount, ref num3);
		DistrictManager instance = Singleton<DistrictManager>.instance;
		byte district = instance.GetDistrict(buildingData.m_position);
		DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
		DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[(int)district].m_taxationPolicies;
		DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
		DistrictPolicies.Special specialPolicies = instance.m_districts.m_buffer[(int)district].m_specialPolicies;
		District[] expr_B9_cp_0 = instance.m_districts.m_buffer;
		byte expr_B9_cp_1 = district;
		expr_B9_cp_0[(int)expr_B9_cp_1].m_servicePoliciesEffect = (expr_B9_cp_0[(int)expr_B9_cp_1].m_servicePoliciesEffect | (servicePolicies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.PetBan | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.SmokingBan)));
		if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
		{
			if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow)) != (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow))
			{
				District[] expr_FF_cp_0 = instance.m_districts.m_buffer;
				byte expr_FF_cp_1 = district;
				expr_FF_cp_0[(int)expr_FF_cp_1].m_taxationPoliciesEffect = (expr_FF_cp_0[(int)expr_FF_cp_1].m_taxationPoliciesEffect | (taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResLow | DistrictPolicies.Taxation.TaxLowerResLow)));
			}
		}
		else if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh)) != (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh))
		{
			District[] expr_134_cp_0 = instance.m_districts.m_buffer;
			byte expr_134_cp_1 = district;
			expr_134_cp_0[(int)expr_134_cp_1].m_taxationPoliciesEffect = (expr_134_cp_0[(int)expr_134_cp_1].m_taxationPoliciesEffect | (taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseResHigh | DistrictPolicies.Taxation.TaxLowerResHigh)));
		}
		District[] expr_158_cp_0 = instance.m_districts.m_buffer;
		byte expr_158_cp_1 = district;
		expr_158_cp_0[(int)expr_158_cp_1].m_cityPlanningPoliciesEffect = (expr_158_cp_0[(int)expr_158_cp_1].m_cityPlanningPoliciesEffect | (cityPlanningPolicies & (DistrictPolicies.CityPlanning.HighTechHousing | DistrictPolicies.CityPlanning.HeavyTrafficBan | DistrictPolicies.CityPlanning.EncourageBiking | DistrictPolicies.CityPlanning.BikeBan | DistrictPolicies.CityPlanning.OldTown)));
		District[] expr_17F_cp_0 = instance.m_districts.m_buffer;
		byte expr_17F_cp_1 = district;
		expr_17F_cp_0[(int)expr_17F_cp_1].m_specialPoliciesEffect = (expr_17F_cp_0[(int)expr_17F_cp_1].m_specialPoliciesEffect | (specialPolicies & (DistrictPolicies.Special.ProHippie | DistrictPolicies.Special.ProHipster | DistrictPolicies.Special.ProRedneck | DistrictPolicies.Special.ProGangsta | DistrictPolicies.Special.AntiHippie | DistrictPolicies.Special.AntiHipster | DistrictPolicies.Special.AntiRedneck | DistrictPolicies.Special.AntiGangsta | DistrictPolicies.Special.ComeOneComeAll | DistrictPolicies.Special.WeAreTheNorm)));
		if (instance.IsPolicyLoaded(DistrictPolicies.Policies.ProHippie))
		{
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			if ((specialPolicies & (DistrictPolicies.Special.ProHippie | DistrictPolicies.Special.ComeOneComeAll)) != DistrictPolicies.Special.None)
			{
				num4 += 100;
			}
			if ((specialPolicies & (DistrictPolicies.Special.AntiHippie | DistrictPolicies.Special.WeAreTheNorm)) != DistrictPolicies.Special.None)
			{
				num4 -= 100;
			}
			if ((specialPolicies & (DistrictPolicies.Special.ProHipster | DistrictPolicies.Special.ComeOneComeAll)) != DistrictPolicies.Special.None)
			{
				num5 += 100;
			}
			if ((specialPolicies & (DistrictPolicies.Special.AntiHipster | DistrictPolicies.Special.WeAreTheNorm)) != DistrictPolicies.Special.None)
			{
				num5 -= 100;
			}
			if ((specialPolicies & (DistrictPolicies.Special.ProRedneck | DistrictPolicies.Special.ComeOneComeAll)) != DistrictPolicies.Special.None)
			{
				num6 += 100;
			}
			if ((specialPolicies & (DistrictPolicies.Special.AntiRedneck | DistrictPolicies.Special.WeAreTheNorm)) != DistrictPolicies.Special.None)
			{
				num6 -= 100;
			}
			if ((specialPolicies & (DistrictPolicies.Special.ProGangsta | DistrictPolicies.Special.ComeOneComeAll)) != DistrictPolicies.Special.None)
			{
				num7 += 100;
			}
			if ((specialPolicies & (DistrictPolicies.Special.AntiGangsta | DistrictPolicies.Special.WeAreTheNorm)) != DistrictPolicies.Special.None)
			{
				num7 -= 100;
			}
			if (num4 < 0)
			{
				num4 = 0;
			}
			if (num5 < 0)
			{
				num5 = 0;
			}
			if (num6 < 0)
			{
				num6 = 0;
			}
			if (num7 < 0)
			{
				num7 = 0;
			}
			int range = Mathf.Max(100, num4 + num5 + num6 + num7);
			Randomizer randomizer = new Randomizer((int)buildingID << 16);
			int num8 = randomizer.Int32((uint)range);
			if (num8 < num4)
			{
				buildingData.SubCultureType = Citizen.SubCulture.Hippie;
			}
			else if (num8 < num4 + num5)
			{
				buildingData.SubCultureType = Citizen.SubCulture.Hipster;
			}
			else if (num8 < num4 + num5 + num6)
			{
				buildingData.SubCultureType = Citizen.SubCulture.Redneck;
			}
			else if (num8 < num4 + num5 + num6 + num7)
			{
				buildingData.SubCultureType = Citizen.SubCulture.Gangsta;
			}
			else
			{
				buildingData.SubCultureType = Citizen.SubCulture.Generic;
			}
		}
		int num9;
		int num10;
		int num11;
		int num12;
		int num13;
		this.GetConsumptionRates(new Randomizer((int)buildingID), 100, out num9, out num10, out num11, out num12, out num13);
		num9 = 1 + (num9 * behaviourData.m_electricityConsumption + 9999) / 10000;
		num10 = 1 + (num10 * behaviourData.m_waterConsumption + 9999) / 10000;
		num11 = 1 + (num11 * behaviourData.m_sewageAccumulation + 9999) / 10000;
		num12 = (num12 * behaviourData.m_garbageAccumulation + 9999) / 10000;
		num13 = (num13 * behaviourData.m_incomeAccumulation + 9999) / 10000;
		if (num12 != 0)
		{
			if ((servicePolicies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
			{
				if ((servicePolicies & DistrictPolicies.Services.PetBan) != DistrictPolicies.Services.None)
				{
					num12 = Mathf.Max(1, num12 * 7650 / 10000);
				}
				else
				{
					num12 = Mathf.Max(1, num12 * 85 / 100);
				}
				num13 = num13 * 95 / 100;
			}
			else if ((servicePolicies & DistrictPolicies.Services.PetBan) != DistrictPolicies.Services.None)
			{
				num12 = Mathf.Max(1, num12 * 90 / 100);
			}
		}
		if (buildingData.m_fireIntensity == 0)
		{
			int num14 = HandleCommonConsumption(buildingID, ref buildingData, ref num9, ref num10, ref num11, ref num12, servicePolicies);
			num13 = (num13 * num14 + 99) / 100;
			if (num13 != 0)
			{
				Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PrivateIncome, num13, this.m_info.m_class, taxationPolicies);
			}
			buildingData.m_flags |= Building.Flags.Active;
		}
		else
		{
			buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Electricity | Notification.Problem.Water | Notification.Problem.Sewage | Notification.Problem.Flood);
			buildingData.m_flags &= ~Building.Flags.Active;
		}
		int num15 = 0;
		int wellbeing = 0;
		float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
		if (behaviourData.m_healthAccumulation != 0)
		{
			if (num != 0)
			{
				num15 = (behaviourData.m_healthAccumulation + (num >> 1)) / num;
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
		int taxRate = Singleton<EconomyManager>.instance.GetTaxRate(this.m_info.m_class, taxationPolicies);
		int num16 = (int)((Citizen.Wealth)11 - Citizen.GetWealthLevel(this.m_info.m_class.m_level));
		if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialHigh)
		{
			num16++;
		}
		if (taxRate >= num16 + 4)
		{
			if (buildingData.m_taxProblemTimer != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(32u) == 0)
			{
				int num17 = taxRate - num16 >> 2;
				buildingData.m_taxProblemTimer = (byte)Mathf.Min(255, (int)buildingData.m_taxProblemTimer + num17);
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
		int happiness = Citizen.GetHappiness(num15, wellbeing);
		buildingData.m_health = (byte)num15;
		buildingData.m_happiness = (byte)happiness;
		buildingData.m_citizenCount = (byte)num;
		buildingData.m_education1 = (byte)behaviourData.m_education1Count;
		buildingData.m_education2 = (byte)behaviourData.m_education2Count;
		buildingData.m_education3 = (byte)behaviourData.m_education3Count;
		buildingData.m_teens = (byte)behaviourData.m_teenCount;
		buildingData.m_youngs = (byte)behaviourData.m_youngCount;
		buildingData.m_adults = (byte)behaviourData.m_adultCount;
		buildingData.m_seniors = (byte)behaviourData.m_seniorCount;
		HandleSick(buildingID, ref buildingData, ref behaviourData, citizenCount);
		HandleDead(buildingID, ref buildingData, ref behaviourData, citizenCount);
		int num18 = behaviourData.m_crimeAccumulation / 10;
		if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
		{
			num18 = num18 * 3 + 3 >> 2;
		}
		HandleCrime(buildingID, ref buildingData, num18, num);
		int num19 = (int)buildingData.m_crimeBuffer;
		if (num != 0)
		{
			Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, num, buildingData.m_position, radius);
			int num20 = behaviourData.m_educated0Count * 30 + behaviourData.m_educated1Count * 15 + behaviourData.m_educated2Count * 10;
			num20 = num20 / num + 50;
			if ((int)buildingData.m_crimeBuffer > num * 40)
			{
				num20 += 30;
			}
			else if ((int)buildingData.m_crimeBuffer > num * 15)
			{
				num20 += 15;
			}
			else if ((int)buildingData.m_crimeBuffer > num * 5)
			{
				num20 += 10;
			}
			buildingData.m_fireHazard = (byte)num20;
			num19 = (num19 + (num >> 1)) / num;
		}
		else
		{
			buildingData.m_fireHazard = 0;
			num19 = 0;
		}
		if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.HighTechHousing) != DistrictPolicies.CityPlanning.None)
		{
			Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 25, this.m_info.m_class);
			Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.LandValue, 50, buildingData.m_position, radius);
		}
		SimulationManager instance2 = Singleton<SimulationManager>.instance;
		uint num21 = (instance2.m_currentFrameIndex & 3840u) >> 8;
		if ((ulong)num21 == (ulong)((long)(buildingID & 15)) && Singleton<ZoneManager>.instance.m_lastBuildIndex == instance2.m_currentBuildIndex && (buildingData.m_flags & Building.Flags.Upgrading) == Building.Flags.None)
		{
			this.CheckBuildingLevel(buildingID, ref buildingData, ref frameData, ref behaviourData);
		}
		if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) != Building.Flags.None)
		{
			if (num3 != 0 && (buildingData.m_problems & Notification.Problem.MajorProblem) == Notification.Problem.None && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
			{
				TransferManager.TransferReason homeReason = this.GetHomeReason(buildingID, ref buildingData, ref Singleton<SimulationManager>.instance.m_randomizer);
				if (homeReason != TransferManager.TransferReason.None)
				{
					TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
					offer.Priority = Mathf.Max(1, num3 * 8 / num2);
					offer.Building = buildingID;
					offer.Position = buildingData.m_position;
					offer.Amount = num3;
					Singleton<TransferManager>.instance.AddIncomingOffer(homeReason, offer);
				}
			}
			instance.m_districts.m_buffer[(int)district].AddResidentialData(ref behaviourData, num, num15, happiness, num19, num2, aliveHomeCount, num3, (int)this.m_info.m_class.m_level, num9, num10, num11, num12, num13, Mathf.Min(100, (int)(buildingData.m_garbageBuffer / 50)), (int)(buildingData.m_waterPollution * 100 / 255), buildingData.SubCultureType);
			SimulationStepActiveP(buildingID, ref buildingData, ref frameData);
			HandleFire(buildingID, ref buildingData, ref frameData, servicePolicies);
		}
	}
	public override bool GetFireParameters(ushort buildingID, ref Building buildingData, out int fireHazard, out int fireSize, out int fireTolerance)
	{
		fireHazard = (int)(((buildingData.m_flags & Building.Flags.Active) != Building.Flags.None) ? buildingData.m_fireHazard : 0);
		fireSize = 127;
		fireTolerance = 8;
		return true;
	}
	private void CheckBuildingLevel(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, ref Citizen.BehaviourData behaviour)
		{
			
				

				DistrictManager instance = Singleton<DistrictManager>.instance;
				byte district = instance.GetDistrict (buildingData.m_position);
				DistrictPolicies.CityPlanning cityPlanning = instance.m_districts.m_buffer [(int)district].m_cityPlanningPolicies;
				int num1 = behaviour.m_educated1Count + behaviour.m_educated2Count * 2 + behaviour.m_educated3Count * 3;
				int num2 = behaviour.m_teenCount + behaviour.m_youngCount * 2 + behaviour.m_adultCount * 3 + behaviour.m_seniorCount * 3;
				int averageEducation;
				ItemClass.Level level1;
				int educationProgress;
				if (num2 != 0) {
					averageEducation = (num1 * 300 + (num2 >> 1)) / num2;
					int num3 = (num1 * 72 + (num2 >> 1)) / num2;
					if (num3 < 15) {
						level1 = ItemClass.Level.Level1;
						educationProgress = 1 + num3;
					} else if (num3 < 30) {
						level1 = ItemClass.Level.Level2;
						educationProgress = 1 + (num3 - 15);
					} else if (num3 < 45) {
						level1 = ItemClass.Level.Level3;
						educationProgress = 1 + (num3 - 30);
					} else if (num3 < 60) {
						level1 = ItemClass.Level.Level4;
						educationProgress = 1 + (num3 - 45);
					} else {
						level1 = ItemClass.Level.Level5;
						educationProgress = 1;
					}
					if (level1 < buildingData.Info.m_class.m_level)
						educationProgress = 1;
					else if (level1 > buildingData.Info.m_class.m_level)
						educationProgress = 15;
				} else {
					level1 = ItemClass.Level.Level1;
					averageEducation = 0;
					educationProgress = 0;
				}
				//Debug.Log ("educationProgress is " + educationProgress);
				int local;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource (ImmaterialResourceManager.Resource.LandValue, buildingData.m_position, out local);
				ItemClass.Level level2;
				int landValueProgress;
				if (educationProgress != 0) {
					if (local < 6) {
						level2 = ItemClass.Level.Level1;
						landValueProgress = 1 + (local * 15 + 3) / 6;
					} else if (local < 21) {
						level2 = ItemClass.Level.Level2;
						landValueProgress = 1 + ((local - 6) * 15 + 7) / 15;
					} else if (local < 41) {
						level2 = ItemClass.Level.Level3;
						landValueProgress = 1 + ((local - 21) * 15 + 10) / 20;
					} else if (local < 61) {
						level2 = ItemClass.Level.Level4;
						landValueProgress = 1 + ((local - 41) * 15 + 10) / 20;
					} else {
						level2 = ItemClass.Level.Level5;
						landValueProgress = 1;
					}
					if (level2 < buildingData.Info.m_class.m_level)
						landValueProgress = 1;
					else if (level2 > buildingData.Info.m_class.m_level)
						landValueProgress = 15;
				} else {
					level2 = ItemClass.Level.Level1;
					landValueProgress = 0;
				}
				//Debug.Log ("landValueProgress is " + landValueProgress);
				bool landValueTooLow = false;
				if (buildingData.Info.m_class.m_level == ItemClass.Level.Level2) {
					if (local == 0)
						landValueTooLow = true;
				} else if (buildingData.Info.m_class.m_level == ItemClass.Level.Level3) {
					if (local < 11)
						landValueTooLow = true;
				} else if (buildingData.Info.m_class.m_level == ItemClass.Level.Level4) {
					if (local < 31)
						landValueTooLow = true;
				} else if (buildingData.Info.m_class.m_level == ItemClass.Level.Level5 && local < 51)
					landValueTooLow = true;
				ItemClass.Level targetLevel = (ItemClass.Level)Mathf.Min ((int)level1, (int)level2);
				Singleton<BuildingManager>.instance.m_LevelUpWrapper.OnCalculateResidentialLevelUp (ref targetLevel, ref educationProgress, ref landValueProgress, ref landValueTooLow, averageEducation, local, buildingID, buildingData.Info.m_class.m_service,  buildingData.Info.m_class.m_subService,  buildingData.Info.m_class.m_level);
				if (landValueTooLow) {
					buildingData.m_serviceProblemTimer = (byte)Mathf.Min ((int)byte.MaxValue, (int)buildingData.m_serviceProblemTimer + 1);
					buildingData.m_problems = (int)buildingData.m_serviceProblemTimer < 8 ? ((int)buildingData.m_serviceProblemTimer < 4 ? Notification.RemoveProblems (buildingData.m_problems, Notification.Problem.LandValueLow) : Notification.AddProblems (buildingData.m_problems, Notification.Problem.LandValueLow)) : Notification.AddProblems (buildingData.m_problems, Notification.Problem.LandValueLow | Notification.Problem.MajorProblem);
				} else {
					buildingData.m_serviceProblemTimer = (byte)0;
					buildingData.m_problems = Notification.RemoveProblems (buildingData.m_problems, Notification.Problem.LandValueLow);
				}
				//Debug.Log ("Target Level is " + targetLevel);

			ItemClass.Level maxlevelclass = ItemClass.Level.Level1;
			if (m_levelmax == 1) {
				maxlevelclass = ItemClass.Level.Level1;
			}
			if (m_levelmax == 2) {
				maxlevelclass = ItemClass.Level.Level2;
			}
			if (m_levelmax == 3) {
				maxlevelclass = ItemClass.Level.Level3;
			}
			if (m_levelmax == 4) {
				maxlevelclass = ItemClass.Level.Level4;
			}
			if (m_levelmax == 5) {
				maxlevelclass = ItemClass.Level.Level5;
			}

			if (maxlevelclass == targetLevel) {
				educationProgress = 0;
				landValueProgress = 0;
			}

				if (targetLevel >  buildingData.Info.m_class.m_level) 
				{
					Debug.Log ("educationProgress on level is " + educationProgress);
					educationProgress = 0;
					Debug.Log ("landValueProgress on level is " + landValueProgress);
					landValueProgress = 0;
					if (buildingData.Info.m_class.m_subService == ItemClass.SubService.ResidentialHigh && (cityPlanning & DistrictPolicies.CityPlanning.HighriseBan) != DistrictPolicies.CityPlanning.None && targetLevel == ItemClass.Level.Level5) {
						instance.m_districts.m_buffer [(int)district].m_cityPlanningPoliciesEffect |= DistrictPolicies.CityPlanning.HighriseBan;
						targetLevel = ItemClass.Level.Level4;
						educationProgress = 1;
					}
					if (buildingData.m_problems == Notification.Problem.None && targetLevel > buildingData.Info.m_class.m_level && GetUpgradeInfo (buildingID, ref buildingData) != null) {
						frameData.m_constructState = (byte)0;
						StartUpgrading (buildingID, ref buildingData);
					}
				}
				//Debug.Log ("Progress is " + (byte)(educationProgress | landValueProgress << 4));
				buildingData.m_levelUpProgress = (byte)(educationProgress | landValueProgress << 4);

		}
	
	public void LevelUp(ushort buildingID, ref Building buildingData, ItemClass.Level targetLevel)
	{
			if (targetLevel > buildingData.Info.m_class.m_level && this.GetUpgradeInfo(buildingID, ref buildingData) != null)
		{
			StartUpgrading(buildingID, ref buildingData);
		}
	}
	private TransferManager.TransferReason GetHomeReason(ushort buildingID, ref Building buildingData, ref Randomizer r)
	{
		if (this.m_info.m_class.m_subService == ItemClass.SubService.ResidentialLow == (r.Int32(10u) != 0))
		{
			switch (this.m_info.m_class.m_level)
			{
			case ItemClass.Level.Level1:
				return TransferManager.TransferReason.Family0;
			case ItemClass.Level.Level2:
				return TransferManager.TransferReason.Family1;
			case ItemClass.Level.Level3:
				return TransferManager.TransferReason.Family2;
			case ItemClass.Level.Level4:
				return TransferManager.TransferReason.Family3;
			case ItemClass.Level.Level5:
				return TransferManager.TransferReason.Family3;
			default:
				return TransferManager.TransferReason.Family0;
			}
		}
		else if (r.Int32(2u) == 0)
		{
			switch (this.m_info.m_class.m_level)
			{
			case ItemClass.Level.Level1:
				return TransferManager.TransferReason.Single0;
			case ItemClass.Level.Level2:
				return TransferManager.TransferReason.Single1;
			case ItemClass.Level.Level3:
				return TransferManager.TransferReason.Single2;
			case ItemClass.Level.Level4:
				return TransferManager.TransferReason.Single3;
			case ItemClass.Level.Level5:
				return TransferManager.TransferReason.Single3;
			default:
				return TransferManager.TransferReason.Single0;
			}
		}
		else
		{
			switch (this.m_info.m_class.m_level)
			{
			case ItemClass.Level.Level1:
				return TransferManager.TransferReason.Single0B;
			case ItemClass.Level.Level2:
				return TransferManager.TransferReason.Single1B;
			case ItemClass.Level.Level3:
				return TransferManager.TransferReason.Single2B;
			case ItemClass.Level.Level4:
				return TransferManager.TransferReason.Single3B;
			case ItemClass.Level.Level5:
				return TransferManager.TransferReason.Single3B;
			default:
				return TransferManager.TransferReason.Single0B;
			}
		}
	}
	private static int GetAverageResidentRequirement(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource)
	{
		CitizenManager instance = Singleton<CitizenManager>.instance;
		uint num = data.m_citizenUnits;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num != 0u)
		{
			uint nextUnit = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
			if ((ushort)(instance.m_units.m_buffer[(int)((UIntPtr)num)].m_flags & CitizenUnit.Flags.Home) != 0)
			{
				int num5 = 0;
				int num6 = 0;
				for (int i = 0; i < 5; i++)
				{
					uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
					if (citizen != 0u && !instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].Dead)
					{
						num5 += GetResidentRequirement(resource, ref instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)]);
						num6++;
					}
				}
				if (num6 == 0)
				{
					num3 += 100;
					num4++;
				}
				else
				{
					num3 += num5;
					num4 += num6;
				}
			}
			num = nextUnit;
			if (++num2 > 524288)
			{
				CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
				break;
			}
		}
		if (num4 != 0)
		{
			return (num3 + (num4 >> 1)) / num4;
		}
		return 0;
	}
	private static int GetResidentRequirement(ImmaterialResourceManager.Resource resource, ref Citizen citizen)
	{
		switch (resource)
		{
		case ImmaterialResourceManager.Resource.HealthCare:
			return Citizen.GetHealthCareRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
		case ImmaterialResourceManager.Resource.FireDepartment:
			return Citizen.GetFireDepartmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
		case ImmaterialResourceManager.Resource.PoliceDepartment:
			return Citizen.GetPoliceDepartmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
		case ImmaterialResourceManager.Resource.EducationElementary:
			{
				Citizen.AgePhase agePhase = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
				if (agePhase < Citizen.AgePhase.Teen0)
				{
					return Citizen.GetEducationRequirement(agePhase);
				}
				return 0;
			}
		case ImmaterialResourceManager.Resource.EducationHighSchool:
			{
				Citizen.AgePhase agePhase2 = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
				if (agePhase2 >= Citizen.AgePhase.Teen0 && agePhase2 < Citizen.AgePhase.Young0)
				{
					return Citizen.GetEducationRequirement(agePhase2);
				}
				return 0;
			}
		case ImmaterialResourceManager.Resource.EducationUniversity:
			{
				Citizen.AgePhase agePhase3 = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
				if (agePhase3 >= Citizen.AgePhase.Young0)
				{
					return Citizen.GetEducationRequirement(agePhase3);
				}
				return 0;
			}
		case ImmaterialResourceManager.Resource.DeathCare:
			return Citizen.GetDeathCareRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
		case ImmaterialResourceManager.Resource.PublicTransport:
			return Citizen.GetTransportRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
		case ImmaterialResourceManager.Resource.Entertainment:
			return Citizen.GetEntertainmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
		}
		return 100;
	}
	public override float GetEventImpact(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource, float amount)
	{
		if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None)
		{
			return 0f;
		}
		switch (resource)
		{
		case ImmaterialResourceManager.Resource.HealthCare:
			{
				int averageResidentRequirement = GetAverageResidentRequirement(buildingID, ref data, resource);
				int num;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num);
				int num2 = ImmaterialResourceManager.CalculateResourceEffect(num, averageResidentRequirement, 500, 20, 40);
				int num3 = ImmaterialResourceManager.CalculateResourceEffect(num + Mathf.RoundToInt(amount), averageResidentRequirement, 500, 20, 40);
				return Mathf.Clamp((float)(num3 - num2) / 20f, -1f, 1f);
			}
		case ImmaterialResourceManager.Resource.FireDepartment:
			{
				int averageResidentRequirement2 = GetAverageResidentRequirement(buildingID, ref data, resource);
				int num4;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num4);
				int num5 = ImmaterialResourceManager.CalculateResourceEffect(num4, averageResidentRequirement2, 500, 20, 40);
				int num6 = ImmaterialResourceManager.CalculateResourceEffect(num4 + Mathf.RoundToInt(amount), averageResidentRequirement2, 500, 20, 40);
				return Mathf.Clamp((float)(num6 - num5) / 20f, -1f, 1f);
			}
		case ImmaterialResourceManager.Resource.PoliceDepartment:
			{
				int averageResidentRequirement3 = GetAverageResidentRequirement(buildingID, ref data, resource);
				int num7;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num7);
				int num8 = ImmaterialResourceManager.CalculateResourceEffect(num7, averageResidentRequirement3, 500, 20, 40);
				int num9 = ImmaterialResourceManager.CalculateResourceEffect(num7 + Mathf.RoundToInt(amount), averageResidentRequirement3, 500, 20, 40);
				return Mathf.Clamp((float)(num9 - num8) / 20f, -1f, 1f);
			}
		case ImmaterialResourceManager.Resource.EducationElementary:
		case ImmaterialResourceManager.Resource.EducationHighSchool:
		case ImmaterialResourceManager.Resource.EducationUniversity:
			{
				int averageResidentRequirement4 = GetAverageResidentRequirement(buildingID, ref data, resource);
				int num10;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num10);
				int num11 = ImmaterialResourceManager.CalculateResourceEffect(num10, averageResidentRequirement4, 500, 20, 40);
				int num12 = ImmaterialResourceManager.CalculateResourceEffect(num10 + Mathf.RoundToInt(amount), averageResidentRequirement4, 500, 20, 40);
				return Mathf.Clamp((float)(num12 - num11) / 20f, -1f, 1f);
			}
		case ImmaterialResourceManager.Resource.DeathCare:
			{
				int averageResidentRequirement5 = GetAverageResidentRequirement(buildingID, ref data, resource);
				int num13;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num13);
				int num14 = ImmaterialResourceManager.CalculateResourceEffect(num13, averageResidentRequirement5, 500, 10, 20);
				int num15 = ImmaterialResourceManager.CalculateResourceEffect(num13 + Mathf.RoundToInt(amount), averageResidentRequirement5, 500, 10, 20);
				return Mathf.Clamp((float)(num15 - num14) / 20f, -1f, 1f);
			}
		case ImmaterialResourceManager.Resource.PublicTransport:
			{
				int averageResidentRequirement6 = GetAverageResidentRequirement(buildingID, ref data, resource);
				int num16;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num16);
				int num17 = ImmaterialResourceManager.CalculateResourceEffect(num16, averageResidentRequirement6, 500, 20, 40);
				int num18 = ImmaterialResourceManager.CalculateResourceEffect(num16 + Mathf.RoundToInt(amount), averageResidentRequirement6, 500, 20, 40);
				return Mathf.Clamp((float)(num18 - num17) / 20f, -1f, 1f);
			}
		case ImmaterialResourceManager.Resource.NoisePollution:
			{
				int num19;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num19);
				int num20 = num19 * 100 / 255;
				int num21 = Mathf.Clamp(num19 + Mathf.RoundToInt(amount), 0, 255) * 100 / 255;
				return Mathf.Clamp((float)(num21 - num20) / 50f, -1f, 1f);
			}
		case ImmaterialResourceManager.Resource.Entertainment:
			{
				int averageResidentRequirement7 = GetAverageResidentRequirement(buildingID, ref data, resource);
				int num22;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num22);
				int num23 = ImmaterialResourceManager.CalculateResourceEffect(num22, averageResidentRequirement7, 500, 30, 60);
				int num24 = ImmaterialResourceManager.CalculateResourceEffect(num22 + Mathf.RoundToInt(amount), averageResidentRequirement7, 500, 30, 60);
				return Mathf.Clamp((float)(num24 - num23) / 30f, -1f, 1f);
			}
		case ImmaterialResourceManager.Resource.Abandonment:
			{
				int num25;
				Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out num25);
				int num26 = ImmaterialResourceManager.CalculateResourceEffect(num25, 15, 50, 10, 20);
				int num27 = ImmaterialResourceManager.CalculateResourceEffect(num25 + Mathf.RoundToInt(amount), 15, 50, 10, 20);
				return Mathf.Clamp((float)(num27 - num26) / 50f, -1f, 1f);
			}
		}
		return base.GetEventImpact(buildingID, ref data, resource, amount);
	}
	public override float GetEventImpact(ushort buildingID, ref Building data, NaturalResourceManager.Resource resource, float amount)
	{
		if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None)
		{
			return 0f;
		}
		if (resource != NaturalResourceManager.Resource.Pollution)
		{
			return base.GetEventImpact(buildingID, ref data, resource, amount);
		}
		byte b;
		Singleton<NaturalResourceManager>.instance.CheckPollution(data.m_position, out b);
		int num = (int)(b * 100 / 255);
		int num2 = Mathf.Clamp((int)b + Mathf.RoundToInt(amount), 0, 255) * 100 / 255;
		return Mathf.Clamp((float)(num2 - num) / 50f, -1f, 1f);
	}
	public int CalculateHomeCount(Randomizer r, int width, int length)
	{
			width = m_households;
			length = 1;

			int num = 100;

			return Mathf.Max(100, width * length * num + r.Int32(100u)) / 100;
	}
	public  int CalculateVisitplaceCount(Randomizer r, int width, int length)
	{
		return 0;
	}
	public void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
	{
		level0 = 0;
		level1 = 0;
		level2 = 0;
		level3 = 0;
	}
	public  int CalculateProductionCapacity(Randomizer r, int width, int length)
	{
		return 0;
	}
	public  void GetConsumptionRates(Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation)
	{
		ItemClass @class = this.m_info.m_class;
		electricityConsumption = 0;
		waterConsumption = 0;
		sewageAccumulation = 0;
		garbageAccumulation = 0;
		incomeAccumulation = 0;
		if (@class.m_subService == ItemClass.SubService.ResidentialLow)
		{
			switch (@class.m_level)
			{
			case ItemClass.Level.Level1:
				electricityConsumption = 60;
				waterConsumption = 120;
				sewageAccumulation = 120;
				garbageAccumulation = 100;
				incomeAccumulation = 120;
				break;
			case ItemClass.Level.Level2:
				electricityConsumption = 60;
				waterConsumption = 110;
				sewageAccumulation = 110;
				garbageAccumulation = 100;
				incomeAccumulation = 160;
				break;
			case ItemClass.Level.Level3:
				electricityConsumption = 60;
				waterConsumption = 100;
				sewageAccumulation = 100;
				garbageAccumulation = 60;
				incomeAccumulation = 240;
				break;
			case ItemClass.Level.Level4:
				electricityConsumption = 60;
				waterConsumption = 90;
				sewageAccumulation = 90;
				garbageAccumulation = 40;
				incomeAccumulation = 360;
				break;
			case ItemClass.Level.Level5:
				electricityConsumption = 60;
				waterConsumption = 80;
				sewageAccumulation = 80;
				garbageAccumulation = 30;
				incomeAccumulation = 450;
				break;
			}
		}
		else
		{
			switch (@class.m_level)
			{
			case ItemClass.Level.Level1:
				electricityConsumption = 30;
				waterConsumption = 60;
				sewageAccumulation = 60;
				garbageAccumulation = 70;
				incomeAccumulation = 70;
				break;
			case ItemClass.Level.Level2:
				electricityConsumption = 28;
				waterConsumption = 55;
				sewageAccumulation = 55;
				garbageAccumulation = 70;
				incomeAccumulation = 100;
				break;
			case ItemClass.Level.Level3:
				electricityConsumption = 26;
				waterConsumption = 50;
				sewageAccumulation = 50;
				garbageAccumulation = 40;
				incomeAccumulation = 130;
				break;
			case ItemClass.Level.Level4:
				electricityConsumption = 24;
				waterConsumption = 45;
				sewageAccumulation = 45;
				garbageAccumulation = 30;
				incomeAccumulation = 160;
				break;
			case ItemClass.Level.Level5:
				electricityConsumption = 22;
				waterConsumption = 40;
				sewageAccumulation = 40;
				garbageAccumulation = 20;
				incomeAccumulation = 200;
				break;
			}
		}
		if (electricityConsumption != 0)
		{
			electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption + r.Int32(100u)) / 100;
		}
		if (waterConsumption != 0)
		{
			int num = r.Int32(100u);
			waterConsumption = Mathf.Max(100, productionRate * waterConsumption + num) / 100;
			if (sewageAccumulation != 0)
			{
				sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + num) / 100;
			}
		}
		else if (sewageAccumulation != 0)
		{
			sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + r.Int32(100u)) / 100;
		}
		if (garbageAccumulation != 0)
		{
			garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation + r.Int32(100u)) / 100;
		}
		if (incomeAccumulation != 0)
		{
			incomeAccumulation = productionRate * incomeAccumulation;
		}
	}
	public void GetPollutionRates(int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
	{
		groundPollution = 0;
		noisePollution = 0;
	}
	public override string GenerateName(ushort buildingID, InstanceID caller)
	{
			return m_name;
	}
		public override int GetConstructionCost()
		{
			int result = (m_constructionCost * 100);
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return result;
		}



}
}