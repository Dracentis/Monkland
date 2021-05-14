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
        public static void Read(AbstractWorldEntity entity, ref BinaryReader reader)
        {
            // Make sure the stream is advanced even if exception happens
            EntityID ID = EntityIDHandler.Read(ref reader);
            WorldCoordinate pos = WorldCoordinateHandler.Read(ref reader);
            bool InDen = reader.ReadBoolean();
            int timeSpentHere = reader.ReadInt32();

            entity.ID = ID;
            entity.pos = pos;
            entity.InDen = InDen;
            entity.timeSpentHere = timeSpentHere;
            return;
        }

    }
}
