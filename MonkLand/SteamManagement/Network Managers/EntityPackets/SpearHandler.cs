
using System;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class SpearHandler
    {

        /// <summary>
        /// Writes Spear packet 
        /// <para>[WEAPON | bool alwaysStickInWalls | int pinToWallCounter | float spearDamageBonus | DIST StuckInObject | vec2 StuckInWall | float stuckRotation]</para>
        /// <para>[min 155 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(Spear spear, ref BinaryWriter writer)
        {
            WeaponHandler.Write(spear, ref writer);
            writer.Write(spear.alwaysStickInWalls);
            writer.Write(spear.pinToWallCounter);
            writer.Write(spear.spearDamageBonus);
            DistHandler.Write(spear.stuckInObject, ref writer);
            Vector2NHandler.Write(spear.stuckInWall, ref writer);
            writer.Write(spear.stuckRotation);
        }

        public static void Read(Spear spear, ref BinaryReader reader)
        {
            WeaponHandler.Read(spear, ref reader);
            spear.alwaysStickInWalls = reader.ReadBoolean();
            spear.pinToWallCounter = reader.ReadInt32();
            spear.spearDamageBonus = reader.ReadSingle();
            spear.stuckInObject = DistHandler.ReadPhysicalObject(ref spear.stuckInObject, ref reader, spear.room);
            spear.stuckInWall = Vector2NHandler.Read(ref reader);
            spear.stuckRotation = reader.ReadSingle();
        }

    }
}
