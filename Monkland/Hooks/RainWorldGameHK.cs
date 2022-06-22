using Monkland.SteamManagement;
using Monkland.Menus;
using System;
using UnityEngine;

namespace Monkland.Hooks
{
    internal static class RainWorldGameHK
    {
        public static void ApplyHook()
        {
            On.RainWorldGame.ctor += new On.RainWorldGame.hook_ctor(CtorHK);
            On.RainWorldGame.Update += new On.RainWorldGame.hook_Update(UpdateHK);
            On.RainWorldGame.ShutDownProcess += new On.RainWorldGame.hook_ShutDownProcess(ShutDownProcessHK);
        }

        public static RainWorldGame rainWorldGame;

        private static void CtorHK(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);


            if (MonklandSteamworks.isInLobby)
            {
                if (self.rainWorld.buildType == RainWorld.BuildType.Development)
                {
                    self.devToolsActive = Monkland.DEVELOPMENT;
                }
            }
            if (rainWorldGame == null) { rainWorldGame = self; }
        }

        private static void UpdateHK(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            if (rainWorldGame == null) { rainWorldGame = self; }

            if (!self.lastPauseButton)
            {
                // Prevent pausing during multiplayer game
                self.lastPauseButton = MonklandSteamworks.isInLobby;
            }

            orig(self);
        }

        private static void ShutDownProcessHK(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            if (MonklandSteamworks.isInLobby)
            {
                rainWorldGame = null;
            }
            orig(self);
        }
    }
}
