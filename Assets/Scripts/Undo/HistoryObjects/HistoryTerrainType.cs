using EditMap.TerrainTypes;

namespace UndoHistory.HistoryObjects
{
    public class HistoryTerrainType : HistoryObject
    {
        private byte[] terrainTypeData;
        
        public override void Register()
        {
            this.terrainTypeData = (byte[])TerrainTypeWindow.GetUndoData().Clone();
        }

        public override void DoUndo()
        {
            if (!RedoGenerated)
            {
                HistoryTerrainType.GenerateRedo(Undo.Current.Prefabs.TerrainTypePaint).Register();
            }
            RedoGenerated = true;
            DoRedo();
        }

        public override void DoRedo()
        {
            TerrainTypeWindow.SetUndoData((byte[])terrainTypeData.Clone());
        }
    }
}