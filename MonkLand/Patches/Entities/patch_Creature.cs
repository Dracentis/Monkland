using System;
using System.Collections.Generic;
using System.Text;
using Monkland.SteamManagement;
using MonoMod;
using RWCustom;

namespace Monkland.Patches
{
    [MonoModPatch("global::Creature")]
    class patch_Creature : Creature
    {
        [MonoModIgnore]
        public patch_Creature(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
        }

        public void Sync(bool dead)
        {
            this.dead = dead;
        }

        public extern void orig_SwitchGrasps(int fromGrasp, int toGrasp);

        public void SwitchGrasps(int fromGrasp, int toGrasp)
        {
            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.EntityManager.SendSwitch(this, fromGrasp, toGrasp);
            }
            orig_SwitchGrasps(fromGrasp, toGrasp);
        }
        public void NetSwitch(int fromGrasp, int toGrasp)
        {
            orig_SwitchGrasps(fromGrasp, toGrasp);
        }

        public extern void orig_ReleaseGrasp(int grasp);
        public override void ReleaseGrasp(int grasp)
        {
            if (this.grasps[grasp] != null)
            {
                if (MonklandSteamManager.isInGame && !(this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
                    MonklandSteamManager.EntityManager.SendRelease(this.grasps[grasp]);
            }
            //if (!MonklandSteamManager.isInGame || !(this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
            orig_ReleaseGrasp(grasp);
        }
        public void NetRelease(int grasp)
        {
            orig_ReleaseGrasp(grasp);
        }

        public extern bool orig_Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying);
        public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            if (MonklandSteamManager.isInGame && (obj.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject && !MonklandSteamManager.WorldManager.commonRooms[obj.room.abstractRoom.name].Contains((obj.abstractPhysicalObject as patch_AbstractPhysicalObject).owner))
                return false;
            if (orig_Grab(obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying))
            {
                if (MonklandSteamManager.isInGame)
                {
                    MonklandSteamManager.EntityManager.SendGrab(this.grasps[graspUsed]);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool NetGrab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool pacifying)
        {
            if (this.grasps[graspUsed] != null && this.grasps[graspUsed].grabbed == obj)
            {
                this.NetRelease(graspUsed);
                this.grasps[graspUsed] = new Creature.Grasp(this, obj, graspUsed, chunkGrabbed, shareability, dominance, true);
                obj.Grabbed(this.grasps[graspUsed]);
                new AbstractPhysicalObject.CreatureGripStick(this.abstractCreature, obj.abstractPhysicalObject, graspUsed, pacifying || obj.TotalMass < base.TotalMass);
                return true;
            }
            for (int i = obj.grabbedBy.Count - 1; i >= 0; i--)
            {
                if (obj.grabbedBy[i].ShareabilityConflict(shareability))
                {
                    obj.grabbedBy[i].Release();
                }
            }
            if (this.grasps[graspUsed] != null)
            {
                this.NetRelease(graspUsed);
            }
            this.grasps[graspUsed] = new Creature.Grasp(this, obj, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
            obj.Grabbed(this.grasps[graspUsed]);
            new AbstractPhysicalObject.CreatureGripStick(this.abstractCreature, obj.abstractPhysicalObject, graspUsed, pacifying || obj.TotalMass < base.TotalMass);
            return true;
        }
    }
}