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
                case (byte)PacketType.Player:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadPlayer(br, sentPlayer);
                    }
                    return;
            }
        }

        #endregion

        #region Outgoing Packets

        public void Send(PhysicalObject physicalObject, List<ulong> targets)
        {
            try
            {
                if (physicalObject == null || targets == null || targets.Count == 0 || physicalObject.room == null || physicalObject.room.abstractRoom == null)
                    return;
                MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
                BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

                if ((physicalObject as PhysicalObject) is Player)
                {
                    writer.Write((byte)PacketType.Player);
                    writer.Write(physicalObject.room.abstractRoom.name);
                    writer.Write((physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
                    WorldCoordinateHandler.Write(physicalObject.abstractPhysicalObject.pos, ref writer);
                    PlayerHandler.Write(physicalObject as PhysicalObject as Player, ref writer);
                }
                else
                {
                    return;
                }

                MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
                foreach (ulong target in targets)
                {
                    if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                    {
                        if (target != playerID)
                            MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendUnreliableNoDelay);
                    }
                }
            }catch(Exception e)
            {
                Debug.LogError(e);
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
            Player
        }

        #endregion

        #region Incomming Packets

        public void ReadPlayer(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning)
                return;
            string roomName = br.ReadString();//Read Room Name
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
                return;
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null)
                return;
            int dist = br.ReadInt32();// Read Disinguisher
            WorldCoordinate startPos = WorldCoordinateHandler.Read(ref br);// Read World Coordinate
            foreach (AbstractCreature cr in abstractRoom.creatures)
            {
                if (((cr as AbstractPhysicalObject) as patch_AbstractPhysicalObject).dist == dist && cr.realizedCreature != null)
                {
                    cr.realizedCreature = PlayerHandler.Read((cr.realizedCreature as Player), ref br);// Read Player
                    return;
                }
            }
            startPos.room = abstractRoom.index;
            startPos.abstractNode = -1;
            AbstractCreature abstractCreature = new AbstractCreature(patch_RainWorldGame.mainGame.world, StaticWorld.GetCreatureTemplate("Slugcat"), null, startPos, new EntityID(-1, dist));
            abstractCreature.state = new PlayerState(abstractCreature, 1, 0, false);
            ((abstractCreature as AbstractPhysicalObject) as patch_AbstractPhysicalObject).dist = dist;
            ((abstractCreature as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner = sentPlayer.m_SteamID;
            patch_RainWorldGame.mainGame.world.GetAbstractRoom(abstractCreature.pos.room).AddEntity(abstractCreature);
            abstractCreature.RealizeInRoom();
            abstractCreature.realizedCreature = PlayerHandler.Read((abstractCreature.realizedCreature as Player), ref br);// Read Player
        }

        #endregion
    }

}
