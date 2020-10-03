using Monkland.Hooks.Entities;
using Monkland.Hooks.Menus;
using Monkland.Hooks.OverWorld;
using Monkland.SteamManagement;
using System;
using UnityEngine;
using static RainWorld;

namespace Monkland.Hooks
{
    public static class RainWorldHK
    {
        public static void Patch()
        {
            AbstractPhysicalObjectHK.Patch();
            MainMenuHK.Patch();
            OverWorldHK.Patch();

            ProcessManagerHK.SubPatch();
            RainWorldGameHK.SubPatch();
            On.RainWorld.Start += new On.RainWorld.hook_Start(StartHK);
        }

        public static RainWorld mainRW;

        private static void StartHK(On.RainWorld.orig_Start orig, RainWorld self)
        {
            mainRW = self;
            try
            {
                if (MonklandSteamManager.instance == null) { MonklandSteamManager.CreateManager(); }
            }
            catch (Exception e) { Debug.Log(e); }

            orig.Invoke(self);
            if (MonklandSteamManager.DEBUG)
            {
                self.buildType = BuildType.Development;
                self.setup.devToolsActive = true;
            }
            else
            {
                self.buildType = BuildType.Development;
                self.setup.devToolsActive = false;
            }
        }
    }
}