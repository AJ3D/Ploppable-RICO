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

		public override int GetConstructionCost()
		{
			int result = (m_constructionCost * 100);
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return result;
		}


		public override void CalculateWorkplaceCount (Randomizer r, int width, int length, out int level1,out int level2,out int level3, out int level4)
		{
			int widths = (m_housemulti + (m_housemulti / 2) );

			base.CalculateWorkplaceCount (r, widths, 1 ,out level1,out level2,out level3, out level4);
		}


		public override void SimulationStep (ushort buildingID, ref Building data)
		{

			BuildingData[] dataArray = BuildingDataManager.buildingData;		
			Bdata = dataArray [(int)buildingID];

			if (Bdata == null) {

				Bdata = new BuildingData ();
				dataArray [(int)buildingID] = Bdata;
				Bdata.level = m_levelmin;
				Bdata.Name = data.Info.name;
				Bdata.saveflag = false;
			}


			if (Bdata.saveflag == false){

				if (Bdata.level == 2) {
					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level2");
				}
				if (Bdata.level == 3) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level3");
				}

				Bdata.saveflag = true;
			}

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