using ColossalFramework.Math;
using UnityEngine;

namespace PloppableRICO
{
    public class PloppableResidential : ResidentialBuildingAI
    {
        public int m_constructionCost = 1;
        public int m_homeCount = 1;
        public int m_pbhomeCount = 0;
        public RICOBuilding m_ricoData;

        // This is the interesting part where shit gets done
        public override int GetConstructionCost()
        {
            return WorkplaceAIHelper.GetConstructionCost(m_constructionCost, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
        }

        protected override int GetConstructionTime()
        {
            return 0;
        }

        public override int CalculateHomeCount(ItemClass.Level level, Randomizer r, int width, int length)
        {
            if (m_ricoData.useReality)
            {
                return base.CalculateHomeCount(level, r, width, length);
            }

            return m_homeCount;
        }

        // This is the boring part, just boilerplate stuff 
        public override void GetWidthRange(out int minWidth, out int maxWidth)
        {
            minWidth = 1;
            maxWidth = 16;
        }

        public override void GetLengthRange(out int minLength, out int maxLength)
        {
            minLength = 1;
            maxLength = 16;
        }

        public override string GenerateName(ushort buildingID, InstanceID caller)
        {
            return base.m_info.GetUncheckedLocalizedTitle();
        }

        public override bool ClearOccupiedZoning()
        {
            //Debug.Log("ClearOccupiedZoning Called");
            return true;
        }

        public bool ClearOccupiedZoning2(ushort id)
        {
            /*
            var data = RICOBuildingManager.RICOInstanceData[(int)id];
            Debug.Log("ClearOccupiedZoning2 Called");
            if (data.plopped)
            { 
                return true;
            }
            else return false;
            */
            return true;
        }


        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {

            //var data = RICOBuildingManager.RICOInstanceData[(int)buildingData.m_buildIndex];

            // only apply settings for plopped RICO assets. 
            Util.buildingFlags(ref buildingData);

            base.SimulationStep(buildingID, ref buildingData, ref frameData);

            Util.buildingFlags(ref buildingData);

        }

        /*
        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            var bdata = RICOBuildingManager.RICOInstanceData[(int)data.m_buildIndex];
            if (bdata.plopped) {
                data.m_frame0.m_constructState = 255;
                this.BuildingCompleted(buildingID, ref data);
            }
                 base.CreateBuilding(buildingID, ref data);
        }
        */
        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {

            //var data = RICOBuildingManager.RICOInstanceData[(int)buildingData.m_buildIndex];
            Util.buildingFlags(ref buildingData);

            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);

            //if (data.plopped) Util.buildingFlags(ref buildingData);
            Util.buildingFlags(ref buildingData);

        }

        public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {

 
                return null; //this will cause a check to fail in CheckBuildingLevel, and prevent the building form leveling

        }
    }
}