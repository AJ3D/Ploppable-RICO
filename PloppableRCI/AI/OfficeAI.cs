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
	

		public override void CalculateWorkplaceCount (Randomizer r, int width, int length, out int level1,out int level2,out int level3, out int level4)
		{
			int widths = (m_workplaceCount * 2);

			base.CalculateWorkplaceCount (r, widths , 1 ,out level1,out level2,out level3, out level4);
		}

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}

		public override void SimulationStep (ushort buildingID, ref Building data)
		{
			data.m_garbageBuffer = 0;
			data.m_fireHazard = 0;
			data.m_fireIntensity = 0;
			data.m_majorProblemTimer = 0;

			//data.m_problems = Notification.Problem.None;
			data.m_flags = Building.Flags.None;
			//data.m_flags |= Building.Flags.Active;
			data.m_flags |= Building.Flags.Created;
			data.m_flags |= Building.Flags.Completed;

			base.SimulationStep(buildingID, ref data);

			//data.m_problems = Notification.Problem.None;
			data.m_flags = Building.Flags.None;
			//data.m_flags |= Building.Flags.Active;
			data.m_flags |= Building.Flags.Created;
			data.m_flags |= Building.Flags.Completed;



		}
			
	}
}