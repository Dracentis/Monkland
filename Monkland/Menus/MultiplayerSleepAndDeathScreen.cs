using Monkland;
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
            this.exitButton.menuLabel.text = MonklandSteamworks.isManager ? "SHUTDOWN" : "DISCONNECT";

            MonklandSteamworks.gameManager.FinishCycle();
            if (!MonklandSteamworks.gameManager.readiedPlayers.Contains(SteamUser.GetSteamID().m_SteamID))
            { MonklandSteamworks.gameManager.readiedPlayers.Add(SteamUser.GetSteamID().m_SteamID); }

            //Player menu
            playerList = new MultiplayerPlayerList(this, this.pages[0], new Vector2(manager.rainWorld.options.ScreenSize.x - 250f, manager.rainWorld.options.ScreenSize.y - 450f), new Vector2(200, 400), new Vector2(180, 180));
            this.pages[0].subObjects.Add(this.playerList);
        }

        public override bool ButtonsGreyedOut
        {
            get
            {
                return !MonklandSteamworks.isManager || gameStarting;
            }
        }

        public bool ExitButtonsGreyedOut => gameStarting;

        public bool gameStarting = false;

        public override bool FreezeMenuFunctions => (base.hud != null && base.hud.rainWorld != null && base.FreezeMenuFunctions);

        public override void Update()
        {
            if (MonklandSteamworks.isInLobby)
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
                MonklandSteamworks.instance.ExitToMultiplayerMenu();
            }
            else if (message == "READYUP")
            {
                MonklandSteamworks.gameManager.ToggleReady();
            }
            else if (message == "CONTINUE")
            {
                if (manager.musicPlayer != null)
                {
                    manager.musicPlayer.FadeOutAllSongs(5f);
                }
                if (MonklandSteamworks.isManager)
                {
                    base.PlaySound(SoundID.MENU_Switch_Page_In);
                    gameStarting = true;
                    MonklandSteamworks.gameManager.QueueStart();
                }
            }
            else { base.Singal(sender, message); }
        }

        public override void ShutDownProcess()
        {
            base.ShutDownProcess();
            playerList.ClearList();
        }
    }
}