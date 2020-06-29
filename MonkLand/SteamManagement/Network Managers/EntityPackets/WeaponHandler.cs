using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RWCustom;

namespace Monkland.SteamManagement
{
    class WeaponHandler
    {

        public static Weapon Read(Weapon weapon, ref BinaryReader reader)
        {
            weapon = PhysicalObjectHandler.Read(weapon, ref reader);
            weapon.changeDirCounter = reader.ReadInt32();
            weapon.closestCritDist = reader.ReadSingle();
            weapon.exitThrownModeSpeed = reader.ReadSingle();
            weapon.firstFrameTraceFromPos = Vector2NHandler.Read(ref reader);
            //Weapon.Mode lastMode = (Weapon.Mode)reader.ReadInt32();
            Weapon.Mode mode = (Weapon.Mode)reader.ReadInt32();
            if (mode != weapon.mode)
            {
                //weapon.ChangeOverlap(true);
                weapon.ChangeMode(mode);
            }
            if (mode == Weapon.Mode.Thrown && weapon.grabbedBy.Count > 0)
            {
                weapon.AllGraspsLetGoOfThisObject(false);
            }
            //weapon.lastMode = lastMode;
            weapon.mode = mode;
            weapon.rotation = Vector2Handler.Read(ref reader);
            weapon.rotationSpeed = reader.ReadSingle();
            weapon.throwModeFrames = reader.ReadInt32();
            weapon.thrownBy = DistHandler.ReadCreature(ref weapon.thrownBy, ref reader, weapon.room);
            weapon.thrownClosestToCreature = DistHandler.ReadCreature(ref weapon.thrownClosestToCreature, ref reader, weapon.room);
            weapon.thrownPos = Vector2Handler.Read(ref reader);
            return weapon;
        }

        public static Rock Read(Rock weapon, ref BinaryReader reader)
        {
            weapon = PhysicalObjectHandler.Read(weapon, ref reader);
            weapon.changeDirCounter = reader.ReadInt32();
            weapon.closestCritDist = reader.ReadSingle();
            weapon.exitThrownModeSpeed = reader.ReadSingle();
            weapon.firstFrameTraceFromPos = Vector2NHandler.Read(ref reader);
            //Weapon.Mode lastMode = (Weapon.Mode)reader.ReadInt32();
            Weapon.Mode mode = (Weapon.Mode)reader.ReadInt32();
            if (mode != weapon.mode)
            {
                //weapon.ChangeOverlap(true);
                weapon.ChangeMode(mode);
            }
            if (mode == Weapon.Mode.Thrown && weapon.grabbedBy.Count > 0)
            {
                weapon.AllGraspsLetGoOfThisObject(false);
            }
            //weapon.lastMode = lastMode;
            weapon.mode = mode;
            weapon.rotation = Vector2Handler.Read(ref reader);
            weapon.rotationSpeed = reader.ReadSingle();
            weapon.throwModeFrames = reader.ReadInt32();
            weapon.thrownBy = DistHandler.ReadCreature(ref weapon.thrownBy, ref reader, weapon.room);
            weapon.thrownClosestToCreature = DistHandler.ReadCreature(ref weapon.thrownClosestToCreature, ref reader, weapon.room);
            weapon.thrownPos = Vector2Handler.Read(ref reader);
            return weapon;
        }

        public static void Write(Weapon weapon, ref BinaryWriter writer)
        {
            PhysicalObjectHandler.Write(weapon, ref writer);
            writer.Write(weapon.changeDirCounter);
            writer.Write(weapon.closestCritDist);
            writer.Write(weapon.exitThrownModeSpeed);
            Vector2NHandler.Write(weapon.firstFrameTraceFromPos, ref writer);
            //writer.Write((int)weapon.lastMode);
            writer.Write((int)weapon.mode);
            Vector2Handler.Write(weapon.rotation, ref writer);
            writer.Write(weapon.rotationSpeed);
            writer.Write(weapon.throwModeFrames);
            DistHandler.Write(weapon.thrownBy, ref writer);
            DistHandler.Write(weapon.thrownClosestToCreature, ref writer);
            Vector2Handler.Write(weapon.thrownPos, ref writer);
        }
    }
}
