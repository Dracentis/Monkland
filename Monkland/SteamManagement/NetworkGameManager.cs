using Monkland.Hooks;
using Monkland.Menus;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkGameManager : NetworkManager
    {
        public static ulong playerID;

        public static ulong managerID;
        public static bool isManager { get { return playerID == managerID; } }

        public byte GameHandler = 0;

        public HashSet<ulong> readiedPlayers = new HashSet<ulong>(); // Set of players who are ready for the next cycle
        public HashSet<ulong> inGamePlayers = new HashSet<ulong>(); // Set of players who are still in-game
        public HashSet<ulong> alivePlayers = new HashSet<ulong>(); // Set of players who are alive and in-game
        public List<Color> playerColors = new List<Color>(); // List of player body colors
        public List<Color> playerEyeColors = new List<Color>(); // List of player eye colors

        public int updateDelay = -1;
        public int startDelay = -1;

        public static string hostShelter = "";
        public const string DEFAULT_SHELTER = "SU_S01";

        public bool IsReady
        {
            get
            {
                return readiedPlayers.Contains(playerID);
            }
            set
            {
                if (value)
                {
                    if (!readiedPlayers.Contains(playerID))
                    {
                        readiedPlayers.Add(playerID);
                    }
                }
                else
                {
                    if (readiedPlayers.Contains(playerID))
                    {
                        readiedPlayers.Remove(playerID);
                    }
                }
            }
        }
        public bool IsInGame
        {
            get
            {
                return inGamePlayers.Contains(playerID);
            }
            set
            {
                if (value)
                {
                    if (!inGamePlayers.Contains(playerID))
                    {
                        inGamePlayers.Add(playerID);
                    }
                }
                else
                {
                    if (inGamePlayers.Contains(playerID))
                    {
                        inGamePlayers.Remove(playerID);
                    }
                }
            }
        }
        public bool IsAlive
        {
            get
            {
                return alivePlayers.Contains(playerID);
            }
            set
            {
                if (value)
                {
                    if (!alivePlayers.Contains(playerID))
                    {
                        alivePlayers.Add(playerID);
                    }
                }
                else
                {
                    if (alivePlayers.Contains(playerID))
                    {
                        alivePlayers.Remove(playerID);
                    }
                }
            }
        }

        public enum MessageType
        {
            StartGame,
            ManagerUpdate,
            StatusUpdate
        }

        public override void Update()
        {
            if (!MonklandSteamManager.isInLobby)
            {
                updateDelay = -1;
                startDelay = -1;
            }
            if (updateDelay > 0)
            {
                updateDelay -= 1;
            }
            else if (updateDelay == 0)
            {
                if (isManager)
                    SendManagerUpdate();
                SendStatusUpdate();
                updateDelay = -1;
            }

            if (startDelay > 0)
            {
                startDelay -= 1;
            }
            else if (startDelay == 0)
            {
                SendStartGame();
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

        public void FinishCycle()
        {
            IsReady = true;
            IsAlive = false;
            IsInGame = false;

            SendStatusUpdate();
        }

        public void ToggleReady()
        {
            IsReady = !IsReady;

            SendStatusUpdate();
        }

        public void MineForSaveData()
        { 
            // Loads shelter from selected save file
            do
            {
                int slugcat = RainWorldHK.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
                if (!RainWorldHK.rainWorld.progression.IsThereASavedGame(slugcat))
                {
                    break;
                }
                if (RainWorldHK.rainWorld.progression.currentSaveState != null && RainWorldHK.rainWorld.progression.currentSaveState.saveStateNumber == slugcat)
                {
                    hostShelter = RainWorldHK.rainWorld.progression.currentSaveState.denPosition;
                    break;
                }
                if (!File.Exists(RainWorldHK.rainWorld.progression.saveFilePath))
                {
                    break;
                }
                string[] progLines = RainWorldHK.rainWorld.progression.GetProgLines();
                if (progLines.Length == 0)
                {
                    break;
                }
                for (int i = 0; i < progLines.Length; i++)
                {
                    string[] array = Regex.Split(progLines[i], "<progDivB>");
                    if (array.Length == 2 && array[0] == "SAVE STATE" && int.Parse(array[1][21].ToString()) == slugcat)
                    {
                        List<SaveStateMiner.Target> targets = new List<SaveStateMiner.Target>();
                        targets.Add(new SaveStateMiner.Target(">DENPOS", "<svB>", "<svA>", 20));
                        List<SaveStateMiner.Result> results = SaveStateMiner.Mine(RainWorldHK.rainWorld, array[1], targets);
                        for (int j = 0; j < results.Count; j++)
                        {
                            string name = results[j].name;
                            switch (name)
                            {
                                case ">DENPOS":
                                    hostShelter = results[j].data;
                                    j = results.Count;
                                    break;
                            }
                        }
                    }
                }
            } while (false);
            if (hostShelter == "")
                hostShelter = DEFAULT_SHELTER;
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
            hostShelter = "";
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
            updateDelay = 100;
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
            GameHandler = MonklandSteamManager.instance.RegisterHandler(GAME_CHANNEL, HandlePackets);
        }

        public void HandlePackets(BinaryReader br, CSteamID sentPlayer)
        {
            byte messageType = br.ReadByte();
            switch ((MessageType)messageType)
            {
                // Start game packet from manager
                case MessageType.StartGame:
                    ReadStartGame(br, sentPlayer);
                    return;
                // Status update packet
                case MessageType.StatusUpdate:
                    ReadStatusUpdate(br, sentPlayer);
                    return;
                // Game update packet from manager
                case MessageType.ManagerUpdate:
                    ReadManagerUpdate(br, sentPlayer);
                    return;
            }
        }

        #endregion Packet Handler

        #region Outgoing Packets

        public void SendStartGame()
        {
            if (!isManager)
                return;

            readiedPlayers.Clear();
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(GAME_CHANNEL, GameHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)MessageType.StartGame);
            
            //writer.Write((string)hostShelter);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }


        public void SendStatusUpdate()
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(GAME_CHANNEL, GameHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)MessageType.StatusUpdate);

            writer.Write(this.IsReady);
            writer.Write(this.IsInGame);
            writer.Write(this.IsAlive);
            writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].r * 255f)));
            writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].g * 255f)));
            writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].b * 255f)));
            writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].r * 255f)));
            writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].g * 255f)));
            writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(playerID)].b * 255f)));

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        public void SendManagerUpdate()
        {
            if (!isManager)
                return;

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(GAME_CHANNEL, GameHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            // Write message type
            writer.Write((byte)MessageType.ManagerUpdate);

            if (hostShelter == "")
                MineForSaveData();

            writer.Write((string)hostShelter);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        #endregion Outgoing Packets

        #region Incoming Packets

        public void ReadStartGame(BinaryReader reader, CSteamID sent)
        {
            if ((ulong)sent != managerID)
                return;

            readiedPlayers.Clear();
            RainWorldHK.rainWorld.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
        }

        public void ReadStatusUpdate(BinaryReader reader, CSteamID sent)
        {
            bool isReady = reader.ReadBoolean();
            bool isInGame = reader.ReadBoolean();
            bool isAlive = reader.ReadBoolean();
            playerColors[MonklandSteamManager.connectedPlayers.IndexOf(sent.m_SteamID)] =
                        new Color(
                            ((float)reader.ReadByte()) / 255f,
                            ((float)reader.ReadByte()) / 255f,
                            ((float)reader.ReadByte()) / 255f);
            playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(sent.m_SteamID)] =
                        new Color(
                            ((float)reader.ReadByte()) / 255f,
                            ((float)reader.ReadByte()) / 255f,
                            ((float)reader.ReadByte()) / 255f);
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
            if (isInGame)
            {
                if (!inGamePlayers.Contains(sent.m_SteamID))
                {
                    inGamePlayers.Add(sent.m_SteamID);
                }
            }
            else
            {
                if (inGamePlayers.Contains(sent.m_SteamID))
                {
                    inGamePlayers.Remove(sent.m_SteamID);
                }
            }
            if (isAlive)
            {
                if (!alivePlayers.Contains(sent.m_SteamID))
                {
                    alivePlayers.Add(sent.m_SteamID);
                }
            }
            else
            {
                if (alivePlayers.Contains(sent.m_SteamID))
                {
                    alivePlayers.Remove(sent.m_SteamID);
                }
            }
        }

        public void ReadManagerUpdate(BinaryReader reader, CSteamID sent)
        {
            if ((ulong) sent != managerID)
                return;
            hostShelter = reader.ReadString();
        }

        #endregion Incoming Packets
    }
}
