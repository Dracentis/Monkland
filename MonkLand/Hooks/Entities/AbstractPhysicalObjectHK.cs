using Monkland.SteamManagement;
using System.Collections.Generic;

namespace Monkland.Hooks.Entities
{
    public static class AbstractPhysicalObjectHK
    {
        public static void ApplyHook()
        {
            fields = new Dictionary<AbstractPhysicalObject, AbstractObjFields>();

            On.AbstractPhysicalObject.ctor += new On.AbstractPhysicalObject.hook_ctor(CtorHK);
            On.AbstractPhysicalObject.AbstractObjectStick.Deactivate += new On.AbstractPhysicalObject.AbstractObjectStick.hook_Deactivate(StickDeactiveHK);
            On.AbstractPhysicalObject.AbstractSpearStick.ctor += new On.AbstractPhysicalObject.AbstractSpearStick.hook_ctor(SpearStickCtorHK);
            On.AbstractPhysicalObject.AbstractSpearAppendageStick.ctor += new On.AbstractPhysicalObject.AbstractSpearAppendageStick.hook_ctor(SpearAppStickCtorHK);
            On.AbstractPhysicalObject.ImpaledOnSpearStick.ctor += new On.AbstractPhysicalObject.ImpaledOnSpearStick.hook_ctor(SpearImpStickCtorHK);
        }

        private static Dictionary<AbstractPhysicalObject, AbstractObjFields> fields;

        public static void Sync(AbstractPhysicalObject self) => AbstractPhysicalObjectHK.GetField(self).networkLife = 60;

        public static void ClearFields() => fields.Clear();

        public static AbstractObjFields GetField(AbstractPhysicalObject self)
        {
            if (fields.TryGetValue(self, out AbstractObjFields field))
            {
                return field;
            }

            field = new AbstractObjFields(self);
            fields.Add(self, field);
            return field;
        }

        private static void CtorHK(On.AbstractPhysicalObject.orig_ctor orig, AbstractPhysicalObject self,
            World world, AbstractPhysicalObject.AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, type, realizedObject, pos, ID);
            //if (ID.number != -1 && ID.number != 5 && ID.number != 0 && self.ID.number != 1 && self.ID.number != 2 && self.ID.number == 3) //What?
            if (ID.number < -1 && ID.number > 5)
            {
                while (self.ID.number >= -1 && self.ID.number <= 15000)
                { 
                    self.ID.number = UnityEngine.Random.Range(int.MinValue, int.MaxValue); 
                }
            }
            GetField(self);
        }

        private static void StickDeactiveHK(On.AbstractPhysicalObject.AbstractObjectStick.orig_Deactivate orig, AbstractPhysicalObject.AbstractObjectStick self)
        {
            if (MonklandSteamManager.isInGame && self.A != null && self.B != null && self.A.Room != null)
            {
                AbstractObjFields As = GetField(self.A);
                AbstractObjFields Bs = GetField(self.B);
                if (As.isNetworkObject || Bs.isNetworkObject)
                { 
                    MonklandSteamManager.EntityManager.SendDeactivate(self.A, self.B, self.A.Room); 
                }
            }
            orig(self);
        }

        private static void SpearStickCtorHK(On.AbstractPhysicalObject.AbstractSpearStick.orig_ctor orig, AbstractPhysicalObject.AbstractSpearStick self,
            AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int bodyPart, float angle)
        {
            if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null && self.A != null && self.B != null)
            {
                AbstractObjFields As = GetField(self.A);
                AbstractObjFields Bs = GetField(self.B);
                if (As.isNetworkObject || Bs.isNetworkObject)
                { 
                    MonklandSteamManager.EntityManager.SendSpearStick(self.A, self.B, self.A.Room, chunk, bodyPart, angle);
                }
            }
            orig(self, spear, stuckIn, chunk, bodyPart, angle);
        }

        private static void SpearAppStickCtorHK(On.AbstractPhysicalObject.AbstractSpearAppendageStick.orig_ctor orig, AbstractPhysicalObject.AbstractSpearAppendageStick self,
            AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int appendage, int prevSeg, float distanceToNext, float angle)
        {
            if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null && self.A != null && self.B != null)
            {
                AbstractObjFields As = GetField(self.A);
                AbstractObjFields Bs = GetField(self.B);
                if (As.isNetworkObject || Bs.isNetworkObject)
                { 
                    MonklandSteamManager.EntityManager.SendSpearAppendageStick(self.A, self.B, self.A.Room, appendage, prevSeg, distanceToNext, angle); 
                }
            }
            orig(self, spear, stuckIn, appendage, prevSeg, distanceToNext, angle);
        }

        private static void SpearImpStickCtorHK(On.AbstractPhysicalObject.ImpaledOnSpearStick.orig_ctor orig, AbstractPhysicalObject.ImpaledOnSpearStick self,
            AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int onSpearPosition)
        {
            if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null && self.A != null && self.B != null)
            {
                AbstractObjFields As = GetField(self.A);
                AbstractObjFields Bs = GetField(self.B);

                if (As.isNetworkObject || Bs.isNetworkObject)
                { 
                    MonklandSteamManager.EntityManager.SendSpearImpaledStick(self.A, self.B, self.A.Room, chunk, onSpearPosition); 
                }
            }
            orig(self, spear, stuckIn, chunk, onSpearPosition);
        }
    }
}