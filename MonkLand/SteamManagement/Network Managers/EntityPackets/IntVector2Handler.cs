using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RWCustom;

namespace Monkland.SteamManagement
{
    static class IntVector2Handler
    {
        public static IntVector2 Read(ref BinaryReader reader)
        {
            IntVector2 intVector2 = new IntVector2();
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            intVector2.x = x;
            intVector2.y = y;
            return intVector2;
        }

        public static void Write(IntVector2 intVector2, ref BinaryWriter writer)
        {
            IntVector2 vec = intVector2;
            writer.Write(vec.x);
            writer.Write(vec.y);
        }

    }
}
