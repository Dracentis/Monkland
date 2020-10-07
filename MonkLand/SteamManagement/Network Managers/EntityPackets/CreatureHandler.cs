using Monkland.Hooks.Entities;
using System.IO;

namespace Monkland.SteamManagement
{
    internal class CreatureHandler
    {
        public static void Read(Creature creature, ref BinaryReader reader)
        {
            AbstractCreatureHandler.Read(creature, ref reader);
            PhysicalObjectHandler.Read(creature, ref reader);
            //creature.blind = reader.ReadInt32();
            CreatureHK.Sync(creature, reader.ReadBoolean());
            creature.enteringShortCut = IntVector2NHandler.Read(ref reader);
            creature.lastCoord = WorldCoordinateHandler.Read(ref reader);
            creature.leechedOut = reader.ReadBoolean();
            creature.newToRoomInvinsibility = reader.ReadInt32();
            creature.NPCTransportationDestination = WorldCoordinateHandler.Read(ref reader);
            creature.shortcutDelay = reader.ReadInt32();
            // return creature;
        }

        /*
        public static Player Read(Player creature, ref BinaryReader reader)
        {
            creature.abstractPhysicalObject = AbstractCreatureHandler.Read(creature.abstractCreature, ref reader);
            creature = PhysicalObjectHandler.Read(creature, ref reader) as Player;
            //creature.blind = reader.ReadInt32();
            PlayerHK.Sync(creature, reader.ReadBoolean());
            creature.enteringShortCut = IntVector2NHandler.Read(ref reader);
            creature.lastCoord = WorldCoordinateHandler.Read(ref reader);
            creature.leechedOut = reader.ReadBoolean();
            creature.newToRoomInvinsibility = reader.ReadInt32();
            creature.NPCTransportationDestination = WorldCoordinateHandler.Read(ref reader);
            creature.shortcutDelay = reader.ReadInt32();
            /*int len = reader.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                if (i < creature.grasps.Length && creature.grasps[i] != null)
                {
                    creature.grasps[i] = GraspHandler.Read(ref creature.grasps[i], creature, ref reader, creature.room);
                }
                else
                {
                    Creature.Grasp grasp = GraspHandler.Read(creature, ref reader, creature.room);
                    if (i < creature.grasps.Length && grasp != null && grasp.grabbed != null && grasp.grabber != null)
                    {
                        MonklandSteamManager.Log("Grabbed Rock!");
                        (creature as Player).switchHandsCounter = 0;
                        ((creature as Player) as patch_Player).setWantToPickUp(0);
                        (creature as Player).noPickUpOnRelease = 20;
                        if (creature.grasps[i] != null)
                        {
                            creature.ReleaseGrasp(i);
                        }
                        creature.grasps[i] = new Creature.Grasp(creature, grasp.grabbed, i, grasp.chunkGrabbed, grasp.shareability, grasp.dominance, grasp.pacifying);
                        grasp.grabbed.Grabbed(creature.grasps[i]);
                        new AbstractPhysicalObject.CreatureGripStick(creature.abstractCreature, grasp.grabbed.abstractPhysicalObject, i, grasp.pacifying);
                    }
                }
            }*/
        //return creature;
        //}

        public static void Write(Creature creature, ref BinaryWriter writer)
        {
            AbstractCreatureHandler.Write(creature.abstractCreature, ref writer);
            PhysicalObjectHandler.Write(creature, ref writer);
            //writer.Write(creature.blind);
            writer.Write(creature.dead);
            IntVector2NHandler.Write(creature.enteringShortCut, ref writer);
            WorldCoordinateHandler.Write(creature.lastCoord, ref writer);
            writer.Write(creature.leechedOut);
            writer.Write(creature.newToRoomInvinsibility);
            WorldCoordinateHandler.Write(creature.NPCTransportationDestination, ref writer);
            writer.Write(creature.shortcutDelay);
            /*for(int i = 0; i < creature.grasps.Length; i++)
            {
                GraspHandler.Write(creature.grasps[i], ref writer);
            }*/
        }
    }
}
