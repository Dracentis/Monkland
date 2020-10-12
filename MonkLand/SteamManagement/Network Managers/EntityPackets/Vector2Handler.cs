using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal static class Vector2Handler
    {

        /// <summary>
        /// Writes Vector2 packet 
        /// <para>[float x | float y]</para>
        /// <para>[8 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(Vector2 vector2, ref BinaryWriter writer)
        {
            writer.Write(vector2.x);
            writer.Write(vector2.y);
        }

        /// <summary>
        /// Reads Vector2 packet [float x | float y] [8 bytes]
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static Vector2 Read(ref BinaryReader reader)
        {
            Vector2 vector2 = new Vector2();
            vector2.x = reader.ReadSingle();
            vector2.y = reader.ReadSingle();
            return vector2;
        }


    }
}