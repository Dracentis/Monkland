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
                MonklandSteamManager.monklandUI = new UI.MonklandUI( Futile.stage );
            /*
            AbstractRoom thisRoom = this.world.GetAbstractRoom( this.overWorld.FIRSTROOM );


            if( thisRoom != null ) {
                int num = thisRoom.index;

                foreach( AbstractCreature ac in session.Players ) {
                    ac.Destroy();
                    this.world.GetAbstractRoom( ac.Room.index ).RemoveEntity( ac );
                }

                foreach( AbstractPhysicalObject absPhys in thisRoom.entities ) {
                    patch_AbstractPhysicalObject patch = absPhys as patch_AbstractPhysicalObject;

                    if( !MonklandSteamManager.ObjectManager.IsNetObject( patch ) ) {
                        patch.ownerID = NetworkGameManager.playerID;
                        MonklandSteamManager.ObjectManager.SendObjectCreation( patch );
                    }
                }

                session.Players.Clear();
                */
                if( mainGame == null )
                    mainGame = this;
                /*
                ulong playerID = MonklandSteamManager.instance.allChannels[0].SendForcewaitPacket( 0, (CSteamID)NetworkGameManager.managerID );

                Debug.Log( "Player Count is " + session.Players.Count );

                if( this.IsStorySession ) {
                    patch_AbstractCreature playerCreature = new AbstractCreature( this.world, StaticWorld.GetCreatureTemplate( "Slugcat" ), null, new WorldCoordinate( num, 15, 25, -1 ), new EntityID( -1, (int)playerID ) ) as patch_AbstractCreature;
                    playerCreature.state = new PlayerState( playerCreature, 0, this.GetStorySession.saveState.saveStateNumber, false );
                    this.world.GetAbstractRoom( playerCreature.pos.room ).AddEntity( playerCreature );
                    if( this.session.Players.Count > 0 )
                        this.session.Players.Insert( 0, playerCreature );
                    else
                        this.session.Players.Add( playerCreature );

                    playerCreature.ownerIDNew = NetworkGameManager.playerID;
                    MonklandSteamManager.ObjectManager.SendObjectCreation( playerCreature );

                    MonklandUI.trackedPlayer = playerCreature;

                    this.cameras[0].followAbstractCreature = playerCreature;
                    this.roomRealizer.followCreature = playerCreature;
                }

            }*/

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
    }
}
