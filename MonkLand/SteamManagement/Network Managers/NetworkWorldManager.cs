using System;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using UnityEngine;
using RWCustom;
using System.IO;
using Monkland.Patches;

namespace Monkland.SteamManagement
{
    class NetworkWorldManager : NetworkManager
    {
        private static ulong playerID
        {
            get{
                return NetworkGameManager.playerID;
            } 
        }
        private static ulong managerID
        {
            get
            {
                return NetworkGameManager.managerID;
            }
        }
        private static bool isManager { get { return playerID == managerID; } }
        
        public HashSet<ulong> ingamePlayers = new HashSet<ulong>();

        public Dictionary<ulong, List<string>> roomDict = new Dictionary<ulong, List<string>>();
        public Dictionary<string, List<ulong>> commonRooms = new Dictionary<string, List<ulong>>();

        public bool gameRunning = false;

        public int cycleLength = 36000;
        public int timer = 0;
        public int joinDelay = -1;
        public int syncDelay = 1000;

        public const int WORLD_CHANNEL = 1;

        public byte WorldHandler = 0;

        public float AmountLeft
        {
            get
            {
                return (float)(this.cycleLength - this.timer) / (float)this.cycleLength;
            }
        }

        public override void Update()
        {
            if (joinDelay > 0)
            {
                joinDelay -= 1;
            }
            else if (joinDelay == 0)
            {
                if (gameRunning)
                {
                    GameStart();
                }
                else
                {
                    GameEnd();
                }
                joinDelay = -1;
            }
        }

        public override void Reset()
        {
            this.cycleLength = 36000;
            this.timer = 0;
            ingamePlayers.Clear();
            commonRooms.Clear();
            roomDict.Clear();
            this.gameRunning = false;
            this.joinDelay = -1;
        }

        public override void PlayerJoined(ulong steamID)
        {
            if (!roomDict.ContainsKey(steamID))
                roomDict.Add(steamID, new List<string>());
            joinDelay = 80;
        }

        public override void PlayerLeft(ulong steamID)
        {
            if (roomDict.ContainsKey(playerID))
                roomDict.Remove(steamID);
            if (ingamePlayers.Contains(playerID))
                ingamePlayers.Remove(playerID);
        }


        #region Logistics
        
        public string GetRegionName(string roomName)
        {
            roomName = roomName.Substring(0, 2);
            string text = "";
            switch (roomName)
            {
                case "CC":
                    text = "Chimney Canopy";
                    break;
                case "DS":
                    text = "Drainage System";
                    break;
                case "HI":
                    text = "Industrial Complex";
                    break;
                case "GW":
                    text = "Garbage Wastes";
                    break;
                case "SI":
                    text = "Sky Islands";
                    break;
                case "SU":
                    text = "Outskirts";
                    break;
                case "SH":
                    text = "Shaded Citadel";
                    break;
                case "IS":
                    text = "Intake System";
                    break;
                case "SL":
                    text = "Shoreline";
                    break;
                case "LF":
                    text = "Farm Arrays";
                    break;
                case "UW":
                    text = "The Exterior";
                    break;
                case "SB":
                    text = "Subterranean";
                    break;
                case "SS":
                    text = "Five Pebbles";
                    break;
                case "RW":
                    text = "Side House";
                    break;
                case "AB":
                    text = "Arid Barrens";
                    break;
                case "TR":
                    text = "The Root";
                    break;
                case "BL":
                    text = "Badlands";
                    break;
                case "AR":
                    text = "Aether Ridge";
                    break;
                case "LM":
                    text = "Looks To the Moon";
                    break;
                case "MW":
                    text = "The Fragmented Exterior";
                    break;
                case "FS":
                    text = "Forest Sanctuary";
                    break;
            }
            return text;
        }

        public void CheckForCommonRooms()
        {
            commonRooms.Clear();
            foreach (ulong player in roomDict.Keys)
            {
                if (player != playerID)
                {
                    foreach (string otherRoom in roomDict[player])
                    {
                        foreach (string myRoom in roomDict[playerID])
                        {
                            if (myRoom.Equals(otherRoom))
                            {
                                if (!commonRooms.ContainsKey(myRoom))
                                    commonRooms.Add(myRoom, new List<ulong>());
                                if (!commonRooms[myRoom].Contains(player))
                                    commonRooms[myRoom].Add(player);
                                }
                        }
                    }
                }
            }
            MonklandSteamManager.Log("Room Packet: Player shares "+ commonRooms.Count + "rooms with other players.");
        }

        public void TickCycle()
        {
            this.timer++;
            if (isManager)
            {
                if (syncDelay > 0)
                {
                    syncDelay--;
                }
                else
                {
                    SyncCycle();
                    syncDelay = 1000;
                }
            }
        }

        #endregion

        #region Packet Handler

        public override void RegisterHandlers()
        {
            WorldHandler = MonklandSteamManager.instance.RegisterHandler(WORLD_CHANNEL, HandleWorldPackets);
        }

        public void HandleWorldPackets(BinaryReader br, CSteamID sentPlayer)
        {
            byte messageType = br.ReadByte();
            switch (messageType)// up to 256 message types
            {
                case 0:// World Loaded or Exited
                    ReadLoadPacket(br, sentPlayer);
                    return;
                case 1:// Rain Sync
                    ReadRainPacket(br, sentPlayer);
                    return;
                case 2:// Realize Room
                    ReadActivateRoom(br, sentPlayer);
                    return;
                case 3:// Abstractize Room
                    ReadDeactivateRoom(br, sentPlayer);
                    return;
            }
        }

        #endregion

        #region Outgoing Packets

        public void ActivateRoom(string roomName)
        {
            if (!roomDict.ContainsKey(playerID))
                roomDict.Add(playerID, new List<string>());
            if (!roomDict[playerID].Contains(roomName))
                roomDict[playerID].Add(roomName);
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(WORLD_CHANNEL, WorldHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write(Convert.ToByte(2));
            writer.Write(roomName);


            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            //MonklandSteamManager.instance.SendPacket(packet, (CSteamID)managerID), EP2PSend.k_EP2PSendReliable);
            MonklandSteamManager.instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendReliable);
            CheckForCommonRooms();
        }
        public void DeactivateRoom(string roomName)
        {
            if (!roomDict.ContainsKey(playerID))
                roomDict.Add(playerID, new List<string>());
            if (roomDict[playerID].Contains(roomName))
                roomDict[playerID].Remove(roomName);
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(WORLD_CHANNEL, WorldHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write(Convert.ToByte(3));
            writer.Write(roomName);


            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            //MonklandSteamManager.instance.SendPacket(packet, (CSteamID)managerID), EP2PSend.k_EP2PSendReliable);
            MonklandSteamManager.instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendReliable);
            CheckForCommonRooms();
        }

        public void PrepareNextCycle()
        {
            float minutes = Mathf.Lerp(400f, 800f, UnityEngine.Random.value) / 60f;
            this.cycleLength = (int)(minutes * 40f * 60f);
            this.timer = 0;
            SyncCycle();
            syncDelay = 1000;
        }

        public void GameStart()
        {
            this.timer = 0;
            if (!ingamePlayers.Contains(playerID))
                ingamePlayers.Add(playerID);
            if (!roomDict.ContainsKey(playerID))
                roomDict.Add(playerID, new List<string>());
            gameRunning = true;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(WORLD_CHANNEL, WorldHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write(Convert.ToByte(0));
            writer.Write(true);


            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            //MonklandSteamManager.instance.SendPacket(packet, (CSteamID)managerID), EP2PSend.k_EP2PSendReliable);
            MonklandSteamManager.instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendReliable);
            if (isManager)
            {
                foreach(ulong pl in ingamePlayers)
                {
                    if (pl != playerID)
                        SyncCycle((CSteamID)pl);
                    syncDelay = 1000;
                }
            }
            MonklandSteamManager.Log("GameStart Packet: " + ingamePlayers + "\n" + ingamePlayers.Count + " players ingame.");
        }

        public void GameEnd()
        {
            if (ingamePlayers.Contains(playerID))
                ingamePlayers.Remove(playerID);
            if (roomDict.ContainsKey(playerID))
                roomDict.Remove(playerID);
            gameRunning = false;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(WORLD_CHANNEL, WorldHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write(Convert.ToByte(0));
            writer.Write(false);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            //MonklandSteamManager.instance.SendPacket(packet, (CSteamID)managerID), EP2PSend.k_EP2PSendReliable);
            MonklandSteamManager.instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendReliable);
            MonklandSteamManager.Log("GameEnd Packet: " + ingamePlayers + "\n" + ingamePlayers.Count + " players ingame.");
        }

        public void SyncCycle(CSteamID target)// Syncs rain values for an individual player called by manager after each player loads the game
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(WORLD_CHANNEL, WorldHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write(Convert.ToByte(1));
            writer.Write(cycleLength);
            writer.Write(timer);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            MonklandSteamManager.instance.SendPacket(packet, target, EP2PSend.k_EP2PSendReliable);
            //MonklandSteamManager.instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendReliable);
        }

        public void SyncCycle()// Syncs rain values for all players called by manager before game loads
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(WORLD_CHANNEL, WorldHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write(Convert.ToByte(1));
            writer.Write(cycleLength);
            writer.Write(timer);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            //MonklandSteamManager.instance.SendPacket(packet, target, EP2PSend.k_EP2PSendReliable);
            MonklandSteamManager.instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendReliable);
        }

        #endregion

        #region Incoming Packets

        public void ReadLoadPacket(BinaryReader reader, CSteamID sent)
        {
            if (reader.ReadBoolean())
            {
                if (!ingamePlayers.Contains(sent.m_SteamID))
                    ingamePlayers.Add(sent.m_SteamID);
                if (!roomDict.ContainsKey(sent.m_SteamID))
                    roomDict.Add(sent.m_SteamID, new List<string>());
                if (isManager && gameRunning)
                {
                    SyncCycle(sent);
                }
            }
            else
            {
                if (ingamePlayers.Contains(sent.m_SteamID))
                    ingamePlayers.Remove(sent.m_SteamID);
                if (roomDict.ContainsKey(sent.m_SteamID))
                    roomDict.Remove(sent.m_SteamID);
            }
            MonklandSteamManager.Log("Ingame Packet: " + ingamePlayers+"\n"+ ingamePlayers.Count +" players ingame.");
        }

        public void ReadDeactivateRoom(BinaryReader reader, CSteamID sent)
        {
            if (sent.m_SteamID == playerID)
                return;
            string roomName = reader.ReadString();
            if (!roomDict.ContainsKey(sent.m_SteamID))
                roomDict.Add(sent.m_SteamID, new List<string>());
            if (roomDict[sent.m_SteamID].Contains(roomName))
                roomDict[sent.m_SteamID].Remove(roomName);
            CheckForCommonRooms();
        }

        public void ReadActivateRoom(BinaryReader reader, CSteamID sent)
        {
            if (sent.m_SteamID == playerID)
                return;
            string roomName = reader.ReadString();
            if (!roomDict.ContainsKey(sent.m_SteamID))
                roomDict.Add(sent.m_SteamID, new List<string>());
            if (!roomDict[sent.m_SteamID].Contains(roomName))
                roomDict[sent.m_SteamID].Add(roomName);
            CheckForCommonRooms();
        }

        public void ReadRainPacket(BinaryReader reader, CSteamID sent)
        {
            this.cycleLength = reader.ReadInt32();
            this.timer = reader.ReadInt32();
            MonklandSteamManager.Log("Rain Packet: "+this.cycleLength+", "+this.timer);
            if (patch_RainWorldGame.mainGame != null && patch_RainWorldGame.mainGame.overWorld != null && patch_RainWorldGame.mainGame.overWorld.activeWorld != null)
            {
                patch_RainWorldGame.mainGame.overWorld.activeWorld.rainCycle.cycleLength = this.cycleLength;
                patch_RainWorldGame.mainGame.overWorld.activeWorld.rainCycle.timer = this.timer;
            }
        }


        #endregion
    }
    
}
