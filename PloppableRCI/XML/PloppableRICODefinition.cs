
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System;

namespace PloppableRICO
{
    public class PloppableRICODefinition
    {
        public List<RICOBuilding> Buildings { get; set; }

        [XmlIgnore]
        public FileInfo sourceFile;

        [XmlIgnore]
        public bool isDirty
        {
            get
            {
                foreach (var building in Buildings)
                    if (building.isDirty)
                        return true;
                return _isDirty;
            }
        }
        private bool _isDirty;

        public void clean()
        {
            foreach (var building in Buildings)
                building.clean();
            _isDirty = false;

        }
        public PloppableRICODefinition()
        {
            Buildings = new List<RICOBuilding>();
        }

        public RICOBuilding addBuilding(RICOBuilding buildingDef = null)
        {
            _isDirty = true;

            if (buildingDef == null)
            {
                buildingDef = new RICOBuilding();
                buildingDef.name = "* unnamed";
                buildingDef.parent = this;
            }

            Buildings.Add(buildingDef);

            return buildingDef;
        }

        public RICOBuilding removeBuilding(int index)
        {
            if (index < 0 && index >= this.Buildings.Count)
                return null;

            return removeBuilding(Buildings[index]);
        }

        public RICOBuilding removeBuilding(RICOBuilding buildingDef)
        {
            Buildings.Remove(buildingDef);
            _isDirty = true;
            return buildingDef;
        }

        [XmlIgnore]
        public List<string> errors
        {
            get
            {
                var errors = new List<string>();
                foreach (var building in Buildings)
                    errors.AddRange(building.errors);
                if (Buildings.Count() == 0)
                    errors.Add(String.Format("XML-Error while deserializing RICO - file."));
                return errors;
            }
        }

        [XmlIgnore]
        public bool isValid
        {
            get { return errors.Count() == 0; }
        }

        public delegate void BuildingChangedEventHandler(object sender, BuildingChangedEventArgs e);
        public event BuildingChangedEventHandler BuildingDirtynessChanged;
        public event BuildingChangedEventHandler BuildingPropertyChanged;

        public void RaiseBuildingDirtynessChanged(RICOBuilding building)
        {
            if (BuildingDirtynessChanged != null)
            {
                var e = new BuildingChangedEventArgs();
                e.building = building;
                BuildingDirtynessChanged(this, e);
            }
        }

        public void RaiseBuildingPropertyChanged(RICOBuilding building)
        {
            if (BuildingPropertyChanged != null)
            {
                var e = new BuildingChangedEventArgs();
                e.building = building;
                BuildingPropertyChanged(this, e);
            }
        }
    }

    public class BuildingChangedEventArgs : EventArgs
    {
        public RICOBuilding building;
    }
}