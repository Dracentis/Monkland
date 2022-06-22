using Monkland.Hooks;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class PlayerManager : NetworkManager
    {
        private enum PlayerPacketType
        {
            PlayerMovement,
            EstablishGrasp,
            SwitchGrasps,
            ReleaseGrasp,
            Hit,
            Throw,
            Stick,
            Deactivate,
            Violence
        }

        #region Packet Handler

        public override void HandlePackets(BinaryReader br, CSteamID sentPlayer)
        {
            PlayerPacketType messageType = (PlayerPacketType)br.ReadByte();
            switch (messageType)// up to 256 message types
            {
                case PlayerPacketType.PlayerMovement:
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