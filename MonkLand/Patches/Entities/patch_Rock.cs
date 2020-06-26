using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using Monkland.SteamManagement;
using MonoMod;
using UnityEngine;
using RWCustom;

namespace Monkland.Patches
{
    [MonoModPatch("global::Rock")]
    class patch_Rock : Rock
    {
        [MonoModIgnore]
        public patch_Rock(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
        }

        [MonoModIgnore]
        public extern void OriginalConstructor(AbstractPhysicalObject abstractPhysicalObject, World world);
        [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
        public void ctor_Rock(AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            OriginalConstructor(abstractPhysicalObject, world);
            this.networkLife = 120;
        }

        public int networkLife = 120;

        public void Sync()
        {
            networkLife = 120;
        }

        public extern void orig_Update(bool eu);

        public void Update(bool eu)
        {
            orig_Update(eu);
            if ((this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
            {
                if (this.networkLife > 0)
                {
                    this.networkLife--;
                }
                else
                {
                    networkLife = 120;
                    foreach (Creature.Grasp grasp in this.grabbedBy){
                        grasp.Release();
                    }
                    this.Destroy();
                }
            }
        }

        public bool NetHit(PhysicalObject hit, BodyChunk chunk)
        {
            if (hit == null)
            {
                return false;
            }
            if (this.thrownBy is Scavenger && (this.thrownBy as Scavenger).AI != null)
            {
                (this.thrownBy as Scavenger).AI.HitAnObjectWithWeapon(this, hit);
            }
            this.vibrate = 20;
            this.ChangeMode(Weapon.Mode.Free);
            if (hit is Creature)
            {
                (hit as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass), chunk, null, Creature.DamageType.Blunt, 0.01f, 45f);
            }
            else if (chunk != null)
            {
                chunk.vel += base.firstChunk.vel * base.firstChunk.mass / chunk.mass;
            }
            base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
            this.room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
            if (chunk != null)
            {
                this.room.AddObject(new ExplosionSpikes(this.room, chunk.pos, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
            }
            this.SetRandomSpin();
            return true;
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj == null)
            {
                return false;
            }
            if (!(this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject && result.obj != null)
            {
                MonklandSteamManager.EntityManager.SendHit(NetworkEntityManager.HitType.Rock, this, result.obj, result.chunk);
            }
            if (this.thrownBy is Scavenger && (this.thrownBy as Scavenger).AI != null)
            {
                (this.thrownBy as Scavenger).AI.HitAnObjectWithWeapon(this, result.obj);
            }
            this.vibrate = 20;
            this.ChangeMode(Weapon.Mode.Free);
            if (result.obj is Creature)
            {
                (result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, 0.01f, 45f);
            }
            else if (result.chunk != null)
            {
                result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
            }
            else if (result.onAppendagePos != null)
            {
                (result.obj as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
            }
            base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
            this.room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
            if (result.chunk != null)
            {
                this.room.AddObject(new ExplosionSpikes(this.room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
            }
            this.SetRandomSpin();
            return true;
        }
    }
}
