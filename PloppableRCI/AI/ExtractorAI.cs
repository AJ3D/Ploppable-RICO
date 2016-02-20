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
		public int m_constructionCost = 1;
        public int m_workplaceCount = 1;

		public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			base.GetWidthRange (out minWidth, out maxWidth);
			minWidth = 1;
			maxWidth = 16;
		}

        public override string GenerateName(ushort buildingID, InstanceID caller)
		{
			return base.m_info.GetUncheckedLocalizedTitle ();
		}

		public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data){

			return null; //this will cause a check to fail in CheckBuildingLevel, and prevent the building form leveling. 
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
			int widths = m_workplaceCount;

			base.CalculateWorkplaceCount (r, widths, 1 ,out level0,out level1,out level2, out level3);
		}

		public override void SimulationStep (ushort buildingID, ref Building buildingData, ref Building.Frame frameData)

		{

			buildingData.m_levelUpProgress = 0;
			buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
			buildingData.m_flags &= ~Building.Flags.Abandoned;
			buildingData.m_flags &= ~Building.Flags.Demolishing;

			buildingData.m_garbageBuffer = 0;
			buildingData.m_fireHazard = 0;
			buildingData.m_fireIntensity = 0;
			buildingData.m_majorProblemTimer = 0;
			base.SimulationStep(buildingID, ref buildingData, ref frameData);
			buildingData.m_majorProblemTimer = 0;
			buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
			buildingData.m_flags &= ~Building.Flags.Abandoned;
			buildingData.m_flags &= ~Building.Flags.Demolishing;
			buildingData.m_levelUpProgress = 0;
		}

		protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData){

			buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
			buildingData.m_flags &= ~Building.Flags.Abandoned;
			buildingData.m_flags &= ~Building.Flags.Demolishing;
			buildingData.m_levelUpProgress = 0;
			buildingData.m_majorProblemTimer = 0;

			base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
			buildingData.m_majorProblemTimer = 0;
			buildingData.m_levelUpProgress = 0;
			buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
			buildingData.m_flags &= ~Building.Flags.Abandoned;
			buildingData.m_flags &= ~Building.Flags.Demolishing;

		}

	}
}