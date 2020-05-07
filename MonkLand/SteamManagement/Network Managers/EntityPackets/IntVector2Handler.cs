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
            intVector2.x = reader.ReadInt32();
            intVector2.y = reader.ReadInt32();
            return intVector2;
        }

        public static void Write(IntVector2 intVector2, ref BinaryWriter writer)
        {
            writer.Write(intVector2.x);
            writer.Write(intVector2.y);
        }

    }
}
