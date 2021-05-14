using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Monkland.SteamManagement
{
    internal static class RegionNameHandler
    {
        /// <summary>
        /// Reads first two chars of a string into the ref BinaryWriter.
        /// </summary>
        /// <param name="ref writer">Binary writer referenced</param>
        /// <returns>Built string</returns>
        public static string Read(ref BinaryReader reader)
        {
            char a = reader.ReadChar();
            char b = reader.ReadChar();
            return new string(new char[] {a, b });
        }

        /// <summary>
        /// Writes first two chars of a string into the ref BinaryWriter.
        /// </summary>
        /// <param name="regionName">Name of the region</param>
        /// <param name="ref writer">Binary writer referenced</param>
        /// <returns>void</returns>
        public static void Write(string regionName, ref BinaryWriter writer)
        {
            char[] characters = regionName.ToCharArray();
            writer.Write(characters[0]);
            writer.Write(characters[1]);
        }
    }
}

