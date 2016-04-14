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
            [XmlAttribute ("name"), DefaultValue (null)]
            public string name { get; set; }

            [XmlAttribute ("service"), DefaultValue ("none")]
            public string service { get; set; }

            [XmlAttribute ("sub-service"), DefaultValue ("none")]
            public string subService { get; set; }

            [XmlAttribute ("construction-cost"), DefaultValue (1)]
            public int constructionCost { get; set; }

            [XmlAttribute("ui-category"), DefaultValue("none")]
            public string UICategory { get; set; }

            [XmlAttribute ("homes"), DefaultValue (0)]
            public int homeCount { get; set; }

            [XmlAttribute("level"), DefaultValue(1)]
            public int level { get; set; }

            //Pollution
            [XmlAttribute("pollution-radius"), DefaultValue(0)]
            public int pollutionRadius { get; set; }

            //Workplace settings
            [XmlAttribute("workplaces"), DefaultValue(0)]
            public int workplaceCount { get; set; }

            [XmlAttribute("uneducated"), DefaultValue(1)]
            public int uneducated { get; set; }

            [XmlAttribute("educated"), DefaultValue(1)]
            public int educated { get; set; }
       
            [XmlAttribute("welleducated"), DefaultValue(1)]
            public int wellEducated { get; set; }

            //Toggles
            [XmlAttribute("enable-popbalance"), DefaultValue(true)]
            public bool popbalanceEnabled { get; set; }

            [XmlAttribute("enable-rico"), DefaultValue(true)]
            public bool ricoEnabled { get; set; }

            [XmlAttribute("enable-educationratio"), DefaultValue(true)]
            public bool educationRatioEnabled { get; set; }

            [XmlAttribute("enable-pollution"), DefaultValue(true)]
            public bool pollutionEnabled { get; set; }

            [XmlAttribute("enable-manualcount"), DefaultValue(true)]
            public bool manualCountEnabled { get; set; }

            [XmlAttribute("enable-constructioncost"), DefaultValue(true)]
            public bool constructionCostEnabled { get; set; }

        }
    }
}