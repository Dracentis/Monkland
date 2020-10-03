﻿using Monkland.Hooks;
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

        public const int CHANNEL = 0;

        public byte UtilityHandler = 0;

        public HashSet<ulong> readiedPlayers = new HashSet<ulong>();// Set of players who are ready for the next cycle
        public List<Color> playerColors = new List<Color>();// List of player body colors
        public List<Color> playerEyeColors = new List<Color>();// List of player eye colors

        public int joinDelay = -1;

        public int startDelay = -1;

        public bool isReady = false;

        public override void Update()
        {
            if (!MonklandSteamManager.isInGame)
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
                SendColor(0);
                SendColor(1);
                SendColor(2);
                SendColor(3);
                SendColor(4);
                SendColor(5);
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
                return;

            MonklandSteamManager.WorldManager.PrepareNextCycle();
            startDelay = 100;
        }

        private void ReadyChat(CSteamID sentPlayer)
        {
            MultiplayerChat.AddChat(string.Format("User {0} is now: " + (readiedPlayers.Contains(sentPlayer.m_SteamID) ? "Ready!" : "Not Ready."), SteamFriends.GetFriendPersonaName(sentPlayer)));
            //MultiplayerChat.chatStrings.Add(string.Format("User {0} changed their color to: " + playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r + ", " + playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g + ", " + playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b, SteamFriends.GetFriendPersonaName(sentPlayer)));
        }

        public override void Reset()
        {
            playerID = SteamUser.GetSteamID().m_SteamID;
            managerID = 0;

            //Readied players
            readiedPlayers.Clear();
            playerColors.Clear();
            playerEyeColors.Clear();
            isReady = false;
        }

        public override void PlayerJoined(ulong steamID)
        {
            if (steamID == playerID)
            {
                playerColors.Add(MonklandSteamManager.bodyColor);
                playerEyeColors.Add(MonklandSteamManager.eyeColor);
                //MonklandSteamManager.Log("Using Last Colors");
            }
            else
            {
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
        }

        #region Packet Handler

        public override void RegisterHandlers()
        {
            UtilityHandler = MonklandSteamManager.instance.RegisterHandler(CHANNEL, HandleUtilPackets);
        }

        public void HandleUtilPackets(BinaryReader br, CSteamID sentPlayer)
        {
            byte messageType = br.ReadByte();
            switch (messageType)
            {
                case 0:
                    ReadReadyUpPacket(br, sentPlayer);
                    return;

                case 1:
                    isReady = false;
                    readiedPlayers.Clear();
                    RainWorldHK.mainRW.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                    return;

                case 2:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = new Color(((float)br.ReadByte()) / 255f, playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g, playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;

                case 3:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = new Color(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r, ((float)br.ReadByte()) / 255f, playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;

                case 4:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = new Color(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r, playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g, ((float)br.ReadByte()) / 255f);
                    return;

                case 5:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = new Color(((float)br.ReadByte()) / 255f, playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g, playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;

                case 6:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = new Color(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r, ((float)br.ReadByte()) / 255f, playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;

                case 7:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = new Color(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r, playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g, ((float)br.ReadByte()) / 255f);
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
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)1);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        public void FinishCycle()
        {
            isReady = true;

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
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
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
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

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)0);

            //Write if the player is ready
            writer.Write(isReady);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        public void SendColor(int colorID)
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            if (!MonklandSteamManager.connectedPlayers.Contains(playerID))
                return;

            //Write message type
            writer.Write(Convert.ToByte(colorID + 2));

            if (colorID == 0)//colorID used to distiquish which color value is being sent
            {
                writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].r * 255f)));//Write red body color
            }
            else if (colorID == 1)
            {
                writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].g * 255f)));//Write green body color
            }
            else if (colorID == 2)
            {
                writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].b * 255f)));//Write blue body color
            }
            else if (colorID == 3)
            {
                writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].r * 255f)));//Write red eye color
            }
            else if (colorID == 4)
            {
                writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].g * 255f)));//Write green eye color
            }
            else if (colorID == 5)
            {
                writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].b * 255f))); //Write blue eye color
            }
            else
            {
                writer.Write((byte)0); //Write default color
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