
using System;
using UnityEngine;

namespace PloppableRICO
{
    /// <summary>
    ///This class assigns the RICO settings to the prefabs. 
    /// </summary>
    ///

    public class ConvertPrefabs
    {
        public void run()
        {
            //Loop through the dictionary, and apply any RICO settings. 
            foreach (var buildingData in RICOPrefabManager.prefabHash.Values)
            {
                if (buildingData != null)
                {
                    //If asset has local settings, apply those. 
                    if (buildingData.hasLocal)
                    {
                        //If local settings disable RICO, dont convert
                        if (buildingData.local.ricoEnabled)
                        {
                            ConvertPrefab(buildingData.local, buildingData.name);
                            continue;
                        }
                    }

                    else if (buildingData.hasAuthor)
                    {
                        if (buildingData.author.ricoEnabled)
                        {
                            //Profiler.Info( " RUN " + buildingData.name );
                            ConvertPrefab(buildingData.author, buildingData.name);
                            continue;
                        }
                    }
                    else if (buildingData.hasMod)
                    {
                        Debug.Log(buildingData.name + "Has Local");
                        ConvertPrefab(buildingData.mod, buildingData.name);
                        continue;
                    }
                }
            }
        }

        public void ConvertPrefab(RICOBuilding buildingData, string name)
        {
            var prefab = PrefabCollection<BuildingInfo>.FindLoaded(name);

            int num2;
            int num3;
            //prefab.GetWidthRange(out num2, out num3);
            int num4;
            int num5;
            //prefab.GetLengthRange(out num4, out num5);

            //This filters out Larger Footprint buildings. 
            //if (!(prefab.m_cellWidth < num2 || prefab.m_cellWidth > num3 || prefab.m_cellLength < num4 || prefab.m_cellLength > num5))
            // {

            if (prefab != null)
            {

                if (buildingData.service == "dummy")
                {
                    var ai = prefab.gameObject.AddComponent<DummyBuildingAI>();

                    prefab.m_buildingAI = ai;
                    prefab.m_buildingAI.m_info = prefab;
                    try
                    {
                        prefab.InitializePrefab();
                    }
                    catch
                    {
                        Debug.Log("InitPrefab Failed" + prefab.name);
                    }
                    prefab.m_placementStyle = ItemClass.Placement.Manual;


                }

                else if (buildingData.service == "residential")
                {
                    var ai = prefab.gameObject.AddComponent<PloppableResidential>();
                    if (ai == null) throw (new Exception("Residential-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_homeCount = buildingData.homeCount;

                    //If GC installed, apply eco service if set
                    if (Util.isGCinstalled())      
                    {
                        Debug.Log("Green Cites Installed");

                        if (buildingData.service == "low eco")
                        {
                            InitializePrefab(prefab, ai, "Low Residential Eco - Level" + buildingData.level);
                        }
                        else if (buildingData.service == "high eco")
                        {
                            InitializePrefab(prefab, ai, "High Residential Eco - Level" + buildingData.level);
                        }
                        else if (buildingData.service == "high")
                        {
                            InitializePrefab(prefab, ai, "High Residential - Level" + buildingData.level);
                        }
                        else {
                            InitializePrefab(prefab, ai, "Low Residential - Level" + buildingData.level);

                        }
                    }
                    //if no DLC, apply normal services
                    else {

                        if (buildingData.service == "high eco" || buildingData.service == "high")
                        {
                            InitializePrefab(prefab, ai, "High" + " Residential - Level" + buildingData.level);
                        }
                        else
                        {
                            InitializePrefab(prefab, ai, "Low" + " Residential - Level" + buildingData.level);
                        }
                    }


                }
                else if (buildingData.service == "office")
                {
                    var ai = prefab.gameObject.AddComponent<PloppableOffice>();
                    if (ai == null) throw (new Exception("Office-AI not found."));
                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;

                    //Apply IT cluster if DLC installed
                    if (Util.isGCinstalled())
                    {
                        if (buildingData.service == "high tech")
                        {
                            InitializePrefab(prefab, ai, "Office - Hightech");
                        }
                        else
                            InitializePrefab(prefab, ai, "Office - Level" + buildingData.level);

                    }
                    //If no DLC, make IT buildngs level 3 office. 
                    else {

                        if (buildingData.service == "high tech") {

                            InitializePrefab(prefab, ai, "Office - Level3");
                        }
                        else
                        InitializePrefab(prefab, ai, "Office - Level" + buildingData.level);
                    }
                    
                }
                else if (buildingData.service == "industrial")
                {
                    var ai = prefab.gameObject.AddComponent<PloppableIndustrial>();
                    if (ai == null) throw (new Exception("Industrial-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_pollutionEnabled = buildingData.pollutionEnabled;

                    if (Util.industryServices.Contains(buildingData.subService))
                        InitializePrefab(prefab, ai, Util.ucFirst(buildingData.subService) + " - Processing");
                    else
                        InitializePrefab(prefab, ai, "Industrial - Level" + buildingData.level);
                }
                else if (buildingData.service == "extractor")
                {
                    var ai = prefab.gameObject.AddComponent<PloppableExtractor>();
                    if (ai == null) throw (new Exception("Extractor-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_pollutionEnabled = buildingData.pollutionEnabled;

                    if (Util.industryServices.Contains(buildingData.subService))
                        InitializePrefab(prefab, ai, Util.ucFirst(buildingData.subService) + " - Extractor");
                }

                else if (buildingData.service == "commercial")
                {
                    string itemClass = "";
                    var ai = prefab.gameObject.AddComponent<PloppableCommercial>();
                    if (ai == null) throw (new Exception("Commercial-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;

                    // high and low
                    if (Util.vanillaCommercialServices.Contains(buildingData.subService))
                        itemClass = Util.ucFirst(buildingData.subService) + " Commercial - Level" + buildingData.level;

                    //apply AD subservice if DLC installed
                    else if (Util.isADinstalled())
                    {
                        if (buildingData.subService == "tourist")
                            itemClass = "Tourist Commercial - Land";
                        else if (buildingData.subService == "leisure")
                            itemClass = "Leisure Commercial";
                    }

                    //apply GC subservice if DLC installed
                    else if (Util.isGCinstalled())
                    {
                        if (buildingData.subService == "eco")
                            itemClass = "Eco Commercial";
                    }

                    //use com high as default if no DLCs installed yet DLC settings found
                    else
                        itemClass = "High Commercial - Level" + buildingData.level;

                    InitializePrefab(prefab, ai, itemClass);
                }
                //}
            }
        }

        public static void InitializePrefab(BuildingInfo prefab, PrivateBuildingAI ai, String aiClass)
        {
            prefab.m_buildingAI = ai;
            ai.m_constructionTime = 30;
            prefab.m_buildingAI.m_info = prefab;

            try
            {
                prefab.InitializePrefab();
            }
            catch
            {
                Debug.Log("InitPrefab Failed" + prefab.name);
            }

            prefab.m_class = ItemClassCollection.FindClass(aiClass);
            prefab.m_placementStyle = ItemClass.Placement.Automatic;
            prefab.m_autoRemove = true;
            //prefab.m_dontSpawnNormally = false;
        }
    }
}
