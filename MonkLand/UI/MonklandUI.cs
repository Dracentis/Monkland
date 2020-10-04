using Monkland.Hooks.Entities;
using Monkland.SteamManagement;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monkland.UI
{
    public class MonklandUI
    {
        public static FStage worldStage;
        public static FContainer uiContainer;

        private static FLabel statusLabel;
        public const string VERSION = "0.3.1";

        private static List<QuickDisplayMessage> displayMessages = new List<QuickDisplayMessage>();
        private static List<FLabel> uiLabels = new List<FLabel>();

        public static Room currentRoom;
        public static AbstractCreature trackedPlayer;
        public static Vector2 cpos = new Vector2(0f, 0f);

        public MonklandUI(FStage stage)
        {
            displayMessages = new List<QuickDisplayMessage>();
            uiLabels = new List<FLabel>();
            worldStage = stage;

            uiContainer = new FContainer();

            string text = "Monkland " + VERSION;
            if (!MonklandSteamManager.DEBUG)
            {
                text = "";
            }

            statusLabel = new FLabel("font", text);
            statusLabel.alignment = FLabelAlignment.Left;
            statusLabel.SetPosition(50.01f, Futile.screen.height - 49.99f);
            uiContainer.AddChild(statusLabel);

            for (int i = 0; i < 200; i++)
            {
                FLabel displayLabel = new FLabel("font", "");
                displayLabel.alignment = FLabelAlignment.Left;
                uiContainer.AddChild(displayLabel);
                uiLabels.Add(displayLabel);
            }

            displayMessages.Clear();
            stage.AddChild(uiContainer);
        }

        public static Vector2 camPos()
        {
            if (trackedPlayer != null && currentRoom != null && trackedPlayer.realizedCreature != null && trackedPlayer.realizedCreature.DangerPos != null && currentRoom.cameraPositions != null && currentRoom.CameraViewingPoint(trackedPlayer.realizedCreature.DangerPos) >= 0 && currentRoom.CameraViewingPoint(trackedPlayer.realizedCreature.DangerPos) < currentRoom.cameraPositions.Length)
            {
                cpos = currentRoom.cameraPositions[currentRoom.CameraViewingPoint(trackedPlayer.realizedCreature.DangerPos)];
            }
            return cpos;
        }

        public static string BuildDeathMessage(CSteamID deadPlayerID, Creature.DamageType damageType, CreatureTemplate.Type killerType, ulong killerID)
        {
            string deadPlayerName = SteamFriends.GetFriendPersonaName(deadPlayerID);
            string killerName = string.Empty;

            if (killerType == CreatureTemplate.Type.Slugcat)
            {
                killerName = SteamFriends.GetFriendPersonaName((CSteamID)killerID);
                if (killerName.Equals(string.Empty))
                {
                    killerName = "Player";
                }
            }
            // TO DO -- When creatures are synced  --
            else
            {
                switch(killerType)
                {
                    //case CreatureTemplate.Type.
                }
            }
            string message = string.Empty;

            switch (damageType)
            {
                case Creature.DamageType.Blunt:
                    message = $"{deadPlayerName} was killed using blunt force by {killerName}";
                    break;
                case Creature.DamageType.Stab:
                    message = $"{deadPlayerName} was stabbed to death by {killerName}";
                    break;
                case Creature.DamageType.Bite:
                    message = $"{deadPlayerName} was bitten to death by {killerName}";
                    break;
                case Creature.DamageType.Water:
                    message = $"{deadPlayerName} drowned.";
                    break;
                case Creature.DamageType.Explosion:
                    message = $"{deadPlayerName} was blown up by {killerName}";
                    break;
                case Creature.DamageType.Electric:
                    break;
            }

            // Generic death message
            if(message.Equals(string.Empty))
            {
                message = $"{deadPlayerName} was killed by {killerName}";
            }

            return message;
        }

        public void Update(RainWorldGame game)
        {
            FindPlayer(game);
            DisplayQuickMessages();
            DisplayPlayerNames(game);
        }

        private void FindPlayer(RainWorldGame game)
        {
            if (game.Players.Count > 0)
            {
                trackedPlayer = game.Players[0];
                if (trackedPlayer != null)
                {
                    currentRoom = trackedPlayer.Room.realizedRoom;
                }
            }
        }

        private void DisplayPlayerNames(RainWorldGame game)
        {
            if (trackedPlayer != null && currentRoom != null)
            {
                foreach (AbstractCreature cr in trackedPlayer.Room.creatures)
                {
                    try
                    {
                        // Player in the same room
                        if (cr.realizedCreature is Player p)
                        {
                            AbstractPhysicalObject player = cr.realizedCreature.abstractPhysicalObject;
                            string playerName = SteamFriends.GetFriendPersonaName((CSteamID)AbstractPhysicalObjectHK.GetField(player).owner);
                            Color color = MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(AbstractPhysicalObjectHK.GetField(player).owner)];
                            (currentRoom.game.cameras[0].hud.parts.Find(x => x is MultiplayerHUD) as MultiplayerHUD).AddLabel(player, playerName, color);
                        }
                    }
                    catch (Exception e)
                    {
                        // Throw exception
                        //Debug.LogException(e);
                        Debug.Log($"Error when trying to add label for Player"+e);

                    }
                }
            }
        }

        private void DisplayQuickMessages()
        {
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

            if (/*!MonklandSteamManager.DEBUG ||*/ !MonklandSteamManager.isInGame)
            {
                return;
            }

            if (redraw)
            {
                for (int j = 0; j < 200; j++)
                {
                    if (!string.IsNullOrEmpty(uiLabels[j].text))
                    {
                        uiLabels[j].text = "";
                        //uiLabels[j].Redraw(false, false);
                    }
                }
            }

            int k = 1;
            for (int i = 0; i < displayMessages.Count && i < 200; i++)
            {
                if (!displayMessages[i].isWorld)
                {
                    k++;
                    if (uiLabels[i].text != displayMessages[i].text || Mathf.Abs(uiLabels[i].GetPosition().y - Futile.screen.height - 49.99f - (20 * k)) > float.Epsilon)
                    {
                        uiLabels[i].text = displayMessages[i].text;
                        uiLabels[i].color = displayMessages[i].color;
                        uiLabels[i].SetPosition(50.01f, Futile.screen.height - 49.99f - (20 * k));
                        //uiLabels[i].Redraw(false, false);
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
                            uiLabels[i].SetPosition(displayMessages[i].worldPos.x + 0.01f, displayMessages[i].worldPos.y + 0.01f);
                            //uiLabels[i].Redraw(false, false);
                        }
                    }
                    else if (!string.IsNullOrEmpty(uiLabels[i].text))
                    { uiLabels[i].text = ""; }
                }
            }
        }

        public static void UpdateStatus(string message)
        {
            if (statusLabel != null)
            { 
                statusLabel.text = message; 
            }
        }

        public static void AddMessage(string message, float time = 3)
        {
            AddMessage(message, time, false, Vector2.zero, Color.white);
        }

        public static void AddMessage(string message, float time, bool isWorld, Vector2 worldPos)
        {
            AddMessage(message, time, isWorld, worldPos, Color.white);
        }

        public static void AddMessage(string message, float time, bool isWorld, Vector2 worldPos, Color color)
        {
            QuickDisplayMessage msg = new QuickDisplayMessage()
            {
                text = message,
                life = time,
                isWorld = isWorld,
                worldPos = worldPos,
                color = color,
                roomID = (trackedPlayer == null ? 0 : trackedPlayer.Room.index)
            };
            displayMessages.Add(msg);
        }

        public static void UpdateMessage(string message, float time, Vector2 worldPos, int dist, int roomID, Color color)
        {
            for (int i = 0; i < displayMessages.Count; i++)
            {
                if (displayMessages[i].tracking == dist)
                {
                    displayMessages[i].worldPos = worldPos - camPos();
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
                worldPos = worldPos - camPos(),
                color = color,
                roomID = roomID
            };
            msg.tracking = dist;
            displayMessages.Add(msg);
        }

#pragma warning disable CA1822
#pragma warning disable CA1034

        public void ClearSprites()
        {
            statusLabel.RemoveFromContainer();
            displayMessages.Clear();
            uiContainer.RemoveAllChildren();
            uiContainer.RemoveFromContainer();
            uiLabels.Clear();
            displayMessages.Clear();
            uiContainer = null;
        }

        public class QuickDisplayMessage
        {
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