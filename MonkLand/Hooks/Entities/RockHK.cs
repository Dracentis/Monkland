using Monkland.SteamManagement;
using RWCustom;
using UnityEngine;

namespace Monkland.Hooks.Entities
{
    internal static class RockHK
    {
        public static void ApplyHook()
        {
            On.Rock.ctor += new On.Rock.hook_ctor(CtorHK);
            On.Rock.Update += new On.Rock.hook_Update(UpdateHK);
            On.Rock.HitSomething += new On.Rock.hook_HitSomething(HitSomethingHK);
            On.Rock.Thrown += new On.Rock.hook_Thrown(ThrownHK);
        }

        private static bool isNet = false;

        public static void SetNet() => isNet = true;

        private static bool CheckNet()
        {
            if (isNet) { isNet = false; return true; }
            return false;
        }

        public static void Sync(Rock self) => AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkLife = 60;

        private static void CtorHK(On.Rock.orig_ctor orig, Rock self, AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            orig(self, abstractPhysicalObject, world);
            AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkLife = 60;
        }

        private static void UpdateHK(On.Rock.orig_Update orig, Rock self, bool eu)
        {
            orig(self, eu);
            AbsPhyObjFields field = AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject);
            if (field.networkObject)
            {
                if (field.networkLife > 0)
                { field.networkLife--; }
                else
                {
                    field.networkLife = 60;
                    for (int i = 0; i < self.grabbedBy.Count; i++)
                    {
                        if (self.grabbedBy[i] != null)
                        {
                            self.grabbedBy[i].Release();
                            i--;
                        }
                    }
                    self.Destroy();
                }
            }
        }

        private static bool HitSomethingHK(On.Rock.orig_HitSomething orig, Rock self, SharedPhysics.CollisionResult result, bool eu)
        {
            bool hit = orig(self, result, eu);
            if (CheckNet()) { return hit; }

            if (hit && MonklandSteamManager.isInGame && !AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkObject && MonklandSteamManager.WorldManager.commonRooms.ContainsKey(self.room.abstractRoom.index))
            {
                MonklandSteamManager.EntityManager.SendHit(self, result.obj, result.chunk);
                MonklandSteamManager.EntityManager.Send(self, MonklandSteamManager.WorldManager.commonRooms[self.room.abstractRoom.index], true);
            }
            return hit;
        }

        private static void ThrownHK(On.Rock.orig_Thrown orig, Rock self,
            Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
            if (CheckNet()) { return; }
            if (MonklandSteamManager.isInGame && !AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkObject && MonklandSteamManager.WorldManager.commonRooms.ContainsKey(self.room.abstractRoom.index))
            {
                MonklandSteamManager.EntityManager.SendThrow(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc);
                MonklandSteamManager.EntityManager.Send(self, MonklandSteamManager.WorldManager.commonRooms[self.room.abstractRoom.index], true);
            }
        }
    }
}