using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Monkland.SteamManagement;
using RWCustom;
using Steamworks;
using Monkland.Hooks.Entities;

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
        public bool mouseDown;
        public bool mouseClick;
        public bool lastMouseDown;
        public MUIPlayerList muiPlayerList;

        public List<MUIHUD> muiElements;
        public FContainer frontContainer;
        private bool exitButton;
        private bool exitting;

        public MultiplayerHUD(HUD.HUD hud) : base(hud)
        {
            //Futile.stage.AddChild(this.inFrontContainer);
            this.hud = hud;
            Debug.Log("Added MultiHUD");
            playerLabels = new Dictionary<ulong, MUIPlayerTag>();
            elementsToBeRemoved = new List<ulong>();
            overLayActive = false;
            exitting = false;
            frontContainer = new FContainer();

            this.backgroundBlack = new FSprite("Futile_White", true);
            this.backgroundBlack.color = new Color(0f, 0f, 0f);
            this.container.AddChild(this.backgroundBlack);
            this.backgroundBlack.scaleX = this.hud.rainWorld.options.ScreenSize.x / 16f;
            this.backgroundBlack.scaleY = 48f;
            this.backgroundBlack.anchorX = 0f;
            this.backgroundBlack.anchorY = 0f;
            this.backgroundBlack.x = 0f;
            this.backgroundBlack.y = 0f;
            this.backgroundBlack.alpha = 0.3f;
            this.backgroundBlack.isVisible = false;
            muiElements = new List<MUIHUD>();

            exitButton = false;

            muiElements.Add(new MUIPlayerList(this, new Vector2(this.hud.rainWorld.options.ScreenSize.x / 2f, this.hud.rainWorld.options.ScreenSize.y / 2f)));
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
                if (!playerLabels.ContainsKey(AbstractPhysicalObjectHK.GetField(player).owner))
                {
                    playerLabels.Add(AbstractPhysicalObjectHK.GetField(player).owner, new MUIPlayerTag(player, name, color, this));
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
            Debug.Log("Requested multi pause menu");
        }

        public void ExitButton()
        {
            exitButton = !exitButton;
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
                    Debug.Log(e.Message);
                }
            }
        }

        public override void Update()
        {
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
                catch (Exception e) { Debug.Log(e.Message); }
            }

            this.counter++;
            this.lastMousePos = this.mousePos;
            this.mousePos = Input.mousePosition;
            this.mouseDown = Input.GetMouseButton(0);
            this.mouseClick = (this.mouseDown && !this.lastMouseDown);
            this.lastMouseDown = this.mouseDown;

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
    }
}