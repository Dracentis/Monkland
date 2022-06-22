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
    internal class GameManager : NetworkManager
    {
        public HashSet<ulong> readiedPlayers = new HashSet<ulong>(); // Set of players who are ready for the next cycle
        public HashSet<ulong> inGamePlayers = new HashSet<ulong>(); // Set of players who are still in-game
        public HashSet<ulong> alivePlayers = new HashSet<ulong>(); // Set of players who are alive and in-game
        public List<Color> playerColors = new List<Color>(); // List of player body colors
        public List<Color> playerEyeColors = new List<Color>(); // List of player eye colors

        public int updateDelay = -1;
        public int startDelay = -1;

        // Host game data
        public string hostShelter = "";
        public int hostSlugcat = 0;
        public int hostCycleNum = 0;
        public int hostFood = 0;
        public int hostKarma = 0;
        public int hostKarmaCap = 0;
        public bool hostHasGlow = false;
        public bool hostHasMark = false;
        public bool hostKarmaReinforced = false;
        public bool hostRedsExtraCycles = false;
        public string DEFAULT_SHELTER = "SU_S01";

        // Persistent game data
        public Color bodyColor = new Color(1f, 1f, 1f);
        public Color eyeColor = new Color(0.004f, 0.004f, 0.004f);
        public string lastShelter = "";
        public int lastFood = 0;
        public int lastKarma = 0;
        public bool lastHasGlow = false;
        public bool lastKarmaReinforced = false;

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
            GameUpdate, // Packet sent by manager to update the next shelter location
            StatusUpdate // Packet sent by players to update there status and color
        }

        public override void Update()
        {
            if (!MonklandSteamworks.isInLobby)
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
                    SendGameUpdate(false);
                SendStatusUpdate();
                updateDelay = -1;
            }

            if (startDelay > 0)
            {
                startDelay -= 1;
            }
            else if (startDelay == 0)
            {
                SendGameUpdate(true);
                startDelay = -1;
            }
        }

        public void QueueStart()
        {
            // Called when manager presses start game or continue button
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
            hostShelter = "";
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
                hostSlugcat = slugcat;
                if (!RainWorldHK.rainWorld.progression.IsThereASavedGame(slugcat))
                {
                    break;
                }
                if (RainWorldHK.rainWorld.progression.currentSaveState != null && RainWorldHK.rainWorld.progression.currentSaveState.saveStateNumber == slugcat)
                {
                    hostShelter = RainWorldHK.rainWorld.progression.currentSaveState.denPosition;
                    hostKarmaCap = RainWorldHK.rainWorld.progression.currentSaveState.deathPersistentSaveData.karmaCap;
                    hostKarma = RainWorldHK.rainWorld.progression.currentSaveState.deathPersistentSaveData.karma;
                    hostKarmaReinforced = RainWorldHK.rainWorld.progression.currentSaveState.deathPersistentSaveData.reinforcedKarma;
                    hostCycleNum = RainWorldHK.rainWorld.progression.currentSaveState.cycleNumber;
                    hostHasGlow = RainWorldHK.rainWorld.progression.currentSaveState.theGlow;
                    hostHasMark = RainWorldHK.rainWorld.progression.currentSaveState.deathPersistentSaveData.theMark;
                    hostRedsExtraCycles = RainWorldHK.rainWorld.progression.currentSaveState.redExtraCycles;
                    hostFood = RainWorldHK.rainWorld.progression.currentSaveState.food;
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
                        targets.Add(new SaveStateMiner.Target(">CYCLENUM", "<svB>", "<svA>", 50));
                        targets.Add(new SaveStateMiner.Target(">FOOD", "<svB>", "<svA>", 20));
                        targets.Add(new SaveStateMiner.Target(">HASTHEGLOW", null, "<svA>", 20));
                        targets.Add(new SaveStateMiner.Target(">REINFORCEDKARMA", "<dpB>", "<dpA>", 20));
                        targets.Add(new SaveStateMiner.Target(">KARMA", "<dpB>", "<dpA>", 20));
                        targets.Add(new SaveStateMiner.Target(">KARMACAP", "<dpB>", "<dpA>", 20));
                        targets.Add(new SaveStateMiner.Target(">HASTHEMARK", null, "<dpA>", 20));
                        targets.Add(new SaveStateMiner.Target(">REDEXTRACYCLES", null, "<svA>", 20));
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
                                case ">CYCLENUM":
                                    try
                                    {
                                        hostCycleNum = int.Parse(results[j].data);
                                    }
                                    catch
                                    {
                                        Debug.Log("failed to assign cycle num. Data: " + results[j].data);
                                    }
                                    break;
                                case ">FOOD":
                                    try
                                    {
                                        hostFood = int.Parse(results[j].data);
                                    }
                                    catch
                                    {
                                        Debug.Log("failed to assign food. Data: " + results[j].data);
                                    }
                                    break;
                                case ">HASTHEGLOW":
                                    hostHasGlow = true;
                                    break;
                                case ">REINFORCEDKARMA":
                                    hostKarmaReinforced = (results[j].data == "1");
                                    break;
                                case ">KARMA":
                                    try
                                    {
                                        hostKarma = int.Parse(results[j].data);
                                    }
                                    catch
                                    {
                                        Debug.Log("failed to assign karma. Data: " + results[j].data);
                                    }
                                    break;
                                case ">KARMACAP":
                                    try
                                    {
                                        hostKarmaCap = int.Parse(results[j].data);
                                    }
                                    catch
                                    {
                                        Debug.Log("failed to assign karma cap. Data: " + results[j].data);
                                    }
                                    break;
                                case ">HASTHEMARK":
                                    hostHasMark = true;
                                    break;
                                case ">REDEXTRACYCLES":
                                    hostRedsExtraCycles = true;
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
                playerColors.Add(bodyColor);
                playerEyeColors.Add(eyeColor);
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
            playerColors.RemoveAt(MonklandSteamworks.connectedPlayers.IndexOf(steamID));
            playerEyeColors.RemoveAt(MonklandSteamworks.connectedPlayers.IndexOf(steamID));
            readiedPlayers.Remove(steamID);
            inGamePlayers.Remove(steamID);
            alivePlayers.Remove(steamID);
        }

        #region Packet Handler

        public override void HandlePackets(BinaryReader br, CSteamID sentPlayer)
        {
            byte messageType = br.ReadByte();
            switch ((MessageType)messageType)
            {
                // Start game packet from manager
                case MessageType.GameUpdate:
                    ReadGameUpdate(br, sentPlayer);
                    return;
                // Status update packet
                case MessageType.StatusUpdate:
                    ReadStatusUpdate(br, sentPlayer);
                    return;
            }
        }

        #endregion Packet Handler

        #region Outgoing Packets

        public void SendGameUpdate(bool startGame)
        {
            if (!isManager)
                return;

            if (startGame)
                readiedPlayers.Clear();

            MonklandSteamworks.DataPacket packet = MonklandSteamworks.instance.GetNewPacket(channel, handler);
            BinaryWriter writer = MonklandSteamworks.instance.GetWriterForPacket(packet);

            // Write message type
            writer.Write((byte)MessageType.GameUpdate);

            if (hostShelter == "")
                MineForSaveData();

            writer.Write((bool)startGame);
            writer.Write((string)hostShelter);
            writer.Write((int)hostSlugcat);
            writer.Write((int)hostCycleNum);
            writer.Write((int)hostFood);
            writer.Write((int)hostKarma);
            writer.Write((int)hostKarmaCap);
            writer.Write((bool)hostHasGlow);
            writer.Write((bool)hostHasMark);
            writer.Write((bool)hostKarmaReinforced);
            writer.Write((bool)hostRedsExtraCycles);

            MonklandSteamworks.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamworks.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        public void SendStatusUpdate()
        {
            MonklandSteamworks.DataPacket packet = MonklandSteamworks.instance.GetNewPacket(channel, handler);
            BinaryWriter writer = MonklandSteamworks.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write((byte)MessageType.StatusUpdate);

            writer.Write(IsReady);
            writer.Write(IsInGame);
            writer.Write(IsAlive);
            writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamworks.connectedPlayers.IndexOf(playerID)].r * 255f)));
            writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamworks.connectedPlayers.IndexOf(playerID)].g * 255f)));
            writer.Write(Convert.ToByte((int)(playerColors[MonklandSteamworks.connectedPlayers.IndexOf(playerID)].b * 255f)));
            writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(playerID)].r * 255f)));
            writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(playerID)].g * 255f)));
            writer.Write(Convert.ToByte((int)(playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(playerID)].b * 255f)));

            MonklandSteamworks.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamworks.instance.SendPacketToAll(packet, false, EP2PSend.k_EP2PSendReliable);
        }

        #endregion Outgoing Packets

        #region Incoming Packets

        public void ReadGameUpdate(BinaryReader reader, CSteamID sent)
        {
            if ((ulong)sent != managerID)
                return;

            bool startGame = reader.ReadBoolean();
            hostShelter = reader.ReadString();
            hostSlugcat = reader.ReadInt32();
            hostCycleNum = reader.ReadInt32();
            hostFood = reader.ReadInt32();
            hostKarma = reader.ReadInt32();
            hostKarmaCap = reader.ReadInt32();
            hostHasGlow = reader.ReadBoolean();
            hostHasMark = reader.ReadBoolean();
            hostKarmaReinforced = reader.ReadBoolean();
            hostRedsExtraCycles = reader.ReadBoolean();

            if (startGame)
            {
                MonklandSteamworks.Log("STARTING GAME! - Shelter:" + hostShelter);
                readiedPlayers.Clear();
                RainWorldHK.rainWorld.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
            }
        }

        public void ReadStatusUpdate(BinaryReader reader, CSteamID sent)
        {
            bool isReady = reader.ReadBoolean();
            bool isInGame = reader.ReadBoolean();
            bool isAlive = reader.ReadBoolean();
            playerColors[MonklandSteamworks.connectedPlayers.IndexOf(sent.m_SteamID)] =
                        new Color(
                            ((float)reader.ReadByte()) / 255f,
                            ((float)reader.ReadByte()) / 255f,
                            ((float)reader.ReadByte()) / 255f);
            playerEyeColors[MonklandSteamworks.connectedPlayers.IndexOf(sent.m_SteamID)] =
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

        #endregion Incoming Packets
    }
}
