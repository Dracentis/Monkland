using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Steamworks;
using Monkland.Patches;
using Monkland.SteamManagement;

namespace Monkland.UI {
    public class MonklandUI {

        public static FStage worldStage;
        public static FContainer uiContainer;

        public static bool shouldDisplayMessages = false;

        private static FLabel statusLabel;

        private static List<QuickDisplayMessage> displayMessages = new List<QuickDisplayMessage>();
        private static List<FLabel> uiLabels = new List<FLabel>();

        public static Room currentRoom;
        public static AbstractCreature trackedPlayer;

        public MonklandUI(FStage stage) {
            worldStage = stage;

            uiContainer = new FContainer();

            statusLabel = new FLabel( "font", "Monkland 0.0.3" );
            statusLabel.alignment = FLabelAlignment.Left;
            statusLabel.SetPosition( 50, Futile.screen.height - 50 );
            uiContainer.AddChild( statusLabel );

            for( int i = 0; i < 200; i++ ) {
                FLabel displayLabel = new FLabel( "font", "" );
                uiContainer.AddChild( displayLabel );
                uiLabels.Add( displayLabel );
            }

            stage.AddChild( uiContainer );
        }

        public void Update(RainWorldGame game) {

            if( Input.GetKeyDown( KeyCode.J ) )
                shouldDisplayMessages = !shouldDisplayMessages;

            FindPlayer( game );

            DisplayQuickMessages();
        }

        private void FindPlayer(RainWorldGame game) {
            if( game.Players.Count > 0 ) {
                trackedPlayer = game.Players[0];
                if( trackedPlayer != null ) {
                    currentRoom = trackedPlayer.Room.realizedRoom;
                }
            }
        }

        private void DisplayQuickMessages() {

            if( !shouldDisplayMessages ) {
                return;
            }

            for (int a = 0; a < displayMessages.Count; a++)
            {
                displayMessages[a].life -= Time.deltaTime;
                if (displayMessages[a].life <= 0)
                {
                    displayMessages.RemoveAt(a);
                    if (a > 0)
                        a--;
                }
            }

        }

        public static void UpdateStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.text = message;
                statusLabel.Redraw(false, false);
            }
        }

        public static void AddMessage(string messsage, float time = 3) {
            AddMessage( messsage, time, false, Vector2.zero, Color.white );
        }
        public static void AddMessage(string messsage, float time, bool isWorld, Vector2 worldPos) {
            AddMessage( messsage, time, isWorld, worldPos, Color.white );
        }
        public static void AddMessage(string messsage, float time, bool isWorld, Vector2 worldPos, Color color) {
            QuickDisplayMessage msg = new QuickDisplayMessage() {
                text = messsage,
                life = time,
                isWorld = isWorld,
                worldPos = worldPos,
                color = color,
                roomID = ( trackedPlayer == null ? 0 : trackedPlayer.Room.index )
            };
            displayMessages.Add( msg );
        }


        public void ClearSprites() {
            statusLabel.RemoveFromContainer();
            displayMessages.Clear();
            uiContainer.RemoveAllChildren();
            uiContainer.RemoveFromContainer();
            uiContainer = null;
        }

        public class QuickDisplayMessage {
            public string text;
            public float life;
            public Color color;
            public bool isWorld = false;
            public Vector2 worldPos;
            public int roomID;
        }

    }
}