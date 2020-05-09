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

        public override void Die()
        {
            if ((this.abstractPhysicalObject as Patches.patch_AbstractPhysicalObject).networkObject)
            {
                return;
            }
            if (this.spearOnBack != null && this.spearOnBack.spear != null)
            {
                this.spearOnBack.DropSpear();
            }
            Room room = this.room;
            if (room == null)
            {
                room = base.abstractCreature.world.GetAbstractRoom(base.abstractCreature.pos).realizedRoom;
            }
            if (room != null)
            {
                if (room.game.setupValues.invincibility)
                {
                    return;
                }
                if (!base.dead)
                {
                    room.game.GameOver(null);
                    room.PlaySound(SoundID.UI_Slugcat_Die, base.mainBodyChunk);
                }
                if (this.PlaceKarmaFlower && room.game.session is StoryGameSession)
                {
                    (room.game.session as StoryGameSession).PlaceKarmaFlowerOnDeathSpot();
                }
            }
            else if (!base.dead && !base.abstractCreature.world.game.setupValues.invincibility)
            {
                base.abstractCreature.world.game.GameOver(null);
            }
            base.Die();
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
                    this.aerobicLevel = Mathf.Max(1f - this.airInLungs, this.aerobicLevel - (1f) / ((1100f) * (1f + 3f * Mathf.InverseLerp(0.9f, 1f, this.aerobicLevel))));
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
                    if (base.mainBodyChunk.pos.y < base.mainBodyChunk.lastPos.y)
                    {
                        this.shootUpCounter = 0;
                    }
                }
                if (this.dangerGrasp == null)
                {
                    this.dangerGraspTime = 0;
                }
                else if (this.dangerGrasp.discontinued)
                {
                    this.dangerGrasp = null;
                    this.dangerGraspTime = 0;
                }
                else
                {
                    this.dangerGraspTime++;
                    if (this.dangerGraspTime == 30)
                    {
                        this.LoseAllGrasps();
                    }
                }
                if (this.dontEatExternalFoodSourceCounter > 0)
                {
                    this.dontEatExternalFoodSourceCounter--;
                }

                if (this.bodyMode == Player.BodyModeIndex.ZeroG)
                {
                    this.privSneak = 0.5f;
                    base.bodyChunks[0].loudness = 0.5f * this.slugcatStats.loudnessFac;
                    base.bodyChunks[1].loudness = 0.5f * this.slugcatStats.loudnessFac;
                }
                else
                {
                    if ((!this.standing || this.bodyMode == Player.BodyModeIndex.Crawl || this.bodyMode == Player.BodyModeIndex.CorridorClimb || this.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut || (this.animation == Player.AnimationIndex.HangFromBeam) || (this.animation == Player.AnimationIndex.ClimbOnBeam)) && this.bodyMode != Player.BodyModeIndex.Default)
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
                else if (this.animation == Player.AnimationIndex.ClimbOnBeam)
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
                if (this.allowRoll > 0)
                {
                    this.allowRoll--;
                }
                if (this.room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, -20f)).Terrain == Room.Tile.TerrainType.Air && (!base.IsTileSolid(1, -1, -1) || !base.IsTileSolid(1, 1, -1)))
                {
                    this.allowRoll = 15;
                }
                if (base.stun == 12)
                {
                    this.room.PlaySound(SoundID.UI_Slugcat_Exit_Stun, base.mainBodyChunk);
                }
                if (this.wantToJump > 0)
                {
                    this.wantToJump--;
                }
                if (this.bodyMode == Player.BodyModeIndex.WallClimb)
                {
                    this.wallSlideCounter++;
                }
                else
                {
                    this.wallSlideCounter = 0;
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

                //Created Delegate to call base Update(eu)
                RuntimeMethodHandle handle = typeof(Creature).GetMethod("Update").MethodHandle;
                RuntimeHelpers.PrepareMethod(handle);
                IntPtr ptr = handle.GetFunctionPointer();
                Action<bool> funct = (Action<bool>)Activator.CreateInstance(typeof(Action<bool>), this, ptr);
                funct(eu);//Creature.Update(eu)

                base.GoThroughFloors = false;
                if (base.stun < 1 && !base.dead && this.enteringShortCut == null && !base.inShortcut)
                {
                    this.MovementUpdate(eu);
                }
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