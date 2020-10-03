using Monkland.SteamManagement;

namespace Monkland.Hooks.Entities
{
    internal static class WeaponHK
    {
        public static void SubPatch()
        {
            On.Weapon.HitThisObject += new On.Weapon.hook_HitThisObject(HitThisObjectHK);
        }

        private static bool HitThisObjectHK(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
        {
            if (!(obj is Player) || !(self is Spear))
            { return true; }
            else if (self.thrownBy != null && (self.thrownBy is Player) && self.room.game.IsArenaSession && !self.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers)
            { return false; }
            else if ((self.thrownBy == null || (self.thrownBy is Player)) && MonklandSteamManager.isInGame && MonklandSteamManager.lobbyInfo != null && !MonklandSteamManager.lobbyInfo.spearsHit)
            { return false; }
            return true;
        }
    }
}