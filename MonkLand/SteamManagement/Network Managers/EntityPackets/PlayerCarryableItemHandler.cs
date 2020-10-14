using Monkland.Hooks.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Monkland.SteamManagement
{
    internal class PlayerCarryableItemHandler
    {
        /// <summary>
        /// Writes Weapon packet 
        /// <para>[PHYSICALOBJ | byte blink]</para>
        /// <para>[min 78 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(PlayerCarryableItem playerCarryableItem, ref BinaryWriter writer)
        {
            //PhysicalObjectHandler.Write(playerCarryableItem, ref writer);
            writer.Write((byte)playerCarryableItem.blink);
        }

        public static void Read(PlayerCarryableItem playerCarryableItem, ref BinaryReader reader)
        {
            //PhysicalObjectHandler.Read(playerCarryableItem, ref reader, ref distinguisher);
            playerCarryableItem.blink = reader.ReadByte();
            AbstractPhysicalObjectHK.GetField(playerCarryableItem.abstractPhysicalObject).networkLife = 60;
        }
    }
}
