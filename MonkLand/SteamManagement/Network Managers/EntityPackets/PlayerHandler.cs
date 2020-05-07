using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RWCustom;

namespace Monkland.SteamManagement
{
    class PlayerHandler
    {
        public static Player Read(Player player, ref BinaryReader reader)
        {
            player = CreatureHandler.Read(player, ref reader);
            return player;
        }

        public static void Write(Player player, ref BinaryWriter writer)
        {
            CreatureHandler.Write(player, ref writer);
        }
    }
}
