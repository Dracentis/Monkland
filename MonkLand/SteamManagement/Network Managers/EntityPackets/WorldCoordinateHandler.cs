using System.IO;

namespace Monkland.SteamManagement
{
    internal static class WorldCoordinateHandler
    {
        /// <summary>
        /// Writes WorldCoordinate packet 
        /// <para>[int x | int y | int room | int abstractNode]</para>
        /// <para>[16 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(WorldCoordinate coordinate, ref BinaryWriter writer)
        {
            writer.Write(coordinate.x);
            writer.Write(coordinate.y);
            writer.Write(coordinate.room);
            writer.Write(coordinate.abstractNode);
        }

        /// <summary>
        /// Reads WorldCoordinate packet 
        /// <para>[int x | int y | int room | int abstractNode]</para>
        /// <para>[16 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static WorldCoordinate Read(ref BinaryReader reader)
        {
            WorldCoordinate coordinate = new WorldCoordinate(0, 0, 0, 0);
            coordinate.x = reader.ReadInt32();
            coordinate.y = reader.ReadInt32();
            coordinate.room = reader.ReadInt32();
            coordinate.abstractNode = reader.ReadInt32();
            return coordinate;
        }



    }
}