using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal static class Vector2Handler
    {
        public static Vector2 Read(ref BinaryReader reader)
        {
            Vector2 vector2 = new Vector2();
            vector2.x = reader.ReadSingle();
            vector2.y = reader.ReadSingle();
            return vector2;
        }

        public static void Write(Vector2 vector2, ref BinaryWriter writer)
        {
            writer.Write(vector2.x);
            writer.Write(vector2.y);
        }
    }
}