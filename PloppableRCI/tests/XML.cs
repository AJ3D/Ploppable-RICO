using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;


namespace PloppableRICOTests
{
    [TestFixture]
    
    public class XMLTests
    {
        string testDirectoryTmpl;


        [SetUp]
        public void Init()
        {
            var dll_path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var tst_path = Path.Combine(new FileInfo(dll_path).Directory.FullName, "..", "..", "Testfiles");
            testDirectoryTmpl = Path.Combine(tst_path, "{0}.xml");
        }
        
        [TestCase("ok")]
        [TestCase("ok_rnd")]
        public void testOK(string tst)
        {
            List<String> ricoErr = new List<string>();
            PloppableRICO.PloppableRICODefinition ricoDef = null;

            try
            {
                ricoErr = new List<String>();
                ricoDef = PloppableRICO.RICOReader.ParseRICODefinition(tst, String.Format(testDirectoryTmpl, tst), ricoErr);

                if (ricoDef != null)
                    Console.WriteLine(ricoDef.Buildings.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("!! Unexpected Exception1: " + ex.Message + " !!");
                Assert.True(false);
            }

            foreach (var s in ricoErr)
                Console.WriteLine("!" + s);

            Assert.True(ricoErr.Count == 0);
        }

        [TestCase("invalid_xml")]
        [TestCase("root_empty")]
        [TestCase("empty")]
        [TestCase("unknown_xml")]
        [TestCase("buildings_empty")]
        [TestCase("single_empty_building")]
        [TestCase("req_str_attr_empty")]
        [TestCase("req_int_attr_empty")]
        [TestCase("zero_jobs")]
        public void testFatalities(string tst)
        {
            List<String> ricoErr = new List<string>();
            PloppableRICO.PloppableRICODefinition ricoDef = null;

            try
            {
                ricoErr = new List<String>();
                ricoDef = PloppableRICO.RICOReader.ParseRICODefinition(tst, String.Format(testDirectoryTmpl, tst), ricoErr);

                if (ricoDef != null) 
                    Console.WriteLine(ricoDef.Buildings.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("!! Unexpected Exception2: " + ex.Message + " !!");
                Assert.True(false);
            }

            foreach (var s in ricoErr)
                Console.WriteLine("!" + s);

            Assert.True(ricoErr.Count > 0);
        }

    }
}
