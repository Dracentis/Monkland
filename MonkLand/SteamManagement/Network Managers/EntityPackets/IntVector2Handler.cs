using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RWCustom;

namespace Monkland.SteamManagement
{
    static class IntVector2Handler
    {
        public static IntVector2? Read(ref BinaryReader reader)
        {
            IntVector2 intVector2 = new IntVector2();
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            if (x == y && y == 0)
            {
                return null;
            }
            else 
            { 
                intVector2.x = x;
                intVector2.y = y;
            }
            return intVector2;
        }

        public static void Write(IntVector2? intVector2, ref BinaryWriter writer)
        {
            if (intVector2 == null)
            {
                writer.Write(0);
                writer.Write(0);
            }
            else
            {
                IntVector2 vec = (IntVector2)intVector2;
                writer.Write(vec.x);
                writer.Write(vec.y);
            }
        }

    }
}
