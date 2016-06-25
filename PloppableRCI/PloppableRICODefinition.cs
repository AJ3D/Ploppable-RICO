using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Linq;
using System;

namespace PloppableRICO
{
    public class PloppableRICODefinition
    {
        public List<Building> Buildings { get; set; }

        public PloppableRICODefinition ()
        {
            Buildings = new List<Building> ();
        }

        public Building AddBuilding( Building buildingDef = null )
        {
            if ( buildingDef == null )
            {
                buildingDef = new Building();
                buildingDef.name = "* unnamed";
            }

            Buildings.Add( buildingDef );

            return buildingDef;
        }

        public Building RemoveBuilding( int index )
        {
            if ( index < 0 && index >= this.Buildings.Count )
                return null;

            return RemoveBuilding( Buildings[index] );
        }

        public Building RemoveBuilding( Building buildingDef )
        {
            Buildings.Remove( buildingDef );
            return buildingDef;
        }

        //    if ( listboxBuildings.SelectedIndex >= 0 )
        //    {
        //        ricoDef.Buildings.RemoveAt( listboxBuildings.SelectedIndex );
        //        listboxBuildings.Items.RemoveAt( listboxBuildings.SelectedIndex );

        //        // If the current building is not not in the building list anymore,
        //        if ( !ricoDef.Buildings.Contains( buildingDef ) )
        //        {
        //            if ( ricoDef.Buildings.Count > 0 )
        //            {
        //                // (because it got deleted) we must show another building.
        //                buildingDef = ricoDef.Buildings[0];
        //                listboxBuildings.SelectedIndex = 0;
        //            }
        //            else
        //            {
        //                // if the building list is empty, add an empty one
        //                AddBuilding_Click( sender, e );
        //            }
        //        }
        //    }
        //}
        public class Building
        {
            public Building()
            {

                workplaceDeviationString = "";
                workplaceDeviation = new int[] { 0, 0, 0, 0 };
                name = "";
                steamId = "";
                service = "";
                subService = "";
                constructionCost = 0;
                UICategory = "";
                homeCount = 0;
                level = 0;
                pollutionRadius = 0;
                workplaces = new int[] {0, 0, 0, 0};
                uneducated = 0;
                educated = 0;
                wellEducated = 0;
                highEducated = 0;
                fireHazard = 0;
                fireTolerance = 0;
                fireSize = 255;
                popbalanceEnabled = true;
                ricoEnabled = true;
                educationRatioEnabled = false;
                pollutionEnabled = true;
                manualWorkerEnabled = true;
                manualHomeEnabled = true;
                constructionCostEnabled = true;
                RealityIgnored = false;
             }

            public int maxLevel
            {
                get { return Util.MaxLevelOf( service, subService ); }
            }

            [XmlAttribute ("name")]
            public string name { get; set; }

            //[XmlAttribute ("service")]
            [XmlAttribute("service")]
            public string service { get; set; }

            [XmlAttribute ("sub-service")]
            public string subService { get; set; }

            [XmlAttribute ("construction-cost")]
            public int constructionCost { get; set; }

            private string _UICategory;

            [XmlAttribute("ui-category")]
            public string UICategory
            {
                get { return _UICategory != "" ? _UICategory : Util.UICategoryOf(service, subService); }
                set { _UICategory = value; }
            }

            [XmlAttribute( "homes" )]
            public int homeCount { get; set; }

            [XmlAttribute( "steam-id" )]
            public string steamId { get; set; }

            [XmlAttribute("level")]
            public int level { get; set; }

            //Pollution
            [XmlAttribute("pollution-radius")]
            public int pollutionRadius { get; set; }

            [XmlAttribute("uneducated")]
            public int uneducated { get; set; }

            [XmlAttribute("educated")]
            public int educated { get; set; }
       
            [XmlAttribute("welleducated")]
            public int wellEducated { get; set; }

            [XmlAttribute( "higheducated" )]
            public int highEducated { get; set; }

            [XmlAttribute( "fire-hazard" )]
            public int fireHazard { get; set; }

            [XmlAttribute( "fire-size" )]
            public int fireSize { get; set; }

            [XmlAttribute( "fire-tolerance" )]
            public int fireTolerance { get; set; }

            //Toggles
            [XmlAttribute("enable-popbalance")]
            public bool popbalanceEnabled { get; set; }

            [XmlAttribute("enable-rico")]
            public bool ricoEnabled { get; set; }

            [XmlAttribute("enable-educationratio")]
            public bool educationRatioEnabled { get; set; }

            [XmlAttribute("enable-pollution")]
            public bool pollutionEnabled { get; set; }

            [XmlAttribute("enable-workercount")]
            public bool manualWorkerEnabled { get; set; }

            [XmlAttribute("enable-homecount")]
            public bool manualHomeEnabled { get; set; }

            [XmlAttribute("enable-constructioncost")]
            public bool constructionCostEnabled { get; set; }

            [XmlIgnore]
            public int[] workplaces { get; set; }

            [XmlAttribute( "workplaces" )]
            public string workplacesString
            {
                get
                {
                    return String.Join( ",", workplaces.Select( n => n.ToString() ).ToArray() );
                }
                set
                {
                    var re1 = new System.Text.RegularExpressions.Regex("^ *(\\d+) *$");
                    var re4 = new System.Text.RegularExpressions.Regex("^ *(\\d+) *, *(\\d+) *, *(\\d+) *, *(\\d+) *");

                    // Split values and convert to int[] if the string is well formed
                    if ( re1.IsMatch( value ) )
                    {
                        workplaces = new int[] { Convert.ToInt32(value), -1, -1, -1 };
                    }
                    else if ( re4.IsMatch( value ) )
                    {
                        workplaces = value.Replace(" ", "").Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    }
                }
            }

            //Workplace settings
            [XmlIgnore]
            public int workplaceCount
            {
                get
                {
                    return workplaces[1] < 0 ? workplaces[0] : workplaces.Sum();
                }
            }


            [XmlIgnoreAttribute]
            public int[] workplaceDeviation { get; set; }

            private string _workplaceDeviationString;
            //Workplace job distribution settings
            [XmlAttribute( "deviations" )]
            public string workplaceDeviationString {
                get {
                    return String.Join( ",", workplaceDeviation.Select( i => i.ToString() ).ToArray() );
                }
                set
                {
                    var re = new System.Text.RegularExpressions.Regex("^ *(\\d+) *, *(\\d+) *, *(\\d+) *, *(\\d+) *");
                    _workplaceDeviationString = value;

                    // Split values and convert to int[] if the string is well formed
                    if ( re.IsMatch( value ) )
                        this.workplaceDeviation = value.Replace( " ", "" ).Split( ',' ).Select( n => Convert.ToInt32( n ) ).ToArray();
                    else
                        this.workplaceDeviation = new int[] { 0, 0, 0, 0 };
                 }
            }

            // Flag wether to ignore the realistic population mod is running 
            // This should probably be "true" by default
            [XmlAttribute("ignore-reality")]
            public bool RealityIgnored { get; set; }
            
            [XmlIgnoreAttribute]
            public bool useReality
            {
                get { return Util.IsModEnabled(426163185ul) && !RealityIgnored; }
            }
        }
    }
}