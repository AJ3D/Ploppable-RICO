using System;

using System.Reflection;


namespace PloppableRICO.Detour
{

	/// <summary>
	///This detours the CheckCollidingBuildngs method in BuildingTool. Its based on boformers Larger Footprints mod. Many thanks to him for his work. 
	/// </summary>


	public class BuildingToolDetour
	{
		private static bool deployed = false;
	
		private static RedirectCallsState _BuildingTool_CheckCollidingBuildings_state;
		private static MethodInfo _BuildingTool_CheckCollidingBuildings_original;
		private static MethodInfo _BuildingTool_CheckCollidingBuildings_detour;


        public static void Deploy ()
		{

			if (!deployed) {

				_BuildingTool_CheckCollidingBuildings_original = typeof(BuildingTool).GetMethod (
					"IsImportantBuilding",
					BindingFlags.Static | BindingFlags.NonPublic,
					null,
					new Type [] {typeof(BuildingInfo), typeof(Building).MakeByRefType()}, 
					null
				);

				_BuildingTool_CheckCollidingBuildings_detour = typeof(BuildingToolDetour).GetMethod(
					"IsImportantBuilding",
					BindingFlags.Static | BindingFlags.NonPublic,
					null,
					new Type [] {typeof(BuildingInfo), typeof(Building).MakeByRefType()}, 
					null
				);
					
				_BuildingTool_CheckCollidingBuildings_state = RedirectionHelper.RedirectCalls (_BuildingTool_CheckCollidingBuildings_original, _BuildingTool_CheckCollidingBuildings_detour);


                deployed = true;

				//Debug.Log("BuildingTool Methods detoured");
			}
		}

		public static void Revert ()
		{
			
			if (deployed) {
				RedirectionHelper.RevertRedirect (_BuildingTool_CheckCollidingBuildings_original, _BuildingTool_CheckCollidingBuildings_state);
				_BuildingTool_CheckCollidingBuildings_original = null;
				_BuildingTool_CheckCollidingBuildings_detour = null;

                deployed = false;

				//Debug.Log("BuildingTool Methods restored");
			}
		}
			

		private static bool IsImportantBuilding (BuildingInfo info, ref Building building)
		{
			int publicServiceIndex = ItemClass.GetPublicServiceIndex (info.m_class.m_service);


            //This exempts RICO buildings from the wrath of the BuildingTool. 

            //var data = RICOBuildingManager.RICOInstanceData[(int)building.m_buildIndex];

            //only plopped RICO buildings are spared. 
            //if ((info.m_buildingAI is PloppableOffice || info.m_buildingAI is PloppableExtractor || info.m_buildingAI is PloppableResidential || info.m_buildingAI is PloppableCommercial || info.m_buildingAI is PloppableIndustrial) & data.plopped) {
                if (info.m_buildingAI is PloppableOffice || info.m_buildingAI is PloppableExtractor || info.m_buildingAI is PloppableResidential || info.m_buildingAI is PloppableCommercial || info.m_buildingAI is PloppableIndustrial)
                {
                    return true;

			} else {
				
				return (publicServiceIndex != -1 && !info.m_autoRemove) || (building.m_flags & Building.Flags.Untouchable) != Building.Flags.None || building.m_fireIntensity != 0;
			}
		}



	}
}
