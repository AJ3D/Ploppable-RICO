using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableAI
{

	public class PloppableOffice : OfficeBuildingAI

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


		public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			base.GetWidthRange (out minWidth, out maxWidth);
			minWidth = 1;
			maxWidth = 16;
		}

		public override void GetLengthRange (out int minLength, out int maxLength)
		{
			base.GetLengthRange (out minLength, out maxLength);
			minLength = 1;
			maxLength = 16;
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
			DebugOutputPanel.AddMessage (PluginManager.MessageType.Message,"this is output"); 
			string stuff = Convert.ToString(this.m_constructionCost);
			DebugOutputPanel.AddMessage (PluginManager.MessageType.Message,"this is output");

			//When this first runs on scene load or after scene save, all Building objects will have the original BuildingInfo object assigned. 
			//We read the custombuffer1 slot in the building object to reassign the appropriate instanciated BuildingInfos. 
			//ex. 0 = Level 1, 2 = level2, 3 = level 3
			//The orignal BuildingInfo is set to Monument service. This part will only run once. 


			if (data.Info.m_class.m_service == ItemClass.Service.Monument){

				OriginalN = data.Info.name; 

				if (data.m_customBuffer1 == 0) {
					OriginalN = data.Info.name;
					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (OriginalN + "_Level1");
					data.Info.m_class = new ItemClass ();
					data.Info.m_class.m_service = ItemClass.Service.Office;
				}

				if (data.m_customBuffer1 == 2) {
					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (OriginalN + "_Level2");
					data.Info.m_class = new ItemClass (); 
					data.Info.m_class.m_service = ItemClass.Service.Office;
					data.Info.m_class.m_level = ItemClass.Level.Level2;
				}
				if (data.m_customBuffer1 == 3) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (OriginalN + "_Level3");
					data.Info.m_class = new ItemClass ();
					data.Info.m_class.m_service = ItemClass.Service.Office;
					data.Info.m_class.m_level = ItemClass.Level.Level3;
				}
			}


			//These allow the building to survive. The ZonesUpdated flag is super important. 

			data.m_flags &= ~Building.Flags.ZonesUpdated;
			data.m_problems = Notification.Problem.None;
			data.m_flags &= ~Building.Flags.Abandoned;

			//This is the leveling logic. It test the level up progress, and assigns the approprate level to the new building. 255 is the normal mark. Its been set lower for testing. 
			//It probably needs to happen at 254. If you let it go all the way, the regular leveling logic will kick in, and destroy the lot and try to spawn a new one. 

			if (data.m_levelUpProgress >= 249) {

				if (data.m_customBuffer1 == 0) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (OriginalN + "_Level2");
					data.Info.m_class = new ItemClass ();
					data.Info.m_class.m_service = ItemClass.Service.Office;
					data.Info.m_class.m_level = ItemClass.Level.Level3;
					data.m_customBuffer1 = 2;

				} else if (data.m_customBuffer1 == 2) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (OriginalN + "_Level3");
					data.Info.m_class = new ItemClass ();
					data.Info.m_class.m_service = ItemClass.Service.Office;
					data.Info.m_class.m_level = ItemClass.Level.Level3;
					data.m_customBuffer1 = 3;

				}

				data.m_levelUpProgress = 240; //once leveled, we set the building back to normal level up progress. 
			}


			base.SimulationStep(buildingID, ref data);


		}

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}



	}
}