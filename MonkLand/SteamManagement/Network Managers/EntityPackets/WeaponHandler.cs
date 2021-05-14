using Monkland.Hooks.Entities;
using System.Diagnostics;
using System.IO;

namespace Monkland.SteamManagement
{
    internal class WeaponHandler
    {
        /// <summary>
        /// Writes Weapon packet 
        /// <para>[PLAYERCARRYABLE | byte changeDirCounter | float closesCritDist | float exitThrownmodeSpeed | vec2 firstFrameTraceFromPos | byte mode | vec2 rot | float rotSpeed | int throwMode | Dist thrownBy | dist thrownClosetCreate | vec2 thrownPos]</para>
        /// <para>[min 130 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(Weapon weapon, ref BinaryWriter writer)
        {
            PlayerCarryableItemHandler.Write(weapon, ref writer);
            writer.Write((byte)weapon.changeDirCounter);
            writer.Write(weapon.closestCritDist);
            writer.Write(weapon.exitThrownModeSpeed);
            Vector2NHandler.Write(weapon.firstFrameTraceFromPos, ref writer);
            writer.Write((byte)weapon.mode);
            Vector2Handler.Write(weapon.rotation, ref writer);
            writer.Write(weapon.rotationSpeed);
            writer.Write(weapon.throwModeFrames);
            DistHandler.Write(weapon.thrownBy, ref writer);
            DistHandler.Write(weapon.thrownClosestToCreature, ref writer);
            Vector2Handler.Write(weapon.thrownPos, ref writer);
        }


        public static void Read(Weapon weapon, ref BinaryReader reader)
        {
            PlayerCarryableItemHandler.Read(weapon, ref reader);
            weapon.changeDirCounter = reader.ReadInt32();
            weapon.closestCritDist = reader.ReadSingle();
            weapon.exitThrownModeSpeed = reader.ReadSingle();
            weapon.firstFrameTraceFromPos = Vector2NHandler.Read(ref reader);

            Weapon.Mode mode = (Weapon.Mode)reader.ReadInt32();
            if (mode != weapon.mode)
            {
                weapon.ChangeMode(mode);
            }
            if (mode == Weapon.Mode.Thrown && weapon.grabbedBy.Count > 0)
            {
                weapon.AllGraspsLetGoOfThisObject(false);
            }
            weapon.mode = mode;
            weapon.rotation = Vector2Handler.Read(ref reader);
            weapon.rotationSpeed = reader.ReadSingle();
            weapon.throwModeFrames = reader.ReadInt32();
            weapon.thrownBy = DistHandler.ReadCreature(ref weapon.thrownBy, ref reader, weapon.room);
            weapon.thrownClosestToCreature = DistHandler.ReadCreature(ref weapon.thrownClosestToCreature, ref reader, weapon.room);
            weapon.thrownPos = Vector2Handler.Read(ref reader);
            // Refresh network
            AbstractPhysicalObjectHK.Refresh(weapon.abstractPhysicalObject, WeaponHK.defaultNetworkLife);
        }
    }
}
