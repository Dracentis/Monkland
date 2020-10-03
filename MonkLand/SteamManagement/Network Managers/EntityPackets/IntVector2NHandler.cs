using RWCustom;
using System.IO;

namespace Monkland.SteamManagement
{
    internal static class IntVector2NHandler
    {
        public static IntVector2? Read(ref BinaryReader reader)
        {
            IntVector2 intVector2 = new IntVector2();
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            if (x == y && y == -50000)
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
                writer.Write(-50000);
                writer.Write(-50000);
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