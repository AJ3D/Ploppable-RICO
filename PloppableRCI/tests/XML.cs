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

        [TestCase( "ok" )]
        [TestCase( "ok_rnd" )]
        [TestCase( "ok old" )]
        public void testOK( string tst )
        {
            PloppableRICO.PloppableRICODefinition ricoDef = null;

            try
            {
                ricoDef = PloppableRICO.RICOReader.ParseRICODefinition( tst, String.Format( testDirectoryTmpl, tst ) );

                if ( ricoDef != null )
                    Console.WriteLine( ricoDef.Buildings.Count );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "!! Unexpected Exception1: " + ex.Message + " !!" );
                Assert.True( false );
            }

            if ( ricoDef == null )
                Assert.True( false );
            else
            {
                foreach ( var s in ricoDef.errors )
                    Console.WriteLine( "!" + s );

                Assert.True( ricoDef.errors.Count == 0 );
            }
        }

        [TestCase("invalid_xml")]
        [TestCase("root_empty")]
        [TestCase("empty")]
        [TestCase("unknown_xml")]
        [TestCase("buildings_empty")]
        [TestCase("single_empty_building")]
        [TestCase("req_str_attr_empty")]
        [TestCase("req_int_attr_empty")]
        [TestCase( "zero_jobs" )]
        [TestCase( "broken_wp" )]
        public void testFatalities(string tst)
        {
            PloppableRICO.PloppableRICODefinition ricoDef = null;

            try
            {
                ricoDef = PloppableRICO.RICOReader.ParseRICODefinition(tst, String.Format(testDirectoryTmpl, tst),true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("!! Unexpected Exception2: " + ex.Message + " !!");
                Assert.True(false);
            }

            Assert.True( PloppableRICO.RICOReader.LastErrors.Count > 0 || ricoDef.errors.Count > 0);
        }

        [Test]
        public void testDirtiness1()
        {
            PloppableRICO.PloppableRICODefinition ricoDef = null;

            try
            {
                ricoDef = PloppableRICO.RICOReader.ParseRICODefinition( "ok", String.Format( testDirectoryTmpl, "ok" ), true );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "!! Unexpected Exception2: " + ex.Message + " !!" );
                Assert.True( false );
            }

            Assert.True( !ricoDef.isDirty );
        }

        [Test]
        public void testDirtiness2()
        {
            PloppableRICO.PloppableRICODefinition ricoDef = null;

            try
            {
                ricoDef = PloppableRICO.RICOReader.ParseRICODefinition( "ok", String.Format( testDirectoryTmpl, "ok" ), true );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "!! Unexpected Exception2: " + ex.Message + " !!" );
                Assert.True( false );
            }
            ricoDef.Buildings[0].workplaceDeviation[1] = 42;
            Assert.True( ricoDef.isDirty );
        }
    }
}
