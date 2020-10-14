using HUD;
using System;
using System.Collections.Generic;
using UnityEngine;
using Monkland.SteamManagement;
using Monkland.Hooks.Entities;
using Monkland.Hooks;

namespace Monkland.UI
{
    public class MultiplayerHUD : HudPart
    {
        //private FNode inFrontContainer;
        // public HUD.HUD hud;

        private int counter;
        private FSprite backgroundBlack;
        public Dictionary<ulong, MUIPlayerTag> playerLabels;
        private List<ulong> elementsToBeRemoved;
        public bool overLayActive;
        public Vector2 lastMousePos;
        public Vector2 mousePos;
        public MUIPlayerList muiPlayerList;

        public List<MUIHUD> muiElements;
        public FContainer frontContainer;
        private bool exitButton;
        private bool exitting;

        public Vector2 screenSize;
        public Vector2 screenPos;

        public MultiplayerHUD(HUD.HUD hud) : base(hud)
        {
            this.screenSize = RainWorldHK.mainRW.options.ScreenSize;
            this.screenPos = this.screenSize - new Vector2(1366f, 768f); // This needs to be adjusted
            //Futile.stage.AddChild(this.inFrontContainer);
            this.hud = hud;
            Debug.Log("Monkland) Added MultiplayerHUD");
            playerLabels = new Dictionary<ulong, MUIPlayerTag>();
            elementsToBeRemoved = new List<ulong>();
            overLayActive = false;
            exitting = false;
            frontContainer = new FContainer();

            this.backgroundBlack = new FSprite("Futile_White", true);
            this.backgroundBlack.color = new Color(0.01f, 0.01f, 0.01f);
            this.container.AddChild(this.backgroundBlack);
            this.backgroundBlack.scaleX = this.screenSize.x / 16f;
            this.backgroundBlack.scaleY = 48f;
            this.backgroundBlack.anchorX = 0f;
            this.backgroundBlack.anchorY = 0f;
            this.backgroundBlack.x = 0f;
            this.backgroundBlack.y = 0f;
            this.backgroundBlack.alpha = 0.3f;
            this.backgroundBlack.isVisible = false;
            muiElements = new List<MUIHUD>();

            exitButton = false;

            muiElements.Add(new MUIButton(this, new Vector2(this.ContinueAndExitButtonsXPos - 320f, 20f), signalShutdown, "SHUTDOWN"));
            muiElements.Add(new MUIPlayerList(this, new Vector2(this.screenSize.x / 2f, this.screenSize.y / 2f)));
            Futile.stage.AddChild(frontContainer);
        }

        public FContainer container
        {
            get
            {
                return this.hud.fContainers[0];
            }
        }

        public void AddLabel(AbstractPhysicalObject player, string name, Color color)
        {
            try
            {
                if (!playerLabels.ContainsKey(AbstractPhysicalObjectHK.GetField(player).ownerID))
                {
                    playerLabels.Add(AbstractPhysicalObjectHK.GetField(player).ownerID, new MUIPlayerTag(player, name, color, this));
                }
            }
            catch //(Exception e)
            {
                // Exception
                // Debug.Log("Playerlabel " + e);
            }
        }

        public void ShowMultiPauseMenu()
        {
            overLayActive = !overLayActive;
            Debug.Log("Monkland) Requested multi pause menu");
        }

        private const string signalShutdown = "SHUTDOWN";

        public void Signal(MUIHUD item, string signal)
        {
            switch (signal)
            {
                case signalShutdown:
                    this.exitButton = !this.exitButton; break;
            }
        }

        public override void ClearSprites()
        {
            if (this.backgroundBlack != null)
            {
                this.backgroundBlack.RemoveFromContainer();
            }

            foreach (KeyValuePair<ulong, MUIPlayerTag> label in playerLabels)
            {
                label.Value.ClearSprites();
            }

            foreach (MUIHUD item in muiElements)
            {
                item.ClearSprites();
            }
            frontContainer.RemoveFromContainer();
            base.ClearSprites();
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            foreach (KeyValuePair<ulong, MUIPlayerTag> label in playerLabels)
            {
                label.Value.Draw(timeStacker);
            }

            foreach (MUIHUD item in muiElements)
            {
                item.Draw(timeStacker);
            }
        }

        public void ExitGame(ProcessManager manager)
        {
            if (!exitting)
            {
                try
                {
                    if (manager.musicPlayer != null)
                    {
                        manager.musicPlayer.FadeOutAllSongs(5f);
                        manager.musicPlayer.MenuRequestsSong("RW_8 - Sundown", 1.4f, 2f);
                    }
                    exitting = true;
                    manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                    MonklandSteamManager.instance.OnGameExit();
                }
                catch (Exception e)
                {
                    Debug.Log($"Monkland Error in MultiplayerHUD.ExitGame: {e.Message}");
                }
            }
        }

        public override void Update()
        {
            this.counter++;
            this.lastMousePos = this.mousePos;
            this.mousePos = (Vector2)Input.mousePosition;

            // Make items visible
            this.backgroundBlack.isVisible = overLayActive;
            foreach (MUIHUD item in muiElements)
            {
                item.isVisible = overLayActive;
                item.Update();
            }
            Screen.showCursor = overLayActive;

            if (this.overLayActive && exitButton)
            {
                try
                {
                    if (hud.owner != null && hud.owner is Player)
                    {
                        ExitGame((hud.owner as Player).room.game.manager);
                    }
                }
                catch (Exception e) { Debug.Log($"Monkland Error in MultiplayerHUD.Update: {e.Message}"); }
            }

            base.Update();

            elementsToBeRemoved.Clear();
            foreach (KeyValuePair<ulong, MUIPlayerTag> label in playerLabels)
            {
                label.Value.Update();
                if (label.Value.slatedForDeletion)
                {
                    label.Value.ClearSprites();
                    elementsToBeRemoved.Add(label.Key);
                }
            }

            foreach (ulong key in elementsToBeRemoved)
            {
                playerLabels.Remove(key);
            }
        }

        public float ContinueAndExitButtonsXPos
        {
            get
            {
                if (this.screenSize.x >= 1360f)
                {
                    return 1366f;
                }
                if (this.screenSize.x == 1280f)
                {
                    return 1280f;
                }
                return this.screenSize.x + 190f;
            }
        }
    }
}
