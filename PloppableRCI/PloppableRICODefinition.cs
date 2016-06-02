using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace PloppableRICO
{
    public class PloppableRICODefinition
    {
        public List<Building> Buildings { get; set; }

        public PloppableRICODefinition ()
        {
            Buildings = new List<Building> ();
        }

        public class Building
        {
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
                popbalanceEnabled = true;
                ricoEnabled = true;
                educationRatioEnabled = false;
                pollutionEnabled = true;
                manualWorkerEnabled = true;
                manualHomeEnabled = true;
                constructionCostEnabled = true;
            }

            [XmlAttribute ("name")]
            public string name { get; set; }

            [XmlAttribute ("service")]
            public string service { get; set; }

            [XmlAttribute ("sub-service")]
            public string subService { get; set; }

            [XmlAttribute ("construction-cost")]
            public int constructionCost { get; set; }

            [XmlAttribute("ui-category")]
            public string UICategory { get; set; }

            [XmlAttribute ("homes")]
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

        }
    }
}