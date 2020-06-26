using System.IO;
using UnityEngine;
using Monkland.Patches;

namespace Monkland.SteamManagement
{
    static class DistHandler
    {
        public static Creature ReadCreature(ref Creature creature, ref BinaryReader reader, Room room)
        {
            if (!reader.ReadBoolean())
            {
                return null;
            }
            int dist = reader.ReadInt32();
            if (creature != null && creature.abstractPhysicalObject != null && (creature.abstractPhysicalObject as patch_AbstractPhysicalObject).dist == dist)
            {
                return creature;
            }
            Creature target = null;
            foreach (AbstractCreature cr in room.abstractRoom.creatures)
            {
                if (((cr as AbstractPhysicalObject) as patch_AbstractPhysicalObject).dist == dist && cr.realizedCreature != null)
                {
                    target = cr.realizedCreature;
                }
            }
            return target;
        }
        public static Creature ReadCreature(ref BinaryReader reader, Room room)
        {
            int dist = reader.ReadInt32();
            Creature target = null;
            foreach (AbstractCreature cr in room.abstractRoom.creatures)
            {
                if (((cr as AbstractPhysicalObject) as patch_AbstractPhysicalObject).dist == dist && cr.realizedCreature != null)
                {
                    target = cr.realizedCreature;
                }
            }
            return target;
        }

        public static PhysicalObject ReadPhysicalObject(ref PhysicalObject physicalObject, ref BinaryReader reader, Room room)
        {
            if (!reader.ReadBoolean())
            {
                return null;
            }
            int dist = reader.ReadInt32();
            if (physicalObject != null && physicalObject.abstractPhysicalObject != null && (physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist == dist)
            {
                return physicalObject;
            }
            PhysicalObject target = null;
            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    if (room.physicalObjects[i][j] != null && room.physicalObjects[i][j].abstractPhysicalObject != null && ((room.physicalObjects[i][j].abstractPhysicalObject as AbstractPhysicalObject) as patch_AbstractPhysicalObject).dist == dist)
                    {
                        target = room.physicalObjects[i][j];
                    }
                }
            }
            return target;
        }

        public static PhysicalObject ReadPhysicalObject(ref BinaryReader reader, Room room)
        {
            int dist = reader.ReadInt32();
            PhysicalObject target = null;
            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    if (room.physicalObjects[i][j] != null && room.physicalObjects[i][j].abstractPhysicalObject != null && ((room.physicalObjects[i][j].abstractPhysicalObject as AbstractPhysicalObject) as patch_AbstractPhysicalObject).dist == dist)
                    {
                        target = room.physicalObjects[i][j];
                    }
                }
            }
            return target;
        }

        public static void Write(PhysicalObject target, ref BinaryWriter writer)
        {
            if (target == null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
            }
            writer.Write((target.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
        }
    }
}