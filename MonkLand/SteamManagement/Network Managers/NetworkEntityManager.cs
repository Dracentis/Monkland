using System;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using UnityEngine;
using RWCustom;
using System.IO;
using Monkland.Patches;
using System.Runtime.CompilerServices;
using Monkland.UI;

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
                case (byte)PacketType.Rock:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadRock(br, sentPlayer);
                    }
                    return;
                case (byte)PacketType.EstablishGrasp:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadGrab(br, sentPlayer);
                    }
                    return;
                case (byte)PacketType.SwitchGrasps:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadSwitch(br, sentPlayer);
                    }
                    return;
                case (byte)PacketType.ReleaseGrasp:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadRelease(br, sentPlayer);
                    }
                    return;
                case (byte)PacketType.Hit:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadHit(br, sentPlayer);
                    }
                    return;
                case (byte)PacketType.Throw:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadThrow(br, sentPlayer);
                    }
                    return;
                case (byte)PacketType.Stick:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadStick(br, sentPlayer);
                    }
                    return;
                case (byte)PacketType.Deactivate:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadDeactivate(br, sentPlayer);
                    }
                    return;
                case (byte)PacketType.Spear:
                    if (patch_RainWorldGame.mainGame != null)
                    {
                        ReadSpear(br, sentPlayer);
                    }
                    return;
            }
        }

        #endregion

        public bool isSynced(PhysicalObject obj)
        {
            if (obj is Player)
                return true;
            if (obj is Rock)
                return true;
            if (obj is Spear)
                return true;
            return false;
        }

        public bool isSynced(AbstractPhysicalObject obj)
        {
            if (obj is AbstractCreature && (obj as AbstractCreature).creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Slugcat)
                return true;
            if (obj.type == AbstractPhysicalObject.AbstractObjectType.Rock)
                return true;
            if (obj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                return true;
            return false;
        }

        #region Outgoing Packets

        public void Send(PhysicalObject physicalObject, List<ulong> targets, bool reliable = false)
        {
            try
            {
                if (physicalObject == null || targets == null || targets.Count == 0 || physicalObject.room == null || physicalObject.room.abstractRoom == null || !isSynced(physicalObject))
                    return;
                MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
                BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

                if (physicalObject is Player)
                {
                    writer.Write((byte)PacketType.Player);
                    writer.Write(physicalObject.room.abstractRoom.name);
                    writer.Write((physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
                    WorldCoordinateHandler.Write(physicalObject.abstractPhysicalObject.pos, ref writer);
                    PlayerHandler.Write(physicalObject as PhysicalObject as Player, ref writer);
                    if (MonklandSteamManager.DEBUG)
                        MonklandUI.UpdateMessage("Player\nOwner:" + (physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).owner + "\nID:" + (physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist, 1, physicalObject.bodyChunks[0].pos, (physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist, physicalObject.room.abstractRoom.index, (physicalObject as patch_Player).ShortCutColor());
                }
                else if (physicalObject is Rock)
                {
                    writer.Write((byte)PacketType.Rock);
                    writer.Write(physicalObject.room.abstractRoom.name);
                    WorldCoordinateHandler.Write(physicalObject.abstractPhysicalObject.pos, ref writer);
                    EntityIDHandler.Write(physicalObject.abstractPhysicalObject.ID, ref writer);
                    WeaponHandler.Write(physicalObject as Rock, ref writer);
                    reliable = true;
                    if (MonklandSteamManager.DEBUG)
                        MonklandUI.UpdateMessage("Rock\nO:" + ((physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).owner%10000) + "\nID:" + ((physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist % 10000), 1, physicalObject.bodyChunks[0].pos, (physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist, physicalObject.room.abstractRoom.index, Color.white);
                }
                else if (physicalObject is Spear)
                {
                    writer.Write((byte)PacketType.Spear);
                    writer.Write(physicalObject.room.abstractRoom.name);
                    WorldCoordinateHandler.Write(physicalObject.abstractPhysicalObject.pos, ref writer);
                    EntityIDHandler.Write(physicalObject.abstractPhysicalObject.ID, ref writer);
                    SpearHandler.Write(physicalObject as Spear, ref writer);
                    reliable = true;
                    if (MonklandSteamManager.DEBUG)
                        MonklandUI.UpdateMessage("Spear\nO:" + ((physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).owner % 10000) + "\nID:" + ((physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist % 10000), 1, physicalObject.bodyChunks[0].pos, (physicalObject.abstractPhysicalObject as patch_AbstractPhysicalObject).dist, physicalObject.room.abstractRoom.index, Color.white);
                }
                else
                {
                    return;
                }

                MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
                foreach (ulong target in targets)
                {
                    if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target) && target != playerID)
                    {
                        if (reliable)
                        {
                            MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                        }
                        else
                        {
                            MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendUnreliableNoDelay);
                        }
                    }
                }
            }catch(Exception e)
            {
                Debug.LogError(e);
            }
        }

        #region Grasps and Sticks
        public void SendGrab(Creature.Grasp grasp)
        {
            if (grasp == null || grasp.grabber == null || grasp.grabbed == null || !isSynced(grasp.grabber) || !isSynced(grasp.grabbed) || (grasp.grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID)
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grasp.grabber.room.abstractRoom.name))
                return;
            MonklandSteamManager.Log("[Entity] Sending Grab: " + (grasp.grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " grabbed " + (grasp.grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.EstablishGrasp);
            writer.Write(grasp.grabber.room.abstractRoom.name);
            writer.Write((grasp.grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            writer.Write((grasp.grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            writer.Write(grasp.graspUsed);
            writer.Write(grasp.grabbedChunk.index);
            writer.Write((int)grasp.shareability);
            writer.Write(grasp.dominance);
            writer.Write(grasp.pacifying);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[grasp.grabber.room.abstractRoom.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendUnreliableNoDelay);
                }
            }
            if ((grasp.grabbed is Player) && !(grasp.grabber is Player))//One of our creatures is grabbing another player so the grabber should belong to the grabbed
            {
                (grasp.grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).owner = (grasp.grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).owner;
            }
            else if (!(grasp.grabbed is Player))//One of our creatures is the grabber so we should take ownership of the grabbed
            {
                (grasp.grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).owner = (grasp.grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).owner;
            }
        }

        public void SendSwitch(Creature grabber, int from, int to)
        {
            if (grabber == null || !isSynced(grabber) || (grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID)
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grabber.room.abstractRoom.name))
                return;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.SwitchGrasps);
            writer.Write(grabber.room.abstractRoom.name);
            writer.Write((grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            writer.Write(from);
            writer.Write(to);
            MonklandSteamManager.Log("[Entity] Sending Switch: " + (grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " switched " + from + ", " + to);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[grabber.room.abstractRoom.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                }
            }

        }

        public void SendRelease(Creature.Grasp grasp)
        {
            if (grasp == null || grasp.grabber == null || grasp.grabbed == null || !isSynced(grasp.grabber) || !isSynced(grasp.grabbed) || (grasp.grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID)
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grasp.grabber.room.abstractRoom.name))
                return;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.ReleaseGrasp);
            writer.Write(grasp.grabber.room.abstractRoom.name);
            writer.Write((grasp.grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            writer.Write((grasp.grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.Log("[Entity] Sending Release: " + (grasp.grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " released " + (grasp.grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[grasp.grabber.room.abstractRoom.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                }
            }
        }

        public void SendHit(Weapon obj, PhysicalObject hit, BodyChunk chunk)
        {
            if (hit == null || obj == null || !isSynced(obj) || !isSynced(hit) || (obj.abstractPhysicalObject as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID)
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(obj.room.abstractRoom.name))
                return;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.Hit);
            writer.Write(obj.room.abstractRoom.name);
            writer.Write((obj.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            writer.Write((hit.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            if (chunk != null)
            {
                writer.Write(chunk.index);
            }
            else
            {
                writer.Write(-1);
            }
            MonklandSteamManager.Log("[Entity] Sending Hit: " + (obj.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " hit " + (hit.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[obj.room.abstractRoom.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                }
            }
        }

        public void SendThrow(Weapon thrown, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc)
        {
            if (thrownBy == null || thrown == null || !isSynced(thrownBy) || !isSynced(thrown) || (thrown.abstractPhysicalObject as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID)
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(thrown.room.abstractRoom.name))
                return;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.Throw);
            writer.Write(thrown.room.abstractRoom.name);
            writer.Write((thrown.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            writer.Write((thrownBy.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            Vector2Handler.Write(thrownPos, ref writer);
            Vector2NHandler.Write(firstFrameTraceFromPos, ref writer);
            IntVector2Handler.Write(throwDir, ref writer);
            writer.Write(frc);
            MonklandSteamManager.Log("[Entity] Sending Throw: " + (thrown.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " thrown by " + (thrownBy.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[thrown.room.abstractRoom.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                }
            }
        }

        public void SendSpearStick(AbstractPhysicalObject A, AbstractPhysicalObject B, AbstractRoom room, int chunk, int bodyPart, float angle)
        {
            if (A == null || B == null || room == null || !isSynced(A) || !isSynced(B) || ((A as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID && (B as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID))
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.name))
                return;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.Stick);
            writer.Write((byte)StickType.Spear);
            writer.Write(room.name);
            writer.Write((A as patch_AbstractPhysicalObject).dist);
            writer.Write((B as patch_AbstractPhysicalObject).dist);
            writer.Write(chunk);
            writer.Write(bodyPart);
            writer.Write(angle);
            MonklandSteamManager.Log("[Entity] Sending Spear Stick: " + (A as patch_AbstractPhysicalObject).dist + ", " + (B as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                }
            }
        }

        public void SendSpearAppendageStick(AbstractPhysicalObject A, AbstractPhysicalObject B, AbstractRoom room, int appendage, int prevSeg, float distanceToNext, float angle)
        {
            if (A == null || B == null || room == null || !isSynced(A) || !isSynced(B) || ((A as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID && (B as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID))
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.name))
                return;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.Stick);
            writer.Write((byte)StickType.SpearAppendage);
            writer.Write(room.name);
            writer.Write((A as patch_AbstractPhysicalObject).dist);
            writer.Write((B as patch_AbstractPhysicalObject).dist);
            writer.Write(appendage);
            writer.Write(prevSeg);
            writer.Write(distanceToNext);
            writer.Write(angle);
            MonklandSteamManager.Log("[Entity] Sending Spear Appendage Stick: " + (A as patch_AbstractPhysicalObject).dist + ", " + (B as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                }
            }
        }

        public void SendSpearImpaledStick(AbstractPhysicalObject A, AbstractPhysicalObject B, AbstractRoom room, int chunk, int onSpearPosition)
        {
            if (A == null || B == null || room == null || !isSynced(A) || !isSynced(B) || ((A as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID && (B as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID))
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.name))
                return;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.Stick);
            writer.Write((byte)StickType.SpearImpaled);
            writer.Write(room.name);
            writer.Write((A as patch_AbstractPhysicalObject).dist);
            writer.Write((B as patch_AbstractPhysicalObject).dist);
            writer.Write(chunk);
            writer.Write(onSpearPosition);
            MonklandSteamManager.Log("[Entity] Sending Spear Impaled Stick: " + (A as patch_AbstractPhysicalObject).dist + ", " + (B as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                }
            }
        }

        public void SendDeactivate(AbstractPhysicalObject A, AbstractPhysicalObject B, AbstractRoom room)
        {
            if (A == null || B == null || room == null || !isSynced(A) || !isSynced(B) || ((A as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID && (B as patch_AbstractPhysicalObject).owner != NetworkGameManager.playerID))
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.name))
                return;
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.Deactivate);
            writer.Write(room.name);
            writer.Write((A as patch_AbstractPhysicalObject).dist);
            writer.Write((B as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.Log("[Entity] Sending Stick Deactivate: " + (A as patch_AbstractPhysicalObject).dist + ", " + (B as patch_AbstractPhysicalObject).dist);
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                }
            }
        }

        #endregion

        public enum PacketType
        {
            Player,
            Rock,
            EstablishGrasp,
            SwitchGrasps,
            ReleaseGrasp,
            Hit,
            Throw,
            Stick,
            Deactivate,
            Spear
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
                    if (((cr as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner == sentPlayer.m_SteamID)
                    {
                        cr.realizedCreature = PlayerHandler.Read((cr.realizedCreature as Player), ref br);// Read Player
                        if (MonklandSteamManager.DEBUG)
                            MonklandUI.UpdateMessage(SteamFriends.GetFriendPersonaName(sentPlayer)+"\nOwner:" + sentPlayer.m_SteamID + "\nID:" + dist, 1, cr.realizedCreature.bodyChunks[0].pos, dist, abstractRoom.index, (cr.realizedCreature as patch_Player).ShortCutColor());
                        cr.pos.room = abstractRoom.index;
                    }
                    return;
                }
            }
            startPos.room = abstractRoom.index;
            startPos.abstractNode = -1;
            AbstractCreature abstractCreature = new AbstractCreature(patch_RainWorldGame.mainGame.world, StaticWorld.GetCreatureTemplate("Slugcat"), null, startPos, new EntityID(-1, dist));
            abstractCreature.state = new PlayerState(abstractCreature, 1, 0, false);
            ((abstractCreature as AbstractPhysicalObject) as patch_AbstractPhysicalObject).ID.number = dist;
            ((abstractCreature as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner = sentPlayer.m_SteamID;
            abstractCreature.pos.room = abstractRoom.index;
            patch_RainWorldGame.mainGame.world.GetAbstractRoom(abstractCreature.pos.room).AddEntity(abstractCreature);
            abstractCreature.RealizeInRoom();
            abstractCreature.realizedCreature = PlayerHandler.Read((abstractCreature.realizedCreature as Player), ref br);// Read Player
            abstractCreature.pos.room = abstractRoom.index;
        }

        public void ReadRock(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning)
                return;
            string roomName = br.ReadString();//Read Room Name
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
                return;
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null)
                return;
            WorldCoordinate startPos = WorldCoordinateHandler.Read(ref br);// Read World Coordinate
            EntityID ID = EntityIDHandler.Read(ref br);// Read EntityID
            int dist = ID.number;
            for (int i = 0; i < abstractRoom.realizedRoom.physicalObjects.Length; i++)
            {
                for (int j = 0; j < abstractRoom.realizedRoom.physicalObjects[i].Count; j++)
                {
                    if (abstractRoom.realizedRoom.physicalObjects[i][j] != null && abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject != null && ((abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject as AbstractPhysicalObject) as patch_AbstractPhysicalObject).dist == dist)
                    {
                        if (((abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner == sentPlayer.m_SteamID)
                        {
                            (abstractRoom.realizedRoom.physicalObjects[i][j] as patch_Rock).Sync();
                            abstractRoom.realizedRoom.physicalObjects[i][j] = WeaponHandler.Read((abstractRoom.realizedRoom.physicalObjects[i][j] as Rock), ref br);// Read Rock
                            if (MonklandSteamManager.DEBUG)
                                MonklandUI.UpdateMessage("Rock\nO:" + (sentPlayer.m_SteamID % 10000) + "\nID:" + (dist % 10000), 2, abstractRoom.realizedRoom.physicalObjects[i][j].bodyChunks[0].pos, dist, abstractRoom.index, Color.white);
                            abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject.pos.room = abstractRoom.index;
                        }
                        return;
                    }
                }
            }
            startPos.room = abstractRoom.index;
            startPos.abstractNode = -1;
            AbstractPhysicalObject abstractObject = new AbstractPhysicalObject(patch_RainWorldGame.mainGame.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, startPos, ID);
            (abstractObject as patch_AbstractPhysicalObject).owner = sentPlayer.m_SteamID;
            abstractObject.pos.room = abstractRoom.index;
            patch_RainWorldGame.mainGame.world.GetAbstractRoom(abstractObject.pos.room).AddEntity(abstractObject);
            abstractObject.RealizeInRoom();
            (abstractObject.realizedObject as patch_Rock).Sync();
            abstractObject.realizedObject = WeaponHandler.Read((abstractObject.realizedObject as Rock), ref br);// Read Rock
            abstractObject.pos.room = abstractRoom.index;
        }

        public void ReadSpear(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning)
                return;
            string roomName = br.ReadString();// Read Room.name
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
                return;
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null)
                return;
            WorldCoordinate startPos = WorldCoordinateHandler.Read(ref br);// Read WorldCoordinate
            EntityID ID = EntityIDHandler.Read(ref br);// Read EntityID
            int dist = ID.number;
            for (int i = 0; i < abstractRoom.realizedRoom.physicalObjects.Length; i++)
            {
                for (int j = 0; j < abstractRoom.realizedRoom.physicalObjects[i].Count; j++)
                {
                    if (abstractRoom.realizedRoom.physicalObjects[i][j] != null && abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject != null && ((abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject as AbstractPhysicalObject) as patch_AbstractPhysicalObject).dist == dist)
                    {
                        if (((abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner == sentPlayer.m_SteamID)
                        {
                            (abstractRoom.realizedRoom.physicalObjects[i][j] as patch_Spear).Sync();
                            abstractRoom.realizedRoom.physicalObjects[i][j] = SpearHandler.Read((abstractRoom.realizedRoom.physicalObjects[i][j] as Spear), ref br);// Read Spear
                            if (MonklandSteamManager.DEBUG)
                                MonklandUI.UpdateMessage("Spear\nO:" + (sentPlayer.m_SteamID % 10000) + "\nID:" + (dist % 10000), 2, abstractRoom.realizedRoom.physicalObjects[i][j].bodyChunks[0].pos, dist, abstractRoom.index, Color.white);
                            abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject.pos.room = abstractRoom.index;
                        }
                        return;
                    }
                }
            }
            startPos.room = abstractRoom.index;
            startPos.abstractNode = -1;
            AbstractSpear abstractObject = new AbstractSpear(patch_RainWorldGame.mainGame.world, null, startPos, ID, false);
            (abstractObject as AbstractPhysicalObject as patch_AbstractPhysicalObject).owner = sentPlayer.m_SteamID;
            abstractObject.pos.room = abstractRoom.index;
            patch_RainWorldGame.mainGame.world.GetAbstractRoom(abstractObject.pos.room).AddEntity(abstractObject);
            abstractObject.RealizeInRoom();
            (abstractObject.realizedObject as patch_Spear).Sync();
            abstractObject.realizedObject = SpearHandler.Read((abstractObject.realizedObject as Spear), ref br);// Read Spear
            abstractObject.pos.room = abstractRoom.index;
        }

        #region Grasps and Sticks
        public void ReadHit(BinaryReader br, CSteamID sentPlayer)
        {
            string roomName = br.ReadString();//Read Room Name
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || sentPlayer.m_SteamID == NetworkGameManager.playerID)
                return;

            if (abstractRoom.realizedRoom != null)
            {
                PhysicalObject obj = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);
                PhysicalObject hit = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);
                int chunk = br.ReadInt32();
                if (obj != null)
                {
                    if (hit != null)
                    {
                        MonklandSteamManager.Log("[Entity] Incomming hit: " + (obj.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " hit " + (hit.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
                    }
                    else
                    {
                        MonklandSteamManager.Log("[Entity] Incomming hit: " + (obj.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " hit something");
                    }
                    if (obj is Rock)
                    {
                        if (chunk != -1)
                        {
                            (obj as patch_Rock).NetHit(hit, hit.bodyChunks[chunk]);
                        }
                        else
                        {
                            (obj as patch_Rock).NetHit(hit, null);
                        }
                    }
                    else if (obj is Spear)
                    {
                        if (chunk != -1)
                        {
                            (obj as patch_Spear).NetHit(hit, hit.bodyChunks[chunk]);
                        }
                        else
                        {
                            (obj as patch_Spear).NetHit(hit, null);
                        }
                    }
                }
            }
        }

        public void ReadThrow(BinaryReader br, CSteamID sentPlayer)
        {
            string roomName = br.ReadString();//Read Room Name
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || sentPlayer.m_SteamID == NetworkGameManager.playerID)
                return;

            if (abstractRoom.realizedRoom != null)
            {
                PhysicalObject weapon = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);
                Creature thrownBy = DistHandler.ReadCreature(ref br, abstractRoom.realizedRoom);
                Vector2 thrownPos = Vector2Handler.Read(ref br);
                Vector2? firstFrameTraceFromPos = Vector2NHandler.Read(ref br);
                IntVector2 throwDir = IntVector2Handler.Read(ref br);
                float frc = br.ReadSingle();
                if (weapon != null && thrownBy != null)
                {
                    MonklandSteamManager.Log("[Entity] Incomming Throw: " + (weapon.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " thrownby " + (thrownBy.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
                    if (weapon is Rock)
                    {
                        (weapon as patch_Rock).NetThrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc);
                    }
                    else if (weapon is Spear)
                    {
                        (weapon as patch_Spear).NetThrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc);
                    }
                }
            }
        }

        public void ReadGrab(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning || patch_RainWorldGame.mainGame == null || patch_RainWorldGame.mainGame.world == null)
                return;
            string roomName = br.ReadString();//Read Room Name
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null)
                return;
            if (abstractRoom.realizedRoom != null)
            {
                Creature grabber = DistHandler.ReadCreature(ref br, abstractRoom.realizedRoom);
                PhysicalObject grabbed = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);
                int graspUsed = br.ReadInt32();
                int grabbedChunk = br.ReadInt32();
                Creature.Grasp.Shareability shareability = (Creature.Grasp.Shareability)br.ReadInt32();
                float dominance = br.ReadSingle();
                bool pacifying = br.ReadBoolean();
                if (grabbed != null)
                {
                    if (grabbed is Player && grabber != null && !(grabber is Player))
                    {
                        (grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).owner = (grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).owner;
                    }
                    else if (!(grabbed is Player))
                    {
                        (grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).owner = sentPlayer.m_SteamID;
                    }
                }
                if (grabber == null || grabbed == null)
                {
                    MonklandSteamManager.Log("[Entity] Incomming grab: One or more targets not found!");
                    return;
                }
                if (grabber.grasps[graspUsed] != null && grabber.grasps[graspUsed].grabbed != null && (grabber.grasps[graspUsed].grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist == (grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist)
                {
                    MonklandSteamManager.Log("[Entity] Incomming grab: Grab already satisfyed, " + (grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " grabbed " + (grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
                    return;
                }
                int preexisting = -1;
                for (int i = 0; i < grabber.grasps.Length; i++)
                {
                    if (grabber.grasps[i] != null && grabber.grasps[i].grabbed != null && (grabber.grasps[i].grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist == (grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist)
                    {
                        preexisting = i;
                    }
                }
                if (preexisting != -1 && preexisting != graspUsed)
                {
                    MonklandSteamManager.Log("[Entity] Incomming grab: Found preexisting satifying grab, " + (grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " grabbed " + (grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
                    (grabber as patch_Creature).NetSwitch(preexisting, graspUsed);
                    return;
                }
                MonklandSteamManager.Log("[Entity] Incomming grab: GRAB! " + (grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " grabbed " + (grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
                (grabber as patch_Creature).NetGrab(grabbed, graspUsed, grabbedChunk, shareability, dominance, pacifying);
            }
            else
            {
                int grabber = br.ReadInt32();
                AbstractCreature abstractGrabber = null;
                int grabbed = br.ReadInt32();
                AbstractPhysicalObject abstractGrabbed = null;
                int graspUsed = br.ReadInt32();
                for (int i = 0; i < abstractRoom.entities.Count; i++)
                {
                    if (abstractRoom.entities[i] != null && abstractRoom.entities[i] is AbstractPhysicalObject)
                    {
                        if ((abstractRoom.entities[i] as patch_AbstractPhysicalObject).dist == grabber && (abstractRoom.entities[i] is AbstractCreature))
                        {
                            abstractGrabber = (abstractRoom.entities[i] as AbstractCreature);
                            for (int j = 0; j < (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects.Count; j++)
                            {
                                if ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j] != null && (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].A != null && (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B != null && ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].A as patch_AbstractPhysicalObject).dist == grabber && ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B as patch_AbstractPhysicalObject).dist == grabbed)
                                {
                                    MonklandSteamManager.Log("[Entity] Incomming grab: Grab already satisfyed" + grabber + " grabbed " + grabbed);
                                    abstractGrabbed = (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B;
                                    if (!(abstractGrabbed is AbstractCreature) || (abstractGrabbed as AbstractCreature).creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Slugcat)
                                        (abstractGrabbed as patch_AbstractPhysicalObject).owner = sentPlayer.m_SteamID;
                                    return;
                                }
                            }
                        }
                        else if ((abstractRoom.entities[i] as patch_AbstractPhysicalObject).dist == grabbed)
                        {
                            abstractGrabbed = (abstractRoom.entities[i] as AbstractPhysicalObject);
                            if (!(abstractGrabbed is AbstractCreature) || (abstractGrabbed as AbstractCreature).creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Slugcat)
                                (abstractGrabbed as patch_AbstractPhysicalObject).owner = sentPlayer.m_SteamID;
                            for (int j = 0; j < (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects.Count; j++)
                            {
                                if ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j] != null && (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].A != null && (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B != null && ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].A as patch_AbstractPhysicalObject).dist == grabber && ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B as patch_AbstractPhysicalObject).dist == grabbed)
                                {
                                    MonklandSteamManager.Log("[Entity] Incomming grab: Grab already satisfyed" + grabber + " grabbed " + grabbed);
                                    return;
                                }
                            }
                        }
                    }
                }
                if (abstractGrabber != null && abstractGrabbed != null)
                {
                    MonklandSteamManager.Log("[Entity] Incomming grab: Abstract GRAB!" + grabber + " grabbed " + grabbed);
                    new AbstractPhysicalObject.CreatureGripStick(abstractGrabber, abstractGrabbed, graspUsed, true);
                }
                else
                {
                    MonklandSteamManager.Log("[Entity] Incomming grab: One or more abstract targets not found!");
                }
            }

        }

        public void ReadRelease(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning || patch_RainWorldGame.mainGame == null || patch_RainWorldGame.mainGame == null)
                return;
            string roomName = br.ReadString();//Read Room Name
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null)
                return;
            if (abstractRoom.realizedRoom != null)
            {
                Creature grabber = DistHandler.ReadCreature(ref br, abstractRoom.realizedRoom);
                int grabbed = br.ReadInt32();
                if (grabber == null)
                {
                    MonklandSteamManager.Log("[Entity] Incomming release grasp: One or more targets not found!");
                    return;
                }
                for (int i = 0; i < grabber.grasps.Length; i++)
                {
                    if (grabber.grasps[i] != null && grabber.grasps[i].grabbed != null && (grabber.grasps[i].grabbed.abstractPhysicalObject as patch_AbstractPhysicalObject).dist == grabbed)
                    {
                        MonklandSteamManager.Log("[Entity] Incomming release grasp: RELEASE! " + (grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist + " released " + grabbed);
                        (grabber as patch_Creature).NetRelease(i);
                    }
                }
            }
            else 
            {
                int grabber = br.ReadInt32();
                int grabbed = br.ReadInt32();
                for (int i = 0; i < abstractRoom.entities.Count; i++)
                {
                    if (abstractRoom.entities[i] != null && abstractRoom.entities[i] is AbstractPhysicalObject)
                    {
                        for (int j = 0; j < (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects.Count; j++)
                        {
                            if ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j] != null && (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].A != null && (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B != null && ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].A as patch_AbstractPhysicalObject).dist == grabber && ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B as patch_AbstractPhysicalObject).dist == grabbed)
                            {
                                MonklandSteamManager.Log("[Entity] Incomming release grasp: Abstract RELEASE! " + grabber + " released " + grabbed);
                                (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].Deactivate();
                            }
                        }
                    }
                }
                MonklandSteamManager.Log("[Entity] Incomming release grasp: One or more abstract targets not found!");
            }

        }

        public void ReadSwitch(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning || patch_RainWorldGame.mainGame == null || patch_RainWorldGame.mainGame == null)
                return;
            string roomName = br.ReadString();//Read Room Name
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
                return;
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null)
            {
                MonklandSteamManager.Log("[Entity] Incomming switch: Room not found!");
                return; 
            }
            Creature grabber = DistHandler.ReadCreature(ref br, abstractRoom.realizedRoom);
            int from = br.ReadInt32();
            int to = br.ReadInt32();
            if (grabber == null)
            {
                MonklandSteamManager.Log("[Entity] Incomming switch: Target not found!");
                return;
            }
            MonklandSteamManager.Log("[Entity] Incomming switch: SWITCH! "+ (grabber.abstractPhysicalObject as patch_AbstractPhysicalObject).dist);
            (grabber as patch_Creature).NetSwitch(from, to);

        }

        public void ReadStick(BinaryReader br, CSteamID sentPlayer)
        {
            StickType type = (StickType)br.ReadByte();
            if (!MonklandSteamManager.WorldManager.gameRunning || patch_RainWorldGame.mainGame == null || patch_RainWorldGame.mainGame == null)
                return;
            string roomName = br.ReadString();//Read Room Name
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null)
                return;
            int distA = br.ReadInt32();
            int distB = br.ReadInt32();
            AbstractPhysicalObject A = null;
            AbstractPhysicalObject B = null;
            for (int i = 0; i < abstractRoom.entities.Count; i++)
            {
                if (abstractRoom.entities[i] != null && abstractRoom.entities[i] is AbstractPhysicalObject)
                {
                    if ((abstractRoom.entities[i] as patch_AbstractPhysicalObject).dist == distA)
                    {
                        A = (abstractRoom.entities[i] as AbstractPhysicalObject);
                    }
                    else if ((abstractRoom.entities[i] as patch_AbstractPhysicalObject).dist == distB)
                    {
                        B = (abstractRoom.entities[i] as AbstractPhysicalObject);
                    }
                }
            }
            if (A == null || B == null)
            {
                MonklandSteamManager.Log("[Entity] Incomming stick: Targets not found!");
                return;
            }
            for (int i = 0; i < A.stuckObjects.Count; i++)
            {
                if ((A.stuckObjects[i].B as patch_AbstractPhysicalObject).dist == distB)
                {
                    MonklandSteamManager.Log("[Entity] Incomming stick: Stick already satisfyed!");
                    return;
                }
            }
            switch (type)
            {
                case StickType.Spear:
                    int chunk = br.ReadInt32();
                    int bodyPart = br.ReadInt32();
                    float angle = br.ReadSingle();
                    new AbstractPhysicalObject.AbstractSpearStick(A, B, chunk, bodyPart, angle);
                    break;
                case StickType.SpearAppendage:
                    int appendage = br.ReadInt32();
                    int prevSeg = br.ReadInt32();
                    float distanceToNext = br.ReadSingle();
                    float ang = br.ReadSingle();
                    new AbstractPhysicalObject.AbstractSpearAppendageStick(A, B, appendage, prevSeg, distanceToNext, ang);
                    break;
                case StickType.SpearImpaled:
                    int index = br.ReadInt32();
                    int onSpearPosition = br.ReadInt32();
                    new AbstractPhysicalObject.ImpaledOnSpearStick(A, B, index, onSpearPosition);
                    break;
            }
            MonklandSteamManager.Log("[Entity] Incomming stick: STICK! " + distA + ", " + distB);
        }

        public void ReadDeactivate(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning || patch_RainWorldGame.mainGame == null || patch_RainWorldGame.mainGame == null)
                return;
            string roomName = br.ReadString();//Read Room Name
            AbstractRoom abstractRoom = patch_RainWorldGame.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null)
                return;
            int A = br.ReadInt32();//Read Grabber dist
            int B = br.ReadInt32();//Read Grabbed dist
            for (int i = 0; i < abstractRoom.entities.Count; i++)
            {
                if (abstractRoom.entities[i] != null && abstractRoom.entities[i] is AbstractPhysicalObject)
                {
                    for (int j = 0; j < (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects.Count; j++)
                    {
                        if ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j] != null && (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].A != null && (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B != null && ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].A as patch_AbstractPhysicalObject).dist == A && ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B as patch_AbstractPhysicalObject).dist == B)
                        {
                            MonklandSteamManager.Log("[Entity] Incomming deactivate: DEACTIVATE! " + A + " released " + B);
                            (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].Deactivate();
                            return;
                        }
                    }
                }
            }
            MonklandSteamManager.Log("[Entity] Incomming deactivate: Targets not found!");
        }

        public enum StickType
        {
            Spear,
            SpearAppendage,
            SpearImpaled
        }

        #endregion

        #endregion
    }

}
