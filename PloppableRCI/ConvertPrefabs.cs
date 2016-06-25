
﻿using System;
using UnityEngine;

namespace PloppableRICO
{
    /// <summary>
    ///This class assigns the RICO settings to the prefabs. 
    /// </summary>
    public class ConvertPrefabs
    {
        public void run()
        {
            //Loop through the dictionary, and apply any RICO settings. 
            foreach (var buildingData in XMLManager.prefabHash.Values)
            {
                if ( buildingData != null)
                {
                    //If asset has local settings, apply those. 
                    if ( buildingData.hasLocal)
                    {
                        //If local settings disable RICO, dont convert
                        if ( buildingData.local.ricoEnabled)
                        {
                            ConvertPrefab( buildingData.local, buildingData.name);
                            continue;
                        }
                    }

                    if (buildingData.hasAuthor)
                    {
                        if ( buildingData.author.ricoEnabled)
                        {
                            ConvertPrefab( buildingData.author, buildingData.name);
                        }
                    }
                }
            }
        }

        public void ConvertPrefab(PloppableRICODefinition.Building buildingData, string name)
        {
            var prefab = PrefabCollection<BuildingInfo>.FindLoaded(name);

            if ( prefab != null)
            {
                if (buildingData.service == "residential")
                {
                    var ai = prefab.gameObject.AddComponent<PloppableResidential>();
                    ai.m_ricoData = buildingData;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_homeCount = buildingData.homeCount;
                    InitializePrefab(prefab, ai, Util.ucFirst(buildingData.subService) + " Residential - Level" + buildingData.level);
                }
                else if (buildingData.service == "office")
                {
                    var ai = prefab.gameObject.AddComponent<PloppableOffice>();
                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    InitializePrefab(prefab, ai, "Office - Level" + buildingData.level);
                }
                else if (buildingData.service == "industrial")
                {
                    var ai = prefab.gameObject.AddComponent<PloppableIndustrial>();
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
                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    
                    // high and low
                    if ( Util.vanillaCommercialServices.Contains(buildingData.subService) )
                        itemClass = Util.ucFirst(buildingData.subService) + " Commercial - Level" + buildingData.level;
                    else
                        if (Util.isADinstalled())
                            if (buildingData.subService == "tourist")
                                itemClass = "Tourist Commercial - Land";
                            else if (buildingData.subService == "leisure")
                                itemClass = "Leisure Commercial";
                            else
                                itemClass = "High Commercial - Level" + buildingData.level;
                        else
                            itemClass = "High Commercial - Level" + buildingData.level;

                    InitializePrefab(prefab, ai, itemClass);
                }
            }
        }

        private void InitializePrefab(BuildingInfo prefab, PrivateBuildingAI ai, String aiClass)
        {
            prefab.m_buildingAI = ai;
            ai.m_constructionTime = 0;
            prefab.m_buildingAI.m_info = prefab;
            prefab.InitializePrefab();
            prefab.m_class = ItemClassCollection.FindClass(aiClass);
        }
    }
}
