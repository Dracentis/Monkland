using Monkland.SteamManagement;
using RWCustom;
using System;

namespace Monkland.Hooks.Entities
{
    internal static class CreatureHK
    {
        public static void ApplyHook()
        {
            On.Creature.SwitchGrasps += new On.Creature.hook_SwitchGrasps(SwitchGraspsHK);
            On.Creature.ReleaseGrasp += new On.Creature.hook_ReleaseGrasp(ReleaseGraspHK);
            On.Creature.Grab += new On.Creature.hook_Grab(GrabHK);

            On.Creature.Violence += Creature_Violence;
        }

        public static void SyncBodyChunk(BodyChunk self, IntVector2 contactPoint) => self.contactPoint = contactPoint;

        public static void Sync(Creature self, bool dead) => self.dead = dead;

        private static bool isNet = false;

        public static void SetNet() => isNet = true;

        private static bool CheckNet()
        {
            if (isNet) { isNet = false; return true; }
            return false;
        }

        private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            if (self is Player && !AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkObject && !self.dead)
            {
                /*
                 * #Violence packet#
                 * packetType
                 * source
                 * damageType
                 */

                if (MonklandSteamManager.isInGame)
                {
                    MonklandSteamManager.GameManager.SendViolence(self, source, type, damage);
                }
            }

            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        }

        private static void SwitchGraspsHK(On.Creature.orig_SwitchGrasps orig, Creature self, int fromGrasp, int toGrasp)
        {
            if (CheckNet()) { orig(self, fromGrasp, toGrasp); return; }
            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.EntityManager.SendSwitch(self, fromGrasp, toGrasp);
            }
            orig(self, fromGrasp, toGrasp);
        }

        private static void ReleaseGraspHK(On.Creature.orig_ReleaseGrasp orig, Creature self, int grasp)
        {
            if (CheckNet()) { orig(self, grasp); return; }
            if (self.grasps[grasp] != null && MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.EntityManager.SendRelease(self.grasps[grasp]);
            }
            orig(self, grasp);
        }

        private static bool GrabHK(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            if (self is Player && AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkObject)
            {
                return false;
            }

            AbsPhyObjFields objs = AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject);
            if (CheckNet())
            {
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

            if (MonklandSteamManager.isInGame && objs.networkObject && !MonklandSteamManager.WorldManager.commonRooms[obj.room.abstractRoom.index].Contains(objs.owner))
            {
                return false;
            }
            if (orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying))
            {
                if (MonklandSteamManager.isInGame)
                {
                    MonklandSteamManager.EntityManager.SendGrab(self.grasps[graspUsed]);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
