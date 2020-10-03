using Menu;
using Monkland.Hooks.Entities;
using Monkland.Hooks.OverWorld;
using Monkland.SteamManagement;
using System;
using UnityEngine;

namespace Monkland.Hooks
{
    internal static class ProcessManagerHK
    {
        public static void SubPatch()
        {
            On.ProcessManager.SwitchMainProcess += new On.ProcessManager.hook_SwitchMainProcess(SwitchMainProcessHK);
        }

        private static void InGameClear()
        {
            AbstractPhysicalObjectHK.ClearSub();
            AbstractRoomHK.ClearSub();
        }

        public static void ImmediateSwitchCustom(ProcessManager self, MainLoopProcess newProcess)
        {
            MainLoopProcess mainLoopProcess = self.currentMainLoop;
            if (self.currentMainLoop != null)
            {
                self.currentMainLoop.ShutDownProcess();
                self.currentMainLoop.processActive = false;
                self.currentMainLoop = null;
                self.soundLoader.ReleaseAllUnityAudio();
                HeavyTexturesCache.ClearRegisteredFutileAtlases();
                GC.Collect();
                Resources.UnloadUnusedAssets();
            }
            self.rainWorld.progression.Revert();
            self.currentMainLoop = newProcess;
            if (mainLoopProcess != null)
            {
                mainLoopProcess.CommunicateWithUpcomingProcess(self.currentMainLoop);
            }
            self.blackFadeTime = self.currentMainLoop.FadeInTime;
            self.blackDelay = self.currentMainLoop.InitialBlackSeconds;
            if (self.fadeSprite != null)
            {
                self.fadeSprite.RemoveFromContainer();
                Futile.stage.AddChild(self.fadeSprite);
            }
            if (self.loadingLabel != null)
            {
                self.loadingLabel.RemoveFromContainer();
                Futile.stage.AddChild(self.loadingLabel);
            }
            if (self.musicPlayer != null)
            {
                self.musicPlayer.UpdateMusicContext(self.currentMainLoop);
            }
            self.pauseFadeUpdate = true;
        }

        private static void SwitchMainProcessHK(On.ProcessManager.orig_SwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            if (self.currentMainLoop?.ID == ProcessManager.ProcessID.Game && ID != ProcessManager.ProcessID.Game) { InGameClear(); }
            if (!MonklandSteamManager.isInGame) { orig.Invoke(self, ID); return; }

            ProcessManager.ProcessID newID = ID;

            switch (ID)
            {
                case ProcessManager.ProcessID.SleepScreen:
                case ProcessManager.ProcessID.DeathScreen:
                case ProcessManager.ProcessID.StarveScreen:
                    break;

                case ProcessManager.ProcessID.GhostScreen:
                case ProcessManager.ProcessID.KarmaToMaxScreen:
                case ProcessManager.ProcessID.SlideShow:
                case ProcessManager.ProcessID.FastTravelScreen:
                case ProcessManager.ProcessID.RegionsOverviewScreen:
                case ProcessManager.ProcessID.Credits:
                case ProcessManager.ProcessID.Statistics:
                    newID = ProcessManager.ProcessID.SleepScreen;
                    break;

                default: orig.Invoke(self, ID); return;
            }

            MainLoopProcess mainLoopProcess = self.currentMainLoop;
            self.shadersTime = 0f;
            if (ID == ProcessManager.ProcessID.Game && self.menuMic != null)
            {
                self.menuMic = null;
                self.sideProcesses.Remove(self.menuMic);
            }
            else if (ID != ProcessManager.ProcessID.Game && self.menuMic == null)
            {
                self.menuMic = new MenuMicrophone(self, self.soundLoader);
                self.sideProcesses.Add(self.menuMic);
            }
            if (self.currentMainLoop != null)
            {
                self.currentMainLoop.ShutDownProcess();
                self.currentMainLoop.processActive = false;
                self.currentMainLoop = null;
                self.soundLoader.ReleaseAllUnityAudio();
                HeavyTexturesCache.ClearRegisteredFutileAtlases();
                GC.Collect();
                Resources.UnloadUnusedAssets();
            }
            if (ID != ProcessManager.ProcessID.SleepScreen && ID != ProcessManager.ProcessID.GhostScreen && ID != ProcessManager.ProcessID.DeathScreen && ID != ProcessManager.ProcessID.KarmaToMaxScreen)
            { self.rainWorld.progression.Revert(); }

            self.currentMainLoop = new MultiplayerSleepAndDeathScreen(self, newID);

            if (mainLoopProcess != null)
            { mainLoopProcess.CommunicateWithUpcomingProcess(self.currentMainLoop); }
            self.blackFadeTime = self.currentMainLoop.FadeInTime;
            self.blackDelay = self.currentMainLoop.InitialBlackSeconds;
            if (self.fadeSprite != null)
            {
                self.fadeSprite.RemoveFromContainer();
                Futile.stage.AddChild(self.fadeSprite);
            }
            if (self.loadingLabel != null)
            {
                self.loadingLabel.RemoveFromContainer();
                Futile.stage.AddChild(self.loadingLabel);
            }
            if (self.musicPlayer != null)
            { self.musicPlayer.UpdateMusicContext(self.currentMainLoop); }
            self.pauseFadeUpdate = true;
        }
    }
}