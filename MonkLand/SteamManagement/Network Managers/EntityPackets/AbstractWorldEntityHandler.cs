using System.IO;

namespace Monkland.SteamManagement
{
    internal static class AbstractWorldEntityHandler
    {
        /// <summary>
        /// Writes AbstractWorldEntity packet 
        /// <para>[ENTITYID | WORLDPOS | bool inDen | int timeSpentHere]</para>
        /// <para>[29 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(AbstractWorldEntity entity, ref BinaryWriter writer)
        {
            EntityIDHandler.Write(entity.ID, ref writer);
            WorldCoordinateHandler.Write(entity.pos, ref writer);
            writer.Write(entity.InDen);
            writer.Write(entity.timeSpentHere);
        }

        /// <summary>
        /// Writes AbstractWorldEntity packet 
        /// <para>[ENTITYID | WORLDPOS | bool inDen | int timeSpentHere]</para>
        /// <para>[29 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static AbstractWorldEntity Read(AbstractWorldEntity entity, ref BinaryReader reader)
        {
            entity.ID = EntityIDHandler.Read(ref reader);
            entity.pos = WorldCoordinateHandler.Read(ref reader);
            entity.InDen = reader.ReadBoolean();
            entity.timeSpentHere = reader.ReadInt32();
            return entity;
        }

    }
}
