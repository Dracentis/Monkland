using Monkland.SteamManagement;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Monkland.Hooks.Entities
{
    internal static class PlayerHK
    {
        public static void ApplyHook()
        {
            IDetour hkMDA = new Hook(typeof(Player).GetProperty("MapDiscoveryActive", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                typeof(PlayerHK).GetMethod("MapDiscoveryActiveHK", BindingFlags.Static | BindingFlags.Public));
            On.Player.ctor += new On.Player.hook_ctor(CtorHK);
            On.Player.ShortCutColor += new On.Player.hook_ShortCutColor(ShortCutColorHK);
            On.Player.Update += new On.Player.hook_Update(UpdateHK);
            On.Player.GrabUpdate += new On.Player.hook_GrabUpdate(GrabUpdateHK);
            On.Player.Die += new On.Player.hook_Die(DieHK);
        }

        public static void Sync(Player self, bool dead)
        {
            self.dead = dead;
            APOFields sub = AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject);
            sub.networkLife = Math.Max(100, sub.networkLife);
        }

        public static void Sync(Player self, bool corridorDrop, int corridorTurnCounter, IntVector2? corridorTurnDir, int crawlTurnDelay)
        {
            self.corridorDrop = corridorDrop;
            self.corridorTurnCounter = corridorTurnCounter;
            self.corridorTurnDir = corridorTurnDir;
            self.crawlTurnDelay = crawlTurnDelay;
            APOFields sub = AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject);
            sub.networkLife = Math.Max(100, sub.networkLife);
        }

        public static void Write(Player self, ref BinaryWriter writer)
        {
            writer.Write(self.corridorDrop);
            writer.Write(self.corridorTurnCounter);
            IntVector2NHandler.Write(self.corridorTurnDir, ref writer);
            writer.Write(self.crawlTurnDelay);
        }

        public delegate bool MapDiscoveryActive(Player self);

        public static bool MapDiscoveryActiveHK(MapDiscoveryActive orig, Player self)
        {
            if (MonklandSteamManager.isInGame)
            { return orig(self) && AbstractPhysicalObjectHK.GetField(self.abstractCreature).owner == NetworkGameManager.playerID; }
            else
            { return orig(self); }
        }

        private static void CtorHK(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkLife = 500;
        }

        private static Color ShortCutColorHK(On.Player.orig_ShortCutColor orig, Player self)
        {
            if (MonklandSteamManager.isInGame)
            { 
                return MonklandSteamManager.GameManager.playerColors[
                    MonklandSteamManager.connectedPlayers.IndexOf(AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).owner)
                    ]; 
            }
            return orig(self);
        }

        private static void DieHK(On.Player.orig_Die orig, Player self)
        {
            if (AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkObject) 
            {
                return; 
            }

            //Send packet each player that dies (not network object)
            //MonklandSteamManager.GameManager.PlayerKilled(AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).owner);
            //MonklandSteamManager.GameManager.SendViolence(self, )

            orig(self);
        }

        private static void UpdateHK(On.Player.orig_Update orig, Player self, bool eu)
        {
            APOFields sub = AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject);
            if (sub.networkObject)
            {
                if (self.lungsExhausted)
                { 
                    self.aerobicLevel = 1f; 
                }
                else
                {
                    self.aerobicLevel = Mathf.Max(1f - self.airInLungs, self.aerobicLevel - ((!self.slugcatStats.malnourished) ? 1f : 1.2f) / (((self.input[0].x != 0 || self.input[0].y != 0) ? 1100f : 400f) * (1f + 3f * Mathf.InverseLerp(0.9f, 1f, self.aerobicLevel))));
                }
                if (self.cantBeGrabbedCounter > 0)
                { 
                    self.cantBeGrabbedCounter--; 
                }
                if (self.poleSkipPenalty > 0)
                { 
                    self.poleSkipPenalty--; 
                }
                if (self.shootUpCounter > 0)
                {
                    self.noGrabCounter = Math.Max(self.noGrabCounter, 2);
                    self.shootUpCounter--;
                    if (!self.input[0].jmp || self.input[0].y < 1 || self.mainBodyChunk.pos.y < self.mainBodyChunk.lastPos.y)
                    { self.shootUpCounter = 0; }
                }

                if (self.bodyMode == Player.BodyModeIndex.ZeroG)
                {
                    self.privSneak = 0.5f;
                    self.bodyChunks[0].loudness = 0.5f * self.slugcatStats.loudnessFac;
                    self.bodyChunks[1].loudness = 0.5f * self.slugcatStats.loudnessFac;
                }
                else
                {
                    if ((!self.standing || self.bodyMode == Player.BodyModeIndex.Crawl || self.bodyMode == Player.BodyModeIndex.CorridorClimb || self.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut || (self.animation == Player.AnimationIndex.HangFromBeam && self.input[0].x == 0) || (self.animation == Player.AnimationIndex.ClimbOnBeam && self.input[0].y == 0)) && self.bodyMode != Player.BodyModeIndex.Default)
                    { self.privSneak = Mathf.Min(self.privSneak + 0.1f, 1f); }
                    else
                    { self.privSneak = Mathf.Max(self.privSneak - 0.04f, 0f); }
                    self.bodyChunks[0].loudness = 1.5f * (1f - self.Sneak) * self.slugcatStats.loudnessFac;
                    self.bodyChunks[1].loudness = 0.7f * (1f - self.Sneak) * self.slugcatStats.loudnessFac;
                }
                SoundID soundID = SoundID.None;
                if (self.Adrenaline > 0.5f)
                { soundID = SoundID.Mushroom_Trip_LOOP; }
                else if (self.Stunned)
                { soundID = SoundID.UI_Slugcat_Stunned_LOOP; }
                else if (self.corridorDrop || self.verticalCorridorSlideCounter > 0 || self.horizontalCorridorSlideCounter > 0)
                { soundID = SoundID.Slugcat_Slide_In_Narrow_Corridor_LOOP; }
                else if (self.slideCounter > 0 && self.bodyMode == Player.BodyModeIndex.Stand)
                { soundID = SoundID.Slugcat_Skid_On_Ground_LOOP; }
                else if (self.animation == Player.AnimationIndex.Roll)
                { soundID = SoundID.Slugcat_Roll_LOOP; }
                else if (self.animation == Player.AnimationIndex.ClimbOnBeam && self.input[0].y < 0)
                { soundID = SoundID.Slugcat_Slide_Down_Vertical_Beam_LOOP; }
                else if (self.animation == Player.AnimationIndex.BellySlide)
                { soundID = SoundID.Slugcat_Belly_Slide_LOOP; }
                else if (self.bodyMode == Player.BodyModeIndex.WallClimb)
                { soundID = SoundID.Slugcat_Wall_Slide_LOOP; }
                if (soundID != self.slideLoopSound)
                {
                    if (self.slideLoop != null)
                    {
                        self.slideLoop.alive = false;
                        self.slideLoop = null;
                    }
                    self.slideLoopSound = soundID;
                    if (self.slideLoopSound != SoundID.None)
                    {
                        self.slideLoop = self.room.PlaySound(self.slideLoopSound, self.mainBodyChunk, true, 1f, 1f);
                        self.slideLoop.requireActiveUpkeep = true;
                    }
                }
                if (self.slideLoop != null)
                {
                    self.slideLoop.alive = true;
                    SoundID soundID2 = self.slideLoopSound;
                    switch (soundID2)
                    {
                        case SoundID.Slugcat_Slide_Down_Vertical_Beam_LOOP:
                            self.slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(self.mainBodyChunk.vel.y) / 4.9f);
                            self.slideLoop.volume = Mathf.Min(1f, Mathf.Abs(self.mainBodyChunk.vel.y) / 2.5f);
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
                                                    self.slideLoop.pitch = 1f;
                                                    self.slideLoop.volume = Mathf.InverseLerp(0f, 0.3f, self.Adrenaline);
                                                }
                                            }
                                            else
                                            {
                                                self.slideLoop.pitch = 0.5f + Mathf.InverseLerp(11f, (float)self.lastStun, (float)self.stun);
                                                self.slideLoop.volume = Mathf.Pow(Mathf.InverseLerp(8f, 27f, (float)self.stun), 0.7f);
                                            }
                                        }
                                        else
                                        {
                                            self.slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, self.mainBodyChunk.vel.magnitude / ((!self.corridorDrop) ? 12.5f : 25f));
                                            if (self.verticalCorridorSlideCounter > 0)
                                            { self.slideLoop.volume = 1f; }
                                            else
                                            { self.slideLoop.volume = Mathf.Min(1f, self.mainBodyChunk.vel.magnitude / 4f); }
                                        }
                                    }
                                    else
                                    {
                                        self.slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(self.mainBodyChunk.pos.y - self.mainBodyChunk.lastPos.y) / 1.75f);
                                        self.slideLoop.volume = Mathf.Min(1f, Mathf.Abs(self.mainBodyChunk.vel.y) * 1.5f);
                                    }
                                }
                                else
                                {
                                    self.slideLoop.pitch = Mathf.Lerp(0.85f, 1.15f, 0.5f + Custom.DirVec(self.mainBodyChunk.pos, self.bodyChunks[1].pos).y * 0.5f);
                                    self.slideLoop.volume = 0.5f + Mathf.Abs(Custom.DirVec(self.mainBodyChunk.pos, self.bodyChunks[1].pos).x) * 0.5f;
                                }
                            }
                            else
                            {
                                self.slideLoop.pitch = Mathf.Lerp(0.5f, 1.5f, Mathf.Abs(self.mainBodyChunk.vel.x) / 25.5f);
                                self.slideLoop.volume = Mathf.Min(1f, Mathf.Abs(self.mainBodyChunk.vel.x) / 10f);
                            }
                            break;

                        case SoundID.Slugcat_Skid_On_Ground_LOOP:
                            self.slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(self.mainBodyChunk.vel.x) / 9.5f);
                            self.slideLoop.volume = Mathf.Min(1f, Mathf.Abs(self.mainBodyChunk.vel.x) / 6f);
                            break;
                    }
                }

                if (self.dontGrabStuff > 0)
                { self.dontGrabStuff--; }
                if (self.bodyMode == Player.BodyModeIndex.CorridorClimb)
                { self.timeSinceInCorridorMode = 0; }
                else
                { self.timeSinceInCorridorMode++; }

                if (self.stun == 12)
                { self.room.PlaySound(SoundID.UI_Slugcat_Exit_Stun, self.mainBodyChunk); }
                bool flag = self.input[0].jmp && !self.input[1].jmp;
                if (flag)
                {
                    if (self.grasps[0] != null && self.grasps[0].grabbed is TubeWorm)
                    { flag = (self.grasps[0].grabbed as TubeWorm).JumpButton(self); }
                    else if (self.grasps[1] != null && self.grasps[1].grabbed is TubeWorm)
                    { flag = (self.grasps[1].grabbed as TubeWorm).JumpButton(self); }
                }
                if (self.canWallJump > 0) { self.canWallJump--; }
                else if (self.canWallJump < 0) { self.canWallJump++; }
                if (self.jumpChunkCounter > 0) { self.jumpChunkCounter--; }
                else if (self.jumpChunkCounter < 0) { self.jumpChunkCounter++; }
                if (self.noGrabCounter > 0) { self.noGrabCounter--; }
                if (self.waterJumpDelay > 0) { self.waterJumpDelay--; }
                if (self.forceFeetToHorizontalBeamTile > 0) { self.forceFeetToHorizontalBeamTile--; }
                if (self.canJump > 0) { self.canJump--; }
                if (self.slowMovementStun > 0) { self.slowMovementStun--; }
                if (self.backwardsCounter > 0) { self.backwardsCounter--; }
                if (self.landingDelay > 0) { self.landingDelay--; }
                if (self.verticalCorridorSlideCounter > 0) { self.verticalCorridorSlideCounter--; }
                if (self.horizontalCorridorSlideCounter > 0) { self.horizontalCorridorSlideCounter--; }
                if (self.jumpStun > 0) { self.jumpStun--; }
                else if (self.jumpStun < 0) { self.jumpStun++; }

                if (self.input[0].downDiagonal != 0 && self.input[0].downDiagonal == self.input[1].downDiagonal)
                { self.consistentDownDiagonal++; }
                else
                { self.consistentDownDiagonal = 0; }
                if (self.dead)
                {
                    self.animation = Player.AnimationIndex.Dead;
                    self.bodyMode = Player.BodyModeIndex.Dead;
                }
                else if (self.stun > 0)
                {
                    self.animation = Player.AnimationIndex.None;
                    self.bodyMode = Player.BodyModeIndex.Stunned;
                }
                if (self.bodyMode != Player.BodyModeIndex.Swimming)
                {
                    if (self.bodyChunks[0].ContactPoint.x != 0 && self.input[0].x == self.bodyChunks[0].ContactPoint.x && self.bodyChunks[0].vel.y < 0f && self.bodyMode != Player.BodyModeIndex.CorridorClimb)
                    { self.bodyChunks[0].vel.y *= Mathf.Clamp(1f - self.surfaceFriction * ((self.bodyChunks[0].pos.y <= self.bodyChunks[1].pos.y) ? 0.5f : 2f), 0f, 1f); }
                    if (self.bodyChunks[1].ContactPoint.x != 0 && self.input[0].x == self.bodyChunks[1].ContactPoint.x && self.bodyChunks[1].vel.y < 0f && self.bodyMode != Player.BodyModeIndex.CorridorClimb)
                    { self.bodyChunks[1].vel.y *= Mathf.Clamp(1f - self.surfaceFriction * ((self.bodyChunks[0].pos.y <= self.bodyChunks[1].pos.y) ? 0.5f : 2f), 0f, 1f); }
                }

                //Created Delegate to call base Update(eu)
                RuntimeMethodHandle handle = typeof(Creature).GetMethod("Update").MethodHandle;
                RuntimeHelpers.PrepareMethod(handle);
                IntPtr ptr = handle.GetFunctionPointer();
                Action<bool> funct = (Action<bool>)Activator.CreateInstance(typeof(Action<bool>), self, ptr);
                funct(eu);//Creature.Update(eu)

                if (self.stun < 1 && !self.dead && self.enteringShortCut == null && !self.inShortcut)
                { self.MovementUpdate(eu); }

                bool flag2 = false;
                if (self.input[0].jmp && !self.input[1].jmp && !self.lastWiggleJump)
                {
                    self.wiggle += 0.025f;
                    self.lastWiggleJump = true;
                }
                IntVector2 intVector = self.wiggleDirectionCounters;
                if (self.input[0].x != 0 && self.input[0].x != self.input[1].x && self.input[0].x != self.lastWiggleDir.x)
                {
                    flag2 = true;
                    if (intVector.y > 0)
                    {
                        self.wiggle += 0.0333333351f;
                        self.wiggleDirectionCounters.y--;
                    }
                    self.lastWiggleDir.x = self.input[0].x;
                    self.lastWiggleJump = false;
                    if (self.wiggleDirectionCounters.x < 5)
                    { self.wiggleDirectionCounters.x++; }
                }
                if (self.input[0].y != 0 && self.input[0].y != self.input[1].y && self.input[0].y != self.lastWiggleDir.y)
                {
                    flag2 = true;
                    if (intVector.x > 0)
                    {
                        self.wiggle += 0.0333333351f;
                        self.wiggleDirectionCounters.x--;
                    }
                    self.lastWiggleDir.y = self.input[0].y;
                    self.lastWiggleJump = false;
                    if (self.wiggleDirectionCounters.y < 5) { self.wiggleDirectionCounters.y++; }
                }
                if (flag2) { self.noWiggleCounter = 0; }
                else { self.noWiggleCounter++; }
                self.wiggle -= Custom.LerpMap((float)self.noWiggleCounter, 5f, 35f, 0f, 0.0333333351f);
                if (self.noWiggleCounter > 20)
                {
                    if (self.wiggleDirectionCounters.x > 0) { self.wiggleDirectionCounters.x--; }
                    if (self.wiggleDirectionCounters.y > 0) { self.wiggleDirectionCounters.y++; }
                }
                self.wiggle = Mathf.Clamp(self.wiggle, 0f, 1f);

                if (sub.networkLife > 0) { sub.networkLife--; }
                else
                {
                    self.RemoveFromRoom();
                    self.Abstractize();
                    self.abstractPhysicalObject.Destroy();
                    self.Destroy();
                }
            }
            else { orig(self, eu); }

            // DEBUG VIOLENCE
            try
            {
                if (/*MonklandSteamManager.DEBUG && */Input.GetKeyDown("k"))
                {
                    self.Violence(self.mainBodyChunk, new Vector2(1, 0) * 8f, self.mainBodyChunk, null, Creature.DamageType.Bite, 1, 1);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error violence" + e);
            }
        }

        private static void GrabUpdateHK(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            APOFields sub = AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject);
            if (sub.networkObject)
            {
                if (self.spearOnBack != null) { self.spearOnBack.Update(eu); }
                bool flag = self.input[0].x == 0 && self.input[0].y == 0 && !self.input[0].jmp && !self.input[0].thrw && self.mainBodyChunk.submersion < 0.5f;
                bool flag2 = false;
                bool flag3 = false;
                if (self.input[0].pckp && !self.input[1].pckp && self.switchHandsProcess == 0f)
                {
                    bool itemSwitch = self.grasps[0] != null || self.grasps[1] != null;
                    if (self.grasps[0] != null && (self.Grabability(self.grasps[0].grabbed) == Player.ObjectGrabability.TwoHands || self.Grabability(self.grasps[0].grabbed) == Player.ObjectGrabability.Drag))
                    { itemSwitch = false; }
                    if (itemSwitch)
                    {
                        if (self.switchHandsCounter == 0)
                        { self.switchHandsCounter = 15; }
                        else
                        {
                            self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, self.mainBodyChunk);
                            self.switchHandsProcess = 0.01f;
                            self.wantToPickUp = 0;
                            self.noPickUpOnRelease = 20;
                        }
                    }
                    else
                    { self.switchHandsProcess = 0f; }
                }
                if (self.switchHandsProcess > 0f)
                {
                    float lastSwitchHandsProcess = self.switchHandsProcess;
                    self.switchHandsProcess += 0.0833333358f;
                    if (lastSwitchHandsProcess < 0.5f && self.switchHandsProcess >= 0.5f)
                    {
                        self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Complete, self.mainBodyChunk);
                        //self.SwitchGrasps(0, 1);
                    }
                    if (self.switchHandsProcess >= 1f)
                    { self.switchHandsProcess = 0f; }
                }
                int num2 = -1;
                if (flag)
                {
                    int num3 = -1;
                    int num4 = -1;
                    int num5 = 0;
                    while (num3 < 0 && num5 < 2)
                    {
                        if (self.grasps[num5] != null && self.grasps[num5].grabbed is IPlayerEdible && (self.grasps[num5].grabbed as IPlayerEdible).Edible)
                        { num3 = num5; }
                        num5++;
                    }
                    if ((num3 == -1 || (self.FoodInStomach >= self.MaxFoodInStomach && !(self.grasps[num3].grabbed is KarmaFlower) && !(self.grasps[num3].grabbed is Mushroom))) && (self.objectInStomach == null || self.CanPutSpearToBack))
                    {
                        int num6 = 0;
                        while (num4 < 0 && num2 < 0 && num6 < 2)
                        {
                            if (self.grasps[num6] != null)
                            {
                                if (self.CanPutSpearToBack && self.grasps[num6].grabbed is Spear)
                                { num2 = num6; }
                                else if (self.CanBeSwallowed(self.grasps[num6].grabbed))
                                { num4 = num6; }
                            }
                            num6++;
                        }
                    }
                    if (num3 > -1 && self.noPickUpOnRelease < 1)
                    {
                        if (!self.input[0].pckp)
                        {
                            int num7 = 1;
                            while (num7 < 10 && self.input[num7].pckp)
                            { num7++; }
                            if (num7 > 1 && num7 < 10)
                            { self.PickupPressed(); }
                        }
                    }
                    else if (self.input[0].pckp && !self.input[1].pckp)
                    { self.PickupPressed(); }
                    if (self.input[0].pckp)
                    {
                        if (num2 > -1 || self.CanRetrieveSpearFromBack)
                        { self.spearOnBack.increment = true; }
                        else if (num4 > -1 || self.objectInStomach != null)
                        { flag3 = true; }
                    }
                    if (num3 > -1 && self.wantToPickUp < 1 && (self.input[0].pckp || self.eatCounter <= 15) && self.Consious && Custom.DistLess(self.mainBodyChunk.pos, self.mainBodyChunk.lastPos, 3.6f))
                    {
                        if (self.graphicsModule != null)
                        { (self.graphicsModule as PlayerGraphics).LookAtObject(self.grasps[num3].grabbed); }
                        flag2 = true;
                        if (self.FoodInStomach < self.MaxFoodInStomach || self.grasps[num3].grabbed is KarmaFlower || self.grasps[num3].grabbed is Mushroom)
                        {
                            flag3 = false;
                            if (self.spearOnBack != null)
                            { self.spearOnBack.increment = false; }
                            if (self.eatCounter < 1)
                            {
                                self.eatCounter = 15;
                                self.BiteEdibleObject(eu);
                            }
                        }
                        else if (self.eatCounter < 20 && self.room.game.cameras[0].hud != null)
                        { self.room.game.cameras[0].hud.foodMeter.RefuseFood(); }
                    }
                }
                else if (self.input[0].pckp && !self.input[1].pckp)
                { self.PickupPressed(); }
                else
                {
                    if (self.CanPutSpearToBack)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (self.grasps[i] != null && self.grasps[i].grabbed is Spear)
                            {
                                num2 = i;
                                break;
                            }
                        }
                    }
                    if (self.input[0].pckp && (num2 > -1 || self.CanRetrieveSpearFromBack))
                    { self.spearOnBack.increment = true; }
                }
                if (self.input[0].pckp && self.grasps[0] != null && self.grasps[0].grabbed is Creature && self.CanEatMeat(self.grasps[0].grabbed as Creature) && (self.grasps[0].grabbed as Creature).Template.meatPoints > 0)
                {
                    self.eatMeat++;
                    //self.EatMeatUpdate();
                    if (self.spearOnBack != null)
                    {
                        self.spearOnBack.increment = false;
                        self.spearOnBack.interactionLocked = true;
                    }
                    if (self.eatMeat % 80 == 0 && ((self.grasps[0].grabbed as Creature).State.meatLeft <= 0 || self.FoodInStomach >= self.MaxFoodInStomach))
                    {
                        self.eatMeat = 0;
                        self.wantToPickUp = 0;
                        //self.TossObject(0, eu);
                        //self.ReleaseGrasp(0);
                        self.standing = true;
                    }
                    return;
                }
                if (!self.input[0].pckp && self.grasps[0] != null && self.eatMeat > 60)
                {
                    self.eatMeat = 0;
                    self.wantToPickUp = 0;
                    //self.TossObject(0, eu);
                    //self.ReleaseGrasp(0);
                    self.standing = true;
                    return;
                }
                self.eatMeat = Custom.IntClamp(self.eatMeat - 1, 0, 50);
                if (flag2 && self.eatCounter > 0)
                { self.eatCounter--; }
                else if (!flag2 && self.eatCounter < 40)
                { self.eatCounter++; }
                if (flag3)
                {
                    self.swallowAndRegurgitateCounter++;
                    if (self.objectInStomach != null && self.swallowAndRegurgitateCounter > 110)
                    {
                        //self.Regurgitate();
                        if (self.spearOnBack != null)
                        { self.spearOnBack.interactionLocked = true; }
                        self.swallowAndRegurgitateCounter = 0;
                    }
                    else if (self.objectInStomach == null && self.swallowAndRegurgitateCounter > 90)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (self.grasps[j] != null && self.CanBeSwallowed(self.grasps[j].grabbed))
                            {
                                self.bodyChunks[0].pos += Custom.DirVec(self.grasps[j].grabbed.firstChunk.pos, self.bodyChunks[0].pos) * 2f;
                                //self.SwallowObject(j);
                                if (self.spearOnBack != null)
                                { self.spearOnBack.interactionLocked = true; }
                                self.swallowAndRegurgitateCounter = 0;
                                (self.graphicsModule as PlayerGraphics).swallowing = 20;
                                break;
                            }
                        }
                    }
                }
                else
                { self.swallowAndRegurgitateCounter = 0; }
                for (int k = 0; k < self.grasps.Length; k++)
                {
                    if (self.grasps[k] != null && self.grasps[k].grabbed.slatedForDeletetion)
                    { self.ReleaseGrasp(k); }
                }
                if (self.grasps[0] != null && self.Grabability(self.grasps[0].grabbed) == Player.ObjectGrabability.TwoHands)
                { self.pickUpCandidate = null; }
                else
                {
                    PhysicalObject physicalObject = (self.dontGrabStuff >= 1) ? null : self.PickupCandidate(20f);
                    if (self.pickUpCandidate != physicalObject && physicalObject != null && physicalObject is PlayerCarryableItem)
                    { (physicalObject as PlayerCarryableItem).Blink(); }
                    self.pickUpCandidate = physicalObject;
                }
                if (self.switchHandsCounter > 0) { self.switchHandsCounter--; }
                if (self.wantToPickUp > 0) { self.wantToPickUp--; }
                if (self.wantToThrow > 0) { self.wantToThrow--; }
                if (self.noPickUpOnRelease > 0) { self.noPickUpOnRelease--; }
                if (self.input[0].thrw && !self.input[1].thrw)
                { self.wantToThrow = 5; }
                if (self.wantToThrow > 0)
                {
                    for (int l = 0; l < 2; l++)
                    {
                        if (self.grasps[l] != null && self.IsObjectThrowable(self.grasps[l].grabbed))
                        {
                            //self.ThrowObject(l, eu);
                            self.wantToThrow = 0;
                            break;
                        }
                    }
                }
                if (self.wantToPickUp > 0)
                {
                    bool flag5 = true;
                    if (self.animation == Player.AnimationIndex.DeepSwim)
                    {
                        if (self.grasps[0] == null && self.grasps[1] == null)
                        { flag5 = false; }
                        else
                        {
                            for (int m = 0; m < 10; m++)
                            {
                                if (self.input[m].y > -1 || self.input[m].x != 0)
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
                            if (self.input[n].y > -1)
                            {
                                flag5 = false;
                                break;
                            }
                        }
                    }
                    if (self.grasps[0] != null && self.HeavyCarry(self.grasps[0].grabbed))
                    { flag5 = true; }
                    if (flag5)
                    {
                        int num8 = -1;
                        for (int num9 = 0; num9 < 2; num9++)
                        {
                            if (self.grasps[num9] != null)
                            { num8 = num9; break; }
                        }
                        if (num8 > -1)
                        {
                            self.wantToPickUp = 0;
                            //self.ReleaseObject(num8, eu);
                        }
                        else if (self.spearOnBack != null && self.spearOnBack.spear != null && self.mainBodyChunk.ContactPoint.y < 0)
                        {
                            //self.room.socialEventRecognizer.CreaturePutItemOnGround(self.spearOnBack.spear, self);
                            //self.spearOnBack.DropSpear();
                        }
                    }
                    else if (self.pickUpCandidate != null)
                    {
                        if (self.pickUpCandidate is Spear && self.CanPutSpearToBack && ((self.grasps[0] != null && self.Grabability(self.grasps[0].grabbed) >= Player.ObjectGrabability.BigOneHand) || (self.grasps[1] != null && self.Grabability(self.grasps[1].grabbed) >= Player.ObjectGrabability.BigOneHand) || (self.grasps[0] != null && self.grasps[1] != null)))
                        {
                            Debug.Log("spear straight to back");
                            self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, self.mainBodyChunk);
                            //self.spearOnBack.SpearToBack(self.pickUpCandidate as Spear);
                        }
                        else
                        {
                            int num10 = 0;
                            for (int num11 = 0; num11 < 2; num11++)
                            { if (self.grasps[num11] == null) { num10++; } }
                            /*
                            if (self.Grabability(self.pickUpCandidate) == Player.ObjectGrabability.TwoHands && num10 < 4)
                            {
                                for (int num12 = 0; num12 < 2; num12++)
                                {
                                    if (self.grasps[num12] != null)
                                    {
                                        //self.ReleaseGrasp(num12);
                                    }
                                }
                            }
                            else if (num10 == 0)
                            {
                                for (int num13 = 0; num13 < 2; num13++)
                                {
                                    if (self.grasps[num13] != null && self.grasps[num13].grabbed is Fly)
                                    {
                                        //self.ReleaseGrasp(num13);
                                        break;
                                    }
                                }
                            }
                            */
                            for (int num14 = 0; num14 < 2; num14++)
                            {
                                if (self.grasps[num14] == null)
                                {
                                    if (self.pickUpCandidate is Creature)
                                    { self.room.PlaySound(SoundID.Slugcat_Pick_Up_Creature, self.pickUpCandidate.firstChunk, false, 1f, 1f); }
                                    else if (self.pickUpCandidate is PlayerCarryableItem)
                                    {
                                        // for (int num15 = 0; num15 < self.pickUpCandidate.grabbedBy.Count; num15++)
                                        // {
                                        //self.pickUpCandidate.grabbedBy[num15].grabber.GrabbedObjectSnatched(self.pickUpCandidate.grabbedBy[num15].grabbed, self);
                                        //self.pickUpCandidate.grabbedBy[num15].grabber.ReleaseGrasp(self.pickUpCandidate.grabbedBy[num15].graspUsed);
                                        // }
                                        //(self.pickUpCandidate as PlayerCarryableItem).PickedUp(self);
                                    }
                                    else
                                    {
                                        self.room.PlaySound(SoundID.Slugcat_Pick_Up_Misc_Inanimate, self.pickUpCandidate.firstChunk, false, 1f, 1f);
                                    }
                                    //self.SlugcatGrab(self.pickUpCandidate, num14);
                                    if (self.pickUpCandidate.graphicsModule != null && self.Grabability(self.pickUpCandidate) < (Player.ObjectGrabability)5)
                                    { self.pickUpCandidate.graphicsModule.BringSpritesToFront(); }
                                    break;
                                }
                            }
                        }
                        self.wantToPickUp = 0;
                    }
                }
            }
            else { orig(self, eu); }
        }
    }
}