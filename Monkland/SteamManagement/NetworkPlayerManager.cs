using Monkland.Hooks;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkPlayerManager : NetworkManager
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

        public byte PlayerHandler = 0;

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

        public override void RegisterHandlers()
        {
            PlayerHandler = MonklandSteamManager.instance.RegisterHandler(PLAYER_CHANNEL, HandlePackets);
        }

        public void HandlePackets(BinaryReader br, CSteamID sentPlayer)
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