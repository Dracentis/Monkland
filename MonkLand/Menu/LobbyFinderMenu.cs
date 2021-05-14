using Menu;
using Monkland.Hooks;
using Monkland.SteamManagement;
using Monkland.UI;
using RWCustom;
using Steamworks;
using UnityEngine;

namespace Monkland
{
    internal class LobbyFinderMenu : Menu.Menu, SelectOneButton.SelectOneButtonOwner, CheckBox.IOwnCheckBox
    {
        public SimpleButton backButton, startLobbyButton;
        public HorizontalSlider bodyRed, bodyGreen, bodyBlue, eyesRed, eyesGreen, eyesBlue;
        public CheckBox debugCheckBox;
        public FLabel settingsLabel, bodyLabel, eyesLabel;
        public RoundedRect colorBox, lobbyFinderBox;
        public SelectOneButton[] modeButtons = new SelectOneButton[2];
        public SelectOneButton[] slugcatButtons = new SelectOneButton[3];

        public FLabel creatorLabel;
        public SelectOneButton[] privacyButtons = new SelectOneButton[3];
        public HorizontalSlider maxPlayersSlider;
        public FLabel maxPlayersLabel;
        public CheckBox spearsHitBox, debugAllowBox;

        public bool hosting = true;
        public float lobbynum = 0;
        public SimpleButton refreshButton;

        public FLabel namesLabel, versionsLabel, playersLabel, maxLabel; // myVersion; //myVersion is not being used???
        public FLabel[] lobbynames, lobbyVersions, lobbyPlayerCounts, lobbyPlayerMaxs;
        public SimpleButton[] joinButtons;

        private readonly FSprite slugcat, eyes, darkSprite, blackFadeSprite;
        private float blackFade, lastBlackFade;

        public LobbyFinderMenu(ProcessManager manager) : base(manager, ProcessManager.ProcessID.MainMenu)
        {
            #region UI ELEMENTS SIZE DEFINITION

            float resOffset = (1366 - manager.rainWorld.screenSize.x) / 2; // shift everything to the right depending on the resolution. 1/2 of the diff with the max resolution.

            float screenWidth = manager.rainWorld.screenSize.x;
            float screenHeight = manager.rainWorld.screenSize.y;
            // initialize some dynamic sizes and positions for UI elements.
            // player list and multiplayer settings have the same size no matter what the resolution.
            // the room chat adjusts to fill all the middle space
            float panelGutter = 15; // gap width between the panels

            float multiplayerSettingsWidth = 300; // constant
            float multiplayerSettingsSliderWidth = multiplayerSettingsWidth - 115; // constant

            float multiplayerSettingsPositionX = screenWidth - (multiplayerSettingsWidth + panelGutter); // position depending on screen width
            float multiplayerSettingsSliderPositionX = multiplayerSettingsPositionX + 10; // constant

            float controlButtonSpaceHeight = 100; // constant
            float panelHeight = screenHeight - (controlButtonSpaceHeight + panelGutter * 2); // leaving a space for bottom control buttons
            float panelPositionY = controlButtonSpaceHeight; // constant

            float lobbyFinderHeight = screenHeight - (controlButtonSpaceHeight + (panelGutter) + 50); // leaving a space for top and bottom control buttons
            float lobbyFinderWidth = (screenWidth - (multiplayerSettingsWidth + panelGutter * 3)) / 1.5f; // depends on resolution
            float lobbyFinderPositionX = resOffset + panelGutter;
            float lobbyFinderPositionY = panelPositionY;

            float lobbyCreatorHeight = screenHeight - (controlButtonSpaceHeight + panelGutter * 2);
            float lobbyCreatorWidth = screenWidth - (multiplayerSettingsWidth + lobbyFinderWidth + panelGutter * 4);
            float privacyButtonWidth = (lobbyCreatorWidth - 20 - (panelGutter * 2)) / 3f;
            float lobbyCreatorSliderWidth = lobbyCreatorWidth - 115;
            float lobbyCreatorPositionX = resOffset + panelGutter * 2 + lobbyFinderWidth;
            float lobbyCreatorPositionY = panelPositionY;
            float lobbyCreatorSliderPositionX = lobbyCreatorPositionX + 10;

            #endregion UI ELEMENTS SIZE DEFINITION

            this.lobbynum = 0;
            hosting = true;
            this.blackFade = 1f;
            this.lastBlackFade = 1f;
            this.pages.Add(new Page(this, null, "main", 0));
            //this.scene = new InteractiveMenuScene( this, this.pages[0], MenuScene.SceneID.Landscape_SU );
            //this.pages[0].subObjects.Add( this.scene );
            this.darkSprite = new FSprite("pixel", true);
            this.darkSprite.color = new Color(0f, 0f, 0f);
            this.darkSprite.anchorX = 0f;
            this.darkSprite.anchorY = 0f;
            this.darkSprite.scaleX = screenWidth;
            this.darkSprite.scaleY = screenHeight;
            this.darkSprite.x = screenWidth / 2f;
            this.darkSprite.y = screenHeight / 2f;
            this.darkSprite.alpha = 0.85f;
            this.pages[0].Container.AddChild(this.darkSprite);
            this.blackFadeSprite = new FSprite("Futile_White", true);
            this.blackFadeSprite.scaleX = 87.5f;
            this.blackFadeSprite.scaleY = 50f;
            this.blackFadeSprite.x = screenWidth / 2f;
            this.blackFadeSprite.y = screenHeight / 2f;
            this.blackFadeSprite.color = new Color(0f, 0f, 0f);
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
            bodyRed.floatValue = MonklandSteamManager.bodyColor.r;
            bodyRed.buttonBehav.greyedOut = false;
            this.pages[0].subObjects.Add(this.bodyRed);
            //Green Slider
            bodyGreen = new HorizontalSlider(this, this.pages[0], "Green", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 170), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            bodyGreen.floatValue = MonklandSteamManager.bodyColor.g;
            bodyGreen.buttonBehav.greyedOut = false;
            this.pages[0].subObjects.Add(this.bodyGreen);
            //Blue Slider
            bodyBlue = new HorizontalSlider(this, this.pages[0], "Blue", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 210), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            bodyBlue.floatValue = MonklandSteamManager.bodyColor.b;
            bodyBlue.buttonBehav.greyedOut = false;
            this.pages[0].subObjects.Add(this.bodyBlue);

            //Eye Color Label
            eyesLabel = new FLabel("font", "Eye Color");
            eyesLabel.SetPosition(new Vector2(multiplayerSettingsPositionX + 70.01f, screenHeight - 240.01f));
            Futile.stage.AddChild(this.eyesLabel);

            //Red Slider
            eyesRed = new HorizontalSlider(this, this.pages[0], "Red ", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 280), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            eyesRed.floatValue = MonklandSteamManager.eyeColor.r;
            this.pages[0].subObjects.Add(this.eyesRed);
            //Green Slider
            eyesGreen = new HorizontalSlider(this, this.pages[0], "Green ", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 320), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            eyesGreen.floatValue = MonklandSteamManager.eyeColor.g;
            this.pages[0].subObjects.Add(this.eyesGreen);
            //Blue Slider
            eyesBlue = new HorizontalSlider(this, this.pages[0], "Blue ", new Vector2(resOffset + multiplayerSettingsSliderPositionX, screenHeight - 360), new Vector2(multiplayerSettingsSliderWidth, 30), (Slider.SliderID)(-1), false);
            eyesBlue.floatValue = MonklandSteamManager.eyeColor.b;
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

            //Debug Mode checkbox
            this.debugCheckBox = new CheckBox(this, this.pages[0], this, new Vector2(resOffset + multiplayerSettingsSliderPositionX + 120, screenHeight - 400), 120f, "Debug Mode", "DEBUG");
            this.pages[0].subObjects.Add(this.debugCheckBox);

            //Back button
            this.backButton = new SimpleButton(this, this.pages[0], base.Translate("BACK"), "EXIT", new Vector2(resOffset + 15f, 50f), new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.backButton);

            //Lobby Finder Box
            lobbyFinderBox = new RoundedRect(this, this.pages[0], new Vector2(lobbyFinderPositionX, lobbyFinderPositionY), new Vector2(lobbyFinderWidth, lobbyFinderHeight), false);
            this.pages[0].subObjects.Add(lobbyFinderBox);

            //Refresh button
            this.refreshButton = new SimpleButton(this, this.pages[0], "Refresh", "REFRESH", new Vector2(lobbyFinderPositionX + lobbyFinderWidth - 110, screenHeight - 50f), new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.refreshButton);

            //Lobby browser Labels:
            this.namesLabel = new FLabel("font", "CREATED BY");
            this.namesLabel.SetPosition(new Vector2(lobbyFinderPositionX + 60.01f - resOffset + (lobbyFinderWidth / 10), screenHeight - 75.01f));
            Futile.stage.AddChild(this.namesLabel);

            this.versionsLabel = new FLabel("font", "VERSION");
            this.versionsLabel.SetPosition(new Vector2(lobbyFinderPositionX + 60.01f - resOffset + 1.8f * (lobbyFinderWidth / 5), screenHeight - 75.01f));
            Futile.stage.AddChild(this.versionsLabel);

            this.playersLabel = new FLabel("font", "PLAYERS");
            this.playersLabel.SetPosition(new Vector2(lobbyFinderPositionX + 60.01f - resOffset + 2.9f * (lobbyFinderWidth / 5), screenHeight - 75.01f));
            Futile.stage.AddChild(this.playersLabel);

            this.maxLabel = new FLabel("font", "MAX");
            this.maxLabel.SetPosition(new Vector2(lobbyFinderPositionX + 60.01f - resOffset + 4 * (lobbyFinderWidth / 6), screenHeight - 75.01f));
            Futile.stage.AddChild(this.maxLabel);

            //Join buttons
            this.joinButtons = new SimpleButton[15];
            this.lobbynames = new FLabel[this.joinButtons.Length];
            this.lobbyVersions = new FLabel[this.joinButtons.Length];
            this.lobbyPlayerCounts = new FLabel[this.joinButtons.Length];
            this.lobbyPlayerMaxs = new FLabel[this.joinButtons.Length];
            float buttonheight = (lobbyFinderHeight - 30) / this.joinButtons.Length;
            for (int i = 0; i < this.joinButtons.Length; i++)
            {
                this.lobbynames[i] = new FLabel("font", "-");
                this.lobbynames[i].SetPosition(new Vector2(lobbyFinderPositionX + 60 - resOffset + (lobbyFinderWidth / 10), screenHeight - 100f - (buttonheight * i)));
                Futile.stage.AddChild(this.lobbynames[i]);

                this.lobbyVersions[i] = new FLabel("font", "-");
                this.lobbyVersions[i].SetPosition(new Vector2(lobbyFinderPositionX + 60 - resOffset + 1.8f * (lobbyFinderWidth / 5), screenHeight - 100f - (buttonheight * i)));
                Futile.stage.AddChild(this.lobbyVersions[i]);

                this.lobbyPlayerCounts[i] = new FLabel("font", "-");
                this.lobbyPlayerCounts[i].SetPosition(new Vector2(lobbyFinderPositionX + 60 - resOffset + 2.9f * (lobbyFinderWidth / 5), screenHeight - 100f - (buttonheight * i)));
                Futile.stage.AddChild(this.lobbyPlayerCounts[i]);

                this.lobbyPlayerMaxs[i] = new FLabel("font", "-");
                this.lobbyPlayerMaxs[i].SetPosition(new Vector2(lobbyFinderPositionX + 60 - resOffset + 4 * (lobbyFinderWidth / 6), screenHeight - 100f - (buttonheight * i)));
                Futile.stage.AddChild(this.lobbyPlayerMaxs[i]);

                this.joinButtons[i] = new SimpleButton(this, this.pages[0], "Join", "JOIN" + i, new Vector2(lobbyFinderPositionX + lobbyFinderWidth - 75f, screenHeight - 110f - (buttonheight * i)), new Vector2(60f, buttonheight - 10));
                this.pages[0].subObjects.Add(this.joinButtons[i]);
            }

            //Host or Join Buttons
            this.modeButtons = new SelectOneButton[2];
            this.modeButtons[1] = new SelectOneButton(this, this.pages[0], base.Translate("JOIN"), "Mode", new Vector2(resOffset + 15, screenHeight - 50f), new Vector2(110f, 30f), this.modeButtons, 1);
            this.pages[0].subObjects.Add(this.modeButtons[1]);
            this.modeButtons[0] = new SelectOneButton(this, this.pages[0], base.Translate("HOST"), "Mode", new Vector2(resOffset + 140, screenHeight - 50f), new Vector2(110f, 30f), this.modeButtons, 0);
            this.pages[0].subObjects.Add(this.modeButtons[0]);

            //Lobby Creator Box
            lobbyFinderBox = new RoundedRect(this, this.pages[0], new Vector2(lobbyCreatorPositionX, lobbyCreatorPositionY), new Vector2(lobbyCreatorWidth, lobbyCreatorHeight), false);
            this.pages[0].subObjects.Add(lobbyFinderBox);

            //Lobby Creator Label
            creatorLabel = new FLabel("font", "Lobby Settings");
            creatorLabel.SetPosition(new Vector2(lobbyCreatorPositionX + 60.01f - resOffset, screenHeight - 60.01f));
            Futile.stage.AddChild(this.creatorLabel);

            //Lobby Type Buttons
            this.privacyButtons = new SelectOneButton[3];
            this.privacyButtons[0] = new SelectOneButton(this, this.pages[0], base.Translate("PUBLIC"), "LobbyType", new Vector2(lobbyCreatorSliderPositionX, screenHeight - 130f), new Vector2(privacyButtonWidth, 30f), this.slugcatButtons, 0);
            this.pages[0].subObjects.Add(this.privacyButtons[0]);
            this.privacyButtons[1] = new SelectOneButton(this, this.pages[0], base.Translate("FRIENDS"), "LobbyType", new Vector2(lobbyCreatorSliderPositionX + privacyButtonWidth + 15, screenHeight - 130f), new Vector2(privacyButtonWidth, 30f), this.slugcatButtons, 1);
            this.pages[0].subObjects.Add(this.privacyButtons[1]);
            this.privacyButtons[2] = new SelectOneButton(this, this.pages[0], base.Translate("PRIVATE"), "LobbyType", new Vector2(lobbyCreatorSliderPositionX + 2 * privacyButtonWidth + 30, screenHeight - 130f), new Vector2(privacyButtonWidth, 30f), this.slugcatButtons, 2);
            this.pages[0].subObjects.Add(this.privacyButtons[2]);

            //Players Slider
            maxPlayersSlider = new HorizontalSlider(this, this.pages[0], "", new Vector2(lobbyCreatorSliderPositionX, screenHeight - 170), new Vector2(lobbyCreatorSliderWidth + 70, 30), (Slider.SliderID)(-1), false);
            maxPlayersSlider.floatValue = 10f / 100f;
            maxPlayersSlider.buttonBehav.greyedOut = false;
            this.pages[0].subObjects.Add(this.maxPlayersSlider);

            //Max Players Label
            maxPlayersLabel = new FLabel("font", "Max Players: 10");
            maxPlayersLabel.SetPosition(new Vector2(lobbyCreatorSliderPositionX + (lobbyCreatorWidth / 2) - 20.01f - resOffset, screenHeight - 200.01f));
            Futile.stage.AddChild(this.maxPlayersLabel);

            //Spears checkbox
            this.spearsHitBox = new CheckBox(this, this.pages[0], this, new Vector2(lobbyCreatorSliderPositionX + 120, screenHeight - 250), 120f, "Spears Hit", "SPEARSHIT");
            this.pages[0].subObjects.Add(this.spearsHitBox);

            //Allow Debug checkbox
            this.debugAllowBox = new CheckBox(this, this.pages[0], this, new Vector2(lobbyCreatorSliderPositionX + 120, screenHeight - 280), 120f, "Allow Debug Mode", "ALLOWDEBUG");
            this.pages[0].subObjects.Add(this.debugAllowBox);

            //Start Game button
            this.startLobbyButton = new SimpleButton(this, this.pages[0], "Start Lobby", "STARTLOBBY", new Vector2(lobbyCreatorPositionX + lobbyCreatorWidth - 110, 50f), new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.startLobbyButton);

            //Controller Combatability
            this.bodyRed.nextSelectable[0] = this.privacyButtons[2];
            this.bodyRed.nextSelectable[1] = this.startLobbyButton;
            this.bodyRed.nextSelectable[3] = this.bodyGreen;
            this.bodyRed.nextSelectable[2] = this.modeButtons[1];
            this.bodyGreen.nextSelectable[0] = this.maxPlayersSlider;
            this.bodyGreen.nextSelectable[1] = this.bodyRed;
            this.bodyGreen.nextSelectable[3] = this.bodyBlue;
            this.bodyGreen.nextSelectable[2] = this.modeButtons[1];
            this.bodyBlue.nextSelectable[0] = this.maxPlayersSlider;
            this.bodyBlue.nextSelectable[1] = this.bodyGreen;
            this.bodyBlue.nextSelectable[3] = this.eyesRed;
            this.bodyBlue.nextSelectable[2] = this.modeButtons[1];
            this.eyesRed.nextSelectable[0] = this.spearsHitBox;
            this.eyesRed.nextSelectable[1] = this.bodyBlue;
            this.eyesRed.nextSelectable[3] = this.eyesGreen;
            this.eyesRed.nextSelectable[2] = this.modeButtons[1];
            this.eyesGreen.nextSelectable[0] = this.debugAllowBox;
            this.eyesGreen.nextSelectable[1] = this.eyesRed;
            this.eyesGreen.nextSelectable[3] = this.eyesBlue;
            this.eyesGreen.nextSelectable[2] = this.modeButtons[1];
            this.eyesBlue.nextSelectable[0] = this.debugAllowBox;
            this.eyesBlue.nextSelectable[1] = this.eyesGreen;
            this.eyesBlue.nextSelectable[3] = this.debugCheckBox;
            this.eyesBlue.nextSelectable[2] = this.modeButtons[1];
            this.debugCheckBox.nextSelectable[0] = this.debugAllowBox;
            this.debugCheckBox.nextSelectable[1] = this.eyesBlue;
            this.debugCheckBox.nextSelectable[3] = this.slugcatButtons[0];
            this.debugCheckBox.nextSelectable[2] = this.backButton;
            this.slugcatButtons[0].nextSelectable[0] = this.debugAllowBox;
            this.slugcatButtons[0].nextSelectable[1] = this.debugCheckBox;
            this.slugcatButtons[0].nextSelectable[3] = this.slugcatButtons[1];
            this.slugcatButtons[0].nextSelectable[2] = this.backButton;
            this.slugcatButtons[1].nextSelectable[0] = this.debugAllowBox;
            this.slugcatButtons[1].nextSelectable[1] = this.slugcatButtons[0];
            this.slugcatButtons[1].nextSelectable[3] = this.slugcatButtons[2];
            this.slugcatButtons[1].nextSelectable[2] = this.backButton;
            this.slugcatButtons[2].nextSelectable[0] = this.debugAllowBox;
            this.slugcatButtons[2].nextSelectable[1] = this.slugcatButtons[1];
            this.slugcatButtons[2].nextSelectable[3] = this.startLobbyButton;
            this.slugcatButtons[2].nextSelectable[2] = this.backButton;
            this.backButton.nextSelectable[0] = this.startLobbyButton;
            this.backButton.nextSelectable[1] = this.modeButtons[1];
            this.backButton.nextSelectable[3] = this.modeButtons[1];
            this.backButton.nextSelectable[2] = this.startLobbyButton;
            this.startLobbyButton.nextSelectable[0] = this.backButton;
            this.startLobbyButton.nextSelectable[1] = this.debugAllowBox;
            this.startLobbyButton.nextSelectable[3] = this.privacyButtons[2];
            this.startLobbyButton.nextSelectable[2] = this.backButton;
            this.modeButtons[1].nextSelectable[0] = this.bodyRed;
            this.modeButtons[1].nextSelectable[1] = this.backButton;
            this.modeButtons[1].nextSelectable[3] = this.backButton;
            this.modeButtons[1].nextSelectable[2] = this.modeButtons[0];
            this.modeButtons[0].nextSelectable[0] = this.modeButtons[1];
            this.modeButtons[0].nextSelectable[1] = this.backButton;
            this.modeButtons[0].nextSelectable[3] = this.backButton;
            this.modeButtons[0].nextSelectable[2] = this.refreshButton;
            this.refreshButton.nextSelectable[0] = this.modeButtons[0];
            this.refreshButton.nextSelectable[1] = this.joinButtons[this.joinButtons.Length - 1];
            this.refreshButton.nextSelectable[3] = this.joinButtons[0];
            this.refreshButton.nextSelectable[2] = this.refreshButton;
            this.privacyButtons[0].nextSelectable[0] = this.joinButtons[0];
            this.privacyButtons[0].nextSelectable[1] = this.startLobbyButton;
            this.privacyButtons[0].nextSelectable[3] = this.maxPlayersSlider;
            this.privacyButtons[0].nextSelectable[2] = this.privacyButtons[1];
            this.privacyButtons[1].nextSelectable[0] = this.privacyButtons[0];
            this.privacyButtons[1].nextSelectable[1] = this.startLobbyButton;
            this.privacyButtons[1].nextSelectable[3] = this.maxPlayersSlider;
            this.privacyButtons[1].nextSelectable[2] = this.privacyButtons[2];
            this.privacyButtons[2].nextSelectable[0] = this.privacyButtons[1];
            this.privacyButtons[2].nextSelectable[1] = this.startLobbyButton;
            this.privacyButtons[2].nextSelectable[3] = this.maxPlayersSlider;
            this.privacyButtons[2].nextSelectable[2] = this.bodyRed;
            this.maxPlayersSlider.nextSelectable[1] = this.privacyButtons[1];
            this.maxPlayersSlider.nextSelectable[3] = this.spearsHitBox;
            this.spearsHitBox.nextSelectable[0] = this.joinButtons[4];
            this.spearsHitBox.nextSelectable[1] = this.maxPlayersSlider;
            this.spearsHitBox.nextSelectable[3] = this.spearsHitBox;
            this.spearsHitBox.nextSelectable[2] = this.bodyGreen;
            this.debugAllowBox.nextSelectable[0] = this.joinButtons[5];
            this.debugAllowBox.nextSelectable[1] = this.spearsHitBox;
            this.debugAllowBox.nextSelectable[3] = this.startLobbyButton;
            this.debugAllowBox.nextSelectable[2] = this.eyesRed;

            //Some Nice Music :)
            if (manager.musicPlayer != null)
            {
                manager.musicPlayer.MenuRequestsSong("NA_05 - Sparkles", 1.2f, 10f);
            }

            //Fix Label pixelperfect
            foreach (MenuObject o in this.pages[0].subObjects)
            { if (o is MenuLabel l) { l.pos += new Vector2(0.01f, 0.01f); } }
        }

        public override void Init()
        {
            base.Init();
        }

        public override void SliderSetValue(Slider slider, float f)
        {
            if (slider.ID == (Slider.SliderID)(-1))
            {
                if (slider == this.maxPlayersSlider)
                {
                    int newmax = (int)(f * 100f);
                    if (newmax < 3) { newmax = 2; }
                    if (maxPlayersLabel != null) { maxPlayersLabel.text = $"Max Players: {newmax}"; }
                    MonklandSteamManager.lobbyMax = newmax;
                    return;
                }
                f = Mathf.Clamp(f, 0.004f, 1.000f);
                if (slider == this.bodyRed) { MonklandSteamManager.bodyColor = new Color(f, MonklandSteamManager.bodyColor.g, MonklandSteamManager.bodyColor.b); }
                if (slider == this.bodyGreen) { MonklandSteamManager.bodyColor = new Color(MonklandSteamManager.bodyColor.r, f, MonklandSteamManager.bodyColor.b); }
                if (slider == this.bodyBlue) { MonklandSteamManager.bodyColor = new Color(MonklandSteamManager.bodyColor.r, MonklandSteamManager.bodyColor.g, f); }
                if (slider == this.eyesRed) { MonklandSteamManager.eyeColor = new Color(f, MonklandSteamManager.eyeColor.g, MonklandSteamManager.eyeColor.b); }
                if (slider == this.eyesGreen) { MonklandSteamManager.eyeColor = new Color(MonklandSteamManager.eyeColor.r, f, MonklandSteamManager.eyeColor.b); }
                if (slider == this.eyesBlue) { MonklandSteamManager.eyeColor = new Color(MonklandSteamManager.eyeColor.r, MonklandSteamManager.eyeColor.g, f); }
                return;
            }
            base.SliderSetValue(slider, f);
        }

        public override float ValueOfSlider(Slider slider)
        {
            if (slider.ID == (Slider.SliderID)(-1))
            {
                if (slider == this.bodyRed) { return MonklandSteamManager.bodyColor.r; }
                if (slider == this.bodyGreen) { return MonklandSteamManager.bodyColor.g; }
                if (slider == this.bodyBlue) { return MonklandSteamManager.bodyColor.b; }
                if (slider == this.eyesRed) { return MonklandSteamManager.eyeColor.r; }
                if (slider == this.eyesGreen) { return MonklandSteamManager.eyeColor.g; }
                if (slider == this.eyesBlue) { return MonklandSteamManager.eyeColor.b; }
                if (slider == this.maxPlayersSlider) { return (float)MonklandSteamManager.lobbyMax / 100f; }
            }
            return base.ValueOfSlider(slider);
        }

        public int GetCurrentlySelectedOfSeries(string series)
        {
            switch (series)
            {
                case "Mode":
                    if (hosting) { return 0; }
                    else { return 1; }
                case "Slugcat":
                    return this.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;

                case "LobbyType":
                    return MonklandSteamManager.lobbyType;
            }
            return -1;
        }

        public void SetCurrentlySelectedOfSeries(string series, int to)
        {
            switch (series)
            {
                case "Mode":
                    hosting = (to == 0);
                    break;

                case "Slugcat":
                    this.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = to;
                    break;

                case "LobbyType":
                    MonklandSteamManager.lobbyType = to;
                    break;
            }
        }

        public bool GetChecked(CheckBox box)
        {
            string idstring = box.IDString;
            switch (idstring)
            {
                case "DEBUG":
                    return MonklandSteamManager.DEBUG;

                case "SPEARSHIT":
                    return MonklandSteamManager.spearsHit;

                case "ALLOWDEBUG":
                    return MonklandSteamManager.debugAllowed;
            }
            return false;
        }

        public void SetChecked(CheckBox box, bool c)
        {
            string idstring = box.IDString;
            switch (idstring)
            {
                case "DEBUG":
                    MonklandSteamManager.DEBUG = c;
                    break;

                case "SPEARSHIT":
                    MonklandSteamManager.spearsHit = c;
                    break;

                case "ALLOWDEBUG":
                    MonklandSteamManager.debugAllowed = c;
                    break;
            }
        }

        public void SearchFinished(int numOfLobbies)
        {
            if (numOfLobbies > this.joinButtons.Length)
            { numOfLobbies = this.joinButtons.Length; }
            this.lobbynum = numOfLobbies;
            for (int i = 0; i < this.joinButtons.Length; i++)
            {
                if (i < lobbynum)
                {
                    this.lobbynames[i].text = SteamFriends.GetFriendPersonaName(MonklandSteamManager.lobbies[i].owner);
                    this.lobbyVersions[i].text = MonklandSteamManager.lobbies[i].version;
                    if (MonklandSteamManager.lobbies[i].version != Monkland.VERSION)
                    { this.lobbyVersions[i].color = new Color(1f, 0f, 0f); }
                    else
                    { this.lobbyVersions[i].color = new Color(0f, 1f, 0f); }
                    this.lobbyPlayerCounts[i].text = "" + MonklandSteamManager.lobbies[i].memberNum;
                    this.lobbyPlayerMaxs[i].text = "" + MonklandSteamManager.lobbies[i].memberLimit;
                }
                else
                {
                    this.lobbynames[i].text = "-";
                    this.lobbyVersions[i].text = "-";
                    this.lobbyVersions[i].color = new Color(1f, 1f, 1f);
                    this.lobbyPlayerCounts[i].text = "-";
                    this.lobbyPlayerMaxs[i].text = "-";
                }
            }
        }

        public override void Update()
        {
            this.lastBlackFade = this.blackFade;
            float num = 0f;
            if (this.blackFade < num)
            { this.blackFade = Custom.LerpAndTick(this.blackFade, num, 0.05f, 0.06666667f); }
            else
            { this.blackFade = Custom.LerpAndTick(this.blackFade, num, 0.05f, 0.125f); }
            slugcat.color = MonklandSteamManager.bodyColor;
            //Debug.Log("Color: " + MonklandSteamManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkGameManager.playerID)].r + ", " + MonklandSteamManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkGameManager.playerID)].g + ", " + MonklandSteamManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkGameManager.playerID)].b);
            eyes.color = MonklandSteamManager.eyeColor;
            this.startLobbyButton.buttonBehav.greyedOut = !hosting;
            this.privacyButtons[0].buttonBehav.greyedOut = !hosting;
            this.privacyButtons[1].buttonBehav.greyedOut = !hosting;
            this.privacyButtons[2].buttonBehav.greyedOut = !hosting;
            this.maxPlayersSlider.buttonBehav.greyedOut = !hosting;
            this.debugAllowBox.buttonBehav.greyedOut = !hosting;
            this.spearsHitBox.buttonBehav.greyedOut = !hosting;
            this.modeButtons[0].buttonBehav.greyedOut = MonklandSteamManager.joining;
            this.modeButtons[1].buttonBehav.greyedOut = MonklandSteamManager.joining;
            this.refreshButton.buttonBehav.greyedOut = hosting || MonklandSteamManager.joining || MonklandSteamManager.searching;
            for (int i = 0; i < joinButtons.Length; i++)
            { joinButtons[i].buttonBehav.greyedOut = hosting || MonklandSteamManager.joining || MonklandSteamManager.searching || i >= lobbynum; }
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
            this.darkSprite.RemoveFromContainer();
            this.eyes.RemoveFromContainer();
            this.slugcat.RemoveFromContainer();
            this.eyesLabel.RemoveFromContainer();
            this.bodyLabel.RemoveFromContainer();
            this.creatorLabel.RemoveFromContainer();
            this.maxPlayersLabel.RemoveFromContainer();
            this.settingsLabel.RemoveFromContainer();
            this.playersLabel.RemoveFromContainer();
            this.namesLabel.RemoveFromContainer();
            this.maxLabel.RemoveFromContainer();
            this.versionsLabel.RemoveFromContainer();
            for (int i = 0; i < lobbynames.Length; i++)
            {
                lobbynames[i].RemoveFromContainer();
                lobbyVersions[i].RemoveFromContainer();
                lobbyPlayerCounts[i].RemoveFromContainer();
                lobbyPlayerMaxs[i].RemoveFromContainer();
            }
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == "EXIT")
            {
                if (manager.musicPlayer != null)
                {
                    manager.musicPlayer.FadeOutAllSongs(5f);
                    this.manager.musicPlayer.MenuRequestsSong("RW_8 - Sundown", 1.4f, 2f);
                }
                manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                //MonklandSteamManager.instance.OnGameExit();
                //Steamworks.SteamMatchmaking.LeaveLobby( SteamManagement.MonklandSteamManager.lobbyID );
            }
            else if (message == "STARTLOBBY")
            {
                base.PlaySound(SoundID.MENU_Switch_Page_In);
                ProcessManagerHK.ImmediateSwitchCustom(this.manager, new SteamMultiplayerMenu(this.manager, true)); //opens lobby finder menu menu
            }
            else if (message.Contains("JOIN"))
            {
                int lobby = int.Parse(message.Substring(4));
                if (lobby < MonklandSteamManager.lobbies.Count && !MonklandSteamManager.joining && !MonklandSteamManager.searching && MonklandSteamManager.lobbies[lobby].version == Monkland.VERSION)
                { MonklandSteamManager.instance.JoinLobby(MonklandSteamManager.lobbies[lobby].ID); }
            }
            else if (message.Contains("REFRESH"))
            {
                if (!MonklandSteamManager.searching && !MonklandSteamManager.joining)
                { MonklandSteamManager.instance.FindLobbies(); }
            }
        }
    }
}
