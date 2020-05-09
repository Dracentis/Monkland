using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RWCustom;

namespace Monkland.SteamManagement
{
    class CreatureHandler
    {

        public static Creature Read(Creature creature, ref BinaryReader reader)
        {
            creature.abstractPhysicalObject = AbstractCreatureHandler.Read(creature.abstractCreature, ref reader);
            creature = PhysicalObjectHandler.Read(creature, ref reader);
            //creature.blind = reader.ReadInt32();
            (creature as Patches.patch_Creature).Sync(reader.ReadBoolean());
            creature.enteringShortCut = IntVector2NHandler.Read(ref reader);
            creature.lastCoord = WorldCoordinateHandler.Read(ref reader);
            creature.leechedOut = reader.ReadBoolean();
            creature.newToRoomInvinsibility = reader.ReadInt32();
            creature.NPCTransportationDestination = WorldCoordinateHandler.Read(ref reader);
            creature.shortcutDelay = reader.ReadInt32();
            //Grasps should be synced here!!
            return creature;
        }

        public static Player Read(Player creature, ref BinaryReader reader)
        {
            creature.abstractPhysicalObject = AbstractCreatureHandler.Read(creature.abstractCreature, ref reader);
            creature = PhysicalObjectHandler.Read(creature, ref reader);
            //creature.blind = reader.ReadInt32();
            (creature as Patches.patch_Player).Sync(reader.ReadBoolean());
            creature.enteringShortCut = IntVector2NHandler.Read(ref reader);
            creature.lastCoord = WorldCoordinateHandler.Read(ref reader);
            creature.leechedOut = reader.ReadBoolean();
            creature.newToRoomInvinsibility = reader.ReadInt32();
            creature.NPCTransportationDestination = WorldCoordinateHandler.Read(ref reader);
            creature.shortcutDelay = reader.ReadInt32();
            //Grasps should be synced here!!
            return creature;
        }

        public static void Write(Creature creature, ref BinaryWriter writer)
        {
            AbstractCreatureHandler.Write(creature.abstractCreature, ref writer);
            PhysicalObjectHandler.Write(creature, ref writer);
            writer.Write(creature.blind);
            writer.Write(creature.dead);
            IntVector2NHandler.Write(creature.enteringShortCut, ref writer);
            WorldCoordinateHandler.Write(creature.lastCoord, ref writer);
            writer.Write(creature.leechedOut);
            writer.Write(creature.newToRoomInvinsibility);
            WorldCoordinateHandler.Write(creature.NPCTransportationDestination, ref writer);
            writer.Write(creature.shortcutDelay);
        }
    }
}
