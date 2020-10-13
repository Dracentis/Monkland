using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Monkland.SteamManagement
{
    internal static class AbstractSpearHandler
    {
        public static void Write(AbstractSpear abstractSpear, ref BinaryWriter writer)
        {
            writer.Write(abstractSpear.explosive);
            writer.Write(abstractSpear.stuckInWallCycles);
            writer.Write(abstractSpear.stuckVertically);
        }

        public static void Read(AbstractSpear abstractSpear, ref BinaryReader reader)
        {
            abstractSpear.explosive = reader.ReadBoolean();
            abstractSpear.stuckInWallCycles = reader.ReadInt32();
            abstractSpear.stuckVertically = reader.ReadBoolean();
        }
    }
}
