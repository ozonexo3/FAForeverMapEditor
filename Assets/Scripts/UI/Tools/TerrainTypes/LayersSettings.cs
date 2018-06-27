using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EditMap.TerrainTypes
{
//    [CreateAssetMenu("LayersSettings.sco","Create Layers Settings",0)]
    [CreateAssetMenu]
    public class LayersSettings : ScriptableObject
    {
        public List<TerrainTypeLayerSettings> layersSettings;

//        public List<TerrainTypeLayerSettings> LayersList
//        {
//            get { return layersSettings.Values.ToList(); }
//            set { layersSettings = value.ToDictionary(settings => settings.index, settings => settings); }
//        }

        public LayersSettings(List<TerrainTypeLayerSettings> layersSettings)
        {
            this.layersSettings = layersSettings;
        }

        public LayersSettings(TerrainTypeLayerSettings[] layersSettings) : this(layersSettings.ToList())
        {
        }

        public LayersSettings()
        {
            layersSettings = new List<TerrainTypeLayerSettings>();
        }

        public TerrainTypeLayerSettings this[int index]
        {
            get { return layersSettings.Find(settings => settings.index == index); }
            set
            {
                int i = layersSettings.FindIndex(settings => settings.index == index);
                if (i != -1)
                {
                    layersSettings.RemoveAt(i);
                }
                layersSettings.Add(value);
            }
        }
    }
}