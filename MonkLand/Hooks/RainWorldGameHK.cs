using Monkland.SteamManagement;
using Monkland.UI;
using System;
using UnityEngine;

namespace Monkland.Hooks
{
    internal static class RainWorldGameHK
    {
        public static bool lastMultiPauseButton;

        public static void ApplyHook()
        {
            On.RainWorldGame.ctor += new On.RainWorldGame.hook_ctor(CtorHK);
            On.RainWorldGame.Update += new On.RainWorldGame.hook_Update(UpdateHK);
            On.RainWorldGame.ShutDownProcess += new On.RainWorldGame.hook_ShutDownProcess(ShutDownProcessHK);
            On.RainWorldGame.GetNewID += new On.RainWorldGame.hook_GetNewID(GetNewIDSwap);
            On.RainWorldGame.GetNewID_1 += new On.RainWorldGame.hook_GetNewID_1(GetNewID1Swap);
        }

        public static RainWorldGame mainGame;

        private static void CtorHK(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);

            lastMultiPauseButton = false;

            if (MonklandSteamManager.isInGame)
            {
                if (self.rainWorld.buildType == RainWorld.BuildType.Development)
                { 
                    self.devToolsActive = MonklandSteamManager.DEBUG; 
                }
                MonklandSteamManager.monklandUI = new UI.MonklandUI(Futile.stage);
            }
            if (mainGame == null) 
            { 
                mainGame = self; 
            }
        }

        private static void UpdateHK(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            if (mainGame == null) 
            { 
                mainGame = self; 
            }

            if (!self.lastPauseButton)
            { 
                self.lastPauseButton = MonklandSteamManager.isInGame; 
            }

            orig(self);

            try
            {
                if (MonklandSteamManager.isInGame)
                {
                    if (MonklandSteamManager.monklandUI != null)
                    { MonklandSteamManager.monklandUI.Update(self); }
                    MonklandSteamManager.WorldManager.TickCycle();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e); 
            }

            bool flag = Input.GetKey(self.rainWorld.options.controls[0].KeyboardPause) || Input.GetKey(self.rainWorld.options.controls[0].GamePadPause) || Input.GetKey(KeyCode.Escape);
            if (flag && !lastMultiPauseButton && (self.cameras[0].hud != null || self.cameras[0].hud.map.fade < 0.1f) && !self.cameras[0].hud.textPrompt.gameOverMode)
            {
                (self.cameras[0].hud.parts.Find(x => x is MultiplayerHUD) as MultiplayerHUD).ShowMultiPauseMenu();
            }
            lastMultiPauseButton = flag;
        }

        private static void ShutDownProcessHK(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.monklandUI.ClearSprites();
                MonklandSteamManager.monklandUI = null;
                MonklandSteamManager.WorldManager.GameEnd();
                mainGame = null;
            }
            orig(self);
        }

        private static EntityID GetNewIDSwap(On.RainWorldGame.orig_GetNewID orig, RainWorldGame self)
        {
            int newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            while (newID >= -1 && newID <= 15000)
            { 
                newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue); 
            }
            return new EntityID(-1, newID);
        }

        private static EntityID GetNewID1Swap(On.RainWorldGame.orig_GetNewID_1 orig, RainWorldGame self, int spawner)
        {
            int newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            while (newID >= -1 && newID <= 15000)
            { 
                newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue); 
            }
            return new EntityID(spawner, newID);
        }
    }
}