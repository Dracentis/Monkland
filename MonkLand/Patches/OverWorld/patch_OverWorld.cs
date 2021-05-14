using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using Monkland.SteamManagement;
using UnityEngine;

namespace Monkland.Patches
{
    [MonoMod.MonoModPatch("global::OverWorld")]
    class patch_OverWorld : OverWorld
    {
        [MonoModIgnore]
        public patch_OverWorld(RainWorldGame game) : base(game)
        {
        }

        private extern void orig_LoadFirstWorld();

        private void LoadFirstWorld()
        {
            orig_LoadFirstWorld();
            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.WorldManager.GameStart();
            }
        }
    }
}