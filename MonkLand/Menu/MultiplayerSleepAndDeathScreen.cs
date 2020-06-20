using System;
using System.Collections.Generic;
using HUD;
using RWCustom;
using UnityEngine;
using Menu;
using Monkland;
using Monkland.SteamManagement;
using System.IO;
using Monkland.Patches;
using Steamworks;

namespace Menu
{
	public class MultiplayerSleepAndDeathScreen : KarmaLadderScreen
	{

		private MultiplayerPlayerList playerList;

		public MultiplayerSleepAndDeathScreen(ProcessManager manager, ProcessManager.ProcessID ID) : base(manager, ID)
		{
			this.endGameSceneCounter = -1;
			this.starvedWarningCounter = -1;
			base.AddContinueButton(false);
			if (NetworkGameManager.isManager)
			{
				this.exitButton = new SimpleButton(this, this.pages[0], base.Translate("SHUTDOWN"), "EXIT", new Vector2(base.ContinueAndExitButtonsXPos - 320f, 15f), new Vector2(110f, 30f));
			}
			else
			{
				this.exitButton = new SimpleButton(this, this.pages[0], base.Translate("DISCONNECT"), "EXIT", new Vector2(base.ContinueAndExitButtonsXPos - 320f, 15f), new Vector2(110f, 30f));
			}
			MonklandSteamManager.GameManager.FinishCycle();
			if (!MonklandSteamManager.GameManager.readiedPlayers.Contains(SteamUser.GetSteamID().m_SteamID))
				MonklandSteamManager.GameManager.readiedPlayers.Add(SteamUser.GetSteamID().m_SteamID);
			this.pages[0].subObjects.Add(this.exitButton);

			//Player menu
			playerList = new MultiplayerPlayerList(this, this.pages[0], new Vector2(manager.rainWorld.options.ScreenSize.x - 250f, manager.rainWorld.options.ScreenSize.y - 450f), new Vector2(200, 400), new Vector2(180, 180));
			this.pages[0].subObjects.Add(this.playerList);

			this.mySoundLoopID = ((!this.IsSleepScreen) ? SoundID.MENU_Death_Screen_LOOP : SoundID.MENU_Sleep_Screen_LOOP);
			base.PlaySound((!this.IsSleepScreen) ? SoundID.MENU_Enter_Death_Screen : SoundID.MENU_Enter_Sleep_Screen);
		}

		public override bool ButtonsGreyedOut
		{
			get
			{
				return !NetworkGameManager.isManager || MonklandSteamManager.WorldManager.ingamePlayers.Count > 0 || gameStarting;
			}
		}

		public bool ExitButtonsGreyedOut
		{
			get
			{
				return gameStarting;
			}
		}

		public bool gameStarting = false;

		public bool AllowFoodMeterTick
		{
			get
			{
				return this.killsDisplay == null || this.killsDisplay.countedAndDone;
			}
		}

		public float StarveLabelAlpha(float timeStacker)
		{
			return Mathf.InverseLerp(40f, 60f, (float)this.starvedWarningCounter + timeStacker);
		}

		public float FoodMeterXPos(float down)
		{
			return Custom.LerpMap(this.manager.rainWorld.options.ScreenSize.x, 1024f, 1366f, this.manager.rainWorld.options.ScreenSize.x / 2f - 110f, 540f);
		}

		public bool IsSleepScreen
		{
			get
			{
				return this.ID == ProcessManager.ProcessID.SleepScreen;
			}
		}

		public bool IsDeathScreen
		{
			get
			{
				return this.ID == ProcessManager.ProcessID.DeathScreen;
			}
		}

		public bool IsStarveScreen
		{
			get
			{
				return this.ID == ProcessManager.ProcessID.StarveScreen;
			}
		}

		public bool IsAnyDeath
		{
			get
			{
				return this.IsDeathScreen || this.IsStarveScreen;
			}
		}

		protected override bool FreezeMenuFunctions
		{
			get
			{
				return (base.hud != null && base.hud.rainWorld != null && base.FreezeMenuFunctions);
			}
		}

		protected override void AddBkgIllustration()
		{
			if (this.IsSleepScreen)
			{
				this.scene = new InteractiveMenuScene(this, this.pages[0], MenuScene.SceneID.SleepScreen);
				this.pages[0].subObjects.Add(this.scene);
			}
			else if (this.IsDeathScreen)
			{
				this.scene = new InteractiveMenuScene(this, this.pages[0], MenuScene.SceneID.NewDeath);
				this.pages[0].subObjects.Add(this.scene);
			}
			else if (this.IsStarveScreen)
			{
				this.scene = new InteractiveMenuScene(this, this.pages[0], MenuScene.SceneID.StarveScreen);
				this.pages[0].subObjects.Add(this.scene);
			}
		}

		public override void Update()
		{
			if (MonklandSteamManager.isInGame)
			{
				MonklandSteamManager.WorldManager.TickCycle();
			}
			if (this.starvedWarningCounter >= 0)
			{
				this.starvedWarningCounter++;
			}
			if (this.continueButton != null)
			{
				this.continueButton.buttonBehav.greyedOut = this.ButtonsGreyedOut;
				this.continueButton.black = Mathf.Max(0f, this.continueButton.black - 0.025f);
			}
			base.Update();
			if (this.exitButton != null)
			{
				this.exitButton.buttonBehav.greyedOut = this.ExitButtonsGreyedOut;
				this.exitButton.black = Mathf.Max(0f, this.exitButton.black - 0.0125f);
			}
			if (this.passageButton != null)
			{
				this.passageButton.buttonBehav.greyedOut = true;
				this.passageButton.black = Mathf.Max(0f, this.passageButton.black - 0.0125f);
			}
			if (this.endGameSceneCounter >= 0)
			{
				this.endGameSceneCounter++;
				if (this.endGameSceneCounter > 140)
				{
					this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.CustomEndGameScreen);
				}
			}
			if (this.RevealMap)
			{
				this.fadeOutIllustration = Custom.LerpAndTick(this.fadeOutIllustration, 1f, 0.02f, 0.025f);
			}
			else
			{
				this.fadeOutIllustration = Custom.LerpAndTick(this.fadeOutIllustration, 0f, 0.02f, 0.025f);
			}
			if (this.IsSleepScreen)
			{
				this.scene.depthIllustrations[0].setAlpha = new float?(Mathf.Lerp(1f, 0.2f, this.fadeOutIllustration));
				this.scene.depthIllustrations[1].setAlpha = new float?(Mathf.Lerp(0.24f, 0.1f, this.fadeOutIllustration));
				this.scene.depthIllustrations[2].setAlpha = new float?(Mathf.Lerp(1f, 0.35f, this.fadeOutIllustration));
			}
			else if (this.IsStarveScreen)
			{
				this.scene.depthIllustrations[0].setAlpha = new float?(Mathf.Lerp(0.85f, 0.4f, this.fadeOutIllustration));
			}
			else if (this.IsDeathScreen)
			{
				this.scene.depthIllustrations[0].setAlpha = new float?(Mathf.Lerp(1f, 0.1f, this.fadeOutIllustration));
				this.scene.depthIllustrations[2].setAlpha = new float?(Mathf.Lerp(1f, 0.25f, this.fadeOutIllustration));
				this.scene.depthIllustrations[3].setAlpha = new float?(Mathf.Lerp(1f, 0.5f, this.fadeOutIllustration));
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			if (this.starvedLabel != null)
			{
				this.starvedLabel.label.color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Color.red, 0.5f - 0.5f * Mathf.Sin((timeStacker + (float)this.starvedWarningCounter) / 30f * 3.14159274f * 2f));
				this.starvedLabel.label.alpha = this.StarveLabelAlpha(timeStacker);
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
				MonklandSteamManager.instance.OnGameExit();
			}
			else if (message == "READYUP")
			{
				MonklandSteamManager.GameManager.SendColor(0);
				MonklandSteamManager.GameManager.SendColor(1);
				MonklandSteamManager.GameManager.SendColor(2);
				MonklandSteamManager.GameManager.SendColor(3);
				MonklandSteamManager.GameManager.SendColor(4);
				MonklandSteamManager.GameManager.SendColor(5);
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
		}

		public void AddPassageButton(bool buttonBlack)
		{
			return;
		}

		public override void GetDataFromGame(KarmaLadderScreen.SleepDeathScreenDataPackage package)
		{
			base.GetDataFromGame(package);
			this.food = package.food;
			this.playerRoom = package.playerRoom;
			this.playerPos = package.playerPos;
			this.mapData = package.mapData;
			this.startMalnourished = package.startMalnourished;
			this.goalMalnourished = package.goalMalnourished;
			(( Monkland.Patches.Menus.patch_HUD )base.hud).InitSleepHud(this, this.mapData, package.characterStats);
			if (this.IsAnyDeath)
			{
				if (this.manager.rainWorld.progression.miscProgressionData.watchedDeathScreens < 2)
				{
					this.forceWatchAnimation = true;
				}
				this.manager.rainWorld.progression.miscProgressionData.watchedDeathScreens++;
				if (package.karmaReinforced)
				{
					if (this.manager.rainWorld.progression.miscProgressionData.watchedDeathScreensWithFlower < 2)
					{
						this.forceWatchAnimation = true;
					}
					this.manager.rainWorld.progression.miscProgressionData.watchedDeathScreensWithFlower++;
				}
				this.showFlower = (package.karmaReinforced || (package.saveState != null && package.saveState.saveStateNumber == 1));
			}
			else if (this.IsSleepScreen)
			{
				if (this.manager.rainWorld.progression.miscProgressionData.watchedSleepScreens < 2)
				{
					this.forceWatchAnimation = true;
				}
				this.manager.rainWorld.progression.miscProgressionData.watchedSleepScreens++;
				if (package.characterStats.name == SlugcatStats.Name.Red && package.sessionRecord != null && package.sessionRecord.kills.Count > 0)
				{
					this.killsDisplay = new SleepScreenKills(this, this.pages[0], new Vector2(base.LeftHandButtonsPosXAdd, 728f), package.sessionRecord.kills);
					this.pages[0].subObjects.Add(this.killsDisplay);
					this.killsDisplay.started = true;
				}
				if (this.goalMalnourished)
				{
					this.starvedLabel = new MenuLabel(this, this.pages[0], base.Translate("You are starving. Your game has not been saved."), new Vector2(0f, 24f), new Vector2(1366f, 20f), true);
					this.pages[0].subObjects.Add(this.starvedLabel);
					if (this.manager.rainWorld.progression.miscProgressionData.watchedMalnourishScreens < 6)
					{
						this.forceWatchAnimation = true;
					}
					this.manager.rainWorld.progression.miscProgressionData.watchedMalnourishScreens++;
				}
				if (this.startMalnourished)
				{
					base.hud.foodMeter.NewShowCount(base.hud.foodMeter.maxFood);
					if (this.manager.rainWorld.progression.miscProgressionData.watchedMalnourishScreens < 4)
					{
						this.forceWatchAnimation = true;
					}
					this.manager.rainWorld.progression.miscProgressionData.watchedMalnourishScreens++;
				}
				if (this.goalMalnourished || this.startMalnourished)
				{
					base.hud.foodMeter.MoveSurvivalLimit((float)base.hud.foodMeter.showCount, false);
					base.hud.foodMeter.eatCircles = base.hud.foodMeter.maxFood;
				}
			}
			if (this.startMalnourished)
			{
				base.hud.foodMeter.MoveSurvivalLimit((float)base.hud.foodMeter.maxFood, false);
			}
			if (this.IsSleepScreen || this.IsDeathScreen || this.IsStarveScreen)
			{
				if (package.characterStats.name != SlugcatStats.Name.Red)
				{
					this.endgameTokens = new EndgameTokens(this, this.pages[0], new Vector2(base.LeftHandButtonsPosXAdd + 140f, 15f), this.container, this.karmaLadder);
					this.pages[0].subObjects.Add(this.endgameTokens);
				}
				if (this.IsSleepScreen && package.saveState != null)
				{
					for (int i = 0; i < this.karmaLadder.endGameMeters.Count; i++)
					{
						if (!package.saveState.deathPersistentSaveData.endGameMetersEverShown[(int)this.karmaLadder.endGameMeters[i].tracker.ID])
						{
							this.forceWatchAnimation = true;
							package.saveState.deathPersistentSaveData.endGameMetersEverShown[(int)this.karmaLadder.endGameMeters[i].tracker.ID] = true;
							this.manager.rainWorld.progression.SaveDeathPersistentDataOfCurrentState(false, false);
							break;
						}
					}
				}
			}
			if (this.manager.rainWorld.setup.devToolsActive)
			{
				this.forceWatchAnimation = false;
			}
		}

		public override bool RevealMap
		{
			get
			{
				return base.hud != null && this.endGameSceneCounter < 0;
			}
		}

		public override int CurrentFood
		{
			get
			{
				return this.food;
			}
		}

		public override Vector2 MapOwnerInRoomPosition
		{
			get
			{
				return this.playerPos;
			}
		}

		public override int MapOwnerRoom
		{
			get
			{
				return this.playerRoom;
			}
		}

		public override void ShutDownProcess()
		{
			base.ShutDownProcess();
			playerList.ClearList();
		}

		public override void FoodCountDownDone()
		{
			Debug.Log("Karma ladder MOVE!");
			if (this.IsSleepScreen)
			{
				this.karmaLadder.GoToKarma(this.karma.x + 1, true);
			}
			else if (this.IsAnyDeath)
			{
				this.karmaLadder.GoToKarma(this.karma.x - 1, true);
				if (this.showFlower && this.scene != null && (this.scene as InteractiveMenuScene).timer < 0)
				{
					(this.scene as InteractiveMenuScene).timer = 0;
				}
			}
			if (this.starvedLabel != null)
			{
				this.starvedWarningCounter = 0;
			}
		}

		public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
		{
			if (nextProcess.ID == ProcessManager.ProcessID.CustomEndGameScreen)
			{
				(nextProcess as CustomEndGameScreen).GetDataFromSleepScreen(this.proceedWithEndgameID.Value);
			}
			if (this.IsDeathScreen && nextProcess is RainWorldGame && (nextProcess as RainWorldGame).world != null && (nextProcess as RainWorldGame).world.rainCycle != null)
			{
				(nextProcess as RainWorldGame).world.rainCycle.timer = 340;
			}
		}

		public SimpleButton exitButton;

		public SimpleButton passageButton;

		public EndgameTokens endgameTokens;

		public bool startMalnourished;

		public bool goalMalnourished;

		private int food;

		private int playerRoom;

		private Vector2 playerPos;

		private Map.MapData mapData;

		public int endGameSceneCounter;

		public WinState.EndgameID? proceedWithEndgameID;

		private bool forceWatchAnimation;

		private bool showFlower;

		public float fadeOutIllustration;

		private SleepScreenKills killsDisplay;

		public MenuLabel starvedLabel;

		public int starvedWarningCounter;
	}
}
