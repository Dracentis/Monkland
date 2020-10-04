using Monkland.Hooks.Entities;
using RWCustom;
using System;
using UnityEngine;

namespace Monkland.Hooks.OverWorld
{
    internal static class ShortcutHandlerHK
    {
        public static void ApplyHook()
        {
            On.ShortcutHandler.SuckInCreature += new On.ShortcutHandler.hook_SuckInCreature(SuckInCreatureHK);
        }

        private static void SuckInCreatureHK(On.ShortcutHandler.orig_SuckInCreature orig, ShortcutHandler self, Creature creature, Room room, ShortcutData shortCut)
        {
            if (creature is Player)
            {
                room.PlaySound(SoundID.Player_Enter_Shortcut, creature.mainBodyChunk.pos);
                if (shortCut.shortCutType == ShortcutData.Type.RoomExit)
                {
                    int cnt = room.abstractRoom.connections[shortCut.destNode];
                    if (cnt > -1 && !AbstractPhysicalObjectHK.GetField(creature.abstractPhysicalObject).networkObject)
                    { 
                        room.world.ActivateRoom(room.world.GetAbstractRoom(cnt)); 
                    }
                }
                if (shortCut.shortCutType == ShortcutData.Type.NPCTransportation && Array.IndexOf<IntVector2>(room.shortcutsIndex, creature.NPCTransportationDestination.Tile) > -1)
                {
                    self.transportVessels.Add(new ShortcutHandler.ShortCutVessel(creature.NPCTransportationDestination.Tile, creature, room.abstractRoom, (int)Vector2.Distance(IntVector2.ToVector2(shortCut.DestTile), IntVector2.ToVector2(creature.NPCTransportationDestination.Tile)))); 
                }
                else
                { 
                    self.transportVessels.Add(new ShortcutHandler.ShortCutVessel(shortCut.StartTile, creature, room.abstractRoom, 0)); 
                }
                return;
            }
            orig(self, creature, room, shortCut);
        }
    }
}