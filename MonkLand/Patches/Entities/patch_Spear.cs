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
    [MonoModPatch("global::Spear")]
    class patch_Spear : Spear
    {
        [MonoModIgnore]
        public patch_Spear(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
        }

        [MonoModIgnore]
        private int stuckBodyPart;

        [MonoModIgnore]
        private int stuckInChunkIndex;

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
                        if(grabbedBy[i] != null)
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
            bool flag = false;
            if (this.abstractPhysicalObject.world.game.IsArenaSession && this.abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && this.thrownBy != null && this.thrownBy is Player && hit is Creature)
            {
                flag = true;
                if ((hit as Creature).State is HealthState && ((hit as Creature).State as HealthState).health <= 0f)
                {
                    flag = false;
                }
                else if (!((hit as Creature).State is HealthState) && (hit as Creature).State.dead)
                {
                    flag = false;
                }
            }
            if (hit is Creature)
            {
                (hit as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass * 2f), chunk, null, Creature.DamageType.Stab, this.spearDamageBonus, 20f);
            }
            else if (chunk != null)
            {
                chunk.vel += this.firstChunk.vel * this.firstChunk.mass / chunk.mass;
            }
            if (hit is Creature && (hit as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, UnityEngine.Random.value), chunk, null, base.firstChunk.vel))
            {
                this.room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk);
                this.stuckInObject = hit;
                this.ChangeMode(Weapon.Mode.StuckInCreature);
                if (chunk != null)
                {
                    this.stuckInChunkIndex = chunk.index;
                    if (this.spearDamageBonus > 0.9f && this.room.GetTile(this.room.GetTilePosition(this.stuckInChunk.pos) + this.throwDir).Terrain == Room.Tile.TerrainType.Solid && this.room.GetTile(this.stuckInChunk.pos).Terrain == Room.Tile.TerrainType.Air)
                    {
                        this.stuckInWall = new Vector2?(this.room.MiddleOfTile(this.stuckInChunk.pos) + this.throwDir.ToVector2() * (10f - this.stuckInChunk.rad));
                        this.stuckRotation = Custom.VecToDeg(this.rotation);
                        this.stuckBodyPart = -1;
                        this.pinToWallCounter = 300;
                    }
                    else if (this.stuckBodyPart == -1)
                    {
                        this.stuckRotation = Custom.Angle(this.throwDir.ToVector2(), this.stuckInChunk.Rotation);
                    }
                    new AbstractPhysicalObject.AbstractSpearStick(this.abstractPhysicalObject, (hit as Creature).abstractCreature, this.stuckInChunkIndex, this.stuckBodyPart, this.stuckRotation);
                }
                if (flag)
                {
                    this.abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(this.thrownBy as Player, this.stuckInObject as Creature);
                }
                return true;
            }
            this.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk);
            this.vibrate = 20;
            this.ChangeMode(Weapon.Mode.Free);
            base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
            this.SetRandomSpin();
            return false;
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

            //Spear
            this.room.PlaySound(SoundID.Slugcat_Throw_Spear, base.firstChunk);
            this.alwaysStickInWalls = false;
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
