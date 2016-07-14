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

        public static string ricoDataXml( PloppableRICODefinition RicoDefinition )
        {
            try
            {
                var ms = new MemoryStream();
                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));
                xmlSerializer.Serialize( ms, RicoDefinition );
                ms.Seek(0 , SeekOrigin.Begin);
                return ms.ToString();
            }
            catch
            { }
            return "";
        }
    }

    public class RICOReader
    {
        public static List<String> LastErrors;
        public static ICrpDataProvider crpDataProvider;

        public static PloppableRICODefinition ParseRICODefinition( string packageName, string ricoDefPath, bool insanityOK = false  )
        {
            var s = new FileStream( ricoDefPath, FileMode.Open);
            var r = ParseRICODefinition( packageName, s, insanityOK );

            if ( r != null )
                r.sourceFile = new FileInfo( ricoDefPath );
            s.Close();
            if ( r != null && crpDataProvider != null )
                addCrpShit( r );
            return r;
        }

        public static PloppableRICODefinition ParseRICODefinition( string packageName, Stream ricoDefStream, bool insanityOK = false )
        {

            LastErrors = new List<string>();

            var ricoDef = DeserializeRICODefinition(packageName, ricoDefStream, LastErrors );
            
            if ( ricoDef != null )
            {
                if ( insanityOK || ricoDef.isValid )
                {
                    LastErrors.AddRange( ricoDef.errors.Select( (n,i) =>
                        string.Format( "Error while processing RICO - file {0} at building #{1} ({2})", packageName, i, n ) 
                    ) );

                    return ricoDef;
                }
            }

            ricoDefStream.Close();
            return null;
        }
        private static void addCrpShit(PloppableRICODefinition ricoDef)
        {
            var crpPath = Util.crpFileIn( ricoDef.sourceFile.Directory );
            if ( crpPath != null )
                foreach ( var building in ricoDef.Buildings )
                    building.crpData = crpDataProvider.getCrpData( crpPath.FullName );
        } 
        public static PloppableRICODefinition DeserializeRICODefinition( string packageName, string ricoDefPath )
        {
            var s = new FileStream(ricoDefPath, FileMode.Open);
            var result = DeserializeRICODefinition( packageName, ricoDefPath );
            result.sourceFile = new FileInfo( ricoDefPath );
            return result;
        }

        public static PloppableRICODefinition DeserializeRICODefinition( string packageName, Stream ricoDefStream, List<string> errors)
        {
            try
            {
                XmlAttributes attrs = new XmlAttributes();

                XmlElementAttribute attr = new XmlElementAttribute();
                attr.ElementName = "RICOBuilding";
                attr.Type = typeof( RICOBuilding );

                XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();
                attrOverrides.Add( typeof( RICOBuilding ), "Building", attrs );

                var streamReader = new System.IO.StreamReader(ricoDefStream);
                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition), attrOverrides);
                var result = xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
                foreach ( var building in result.Buildings )
                    building.parent = result;
                streamReader.Close();
                result.clean();
                return result;
            }
            catch (Exception e)
            {
                errors.Add( String.Format( "Unexpected Exception while deserializing RICO - file {0} ({1} [{2}])", packageName, e.Message, e.InnerException != null ? e.InnerException.Message : "" ) );
                return null;
            }
        }
    }
}
