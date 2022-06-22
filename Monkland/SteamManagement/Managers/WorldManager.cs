using Monkland.Hooks;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class WorldManager : NetworkManager
    {
        private enum WorldPacketType
        {
            NullPacket
        }

        #region Packet Handler

        public override void HandlePackets(BinaryReader br, CSteamID sentPlayer)
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