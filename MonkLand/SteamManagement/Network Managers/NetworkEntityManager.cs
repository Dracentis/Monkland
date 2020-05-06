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
    class NetworkEntityManager : NetworkManager
    {
        private static ulong playerID
        {
            get
            {
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

        public const int CHANNEL = 2;

        public byte UtilityHandler = 0;

        public override void Update()
        {
        }

        public override void Reset()
        {
        }

        public override void PlayerJoined(ulong steamID)
        {

        }

        public override void PlayerLeft(ulong steamID)
        {

        }

        #region Packet Handler

        public override void RegisterHandlers()
        {
            UtilityHandler = MonklandSteamManager.instance.RegisterHandler(CHANNEL, HandlePackets);
        }

        public void HandlePackets(BinaryReader br, CSteamID sentPlayer)
        {
            byte messageType = br.ReadByte();
            switch (messageType)// up to 256 message types
            {
                case 0:// World Loaded or Exited
                    Read(br, sentPlayer);
                    return;
                case 1:// Rain Sync
                    Read(br, sentPlayer);
                    return;
                case 2:// Realize Room
                    Read(br, sentPlayer);
                    return;
                case 3:// Abstractize Room
                    Read(br, sentPlayer);
                    return;
            }
        }

        #endregion

        #region Outgoing Packets

        public void Send()
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            //Write message type
            writer.Write(Convert.ToByte(2));
            writer.Write(1.0f);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            //MonklandSteamManager.instance.SendPacket(packet, (CSteamID)managerID), EP2PSend.k_EP2PSendReliable);
            MonklandSteamManager.instance.SendPacketToAll(packet, true, EP2PSend.k_EP2PSendReliable);
        }

        #endregion

        #region Incoming Packets

        public void Read(BinaryReader reader, CSteamID sent)
        {
            ulong from = sent.m_SteamID;
            reader.ReadBoolean();
        }

        #endregion
    }

}
