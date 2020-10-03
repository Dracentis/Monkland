using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using Monkland.SteamManagement;
using UnityEngine;

namespace Monkland.Patches
{
    [MonoModPatch("global::AbstractPhysicalObject")]
    class patch_AbstractPhysicalObject : AbstractPhysicalObject
    {
        [MonoModIgnore]
        public patch_AbstractPhysicalObject(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID) : base(world, type, realizedObject, pos, ID)
        {
        }

        public int dist
        {
            get
            {
                if (this.ID.number == 0)
                {
                    return this.playerdist;
                }
                return this.ID.number;
            }
        }
        public int playerdist = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        public ulong owner = 0;

        public bool networkObject
        {
            get
            {
                return (MonklandSteamManager.isInGame && owner != NetworkGameManager.playerID);
            }
        }

        [MonoModIgnore]
        public extern void OriginalConstructor(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID);
        [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
        public void ctor_AbstractPhysicalObject(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
        {
            OriginalConstructor(world, type, realizedObject, pos, ID);
            if (ID.number != -1 && ID.number != 5 && ID.number != 0 && this.ID.number != 1 && this.ID.number != 2 && this.ID.number == 3)
            {
                while ((this.ID.number >= -1 && this.ID.number <= 15000))
                {
                    this.ID.number = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                }
            }
            if (ID.number == 0)
            {
                playerdist = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            if (MonklandSteamManager.isInGame)
            {
                owner = NetworkGameManager.playerID;
            }
        }

        [MonoModPatch("global::AbstractPhysicalObject.AbstractObjectStick")]
        public abstract class patch_AbstractObjectStick : AbstractPhysicalObject.AbstractObjectStick
        {
            [MonoModIgnore]
            protected patch_AbstractObjectStick(AbstractPhysicalObject A, AbstractPhysicalObject B) : base(A, B)
            {
            }

            public void Deactivate()
            {
                if (MonklandSteamManager.isInGame && A != null && B != null && A.Room != null && ((A as patch_AbstractPhysicalObject).networkObject || (B as patch_AbstractPhysicalObject).networkObject))
                {
                    MonklandSteamManager.EntityManager.SendDeactivate(A, B, A.Room);
                }
                this.A.stuckObjects.Remove(this);
                this.B.stuckObjects.Remove(this);
            }
        }

        [MonoModPatch("global::AbstractPhysicalObject.AbstractSpearStick")]
        public class patch_AbstractSpearStick : AbstractPhysicalObject.AbstractSpearStick
        {
            [MonoModIgnore]
            public patch_AbstractSpearStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int bodyPart, float angle) : base(spear, stuckIn, chunk, bodyPart, angle)
            {
            }

            [MonoModIgnore]
            public extern void OriginalConstructor(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int bodyPart, float angle);
            [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
            public void ctor_AbstractSpearStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int bodyPart, float angle)
            {
                if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null && ((spear as patch_AbstractPhysicalObject).networkObject || (stuckIn as patch_AbstractPhysicalObject).networkObject))
                {
                    MonklandSteamManager.EntityManager.SendSpearStick(A, B, A.Room, chunk, bodyPart, angle);
                }
                OriginalConstructor(spear, stuckIn, chunk, bodyPart, angle);
            }
        }

        [MonoModPatch("global::AbstractPhysicalObject.AbstractSpearAppendageStick")]
        public class patch_bstractSpearAppendageStick : AbstractPhysicalObject.AbstractSpearAppendageStick
        {
            [MonoModIgnore]
            public patch_bstractSpearAppendageStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int appendage, int prevSeg, float distanceToNext, float angle) : base(spear, stuckIn, appendage, prevSeg, distanceToNext, angle)
            {
            }

            [MonoModIgnore]
            public extern void OriginalConstructor(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int appendage, int prevSeg, float distanceToNext, float angle);
            [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
            public void ctor_AbstractSpearAppendageStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int appendage, int prevSeg, float distanceToNext, float angle)
            {
                if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null && ((spear as patch_AbstractPhysicalObject).networkObject || (stuckIn as patch_AbstractPhysicalObject).networkObject))
                {
                    MonklandSteamManager.EntityManager.SendSpearAppendageStick(A, B, A.Room, appendage, prevSeg, distanceToNext, angle);
                }
                OriginalConstructor(spear, stuckIn, appendage, prevSeg, distanceToNext, angle);
            }
        }

        [MonoModPatch("global::AbstractPhysicalObject.ImpaledOnSpearStick")]
        public class patch_ImpaledOnSpearStick : AbstractPhysicalObject.ImpaledOnSpearStick
        {
            [MonoModIgnore]
            public patch_ImpaledOnSpearStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int onSpearPosition) : base(spear, stuckIn, chunk, onSpearPosition)
            {
            }

            [MonoModIgnore]
            public extern void OriginalConstructor(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int onSpearPosition);
            [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
            public void ctor_ImpaledOnSpearStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int onSpearPosition)
            {
                if (MonklandSteamManager.isInGame && spear != null && stuckIn != null && spear.Room != null && ((spear as patch_AbstractPhysicalObject).networkObject || (stuckIn as patch_AbstractPhysicalObject).networkObject))
                {
                    MonklandSteamManager.EntityManager.SendSpearImpaledStick(A, B, A.Room, chunk, onSpearPosition);
                }
                OriginalConstructor(spear, stuckIn, chunk, onSpearPosition);
            }
        }
    }
}