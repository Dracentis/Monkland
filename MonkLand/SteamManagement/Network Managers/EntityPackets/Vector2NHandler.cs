using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal static class Vector2NHandler
    {
        public static Vector2? Read(ref BinaryReader reader)
        {
            Vector2 vector2 = new Vector2();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            if (x == y && y == -50000f)
            {
                return null;
            }
            else
            {
                vector2.x = x;
                vector2.y = y;
            }
            return vector2;
        }

        public static void Write(Vector2? vector2, ref BinaryWriter writer)
        {
            if (vector2 == null)
            {
                writer.Write(-50000f);
                writer.Write(-50000f);
            }
            else
            {
                writer.Write(vector2.Value.x);
                writer.Write(vector2.Value.y);
            }
        }
    }
}