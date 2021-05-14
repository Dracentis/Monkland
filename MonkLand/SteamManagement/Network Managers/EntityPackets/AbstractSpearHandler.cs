using System;
using System.Collections.Generic;
using UnityEngine;
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
            bool explosive = reader.ReadBoolean();
            int stuckInWallCycles = reader.ReadInt32();
            bool stuckVertically = reader.ReadBoolean();

            try
            {
                abstractSpear.explosive = explosive;
                abstractSpear.stuckInWallCycles = stuckInWallCycles;
                abstractSpear.stuckVertically = stuckVertically;
            } catch (Exception e) { Debug.Log(e); }
        }
    }
}
