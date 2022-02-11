using Monkland.SteamManagement;
using Monkland.Menus;
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
        }

        public static RainWorldGame rainWorldGame;

        private static void CtorHK(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);


            if (MonklandSteamManager.isInLobby)
            {
                lastMultiPauseButton = false;
                if (self.rainWorld.buildType == RainWorld.BuildType.Development)
                {
                    self.devToolsActive = Monkland.DEVELOPMENT;
                }
                //MonklandSteamManager.monklandUI = new UI.MonklandUI(Futile.stage);
            }
            if (rainWorldGame == null) { rainWorldGame = self; }
        }

        private static void UpdateHK(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            if (rainWorldGame == null) { rainWorldGame = self; }

            if (!self.lastPauseButton)
            {
                // Prevent pausing during multiplayer game
                self.lastPauseButton = MonklandSteamManager.isInLobby;
            }

            orig(self);

            /*
            try
            {
                if (MonklandSteamManager.isInLobby)
                {
                    if (MonklandSteamManager.monklandUI != null)
                    { MonklandSteamManager.monklandUI.Update(self); }
                    MonklandSteamManager.WorldManager.TickCycle();

                    bool flag = Input.GetKey(self.rainWorld.options.controls[0].KeyboardPause) || Input.GetKey(self.rainWorld.options.controls[0].GamePadPause) || Input.GetKey(KeyCode.Escape);
                    if (flag && !lastMultiPauseButton && (self.cameras[0].hud != null || self.cameras[0].hud.map.fade < 0.1f) && !self.cameras[0].hud.textPrompt.gameOverMode)
                    {
                        (self.cameras[0].hud.parts.Find(x => x is MultiplayerHUD) as MultiplayerHUD).ShowMultiPauseMenu();
                    }

                    lastMultiPauseButton = flag;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            */
        }

        private static void ShutDownProcessHK(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            if (MonklandSteamManager.isInLobby)
            {
                /*
                MonklandSteamManager.monklandUI.ClearSprites();
                MonklandSteamManager.monklandUI = null;
                MonklandSteamManager.WorldManager.GameEnd();
                */
                rainWorldGame = null;
            }
            orig(self);
        }
    }
}
