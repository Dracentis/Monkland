using Monkland.SteamManagement;
using System;
using UnityEngine;
using static RainWorld;

namespace Monkland.Hooks
{
    public static class RainWorldHK
    {
        public static void ApplyHook()
        {
            On.RainWorld.Start += new On.RainWorld.hook_Start(StartHK);
        }

        public static RainWorld rainWorld; // Reference to main RainWorld object

        private static void StartHK(On.RainWorld.orig_Start orig, RainWorld self)
        {
            rainWorld = self;
            if (MonklandSteamworks.instance == null)
            {
                MonklandSteamworks.CreateManagerGameObject();
            }

            orig(self);

            if (Monkland.DEVELOPMENT)
            {
                self.buildType = BuildType.Development;
                self.setup.devToolsActive = true;
            }
            else
            {
                self.buildType = BuildType.Distribution;
                self.setup.devToolsActive = false;
            }
        }
    }
}