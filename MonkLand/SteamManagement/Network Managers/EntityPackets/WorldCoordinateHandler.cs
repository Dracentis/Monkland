using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    static class WorldCoordinateHandler
    {
        public static WorldCoordinate Read(ref BinaryReader reader)
        {
            WorldCoordinate coordinate = new WorldCoordinate(0,0,0,0);
            coordinate.x = reader.ReadInt32();
            coordinate.y = reader.ReadInt32();
            coordinate.room = reader.ReadInt32();
            coordinate.abstractNode = reader.ReadInt32();
            return coordinate;
        }

        public static void Write(WorldCoordinate coordinate, ref BinaryWriter writer)
        {
            writer.Write(coordinate.x);
            writer.Write(coordinate.y);
            writer.Write(coordinate.room);
            writer.Write(coordinate.abstractNode);
        }
    }
}