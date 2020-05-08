using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using Monkland.SteamManagement;


namespace Monkland.Patches
{
    [MonoModPatch("global::Room")]
    class patch_Room : Room
    {
        [MonoModIgnore]
        public patch_Room(RainWorldGame game, World world, AbstractRoom abstractRoom) : base(game, world, abstractRoom)
        {
        }

        public extern void orig_Update();

        public void Update()
        {
            orig_Update();
            if (MonklandSteamManager.isInGame)
            {
                if (MonklandSteamManager.WorldManager.commonRooms.ContainsKey(this.abstractRoom.name) && this.game.Players[0].realizedObject != null && this.game.Players[0].Room.name == this.abstractRoom.name)
                {
                    MonklandSteamManager.EntityManager.Send(this.game.Players[0].realizedObject, MonklandSteamManager.WorldManager.commonRooms[this.abstractRoom.name]);
                }
            }
        }
    }
}
