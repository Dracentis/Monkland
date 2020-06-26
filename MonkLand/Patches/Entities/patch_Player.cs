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
        private enum ObjectGrabability
        {
            CantGrab,
            OneHand,
            BigOneHand,
            TwoHands,
            Drag
        }

        [MonoModIgnore]
        private extern patch_Player.ObjectGrabability Grabability(PhysicalObject obj);

        [MonoModIgnore]
        private void PickupPressed()
        {
            this.wantToPickUp = 5;
            this.swallowAndRegurgitateCounter = 0;
        }

        [MonoModIgnore]
        private void BiteEdibleObject(bool eu)
        {
            for (int i = 0; i < 2; i++)
            {
                if (base.grasps[i] != null && base.grasps[i].grabbed is IPlayerEdible && (base.grasps[i].grabbed as IPlayerEdible).Edible)
                {
                    if ((base.grasps[i].grabbed as IPlayerEdible).BitesLeft == 1 && this.SessionRecord != null)
                    {
                        this.SessionRecord.AddEat(base.grasps[i].grabbed);
                    }
                    if (base.grasps[i].grabbed is Creature)
                    {
                        (base.grasps[i].grabbed as Creature).SetKillTag(base.abstractCreature);
                    }
                    if (base.graphicsModule != null)
                    {
                        (base.graphicsModule as PlayerGraphics).BiteFly(i);
                    }
                    (base.grasps[i].grabbed as IPlayerEdible).BitByPlayer(base.grasps[i], eu);
                    return;
                }
            }
        }

        [MonoModIgnore]
        private void TossObject(int grasp, bool eu)
        {
            if (base.grasps[grasp].grabbed is Creature)
            {
                this.room.PlaySound(SoundID.Slugcat_Throw_Creature, base.grasps[grasp].grabbedChunk, false, 1f, 1f);
            }
            else
            {
                this.room.PlaySound(SoundID.Slugcat_Throw_Misc_Inanimate, base.grasps[grasp].grabbedChunk, false, 1f, 1f);
            }
            PhysicalObject grabbed = base.grasps[grasp].grabbed;
            float num = 45f;
            float num2 = 4f;
            if (this.input[0].x != 0 && this.input[0].y == 0)
            {
                num = Custom.LerpMap(grabbed.TotalMass, 0.2f, 0.3f, 60f, 50f);
                num2 = Custom.LerpMap(grabbed.TotalMass, 0.2f, 0.3f, 12.5f, 8f, 2f);
            }
            else if (this.input[0].x != 0 && this.input[0].y == 1)
            {
                num = 25f;
                num2 = 9f;
            }
            else if (this.input[0].x == 0 && this.input[0].y == 1)
            {
                num = 5f;
                num2 = 8f;
            }
            if (this.Grabability(grabbed) == patch_Player.ObjectGrabability.OneHand)
            {
                num2 *= 2f;
                if (this.input[0].x != 0 && this.input[0].y == 0)
                {
                    num = 70f;
                }
            }
            if (this.animation == Player.AnimationIndex.Flip && this.input[0].y < 0 && this.input[0].x == 0)
            {
                num = 180f;
                num2 = 8f;
                for (int i = 0; i < grabbed.bodyChunks.Length; i++)
                {
                    grabbed.bodyChunks[i].goThroughFloors = true;
                }
            }
            if (grabbed is PlayerCarryableItem)
            {
                num2 *= (grabbed as PlayerCarryableItem).ThrowPowerFactor;
            }
            if (grabbed is JellyFish)
            {
                (grabbed as JellyFish).Tossed(this);
            }
            else if (grabbed is VultureGrub)
            {
                (grabbed as VultureGrub).InitiateSignalCountDown();
                num2 *= 0.5f;
            }
            else if (grabbed is Hazer)
            {
                (grabbed as Hazer).tossed = true;
                num2 = Mathf.Max(num2, 9f);
            }
            if (grabbed.TotalMass < base.TotalMass * 2f && this.ThrowDirection != 0)
            {
                float num3 = (this.ThrowDirection >= 0) ? Mathf.Max(base.bodyChunks[0].pos.x, base.bodyChunks[1].pos.x) : Mathf.Min(base.bodyChunks[0].pos.x, base.bodyChunks[1].pos.x);
                for (int j = 0; j < grabbed.bodyChunks.Length; j++)
                {
                    if (this.ThrowDirection < 0)
                    {
                        if (grabbed.bodyChunks[j].pos.x > num3 - 8f)
                        {
                            grabbed.bodyChunks[j].pos.x = num3 - 8f;
                        }
                        if (grabbed.bodyChunks[j].vel.x > 0f)
                        {
                            grabbed.bodyChunks[j].vel.x = 0f;
                        }
                    }
                    else if (this.ThrowDirection > 0)
                    {
                        if (grabbed.bodyChunks[j].pos.x < num3 + 8f)
                        {
                            grabbed.bodyChunks[j].pos.x = num3 + 8f;
                        }
                        if (grabbed.bodyChunks[j].vel.x < 0f)
                        {
                            grabbed.bodyChunks[j].vel.x = 0f;
                        }
                    }
                }
            }
            if (!this.HeavyCarry(grabbed) && grabbed.TotalMass < base.TotalMass * 0.75f)
            {
                for (int k = 0; k < grabbed.bodyChunks.Length; k++)
                {
                    grabbed.bodyChunks[k].pos.y = base.mainBodyChunk.pos.y;
                }
            }
            if (this.Grabability(grabbed) == patch_Player.ObjectGrabability.Drag)
            {
                base.grasps[grasp].grabbedChunk.vel += Custom.DegToVec(num * (float)this.ThrowDirection) * num2 / Mathf.Max(0.5f, base.grasps[grasp].grabbedChunk.mass);
            }
            else
            {
                for (int l = 0; l < grabbed.bodyChunks.Length; l++)
                {
                    if (grabbed.bodyChunks[l].vel.y < 0f)
                    {
                        grabbed.bodyChunks[l].vel.y = 0f;
                    }
                    grabbed.bodyChunks[l].vel = Vector2.Lerp(grabbed.bodyChunks[l].vel * 0.35f, base.mainBodyChunk.vel, Custom.LerpMap(grabbed.TotalMass, 0.2f, 0.5f, 0.6f, 0.3f));
                    grabbed.bodyChunks[l].vel += Custom.DegToVec(num * (float)this.ThrowDirection) * Mathf.Clamp(num2 / (Mathf.Lerp(grabbed.TotalMass, 0.4f, 0.2f) * (float)grabbed.bodyChunks.Length), 4f, 14f);
                    if (num2 > 4f && grabbed is LanternMouse)
                    {
                        (grabbed as LanternMouse).Stun(20);
                    }
                }
            }
            if (base.grasps[grasp].grabbed is Snail)
            {
                (base.grasps[grasp].grabbed as Snail).triggerTicker = 40;
            }
            this.room.socialEventRecognizer.CreaturePutItemOnGround(base.grasps[grasp].grabbed, this);
        }

        [MonoModIgnore]
        private bool CanIPickThisUp(PhysicalObject obj)
        {
            if (this.Grabability(obj) == patch_Player.ObjectGrabability.CantGrab)
            {
                return false;
            }
            if (obj is Spear)
            {
                if ((obj as Spear).mode == Weapon.Mode.OnBack)
                {
                    return false;
                }
                if (((obj as Spear).mode == Weapon.Mode.Free || (obj as Spear).mode == Weapon.Mode.StuckInCreature) && this.CanPutSpearToBack)
                {
                    return true;
                }
            }
            int num = (int)this.Grabability(obj);
            if (num == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (base.grasps[i] != null && this.Grabability(base.grasps[i].grabbed) > patch_Player.ObjectGrabability.OneHand)
                    {
                        return false;
                    }
                }
            }
            if (obj is Weapon)
            {
                if ((obj as Weapon).mode == Weapon.Mode.StuckInWall)
                {
                    return false;
                }
                if ((obj as Weapon).mode == Weapon.Mode.Thrown)
                {
                    return false;
                }
                if ((obj as Weapon).forbiddenToPlayer > 0)
                {
                    return false;
                }
            }
            int num2 = 0;
            for (int j = 0; j < 2; j++)
            {
                if (base.grasps[j] != null)
                {
                    if (base.grasps[j].grabbed == obj)
                    {
                        return false;
                    }
                    if (this.Grabability(base.grasps[j].grabbed) > patch_Player.ObjectGrabability.OneHand)
                    {
                        num2++;
                    }
                }
            }
            return num2 != 2 && (num2 <= 0 || num <= 2);
        }

        [MonoModIgnore]
        private PhysicalObject PickupCandidate(float favorSpears)
        {
            PhysicalObject result = null;
            float num = float.MaxValue;
            for (int i = 0; i < this.room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < this.room.physicalObjects[i].Count; j++)
                {
                    if ((!(this.room.physicalObjects[i][j] is PlayerCarryableItem) || (this.room.physicalObjects[i][j] as PlayerCarryableItem).forbiddenToPlayer < 1) && Custom.DistLess(base.bodyChunks[0].pos, this.room.physicalObjects[i][j].bodyChunks[0].pos, this.room.physicalObjects[i][j].bodyChunks[0].rad + 40f) && (Custom.DistLess(base.bodyChunks[0].pos, this.room.physicalObjects[i][j].bodyChunks[0].pos, this.room.physicalObjects[i][j].bodyChunks[0].rad + 20f) || this.room.VisualContact(base.bodyChunks[0].pos, this.room.physicalObjects[i][j].bodyChunks[0].pos)) && this.CanIPickThisUp(this.room.physicalObjects[i][j]))
                    {
                        float num2 = Vector2.Distance(base.bodyChunks[0].pos, this.room.physicalObjects[i][j].bodyChunks[0].pos);
                        if (this.room.physicalObjects[i][j] is Spear)
                        {
                            num2 -= favorSpears;
                        }
                        if (this.room.physicalObjects[i][j].bodyChunks[0].pos.x < base.bodyChunks[0].pos.x == this.flipDirection < 0)
                        {
                            num2 -= 10f;
                        }
                        if (num2 < num)
                        {
                            result = this.room.physicalObjects[i][j];
                            num = num2;
                        }
                    }
                }
            }
            return result;
        }

        [MonoModIgnore]
        private bool IsObjectThrowable(PhysicalObject obj)
        {
            return !(obj is VultureMask) && !(obj is TubeWorm);
        }

        [MonoModIgnore]
        private void ReleaseObject(int grasp, bool eu)
        {
            this.room.PlaySound((!(base.grasps[grasp].grabbed is Creature)) ? SoundID.Slugcat_Lay_Down_Object : SoundID.Slugcat_Lay_Down_Creature, base.grasps[grasp].grabbedChunk, false, 1f, 1f);
            this.room.socialEventRecognizer.CreaturePutItemOnGround(base.grasps[grasp].grabbed, this);
            if (base.grasps[grasp].grabbed is PlayerCarryableItem)
            {
                (base.grasps[grasp].grabbed as PlayerCarryableItem).Forbid();
            }
            this.ReleaseGrasp(grasp);
        }

        [MonoModIgnore]
        private PhysicalObject pickUpCandidate;

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

        public void setWantToPickUp(int value)
        {
            this.wantToPickUp = value;
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

        public extern void orig_GrabUpdate(bool eu);

        public void GrabUpdate(bool eu)
        {
            if ((this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
            {

                if (this.spearOnBack != null)
                {
                    this.spearOnBack.Update(eu);
                }
                bool flag = this.input[0].x == 0 && this.input[0].y == 0 && !this.input[0].jmp && !this.input[0].thrw && base.mainBodyChunk.submersion < 0.5f;
                bool flag2 = false;
                bool flag3 = false;
                if (this.input[0].pckp && !this.input[1].pckp && this.switchHandsProcess == 0f)
                {
                    bool flag4 = base.grasps[0] != null || base.grasps[1] != null;
                    if (base.grasps[0] != null && (this.Grabability(base.grasps[0].grabbed) == patch_Player.ObjectGrabability.TwoHands || this.Grabability(base.grasps[0].grabbed) == patch_Player.ObjectGrabability.Drag))
                    {
                        flag4 = false;
                    }
                    if (flag4)
                    {
                        if (this.switchHandsCounter == 0)
                        {
                            this.switchHandsCounter = 15;
                        }
                        else
                        {
                            this.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, base.mainBodyChunk);
                            this.switchHandsProcess = 0.01f;
                            this.wantToPickUp = 0;
                            this.noPickUpOnRelease = 20;
                        }
                    }
                    else
                    {
                        this.switchHandsProcess = 0f;
                    }
                }
                if (this.switchHandsProcess > 0f)
                {
                    float num = this.switchHandsProcess;
                    this.switchHandsProcess += 0.0833333358f;
                    if (num < 0.5f && this.switchHandsProcess >= 0.5f)
                    {
                        this.room.PlaySound(SoundID.Slugcat_Switch_Hands_Complete, base.mainBodyChunk);
                        //base.SwitchGrasps(0, 1);
                    }
                    if (this.switchHandsProcess >= 1f)
                    {
                        this.switchHandsProcess = 0f;
                    }
                }
                int num2 = -1;
                if (flag)
                {
                    int num3 = -1;
                    int num4 = -1;
                    int num5 = 0;
                    while (num3 < 0 && num5 < 2)
                    {
                        if (base.grasps[num5] != null && base.grasps[num5].grabbed is IPlayerEdible && (base.grasps[num5].grabbed as IPlayerEdible).Edible)
                        {
                            num3 = num5;
                        }
                        num5++;
                    }
                    if ((num3 == -1 || (this.FoodInStomach >= this.MaxFoodInStomach && !(base.grasps[num3].grabbed is KarmaFlower) && !(base.grasps[num3].grabbed is Mushroom))) && (this.objectInStomach == null || this.CanPutSpearToBack))
                    {
                        int num6 = 0;
                        while (num4 < 0 && num2 < 0 && num6 < 2)
                        {
                            if (base.grasps[num6] != null)
                            {
                                if (this.CanPutSpearToBack && base.grasps[num6].grabbed is Spear)
                                {
                                    num2 = num6;
                                }
                                else if (this.CanBeSwallowed(base.grasps[num6].grabbed))
                                {
                                    num4 = num6;
                                }
                            }
                            num6++;
                        }
                    }
                    if (num3 > -1 && this.noPickUpOnRelease < 1)
                    {
                        if (!this.input[0].pckp)
                        {
                            int num7 = 1;
                            while (num7 < 10 && this.input[num7].pckp)
                            {
                                num7++;
                            }
                            if (num7 > 1 && num7 < 10)
                            {
                                this.PickupPressed();
                            }
                        }
                    }
                    else if (this.input[0].pckp && !this.input[1].pckp)
                    {
                        this.PickupPressed();
                    }
                    if (this.input[0].pckp)
                    {
                        if (num2 > -1 || this.CanRetrieveSpearFromBack)
                        {
                            this.spearOnBack.increment = true;
                        }
                        else if (num4 > -1 || this.objectInStomach != null)
                        {
                            flag3 = true;
                        }
                    }
                    if (num3 > -1 && this.wantToPickUp < 1 && (this.input[0].pckp || this.eatCounter <= 15) && base.Consious && Custom.DistLess(base.mainBodyChunk.pos, base.mainBodyChunk.lastPos, 3.6f))
                    {
                        if (base.graphicsModule != null)
                        {
                            (base.graphicsModule as PlayerGraphics).LookAtObject(base.grasps[num3].grabbed);
                        }
                        flag2 = true;
                        if (this.FoodInStomach < this.MaxFoodInStomach || base.grasps[num3].grabbed is KarmaFlower || base.grasps[num3].grabbed is Mushroom)
                        {
                            flag3 = false;
                            if (this.spearOnBack != null)
                            {
                                this.spearOnBack.increment = false;
                            }
                            if (this.eatCounter < 1)
                            {
                                this.eatCounter = 15;
                                this.BiteEdibleObject(eu);
                            }
                        }
                        else if (this.eatCounter < 20 && this.room.game.cameras[0].hud != null)
                        {
                            this.room.game.cameras[0].hud.foodMeter.RefuseFood();
                        }
                    }
                }
                else if (this.input[0].pckp && !this.input[1].pckp)
                {
                    this.PickupPressed();
                }
                else
                {
                    if (this.CanPutSpearToBack)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (base.grasps[i] != null && base.grasps[i].grabbed is Spear)
                            {
                                num2 = i;
                                break;
                            }
                        }
                    }
                    if (this.input[0].pckp && (num2 > -1 || this.CanRetrieveSpearFromBack))
                    {
                        this.spearOnBack.increment = true;
                    }
                }
                if (this.input[0].pckp && base.grasps[0] != null && base.grasps[0].grabbed is Creature && this.CanEatMeat(base.grasps[0].grabbed as Creature) && (base.grasps[0].grabbed as Creature).Template.meatPoints > 0)
                {
                    this.eatMeat++;
                    //this.EatMeatUpdate();
                    if (this.spearOnBack != null)
                    {
                        this.spearOnBack.increment = false;
                        this.spearOnBack.interactionLocked = true;
                    }
                    if (this.eatMeat % 80 == 0 && ((base.grasps[0].grabbed as Creature).State.meatLeft <= 0 || this.FoodInStomach >= this.MaxFoodInStomach))
                    {
                        this.eatMeat = 0;
                        this.wantToPickUp = 0;
                        //this.TossObject(0, eu);
                        //this.ReleaseGrasp(0);
                        this.standing = true;
                    }
                    return;
                }
                if (!this.input[0].pckp && base.grasps[0] != null && this.eatMeat > 60)
                {
                    this.eatMeat = 0;
                    this.wantToPickUp = 0;
                    //this.TossObject(0, eu);
                    //this.ReleaseGrasp(0);
                    this.standing = true;
                    return;
                }
                this.eatMeat = Custom.IntClamp(this.eatMeat - 1, 0, 50);
                if (flag2 && this.eatCounter > 0)
                {
                    this.eatCounter--;
                }
                else if (!flag2 && this.eatCounter < 40)
                {
                    this.eatCounter++;
                }
                if (flag3)
                {
                    this.swallowAndRegurgitateCounter++;
                    if (this.objectInStomach != null && this.swallowAndRegurgitateCounter > 110)
                    {
                        //this.Regurgitate();
                        if (this.spearOnBack != null)
                        {
                            this.spearOnBack.interactionLocked = true;
                        }
                        this.swallowAndRegurgitateCounter = 0;
                    }
                    else if (this.objectInStomach == null && this.swallowAndRegurgitateCounter > 90)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (base.grasps[j] != null && this.CanBeSwallowed(base.grasps[j].grabbed))
                            {
                                base.bodyChunks[0].pos += Custom.DirVec(base.grasps[j].grabbed.firstChunk.pos, base.bodyChunks[0].pos) * 2f;
                                //this.SwallowObject(j);
                                if (this.spearOnBack != null)
                                {
                                    this.spearOnBack.interactionLocked = true;
                                }
                                this.swallowAndRegurgitateCounter = 0;
                                (base.graphicsModule as PlayerGraphics).swallowing = 20;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    this.swallowAndRegurgitateCounter = 0;
                }
                for (int k = 0; k < base.grasps.Length; k++)
                {
                    if (base.grasps[k] != null && base.grasps[k].grabbed.slatedForDeletetion)
                    {
                        this.ReleaseGrasp(k);
                    }
                }
                if (base.grasps[0] != null && this.Grabability(base.grasps[0].grabbed) == patch_Player.ObjectGrabability.TwoHands)
                {
                    this.pickUpCandidate = null;
                }
                else
                {
                    PhysicalObject physicalObject = (this.dontGrabStuff >= 1) ? null : this.PickupCandidate(20f);
                    if (this.pickUpCandidate != physicalObject && physicalObject != null && physicalObject is PlayerCarryableItem)
                    {
                        (physicalObject as PlayerCarryableItem).Blink();
                    }
                    this.pickUpCandidate = physicalObject;
                }
                if (this.switchHandsCounter > 0)
                {
                    this.switchHandsCounter--;
                }
                if (this.wantToPickUp > 0)
                {
                    this.wantToPickUp--;
                }
                if (this.wantToThrow > 0)
                {
                    this.wantToThrow--;
                }
                if (this.noPickUpOnRelease > 0)
                {
                    this.noPickUpOnRelease--;
                }
                if (this.input[0].thrw && !this.input[1].thrw)
                {
                    this.wantToThrow = 5;
                }
                if (this.wantToThrow > 0)
                {
                    for (int l = 0; l < 2; l++)
                    {
                        if (base.grasps[l] != null && this.IsObjectThrowable(base.grasps[l].grabbed))
                        {
                            //this.ThrowObject(l, eu);
                            this.wantToThrow = 0;
                            break;
                        }
                    }
                }
                if (this.wantToPickUp > 0)
                {
                    bool flag5 = true;
                    if (this.animation == Player.AnimationIndex.DeepSwim)
                    {
                        if (base.grasps[0] == null && base.grasps[1] == null)
                        {
                            flag5 = false;
                        }
                        else
                        {
                            for (int m = 0; m < 10; m++)
                            {
                                if (this.input[m].y > -1 || this.input[m].x != 0)
                                {
                                    flag5 = false;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int n = 0; n < 5; n++)
                        {
                            if (this.input[n].y > -1)
                            {
                                flag5 = false;
                                break;
                            }
                        }
                    }
                    if (base.grasps[0] != null && this.HeavyCarry(base.grasps[0].grabbed))
                    {
                        flag5 = true;
                    }
                    if (flag5)
                    {
                        int num8 = -1;
                        for (int num9 = 0; num9 < 2; num9++)
                        {
                            if (base.grasps[num9] != null)
                            {
                                num8 = num9;
                                break;
                            }
                        }
                        if (num8 > -1)
                        {
                            this.wantToPickUp = 0;
                            //this.ReleaseObject(num8, eu);
                        }
                        else if (this.spearOnBack != null && this.spearOnBack.spear != null && base.mainBodyChunk.ContactPoint.y < 0)
                        {
                            //this.room.socialEventRecognizer.CreaturePutItemOnGround(this.spearOnBack.spear, this);
                            //this.spearOnBack.DropSpear();
                        }
                    }
                    else if (this.pickUpCandidate != null)
                    {
                        if (this.pickUpCandidate is Spear && this.CanPutSpearToBack && ((base.grasps[0] != null && this.Grabability(base.grasps[0].grabbed) >= patch_Player.ObjectGrabability.BigOneHand) || (base.grasps[1] != null && this.Grabability(base.grasps[1].grabbed) >= patch_Player.ObjectGrabability.BigOneHand) || (base.grasps[0] != null && base.grasps[1] != null)))
                        {
                            Debug.Log("spear straight to back");
                            this.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, base.mainBodyChunk);
                            //this.spearOnBack.SpearToBack(this.pickUpCandidate as Spear);
                        }
                        else
                        {
                            int num10 = 0;
                            for (int num11 = 0; num11 < 2; num11++)
                            {
                                if (base.grasps[num11] == null)
                                {
                                    num10++;
                                }
                            }
                            if (this.Grabability(this.pickUpCandidate) == patch_Player.ObjectGrabability.TwoHands && num10 < 4)
                            {
                                for (int num12 = 0; num12 < 2; num12++)
                                {
                                    if (base.grasps[num12] != null)
                                    {
                                        //this.ReleaseGrasp(num12);
                                    }
                                }
                            }
                            else if (num10 == 0)
                            {
                                for (int num13 = 0; num13 < 2; num13++)
                                {
                                    if (base.grasps[num13] != null && base.grasps[num13].grabbed is Fly)
                                    {
                                        //this.ReleaseGrasp(num13);
                                        break;
                                    }
                                }
                            }
                            for (int num14 = 0; num14 < 2; num14++)
                            {
                                if (base.grasps[num14] == null)
                                {
                                    if (this.pickUpCandidate is Creature)
                                    {
                                        this.room.PlaySound(SoundID.Slugcat_Pick_Up_Creature, this.pickUpCandidate.firstChunk, false, 1f, 1f);
                                    }
                                    else if (this.pickUpCandidate is PlayerCarryableItem)
                                    {
                                        for (int num15 = 0; num15 < this.pickUpCandidate.grabbedBy.Count; num15++)
                                        {
                                            //this.pickUpCandidate.grabbedBy[num15].grabber.GrabbedObjectSnatched(this.pickUpCandidate.grabbedBy[num15].grabbed, this);
                                            //this.pickUpCandidate.grabbedBy[num15].grabber.ReleaseGrasp(this.pickUpCandidate.grabbedBy[num15].graspUsed);
                                        }
                                        //(this.pickUpCandidate as PlayerCarryableItem).PickedUp(this);
                                    }
                                    else
                                    {
                                        this.room.PlaySound(SoundID.Slugcat_Pick_Up_Misc_Inanimate, this.pickUpCandidate.firstChunk, false, 1f, 1f);
                                    }
                                    //this.SlugcatGrab(this.pickUpCandidate, num14);
                                    if (this.pickUpCandidate.graphicsModule != null && this.Grabability(this.pickUpCandidate) < (patch_Player.ObjectGrabability)5)
                                    {
                                        this.pickUpCandidate.graphicsModule.BringSpritesToFront();
                                    }
                                    break;
                                }
                            }
                        }
                        this.wantToPickUp = 0;
                    }
                }
            }
            else
            {
                orig_GrabUpdate(eu);
            }
        }

        public void SlugcatGrab(PhysicalObject obj, int graspUsed)
        {
            if (obj is IPlayerEdible)
            {
                if (!(this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
                    this.Grab(obj, graspUsed, 0, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, false, true);
            }
            int chunkGrabbed = 0;
            if (this.Grabability(obj) == patch_Player.ObjectGrabability.Drag)
            {
                float dst = float.MaxValue;
                for (int i = 0; i < obj.bodyChunks.Length; i++)
                {
                    if (Custom.DistLess(base.mainBodyChunk.pos, obj.bodyChunks[i].pos, dst))
                    {
                        dst = Vector2.Distance(base.mainBodyChunk.pos, obj.bodyChunks[i].pos);
                        chunkGrabbed = i;
                    }
                }
            }
            this.switchHandsCounter = 0;
            this.wantToPickUp = 0;
            this.noPickUpOnRelease = 20;
            if (!(this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
                this.Grab(obj, graspUsed, chunkGrabbed, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, true, !(obj is Cicada) && !(obj is JetFish));
        }
    }
}