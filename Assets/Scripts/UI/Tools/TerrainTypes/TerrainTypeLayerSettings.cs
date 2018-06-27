using System;
using UnityEngine;

namespace EditMap.TerrainTypes
{
    [Serializable]
    public class TerrainTypeLayerSettings
    {
        public string name;
        public byte index;
        public Color color;
        public bool blocking;
        public Style style;
        public string description;

        public TerrainTypeLayerSettings()
        {
            name = "";
            index = 0;
            color = new Color(0,0,0,1);
            blocking = false;
            style = Style.Default;
            description = "";
        }

        public enum Style
        {
            Default,
            Evergreen,
            RedRock,
            Desert,
            Tropical,
            Lava,
            Geothermal,
            Tundra,
        }
    }
}