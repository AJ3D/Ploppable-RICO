using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableAI
{

	public class Industrial : IndustrialBuildingAI

	{
		[CustomizableProperty("Level Min", "Gameplay common")]
		public int m_levelmin = 1;
		[CustomizableProperty("Level Max", "Gameplay common")]
		public int m_levelmax = 1;
		[CustomizableProperty("Maintenance Cost", "Gameplay common")]
		public int m_maintenanceCost = 100;
		[CustomizableProperty("Construction Cost", "Gameplay common")]
		public int m_constructionCost = 100;
		public int BID = 2;
		int Tester = 1;
		private int timer = 1;
		string OriginalN;
		float m_pollutionRadius = 400f;


		public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			base.GetWidthRange (out minWidth, out maxWidth);
			minWidth = 1;
			maxWidth = 255;
		}
		public override void GetDecorationArea (out int width, out int length, out float offset)
		{
			base.GetDecorationArea (out width, out length, out offset);
			width = this.m_info.m_cellWidth;
			length = this.m_info.m_cellLength;
			offset = 0f;
		}

		public override void GetLengthRange (out int minLength, out int maxLength)
		{
			base.GetLengthRange (out minLength, out maxLength);
			minLength = 1;
			maxLength = 255;
		}
		public override int  GetMaintenanceCost ()
		{
			int result = this.m_maintenanceCost * 100;
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetMaintenanceCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return result;
			//return base.GetMaintenanceCost ();
		}
		public override int CalculateHomeCount (Randomizer r, int width, int length)
		{
			//width = width * 2
			//length = length * 2 ;
			return base.CalculateHomeCount (r, width, length);
		}
		public override int GetConstructionCost()
		{
			int result = this.m_constructionCost * 100;
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return result;
		}

		public override void SimulationStep (ushort buildingID, ref Building data)

		{
			Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, 500, 500, data.m_position, this.m_pollutionRadius);

			data.Info.m_class = new ItemClass ();
			data.Info.m_class.m_service = ItemClass.Service.Industrial;
			data.Info.m_class.m_subService = ItemClass.SubService.IndustrialGeneric;

			data.m_flags &= ~Building.Flags.ZonesUpdated;
			data.m_problems = Notification.Problem.None;
			data.m_flags &= ~Building.Flags.Abandoned;

			base.SimulationStep(buildingID, ref data);

		}

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}
			
	}
}