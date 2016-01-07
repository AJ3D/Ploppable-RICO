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
		[CustomizableProperty("Level Min", "Gameplay common")]
		public int m_levelmin = 1;
		[CustomizableProperty("Level Max", "Gameplay common")]
		public int m_levelmax = 1;

		[CustomizableProperty("Household Multi", "Gameplay common")]
		public int m_housemulti = 0;

		public int BID = 2;
		int Tester = 1;
		public int timer = 0;
		string OriginalN;


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
	

		public override void CalculateWorkplaceCount (Randomizer r, int width, int length, out int level1,out int level2,out int level3, out int level4)
		{
			base.CalculateWorkplaceCount (r, width + this.m_housemulti, length + this.m_housemulti,out level1,out level2,out level3, out level4);
		}
	

		public override void SimulationStep (ushort buildingID, ref Building data)
		{
			
			if (timer == 0) {
				OriginalN = data.Info.name;
			}

			data.UpdateBuilding ((ushort)data.m_buildIndex);


			data.m_flags &= ~Building.Flags.ZonesUpdated;
			data.m_problems = Notification.Problem.None;
			data.m_flags &= ~Building.Flags.Abandoned;

			//OriginalN = data.Info.name;

			data.m_levelUpProgress = 240;

			if (data.Info.m_class.m_service != ItemClass.Service.Office){

				data.Info = PrefabCollection<BuildingInfo>.FindLoaded (OriginalN + "_Level3");
		
			}




			base.SimulationStep(buildingID, ref data);


		}
			
	}
}