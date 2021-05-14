using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monkland.SteamManagement;
using MonoMod;

namespace Monkland.Patches
{
    [MonoModPatch("global::Weapon")]
    class patch_Weapon : Weapon
    {
        [MonoModIgnore]
        public patch_Weapon(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
        }

        public bool HitThisObject(PhysicalObject obj)
        {
            if (!(obj is Player) || !(this is Spear))
            {
                return true;
            }
            else if (this.thrownBy != null && (this.thrownBy is Player) && this.room.game.IsArenaSession && !this.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers)
            {
                return false;
            }
            else if ((this.thrownBy == null || (this.thrownBy is Player)) && MonklandSteamManager.isInGame && MonklandSteamManager.lobbyInfo != null && !MonklandSteamManager.lobbyInfo.spearsHit)
            {
                return false;
            }
            return true;
        }
    }
}