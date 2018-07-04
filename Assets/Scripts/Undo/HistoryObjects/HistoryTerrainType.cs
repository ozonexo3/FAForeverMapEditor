using EditMap.TerrainTypes;

namespace UndoHistory
{
    public class HistoryTerrainType : HistoryObject
    {
        private byte[] terrainTypeData;
        
        public override void Register(HistoryParameter Param)
        {
            this.terrainTypeData = (byte[])TerrainTypeWindow.GetUndoData().Clone();
        }

        public override void DoUndo()
        {
            if (!RedoGenerated)
            {
				Undo.RegisterRedo(new HistoryTerrainType());
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