using Monkland.Hooks.OverWorld;
using Monkland.SteamManagement;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Monkland.Hooks.Entities
{
    internal static class RoomHK
    {
        public static void ApplyHook()
        {
            On.Room.Update += new On.Room.hook_Update(UpdateHK);
            On.RoomSpecificScript.AddRoomSpecificScript += new On.RoomSpecificScript.hook_AddRoomSpecificScript(AddRoomSpecificScriptHK);
        }

        /// <summary>
        /// Entity Live Updates HERE
        /// </summary>
        private static void UpdateHK(On.Room.orig_Update orig, Room self)
        {
            orig(self);
            if (MonklandSteamManager.isInGame)
            {
                /*
                if (MonklandSteamManager.gameMode == MonklandSteamManager.GameMode.BattleRoyale && MonklandSteamManager.WorldManager.sessionTotalCycles >= 0)
                {

                    if (AbstractRoomHK.GetField(self.abstractRoom).battleRain == null)
                    {
                        AbstractRoomHK.GetField(self.abstractRoom).battleRain = new BattleRoomRain(self);
                        self.AddObject(AbstractRoomHK.GetField(self.abstractRoom).battleRain);
                    }

                }
                */

                if (MonklandSteamManager.WorldManager.commonRooms.ContainsKey(self.abstractRoom.name) && self.game.Players[0].realizedObject != null && self.game.Players[0].Room.name == self.abstractRoom.name)
                { 
                    AbsRoomFields field = AbstractRoomHK.GetField(self.abstractRoom);
                    if (self.game.Players[0].realizedCreature != null)
                    {
                        MonklandSteamManager.EntityManager.SendCreature(self.game.Players[0].realizedCreature, MonklandSteamManager.WorldManager.commonRooms[self.abstractRoom.name]);
                    }
                    for (int i = 0; i < self.physicalObjects.Length; i++)
                    {
                        for (int j = 0; j < self.physicalObjects[i].Count; j++)
                        {
                            if (self.physicalObjects[i][j] != null && self.physicalObjects[i][j].abstractPhysicalObject != null && AbstractPhysicalObjectHK.GetField(self.physicalObjects[i][j].abstractPhysicalObject).owner == NetworkGameManager.playerID)
                            {
                                if (MonklandSteamManager.EntityManager.isSynced(self.physicalObjects[i][j]))
                                {
                                    if (field.syncDelay == 0)
                                    {
                                        MonklandSteamManager.EntityManager.SendPhysicalObject(self.physicalObjects[i][j], MonklandSteamManager.WorldManager.commonRooms[self.abstractRoom.name], true);
                                    }
                                }
                                /*
                                else if (self.physicalObjects[i][j] is Spear)
                                {
                                    if (field.syncDelay == 0)
                                    {
                                        MonklandSteamManager.EntityManager.SendPhysicalObject(self.physicalObjects[i][j] as Spear, MonklandSteamManager.WorldManager.commonRooms[self.abstractRoom.name], true);
                                    }
                                }*/
                            }
                        }
                    }
                    if (field.syncDelay <= 0)
                    {
                        field.syncDelay = 20;
                    }
                    else
                    {
                        field.syncDelay--;
                    }
                }
            }
        }

        /// <summary>
        /// Entity Room Start Updates HERE
        /// </summary>
        public static void MultiplayerNewToRoom(Room self, List<ulong> players)
        {
            if (MonklandSteamManager.isInGame && self.abstractRoom != null && self.physicalObjects != null)
            {
                if (self.game.Players[0].realizedCreature != null)
                {
                    MonklandSteamManager.EntityManager.SendCreature(self.game.Players[0].realizedCreature, players, true);
                }
                for (int i = 0; i < self.physicalObjects.Length; i++)
                {
                    for (int j = 0; j < self.physicalObjects[i].Count; j++)
                    {
                        if (self.physicalObjects[i][j] != null && self.physicalObjects[i][j].abstractPhysicalObject != null && (AbstractPhysicalObjectHK.GetField(self.physicalObjects[i][j].abstractPhysicalObject).owner == NetworkGameManager.playerID))
                        {
                            if (MonklandSteamManager.EntityManager.isSynced(self.physicalObjects[i][j]))
                            {
                                MonklandSteamManager.EntityManager.SendPhysicalObject(self.physicalObjects[i][j], players, true);
                            }
                            /*
                            else if (self.physicalObjects[i][j] is Spear)
                            {
                                MonklandSteamManager.EntityManager.Send(self.physicalObjects[i][j] as Spear, players, true);
                            }
                            */
                        }
                    }
                }
                for (int i = 0; i < self.physicalObjects.Length; i++)
                {
                    for (int j = 0; j < self.physicalObjects[i].Count; j++)
                    {
                        if (self.physicalObjects[i][j] != null && self.physicalObjects[i][j].abstractPhysicalObject != null && (AbstractPhysicalObjectHK.GetField(self.physicalObjects[i][j].abstractPhysicalObject).owner == NetworkGameManager.playerID))
                        {
                            if (self.physicalObjects[i][j] is Creature && (self.physicalObjects[i][j] as Creature).grasps != null && (self.physicalObjects[i][j] as Creature).grasps.Length > 0)
                            {
                                foreach (Creature.Grasp grasp in (self.physicalObjects[i][j] as Creature).grasps)
                                { MonklandSteamManager.EntityManager.SendGrab(grasp); }
                            }
                            foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.physicalObjects[i][j].abstractPhysicalObject.stuckObjects)
                            {
                                if (AbstractPhysicalObjectHK.GetField(stick.A).dist == AbstractPhysicalObjectHK.GetField(self.physicalObjects[i][j].abstractPhysicalObject).dist)
                                {
                                    if (stick is AbstractPhysicalObject.AbstractSpearStick)
                                    {
                                        MonklandSteamManager.EntityManager.SendSpearStick(stick.A, stick.B, stick.A.Room, (stick as AbstractPhysicalObject.AbstractSpearStick).chunk, (stick as AbstractPhysicalObject.AbstractSpearStick).bodyPart, (stick as AbstractPhysicalObject.AbstractSpearStick).angle);
                                    }
                                    else if (stick is AbstractPhysicalObject.AbstractSpearAppendageStick)
                                    {
                                        MonklandSteamManager.EntityManager.SendSpearAppendageStick(stick.A, stick.B, stick.A.Room, (stick as AbstractPhysicalObject.AbstractSpearAppendageStick).appendage, (stick as AbstractPhysicalObject.AbstractSpearAppendageStick).prevSeg, (stick as AbstractPhysicalObject.AbstractSpearAppendageStick).distanceToNext, (stick as AbstractPhysicalObject.AbstractSpearAppendageStick).angle);
                                    }
                                    else if (stick is AbstractPhysicalObject.ImpaledOnSpearStick)
                                    {
                                        MonklandSteamManager.EntityManager.SendSpearImpaledStick(stick.A, stick.B, stick.A.Room, (stick as AbstractPhysicalObject.ImpaledOnSpearStick).chunk, (stick as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fixes the Gravity Room Problem
        /// </summary>
        private static void AddRoomSpecificScriptHK(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
        {
            if (room.abstractRoom.name == "SS_E08") { room.AddObject(new SS_E08GradientGravity(room)); }
            else { orig(room); }
        }
    }
}
