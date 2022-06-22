using Menu;
using Monkland.Hooks;
using Monkland.SteamManagement;
using RWCustom;
using UnityEngine;

namespace Monkland.Menus
{
    internal class SteamMultiplayerMenu : Menu.Menu, SelectOneButton.SelectOneButtonOwner, CheckBox.IOwnCheckBox
    {
        public MultiplayerChat gameChat;
        public MultiplayerPlayerList playerList;

        public SimpleButton backButton, startGameButton, readyUpButton;
        public HorizontalSlider bodyRed, bodyGreen, bodyBlue, eyesRed, eyesGreen, eyesBlue;
        public FLabel settingsLabel, bodyLabel, eyesLabel;
        public RoundedRect colorBox;

        public SelectOneButton[] slugcatButtons;

        private readonly FSprite slugcat, eyes, darkSprite, blackFadeSprite;
        private float blackFade;
        private float lastBlackFade;
        private bool gameStarting = false;

        public SteamMultiplayerMenu(ProcessManager manager, bool shouldCreateLobby = false) : base(manager, ProcessManager.ProcessID.MainMenu)
        {
            if (shouldCreateLobby)
            {
                MonklandSteamworks.instance.CreateLobby();
            }

            #region UI ELEMENTS SIZE DEFINITION

            float resOffset = (1366 - manager.rainWorld.screenSize.x) / 2; // shift everything to the right depending on the resolution. 1/2 of the diff with the max resolution.

            float screenWidth = manager.rainWorld.screenSize.x;
            float screenHeight = manager.rainWorld.screenSize.y;
            // initialize some dynamic sizes and positions for UI elements.
            // player list and multiplayer settings have the same size no matter what the resolution.
            // the room chat adjusts to fill all the middle space
            float panelGutter = 15; // gap width between the panels

            float playerListWidth = 200; // constant
            float multiplayerSettingsWidth = 300; // constant
            float multiplayerSettingsSliderWidth = multiplayerSettingsWidth - 115; // constant
            float roomChatWidth = screenWidth - (playerListWidth + multiplayerSettingsWidth + panelGutter * 4); // depends on resolution

            float playerListPositionX = panelGutter; // position depending on size
            float multiplayerSettingsPositionX = screenWidth - (multiplayerSettingsWidth + panelGutter); // position depending on screen width
            float multiplayerSettingsSliderPositionX = multiplayerSettingsPositionX + 10; // constant
            float roomChatPositionX = playerListWidth + panelGutter * 2; // position depending on player list width

            float controlButtonSpaceHeight = 100; // constant
            float panelHeight = screenHeight - (controlButtonSpaceHeight + panelGutter * 2); // leaving a space for control buttons
            float panelPositionY = controlButtonSpaceHeight; // constant

            #endregion UI ELEMENTS SIZE DEFINITION

            this.blackFade = 1f;
            this.lastBlackFade = 1f;
            this.pages.Add(new Page(this, null, "main", 0));
            //this.scene = new InteractiveMenuScene( this, this.pages[0], MenuScene.SceneID.Landscape_SU );
            //this.pages[0].subObjects.Add( this.scene );
            this.darkSprite = new FSprite("pixel", true)
            {
                color = new Color(0f, 0f, 0f),
                anchorX = 0f,
                anchorY = 0f,
                scaleX = screenWidth,
                scaleY = screenHeight,
                x = screenWidth / 2f,
                y = screenHeight / 2f,
                alpha = 0.85f
            };
            this.pages[0].Container.AddChild(this.darkSprite);
            this.blackFadeSprite = new FSprite("Futile_White", true)
            {
                scaleX = 87.5f,
                scaleY = 50f,
                x = screenWidth / 2f,
                y = screenHeight / 2f,
                color = new Color(0f, 0f, 0f)
            };
            Futile.stage.AddChild(this.blackFadeSprite);

            //Multiplayer Settings Box
            colorBox = new RoundedRect(this, this.pages[0], new Vector2(resOffset + multiplayerSettingsPositionX, panelPositionY), new Vector2(multiplayerSettingsWidth, panelHeight), false);
            this.pages[0].subObjects.Add(colorBox);

            //Settings Label
            settingsLabel = new FLabel("font", "Multiplayer Settings");
            settingsLabel.SetPosition(new Vector2(multiplayerSettingsPositionX + 70.01f, screenHeight - 60.01f));
            Futile.stage.AddChild(this.settingsLabel);

            //Body Color Label
            bodyLabel = new FLabel("font", "Body Color");
            bodyLabel.SetPosition(new Vector2(multiplayerSettingsPositionX + 70.01f, screenHeight - 90.01f));
            Futile.stage.AddChild(this.bodyLabel);

            //Red Slider
            bodyRed = new HorizontalSlider(this, this.pages[0], "Red", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 130), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            bodyRed.floatValue = MonklandSteamworks.gameManager.bodyColor.r;
            bodyRed.buttonBehav.greyedOut = false;
            this.pages[0].subObjects.Add(this.bodyRed);
            //Green Slider
            bodyGreen = new HorizontalSlider(this, this.pages[0], "Green", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 170), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            bodyGreen.floatValue = MonklandSteamworks.gameManager.bodyColor.g;
            bodyGreen.buttonBehav.greyedOut = false;
            this.pages[0].subObjects.Add(this.bodyGreen);
            //Blue Slider
            bodyBlue = new HorizontalSlider(this, this.pages[0], "Blue", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 210), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            bodyBlue.floatValue = MonklandSteamworks.gameManager.bodyColor.b;
            bodyBlue.buttonBehav.greyedOut = false;
            this.pages[0].subObjects.Add(this.bodyBlue);

            //Eye Color Label
            eyesLabel = new FLabel("font", "Eye Color");
            eyesLabel.SetPosition(new Vector2(multiplayerSettingsPositionX + 70.01f, screenHeight - 240.01f));
            Futile.stage.AddChild(this.eyesLabel);

            //Red Slider
            eyesRed = new HorizontalSlider(this, this.pages[0], "Red ", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 280), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            eyesRed.floatValue = MonklandSteamworks.gameManager.eyeColor.r;
            this.pages[0].subObjects.Add(this.eyesRed);
            //Green Slider
            eyesGreen = new HorizontalSlider(this, this.pages[0], "Green ", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 320), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            eyesGreen.floatValue = MonklandSteamworks.gameManager.eyeColor.g;
            this.pages[0].subObjects.Add(this.eyesGreen);
            //Blue Slider
            eyesBlue = new HorizontalSlider(this, this.pages[0], "Blue ", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 360), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            eyesBlue.floatValue = MonklandSteamworks.gameManager.eyeColor.b;
            this.pages[0].subObjects.Add(this.eyesBlue);

            //Slugcat Eyes Sprite
            eyes = new FSprite("FoodCircleB", true);
            eyes.scaleX = 1f;
            eyes.scaleY = 1.1f;
            eyes.color = new Color(0, 0, 0);
            eyes.x = multiplayerSettingsPositionX + 130;
            eyes.y = manager.rainWorld.screenSize.y - 236;
            eyes.isVisible = true;
            this.pages[0].Container.AddChild(this.eyes);

            //Slugcat Sprite
            slugcat = new FSprite("slugcatSleeping", true);
            slugcat.scaleX = 1f;
            slugcat.scaleY = 1f;
            slugcat.color = new Color(1f, 1f, 1f);
            slugcat.x = multiplayerSettingsPositionX + 136;
            slugcat.y = manager.rainWorld.screenSize.y - 235;
            slugcat.isVisible = true;
            this.pages[0].Container.AddChild(this.slugcat);

            //Slugcat Buttons
            this.slugcatButtons = new SelectOneButton[3];
            this.slugcatButtons[0] = new SelectOneButton(this, this.pages[0], base.Translate("SURVIOR"), "Slugcat", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 445f), new Vector2(110f, 30f), this.slugcatButtons, 0);
            this.pages[0].subObjects.Add(this.slugcatButtons[0]);
            this.slugcatButtons[1] = new SelectOneButton(this, this.pages[0], base.Translate("MONK"), "Slugcat", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 490f), new Vector2(110f, 30f), this.slugcatButtons, 1);
            this.pages[0].subObjects.Add(this.slugcatButtons[1]);
            this.slugcatButtons[2] = new SelectOneButton(this, this.pages[0], base.Translate("HUNTER"), "Slugcat", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 535f), new Vector2(110f, 30f), this.slugcatButtons, 2);
            this.pages[0].subObjects.Add(this.slugcatButtons[2]);

            //Back button
            this.backButton = new SimpleButton(this, this.pages[0], base.Translate("BACK"), "EXIT", new Vector2(resOffset + 15f, 50f), new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.backButton);

            //Start Game button
            this.startGameButton = new SimpleButton(this, this.pages[0], "Start Game", "STARTGAME", new Vector2(resOffset + screenWidth - 125, 50f), new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.startGameButton);

            //Ready Up button
            this.readyUpButton = new SimpleButton(this, this.pages[0], "Ready UP", "READYUP", new Vector2(resOffset + screenWidth - 250, 50f), new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.readyUpButton);

            //Multiplayer Chat
            this.gameChat = new MultiplayerChat(this, this.pages[0], new Vector2(resOffset + roomChatPositionX, panelPositionY), new Vector2(roomChatWidth, panelHeight));
            this.pages[0].subObjects.Add(this.gameChat);

            //Invite menu
            playerList = new MultiplayerPlayerList(this, this.pages[0], new Vector2(resOffset + playerListPositionX, panelPositionY), new Vector2(playerListWidth, panelHeight), new Vector2(playerListWidth - 20, playerListWidth - 20));
            this.pages[0].subObjects.Add(this.playerList);

            //Controller Combatability
            this.bodyRed.nextSelectable[1] = this.readyUpButton;
            this.bodyRed.nextSelectable[3] = this.bodyGreen;
            this.bodyGreen.nextSelectable[1] = this.bodyRed;
            this.bodyGreen.nextSelectable[3] = this.bodyBlue;
            this.bodyBlue.nextSelectable[1] = this.bodyGreen;
            this.bodyBlue.nextSelectable[3] = this.eyesRed;
            this.eyesRed.nextSelectable[1] = this.bodyBlue;
            this.eyesRed.nextSelectable[3] = this.eyesGreen;
            this.eyesGreen.nextSelectable[1] = this.eyesRed;
            this.eyesGreen.nextSelectable[3] = this.eyesBlue;
            this.eyesBlue.nextSelectable[1] = this.eyesGreen;
            this.eyesBlue.nextSelectable[3] = this.readyUpButton;
            this.readyUpButton.nextSelectable[0] = this.backButton;
            this.readyUpButton.nextSelectable[1] = this.eyesBlue;
            this.readyUpButton.nextSelectable[2] = this.startGameButton;
            this.readyUpButton.nextSelectable[3] = this.bodyRed;
            this.startGameButton.nextSelectable[0] = this.readyUpButton;
            this.startGameButton.nextSelectable[1] = this.eyesBlue;
            this.startGameButton.nextSelectable[2] = this.backButton;
            this.startGameButton.nextSelectable[3] = this.bodyRed;
            this.backButton.nextSelectable[0] = this.startGameButton;
            this.backButton.nextSelectable[2] = this.readyUpButton;
            this.backButton.nextSelectable[1] = this.bodyRed;
            this.backButton.nextSelectable[3] = this.eyesBlue;

            //Some Nice Music :)
            if (manager.musicPlayer != null)
            { manager.musicPlayer.MenuRequestsSong("NA_05 - Sparkles", 1.2f, 10f); }

            //Fix Label pixelperfect
            foreach (MenuObject o in this.pages[0].subObjects)
            { if (o is MenuLabel l) { l.pos += new Vector2(0.01f, 0.01f); } }
        }

        public override void SliderSetValue(Slider slider, float f)
        {
            if (MonklandSteamworks.connectedPlayers.Count > 0 && slider.ID == (Slider.SliderID)(-1))
            {
                f = Mathf.Clamp(f, 0.004f, 1.000f);
                if (MonklandSteamworks.gameManager.updateDelay == -1)
                {
                    MonklandSteamworks.gameManager.updateDelay = 100;
                }
                if (slider == this.bodyRed)
                {
                    MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)] = new Color(f, MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g, MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b);
                    MonklandSteamworks.gameManager.bodyColor = new Color(f, MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g, MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b);
                }
                else if (slider == this.bodyGreen)
                {
                    MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)] = new Color(MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r, f, MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b);
                    MonklandSteamworks.gameManager.bodyColor = new Color(MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r, f, MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b);
                }
                else if (slider == this.bodyBlue)
                {
                    MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)] = new Color(MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r, MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g, f);
                    MonklandSteamworks.gameManager.bodyColor = new Color(MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r, MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g, f);
                }
                else if (slider == this.eyesRed)
                {
                    MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)] = new Color(f, MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g, MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b);
                    MonklandSteamworks.gameManager.eyeColor = new Color(f, MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g, MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b);
                }
                else if (slider == this.eyesGreen)
                {
                    MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)] = new Color(MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r, f, MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b);
                    MonklandSteamworks.gameManager.eyeColor = new Color(MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r, f, MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b);
                }
                else if (slider == this.eyesBlue)
                {
                    MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)] = new Color(MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r, MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g, f);
                    MonklandSteamworks.gameManager.eyeColor = new Color(MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r, MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g, f);
                }
                return;
            }
            base.SliderSetValue(slider, f);
        }

        public override float ValueOfSlider(Slider slider)
        {
            if (MonklandSteamworks.connectedPlayers.Count > 0 && slider.ID == (Slider.SliderID)(-1))
            {
                if (slider == this.bodyRed) { return MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r; }
                if (slider == this.bodyGreen) { return MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g; }
                if (slider == this.bodyBlue) { return MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b; }
                if (slider == this.eyesRed) { return MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].r; }
                if (slider == this.eyesGreen) { return MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].g; }
                if (slider == this.eyesBlue) { return MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)].b; }
            }
            return base.ValueOfSlider(slider);
        }

        public int GetCurrentlySelectedOfSeries(string series)
        {
            switch (series)
            {
                case "Slugcat":
                    return this.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
            }
            return -1;
        }

        public void SetCurrentlySelectedOfSeries(string series, int to)
        {
            switch (series)
            {
                case "Slugcat":
                    this.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = to;
                    break;
            }
        }

        public bool GetChecked(CheckBox box)
        {
            string idstring = box.IDString;
            /*
            switch (idstring)
            {
                case "DEBUG":
                    return MonklandSteamManager.DEBUG;
            }
            */
            return false;
        }

        public void SetChecked(CheckBox box, bool c)
        {
            string idstring = box.IDString;
            /*
            switch (idstring)
            {
                case "DEBUG":
                    MonklandSteamManager.DEBUG = c;
                    break;
            }
            */
        }

        public override void Update()
        {
            this.lastBlackFade = this.blackFade;
            float num = 0f;
            if (this.blackFade < num)
            { this.blackFade = Custom.LerpAndTick(this.blackFade, num, 0.05f, 0.06666667f); }
            else
            { this.blackFade = Custom.LerpAndTick(this.blackFade, num, 0.05f, 0.125f); }
            if (MonklandSteamworks.connectedPlayers.Count > 0)
            {
                slugcat.color = MonklandSteamworks.gameManager.playerColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)];
                //Debug.Log("Color: " + MonklandSteamManager.gameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkgameManager.playerID)].r + ", " + MonklandSteamManager.gameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkgameManager.playerID)].g + ", " + MonklandSteamManager.gameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkgameManager.playerID)].b);
                eyes.color = MonklandSteamworks.gameManager.playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(MonklandSteamworks.playerID)];
            }
            if (MonklandSteamworks.managerID == MonklandSteamworks.playerID)
            {
                //startGameButton.pos = new Vector2( 1060, 50f );
                startGameButton.buttonBehav.greyedOut = gameStarting;
                readyUpButton.buttonBehav.greyedOut = gameStarting;
                backButton.buttonBehav.greyedOut = gameStarting;
            }
            else
            {
                //startGameButton.pos = new Vector2(-100000, 50f);
                startGameButton.buttonBehav.greyedOut = true;
                readyUpButton.buttonBehav.greyedOut = gameStarting;
                backButton.buttonBehav.greyedOut = gameStarting;
            }
            base.Update();
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            this.blackFadeSprite.alpha = Mathf.Lerp(this.lastBlackFade, this.blackFade, timeStacker);
        }

        public override void ShutDownProcess()
        {
            base.ShutDownProcess();
            gameChat.ClearMessages();
            playerList.ClearList();
            this.darkSprite.RemoveFromContainer();
            this.eyes.RemoveFromContainer();
            this.slugcat.RemoveFromContainer();
            this.eyesLabel.RemoveFromContainer();
            this.bodyLabel.RemoveFromContainer();
            this.settingsLabel.RemoveFromContainer();
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
            else if (message == "STARTGAME")
            {
                if (manager.musicPlayer != null)
                { manager.musicPlayer.FadeOutAllSongs(5f); }
                if (MonklandSteamworks.isManager)
                {
                    settingsLabel.text = "";
                    bodyLabel.text = "";
                    eyesLabel.text = "";
                    base.PlaySound(SoundID.MENU_Switch_Page_In);
                    gameStarting = true;
                    MonklandSteamworks.gameManager.QueueStart();
                }
            }
        }
    }
}