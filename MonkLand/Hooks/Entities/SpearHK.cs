using Monkland.SteamManagement;
using RWCustom;
using System;
using UnityEngine;

namespace Monkland.Hooks.Entities
{
    internal static class SpearHK
    {
        public static void ApplyHook()
        {
            On.Spear.ctor += new On.Spear.hook_ctor(CtorHK);
            On.Spear.Update += new On.Spear.hook_Update(UpdateHK);
            On.Spear.HitSomething += new On.Spear.hook_HitSomething(HitSomethingHK);
            On.Spear.Thrown += new On.Spear.hook_Thrown(ThrownHK);
        }

        private static bool isNet = false;

        public static void SetNet() => isNet = true;

        private static bool CheckNet()
        {
            if (isNet) { isNet = false; return true; }
            return false;
        }

        public static void Sync(Spear self) => AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkLife = 60;

        private static void CtorHK(On.Spear.orig_ctor orig, Spear self, AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            orig(self, abstractPhysicalObject, world);
            AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkLife = 60;
        }

        private static void NoChunkUpdate(Spear self, bool eu)
        {
            ((Action)Activator.CreateInstance(typeof(Action), self, typeof(Weapon).GetMethod("Update").MethodHandle.GetFunctionPointer()))();
            self.soundLoop.sound = SoundID.None;
            if (self.firstChunk.vel.magnitude > 5f)
            {
                if (self.mode == Weapon.Mode.Thrown)
                { self.soundLoop.sound = SoundID.Spear_Thrown_Through_Air_LOOP; }
                else if (self.mode == Weapon.Mode.Free)
                { self.soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP; }
                self.soundLoop.Volume = Mathf.InverseLerp(5f, 15f, self.firstChunk.vel.magnitude);
            }
            self.soundLoop.Update();
            self.lastPivotAtTip = self.pivotAtTip;
            self.pivotAtTip = (self.mode == Weapon.Mode.Thrown || self.mode == Weapon.Mode.StuckInCreature);
            if (self.addPoles && self.room.readyForAI)
            {
                if (self.abstractSpear.stuckInWallCycles >= 0)
                {
                    self.room.GetTile(self.stuckInWall.Value).horizontalBeam = true;
                    for (int i = -1; i < 2; i += 2)
                    {
                        if (!self.room.GetTile(self.stuckInWall.Value + new Vector2(20f * (float)i, 0f)).Solid)
                        { self.room.GetTile(self.stuckInWall.Value + new Vector2(20f * (float)i, 0f)).horizontalBeam = true; }
                    }
                }
                else
                {
                    self.room.GetTile(self.stuckInWall.Value).verticalBeam = true;
                    for (int j = -1; j < 2; j += 2)
                    {
                        if (!self.room.GetTile(self.stuckInWall.Value + new Vector2(0f, 20f * (float)j)).Solid)
                        { self.room.GetTile(self.stuckInWall.Value + new Vector2(0f, 20f * (float)j)).verticalBeam = true; }
                    }
                }
                self.addPoles = false;
            }
            switch (self.mode)
            {
                case Weapon.Mode.Free:
                    if (self.spinning)
                    {
                        if (Custom.DistLess(self.firstChunk.pos, self.firstChunk.lastPos, 4f * self.room.gravity))
                        { self.stillCounter++; }
                        else
                        { self.stillCounter = 0; }
                        if (self.firstChunk.ContactPoint.y < 0 || self.stillCounter > 20)
                        {
                            self.spinning = false;
                            self.rotationSpeed = 0f;
                            self.rotation = Custom.DegToVec(Mathf.Lerp(-50f, 50f, UnityEngine.Random.value) + 180f);
                            self.firstChunk.vel *= 0f;
                            self.room.PlaySound(SoundID.Spear_Stick_In_Ground, self.firstChunk);
                        }
                    }
                    else if (!Custom.DistLess(self.firstChunk.lastPos, self.firstChunk.pos, 6f))
                    { self.SetRandomSpin(); }
                    break;

                case Weapon.Mode.Thrown:
                    {
                        BodyChunk firstChunk = self.firstChunk;
                        firstChunk.vel.y += 0.45f;
                        if (Custom.DistLess(self.thrownPos, self.firstChunk.pos, 560f * Mathf.Max(1f, self.spearDamageBonus)) && self.firstChunk.ContactPoint == self.throwDir && self.room.GetTile(self.firstChunk.pos).Terrain == Room.Tile.TerrainType.Air && self.room.GetTile(self.firstChunk.pos + self.throwDir.ToVector2() * 20f).Terrain == Room.Tile.TerrainType.Solid && (UnityEngine.Random.value < ((!(self is ExplosiveSpear)) ? 0.33f : 0.8f) || Custom.DistLess(self.thrownPos, self.firstChunk.pos, 140f) || self.alwaysStickInWalls))
                        {
                            bool stuck = true;
                            foreach (AbstractWorldEntity abstractWorldEntity in self.room.abstractRoom.entities)
                            {
                                if (abstractWorldEntity is AbstractSpear && (abstractWorldEntity as AbstractSpear).realizedObject != null && ((abstractWorldEntity as AbstractSpear).realizedObject as Weapon).mode == Weapon.Mode.StuckInWall && abstractWorldEntity.pos.Tile == self.abstractPhysicalObject.pos.Tile)
                                {
                                    stuck = false;
                                    break;
                                }
                            }
                            if (stuck && !(self is ExplosiveSpear))
                            {
                                for (int k = 0; k < self.room.roomSettings.placedObjects.Count; k++)
                                {
                                    if (self.room.roomSettings.placedObjects[k].type == PlacedObject.Type.NoSpearStickZone && Custom.DistLess(self.room.MiddleOfTile(self.firstChunk.pos), self.room.roomSettings.placedObjects[k].pos, (self.room.roomSettings.placedObjects[k].data as PlacedObject.ResizableObjectData).Rad))
                                    { stuck = false; break; }
                                }
                            }
                            if (stuck)
                            {
                                self.stuckInWall = new Vector2?(self.room.MiddleOfTile(self.firstChunk.pos));
                                self.vibrate = 10;
                                self.ChangeMode(Weapon.Mode.StuckInWall);
                                self.room.PlaySound(SoundID.Spear_Stick_In_Wall, self.firstChunk);
                                self.firstChunk.collideWithTerrain = false;
                            }
                        }
                        break;
                    }
                case Weapon.Mode.StuckInCreature:
                    self.setRotation = new Vector2?(Custom.DegToVec(self.stuckRotation));
                    if (self.stuckInWall != null)
                    {
                        if (self.pinToWallCounter > 0)
                        {
                            self.pinToWallCounter--;
                        }
                        self.firstChunk.vel *= 0f;
                        self.firstChunk.pos = self.stuckInWall.Value;
                    }
                    break;

                case Weapon.Mode.StuckInWall:
                    self.firstChunk.pos = self.stuckInWall.Value;
                    self.firstChunk.vel *= 0f;
                    break;
            }
            for (int l = self.abstractPhysicalObject.stuckObjects.Count - 1; l >= 0; l--)
            {
                if (self.abstractPhysicalObject.stuckObjects[l] is AbstractPhysicalObject.ImpaledOnSpearStick)
                {
                    if (self.abstractPhysicalObject.stuckObjects[l].B.realizedObject != null && (self.abstractPhysicalObject.stuckObjects[l].B.realizedObject.slatedForDeletetion || self.abstractPhysicalObject.stuckObjects[l].B.realizedObject.grabbedBy.Count > 0))
                    { self.abstractPhysicalObject.stuckObjects[l].Deactivate(); }
                    else if (self.abstractPhysicalObject.stuckObjects[l].B.realizedObject != null && self.abstractPhysicalObject.stuckObjects[l].B.realizedObject.room == self.room)
                    {
                        self.abstractPhysicalObject.stuckObjects[l].B.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, self.firstChunk.pos + self.rotation * Custom.LerpMap((float)(self.abstractPhysicalObject.stuckObjects[l] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition, 0f, 4f, 15f, -15f));
                        self.abstractPhysicalObject.stuckObjects[l].B.realizedObject.firstChunk.vel *= 0f;
                    }
                }
            }
        }

        private static void UpdateHK(On.Spear.orig_Update orig, Spear self, bool eu)
        {
            //Sanity check kind of thing, don't know if this actually happens but there's another cause for spear crashes and I want to rule this out
            if (self.mode == Weapon.Mode.StuckInWall && !self.stuckInWall.HasValue)
            {
                Debug.Log("SPEAR STUCK IN WALL WITH NO STUCK POS!");
                self.mode = Weapon.Mode.Free;
            }
            //Alt update called if stuck in creature without a stuckobject, prevents clients crashing when someone sticks a spear in a non-synced creature
            if (self.stuckInObject == null && self.mode == Weapon.Mode.StuckInCreature)
            { NoChunkUpdate(self, eu); }
            else
            { orig(self, eu); }
            APOFields sub = AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject);
            if (sub.networkObject)
            {
                if (sub.networkLife > 0) { sub.networkLife--; }
                else
                {
                    sub.networkLife = 60;
                    for (int i = 0; i < self.grabbedBy.Count; i++)
                    {
                        if (self.grabbedBy[i] != null)
                        {
                            self.grabbedBy[i].Release();
                            i--;
                        }
                    }
                    self.Destroy();
                }
            }
        }

        private static bool HitSomethingHK(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            bool hit = orig(self, result, eu);
            if (CheckNet()) { return hit; }
            if (hit && MonklandSteamManager.isInGame && !AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkObject && MonklandSteamManager.WorldManager.commonRooms.ContainsKey(self.room.abstractRoom.name))
            {
                MonklandSteamManager.EntityManager.SendHit(self, result.obj, result.chunk);
                MonklandSteamManager.EntityManager.Send(self, MonklandSteamManager.WorldManager.commonRooms[self.room.abstractRoom.name], true);
            }
            return hit;
        }

        private static void ThrownHK(On.Spear.orig_Thrown orig, Spear self,
            Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
            if (CheckNet()) { return; }
            if (MonklandSteamManager.isInGame && !AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkObject && MonklandSteamManager.WorldManager.commonRooms.ContainsKey(self.room.abstractRoom.name))
            {
                MonklandSteamManager.EntityManager.SendThrow(self, thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc);
                MonklandSteamManager.EntityManager.Send(self, MonklandSteamManager.WorldManager.commonRooms[self.room.abstractRoom.name], true);
            }
        }
    }
}