using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using RWCustom;
using UnityEngine;
using Monkland.SteamManagement;


namespace Monkland.Patches
{
    [MonoModPatch("global::Player")]
    class patch_Player : Player
    {
        [MonoModIgnore]
        public patch_Player(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
        }

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
        }


    }
}