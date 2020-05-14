using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public extern void OriginalConstructor(ProcessManager manager);
        [MonoModConstructor, MonoModOriginalName( "OriginalConstructor" )]
        public void ctor_RainWorldGame(ProcessManager manager) {
            OriginalConstructor( manager );

            if (SteamManagement.MonklandSteamManager.isInGame)
                SteamManagement.MonklandSteamManager.monklandUI = new UI.MonklandUI( Futile.stage );
                if( mainGame == null )
                    mainGame = this;

        }

        public extern void orig_Update();
        public void Update() {
            if( mainGame == null )
                mainGame = this;

			if (!this.lastPauseButton)
                this.lastPauseButton = MonklandSteamManager.isInGame;

            orig_Update();

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
        public extern void orig_ShutDownProcess();
        public void ShutDownProcess() {
            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.monklandUI.ClearSprites();
                MonklandSteamManager.monklandUI = null;
                MonklandSteamManager.WorldManager.GameEnd();
                mainGame = null;
            }
            orig_ShutDownProcess();
        }
    }
}
