using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.IO;
using UnityEngine;

namespace PloppableRICO
{
    public class ConvertPrefabs
    {
        public void run()
        {

            foreach (var buildingData in XMLManager.xmlData.Values)
            {

                if (buildingData != null)
                {
                    if (buildingData.hasLocal)
                    {
                        ConvertPrefab(buildingData.local, buildingData.name);
                        break;
                    }

                    else if (buildingData.hasAuthor)
                    {
                        ConvertPrefab(buildingData.author, buildingData.name);
                        //Debug.Log(buildingData.author.name + " is " + buildingData.author.service);
                    }
                }
            }
        }

        public void ConvertPrefab(PloppableRICODefinition.Building buildingData, string name)
        {
            var prefab = PrefabCollection<BuildingInfo>.FindLoaded(name);

            if (prefab != null)
            {

                if (buildingData.service == "residential")
                {
                    var ai = prefab.gameObject.AddComponent<PloppableResidential>();

                    prefab.m_buildingAI = ai;
                    ai.m_homeCount = buildingData.homeCount;

                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_constructionTime = 0;
                    prefab.m_buildingAI.m_info = prefab;
                    prefab.InitializePrefab();

                    if (buildingData.subService == "low") prefab.m_class = ItemClassCollection.FindClass("Low Residential - Level" + buildingData.level);
                    else prefab.m_class = ItemClassCollection.FindClass("High Residential - Level" + buildingData.level);

                }

                else if (buildingData.service == "office")
                {

                    var ai = prefab.gameObject.AddComponent<PloppableOffice>();

                    prefab.m_buildingAI = ai;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_constructionTime = 0;
                    prefab.m_buildingAI.m_info = prefab;
                    prefab.InitializePrefab();

                    prefab.m_class = ItemClassCollection.FindClass("Office - Level" + buildingData.level);


                }
                else if (buildingData.service == "industrial")
                {

                    var ai = prefab.gameObject.AddComponent<PloppableIndustrial>();
                    prefab.m_buildingAI = ai;

                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_constructionTime = 0;
                    ai.m_pollutionEnabled = buildingData.pollutionEnabled;
                    prefab.m_buildingAI.m_info = prefab;
                    prefab.InitializePrefab();

                    if (buildingData.subService == "farming") prefab.m_class = ItemClassCollection.FindClass("Farming - Processing");

                    else if (buildingData.subService == "forest") prefab.m_class = ItemClassCollection.FindClass("Forest - Processing");

                    else if (buildingData.subService == "oil") prefab.m_class = ItemClassCollection.FindClass("Oil - Processing");

                    else if (buildingData.subService == "ore") prefab.m_class = ItemClassCollection.FindClass("Ore - Processing");

                    else prefab.m_class = ItemClassCollection.FindClass("Industrial - Level" + buildingData.level);

                }
                else if (buildingData.service == "extractor")
                {

                    var ai = prefab.gameObject.AddComponent<PloppableExtractor>();

                    prefab.m_buildingAI = ai;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_constructionTime = 0;
                    ai.m_pollutionEnabled = buildingData.pollutionEnabled;
                    prefab.m_buildingAI.m_info = prefab;
                    prefab.InitializePrefab();

                    if (buildingData.subService == "farming") prefab.m_class = ItemClassCollection.FindClass("Farming - Extractor");

                    else if (buildingData.subService == "forest") prefab.m_class = ItemClassCollection.FindClass("Forest - Extractor");

                    else if (buildingData.subService == "oil") prefab.m_class = ItemClassCollection.FindClass("Oil - Extractor");

                    else prefab.m_class = ItemClassCollection.FindClass("Ore - Extractor");
                }

                else if (buildingData.service == "commercial")
                {

                    var ai = prefab.gameObject.AddComponent<PloppableCommercial>();
                    prefab.m_buildingAI = ai;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_constructionTime = 0;
                    prefab.m_buildingAI.m_info = prefab;
                    prefab.InitializePrefab();

                    if (buildingData.subService == "low") prefab.m_class = ItemClassCollection.FindClass("Low Commercial - Level" + buildingData.level);

                    else if (buildingData.subService == "high") prefab.m_class = ItemClassCollection.FindClass("High Commercial - Level" + buildingData.level);

                    else if (buildingData.subService == "tourist")
                    {
                        if (Util.isADinstalled())
                        {
                            prefab.m_class = ItemClassCollection.FindClass("Tourist Commercial - Land");
                        }
                        else {
                            prefab.m_class = ItemClassCollection.FindClass("High Commercial - Level" + buildingData.level);
                        }
                    }
                    else if (buildingData.subService == "leisure")
                    {
                        if (Util.isADinstalled()) prefab.m_class = ItemClassCollection.FindClass("Leisure Commercial");

                        else prefab.m_class = ItemClassCollection.FindClass("High Commercial - Level" + buildingData.level);

                    }
                }
            }
        }
    }
}
