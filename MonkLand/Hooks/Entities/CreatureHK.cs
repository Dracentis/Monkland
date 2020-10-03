using Monkland.SteamManagement;
using RWCustom;
using System;

namespace Monkland.Hooks.Entities
{
    internal static class CreatureHK
    {
        public static void SubPatch()
        {
            On.Creature.SwitchGrasps += new On.Creature.hook_SwitchGrasps(SwitchGraspsHK);
            On.Creature.ReleaseGrasp += new On.Creature.hook_ReleaseGrasp(ReleaseGraspHK);
            On.Creature.Grab += new On.Creature.hook_Grab(GrabHK);
        }

        public static void SyncBodyChunk(BodyChunk self, IntVector2 contactPoint) => self.contactPoint = contactPoint;

        public static void Sync(Creature self, bool dead) => self.dead = dead;

        public static void SyncPhysicalObject(PhysicalObject self, BodyChunk[] bodyChunks) => self.bodyChunks = bodyChunks;

        private static bool CheckNet(params int[] flags)
        {
            foreach (int i in flags) { if (i < 0) { return true; } }
            return false;
        }

        private static void SwitchGraspsHK(On.Creature.orig_SwitchGrasps orig, Creature self, int fromGrasp, int toGrasp)
        {
            if (CheckNet(fromGrasp, toGrasp)) { orig.Invoke(self, Math.Abs(fromGrasp), Math.Abs(toGrasp)); return; }
            if (MonklandSteamManager.isInGame)
            { MonklandSteamManager.EntityManager.SendSwitch(self, fromGrasp, toGrasp); }
            orig.Invoke(self, fromGrasp, toGrasp);
        }

        private static void ReleaseGraspHK(On.Creature.orig_ReleaseGrasp orig, Creature self, int grasp)
        {
            if (CheckNet(grasp)) { orig.Invoke(self, Math.Abs(grasp)); return; }
            if (self.grasps[grasp] != null && MonklandSteamManager.isInGame)
            { MonklandSteamManager.EntityManager.SendRelease(self.grasps[grasp]); }
            orig.Invoke(self, grasp);
        }

        private static bool GrabHK(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            if (self is Player && AbstractPhysicalObjectHK.GetSub(self.abstractPhysicalObject).networkObject) { return false; }

            APOMonkSub objs = AbstractPhysicalObjectHK.GetSub(obj.abstractPhysicalObject);
            if (CheckNet(graspUsed, chunkGrabbed))
            {
                graspUsed = Math.Abs(graspUsed); chunkGrabbed = Math.Abs(chunkGrabbed);
                if (self.grasps[graspUsed] != null && self.grasps[graspUsed].grabbed == obj)
                {
                    self.ReleaseGrasp(-graspUsed); // NetReleaseGrasp
                    self.grasps[graspUsed] = new Creature.Grasp(self, obj, graspUsed, chunkGrabbed, shareability, dominance, true);
                    obj.Grabbed(self.grasps[graspUsed]);
                    new AbstractPhysicalObject.CreatureGripStick(self.abstractCreature, obj.abstractPhysicalObject, graspUsed, pacifying || obj.TotalMass < self.TotalMass);
                    return true;
                }
                for (int i = obj.grabbedBy.Count - 1; i >= 0; i--)
                {
                    if (obj.grabbedBy[i].ShareabilityConflict(shareability)) { obj.grabbedBy[i].Release(); }
                }
                if (self.grasps[graspUsed] != null) { self.ReleaseGrasp(-graspUsed); } // NetReleaseGrasp
                self.grasps[graspUsed] = new Creature.Grasp(self, obj, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
                obj.Grabbed(self.grasps[graspUsed]);
                new AbstractPhysicalObject.CreatureGripStick(self.abstractCreature, obj.abstractPhysicalObject, graspUsed, pacifying || obj.TotalMass < self.TotalMass);
                return true;
            }

            if (MonklandSteamManager.isInGame && objs.networkObject && !MonklandSteamManager.WorldManager.commonRooms[obj.room.abstractRoom.name].Contains(objs.owner))
            { return false; }
            if (orig.Invoke(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying))
            {
                if (MonklandSteamManager.isInGame) { MonklandSteamManager.EntityManager.SendGrab(self.grasps[graspUsed]); }
                return true;
            }
            else { return false; }
        }
    }
}