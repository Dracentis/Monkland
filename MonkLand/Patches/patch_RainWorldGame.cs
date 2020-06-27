using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod;
using UnityEngine;
using Monkland.SteamManagement;
using Steamworks;
using Monkland.UI;
using Menu;

namespace Monkland.Patches
{
    [MonoMod.MonoModPatch("global::RainWorldGame")]
    class patch_RainWorldGame : RainWorldGame {

        public static RainWorldGame mainGame;

        [MonoModIgnore]
        public patch_RainWorldGame(ProcessManager manager) : base( manager ) {
        }

		[MonoModIgnore]
		private bool oDown;

		[MonoModIgnore]
		private bool hDown;

		[MonoModIgnore]
		private bool lastRestartButton;

		[MonoModIgnore]
		private bool lastPauseButton;

		[MonoModIgnore]
		private int updateAbstractRoom;

		[MonoModIgnore]
		private int updateShortCut;

		[MonoModIgnore]
        public extern void OriginalConstructor(ProcessManager manager);
        [MonoModConstructor, MonoModOriginalName( "OriginalConstructor" )]
        public void ctor_RainWorldGame(ProcessManager manager) {
            OriginalConstructor( manager );

            if (MonklandSteamManager.isInGame)
            {
                this.devToolsActive = MonklandSteamManager.DEBUG;
                MonklandSteamManager.monklandUI = new UI.MonklandUI(Futile.stage);
            }
            if( mainGame == null )
                    mainGame = this;

        }

        public extern void orig_Update();
        public void Update() {
            if( mainGame == null )
                mainGame = this;

			if (!lastPauseButton)
				lastPauseButton = MonklandSteamManager.isInGame;

            orig_Update();

			//New Code:
			try {
                if (MonklandSteamManager.isInGame)
                {
                    if (MonklandSteamManager.monklandUI != null)
                        MonklandSteamManager.monklandUI.Update(this);
                    MonklandSteamManager.WorldManager.TickCycle();
                }
            } catch( System.Exception e ) {
                Debug.LogError( e );
            }


        }

        [MonoModIgnore]
        public extern void OriginalShutdown();
        [MonoModOriginalName( "OriginalShutdown" )]
        public void ShutDownProcess() {
            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.monklandUI.ClearSprites();
                MonklandSteamManager.monklandUI = null;
                MonklandSteamManager.WorldManager.GameEnd();
                mainGame = null;
            }
            OriginalShutdown();
        }

        public EntityID GetNewID()
        {
            int newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            while ((newID >= -1 && newID <= 15000))
            {
                newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            return new EntityID(-1, newID);
        }

        public EntityID GetNewID(int spawner)
        {
            int newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            while ((newID >= -1 && newID <= 15000))
            {
                newID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            return new EntityID(spawner, newID);
        }
    }
}
