using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableRICO
{

	public class PloppableResidential : ResidentialBuildingAI
	{
		public int m_levelmin = 1;
		public int m_levelmax = 1;
		public int m_constructionCost = 1;
		public int m_households = 1;

	

		BuildingData Bdata;



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

		public override int CalculateHomeCount (Randomizer r, int width, int length)
		{
			width = m_households;
			length = 1;

			int num = 100;

			return Mathf.Max(100, width * length * num + r.Int32(100u)) / 100;
		}

		public override void SimulationStep (ushort buildingID, ref Building data)
		{
			data.UpdateBuilding ((ushort)buildingID);
		

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
				if (Bdata.level == 4) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level4");
				}
				if (Bdata.level == 5) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level5");
				}
				Bdata.saveflag = true;
			}

			if (m_levelmax == Bdata.level) { ///If Its reached max level, then dont level up. 
				data.m_levelUpProgress = 240;
			}
				
			if (data.m_levelUpProgress >= 253) { //254 is normal

				if (Bdata.level == 1) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level2");
					Bdata.level = 2;

				} else if (Bdata.level == 2) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level3");
					Bdata.level = 3;

				} else if (Bdata.level == 3) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level4");
					Bdata.level = 4;

				} else if (Bdata.level == 4) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (Bdata.Name + "_Level5");
					Bdata.level = 5;

				}
				data.m_levelUpProgress = 240; //once leveled, set the building back to normal level up progress. 
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

			this.m_info.m_autoRemove = false;

			base.SimulationStep (buildingID, ref data);

			//data.m_problems = Notification.Problem.None;
			data.m_flags = Building.Flags.None;
			//data.m_flags |= Building.Flags.Active;
			data.m_flags |= Building.Flags.Created;
			data.m_flags |= Building.Flags.Completed;

		}

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}

	}
}