using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ColossalFramework.UI;
using ColossalFramework.DataBinding;



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

        private static RedirectCallsState _GeneratedScrollPanel_IsWonder_state;
        private static MethodInfo _GeneratedScrollPanel_IsWonder_original;
        private static MethodInfo _GeneratedScrollPanel_IsWonder_detour;

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
                /*
                _GeneratedScrollPanel_IsWonder_original = typeof(GeneratedScrollPanel).GetMethod("FilterWonders", BindingFlags.Instance |  BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(GeneratedScrollPanel.AssetFilter), typeof(BuildingInfo) },
                    null
                    );

                _GeneratedScrollPanel_IsWonder_detour = typeof(BuildingToolDetour).GetMethod("FilterWonders", BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(GeneratedScrollPanel.AssetFilter), typeof(BuildingInfo) },
                    null
                    );
             */
                _BuildingTool_CheckCollidingBuildings_state = RedirectionHelper.RedirectCalls (_BuildingTool_CheckCollidingBuildings_original, _BuildingTool_CheckCollidingBuildings_detour);

                //_GeneratedScrollPanel_IsWonder_state = RedirectionHelper.RedirectCalls(_GeneratedScrollPanel_IsWonder_original, _GeneratedScrollPanel_IsWonder_detour);
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


                //RedirectionHelper.RevertRedirect(_GeneratedScrollPanel_IsWonder_original, _GeneratedScrollPanel_IsWonder_state);
                //_GeneratedScrollPanel_IsWonder_original = null;
                //_GeneratedScrollPanel_IsWonder_detour = null;


                deployed = false;
				//Debug.Log("BuildingTool Methods restored");
			}
		}
			

		private static bool IsImportantBuilding (BuildingInfo info, ref Building building)
		{
			int publicServiceIndex = ItemClass.GetPublicServiceIndex (info.m_class.m_service);

			//This exempts RICO buildings from the wrath of the BuildingTool. 
			if (info.m_buildingAI is PloppableOffice || info.m_buildingAI is PloppableExtractor || info.m_buildingAI is PloppableResidential || info.m_buildingAI is PloppableCommercial || info.m_buildingAI is PloppableIndustrial) {
				return true;
				
			} else {
				
				return (publicServiceIndex != -1 && !info.m_autoRemove) || (building.m_flags & Building.Flags.Untouchable) != Building.Flags.None || building.m_fireIntensity != 0;
			}
		}

        private static bool FilterWonders(GeneratedScrollPanel.AssetFilter filter, BuildingInfo info)
        {
            //Debug.Log("Detour Worked!!!!");
    /*
            if (filter.IsFlagSet((GeneratedScrollPanel.AssetFilter)18))
            {
                if (info.m_buildingAI is PloppableResidential)
                {
                    return false;
                }
                return true;
            }
            if (filter.IsFlagSet(GeneratedScrollPanel.AssetFilter.Wonder))
            {
                if (info.m_buildingAI is PloppableResidential)
                {
                    return false;
                }
                return info.m_buildingAI.IsWonder();
            }
            */
           ///return filter.IsFlagSet(GeneratedScrollPanel.AssetFilter.Building) && !info.m_buildingAI.IsWonder();
            return false;
          
        }

    }
}
