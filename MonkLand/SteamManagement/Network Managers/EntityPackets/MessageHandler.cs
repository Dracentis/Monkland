using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Monkland.SteamManagement.Network_Managers
{
    internal static class MessageHandler
    {
       /* public static EntityID Read(EntityID ID, ref BinaryReader reader)
        {
            ID.number = reader.ReadInt32();
            ID.spawner = reader.ReadInt32();
            return ID;
        }
        */

        public static string Read(ref BinaryReader reader)
        {
            string message = string.Empty;
            message = reader.ReadString();
            return message;
        }

        public static void Write(string message, ref BinaryWriter writer)
        {
            writer.Write(message);
        }
    }
 
}
