using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;

namespace PloppableRICO
{
    public interface IWorkplaceLevelCalculator
    {
        void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3);
        void CalculateLevels(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3);
        void CalculateBaseLevels(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3);
    }

    public static class WorkplaceAIHelper
    {
        public static int GetConstructionCost(int constructionCostValue, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level)
        {
            int result = (constructionCostValue * 100);
            Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, service, subService, level);
            return result;
        }

        public static void CalculateWorkplaceCount(PloppableRICODefinition.Building ricoData, IWorkplaceLevelCalculator ai, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            SetWorkplaceLevels(out level0, out level1, out level2, out level3, 0, 0, 0, 0);
            PloppableRICODefinition.Building rc = ricoData;

            if (rc == null)
                WorkplaceAIHelper.SetWorkplaceLevels(out level0, out level1, out level2, out level3, 10, 20, 30, 40);
            else
                // reality mod is running and the xml file says ignore-reality="false"
                if (rc.useReality)
                    ai.CalculateBaseLevels(r, width, length, out level0, out level1, out level2, out level3);
                else
                {
                    if (rc.workplaceCount > 0)
                        ai.CalculateLevels(r, width, length, out level0, out level1, out level2, out level3);
                    if (rc.workplaceDetailsEnabled)
                        // this adds to the results of the usual workplaces calculation
                        WorkplaceAIHelper.SetWorkplaceLevels(out level0, out level1, out level2, out level3, rc.uneducated + level0, rc.educated + level1, rc.wellEducated + level2, rc.highEducated + level3);
                    // Comment that out and uncomment the following to ignore "workplaces" 
                    // and just use the details setting
                    // WorkplaceAIHelper.SetWorkplaceLevels(out level0, out level1, out level2, out level3, m_ricoData);
                }
        }

        public static void SetWorkplaceLevels(out int level0, out int level1, out int level2, out int level3, PloppableRICODefinition.Building ricoData)
        {
            SetWorkplaceLevels(out level0, out level1, out level2, out level3, new int[] { ricoData.uneducated, ricoData.educated, ricoData.wellEducated, ricoData.highEducated });
        }

        public static void SetWorkplaceLevels(out int level0, out int level1, out int level2, out int level3, int l0, int l1, int l2, int l3)
        {
            SetWorkplaceLevels(out level0, out level1, out level2, out level3, new int[] { l0, l1, l2, l3 });
        }

        public static void SetWorkplaceLevels(out int level0, out int level1, out int level2, out int level3, int[] values)
        {
            level0 = values[0];
            level1 = values[1];
            level2 = values[2];
            level3 = values[3];
        }
        
        public static void distributeWorkplaceLevels(Randomizer r, int[] workplaceDistribution, int widths, out int level0, out int level1, out int level2, out int level3)
        {
            int[] wd = workplaceDistribution;

            level0 = 0; level1 = 0; level2 = 0; level3 = 0;
            int num = Mathf.Max(200, widths * workplaceDistribution[0] + r.Int32(100u)) / 100;
            int num2 = wd[1] + wd[2] + wd[3] + wd[4];

            if (num <= 0)
                return;

            if (num2 != 0)
            {
                level0 = (num * wd[1] + r.Int32((uint)num2)) / num2;
                num -= level0;
            }
            num2 = wd[2] + wd[3] + wd[4];
            if (num2 != 0)
            {
                level1 = (num * wd[2] + r.Int32((uint)num2)) / num2;
                num -= level1;
            }
            num2 = wd[3] + wd[4];
            if (num2 != 0)
            {
                level2 = (num * wd[3] + r.Int32((uint)num2)) / num2;
                num -= level2;
            }
            level3 = num;
        }
    }
}
