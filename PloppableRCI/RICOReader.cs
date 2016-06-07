using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace PloppableRICO
{
    public class RICOReader
    {
        private static XmlSchemaSet schemas;
        private static string xsdMarkup =
    @"
<xsd:schema xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
    <xsd:simpleType name='NonEmptyString'>
        <xsd:restriction base='xsd:string'>
            <xsd:minLength value = '1' />
            <xsd:pattern value = '.*[^\s].*' />
        </xsd:restriction>
    </xsd:simpleType>

    <xsd:simpleType name='Tupel'>
        <xsd:restriction base='xsd:string'>
            <xsd:minLength value = '1' />
            <xsd:pattern value = '^\d+( *: *\d+)?$' />
        </xsd:restriction>
    </xsd:simpleType>

    <xsd:simpleType name='Empty'>
        <xsd:restriction base='xsd:string'>
            <xsd:length value = '0' />
        </xsd:restriction>
    </xsd:simpleType>
    <xsd:simpleType name='TupelOrEmpty'>
        <xsd:union>
            <xsd:simpleType>
                <xsd:restriction base='Tupel' />
            </xsd:simpleType>
            <xsd:simpleType>
                <xsd:restriction base='Empty' />
            </xsd:simpleType>
        </xsd:union>
    </xsd:simpleType>

    <xsd:simpleType name='WorkplaceDistribution'>
        <xsd:restriction base='xsd:string'>
            <xsd:minLength value = '1' />
            <xsd:pattern value = '^\d+( *: *\d+)? *, *\d+( *: *\d+)? *, *\d+( *: *\d+)? *, *\d+( *: *\d+)? *, *\d+( *: *\d+)? *$' />
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
                                    <xsd:attribute name = 'name' type='NonEmptyString' />
                                    <xsd:attribute name = 'service' type='NonEmptyString' use='required' />
                                    <xsd:attribute name = 'sub-service' type='NonEmptyString' use='required' />
                                    <xsd:attribute name = 'level' type='xsd:positiveInteger' use='required'/>
                                    <xsd:attribute name = 'ui-category' type='NonEmptyString' use='required' />
                                    <xsd:attribute name = 'workplaces' type='TupelOrEmpty' />
                                    <xsd:attribute name = 'uneducated' type='TupelOrEmpty' />
                                    <xsd:attribute name = 'educated' type='TupelOrEmpty' />
                                    <xsd:attribute name = 'welleducated' type='TupelOrEmpty' />
                                    <xsd:attribute name = 'higheducated' type='TupelOrEmpty' />
                                    <xsd:attribute name = 'ignore-reality' type='xsd:boolean' />
                                    <xsd:attribute name = 'workplace-distribution' type='WorkplaceDistribution' />
                                    <xsd:attribute name = 'homes' type='TupelOrEmpty' />
                                    <xsd:attribute name = 'construction-cost' type='TupelOrEmpty' />
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

            if (ValidateRICODefinition(packageName, ricoDefPath, ricoDefParseErrors))
            {
                ricoDef = DeserializeRICODefinition(packageName, ricoDefPath, ricoDefParseErrors);

                if (ricoDef != null && SanitizeRICODefinition(packageName, ricoDef, ricoDefParseErrors))
                    return ricoDef;
            }

            return null;
        }

        private static bool ValidateRICODefinition(string packageName, string ricoDefPath, List<String> ricoDefParseErrors)
        {
            var error = "";

            // This is validating the input XML against the XML-Schema defined inline above
            try
            {
                var doc = XDocument.Load(ricoDefPath);
                doc.Validate(schemas, (o, e) => { error = e.Message; });
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

        private static PloppableRICODefinition DeserializeRICODefinition(string packageName, string ricoDefPath, List<string> ricoDefParseErrors)
        { 
            try
            {
                var streamReader = new System.IO.StreamReader(ricoDefPath);
                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));
                return xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
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
        private static bool SanitizeRICODefinition(string packageName, PloppableRICODefinition ricoDef, List<string> ricoDefParseErrors)
        {
            var i = 0;
            foreach (var b in ricoDef.Buildings)
            {
                i++;

                if (b.service == "residential")
                {
                    if (b.homeCount == 0)
                        ricoDefParseErrors.Add(String.Format("Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, i, "Service is 'residential' but no homes are set in the file (or set to zero). Am I supposed to guess? Fine: 42?", b.service));
                }
                else
                {
                    if (b.workplaceCount + b.educated + b.uneducated + b.wellEducated + b.highEducated == 0)
                        ricoDefParseErrors.Add(String.Format("Insanity while processing RICO - file {0} at building #{1} ({2})", packageName, "Service is set to {3} that provides jobs but no jobs are set in the file (or set to zero). Do I look like a goddamn chrystal ball?", b.service));
                }
            }

            return true;
        }
    }
}
