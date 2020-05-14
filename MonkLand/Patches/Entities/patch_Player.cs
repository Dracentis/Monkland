using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using RWCustom;
using UnityEngine;
using Monkland.SteamManagement;
using System.IO;
using System.Runtime.CompilerServices;

namespace Monkland.Patches
{
    [MonoModPatch("global::Player")]
    class patch_Player : Player
    {
        [MonoModIgnore]
        public patch_Player(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
        }

        [MonoModIgnore]
        private int lastStun;

        [MonoModIgnore]
        private int cantBeGrabbedCounter;

        [MonoModIgnore]
        private int goIntoCorridorClimb;

        [MonoModIgnore]
        private bool corridorDrop;

        [MonoModIgnore]
        private int verticalCorridorSlideCounter;

        [MonoModIgnore]
        private int horizontalCorridorSlideCounter;

        [MonoModIgnore]
        private IntVector2? corridorTurnDir;

        [MonoModIgnore]
        private int corridorTurnCounter;

        [MonoModIgnore]
        private int timeSinceInCorridorMode;

        [MonoModIgnore]
        private float[] dynamicRunSpeed;

        [MonoModIgnore]
        private float wiggle;

        [MonoModIgnore]
        private int noWiggleCounter;

        [MonoModIgnore]
        private int canCorridorJump;

        [MonoModIgnore]
        private int noGrabCounter;

        [MonoModIgnore]
        private int poleSkipPenalty;

        [MonoModIgnore]
        private int wantToPickUp;

        [MonoModIgnore]
        private int wantToThrow;

        [MonoModIgnore]
        private int dontGrabStuff;

        [MonoModIgnore]
        private int waterJumpDelay;

        [MonoModIgnore]
        private float swimForce;

        [MonoModIgnore]
        private Vector2? feetStuckPos;

        [MonoModIgnore]
        private int backwardsCounter;

        [MonoModIgnore]
        private int landingDelay;

        [MonoModIgnore]
        private int crawlTurnDelay;

        [MonoModIgnore]
        private IntVector2 lastWiggleDir;

        [MonoModIgnore]
        private IntVector2 wiggleDirectionCounters;

        [MonoModIgnore]
        private bool lastWiggleJump;

        [MonoModIgnore]
        private int ledgeGrabCounter;

        [MonoModIgnore]
        private bool straightUpOnHorizontalBeam;

        [MonoModIgnore]
        private Vector2 upOnHorizontalBeamPos;

        [MonoModIgnore]
        private int exitBellySlideCounter;

        [MonoModIgnore]
        private float privSneak;

        public int networkLife = 500;


        public override Color ShortCutColor()
        {
            if (MonklandSteamManager.isInGame)
            {
                return MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf((this.abstractPhysicalObject as patch_AbstractPhysicalObject).owner)];
            }
            return PlayerGraphics.SlugcatColor((base.State as PlayerState).slugcatCharacter);
        }

        public void Sync(bool dead)
        {
            this.dead = dead;
            networkLife = 100;
    }

        public void Sync(bool corridorDrop, int corridorTurnCounter, IntVector2? corridorTurnDir, int crawlTurnDelay)
        {
            this.corridorDrop = corridorDrop;
            this.corridorTurnCounter = corridorTurnCounter;
            this.corridorTurnDir = corridorTurnDir;
            this.crawlTurnDelay = crawlTurnDelay;
            networkLife = 100;
        }

        public void Write(ref BinaryWriter writer)
        {
            writer.Write(corridorDrop);
            writer.Write(corridorTurnCounter);
            IntVector2NHandler.Write(corridorTurnDir, ref writer);
            writer.Write(crawlTurnDelay);
        }

        public bool MapDiscoveryActive
        {
            get
            {
                if (MonklandSteamManager.isInGame)
                {
                    return base.Consious && base.abstractCreature.Room.realizedRoom != null && this.dangerGrasp == null && ((this.abstractCreature as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner == NetworkGameManager.playerID && base.mainBodyChunk != null && base.mainBodyChunk.pos.x > 0f && base.mainBodyChunk.pos.x < base.abstractCreature.Room.realizedRoom.PixelWidth && base.mainBodyChunk.pos.y > 0f && base.mainBodyChunk.pos.y < base.abstractCreature.Room.realizedRoom.PixelHeight;
                }
                else
                {
                    return base.Consious && base.abstractCreature.Room.realizedRoom != null && this.dangerGrasp == null && base.mainBodyChunk.pos.x > 0f && base.mainBodyChunk.pos.x < base.abstractCreature.Room.realizedRoom.PixelWidth && base.mainBodyChunk.pos.y > 0f && base.mainBodyChunk.pos.y < base.abstractCreature.Room.realizedRoom.PixelHeight;
                }
            }
        }

        public extern void orig_Die();

        public override void Die()
        {
            if ((this.abstractPhysicalObject as Patches.patch_AbstractPhysicalObject).networkObject)
            {
                return;
            }
            orig_Die();
        }

        public extern void orig_Update(bool eu);

        public void Update(bool eu)
        {
            if ((this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
            {
                if (this.lungsExhausted)
                {
                    this.aerobicLevel = 1f;
                }
                else
                {
                    this.aerobicLevel = Mathf.Max(1f - this.airInLungs, this.aerobicLevel - ((!this.slugcatStats.malnourished) ? 1f : 1.2f) / (((this.input[0].x != 0 || this.input[0].y != 0) ? 1100f : 400f) * (1f + 3f * Mathf.InverseLerp(0.9f, 1f, this.aerobicLevel))));
                }
                if (this.cantBeGrabbedCounter > 0)
                {
                    this.cantBeGrabbedCounter--;
                }
                if (this.poleSkipPenalty > 0)
                {
                    this.poleSkipPenalty--;
                }
                if (this.shootUpCounter > 0)
                {
                    this.noGrabCounter = Math.Max(this.noGrabCounter, 2);
                    this.shootUpCounter--;
                    if (!this.input[0].jmp || this.input[0].y < 1 || base.mainBodyChunk.pos.y < base.mainBodyChunk.lastPos.y)
                    {
                        this.shootUpCounter = 0;
                    }
                }

                if (this.bodyMode == Player.BodyModeIndex.ZeroG)
                {
                    this.privSneak = 0.5f;
                    base.bodyChunks[0].loudness = 0.5f * this.slugcatStats.loudnessFac;
                    base.bodyChunks[1].loudness = 0.5f * this.slugcatStats.loudnessFac;
                }
                else
                {
                    if ((!this.standing || this.bodyMode == Player.BodyModeIndex.Crawl || this.bodyMode == Player.BodyModeIndex.CorridorClimb || this.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut || (this.animation == Player.AnimationIndex.HangFromBeam && this.input[0].x == 0) || (this.animation == Player.AnimationIndex.ClimbOnBeam && this.input[0].y == 0)) && this.bodyMode != Player.BodyModeIndex.Default)
                    {
                        this.privSneak = Mathf.Min(this.privSneak + 0.1f, 1f);
                    }
                    else
                    {
                        this.privSneak = Mathf.Max(this.privSneak - 0.04f, 0f);
                    }
                    base.bodyChunks[0].loudness = 1.5f * (1f - this.Sneak) * this.slugcatStats.loudnessFac;
                    base.bodyChunks[1].loudness = 0.7f * (1f - this.Sneak) * this.slugcatStats.loudnessFac;
                }
                SoundID soundID = SoundID.None;
                if (this.Adrenaline > 0.5f)
                {
                    soundID = SoundID.Mushroom_Trip_LOOP;
                }
                else if (base.Stunned)
                {
                    soundID = SoundID.UI_Slugcat_Stunned_LOOP;
                }
                else if (this.corridorDrop || this.verticalCorridorSlideCounter > 0 || this.horizontalCorridorSlideCounter > 0)
                {
                    soundID = SoundID.Slugcat_Slide_In_Narrow_Corridor_LOOP;
                }
                else if (this.slideCounter > 0 && this.bodyMode == Player.BodyModeIndex.Stand)
                {
                    soundID = SoundID.Slugcat_Skid_On_Ground_LOOP;
                }
                else if (this.animation == Player.AnimationIndex.Roll)
                {
                    soundID = SoundID.Slugcat_Roll_LOOP;
                }
                else if (this.animation == Player.AnimationIndex.ClimbOnBeam && this.input[0].y < 0)
                {
                    soundID = SoundID.Slugcat_Slide_Down_Vertical_Beam_LOOP;
                }
                else if (this.animation == Player.AnimationIndex.BellySlide)
                {
                    soundID = SoundID.Slugcat_Belly_Slide_LOOP;
                }
                else if (this.bodyMode == Player.BodyModeIndex.WallClimb)
                {
                    soundID = SoundID.Slugcat_Wall_Slide_LOOP;
                }
                if (soundID != this.slideLoopSound)
                {
                    if (this.slideLoop != null)
                    {
                        this.slideLoop.alive = false;
                        this.slideLoop = null;
                    }
                    this.slideLoopSound = soundID;
                    if (this.slideLoopSound != SoundID.None)
                    {
                        this.slideLoop = this.room.PlaySound(this.slideLoopSound, base.mainBodyChunk, true, 1f, 1f);
                        this.slideLoop.requireActiveUpkeep = true;
                    }
                }
                if (this.slideLoop != null)
                {
                    this.slideLoop.alive = true;
                    SoundID soundID2 = this.slideLoopSound;
                    switch (soundID2)
                    {
                        case SoundID.Slugcat_Slide_Down_Vertical_Beam_LOOP:
                            this.slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(base.mainBodyChunk.vel.y) / 4.9f);
                            this.slideLoop.volume = Mathf.Min(1f, Mathf.Abs(base.mainBodyChunk.vel.y) / 2.5f);
                            break;
                        default:
                            if (soundID2 != SoundID.Slugcat_Belly_Slide_LOOP)
                            {
                                if (soundID2 != SoundID.Slugcat_Roll_LOOP)
                                {
                                    if (soundID2 != SoundID.Slugcat_Wall_Slide_LOOP)
                                    {
                                        if (soundID2 != SoundID.Slugcat_Slide_In_Narrow_Corridor_LOOP)
                                        {
                                            if (soundID2 != SoundID.UI_Slugcat_Stunned_LOOP)
                                            {
                                                if (soundID2 == SoundID.Mushroom_Trip_LOOP)
                                                {
                                                    this.slideLoop.pitch = 1f;
                                                    this.slideLoop.volume = Mathf.InverseLerp(0f, 0.3f, this.Adrenaline);
                                                }
                                            }
                                            else
                                            {
                                                this.slideLoop.pitch = 0.5f + Mathf.InverseLerp(11f, (float)this.lastStun, (float)base.stun);
                                                this.slideLoop.volume = Mathf.Pow(Mathf.InverseLerp(8f, 27f, (float)base.stun), 0.7f);
                                            }
                                        }
                                        else
                                        {
                                            this.slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, base.mainBodyChunk.vel.magnitude / ((!this.corridorDrop) ? 12.5f : 25f));
                                            if (this.verticalCorridorSlideCounter > 0)
                                            {
                                                this.slideLoop.volume = 1f;
                                            }
                                            else
                                            {
                                                this.slideLoop.volume = Mathf.Min(1f, base.mainBodyChunk.vel.magnitude / 4f);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        this.slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(base.mainBodyChunk.pos.y - base.mainBodyChunk.lastPos.y) / 1.75f);
                                        this.slideLoop.volume = Mathf.Min(1f, Mathf.Abs(base.mainBodyChunk.vel.y) * 1.5f);
                                    }
                                }
                                else
                                {
                                    this.slideLoop.pitch = Mathf.Lerp(0.85f, 1.15f, 0.5f + Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[1].pos).y * 0.5f);
                                    this.slideLoop.volume = 0.5f + Mathf.Abs(Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[1].pos).x) * 0.5f;
                                }
                            }
                            else
                            {
                                this.slideLoop.pitch = Mathf.Lerp(0.5f, 1.5f, Mathf.Abs(base.mainBodyChunk.vel.x) / 25.5f);
                                this.slideLoop.volume = Mathf.Min(1f, Mathf.Abs(base.mainBodyChunk.vel.x) / 10f);
                            }
                            break;
                        case SoundID.Slugcat_Skid_On_Ground_LOOP:
                            this.slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(base.mainBodyChunk.vel.x) / 9.5f);
                            this.slideLoop.volume = Mathf.Min(1f, Mathf.Abs(base.mainBodyChunk.vel.x) / 6f);
                            break;
                    }
                }

                if (this.dontGrabStuff > 0)
                {
                    this.dontGrabStuff--;
                }
                if (this.bodyMode == Player.BodyModeIndex.CorridorClimb)
                {
                    this.timeSinceInCorridorMode = 0;
                }
                else
                {
                    this.timeSinceInCorridorMode++;
                }


                if (base.stun == 12)
                {
                    this.room.PlaySound(SoundID.UI_Slugcat_Exit_Stun, base.mainBodyChunk);
                }
                bool flag = this.input[0].jmp && !this.input[1].jmp;
                if (flag)
                {
                    if (base.grasps[0] != null && base.grasps[0].grabbed is TubeWorm)
                    {
                        flag = (base.grasps[0].grabbed as TubeWorm).JumpButton(this);
                    }
                    else if (base.grasps[1] != null && base.grasps[1].grabbed is TubeWorm)
                    {
                        flag = (base.grasps[1].grabbed as TubeWorm).JumpButton(this);
                    }
                }
                if (this.canWallJump > 0)
                {
                    this.canWallJump--;
                }
                else if (this.canWallJump < 0)
                {
                    this.canWallJump++;
                }
                if (this.jumpChunkCounter > 0)
                {
                    this.jumpChunkCounter--;
                }
                else if (this.jumpChunkCounter < 0)
                {
                    this.jumpChunkCounter++;
                }
                if (this.noGrabCounter > 0)
                {
                    this.noGrabCounter--;
                }
                if (this.waterJumpDelay > 0)
                {
                    this.waterJumpDelay--;
                }
                if (this.forceFeetToHorizontalBeamTile > 0)
                {
                    this.forceFeetToHorizontalBeamTile--;
                }
                if (this.canJump > 0)
                {
                    this.canJump--;
                }
                if (this.slowMovementStun > 0)
                {
                    this.slowMovementStun--;
                }
                if (this.backwardsCounter > 0)
                {
                    this.backwardsCounter--;
                }
                if (this.landingDelay > 0)
                {
                    this.landingDelay--;
                }
                if (this.verticalCorridorSlideCounter > 0)
                {
                    this.verticalCorridorSlideCounter--;
                }
                if (this.horizontalCorridorSlideCounter > 0)
                {
                    this.horizontalCorridorSlideCounter--;
                }
                if (this.jumpStun > 0)
                {
                    this.jumpStun--;
                }
                else if (this.jumpStun < 0)
                {
                    this.jumpStun++;
                }

                if (this.input[0].downDiagonal != 0 && this.input[0].downDiagonal == this.input[1].downDiagonal)
                {
                    this.consistentDownDiagonal++;
                }
                else
                {
                    this.consistentDownDiagonal = 0;
                }
                if (base.dead)
                {
                    this.animation = Player.AnimationIndex.Dead;
                    this.bodyMode = Player.BodyModeIndex.Dead;
                }
                else if (base.stun > 0)
                {
                    this.animation = Player.AnimationIndex.None;
                    this.bodyMode = Player.BodyModeIndex.Stunned;
                }
                if (this.bodyMode != Player.BodyModeIndex.Swimming)
                {
                    if (base.bodyChunks[0].ContactPoint.x != 0 && this.input[0].x == base.bodyChunks[0].ContactPoint.x && base.bodyChunks[0].vel.y < 0f && this.bodyMode != Player.BodyModeIndex.CorridorClimb)
                    {
                        BodyChunk bodyChunk3 = base.bodyChunks[0];
                        bodyChunk3.vel.y = bodyChunk3.vel.y * Mathf.Clamp(1f - this.surfaceFriction * ((base.bodyChunks[0].pos.y <= base.bodyChunks[1].pos.y) ? 0.5f : 2f), 0f, 1f);
                    }
                    if (base.bodyChunks[1].ContactPoint.x != 0 && this.input[0].x == base.bodyChunks[1].ContactPoint.x && base.bodyChunks[1].vel.y < 0f && this.bodyMode != Player.BodyModeIndex.CorridorClimb)
                    {
                        BodyChunk bodyChunk4 = base.bodyChunks[1];
                        bodyChunk4.vel.y = bodyChunk4.vel.y * Mathf.Clamp(1f - this.surfaceFriction * ((base.bodyChunks[0].pos.y <= base.bodyChunks[1].pos.y) ? 0.5f : 2f), 0f, 1f);
                    }
                }

                //Created Delegate to call base Update(eu)
                RuntimeMethodHandle handle = typeof(Creature).GetMethod("Update").MethodHandle;
                RuntimeHelpers.PrepareMethod(handle);
                IntPtr ptr = handle.GetFunctionPointer();
                Action<bool> funct = (Action<bool>)Activator.CreateInstance(typeof(Action<bool>), this, ptr);
                funct(eu);//Creature.Update(eu)

                if (base.stun < 1 && !base.dead && this.enteringShortCut == null && !base.inShortcut)
                {
                    this.MovementUpdate(eu);
                }

                bool flag2 = false;
                if (this.input[0].jmp && !this.input[1].jmp && !this.lastWiggleJump)
                {
                    this.wiggle += 0.025f;
                    this.lastWiggleJump = true;
                }
                IntVector2 intVector = this.wiggleDirectionCounters;
                if (this.input[0].x != 0 && this.input[0].x != this.input[1].x && this.input[0].x != this.lastWiggleDir.x)
                {
                    flag2 = true;
                    if (intVector.y > 0)
                    {
                        this.wiggle += 0.0333333351f;
                        this.wiggleDirectionCounters.y = this.wiggleDirectionCounters.y - 1;
                    }
                    this.lastWiggleDir.x = this.input[0].x;
                    this.lastWiggleJump = false;
                    if (this.wiggleDirectionCounters.x < 5)
                    {
                        this.wiggleDirectionCounters.x = this.wiggleDirectionCounters.x + 1;
                    }
                }
                if (this.input[0].y != 0 && this.input[0].y != this.input[1].y && this.input[0].y != this.lastWiggleDir.y)
                {
                    flag2 = true;
                    if (intVector.x > 0)
                    {
                        this.wiggle += 0.0333333351f;
                        this.wiggleDirectionCounters.x = this.wiggleDirectionCounters.x - 1;
                    }
                    this.lastWiggleDir.y = this.input[0].y;
                    this.lastWiggleJump = false;
                    if (this.wiggleDirectionCounters.y < 5)
                    {
                        this.wiggleDirectionCounters.y = this.wiggleDirectionCounters.y + 1;
                    }
                }
                if (flag2)
                {
                    this.noWiggleCounter = 0;
                }
                else
                {
                    this.noWiggleCounter++;
                }
                this.wiggle -= Custom.LerpMap((float)this.noWiggleCounter, 5f, 35f, 0f, 0.0333333351f);
                if (this.noWiggleCounter > 20)
                {
                    if (this.wiggleDirectionCounters.x > 0)
                    {
                        this.wiggleDirectionCounters.x = this.wiggleDirectionCounters.x - 1;
                    }
                    if (this.wiggleDirectionCounters.y > 0)
                    {
                        this.wiggleDirectionCounters.y = this.wiggleDirectionCounters.y - 1;
                    }
                }
                this.wiggle = Mathf.Clamp(this.wiggle, 0f, 1f);

                if (networkLife > 0)
                {
                    networkLife--;
                }
                else
                {
                    this.RemoveFromRoom();
                    this.Abstractize();
                    this.abstractPhysicalObject.Destroy();
                    this.Destroy();
                }
            }
            else
            {
                orig_Update(eu);
            }
        }

    }
}