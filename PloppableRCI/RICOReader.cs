using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PloppableRICO
{
    public class RicoWriter
    {
        public static bool saveRicoData(string fileName, PloppableRICODefinition RicoDefinition)
        {
            try
            {
                var streamWriter = new System.IO.StreamWriter(fileName);
                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));
                xmlSerializer.Serialize(streamWriter, RicoDefinition);
                streamWriter.Close();
                return true;
            }
            catch
            { }
            return false;
        } 
    }

    public class RICOReader
    {
        private static XmlSchemaSet schemas;
        private static string xsdMarkup =
    @"
<xsd:schema xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
    <xsd:simpleType name='IntList4OrEmpty'>
        <xsd:restriction base='xsd:string'>
            <xsd:pattern value = '^(( *\d+[,;:/ -] *\d+[,;:/ -] *\d+[,;:/ -])? *\d+ *)?$' />
        </xsd:restriction>
    </xsd:simpleType>
    <xsd:simpleType name='NonEmptyString'>
        <xsd:restriction base='xsd:string'>
            <xsd:minLength value = '1' />
        </xsd:restriction>
    </xsd:simpleType>
    <xsd:element name = 'PloppableRICODefinition'>
        <xsd:complexType>
            <xsd:choice minOccurs = '1' maxOccurs='1'>
                <xsd:element name = 'Buildings' >
                    <xsd:complexType>
                        <xsd:sequence>
                            <xsd:element name = 'Building' minOccurs='1' maxOccurs='unbounded'>
                                <xsd:complexType>
                                <xsd:simpleContent>
                                <xsd:extension base='xsd:string'>
                                    <xsd:attribute name = 'name' type='NonEmptyString' />
                                    <xsd:attribute name = 'service' type='NonEmptyString' use='required' />
                                    <xsd:attribute name = 'sub-service' type='NonEmptyString' use='required' />
                                    <xsd:attribute name = 'level' type='xsd:positiveInteger' use='required'/>
                                    <xsd:attribute name = 'ui-category' type='NonEmptyString' />
                                    <xsd:attribute name = 'workplaces' type='IntList4OrEmpty' />
                                    <xsd:attribute name = 'pollution-radius' type='xsd:integer' />
                                    <xsd:attribute name = 'uneducated' type='xsd:integer' />
                                    <xsd:attribute name = 'steam-id' type='xsd:string' />
                                    <xsd:attribute name = 'fire-hazard' type='xsd:integer' />
                                    <xsd:attribute name = 'fire-size' type='xsd:integer' />
                                    <xsd:attribute name = 'fire-tolerance' type='xsd:integer' />
                                    <xsd:attribute name = 'educated' type='xsd:integer' />
                                    <xsd:attribute name = 'welleducated' type='xsd:integer' />
                                    <xsd:attribute name = 'higheducated' type='xsd:integer' />
                                    <xsd:attribute name = 'ignore-reality' type='xsd:boolean' />
                                    <xsd:attribute name = 'enable-pollution' type='xsd:boolean' />
                                    <xsd:attribute name = 'enable-popbalance' type='xsd:boolean' />
                                    <xsd:attribute name = 'enable-educationratio' type='xsd:boolean' />
                                    <xsd:attribute name = 'enable-rico' type='xsd:boolean' />
                                    <xsd:attribute name = 'enable-workercount' type='xsd:boolean' />
                                    <xsd:attribute name = 'enable-homecount' type='xsd:boolean'  />
                                    <xsd:attribute name = 'enable-constructioncost' type='xsd:boolean' />
                                    <xsd:attribute name = 'deviations' type='IntList4OrEmpty' />
                                    <xsd:attribute name = 'homes' type='xsd:integer' />
                                    <xsd:attribute name = 'construction-cost' type='xsd:integer' />
                                </xsd:extension>
                                </xsd:simpleContent>                    
                                </xsd:complexType>
                            </xsd:element>
                        </xsd:sequence>
                    </xsd:complexType>
                </xsd:element>
            </xsd:choice>
        </xsd:complexType>
    </xsd:element>
</xsd:schema>";
        
        private static void InitSchemas()
        {
            if (schemas != null) return;
            schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(xsdMarkup)));
        }

        public static PloppableRICODefinition ParseRICODefinition(string packageName, string ricoDefPath, List<string> ricoDefParseErrors)
        {

            InitSchemas();

            PloppableRICODefinition ricoDef;

            if ( ValidateRICODefinition(packageName, ricoDefPath, ricoDefParseErrors))
            {

                ricoDef = DeserializeRICODefinition(packageName, ricoDefPath, ricoDefParseErrors);

                if ( ricoDef != null )
                {
                    if ( SanitizeRICODefinition( packageName, ricoDef, ricoDefParseErrors ) )
                    {
                        return ricoDef;
                    }
                }
            }

            return null;
        }

        public static bool ValidateRICODefinition(string packageName, string ricoDefPath, List<String> ricoDefParseErrors)
        {
            // return true;
            var error = "";

            // This is validating the input XML against the XML-Schema defined inline above
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(schemas);
                settings.ValidationType = ValidationType.Schema;

                XmlReader reader = XmlReader.Create(ricoDefPath, settings);
                XmlDocument document = new XmlDocument();
                document.Load( reader );
                document.Validate( ( o, e ) => {
                    error = e.Message;
                } );
            }
            catch (Exception e)
            {
                ricoDefParseErrors.Add(String.Format("Unexpected Exception while validating RICO - file {0} ({1})", packageName, e.Message));
                return false;
            }

            if (error != null && error != "")
            {
                ricoDefParseErrors.Add(String.Format("Error while parsing RICO - file {0}. ({1})", packageName, error));
                return false;
            }

            return true;
        }

        public static PloppableRICODefinition DeserializeRICODefinition(string packageName, string ricoDefPath, List<string> ricoDefParseErrors)
        { 
            try
            {
                XmlAttributes serviceA = new XmlAttributes();
                serviceA.XmlAttribute = new XmlAttributeAttribute( "service" );
                XmlAttributeOverrides xao = new XmlAttributeOverrides();
                xao.Add( typeof( String ), "service", serviceA );
                var streamReader = new System.IO.StreamReader(ricoDefPath);
                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition), xao);
                var result = xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
                streamReader.Close();

                return result;
            }
            catch (Exception e)
            {
                // There shouldn't be anything left in the XML the Deserializer could trip over,
                // but better safe than sorry
                ricoDefParseErrors.Add(String.Format("Unexpected Exception while deserializing RICO - file {0} ({1})", packageName, e.Message));
                return null;
            }
        }

        // Lets check if someone reads comments here.
        // What's the answer to the ultimate question of life, universe and everything?
        public static bool SanitizeRICODefinition(string packageName, PloppableRICODefinition ricoDef, List<string> ricoDefParseErrors)
        {
            var i = 0; var b = true;
            foreach (var building in ricoDef.Buildings)
            {
                i++;
                if (!SanitizeRICOBuilding(packageName, i, building, ricoDefParseErrors))
                    b = false;
            }

            if ( ricoDef.Buildings.Count() == 0 )
                ricoDefParseErrors.Add(
                    String.Format( "Insanity while processing RICO - file. (File contains no buildings)" )
                );

            return b;
        }

        public static bool SanitizeRICOBuilding(string packageName, int index, PloppableRICODefinition.Building building, List<String>ricoDefParseErrors )
        {
            var i = ricoDefParseErrors.Count();

            if (!new Regex(
                String.Format( @"[^<>:/\\\|\?\*{0}]", "\"" )
            ).IsMatch(building.name) || building.name == "* unnamed")
            {
                ricoDefParseErrors.Add(String.Format("Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, index, String.Format("The building has {0} name. May I suggest Brian?", building.name == "" || building.name == "* unnamed" ? "no" : "a funny")  ) );
            }

            if (!new Regex(@"^(residential|commercial|office|industrial|extractor)$").IsMatch(building.service))
            {
                ricoDefParseErrors.Add(String.Format("Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, index,
                    "The building has " + ( building.service == "" ? "no " : "an incorrect ") + "service. Make up your mind already."));
            }
            if (!new Regex(@"^(high|low|generic|farming|oil|forest|ore|none)$").IsMatch(building.subService))
            {
                ricoDefParseErrors.Add(String.Format("Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, index,
                    "The building has " + (building.service == "" ? "no " : "an incorrect ") + "sub-service. Do you want to know more?"));
            }

            if (!new Regex(@"^[12345]$").IsMatch(building.level.ToString()))
            {
                ricoDefParseErrors.Add(String.Format("Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, index,
                    "The building has an incorrect level. Can't be THAT hard to choose between 1 to 5."));
            }

            if (!new Regex(@"^(comlow|comhigh|reslow|reshigh|office|industrial|oil|ore|farming|forest)$").IsMatch(building.UICategory))
            {
                ricoDefParseErrors.Add(String.Format("Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, index,
                    "The building has an incorrect ui-category. It's not as complicated as it seems."));
            }
            
            if (building.service == "residential")
            {
                if (building.homeCount == 0)
                    ricoDefParseErrors.Add(String.Format("Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, index, "Service is 'residential' but no homes are set. Am I supposed to guess? Fine: 42?"));
            } 
            else
            {
                if ((building.workplaceCount == 0) && building.service != "" && building.service != "none" )
                ricoDefParseErrors.Add(String.Format(
                    "Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, index, 
                    String.Format( 
                        "{0} provides jobs but no jobs are set! Do I look like a goddamn chrystal ball?", building.service)
                 ));
            }

            return i == ricoDefParseErrors.Count();
        }
    }
}
