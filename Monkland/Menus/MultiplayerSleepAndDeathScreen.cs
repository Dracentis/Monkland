﻿using Monkland;
using Monkland.SteamManagement;
using Steamworks;
using UnityEngine;
using Menu;

namespace Monkland.Menus
{
    public class MultiplayerSleepAndDeathScreen : SleepAndDeathScreen
    {
        private readonly MultiplayerPlayerList playerList;

        public MultiplayerSleepAndDeathScreen(ProcessManager manager, ProcessManager.ProcessID ID) : base(manager, ID)
        {
            this.endGameSceneCounter = -1;
            this.starvedWarningCounter = -1;
            this.exitButton.menuLabel.text = NetworkGameManager.isManager ? "SHUTDOWN" : "DISCONNECT";

            MonklandSteamManager.GameManager.FinishCycle();
            if (!MonklandSteamManager.GameManager.readiedPlayers.Contains(SteamUser.GetSteamID().m_SteamID))
            { MonklandSteamManager.GameManager.readiedPlayers.Add(SteamUser.GetSteamID().m_SteamID); }

            //Player menu
            playerList = new MultiplayerPlayerList(this, this.pages[0], new Vector2(manager.rainWorld.options.ScreenSize.x - 250f, manager.rainWorld.options.ScreenSize.y - 450f), new Vector2(200, 400), new Vector2(180, 180));
            this.pages[0].subObjects.Add(this.playerList);
        }

        public override bool ButtonsGreyedOut
        {
            get
            {
                //TODO: choose when next cycle can be started: (MonklandSteamManager.WorldManager.ingamePlayers.Count > 0 && MonklandSteamManager.WorldManager.cycleLength - MonklandSteamManager.WorldManager.timer > -2500)
                return !NetworkGameManager.isManager || gameStarting;
            }
        }

        public bool ExitButtonsGreyedOut => gameStarting;

        public bool gameStarting = false;

        public override bool FreezeMenuFunctions => (base.hud != null && base.hud.rainWorld != null && base.FreezeMenuFunctions);

        public override void Update()
        {
            if (MonklandSteamManager.isInLobby)
            {
                //MonklandSteamManager.WorldManager.TickCycle();
            }
            if (this.continueButton != null)
            {
                this.continueButton.buttonBehav.greyedOut = this.ButtonsGreyedOut;
                this.continueButton.black = Mathf.Max(0f, this.continueButton.black - 0.025f);
            }
            base.Update();
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == "EXIT")
            {
                MonklandSteamManager.instance.ExitToMultiplayerMenu();
            }
            else if (message == "READYUP")
            {
                MonklandSteamManager.GameManager.SendColor(NetworkGameManager.MessageType.BodyColorR);
                MonklandSteamManager.GameManager.SendColor(NetworkGameManager.MessageType.BodyColorG);
                MonklandSteamManager.GameManager.SendColor(NetworkGameManager.MessageType.BodyColorB);
                MonklandSteamManager.GameManager.SendColor(NetworkGameManager.MessageType.EyeColorR);
                MonklandSteamManager.GameManager.SendColor(NetworkGameManager.MessageType.EyeColorG);
                MonklandSteamManager.GameManager.SendColor(NetworkGameManager.MessageType.EyeColorB);
                MonklandSteamManager.GameManager.ToggleReady();
            }
            else if (message == "CONTINUE")
            {
                if (manager.musicPlayer != null)
                {
                    manager.musicPlayer.FadeOutAllSongs(5f);
                }
                if (NetworkGameManager.isManager)
                {
                    base.PlaySound(SoundID.MENU_Switch_Page_In);
                    gameStarting = true;
                    MonklandSteamManager.GameManager.QueueStart();
                }
            }
            else { base.Singal(sender, message); }
        }

        public new void AddPassageButton(bool buttonBlack)
        {
            // No passages in a multiplayer game
            return;
        }

        public override bool RevealMap
        {
            get
            {
                return base.hud != null && this.endGameSceneCounter < 0;
            }
        }

        public override void ShutDownProcess()
        {
            base.ShutDownProcess();
            playerList.ClearList();
        }
    }
}