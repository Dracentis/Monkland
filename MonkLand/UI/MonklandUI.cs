using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Steamworks;
using Monkland.Patches;
using Monkland.SteamManagement;

namespace Monkland.UI {
    public class MonklandUI {

        public static FStage worldStage;
        public static FContainer uiContainer;

        private static FLabel statusLabel;
        public const string VERSION = "0.2.6";
            
        private static List<QuickDisplayMessage> displayMessages = new List<QuickDisplayMessage>();
        private static List<FLabel> uiLabels = new List<FLabel>();

        public static Room currentRoom;
        public static AbstractCreature trackedPlayer;

        public MonklandUI(FStage stage) {
            worldStage = stage;

            uiContainer = new FContainer();

            string text = "Monkland " + VERSION;
            if (!MonklandSteamManager.DEBUG)
            {
                text = "";
            }

            statusLabel = new FLabel( "font", text);
            statusLabel.alignment = FLabelAlignment.Left;
            statusLabel.SetPosition( 50, Futile.screen.height - 50 );
            uiContainer.AddChild( statusLabel );

            for ( int i = 0; i < 200; i++ ) {
                FLabel displayLabel = new FLabel( "font", "" );
                displayLabel.alignment = FLabelAlignment.Left;
                uiContainer.AddChild( displayLabel );
                uiLabels.Add( displayLabel );
            }

            displayMessages.Clear();
            stage.AddChild( uiContainer );
        }

        public void Update(RainWorldGame game) {
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
            bool redraw = false;
            for (int i = 0; i < displayMessages.Count; i++)
            {
                displayMessages[i].life -= Time.deltaTime;
                if (displayMessages[i].life <= 0)
                {
                    displayMessages.RemoveAt(i);
                    redraw = true;
                    if (i > 0)
                        i--;
                }
            }

            if (!MonklandSteamManager.DEBUG || !MonklandSteamManager.isInGame)
            {
                return;
            }

            if (redraw)
            {
                for (int j = 0; j < 200; j++)
                {
                    if (uiLabels[j].text != "")
                    {
                        uiLabels[j].text = "";
                        uiLabels[j].Redraw(false, false);
                    }
                }
            }

            int k = 1;
            for (int i = 0; i < displayMessages.Count && i < 200; i++)
            {
                if (!displayMessages[i].isWorld)
                {
                    k++;
                    if (uiLabels[i].text != displayMessages[i].text || uiLabels[i].GetPosition().y != (Futile.screen.height - 50 - (20 * k)))
                    {
                        uiLabels[i].text = displayMessages[i].text;
                        uiLabels[i].color = displayMessages[i].color;
                        uiLabels[i].SetPosition(50, Futile.screen.height - 50 - (20 * k));
                        uiLabels[i].Redraw(false, false);
                    }
                }
                else
                {
                    if (currentRoom != null && displayMessages[i].roomID == currentRoom.abstractRoom.index)
                    {
                        if (uiLabels[i].text != displayMessages[i].text || uiLabels[i].GetPosition() != displayMessages[i].worldPos)
                        {
                            uiLabels[i].text = displayMessages[i].text;
                            uiLabels[i].color = displayMessages[i].color;
                            uiLabels[i].SetPosition(displayMessages[i].worldPos.x, displayMessages[i].worldPos.y);
                            uiLabels[i].Redraw(false, false);
                        }
                    }
                    else if (uiLabels[i].text != "")
                    {
                        uiLabels[i].text = "";
                        uiLabels[i].Redraw(false, false);
                    }
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

        public static void AddMessage(string message, float time = 3) {
            AddMessage( message, time, false, Vector2.zero, Color.white );
        }

        public static void AddMessage(string message, float time, bool isWorld, Vector2 worldPos) {
            AddMessage( message, time, isWorld, worldPos, Color.white );
        }

        public static void AddMessage(string message, float time, bool isWorld, Vector2 worldPos, Color color) {
            QuickDisplayMessage msg = new QuickDisplayMessage() {
                text = message,
                life = time,
                isWorld = isWorld,
                worldPos = worldPos,
                color = color,
                roomID = ( trackedPlayer == null ? 0 : trackedPlayer.Room.index )
            };
            displayMessages.Add( msg );
        }

        public static void UpdateMessage(string message, float time, Vector2 worldPos, int dist, int roomID, Color color)
        {
            for (int i = 0; i < displayMessages.Count; i++)
            {
                if (displayMessages[i].tracking == dist)
                {
                    displayMessages[i].worldPos = worldPos;
                    displayMessages[i].text = message;
                    displayMessages[i].life = time;
                    displayMessages[i].color = color;
                    displayMessages[i].roomID = roomID;
                    return;
                }
            }
            QuickDisplayMessage msg = new QuickDisplayMessage()
            {
                text = message,
                life = time,
                isWorld = true,
                worldPos = worldPos,
                color = color,
                roomID = roomID
            };
            msg.tracking = dist;
            displayMessages.Add(msg);
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
            public int tracking = -1;
            public int roomID;
        }

    }
}