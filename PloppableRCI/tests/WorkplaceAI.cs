using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

    namespace ClassLibrary1
{
    [TestFixture]
    class WorkplaceAI
    {
        [Test]
        public void WorkplaceDistribution1()
        {
            int level0, level1, level2, level3;

            PloppableRICO.WorkplaceAIHelper.distributeWorkplaceLevels(
                80,

                new int[] { 100, 50, 25, 15, 10 },
                new int[] { 0, 0, 0, 0 },
                out level0, out level1, out level2, out level3);
            Assert.True(level0 == 40 && level1 == 20 && level2 == 12 && level3 == 8);
        }
        
        [Test]
        public void WorkplaceDistribution3()
        {
            int level0, level1, level2, level3;

            PloppableRICO.WorkplaceAIHelper.distributeWorkplaceLevels(
                100,
                new int[] { 100, 10, 20, 30, 40 },
                new int[] { 1, 2, 3, 4 },
                out level0, out level1, out level2, out level3);
            Assert.True(
                (level0 > 8 && level0 < 12) &&
                (level1 > 17 && level1 < 23) &&
                (level2 > 26 && level2 < 34) &&
                (level3 > 35 && level3 < 45));
        }

        [Test]
        public void Dirtiness1()
        {
            var b = new PloppableRICO.RICOBuilding();
            b.workplaces[1] = 22;
            Assert.True( b.isDirty );
        }
    }
}
