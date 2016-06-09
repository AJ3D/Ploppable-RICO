
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

        public PloppableRICODefinition()
        {
            Buildings = new List<Building>();
        }

        public class Building : ICloneable
        {
            public object Clone()
            {
                return this.MemberwiseClone();
            }
            public Building()
            {
                name = "none";
                service = "none";
                subService = "none";
                constructionCost = 0;
                UICategory = "none";
                homeCount = 1;
                level = 1;
                pollutionRadius = 0;
                workplaceCount = 0;
                uneducated = 0;
                educated = 0;
                wellEducated = 0;
                highEducated = 0;
                popbalanceEnabled = true;
                ricoEnabled = true;
                educationRatioEnabled = false;
                pollutionEnabled = true;
                RealityIgnored = false;
                workplaceDistributionString = "";
            }

            [XmlAttribute("name")]
            public string name { get; set; }

            [XmlAttribute("service")]
            public string service { get; set; }

            [XmlAttribute("sub-service")]
            public string subService { get; set; }

            [XmlAttribute("construction-cost")]
            public int constructionCost { get; set; }

            [XmlAttribute("ui-category")]
            public string UICategory { get; set; }

            [XmlAttribute("homes")]
            public int homeCount { get; set; }

            [XmlAttribute("level")]
            public int level { get; set; }

            //Pollution
            [XmlAttribute("pollution-radius")]
            public int pollutionRadius { get; set; }

            //Workplace settings
            [XmlAttribute("workplaces")]
            public int workplaceCount { get; set; }

            [XmlAttribute("uneducated")]
            public int uneducated { get; set; }

            [XmlAttribute("educated")]
            public int educated { get; set; }

            [XmlAttribute("welleducated")]
            public int wellEducated { get; set; }

            [XmlAttribute("higheducated")]
            public int highEducated { get; set; }

            //Toggles
            [XmlAttribute("enable-popbalance")]
            public bool popbalanceEnabled { get; set; }

            [XmlAttribute("enable-rico")]
            public bool ricoEnabled { get; set; }

            [XmlAttribute("enable-educationratio")]
            public bool educationRatioEnabled { get; set; }

            [XmlAttribute("enable-pollution")]
            public bool pollutionEnabled { get; set; }


            //Workplace job distribution settings
            [XmlAttribute("workplace-distribution")]
            public string workplaceDistributionString { get; set; }

            // User-defined distribution of workplaces to the knowledge levels
            // value must be a comma separated string of integers
            // first value is the base, the next 4 represent the share relative to the base 
            // so 200, 100, 50, 40, 10 means 50 uneducated, 25 educated, 20 well educated and 5 high educated jobs
            [XmlIgnoreAttribute]
            public int[] workplaceDistribution
            {
                get
                {
                    string wds = workplaceDistributionString;
                    var re = new System.Text.RegularExpressions.Regex("^ *(\\d+) *, *(\\d+) *, *(\\d+) *, *(\\d+) *, *(\\d+) *");

                    // Not set
                    if (wds == "")
                        return null;

                    // Split values and convert to int[] if the string is well formed
                    if (re.IsMatch(wds))
                        return wds.Replace(" ", "").Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    // Set but faulty
                    return new int[] { 100, 25, 25, 25, 25 };
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

            // Flag to indicate wether any values for uneducated, educated, welleducated or higheducated are present in the xml
            [XmlIgnoreAttribute]
            public bool workplaceDetailsEnabled
            {
                get
                {
                    return uneducated > 0 || educated > 0 || wellEducated > 0 || highEducated > 0;
                }
            }

        }
    }
}