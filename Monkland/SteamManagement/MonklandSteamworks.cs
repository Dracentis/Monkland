using Monkland.Hooks;
using Monkland.Menus;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class MonklandSteamworks : MonoBehaviour
    {
        public static MonklandSteamworks instance;
        public const ulong APPID = 312520;
        public static CSteamID lobbyID = new CSteamID(0); // Current lobby id
        public static LobbyInfo lobbyInfo = new LobbyInfo(lobbyID); // Current lobby info
        public static bool isInLobby = false; // Is currently in a lobby
        public static List<ulong> connectedPlayers = new List<ulong>(); // List of all players
        public static List<ulong> otherPlayers = new List<ulong>(); // List of all players excluding this player
        public static ulong playerID;
        public static ulong managerID;
        public static bool isManager { get { return playerID == managerID; } }

        // New lobby parameters
        public static int lobbyType = 0; // 0 = Public, 1 = Friends, 2 = Private
        public static int lobbyMax = 10; // Max number of players in a lobby

        // Lobby search parameters
        public static bool searching = false; // Is searching for lobbies
        public static bool joining = false; // Is joining a lobby
        public static List<LobbyInfo> lobbies = new List<LobbyInfo>(); // Lobby list


        public static void Log(object message, bool visible = false)
        {
            if (Monkland.DEVELOPMENT)
            {
                Debug.Log("[MONKLAND] " + message.ToString());
            }
        }

        public static void CreateManagerGameObject()
        {
            // Create the instance of MonklandSteamManager for this session
            GameObject gObject = new GameObject("MonklandSteamManagerObject");
            gObject.AddComponent<MonklandSteamworks>();
            DontDestroyOnLoad(gObject);
        }

        public void Awake()
        {
            instance = this;

            RegisterCallbacks();
            InitializePacketTools();
        }

        public void Start()
        {
            Log("Monkland v" + Monkland.VERSION + " Loaded!");
            createChannelsCallback += RegisterDefaultChannels;
            registerManagersCallback += RegisterDefaultNetworkManagers;
            registerHandlersCallback += RegisterDefaultHandlers;

            createChannelsCallback();
            registerManagersCallback();
            registerHandlersCallback();
        }

        public void Update()
        {
            if (lobbyMax > 250)
            {
                lobbyMax = 250;
            }
            ReadPackets();
            UpdateManagers();
        }

        public void CreateLobby()
        {
            if (lobbyType == 0)
            {
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, lobbyMax); // Create a public lobby
            }
            else if (lobbyType == 1)
            {
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, lobbyMax); // Create a friends only lobby
            }
            else
            {
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, lobbyMax); // Create a private lobby
            }
            joining = true;
        }

        public void LeaveLobby()
        {
            // Leave the current lobby
            if (lobbyID.m_SteamID != 0)
            { SteamMatchmaking.LeaveLobby(lobbyID); }
            foreach (ulong player in otherPlayers)
            {
                foreach (PacketChannel channel in allChannels)
                {
                    SteamNetworking.CloseP2PChannelWithUser((CSteamID)player, channel.channelIndex);
                }
                SteamNetworking.CloseP2PSessionWithUser((CSteamID)player);
            }
            connectedPlayers.Clear();
            otherPlayers.Clear();
            ResetManagers();
            lobbyID = new CSteamID(0);
            lobbyInfo = new LobbyInfo(lobbyID);
            joining = false;
            isInLobby = false;
            Log("Left lobby!", true);
        }

        public void FindLobbies() 
        {
            // Start search for lobbies
            // SteamMatchmaking()->AddRequestLobbyListFilter*() functions would be called here, before RequestLobbyList()
            SteamMatchmaking.RequestLobbyList();
            searching = true;
            lobbies.Clear();
        }

        public void ExitToMultiplayerMenu()
        {
            // Exits from current process to the multiplay menu and leaves the current lobby
            if (RainWorldHK.rainWorld.processManager.currentMainLoop is RainWorldGame)
                (RainWorldHK.rainWorld.processManager.currentMainLoop as RainWorldGame).ExitGame(true, true);
            ProcessManagerHK.ImmediateSwitchCustom(RainWorldHK.rainWorld.processManager, new LobbyFinderMenu(RainWorldHK.rainWorld.processManager));
            /*
            if (monklandUI != null)
            {
                monklandUI.ClearSprites();
                monklandUI = null;
            }
            */
            LeaveLobby();
        }

        public void JoinLobby(CSteamID lobby)
        {
            if (lobbyID.m_SteamID != 0)
            { LeaveLobby(); }
            joining = true;
            SteamMatchmaking.JoinLobby(lobby);
        }

        #region Callbacks

        public delegate void GenericCallback();

        public Callback<LobbyCreated_t> lobbyCreated;
        public Callback<LobbyChatUpdate_t> lobbyUpdate;
        public Callback<LobbyChatMsg_t> lobbyChatMessage;
        public Callback<LobbyKicked_t> lobbyKicked;
        public Callback<LobbyEnter_t> lobbyEntered;
        public Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
        public Callback<LobbyMatchList_t> lobbySearchFinished;

        public Callback<P2PSessionRequest_t> p2pRequest;
        public Callback<P2PSessionConnectFail_t> p2pConnectFail;

        public void RegisterCallbacks()
        {
            lobbyCreated = Callback<LobbyCreated_t>.Create(LobbyCreated);
            lobbyUpdate = Callback<LobbyChatUpdate_t>.Create(LobbyChatUpdated);
            lobbyChatMessage = Callback<LobbyChatMsg_t>.Create(LobbyChatMsg);
            lobbyKicked = Callback<LobbyKicked_t>.Create(LobbyKicked);
            lobbyEntered = Callback<LobbyEnter_t>.Create(LobbyEntered);
            lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(LobbyJoinRequested);
            lobbySearchFinished = Callback<LobbyMatchList_t>.Create(LobbySearchFinished);

            p2pRequest = Callback<P2PSessionRequest_t>.Create(P2PRequest);
            p2pConnectFail = Callback<P2PSessionConnectFail_t>.Create(P2PConnectionFail);
        }

        public void LobbyCreated(LobbyCreated_t result)
        {
            SteamMatchmaking.JoinLobby((CSteamID)result.m_ulSteamIDLobby);
            lobbyID = (CSteamID)result.m_ulSteamIDLobby;
            lobbyInfo = new LobbyInfo((CSteamID)result.m_ulSteamIDLobby);
            joining = false;

            SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "ManagerID", SteamUser.GetSteamID().ToString());
            lobbyInfo.owner = SteamUser.GetSteamID();
            lobbyInfo.memberLimit = lobbyMax;
            lobbyInfo.memberNum = 1;
            SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "Version", Monkland.VERSION);
            lobbyInfo.version = Monkland.VERSION;
            Log("Created Lobby!", true);
        }

        public void LobbyChatUpdated(LobbyChatUpdate_t update)
        {
            try
            {
                EChatMemberStateChange change = (EChatMemberStateChange)update.m_rgfChatMemberStateChange;
                if (change == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                {
                    Log(string.Format("User {0} joined the game!", SteamFriends.GetFriendPersonaName(new CSteamID(update.m_ulSteamIDUserChanged))), true);

                    if (!connectedPlayers.Contains(update.m_ulSteamIDUserChanged))
                    {
                        PlayerJoinedManagers(update.m_ulSteamIDUserChanged);
                        connectedPlayers.Add(update.m_ulSteamIDUserChanged);
                    }
                    if (update.m_ulSteamIDUserChanged != SteamUser.GetSteamID().m_SteamID)
                    {
                        otherPlayers.Add(update.m_ulSteamIDUserChanged);
                    }
                }
                else if (change == EChatMemberStateChange.k_EChatMemberStateChangeLeft || change == EChatMemberStateChange.k_EChatMemberStateChangeKicked || change == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
                {
                    Log(string.Format("User {0} left the game!", SteamFriends.GetFriendPersonaName(new CSteamID(update.m_ulSteamIDUserChanged))), true);
                    PlayerLeftManagers(update.m_ulSteamIDUserChanged);
                    connectedPlayers.Remove(update.m_ulSteamIDUserChanged);
                    MultiplayerPlayerList.RemovePlayerLabel(update.m_ulSteamIDUserChanged);
                    if (update.m_ulSteamIDUserChanged != SteamUser.GetSteamID().m_SteamID)
                    {
                        otherPlayers.Remove(update.m_ulSteamIDUserChanged);
                    }
                    if (update.m_ulSteamIDUserChanged == managerID)
                    {
                        // Host left the lobby exit to multiplayer menu
                        Log("Host left the Lobby!", true);
                        ExitToMultiplayerMenu();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void LobbyChatMsg(LobbyChatMsg_t message)
        {
        }

        public void LobbySearchFinished(LobbyMatchList_t pLobbyMatchList)
        {
            lobbies.Clear();
            int numberOfResults = (int)pLobbyMatchList.m_nLobbiesMatching;
            int numberOfLobbies = 0;
            for (int i = 0; i < numberOfResults; i++)
            {
                CSteamID lobby = SteamMatchmaking.GetLobbyByIndex(i);
                LobbyInfo info = new LobbyInfo(lobby);
                if (info.UpdateLobbyInfo(lobby))
                {
                    lobbies.Add(info);
                    numberOfLobbies++;
                }
            }
            searching = false;
            {
                if (RainWorldHK.rainWorld.processManager.currentMainLoop is LobbyFinderMenu lfm)
                { lfm.SearchFinished(numberOfLobbies); }
            }
        }

        public void LobbyKicked(LobbyKicked_t kickResult)
        {
            // Kicked from lobby exit to multiplayer menu
            Log("Kicked from Lobby!", true);
            ExitToMultiplayerMenu();
        }

        public void LobbyJoinRequested(GameLobbyJoinRequested_t request)// Lobby join from Shift-Tab Menu
        {
            if (lobbyID.m_SteamID != 0) { LeaveLobby(); }
            ProcessManagerHK.ImmediateSwitchCustom(RainWorldHK.rainWorld.processManager, new SteamMultiplayerMenu(RainWorldHK.rainWorld.processManager));
            joining = true;
            SteamMatchmaking.JoinLobby(request.m_steamIDLobby);
        }

        public void LobbyEntered(LobbyEnter_t enterLobby)
        {
            connectedPlayers.Clear();
            otherPlayers.Clear();
            ResetManagers();
            {
                if (!(RainWorldHK.rainWorld.processManager.currentMainLoop is SteamMultiplayerMenu))
                { ProcessManagerHK.ImmediateSwitchCustom(RainWorldHK.rainWorld.processManager, new SteamMultiplayerMenu(RainWorldHK.rainWorld.processManager)); }
            }

            joining = false;
            isInLobby = true;
            lobbyID = (CSteamID)enterLobby.m_ulSteamIDLobby;
            int playerCount = SteamMatchmaking.GetNumLobbyMembers((CSteamID)enterLobby.m_ulSteamIDLobby);

            //Send packets to all players, to establish P2P connections with them
            if (playerCount > 1)
            {
                for (int i = 0; i < playerCount; i++)
                {
                    CSteamID lobbyMember = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
                    SteamNetworking.SendP2PPacket(lobbyMember, new byte[] { 255 }, 1, EP2PSend.k_EP2PSendReliable, 0);
                    SteamNetworking.SendP2PPacket(lobbyMember, new byte[] { 255 }, 1, EP2PSend.k_EP2PSendReliable, 1);
                    SteamNetworking.SendP2PPacket(lobbyMember, new byte[] { 255 }, 1, EP2PSend.k_EP2PSendReliable, 2);
                    if (!connectedPlayers.Contains(lobbyMember.m_SteamID))
                    {
                        connectedPlayers.Add(lobbyMember.m_SteamID);
                        PlayerJoinedManagers(lobbyMember.m_SteamID);
                    }
                    if (lobbyMember != SteamUser.GetSteamID())
                        otherPlayers.Add(lobbyMember.m_SteamID);
                }
            }

            //Set up network data
            managerID = ulong.Parse(SteamMatchmaking.GetLobbyData(lobbyID, "ManagerID"));
            playerID = SteamUser.GetSteamID().m_SteamID;
            if (isManager)
            {
                lobbyInfo = new LobbyInfo((CSteamID)enterLobby.m_ulSteamIDLobby);
                lobbyInfo.UpdateLobbyInfo((CSteamID)enterLobby.m_ulSteamIDLobby);
                lobbyInfo.owner = new CSteamID(managerID);
            }

            if (!connectedPlayers.Contains(SteamUser.GetSteamID().m_SteamID))
            {
                connectedPlayers.Add(SteamUser.GetSteamID().m_SteamID);
                PlayerJoinedManagers(SteamUser.GetSteamID().m_SteamID);
            }

            MultiplayerChat.AddChat("Entered Lobby!");
            MultiplayerChat.AddChat("This game's manager is " + SteamFriends.GetFriendPersonaName((CSteamID)managerID));
            Log("Entered Lobby! - This game's manager is " + SteamFriends.GetFriendPersonaName((CSteamID)managerID));
        }

        public void P2PRequest(P2PSessionRequest_t request)
        {
            if (connectedPlayers.Contains(request.m_steamIDRemote.m_SteamID))
                SteamNetworking.AcceptP2PSessionWithUser(request.m_steamIDRemote);
        }

        public void P2PConnectionFail(P2PSessionConnectFail_t failResult)
        {
            Debug.LogError("P2P Error:" + ((EP2PSessionError)failResult.m_eP2PSessionError));
        }

        #endregion Callbacks

        #region Channels

        public GenericCallback createChannelsCallback = delegate () { };

        public List<PacketChannel> allChannels = new List<PacketChannel>();

        public void ReadPackets(bool forceRead = false)
        {
            foreach (PacketChannel pc in allChannels)
            {
                pc.ReadPackets(forceRead);
            }
        }

        public void ClearAllPackets()
        {
            foreach (PacketChannel p in allChannels)
                p.Clear();
        }

        public class PacketChannel
        {
            //The name of this channel
            public string channelName = "CHANNEL";

            //The channel to send packets on over steam
            public int channelIndex = 0;

            //The steam manager
            public MonklandSteamworks manager;

            public PacketChannel(string name, int channelIndex, MonklandSteamworks manager, int packetsPerUpdate = 20, int maxPackets = 100, bool priorityBased = false, bool ordered = false, bool scrapOldOrdered = false)
            {
                this.channelName = name;
                this.channelIndex = channelIndex;
                this.manager = manager;

                lastPacketID = new Dictionary<ulong, uint>();
                this.packetsPerUpdate = packetsPerUpdate;
                this.isPriorityBased = priorityBased;
                this.isOrdered = ordered;
                this.scrapOld = scrapOldOrdered;
                this.maxPackets = maxPackets;
            }

            #region Packet Management

            public uint currentID;

            // The ID of the past packet, from each player.
            public Dictionary<ulong, uint> lastPacketID;

            // True if this channel should sort packets based on priority
            public bool isPriorityBased = false;

            // True if this channel's packets are ordered
            public bool isOrdered = false;

            // True if the channel's old ordered packets should be scrapped
            public bool scrapOld = false;

            public bool requireGameState = true;

            // The amount of packets we can process per update
            public int packetsPerUpdate = 20;

            // The max amount of packets the channel is allowed to store at once
            private readonly int maxPackets = 100;

            // No-priority packet
            private readonly Queue<DataPacket> queuedPackets = new Queue<DataPacket>();

            // Priority packets
            private readonly Dictionary<byte, Queue<DataPacket>> priorityQueues = new Dictionary<byte, Queue<DataPacket>>();

            public bool isForceWait = false;
            public ulong replyValue = 0;

            //Reads packets from steam
            public void ReadPackets(bool forceRead = false)
            {
                if (forceRead == false && requireGameState)
                {
                    if (RainWorldGameHK.rainWorldGame == null || RainWorldGameHK.rainWorldGame.Players.Count == 0)
                    {
                        return;
                    }
                }

                //The number of packets we have left to process
                int packetsLeft = packetsPerUpdate;
                //The data in the packet
                byte[] packetData;

                // BinaryReader reader; // unused

                //Read all packets to the queues
                while (SteamNetworking.IsP2PPacketAvailable(out uint size, channelIndex))
                {
                    try
                    {
                        //Initialize array for packet
                        packetData = new byte[size]; //size == Size of the packet
                        //Read the packet from steam's network
                        SteamNetworking.ReadP2PPacket(packetData, size, out size, out CSteamID sentUser, channelIndex);
                        //sentUser == The user who sent the packet

                        if (size == 1)
                            continue;

                        //Create the sctruct to store the packet
                        DataPacket newPacket = new DataPacket(channelIndex, sentUser);

                        newPacket.isOrderedPacket = isOrdered;
                        newPacket.scrapOldPackets = scrapOld;
                        newPacket.isPriority = isPriorityBased;

                        newPacket.data = packetData;

                        if (isPriorityBased)
                        {
                            if (!priorityQueues.ContainsKey(newPacket.priority))
                            {
                                Log("Creating queue for priority " + newPacket.priority);
                                priorityQueues[newPacket.priority] = new Queue<DataPacket>();
                            }
                            priorityQueues[newPacket.priority].Enqueue(newPacket);
                        }
                        else
                        {
                            queuedPackets.Enqueue(newPacket);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                //Discard old packets, then process as many as we can.
                if (isPriorityBased)
                {
                    int skipCount = 0;
                    int ditchedCount = 0;
                    //Loop through all priorities in order
                    for (byte priorityI = 0; packetsLeft > 0 && priorityI < byte.MaxValue; priorityI++)
                    {
                        //If there's no packets of this priority, skip
                        if (!priorityQueues.ContainsKey(priorityI))
                        {
                            skipCount++;
                            continue;
                        }

                        //Gets the packets that need to be processed from this priority
                        Queue<DataPacket> packetQueue = priorityQueues[priorityI];

                        //Throws out packets if there's too many. This could be bad. Let's try to avoid this.
                        while (packetQueue.Count > maxPackets)
                        {
                            packetQueue.Dequeue();
                            ditchedCount++;
                        }

                        //Finally, process as many packets as we can from this priority level.
                        while (packetsLeft > 0 && packetQueue.Count > 0)
                        {
                            DataPacket getPacket = packetQueue.Dequeue();

                            try
                            {
                                if (!ProcessPacket(getPacket))
                                    packetsLeft--;
                            }
                            catch (System.Exception e)
                            {
                                StringBuilder sb = new StringBuilder();
                                for (int j = 0; j < getPacket.data.Length; j++)
                                    sb.Append(getPacket.data[j] + " " + ((getPacket.readCount == j) ? "|" : ""));

                                Debug.LogError("Packet data was " + sb.ToString() + " in channel " + channelName);
                                Debug.LogError(e);
                                packetsLeft--;
                            }
                        }
                    }
                }
                else
                {
                    //Throws out packets if there's too many. This could be bad. Let's try to avoid this.
                    while (queuedPackets.Count > maxPackets)
                        queuedPackets.Dequeue();

                    //Process as many packets as we can from this priority level.
                    while (packetsLeft > 0 && queuedPackets.Count > 0)
                    {
                        DataPacket getPacket = queuedPackets.Dequeue();
                        try
                        {
                            if (!ProcessPacket(getPacket))
                                packetsLeft--;
                        }
                        catch (System.Exception e)
                        {
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < getPacket.data.Length; i++)
                                sb.Append(getPacket.data[i] + "" + ((getPacket.readCount == i) ? "|" : ""));

                            Debug.LogError("Packet data was " + sb.ToString() + " in channel " + channelName);
                            Debug.LogError($"Packet Type {getPacket.data[2]}, ObjectType {(AbstractCreature.AbstractObjectType)getPacket.data[5]}");
                            Debug.LogError(e);
                            packetsLeft--;
                        }
                    }
                }
            }

            public void Clear()
            {
                queuedPackets.Clear();
                foreach (Queue<DataPacket> packs in priorityQueues.Values)
                {
                    packs.Clear();
                }
                priorityQueues.Clear();
            }

            public bool ProcessPacket(DataPacket packet)
            {
                try
                {
                    processedPackets++;
                    //If packets are ordered
                    if (isOrdered)
                    {
                        //If the last packet we got has a higher ID than this one, then we're
                        if (lastPacketID.ContainsKey(packet.sentPlayer.m_SteamID) && lastPacketID[packet.sentPlayer.m_SteamID] > packet.packetID)
                        {
                            return true;
                        }
                        lastPacketID[packet.sentPlayer.m_SteamID] = packet.packetID;
                    }

                    BinaryReader br = manager.GetReaderForPacket(packet, packet.data);

                    byte forcewaitData = br.ReadByte();

                    if (forcewaitData == 1)
                    {
                        RecievedForcewaitPacket(br, packet.sentPlayer);
                        return true;
                    }
                    else if (forcewaitData == 2)
                    {
                        RecieveForcewaitReply(br, packet.sentPlayer);
                        return true;
                    }
                    else
                    {
                        /*if (DEBUG && this.channelIndex != 2)
                        {
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < packet.data.Length; i++)
                                sb.Append(packet.data[i] + "|");
                            Log("Received packet from player " + SteamFriends.GetFriendPersonaName(packet.sentPlayer) + " with data " + sb.ToString() + " in channel " + this.channelName);
                        }*/
                        handlers[packet.handlerID](br, packet.sentPlayer);
                    }
                    return false;
                }
                catch (System.Exception e)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < packet.data.Length; j++)
                        sb.Append(packet.data[j] + " " + ((packet.readCount == j) ? "|" : ""));

                    Debug.LogError("Packet data was " + sb.ToString() + " in channel " + channelName);
                    Debug.LogError(e);

                    return false;
                }
            }

            public void SendPacketToAll(DataPacket packet, bool othersOnly = false, EP2PSend sendType = EP2PSend.k_EP2PSendUnreliableNoDelay)
            {
                if (othersOnly)
                {
                    foreach (CSteamID id in otherPlayers)
                        SendPacket(packet, id, sendType);
                }
                else
                {
                    foreach (CSteamID id in connectedPlayers)
                        SendPacket(packet, id, sendType);
                }
            }

            public void SendPacket(DataPacket packet, CSteamID user, EP2PSend sendType = EP2PSend.k_EP2PSendUnreliableNoDelay)
            {
                if (Monkland.DEVELOPMENT)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < packet.data.Length; i++)
                        sb.Append(packet.data[i] + "|");
                    //if (this.channelIndex != 2)
                    //    Log("Sending packet to player " + SteamFriends.GetFriendPersonaName(user) + " with data " + sb.ToString() + " in channel " + channelName);
                    //Log(Environment.StackTrace);
                }

                if (user.m_SteamID == playerID)
                {
                    //Process packet immediately if we sent it to ourself.
                    ProcessPacket(packet);
                }
                else
                {
                    //Send it off to another player
                    SteamNetworking.SendP2PPacket(user, packet.data, (uint)packet.data.Length, sendType, channelIndex);
                }
            }

            public ulong SendForcewaitPacket(ulong id, CSteamID user)
            {
                //Tag this to be waiting
                isForceWait = true;
                //Send packet
                {
                    MonklandSteamworks.DataPacket packet = MonklandSteamworks.instance.GetNewPacket(channelIndex, 0);
                    packet.priority = 0;
                    BinaryWriter writer = MonklandSteamworks.instance.GetWriterForPacket(packet, 1);
                    writer.Write(id);
                    MonklandSteamworks.instance.FinalizeWriterToPacket(writer, packet);
                    MonklandSteamworks.instance.SendPacket(packet, user, EP2PSend.k_EP2PSendReliable);
                }

                System.DateTime startTime = System.DateTime.Now;

                //As long as we're supposed to wait
                while (isForceWait)
                {
                    //Read all packets
                    ReadPackets(true);
                    //Sleep for 10 ms
                    System.Threading.Thread.Sleep(10);

                    //If it's been 30 seconds and we haven't gotten a reply, return 0
                    if ((startTime - System.DateTime.Now).TotalSeconds > 30)
                        return 0;
                }

                return replyValue;
            }

            public void SendForcewaitReply(ulong response, CSteamID user)
            {
                MonklandSteamworks.DataPacket packet = MonklandSteamworks.instance.GetNewPacket(channelIndex, 0);
                packet.priority = 0;
                BinaryWriter writer = MonklandSteamworks.instance.GetWriterForPacket(packet, 2);
                writer.Write(response);
                MonklandSteamworks.instance.FinalizeWriterToPacket(writer, packet);
                MonklandSteamworks.instance.SendPacket(packet, user, EP2PSend.k_EP2PSendReliable);
            }

            public void RecieveForcewaitReply(BinaryReader reader, CSteamID sender)
            {
                isForceWait = false;
                replyValue = reader.ReadUInt64();
            }

            public void RecievedForcewaitPacket(BinaryReader reader, CSteamID sender)
            {
                int id = reader.ReadInt32();

                switch (id)
                {
                    case 0:
                        SendForcewaitReply((ulong)RainWorldGameHK.rainWorldGame.Players.Count, sender);
                        return;
                }
            }

            public uint GetNewID()
            {
                return currentID++;
            }

            #endregion Packet Management

            #region Handlers

            public List<PacketHandler> handlers = new List<PacketHandler>();

            public byte RegisterHandler(PacketHandler handler)
            {
                if (handlers.Count == 255)
                {
                    throw new Exception("Already registered max number of handlers in channel " + channelName);
                }
                handlers.Add(handler);
                return (byte)(handlers.Count - 1);
            }

            #endregion Handlers

            #region Debug Info

            private int processedPackets;

            public int GetPacketCount()
            {
                if (isPriorityBased)
                {
                    int i = 0;
                    for (byte b = 0; b < byte.MaxValue; b++)
                        if (priorityQueues.ContainsKey(b))
                            i += priorityQueues[b].Count;
                    return i;
                }
                else
                {
                    return queuedPackets.Count;
                }
            }

            public int GetProcessedPacketsCount()
            {
                return processedPackets;
            }

            #endregion Debug Info
        }

        #endregion Channels

        #region Packet Handler

        public GenericCallback registerHandlersCallback = delegate () { };

        public delegate void PacketHandler(BinaryReader reader, CSteamID sentPlayer);

        public MemoryStream outStream;
        public MemoryStream inStream;

        public BinaryWriter packetWriter;
        public BinaryReader packetReader;

        private readonly Dictionary<int, byte[]> writeBuffers = new Dictionary<int, byte[]>();

        public void InitializePacketTools()
        {
            //Create buffers
            outStream = new MemoryStream(new byte[1100], true);
            inStream = new MemoryStream(new byte[1100], true);

            packetWriter = new BinaryWriter(outStream);
            packetReader = new BinaryReader(inStream);
            ClearPacketBuffers();

            //Create channels
        }

        private void RegisterDefaultChannels()
        {
            allChannels.Add(
                new PacketChannel(
                    "Game", //Used by Game Manager
                    0, //Channel Index
                    this, //Monkland Steam Manager
                    40, //Packets per update
                    200 //Max packets
                )
                {
                    requireGameState = false
                }
            );
            allChannels.Add(
                new PacketChannel(
                    "World", //Used by World Manager
                    1, //Channel Index
                    this, //Monkland Steam Manager
                    240, //Packets per update
                    1200 //Max packets
                )
                {
                    requireGameState = true
                }
            );
            allChannels.Add(
                new PacketChannel(
                    "Player", //Used by Player Manager
                    2, //Channel Index
                    this, //Monkland Steam Manager
                    120, //Packets per update
                    600 //Max packets
                )
                {
                    requireGameState = true
                }
            );
        }

        private void RegisterDefaultHandlers()
        {
            foreach (NetworkManager nm in netManagersList)
                nm.RegisterHandlers();
        }

        public byte RegisterHandler(int channel, PacketHandler handler)
        {
            return allChannels[channel].RegisterHandler(handler);
        }

        /// <summary>
        /// Resets the binarywriter used to create packets, and returns it so that we can write a new one.
        /// </summary>
        /// <returns></returns>
        public BinaryWriter GetWriterForPacket(DataPacket packet, byte forceData = 0)
        {
            packetWriter.Seek(0, SeekOrigin.Begin);

            packet.Write(packetWriter);
            packetWriter.Write(forceData);

            return packetWriter;
        }

        /// <summary>
        /// Resets the binaryreader used to read packets, and returns it so that we can write a new one.
        /// </summary>
        /// <returns></returns>
        public BinaryReader GetReaderForPacket(DataPacket packet, byte[] data)
        {
            packetReader.BaseStream.Seek(0, SeekOrigin.Begin);
            packetReader.BaseStream.Write(data, 0, data.Length);
            packetReader.BaseStream.Seek(0, SeekOrigin.Begin);

            packet.Read(packetReader);
            return packetReader;
        }

        //Gets a new packet with all the values set based on the channel's settings.
        public DataPacket GetNewPacket(int channel)
        {
            return GetNewPacket(channel, 0);
        }

        public DataPacket GetNewPacket(int channel, byte handlerID)
        {
            DataPacket newPacket = new DataPacket(channel, (CSteamID)playerID);
            PacketChannel packetChannel = allChannels[channel];

            newPacket.handlerID = handlerID;
            newPacket.isOrderedPacket = packetChannel.isOrdered;
            newPacket.isPriority = packetChannel.isPriorityBased;
            newPacket.scrapOldPackets = packetChannel.scrapOld;
            if (packetChannel.isOrdered)
                newPacket.packetID = packetChannel.GetNewID();
            return newPacket;
        }

        //Fills out the packet with the data from the BinaryWriter
        public void FinalizeWriterToPacket(BinaryWriter writer, DataPacket packet)
        {
            //Get the size of the packet
            int size = (int)writer.BaseStream.Position;
            //Get a buffer to write the data to
            byte[] buffer = GetWriteBufferOfSize(size);
            //Reset the position
            writer.BaseStream.Position = 0;

            //Write the data to the buffer
            writer.BaseStream.Read(buffer, 0, size);
            packet.data = buffer;
        }

        public void SendPacket(DataPacket toSend, CSteamID targetPlayer, EP2PSend sendType = EP2PSend.k_EP2PSendUnreliableNoDelay)
        {
            allChannels[toSend.channel].SendPacket(toSend, targetPlayer, sendType);
        }

        public void SendPacketToAll(DataPacket toSend, bool otherOnly = false, EP2PSend sendType = EP2PSend.k_EP2PSendUnreliableNoDelay)
        {
            allChannels[toSend.channel].SendPacketToAll(toSend, otherOnly, sendType);
        }

        public byte[] GetWriteBufferOfSize(int size)
        {
            if (!writeBuffers.ContainsKey(size))
            {
                writeBuffers[size] = new byte[size];
            }
            return writeBuffers[size];
        }

        public void ClearPacketBuffers()
        {
            writeBuffers.Clear();
        }

        /// <summary>
        /// The struct used to store packet data so it's easier to keep track of
        /// </summary>
        public class DataPacket
        {
            //The ID of the function used to parse this packet
            public byte handlerID;

            //If the packet is supposed to arrive in a specific order, this will be true
            public bool isOrderedPacket;

            //If the packet can be scrapped if it's out of order, this will be true
            public bool scrapOldPackets;

            //If the packet has a priority, this will be true
            public bool isPriority;

            //The packet's ID, so that ordered ones know which is which
            public uint packetID;

            //The priority of the pacekt
            public byte priority;

            //The channel this packet is from
            public int channel;

            //The player that sent this packet
            public CSteamID sentPlayer;

            //The whole byte array containing all the data
            public byte[] data;

            //Debug info
            public int readCount;

            public int HeapIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public DataPacket(int channel, CSteamID fromPlayer)
            {
                handlerID = 0;
                isOrderedPacket = false;
                scrapOldPackets = false;
                packetID = 0;
                this.channel = channel;
                this.sentPlayer = fromPlayer;
                data = null;
                isPriority = false;
                priority = 0;
            }

            //Writes the properties for the packet to the buffer
            public void Write(BinaryWriter writer)
            {
                writer.Write(handlerID);

                if (isOrderedPacket)
                {
                    writer.Write(packetID);
                }

                if (isPriority)
                {
                    writer.Write(priority);
                }
            }

            //Reads the properties for the packet from the buffer
            public void Read(BinaryReader reader)
            {
                handlerID = reader.ReadByte();

                if (isOrderedPacket)
                {
                    packetID = reader.ReadUInt32();
                }

                if (isPriority)
                {
                    priority = reader.ReadByte();
                }

                readCount = (int)reader.BaseStream.Position;
            }
        }

        #endregion Packet Handler

        #region NetworkManagers

        public static GameManager gameManager;
        public static WorldManager worldManager;
        public static PlayerManager playerManager;

        public static Dictionary<string, NetworkManager> netManagers = new Dictionary<string, NetworkManager>();
        private static readonly List<NetworkManager> netManagersList = new List<NetworkManager>();
        public GenericCallback registerManagersCallback = delegate () { };

        public void RegisterDefaultNetworkManagers()
        {
            gameManager = new GameManager();
            worldManager = new WorldManager();
            playerManager = new PlayerManager();

            RegisterNetworkManager("Game", gameManager);
            RegisterNetworkManager("World", worldManager);
            RegisterNetworkManager("Player", playerManager);
        }

        public int RegisterNetworkManager(string name, NetworkManager manager)
        {
            netManagersList.Add(manager);
            netManagers[name] = manager;
            return netManagersList.Count - 1;
        }

        public void UpdateManagers()
        {
            foreach (NetworkManager nm in netManagersList)
                nm.Update();
        }

        public void ResetManagers()
        {
            foreach (NetworkManager nm in netManagersList)
                nm.Reset();
        }

        public void PlayerJoinedManagers(ulong steamID)
        {
            foreach (NetworkManager nm in netManagersList)
                nm.PlayerJoined(steamID);
        }

        public void PlayerLeftManagers(ulong steamID)
        {
            foreach (NetworkManager nm in netManagersList)
                nm.PlayerLeft(steamID);
        }

        #endregion NetworkManagers

        public class LobbyInfo
        {
            public string version;

            public int memberLimit = 10;
            public int memberNum = 10;
            public CSteamID owner;
            public CSteamID ID;

            public LobbyInfo(CSteamID lobbyID)
            {
                this.ID = lobbyID;
                this.version = Monkland.VERSION;
                this.memberLimit = 10;
                this.memberNum = 10;
                this.owner = new CSteamID(0);
            }

            public bool UpdateLobbyInfo(CSteamID lobbyID)
            {
                bool success = true;
                if (lobbyID.m_SteamID == 0) // Invalid lobbyID
                { return false; }
                this.ID = lobbyID;
                string ownerData = SteamMatchmaking.GetLobbyData(lobbyID, "ManagerID");
                if (string.IsNullOrEmpty(ownerData))
                { success = false; }
                else
                { this.owner = new CSteamID(ulong.Parse(ownerData)); }
                if (owner.m_SteamID == 0)
                { success = false; }
                this.version = SteamMatchmaking.GetLobbyData(lobbyID, "Version");
                if (string.IsNullOrEmpty(this.version))
                {
                    this.version = "UNAVAILABLE";
                    success = false;
                }
                this.memberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
                this.memberNum = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
                return success;
            }
        }
    }
}
