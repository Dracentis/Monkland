using System.IO;

namespace Monkland.SteamManagement
{
    internal static class AbstractWorldEntityHandler
    {
        public static AbstractWorldEntity Read(AbstractWorldEntity entity, ref BinaryReader reader)
        {
            entity.ID = EntityIDHandler.Read(ref reader);
            entity.pos = WorldCoordinateHandler.Read(ref reader);
            entity.InDen = reader.ReadBoolean();
            entity.timeSpentHere = reader.ReadInt32();
            return entity;
        }

        public static void Write(AbstractWorldEntity entity, ref BinaryWriter writer)
        {
            EntityIDHandler.Write(entity.ID, ref writer);
            WorldCoordinateHandler.Write(entity.pos, ref writer);
            writer.Write(entity.InDen);
            writer.Write(entity.timeSpentHere);
        }
    }
}