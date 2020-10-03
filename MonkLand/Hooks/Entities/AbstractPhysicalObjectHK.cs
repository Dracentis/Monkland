using Monkland.SteamManagement;
using System.Collections.Generic;

namespace Monkland.Hooks.Entities
{
    public static class AbstractPhysicalObjectHK
    {
        public static void Patch()
        {
            Subs = new Dictionary<AbstractPhysicalObject, APOMonkSub>();

            CreatureHK.SubPatch();
            PlayerGraphicsHK.SubPatch();
            PlayerHK.SubPatch();
            RockHK.SubPatch();
            RoomHK.SubPatch();
            SpearHK.SubPatch();
            WeaponHK.SubPatch();

            On.AbstractPhysicalObject.ctor += new On.AbstractPhysicalObject.hook_ctor(CtorHK);
            On.AbstractPhysicalObject.AbstractObjectStick.Deactivate += new On.AbstractPhysicalObject.AbstractObjectStick.hook_Deactivate(StickDeactiveHK);
            On.AbstractPhysicalObject.AbstractSpearStick.ctor += new On.AbstractPhysicalObject.AbstractSpearStick.hook_ctor(SpearStickCtorHK);
            On.AbstractPhysicalObject.AbstractSpearAppendageStick.ctor += new On.AbstractPhysicalObject.AbstractSpearAppendageStick.hook_ctor(SpearAppStickCtorHK);
            On.AbstractPhysicalObject.ImpaledOnSpearStick.ctor += new On.AbstractPhysicalObject.ImpaledOnSpearStick.hook_ctor(SpearImpStickCtorHK);
        }

        private static Dictionary<AbstractPhysicalObject, APOMonkSub> Subs;

        public static void ClearSub() => Subs.Clear();

        public static APOMonkSub GetSub(AbstractPhysicalObject self)
        {
            if (Subs.TryGetValue(self, out APOMonkSub sub)) { return sub; }
            sub = new APOMonkSub(self);
            Subs.Add(self, sub);
            return sub;
        }

        private static void CtorHK(On.AbstractPhysicalObject.orig_ctor orig, AbstractPhysicalObject self,
            World world, AbstractPhysicalObject.AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
        {
            orig.Invoke(self, world, type, realizedObject, pos, ID);
            //if (ID.number != -1 && ID.number != 5 && ID.number != 0 && self.ID.number != 1 && self.ID.number != 2 && self.ID.number == 3) //What?
            if (ID.number < -1 && ID.number > 5)
            {
                while (self.ID.number >= -1 && self.ID.number <= 15000)
                { self.ID.number = UnityEngine.Random.Range(int.MinValue, int.MaxValue); }
            }
            GetSub(self);
        }

        private static void StickDeactiveHK(On.AbstractPhysicalObject.AbstractObjectStick.orig_Deactivate orig, AbstractPhysicalObject.AbstractObjectStick self)
        {
            if (MonklandSteamManager.isInGame && self.A != null && self.B != null && self.A.Room != null)
            {
                APOMonkSub As = GetSub(self.A);
                APOMonkSub Bs = GetSub(self.B);
                if (As.networkObject || Bs.networkObject)
                { MonklandSteamManager.EntityManager.SendDeactivate(self.A, self.B, self.A.Room); }
            }
            orig.Invoke(self);
        }

        private static void SpearStickCtorHK(On.AbstractPhysicalObject.AbstractSpearStick.orig_ctor orig, AbstractPhysicalObject.AbstractSpearStick self,
            AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int bodyPart, float angle)
        {
            if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null)
            {
                APOMonkSub As = GetSub(self.A);
                APOMonkSub Bs = GetSub(self.B);
                if (As.networkObject || Bs.networkObject)
                { MonklandSteamManager.EntityManager.SendSpearStick(self.A, self.B, self.A.Room, chunk, bodyPart, angle); }
            }
            orig.Invoke(self, spear, stuckIn, chunk, bodyPart, angle);
        }

        private static void SpearAppStickCtorHK(On.AbstractPhysicalObject.AbstractSpearAppendageStick.orig_ctor orig, AbstractPhysicalObject.AbstractSpearAppendageStick self,
            AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int appendage, int prevSeg, float distanceToNext, float angle)
        {
            if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null)
            {
                APOMonkSub As = GetSub(self.A);
                APOMonkSub Bs = GetSub(self.B);
                if (As.networkObject || Bs.networkObject)
                { MonklandSteamManager.EntityManager.SendSpearAppendageStick(self.A, self.B, self.A.Room, appendage, prevSeg, distanceToNext, angle); }
            }
            orig.Invoke(self, spear, stuckIn, appendage, prevSeg, distanceToNext, angle);
        }

        private static void SpearImpStickCtorHK(On.AbstractPhysicalObject.ImpaledOnSpearStick.orig_ctor orig, AbstractPhysicalObject.ImpaledOnSpearStick self,
            AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int onSpearPosition)
        {
            if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null)
            {
                APOMonkSub As = GetSub(self.A);
                APOMonkSub Bs = GetSub(self.B);
                if (As.networkObject || Bs.networkObject)
                { MonklandSteamManager.EntityManager.SendSpearImpaledStick(self.A, self.B, self.A.Room, chunk, onSpearPosition); }
            }
            orig.Invoke(self, spear, stuckIn, chunk, onSpearPosition);
        }
    }
}