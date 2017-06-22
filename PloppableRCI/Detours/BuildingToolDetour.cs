using ColossalFramework;
using PloppableRICO.Redirection;
using UnityEngine;
using System;



namespace PloppableRICO.Detours
{
    //Reason for detour: This will return the ID of a manualy plopped buildng.

    [TargetType(typeof(BuildingTool))]
    public static class BuildingToolDetour
    {
        public static Redirector CreateBuildingRedirector = null;

        [RedirectMethod(true)]
        private static ushort CreateBuilding(BuildingTool _this, BuildingInfo info, Vector3 position, float angle, int relocating, bool needMoney, bool fixedHeight)
        {
            Debug.Log("BuildIndex is : " + Singleton<SimulationManager>.instance.m_currentBuildIndex);

            //call original method

            try
            {
                RICOBuildingManager.AddBuilding(info, Singleton<SimulationManager>.instance.m_currentBuildIndex);
            }

            catch (Exception e)
            {
                Debug.LogException(e);
            }

           

            CreateBuildingRedirector.Revert();
            CreateBuildingAlt(_this, info, position, angle, relocating, needMoney, fixedHeight);
            CreateBuildingRedirector.Apply();

            //Add plopped building to RICOBuildingManager

            ushort num = 0;

            return num; //no clue why it needs to return a ushort, source code doesnt appear to do anything with it. 


        }

        [RedirectReverse(true)]
        private static void CreateBuildingAlt(BuildingTool _this, BuildingInfo info, Vector3 position, float angle, int relocating, bool needMoney, bool fixedHeight)
        {
            Debug.Log("CreateBuilding");
        }
    }
}
