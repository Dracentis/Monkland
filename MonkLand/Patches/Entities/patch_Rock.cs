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
            this.networkLife = 60;
        }

        public int networkLife = 60;

        public void Sync()
        {
            networkLife = 60;
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
                    networkLife = 60;
                    for (int i = 0; i < this.grabbedBy.Count; i++)
                    {
                        if (grabbedBy[i] != null)
                        {
                            grabbedBy[i].Release();
                            i--;
                        }
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
            if (this.thrownBy != null && this.thrownBy is Scavenger && (this.thrownBy as Scavenger).AI != null)
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
            this.firstChunk.vel = this.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * this.firstChunk.vel.magnitude;
            this.room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
            if (chunk != null)
            {
                this.room.AddObject(new ExplosionSpikes(this.room, chunk.pos, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
            }
            this.SetRandomSpin();
            return true;
        }

        public extern bool orig_HitSomething(SharedPhysics.CollisionResult result, bool eu);

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            bool hit = orig_HitSomething(result, eu);
            if (hit && MonklandSteamManager.isInGame && !(this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject && MonklandSteamManager.WorldManager.commonRooms.ContainsKey(this.room.abstractRoom.name))
            {
                MonklandSteamManager.EntityManager.SendHit(this, result.obj, result.chunk);
                MonklandSteamManager.EntityManager.Send(this, MonklandSteamManager.WorldManager.commonRooms[this.room.abstractRoom.name], true);
            }
            return hit;
        }

        public void NetThrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc)
        {
            //base.Thrown()
            this.thrownBy = thrownBy;
            this.thrownPos = thrownPos;
            this.throwDir = throwDir;
            this.firstFrameTraceFromPos = firstFrameTraceFromPos;
            this.changeDirCounter = 3;
            this.ChangeOverlap(true);
            this.firstChunk.HardSetPosition(thrownPos);
            this.GoThroughFloors = false;
            if (throwDir.x != 0)
            {
                base.firstChunk.vel.y = thrownBy.mainBodyChunk.vel.y * 0.5f;
                base.firstChunk.vel.x = thrownBy.mainBodyChunk.vel.x * 0.2f;
                BodyChunk firstChunk = base.firstChunk;
                firstChunk.vel.x = firstChunk.vel.x + (float)throwDir.x * 40f * frc;
                BodyChunk firstChunk2 = base.firstChunk;
                firstChunk2.vel.y = firstChunk2.vel.y + ((!(this is Spear)) ? 3f : 1.5f);
            }
            else
            {
                if (throwDir.y == 0)
                {
                    this.ChangeMode(Weapon.Mode.Free);
                    return;
                }
                base.firstChunk.vel.x = thrownBy.mainBodyChunk.vel.x * 0.5f;
                base.firstChunk.vel.y = (float)throwDir.y * 40f * frc;
            }
            this.ChangeMode(Weapon.Mode.Thrown);
            this.setRotation = new Vector2?(throwDir.ToVector2());
            this.rotationSpeed = 0f;
            this.meleeHitChunk = null;

            this.room.PlaySound(SoundID.Slugcat_Throw_Rock, base.firstChunk);//Rock
        }

        public extern void orig_Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu);

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            orig_Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
            if (MonklandSteamManager.isInGame && !(this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject && MonklandSteamManager.WorldManager.commonRooms.ContainsKey(this.room.abstractRoom.name))
            {
                MonklandSteamManager.EntityManager.SendThrow(this, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc);
                MonklandSteamManager.EntityManager.Send(this, MonklandSteamManager.WorldManager.commonRooms[this.room.abstractRoom.name], true);
            }
        }
    }
}