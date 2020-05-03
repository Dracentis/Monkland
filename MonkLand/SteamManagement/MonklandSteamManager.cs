using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using Monkland.UI;
using Monkland.Patches;


namespace Monkland.SteamManagement {
    class MonklandSteamManager: MonoBehaviour {

        public static MonklandSteamManager instance;
        public static CSteamID lobbyID;
        public static SteamMultiplayerMenu menu;

        public const ulong appID = 312520;

        public static List<ulong> connectedPlayers = new List<ulong>(); // List of all players
        public static List<ulong> otherPlayers = new List<ulong>();// List of all players excluding this player

        public const string MANAGER_ID = "ManagerID";

        public static MonklandUI monklandUI;

        public static bool isInGame = false;

        public static void CreateManager() {
            GameObject gObject = new GameObject( "MonkManager" );
            gObject.AddComponent<MonklandSteamManager>();
            DontDestroyOnLoad( gObject );
        }

        public void Awake() {
            instance = this;

            RegisterCallbacks();
            InitializePacketTools();
        }

        public void Start()
        {
            createChannelsCallback += RegisterDefaultChannels;
            registerManagersCallback += RegisterDefaultNetworkManagers;
            registerHandlersCallback += RegisterDefaultHandlers;

            createChannelsCallback();
            registerManagersCallback();
            registerHandlersCallback();
        }

        public void Update() {
            ReadPackets();
            UpdateManagers();
        }

        public void CreateLobby() {
            SteamMatchmaking.CreateLobby( ELobbyType.k_ELobbyTypeFriendsOnly, 4 );
        }

        public void LeaveLobby() {
            if( lobbyID.m_SteamID != 0 )
                SteamMatchmaking.LeaveLobby( lobbyID );
            foreach( ulong player in otherPlayers ) {
                SteamNetworking.CloseP2PSessionWithUser( (CSteamID)player );
            }
            connectedPlayers.Clear();
            otherPlayers.Clear();
            ResetManagers();
            lobbyID = new CSteamID( 0 );
            isInGame = false;
        }

        public void OnGameExit() {
            if( monklandUI != null ) {
                monklandUI.ClearSprites();
                monklandUI = null;
            }
            LeaveLobby();
            ResetManagers();
        }

        #region Callbacks

        public delegate void GenericCallback();

        public Callback<LobbyCreated_t> lobbyCreated;
        public Callback<LobbyChatUpdate_t> lobbyUpdate;
        public Callback<LobbyChatMsg_t> lobbyChatMessage;
        public Callback<LobbyKicked_t> lobbyKicked;
        public Callback<LobbyEnter_t> lobbyEntered;
        public Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;

        public Callback<P2PSessionRequest_t> p2pRequest;
        public Callback<P2PSessionConnectFail_t> p2pConnectFail;

        public Callback<LobbyMatchList_t> matchList;

        public void RegisterCallbacks() {
            lobbyCreated = Callback<LobbyCreated_t>.Create( LobbyCreated );
            lobbyUpdate = Callback<LobbyChatUpdate_t>.Create( LobbyChatUpdated );
            lobbyChatMessage = Callback<LobbyChatMsg_t>.Create( LobbyChatMsg );
            lobbyKicked = Callback<LobbyKicked_t>.Create( LobbyKicked );
            lobbyEntered = Callback<LobbyEnter_t>.Create( LobbyEntered );
            lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create( LobbyJoinRequested );

            p2pRequest = Callback<P2PSessionRequest_t>.Create( P2PRequest );
            p2pConnectFail = Callback<P2PSessionConnectFail_t>.Create( P2PConnectionFail );
        }

        public void LobbyCreated(LobbyCreated_t result) {
            SteamMatchmaking.JoinLobby( (CSteamID)result.m_ulSteamIDLobby );
            MultiplayerChat.chatStrings.Add( "Created Lobby!" );

            SteamMatchmaking.SetLobbyData( (CSteamID)result.m_ulSteamIDLobby, MANAGER_ID, SteamUser.GetSteamID().ToString() );
        }
        public void LobbyChatUpdated(LobbyChatUpdate_t update) {
            try {
                EChatMemberStateChange change = (EChatMemberStateChange)update.m_rgfChatMemberStateChange;
                if( change == EChatMemberStateChange.k_EChatMemberStateChangeEntered ) {
                    MultiplayerChat.chatStrings.Add( string.Format( "User {0} joined the game!", SteamFriends.GetFriendPersonaName( new CSteamID( update.m_ulSteamIDUserChanged ) ) ) );
                    if (!connectedPlayers.Contains(update.m_ulSteamIDUserChanged))
                    {
                        PlayerJoinedManagers(update.m_ulSteamIDUserChanged);
                        connectedPlayers.Add(update.m_ulSteamIDUserChanged);

                    }
                    if ( update.m_ulSteamIDUserChanged != SteamUser.GetSteamID().m_SteamID ) {
                        otherPlayers.Add( update.m_ulSteamIDUserChanged );
                    }
                    
                }
                else if( change == EChatMemberStateChange.k_EChatMemberStateChangeLeft || change == EChatMemberStateChange.k_EChatMemberStateChangeKicked || change == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected ) {
                    MultiplayerChat.chatStrings.Add( string.Format( "User {0} left the game!", SteamFriends.GetFriendPersonaName( new CSteamID( update.m_ulSteamIDUserChanged ) ) ) );
                    PlayerLeftManagers(update.m_ulSteamIDUserChanged); 
                    connectedPlayers.Remove(update.m_ulSteamIDUserChanged);
                    MultiplayerPlayerList.RemovePlayerLabel( update.m_ulSteamIDUserChanged );
                    if( update.m_ulSteamIDUserChanged != SteamUser.GetSteamID().m_SteamID ) {
                        otherPlayers.Remove( update.m_ulSteamIDUserChanged );
                    }
                }
            } catch( System.Exception e ) {
                Debug.LogError( e );
            }
        }
        public void LobbyChatMsg(LobbyChatMsg_t message) {

        }

        public void LobbyKicked(LobbyKicked_t kickResult) {
            lobbyID = new CSteamID( 0 );
            LeaveLobby();
        }

        public void LobbyJoinRequested(GameLobbyJoinRequested_t request) {

            {
                patch_ProcessManager patchPM = ( (patch_ProcessManager)Patches.patch_Rainworld.mainRW.processManager );
                if( patchPM.currentMainLoop is SteamMultiplayerMenu ) {
                    if( lobbyID.m_SteamID != 0 ) {
                        LeaveLobby();
                    }
                }
                patchPM.ImmediateSwitchCustom( new SteamMultiplayerMenu( patchPM ) );

            }

            SteamMatchmaking.JoinLobby( request.m_steamIDLobby );
        }
        public void LobbyEntered(LobbyEnter_t enterLobby) {
            connectedPlayers.Clear();
            otherPlayers.Clear();
            ResetManagers();

            lobbyID = (CSteamID)enterLobby.m_ulSteamIDLobby;
            int playerCount = SteamMatchmaking.GetNumLobbyMembers( lobbyID );
            MultiplayerChat.chatStrings.Add( "Entered Lobby!" );


            //Send packets to all players, to establish P2P connections with them
            if( playerCount > 1 ) {
                for( int i = 0; i < playerCount; i++ ) {
                    CSteamID lobbyMember = SteamMatchmaking.GetLobbyMemberByIndex( lobbyID, i );
                    SteamNetworking.SendP2PPacket( lobbyMember, new byte[] { 255 }, 1, EP2PSend.k_EP2PSendReliable, 0 );
                    if (!connectedPlayers.Contains(lobbyMember.m_SteamID))
                    {
                        connectedPlayers.Add(lobbyMember.m_SteamID);
                        PlayerJoinedManagers(lobbyMember.m_SteamID);
                    }
                    if ( lobbyMember != SteamUser.GetSteamID() )
                        otherPlayers.Add( lobbyMember.m_SteamID );
                }
            }

            //Set up network data
            NetworkGameManager.managerID = ulong.Parse( SteamMatchmaking.GetLobbyData( lobbyID, MANAGER_ID ) );
            NetworkGameManager.playerID = SteamUser.GetSteamID().m_SteamID;

            if (!connectedPlayers.Contains(SteamUser.GetSteamID().m_SteamID))
            {
                connectedPlayers.Add(SteamUser.GetSteamID().m_SteamID);
                PlayerJoinedManagers(SteamUser.GetSteamID().m_SteamID);
            }


            MultiplayerChat.chatStrings.Add( "This game's manager is " + SteamFriends.GetFriendPersonaName( (CSteamID)NetworkGameManager.managerID ) );
            isInGame = true;
        }

        public void P2PRequest(P2PSessionRequest_t request) {
            if( connectedPlayers.Contains( request.m_steamIDRemote.m_SteamID ) )
                SteamNetworking.AcceptP2PSessionWithUser( request.m_steamIDRemote );
            P2PEstablishedManagers(request.m_steamIDRemote.m_SteamID);
        }
        public void P2PConnectionFail(P2PSessionConnectFail_t failResult) {
            Debug.LogError( "P2P Error:" + ( (EP2PSessionError)failResult.m_eP2PSessionError ) );
        }


        #endregion

        #region Channels

        public GenericCallback createChannelsCallback = delegate () { };

        public List<PacketChannel> allChannels = new List<PacketChannel>();

        public void ReadPackets(bool forceRead = false) {
            foreach( PacketChannel pc in allChannels ) {
                pc.ReadPackets( forceRead );
            }
        }

        public void ClearAllPackets() {
            foreach( PacketChannel p in allChannels )
                p.Clear();
        }

        public class PacketChannel {

            //The name of this channel
            public string channelName = "CHANNEL";

            //The channel to send packets on over steam
            public int channelIndex = 0;

            //The steam manager
            public MonklandSteamManager manager;

            public PacketChannel(string name, int channelIndex, MonklandSteamManager manager, int packetsPerUpdate = 20, int maxPackets = 100, bool priorityBased = false, bool ordered = false, bool scrapOldOrdered = false) {
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
            //The ID of the past packet, from each player. Keep things in order!
            public Dictionary<ulong, uint> lastPacketID;
            //True if this channel should sort packets based on priority
            public bool isPriorityBased = false;
            //True if this channel's packets are ordered
            public bool isOrdered = false;
            //True if the channel's old ordered packets should be scrapped
            public bool scrapOld = false;
            public bool requireGameState = true;

            //The amount of packets we can process per update
            public int packetsPerUpdate = 20;

            //The max amount of packets the channel is allowed to store at once
            private int maxPackets = 100;

            //No-priority packet
            private Queue<DataPacket> queuedPackets = new Queue<DataPacket>();

            //Priority packets
            private Dictionary<byte, Queue<DataPacket>> priorityQueues = new Dictionary<byte, Queue<DataPacket>>();

            public bool isForceWait = false;
            public ulong replyValue = 0;

            //Reads packets from steam
            public void ReadPackets(bool forceRead = false) {

                if( forceRead == false && requireGameState ) {
                    if( patch_RainWorldGame.mainGame == null || patch_RainWorldGame.mainGame.Players.Count == 0 ) {
                        return;
                    }
                }

                //Size of the packet
                uint size = 0;
                //The number of packets we have left to process
                int packetsLeft = packetsPerUpdate;
                //The data in the packet
                byte[] packetData;
                //The user who sent the packet
                CSteamID sentUser;
                BinaryReader reader;

                //Read all packets to the queues
                while( SteamNetworking.IsP2PPacketAvailable( out size, channelIndex ) ) {

                    try {
                        //Initialize array for packet
                        packetData = new byte[size];
                        //Read the packet from steam's network
                        SteamNetworking.ReadP2PPacket( packetData, size, out size, out sentUser, channelIndex );

                        if( size == 1 )
                            continue;

                        //Create the sctruct to store the packet
                        DataPacket newPacket = new DataPacket( channelIndex, sentUser );

                        newPacket.isOrderedPacket = isOrdered;
                        newPacket.scrapOldPackets = scrapOld;
                        newPacket.isPriority = isPriorityBased;

                        newPacket.data = packetData;

                        if( isPriorityBased ) {
                            if( !priorityQueues.ContainsKey( newPacket.priority ) ) {
                                Debug.Log( "Creating queue for priority " + newPacket.priority );
                                priorityQueues[newPacket.priority] = new Queue<DataPacket>();
                            }
                            priorityQueues[newPacket.priority].Enqueue( newPacket );
                        } else {
                            queuedPackets.Enqueue( newPacket );
                        }
                    } catch( System.Exception e ) {
                        Debug.LogError( e );
                    }
                }

                //Discard old packets, then process as many as we can.
                if( isPriorityBased ) {

                    int skipCount = 0;
                    int ditchedCount = 0;
                    //Loop through all priorities in order
                    for( byte priorityI = 0; packetsLeft > 0 && priorityI < byte.MaxValue; priorityI++ ) {
                        //If there's no packets of this priority, skip
                        if( !priorityQueues.ContainsKey( priorityI ) ) {
                            skipCount++;
                            continue;
                        }

                        //Gets the packets that need to be processed from this priority
                        Queue<DataPacket> packetQueue = priorityQueues[priorityI];

                        //Throws out packets if there's too many. This could be bad. Let's try to avoid this.
                        while( packetQueue.Count > maxPackets ) {
                            packetQueue.Dequeue();
                            ditchedCount++;
                        }

                        //Finally, process as many packets as we can from this priority level.
                        while( packetsLeft > 0 && packetQueue.Count > 0 ) {

                            DataPacket getPacket = packetQueue.Dequeue();

                            try {
                                if( !ProcessPacket( getPacket ) )
                                    packetsLeft--;

                            } catch( System.Exception e ) {
                                StringBuilder sb = new StringBuilder();
                                for( int j = 0; j < getPacket.data.Length; j++ )
                                    sb.Append( getPacket.data[j] + " " + ( ( getPacket.readCount == j ) ? "|" : "" ) );

                                Debug.LogError( "Packet data was " + sb.ToString() + " in channel " + channelName );
                                Debug.LogError( e );
                                packetsLeft--;
                            }
                        }
                    }

                } else {
                    //Throws out packets if there's too many. This could be bad. Let's try to avoid this.
                    while( queuedPackets.Count > maxPackets )
                        queuedPackets.Dequeue();

                    //Process as many packets as we can from this priority level.
                    while( packetsLeft > 0 && queuedPackets.Count > 0 ) {
                        DataPacket getPacket = queuedPackets.Dequeue();
                        try {
                            if( !ProcessPacket( getPacket ) )
                                packetsLeft--;
                        } catch( System.Exception e ) {

                            StringBuilder sb = new StringBuilder();
                            for( int i = 0; i < getPacket.data.Length; i++ )
                                sb.Append( getPacket.data[i] + "" + ( ( getPacket.readCount == i ) ? "|" : "" ) );

                            Debug.LogError( "Packet data was " + sb.ToString() + " in channel " + channelName );
                            Debug.LogError( e );
                            packetsLeft--;
                        }
                    }
                }

            }

            public void Clear() {
                queuedPackets.Clear();
                foreach( Queue<DataPacket> packs in priorityQueues.Values )
                    packs.Clear();
                priorityQueues.Clear();
            }

            public bool ProcessPacket(DataPacket packet) {
                try {
                    processedPackets++;
                    //If packets are ordered
                    if( isOrdered ) {
                        //If the last packet we got has a higher ID than this one, then we're 
                        if( lastPacketID.ContainsKey( packet.sentPlayer.m_SteamID ) && lastPacketID[packet.sentPlayer.m_SteamID] > packet.packetID )
                            return true;
                        lastPacketID[packet.sentPlayer.m_SteamID] = packet.packetID;
                    }

                    BinaryReader br = manager.GetReaderForPacket( packet, packet.data );

                    byte forcewaitData = br.ReadByte();

                    if( forcewaitData == 1 ) {
                        RecievedForcewaitPacket( br, packet.sentPlayer );
                        return true;
                    } else if( forcewaitData == 2 ) {
                        RecieveForcewaitReply( br, packet.sentPlayer );
                        return true;
                    } else {
                        handlers[packet.handlerID]( br, packet.sentPlayer );
                    }
                    return false;
                } catch(System.Exception e ) {
                    StringBuilder sb = new StringBuilder();
                    for( int j = 0; j < packet.data.Length; j++ )
                        sb.Append( packet.data[j] + " " + ( ( packet.readCount == j ) ? "|" : "" ) );

                    Debug.LogError( "Packet data was " + sb.ToString() + " in channel " + channelName );
                    Debug.LogError( e );

                    return false;
                }

            }

            public void SendPacketToAll(DataPacket packet, bool othersOnly = false, EP2PSend sendType = EP2PSend.k_EP2PSendUnreliableNoDelay) {
                if( othersOnly ) {
                    foreach( CSteamID id in otherPlayers )
                        SendPacket( packet, id, sendType );
                } else {
                    foreach( CSteamID id in connectedPlayers )
                        SendPacket( packet, id, sendType );
                }
            }
            public void SendPacket(DataPacket packet, CSteamID user, EP2PSend sendType = EP2PSend.k_EP2PSendUnreliableNoDelay) {

                /*
                StringBuilder sb = new StringBuilder();
                for( int i = 0; i < packet.data.Length; i++ )
                    sb.Append( packet.data[i] + "|" );
                Debug.Log( "Sending packet to player " + SteamFriends.GetFriendPersonaName( user ) + " with data " + sb.ToString() );
                Debug.Log( Environment.StackTrace );*/

                if( user.m_SteamID == NetworkGameManager.playerID ) {
                    //Process packet immediately if we sent it to ourself.
                    ProcessPacket( packet );
                } else {
                    //Send it off to another player
                    SteamNetworking.SendP2PPacket( user, packet.data, (uint)packet.data.Length, sendType, channelIndex );
                }
            }

            public ulong SendForcewaitPacket(ulong id, CSteamID user) {

                //Tag this to be waiting
                isForceWait = true;
                //Send packet
                {
                    MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket( channelIndex, 0 );
                    packet.priority = 0;
                    BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket( packet, 1 );
                    writer.Write( id );
                    MonklandSteamManager.instance.FinalizeWriterToPacket( writer, packet );
                    MonklandSteamManager.instance.SendPacket( packet, user, EP2PSend.k_EP2PSendReliable );
                }

                System.DateTime startTime = System.DateTime.Now;

                //As long as we're supposed to wait
                while( isForceWait ) {
                    //Read all packets
                    ReadPackets( true );
                    //Sleep for 10 ms
                    System.Threading.Thread.Sleep( 10 );

                    //If it's been 30 seconds and we haven't gotten a reply, return 0
                    if( ( startTime - System.DateTime.Now ).TotalSeconds > 30 )
                        return 0;
                }

                return replyValue;
            }
            public void SendForcewaitReply(ulong response, CSteamID user) {
                MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket( channelIndex, 0 );
                packet.priority = 0;
                BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket( packet, 2 );
                writer.Write( response );
                MonklandSteamManager.instance.FinalizeWriterToPacket( writer, packet );
                MonklandSteamManager.instance.SendPacket( packet, user, EP2PSend.k_EP2PSendReliable );
            }

            public void RecieveForcewaitReply(BinaryReader reader, CSteamID sender) {
                isForceWait = false;
                replyValue = reader.ReadUInt64();
            }
            public void RecievedForcewaitPacket(BinaryReader reader, CSteamID sender) {
                int id = reader.ReadInt32();

                switch( id ) {
                    case 0:
                    SendForcewaitReply( (ulong)patch_RainWorldGame.mainGame.Players.Count, sender );
                    return;
                }
            }

            public uint GetNewID() {
                return currentID++;
            }

            #endregion

            #region Handlers

            public List<PacketHandler> handlers = new List<PacketHandler>();

            public byte RegisterHandler(PacketHandler handler) {
                if( handlers.Count == 255 ) {
                    throw new Exception( "Already registered max number of handlers in channel " + channelName );
                }
                handlers.Add( handler );
                return (byte)( handlers.Count - 1 );
            }

            #endregion

            #region Debug Info

            private int processedPackets;

            public int GetPacketCount() {
                if( isPriorityBased ) {
                    int i = 0;
                    for( byte b = 0; b < byte.MaxValue; b++ )
                        if( priorityQueues.ContainsKey( b ) )
                            i += priorityQueues[b].Count;
                    return i;
                } else {
                    return queuedPackets.Count;
                }
            }

            public int GetProcessedPacketsCount() {
                return processedPackets;
            }

            #endregion

        }

        #endregion

        #region Packet Handler

        public GenericCallback registerHandlersCallback = delegate () { };

        public delegate void PacketHandler(BinaryReader reader, CSteamID sentPlayer);

        public MemoryStream outStream;
        public MemoryStream inStream;

        public BinaryWriter packetWriter;
        public BinaryReader packetReader;

        private Dictionary<int, byte[]> writeBuffers = new Dictionary<int, byte[]>();

        public void InitializePacketTools() {
            //Create buffers
            outStream = new MemoryStream( new byte[1100], true );
            inStream = new MemoryStream( new byte[1100], true );

            packetWriter = new BinaryWriter( outStream );
            packetReader = new BinaryReader( inStream );
            ClearPacketBuffers();

            //Create channels
        }

        private void RegisterDefaultChannels() {
            allChannels.Add(
                new PacketChannel(
                    "Default Channel", //Channel name
                    0, //Channel Index
                    this, //Monkland Steam Manager
                    50, //Packets per update
                    200 //Max packets
                ) {
                    requireGameState = false
                }
            );
            allChannels.Add(
                new PacketChannel(
                    "Game Management", //Channel name
                    1, //Channel Index
                    this, //Monkland Steam Manager
                    100, //Packets per update
                    300, //Max packets
                    true
                )
            );
            allChannels.Add(
                new PacketChannel(
                    "Entity Updates", //Channel name
                    2, //Channel Index
                    this, //Monkland Steam Manager
                    100, //Packets per update
                    600, //Max packets
                    true
                )
            );

        }
        private void RegisterDefaultHandlers() {
            foreach( NetworkManager nm in netManagersList )
                nm.RegisterHandlers();
        }

        public byte RegisterHandler(int channel, PacketHandler handler) {
            return allChannels[channel].RegisterHandler( handler );
        }
    
        /// <summary>
        /// Resets the binarywriter used to create packets, and returns it so that we can write a new one.
        /// </summary>
        /// <returns></returns>
        public BinaryWriter GetWriterForPacket(DataPacket packet, byte forceData = 0) {
            packetWriter.Seek( 0, SeekOrigin.Begin );

            packet.Write( packetWriter );
            packetWriter.Write( forceData );

            return packetWriter;
        }

        /// <summary>
        /// Resets the binaryreader used to read packets, and returns it so that we can write a new one.
        /// </summary>
        /// <returns></returns>
        public BinaryReader GetReaderForPacket(DataPacket packet, byte[] data) {
            packetReader.BaseStream.Seek( 0, SeekOrigin.Begin );
            packetReader.BaseStream.Write( data, 0, data.Length );
            packetReader.BaseStream.Seek( 0, SeekOrigin.Begin );

            packet.Read( packetReader );
            return packetReader;
        }

        //Gets a new packet with all the values set based on the channel's settings.
        public DataPacket GetNewPacket(int channel) {
            return GetNewPacket( channel, 0 );
        }
        public DataPacket GetNewPacket(int channel, byte handlerID) {
            DataPacket newPacket = new DataPacket( channel, (CSteamID)NetworkGameManager.playerID );
            PacketChannel packetChannel = allChannels[channel];

            newPacket.handlerID = handlerID;
            newPacket.isOrderedPacket = packetChannel.isOrdered;
            newPacket.isPriority = packetChannel.isPriorityBased;
            newPacket.scrapOldPackets = packetChannel.scrapOld;
            if( packetChannel.isOrdered )
                newPacket.packetID = packetChannel.GetNewID();
            return newPacket;
        }

        //Fills out the packet with the data from the BinaryWriter
        public void FinalizeWriterToPacket(BinaryWriter writer, DataPacket packet) {
            //Get the size of the packet
            int size = (int)writer.BaseStream.Position;
            //Get a buffer to write the data to
            byte[] buffer = GetWriteBufferOfSize( size );
            //Reset the position
            writer.BaseStream.Position = 0;

            //Write the data to the buffer
            writer.BaseStream.Read( buffer, 0, size );
            packet.data = buffer;
        }

        public void SendPacket(DataPacket toSend, CSteamID targetPlayer, EP2PSend sendType = EP2PSend.k_EP2PSendUnreliableNoDelay) {
            allChannels[toSend.channel].SendPacket( toSend, targetPlayer, sendType );
        }
        public void SendPacketToAll(DataPacket toSend, bool otherOnly = false, EP2PSend sendType = EP2PSend.k_EP2PSendUnreliableNoDelay) {
            allChannels[toSend.channel].SendPacketToAll( toSend, otherOnly, sendType );
        }

        public byte[] GetWriteBufferOfSize(int size) {
            if( !writeBuffers.ContainsKey( size ) ) {
                writeBuffers[size] = new byte[size];
            }
            return writeBuffers[size];
        }
        public void ClearPacketBuffers() {
            writeBuffers.Clear();
        }

        /// <summary>
        /// The struct used to store packet data so it's easier to keep track of
        /// </summary>
        public class DataPacket {

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

            public DataPacket(int channel, CSteamID fromPlayer) {
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
            public void Write(BinaryWriter writer) {
                writer.Write( handlerID );

                if( isOrderedPacket ) {
                    writer.Write( packetID );
                }

                if( isPriority ) {
                    writer.Write( priority );
                }
            }

            //Reads the properties for the packet from the buffer
            public void Read(BinaryReader reader) {

                handlerID = reader.ReadByte();

                if( isOrderedPacket ) {
                    packetID = reader.ReadUInt32();
                }

                if( isPriority ) {
                    priority = reader.ReadByte();
                }

                readCount = (int)reader.BaseStream.Position;
            }
        }


        #endregion

        #region NetworkManagers

        public static NetworkGameManager GameManager;
        //public static NetworkRoomManager RoomManager;
        //public static NetworkObjectManager ObjectManager;

        public static Dictionary<string, NetworkManager> netManagers = new Dictionary<string, NetworkManager>();
        private static List<NetworkManager> netManagersList = new List<NetworkManager>();
        public GenericCallback registerManagersCallback = delegate () { };

        public void RegisterDefaultNetworkManagers() {
            GameManager = new NetworkGameManager();
            //RoomManager = new NetworkRoomManager();
            //ObjectManager = new NetworkObjectManager();

            RegisterNetworkManager( "Game", GameManager );
            //RegisterNetworkManager( "Room", RoomManager );
            //RegisterNetworkManager( "Object", ObjectManager );
        }

        public int RegisterNetworkManager(string name, NetworkManager manager) {
            netManagersList.Add( manager );
            netManagers[name] = manager;
            return netManagersList.Count - 1;
        }

        public void UpdateManagers() {
            foreach( NetworkManager nm in netManagersList )
                nm.Update();
        }

        public void ResetManagers() {
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
        
        public void P2PEstablishedManagers(ulong steamID)
        {
            foreach (NetworkManager nm in netManagersList)
                nm.P2PEstablished(steamID);
        }

        #endregion

    }
}