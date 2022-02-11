using Monkland.Hooks;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkWorldManager : NetworkManager
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

        public byte WorldHandler = 0;

        private enum WorldPacketType
        {
            NullPacket
        }

        #region Packet Handler

        public override void RegisterHandlers()
        {
            WorldHandler = MonklandSteamManager.instance.RegisterHandler(WORLD_CHANNEL, HandlePackets);
        }

        public void HandlePackets(BinaryReader br, CSteamID sentPlayer)
        {
            WorldPacketType messageType = (WorldPacketType)br.ReadByte();
            switch (messageType)// up to 256 message types
            {
                case WorldPacketType.NullPacket:// Placeholder
                    //ReadLoadPacket(br, sentPlayer);
                    return;
            }
        }

        #endregion Packet Handler

        #region Outgoing Packets

        #endregion Outgoing Packets

        #region Incoming Packets

        #endregion Incoming Packets
    }
}