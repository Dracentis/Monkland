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
                case (byte)PacketType.IntVector2:
                    IntVector2 intVector2 = IntVector2Handler.Read(ref br);
                    return;
                case (byte)PacketType.Vector2:
                    Vector2 vector2 = Vector2Handler.Read(ref br);
                    return;
            }
        }

        #endregion

        #region Outgoing Packets

        public void Send(patch_PhysicalObject physicalObject, List<ulong> targets)
        {
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            if (physicalObject is patch_PhysicalObject)
            {
                writer.Write((byte)PacketType.PhyscialObject);
            }else if (physicalObject is Creature)
            {
                writer.Write((byte)PacketType.Creature);
            }
            else
            {
                return;
            }

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in targets)
            {
                MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendUnreliableNoDelay);
            }
        }

        public enum PacketType
        {
            IntVector2,
            Vector2,
            WorldCoordinate,
            AbstractPhysicalObject,
            PhyscialObject,
            BodyChunk,
            AbstractCreature,
            Creature,
            Grasp,
        }

        #endregion
    }

}
