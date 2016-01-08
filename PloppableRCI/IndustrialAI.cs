using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableRICO
{

	public class PloppableIndustrial : IndustrialBuildingAI

	{

		public int m_levelmin = 1;

		public int m_levelmax = 1;

		public int BID = 2;
		int Tester = 0;
		private int timer = 0;
		string OriginalName;

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

		public override int CalculateHomeCount (Randomizer r, int width, int length)
		{
			//width = width * 2
			//length = length * 2 ;
			return base.CalculateHomeCount (r, width, length);
		}

		public override void SimulationStep (ushort buildingID, ref Building data)

		{
			//Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, 500, 500, data.m_position, this.m_pollutionRadius);
			data.UpdateBuilding ((ushort)data.m_buildIndex);

			if (timer == 0) {
				OriginalName = data.Info.name; 
				timer = 1;
				//data.Info.name = OriginalName + "_Level1";
			}

			if (data.Info.m_class.m_service != ItemClass.Service.Industrial) {

					data.Info = PrefabCollection<BuildingInfo>.FindLoaded (OriginalName + "_Level1");

			}

			data.m_garbageBuffer = 0;
			data.m_fireHazard = 0;
			data.m_fireIntensity = 0;
			data.m_majorProblemTimer = 0;

			data.m_problems = Notification.Problem.None;
			data.m_flags = Building.Flags.None;
			data.m_flags |= Building.Flags.Active;
			data.m_flags |= Building.Flags.Created;
			data.m_flags |= Building.Flags.Completed;
			data.m_flags |= Building.Flags.FixedHeight;

			base.SimulationStep(buildingID, ref data);
	
			data.m_problems = Notification.Problem.None;
			data.m_flags = Building.Flags.None;
			data.m_flags |= Building.Flags.Active;
			data.m_flags |= Building.Flags.Created;
			data.m_flags |= Building.Flags.FixedHeight;
			data.m_flags |= Building.Flags.Completed;

		}
			
	}
}