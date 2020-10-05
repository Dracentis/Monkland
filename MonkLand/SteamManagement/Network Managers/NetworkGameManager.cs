using Monkland.Hooks;
using Monkland.Hooks.Entities;
using Monkland.UI;
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

        // Set of players who are ready for the next cycle
        public HashSet<ulong> readiedPlayers = new HashSet<ulong>();

        // List of player body colors
        public List<Color> playerColors = new List<Color>();

        // List of player eye colors
        public List<Color> playerEyeColors = new List<Color>();

        public int joinDelay = -1;

        public int startDelay = -1;

        public bool isReady = false;

        public enum UtilMessageType
        {
            PlayerReadyUp,
            SentToGame,
            BodyColorR,
            BodyColorG,
            BodyColorB,
            EyeColorR,
            EyeColorG,
            EyeColorB,
            PlayerViolence,
        }

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
                SendColor(NetworkGameManager.UtilMessageType.BodyColorR);
                SendColor(NetworkGameManager.UtilMessageType.BodyColorG);
                SendColor(NetworkGameManager.UtilMessageType.BodyColorB);
                SendColor(NetworkGameManager.UtilMessageType.EyeColorR);
                SendColor(NetworkGameManager.UtilMessageType.EyeColorG);
                SendColor(NetworkGameManager.UtilMessageType.EyeColorB);
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
            switch ((UtilMessageType)messageType)
            {
                // ReadyUP player
                case UtilMessageType.PlayerReadyUp:
                    ReadReadyUpPacket(br, sentPlayer);
                    return;
                // Send players to game
                case UtilMessageType.SentToGame:
                    isReady = false;
                    readiedPlayers.Clear();
                    RainWorldHK.mainRW.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                    return;
                // Color body R
                case UtilMessageType.BodyColorR:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = 
                        new Color(
                            ((float)br.ReadByte()) / 255f, 
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g, 
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;
                // Color body G
                case UtilMessageType.BodyColorG:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = 
                        new Color(
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r, 
                            ((float)br.ReadByte()) / 255f, 
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;
                // Color body B
                case UtilMessageType.BodyColorB:
                    playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = 
                        new Color(
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r, 
                            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g, 
                            ((float)br.ReadByte()) / 255f);
                    return;
                // Color eye R
                case UtilMessageType.EyeColorR:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = 
                        new Color(
                            ((float)br.ReadByte()) / 255f, 
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g, 
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;
                // Color eye G
                case UtilMessageType.EyeColorG:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = 
                        new Color(
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r, 
                            ((float)br.ReadByte()) / 255f, 
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].b);
                    return;
                // Color eye B
                case UtilMessageType.EyeColorB:
                    playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)] = 
                        new Color(
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].r, 
                            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sentPlayer.m_SteamID)].g, 
                            ((float)br.ReadByte()) / 255f);
                    return;
                case UtilMessageType.PlayerViolence:
                    /*
                    ulong packetReceived = br.ReadUInt64();
                    MonklandUI.AddMessage($"{SteamFriends.GetFriendPersonaName((CSteamID)packetReceived)} was killed."); 
                    Debug.Log($"{SteamFriends.GetFriendPersonaName((CSteamID)packetReceived)} was killed. Data in packet [{packetReceived}]. " +
                        $"Sent by [{SteamFriends.GetFriendPersonaName((CSteamID)sentPlayer.m_SteamID)}]");
                        */
                    ReadViolence(br, sentPlayer);
                    break;

            }
        }

        private void ReadViolence(BinaryReader br, CSteamID sentPlayer)
        {
            // Message type

            // Damage type
            Creature.DamageType type = (Creature.DamageType)br.ReadByte();
            // Lethal
            bool lethal = br.ReadBoolean();
            // Source Template
            CreatureTemplate.Type sourceTemplate = (CreatureTemplate.Type)br.ReadByte();
            // Source ID
            ulong sourceID = (ulong)br.ReadUInt64();

            if (lethal)
            {
                string message = MonklandUI.BuildDeathMessage(sentPlayer, type, sourceTemplate, sourceID);
                Debug.Log(message);
                MonklandUI.AddMessage(message, 10);
            }

            MonklandSteamManager.Log($"[GameMNG] Reading Player Violence: Damage type {type}, Source Template {sourceTemplate}, Source ID {sourceID}");
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
            writer.Write((byte)UtilMessageType.SentToGame);

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

        /*
        public void PlayerKilled(ulong playerDead)
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)UtilMessageType.PlayerDead);
            writer.Write(playerDead);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);

            Debug.Log($"Writing packet: {packet.data}");
        }
        */

        public void SendViolence(Creature self, BodyChunk source, Creature.DamageType type, float damage)
        {
            /* 
            * Violence packet
            * packetType
            * damageType
            * sourceType (player/scav/lizard)
            * source ID
            */

            if (self == null || AbstractPhysicalObjectHK.GetField(self.abstractPhysicalObject).networkObject)
            {
                return;
            }
            
            // Source ID field
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            // SourceTemplate
            byte sourceTemplate = byte.MaxValue - 1;
            try
            {
                sourceTemplate = (byte)(source.owner as Creature).abstractCreature.creatureTemplate.type;
                AbstractPhysicalObjectHK.GetField(stick.A).dist == AbstractPhysicalObjectHK.GetField(self.physicalObjects[i][j].abstractPhysicalObject).dist
            }
            catch (Exception e) { Debug.Log("Error getting source type " + e.Message); }

            // SourceID
            ulong sourceID = 0;
            try
            {
                sourceID = (AbstractPhysicalObjectHK.GetField(source.owner.abstractPhysicalObject).owner);
            }
            catch (Exception e) {/*Debug.Log()*/}

            // Message type
            writer.Write((byte)UtilMessageType.PlayerViolence);
            // Damage type
            writer.Write((byte)type);
            // Damage
            writer.Write((bool)(damage >= 1f));
            // Source Template
            writer.Write(sourceTemplate);
            // Source ID
            writer.Write(sourceID);

            MonklandSteamManager.Log($"[GameMNG] Sending Player Violence: Type {type}, damage {damage} Source Template {(CreatureTemplate.Type)sourceTemplate}, Source ID {sourceID}");
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
            writer.Write((byte)UtilMessageType.PlayerReadyUp);

            //Write if the player is ready
            writer.Write(isReady);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }



        public void SendColor(UtilMessageType messageType)
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            if (!MonklandSteamManager.connectedPlayers.Contains(playerID))
            {
                return;
            }

            //Write message type
            writer.Write(Convert.ToByte(messageType));
            switch (messageType)
            {
                case UtilMessageType.BodyColorR:
                    //Write red body color
                    writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].r * 255f)));
                    break;
                case UtilMessageType.BodyColorG:
                    //Write green body color
                    writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].g * 255f)));
                    break;
                case UtilMessageType.BodyColorB:
                    //Write blue body color
                    writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].b * 255f)));
                    break;
                case UtilMessageType.EyeColorR:
                    //Write red eye color
                    writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].r * 255f)));
                    break;
                case UtilMessageType.EyeColorG:
                    //Write green eye color
                    writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].g * 255f)));
                    break;
                case UtilMessageType.EyeColorB:
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