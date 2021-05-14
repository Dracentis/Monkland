using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod;
using UnityEngine;
using RWCustom;

namespace Monkland.Patches
{
    [MonoModPatch("global::ShortcutHandler")]
    class patch_ShortcutHandler : ShortcutHandler
    {
        [MonoModIgnore]
        public patch_ShortcutHandler(RainWorldGame gm) : base(gm)
        {
        }

        [MonoModIgnore]
        private extern SoundID NPCShortcutSound(Creature creature, int situation);

        public void SuckInCreature(Creature creature, Room room, ShortcutData shortCut)
        {
            room.PlaySound((!(creature is Player)) ? this.NPCShortcutSound(creature, 0) : SoundID.Player_Enter_Shortcut, creature.mainBodyChunk.pos);
            if (creature is Player && shortCut.shortCutType == ShortcutData.Type.RoomExit)
            {
                int num = room.abstractRoom.connections[shortCut.destNode];
                if (num > -1 && !(creature.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
                {
                    room.world.ActivateRoom(room.world.GetAbstractRoom(num));
                }
            }
            if (shortCut.shortCutType == ShortcutData.Type.NPCTransportation && Array.IndexOf<IntVector2>(room.shortcutsIndex, creature.NPCTransportationDestination.Tile) > -1)
            {
                this.transportVessels.Add(new ShortcutHandler.ShortCutVessel(creature.NPCTransportationDestination.Tile, creature, room.abstractRoom, (int)Vector2.Distance(IntVector2.ToVector2(shortCut.DestTile), IntVector2.ToVector2(creature.NPCTransportationDestination.Tile))));
            }
            else
            {
                this.transportVessels.Add(new ShortcutHandler.ShortCutVessel(shortCut.StartTile, creature, room.abstractRoom, 0));
            }
        }
    }
}