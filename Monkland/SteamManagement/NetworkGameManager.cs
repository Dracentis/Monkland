using Monkland.Hooks;
using Monkland.Menus;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkGameManager : NetworkManager
    {
        public static ulong playerID;

        public static ulong managerID;
        public static bool isManager { get { return playerID == managerID; } }


        public byte GameyHandler = 0;

        /*
         * color
           hostShelter
           isReady
           isInGame
	        isAlive
	        isSpectating
         */

        public HashSet<ulong> readiedPlayers = new HashSet<ulong>(); // Set of players who are ready for the next cycle
        public HashSet<ulong> inGamePlayers = new HashSet<ulong>(); // Set of players who are still in-game
        public HashSet<ulong> alivePlayers = new HashSet<ulong>(); // Set of players who are alive and in-game
        public List<Color> playerColors = new List<Color>(); // List of player body colors
        public List<Color> playerEyeColors = new List<Color>(); // List of player eye colors

        public int joinDelay = -1;
        public int startDelay = -1;

        public String hostShelter = "SU_S01";
        public bool isReady = false;
        public bool isInGame = false;
        public bool isAlive = false;

        public enum MessageType
        {
            PlayerReadyUp,
            SendToGame,
            GameStarted,
            GameEnded,
            Death,
            BodyColorR,
            BodyColorG,
            BodyColorB,
            EyeColorR,
            EyeColorG,
            EyeColorB,
        }

        public override void Update()
        {
            if (!MonklandSteamManager.isInLobby)
            {
                joinDelay = -1;
                startDelay = -1;
            }
            if (joinDelay > 0)
            {
                joinDelay -= 1;
            }
            else if (joinDelay == 0)
            {
                SendColor(NetworkGameManager.MessageType.BodyColorR);
                SendColor(NetworkGameManager.MessageType.BodyColorG);
                SendColor(NetworkGameManager.MessageType.BodyColorB);
                SendColor(NetworkGameManager.MessageType.EyeColorR);
                SendColor(NetworkGameManager.MessageType.EyeColorG);
                SendColor(NetworkGameManager.MessageType.EyeColorB);
                SendReady();
                joinDelay = -1;
            }

            if (startDelay > 0)
            {
                startDelay -= 1;
            }
            else if (startDelay == 0)
            {
                SendPlayersToGame();
                startDelay = -1;
            }
        }

        public void QueueStart()
        {
            if (!isManager)
            {
                return;
            }

            startDelay = 100;
        }

        private void ReadyChat(CSteamID sentPlayer)
        {
            MultiplayerChat.AddChat(string.Format("User {0} is now: " + (readiedPlayers.Contains(sentPlayer.m_SteamID) ? "Ready!" : "Not Ready."), SteamFriends.GetFriendPersonaName(sentPlayer)));
        }

        public override void Reset()
        {
            playerID = SteamUser.GetSteamID().m_SteamID;
            managerID = 0;
            readiedPlayers.Clear();
            playerColors.Clear();
            playerEyeColors.Clear();
            inGamePlayers.Clear();
            alivePlayers.Clear();
            isReady = false;
            isInGame = false;
            isAlive = false;
        }

        public override void PlayerJoined(ulong steamID)
        {
            if (steamID == playerID)
            {
                // Use last colors
                playerColors.Add(MonklandSteamManager.bodyColor);
                playerEyeColors.Add(MonklandSteamManager.eyeColor);
            }
            else
            {
                // Use default colors
                playerColors.Add(new Color(1f, 1f, 1f));
                playerEyeColors.Add(new Color(0.004f, 0.004f, 0.004f));
            }
            joinDelay = 100;
        }

        public override void PlayerLeft(ulong steamID)
        {
            playerColors.RemoveAt(MonklandSteamManager.connectedPlayers.IndexOf(steamID));
            playerEyeColors.RemoveAt(MonklandSteamManager.connectedPlayers.IndexOf(steamID));
            readiedPlayers.Remove(steamID);
            inGamePlayers.Remove(steamID);
            alivePlayers.Remove(steamID);
        }

        #region Packet Handler

        public override void RegisterHandlers()
        {
            GameyHandler = MonklandSteamManager.instance.RegisterHandler(GAME_CHANNEL, HandlePackets);
        }

        public void HandlePackets(BinaryReader br, CSteamID sentPlayer)
        {
            byte messageType = br.ReadByte();
            switch ((MessageType)messageType)
            {
                // ReadyUP player
                case MessageType.PlayerReadyUp:
                    ReadReadyUpPacket(br, sentPlayer);
                    return;
                // Send players to game
                case MessageType.SendToGame:
                    isReady = false;
                    readiedPlayers.Clear();
                    RainWorldHK.rainWorld.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                    return;
                // Color body R
                case MessageType.BodyColorR:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] =
                        new Color(
                            ((float)br.ReadByte()) / 255f,
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g,
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;
                // Color body G
                case MessageType.BodyColorG:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] =
                        new Color(
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r,
                            ((float)br.ReadByte()) / 255f,
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;
                // Color body B
                case MessageType.BodyColorB:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] =
                        new Color(
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r,
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g,
                            ((float)br.ReadByte()) / 255f);
                    return;
                // Color eye R
                case MessageType.EyeColorR:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] =
                        new Color(
                            ((float)br.ReadByte()) / 255f,
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g,
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;
                // Color eye G
                case MessageType.EyeColorG:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] =
                        new Color(
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r,
                            ((float)br.ReadByte()) / 255f,
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;
                // Color eye B
                case MessageType.EyeColorB:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] =
                        new Color(
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r,
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g,
                            ((float)br.ReadByte()) / 255f);
                    return;
            }
        }

        #endregion Packet Handler

        #region Outgoing Packets

        public void SendPlayersToGame()
        {
            if (!isManager)
                return;

            readiedPlayers.Clear();
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(GAME_CHANNEL, GameyHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)MessageType.SendToGame);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        public void FinishCycle()
        {
            isReady = true;

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(GAME_CHANNEL, GameyHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)0);

            //Write if the player is ready
            writer.Write(isReady);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        public void SendReady()
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(GAME_CHANNEL, GameyHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)0);

            //Write if the player is ready
            writer.Write(isReady);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        public void ToggleReady()
        {
            isReady = !isReady;

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(GAME_CHANNEL, GameyHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)MessageType.PlayerReadyUp);

            //Write if the player is ready
            writer.Write(isReady);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        public void SendColor(MessageType messageType)
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(GAME_CHANNEL, GameyHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            if (!MonklandSteamManager.connectedPlayers.Contains(playerID))
            {
                return;
            }

            //Write message type
            writer.Write(Convert.ToByte(messageType));
            switch (messageType)
            {
                case MessageType.BodyColorR:
                    //Write red body color
                    writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].r * 255f)));
                    break;
                case MessageType.BodyColorG:
                    //Write green body color
                    writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].g * 255f)));
                    break;
                case MessageType.BodyColorB:
                    //Write blue body color
                    writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].b * 255f)));
                    break;
                case MessageType.EyeColorR:
                    //Write red eye color
                    writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].r * 255f)));
                    break;
                case MessageType.EyeColorG:
                    //Write green eye color
                    writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].g * 255f)));
                    break;
                case MessageType.EyeColorB:
                    //Write blue eye color
                    writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].b * 255f)));
                    break;
                default:
                    //Write default color
                    writer.Write((byte)0);
                    break;
            }

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendReliableWithBuffering);
        }

        #endregion Outgoing Packets

        #region Incoming Packets

        public void ReadReadyUpPacket(BinaryReader reader, CSteamID sent)
        {
            bool isReady = reader.ReadBoolean();
            if (isReady)
            {
                if (!readiedPlayers.Contains(sent.m_SteamID))
                {
                    readiedPlayers.Add(sent.m_SteamID);
                    ReadyChat(sent);
                }
            }
            else
            {
                if (readiedPlayers.Contains(sent.m_SteamID))
                {
                    readiedPlayers.Remove(sent.m_SteamID);
                    ReadyChat(sent);
                }
            }
        }

        #endregion Incoming Packets
    }
}
