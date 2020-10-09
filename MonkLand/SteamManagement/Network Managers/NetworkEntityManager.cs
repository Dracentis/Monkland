using Monkland.Hooks;
using Monkland.Hooks.Entities;
using Monkland.UI;
using RWCustom;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkEntityManager : NetworkManager
    {
#pragma warning disable IDE0060
#pragma warning disable IDE1006

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
                    if (RainWorldGameHK.mainGame != null)
                    { ReadPlayer(br, sentPlayer); }
                    return;

                case (byte)PacketType.Rock:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadRock(br, sentPlayer); }
                    return;

                case (byte)PacketType.EstablishGrasp:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadGrab(br, sentPlayer); }
                    return;

                case (byte)PacketType.SwitchGrasps:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadSwitch(br, sentPlayer); }
                    return;

                case (byte)PacketType.ReleaseGrasp:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadRelease(br, sentPlayer); }
                    return;

                case (byte)PacketType.Hit:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadHit(br, sentPlayer); }
                    return;

                case (byte)PacketType.Throw:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadThrow(br, sentPlayer); }
                    return;

                case (byte)PacketType.Stick:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadStick(br, sentPlayer); }
                    return;

                case (byte)PacketType.Deactivate:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadDeactivate(br, sentPlayer); }
                    return;

                case (byte)PacketType.Spear:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadSpear(br, sentPlayer); }
                    return;
            }
        }

        #endregion Packet Handler

        public bool isSynced(PhysicalObject obj)
        {
            if (obj == null)
                return false;
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
            if (obj == null)
            { return false; }
            if (obj is AbstractCreature && (obj as AbstractCreature).creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Slugcat)
            { return true; }
            if (obj.type == AbstractPhysicalObject.AbstractObjectType.Rock)
            { return true; }
            if (obj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            { return true; }
            return false;
        }

        #region Outgoing Packets

        public void Send(PhysicalObject physicalObject, List<ulong> targets, bool reliable = false)
        {
            try
            {
                if (physicalObject == null || targets == null || targets.Count == 0 || physicalObject.room == null || physicalObject.room.abstractRoom == null || !isSynced(physicalObject))
                {
                    return;
                }
                MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
                BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
                AbsPhyObjFields field = AbstractPhysicalObjectHK.GetField(physicalObject.abstractPhysicalObject);

                if (physicalObject is Player)
                {
                    writer.Write((byte)PacketType.Player);
                    writer.Write(physicalObject.room.abstractRoom.index);
                    writer.Write(field.dist);
                    WorldCoordinateHandler.Write(physicalObject.abstractPhysicalObject.pos, ref writer);
                    PlayerHandler.Write(physicalObject as PhysicalObject as Player, ref writer);
                    if (MonklandSteamManager.DEBUG)
                    {
                        MonklandUI.UpdateMessage($"Player\nOwner: {field.owner}\nID: {field.dist}", 1, physicalObject.bodyChunks[0].pos, field.dist, physicalObject.room.abstractRoom.index, (physicalObject as Player).ShortCutColor());
                    }
                }
                else if (physicalObject is Rock)
                {
                    writer.Write((byte)PacketType.Rock);
                    writer.Write(physicalObject.room.abstractRoom.index);
                    // Dist
                    WorldCoordinateHandler.Write(physicalObject.abstractPhysicalObject.pos, ref writer);
                    EntityIDHandler.Write(physicalObject.abstractPhysicalObject.ID, ref writer);
                    WeaponHandler.Write(physicalObject as Rock, ref writer);
                    reliable = true;
                    if (MonklandSteamManager.DEBUG)
                    {
                        MonklandUI.UpdateMessage($"Rock\nO: {field.owner % 10000:0000}\nID: {field.dist % 10000:0000}", 1, physicalObject.bodyChunks[0].pos, field.dist, physicalObject.room.abstractRoom.index, Color.white);
                    }
                }
                else if (physicalObject is Spear)
                {
                    writer.Write((byte)PacketType.Spear);
                    writer.Write(physicalObject.room.abstractRoom.index);
                    // Dist
                    WorldCoordinateHandler.Write(physicalObject.abstractPhysicalObject.pos, ref writer);
                    EntityIDHandler.Write(physicalObject.abstractPhysicalObject.ID, ref writer);
                    SpearHandler.Write(physicalObject as Spear, ref writer);
                    reliable = true;
                    if (MonklandSteamManager.DEBUG)
                    {
                        MonklandUI.UpdateMessage($"Spear\nO: {field.owner % 10000:0000}\nID: {field.dist % 10000:0000}", 1, physicalObject.bodyChunks[0].pos, field.dist, physicalObject.room.abstractRoom.index, Color.white);
                    }
                }
                else
                { return; }

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
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        #region Grasps and Sticks

        public void SendGrab(Creature.Grasp grasp)
        {
            if (grasp == null || grasp.grabber == null || grasp.grabbed == null || !isSynced(grasp.grabber) || !isSynced(grasp.grabbed))
            { return; }

            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grasp.grabber.room.abstractRoom.index)) { return; }

            AbsPhyObjFields grabberField = AbstractPhysicalObjectHK.GetField(grasp.grabber.abstractPhysicalObject);

            if (grabberField.owner != NetworkGameManager.playerID) { return; }

            AbsPhyObjFields grabbedSub = AbstractPhysicalObjectHK.GetField(grasp.grabbed.abstractPhysicalObject);

            MonklandSteamManager.Log($"[Entity] Sending Grab: {grabberField.dist} grabbed {grabbedSub.dist}");
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)PacketType.EstablishGrasp);
            writer.Write(grasp.grabber.room.abstractRoom.index);
            writer.Write(grabberField.dist);
            writer.Write(grabbedSub.dist);
            writer.Write(grasp.graspUsed);
            writer.Write(grasp.grabbedChunk.index);
            writer.Write((int)grasp.shareability);
            writer.Write(grasp.dominance);
            writer.Write(grasp.pacifying);

            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[grasp.grabber.room.abstractRoom.index])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID) { MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendUnreliableNoDelay); }
                }
            }
            if ((grasp.grabbed is Player) && !(grasp.grabber is Player))//One of our creatures is grabbing another player so the grabber should belong to the grabbed
            {
                grabberField.owner = grabbedSub.owner;
            }
            else if (!(grasp.grabbed is Player))//One of our creatures is the grabber so we should take ownership of the grabbed
            {
                grabbedSub.owner = grabberField.owner;
            }
        }

        public void SendSwitch(Creature grabber, int from, int to)
        {
            if (grabber == null || !isSynced(grabber)) { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grabber.room.abstractRoom.index)) { return; }
            AbsPhyObjFields grabberSub = AbstractPhysicalObjectHK.GetField(grabber.abstractPhysicalObject);
            if (grabberSub.owner != NetworkGameManager.playerID) { return; }
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.SwitchGrasps);
            writer.Write(grabber.room.abstractRoom.index);
            writer.Write(grabberSub.dist);
            writer.Write(from);
            writer.Write(to);
            MonklandSteamManager.Log($"[Entity] Sending Switch: {grabberSub.dist} switched {from} => {to}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[grabber.room.abstractRoom.index])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    { MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable); }
                }
            }
        }

        public void SendRelease(Creature.Grasp grasp)
        {
            if (grasp == null || grasp.grabber == null || grasp.grabbed == null || !isSynced(grasp.grabber) || !isSynced(grasp.grabbed))
            { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grasp.grabber.room.abstractRoom.index)) { return; }
            AbsPhyObjFields grabberSub = AbstractPhysicalObjectHK.GetField(grasp.grabber.abstractPhysicalObject);
            if (grabberSub.owner != NetworkGameManager.playerID) { return; }
            AbsPhyObjFields grabbedSub = AbstractPhysicalObjectHK.GetField(grasp.grabbed.abstractPhysicalObject);
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)PacketType.ReleaseGrasp);
            writer.Write(grasp.grabber.room.abstractRoom.index);
            writer.Write(grabberSub.dist);
            writer.Write(grabbedSub.dist);
            MonklandSteamManager.Log($"[Entity] Sending Release: {grabberSub.dist} released {grabbedSub.dist}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[grasp.grabber.room.abstractRoom.index])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    { MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable); }
                }
            }
        }

        public void SendHit(Weapon obj, PhysicalObject hit, BodyChunk chunk)
        {
            if (hit == null || obj == null || !isSynced(obj) || !isSynced(hit)) { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(obj.room.abstractRoom.index)) { return; }

            AbsPhyObjFields objField = AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject);
            AbsPhyObjFields hitField = AbstractPhysicalObjectHK.GetField(hit.abstractPhysicalObject);

            if (objField.owner != NetworkGameManager.playerID) { return; }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)PacketType.Hit);
            writer.Write(obj.room.abstractRoom.index);
            writer.Write(objField.dist);
            writer.Write(hitField.dist);

            if (chunk != null) { writer.Write(chunk.index); }
            else { writer.Write(-1); }

            MonklandSteamManager.Log($"[Entity] Sending Hit: {objField.dist} hit {hitField.dist}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[obj.room.abstractRoom.index])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    {
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                    }
                }
            }
        }

        public void SendThrow(Weapon thrown, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc)
        {
            if (thrownBy == null || thrown == null || !isSynced(thrownBy) || !isSynced(thrown))
            {
                return;
            }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(thrown.room.abstractRoom.index))
            {
                return;
            }

            AbsPhyObjFields thrownField = AbstractPhysicalObjectHK.GetField(thrown.abstractPhysicalObject);

            if (thrownField.owner != NetworkGameManager.playerID)
            {
                return;
            }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            AbsPhyObjFields thrownBySub = AbstractPhysicalObjectHK.GetField(thrownBy.abstractPhysicalObject);

            writer.Write((byte)PacketType.Throw);
            writer.Write(thrown.room.abstractRoom.index);
            writer.Write(thrownField.dist);
            writer.Write(thrownBySub.dist);
            Vector2Handler.Write(thrownPos, ref writer);
            Vector2NHandler.Write(firstFrameTraceFromPos, ref writer);
            IntVector2Handler.Write(throwDir, ref writer);
            writer.Write(frc);

            MonklandSteamManager.Log($"[Entity] Sending Throw: {thrownField.dist} thrown by {thrownBySub.dist}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[thrown.room.abstractRoom.index])

            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    {
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                    }
                }
            }
        }

        public void SendSpearStick(AbstractPhysicalObject A, AbstractPhysicalObject B, AbstractRoom room, int chunk, int bodyPart, float angle)
        {
            if (A == null || B == null || room == null || !isSynced(A) || !isSynced(B)) { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.index)) { return; }

            AbsPhyObjFields AField = AbstractPhysicalObjectHK.GetField(A);
            AbsPhyObjFields BField = AbstractPhysicalObjectHK.GetField(B);

            if (AField.owner != NetworkGameManager.playerID && BField.owner != NetworkGameManager.playerID) { return; }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)PacketType.Stick);
            writer.Write((byte)StickType.Spear);
            writer.Write(room.name);
            writer.Write(AField.dist);
            writer.Write(BField.dist);
            writer.Write(chunk);
            writer.Write(bodyPart);
            writer.Write(angle);

            MonklandSteamManager.Log($"[Entity] Sending Spear Stick: {AField.dist}, {BField.dist}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.index])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    {
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                    }
                }
            }
        }

        public void SendSpearAppendageStick(AbstractPhysicalObject A, AbstractPhysicalObject B, AbstractRoom room, int appendage, int prevSeg, float distanceToNext, float angle)
        {
            if (A == null || B == null || room == null || !isSynced(A) || !isSynced(B)) { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.index)) { return; }

            AbsPhyObjFields AField = AbstractPhysicalObjectHK.GetField(A);
            AbsPhyObjFields BField = AbstractPhysicalObjectHK.GetField(B);

            if (AField.owner != NetworkGameManager.playerID && BField.owner != NetworkGameManager.playerID) { return; }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)PacketType.Stick);
            writer.Write((byte)StickType.SpearAppendage);
            writer.Write(room.name);
            writer.Write(AField.dist);
            writer.Write(BField.dist);
            writer.Write(appendage);
            writer.Write(prevSeg);
            writer.Write(distanceToNext);
            writer.Write(angle);

            MonklandSteamManager.Log($"[Entity] Sending Spear Appendage Stick: {AField.dist}, {BField.dist}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.index])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    {
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                    }
                }
            }
        }

        public void SendSpearImpaledStick(AbstractPhysicalObject A, AbstractPhysicalObject B, AbstractRoom room, int chunk, int onSpearPosition)
        {
            if (A == null || B == null || room == null || !isSynced(A) || !isSynced(B)) { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.index)) { return; }

            AbsPhyObjFields AField = AbstractPhysicalObjectHK.GetField(A);
            AbsPhyObjFields BField = AbstractPhysicalObjectHK.GetField(B);

            if (AField.owner != NetworkGameManager.playerID && BField.owner != NetworkGameManager.playerID) { return; }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)PacketType.Stick);
            writer.Write((byte)StickType.SpearImpaled);
            writer.Write(room.name);
            writer.Write(AField.dist);
            writer.Write(BField.dist);
            writer.Write(chunk);
            writer.Write(onSpearPosition);

            MonklandSteamManager.Log($"[Entity] Sending Spear Impaled Stick: {AField.dist}, {BField.dist}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.index])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    {
                        MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable);
                    }
                }
            }
        }

        public void SendDeactivate(AbstractPhysicalObject A, AbstractPhysicalObject B, AbstractRoom room)
        {
            if (A == null || B == null || room == null || !isSynced(A) || !isSynced(B)) { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.index)) { return; }
            AbsPhyObjFields ASub = AbstractPhysicalObjectHK.GetField(A);
            AbsPhyObjFields BSub = AbstractPhysicalObjectHK.GetField(B);
            if (ASub.owner != NetworkGameManager.playerID && BSub.owner != NetworkGameManager.playerID) { return; }
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(CHANNEL, UtilityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)PacketType.Deactivate);
            writer.Write(room.name);
            writer.Write(ASub.dist);
            writer.Write(BSub.dist);
            MonklandSteamManager.Log($"[Entity] Sending Stick Deactivate: {ASub.dist}, {BSub.dist}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.index])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    { MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable); }
                }
            }
        }

        #endregion Grasps and Sticks

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
            Spear,
            Violence
        }

        #endregion Outgoing Packets

        #region Incomming Packets

        public void ReadPlayer(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning)
            { return; }

            //Read Room Name
            int roomName = br.ReadInt32();

            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
            { return; }

            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null)
            { return; }

            // Read Distinguisher
            int dist = br.ReadInt32();

            // Read World Coordinate
            WorldCoordinate startPos = WorldCoordinateHandler.Read(ref br);
            foreach (AbstractCreature cr in abstractRoom.creatures)
            {
                AbsPhyObjFields field = AbstractPhysicalObjectHK.GetField(cr);
                if (field.dist == dist && cr.realizedCreature != null)
                {
                    if (field.owner == sentPlayer.m_SteamID)
                    {
                        // Read Player
                        PlayerHandler.Read(cr.realizedCreature as Player, ref br);
                        if (MonklandSteamManager.DEBUG)
                        { MonklandUI.UpdateMessage($"{SteamFriends.GetFriendPersonaName(sentPlayer)}\nOwner: {sentPlayer.m_SteamID}\nID: {dist}", 1, cr.realizedCreature.bodyChunks[0].pos, dist, abstractRoom.index, (cr.realizedCreature as Player).ShortCutColor()); }
                        cr.pos.room = abstractRoom.index;
                    }
                    return;
                }
            }
            startPos.room = abstractRoom.index;
            startPos.abstractNode = -1;
            AbstractCreature abstractCreature = new AbstractCreature(RainWorldGameHK.mainGame.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, startPos, new EntityID(-1, dist));
            abstractCreature.state = new PlayerState(abstractCreature, 1, 0, false);
            abstractCreature.ID.number = dist;
            AbstractPhysicalObjectHK.GetField(abstractCreature).owner = sentPlayer.m_SteamID;
            abstractCreature.pos.room = abstractRoom.index;
            RainWorldGameHK.mainGame.world.GetAbstractRoom(abstractCreature.pos.room).AddEntity(abstractCreature);
            abstractCreature.RealizeInRoom();
            // Read Player
            PlayerHandler.Read(abstractCreature.realizedCreature as Player, ref br);
            abstractCreature.pos.room = abstractRoom.index;
        }

        public void ReadRock(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning)
            { return; }

            //Read Room Name
            int roomName = br.ReadInt32();
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
            { return; }

            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null)
            { return; }

            // Read World Coordinate
            WorldCoordinate startPos = WorldCoordinateHandler.Read(ref br);
            // Read EntityID
            EntityID ID = EntityIDHandler.Read(ref br);
            int dist = ID.number;

            for (int i = 0; i < abstractRoom.realizedRoom.physicalObjects.Length; i++)
            {
                for (int j = 0; j < abstractRoom.realizedRoom.physicalObjects[i].Count; j++)
                {
                    if (abstractRoom.realizedRoom.physicalObjects[i][j] != null && abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject != null)
                    {
                        AbsPhyObjFields field = AbstractPhysicalObjectHK.GetField(abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject);
                        if (field.dist != dist) { continue; }
                        if (field.owner == sentPlayer.m_SteamID)
                        {
                            RockHK.Sync(abstractRoom.realizedRoom.physicalObjects[i][j] as Rock);
                            WeaponHandler.Read((abstractRoom.realizedRoom.physicalObjects[i][j] as Rock), ref br);// Read Rock
                            if (MonklandSteamManager.DEBUG)
                            { MonklandUI.UpdateMessage($"Rock\nO: {sentPlayer.m_SteamID % 10000:0000}\nID: {dist % 10000:1000}", 2, abstractRoom.realizedRoom.physicalObjects[i][j].bodyChunks[0].pos, dist, abstractRoom.index, Color.white); }
                            abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject.pos.room = abstractRoom.index;
                        }
                        return;
                    }
                }
            }
            startPos.room = abstractRoom.index;
            startPos.abstractNode = -1;
            AbstractPhysicalObject abstractObject = new AbstractPhysicalObject(RainWorldGameHK.mainGame.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, startPos, ID);
            AbstractPhysicalObjectHK.GetField(abstractObject).owner = sentPlayer.m_SteamID;
            abstractObject.pos.room = abstractRoom.index;
            RainWorldGameHK.mainGame.world.GetAbstractRoom(abstractObject.pos.room).AddEntity(abstractObject);
            abstractObject.RealizeInRoom();
            RockHK.Sync(abstractObject.realizedObject as Rock);
            WeaponHandler.Read(abstractObject.realizedObject as Rock, ref br);// Read Rock
            abstractObject.pos.room = abstractRoom.index;
        }

        public void ReadSpear(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning)
            {
                return;
            }

            //Read Room Name
            int roomName = br.ReadInt32();

            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
            {
                return;
            }
            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null)
            {
                return;
            }
            WorldCoordinate startPos = WorldCoordinateHandler.Read(ref br);// Read WorldCoordinate
            EntityID ID = EntityIDHandler.Read(ref br);// Read EntityID
            int dist = ID.number;
            for (int i = 0; i < abstractRoom.realizedRoom.physicalObjects.Length; i++)
            {
                for (int j = 0; j < abstractRoom.realizedRoom.physicalObjects[i].Count; j++)
                {
                    if (abstractRoom.realizedRoom.physicalObjects[i][j] != null && abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject != null)
                    {
                        AbsPhyObjFields field = AbstractPhysicalObjectHK.GetField(abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject);
                        if (field.dist != dist)
                        {
                            continue;
                        }
                        if (field.owner == sentPlayer.m_SteamID)
                        {
                            SpearHK.Sync(abstractRoom.realizedRoom.physicalObjects[i][j] as Spear);
                            abstractRoom.realizedRoom.physicalObjects[i][j] = SpearHandler.Read((abstractRoom.realizedRoom.physicalObjects[i][j] as Spear), ref br);// Read Spear
                            if (MonklandSteamManager.DEBUG)
                            {
                                MonklandUI.UpdateMessage($"Spear\nO: {sentPlayer.m_SteamID % 10000:1000}\nID: {dist % 10000:1000}", 2,
                                    abstractRoom.realizedRoom.physicalObjects[i][j].bodyChunks[0].pos, dist, abstractRoom.index, Color.white);
                            }
                            abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject.pos.room = abstractRoom.index;
                        }
                        return;
                    }
                }
            }
            startPos.room = abstractRoom.index;
            startPos.abstractNode = -1;
            AbstractSpear abstractObject = new AbstractSpear(RainWorldGameHK.mainGame.world, null, startPos, ID, false);
            AbstractPhysicalObjectHK.GetField(abstractObject).owner = sentPlayer.m_SteamID;
            abstractObject.pos.room = abstractRoom.index;
            RainWorldGameHK.mainGame.world.GetAbstractRoom(abstractObject.pos.room).AddEntity(abstractObject);
            abstractObject.RealizeInRoom();
            SpearHK.Sync(abstractObject.realizedObject as Spear);
            abstractObject.realizedObject = SpearHandler.Read((abstractObject.realizedObject as Spear), ref br);// Read Spear
            abstractObject.pos.room = abstractRoom.index;
        }

        #region Grasps and Sticks

        public void ReadHit(BinaryReader br, CSteamID sentPlayer)
        {
            int roomName = br.ReadInt32();//Read Room Name
            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || sentPlayer.m_SteamID == NetworkGameManager.playerID) { return; }

            if (abstractRoom.realizedRoom != null)
            {
                PhysicalObject obj = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);
                PhysicalObject hit = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);
                int chunk = br.ReadInt32();
                if (obj != null)
                {
                    if (hit != null)
                    {
                        MonklandSteamManager.Log($"[Entity] Incoming hit: {AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject).dist} hit {AbstractPhysicalObjectHK.GetField(hit.abstractPhysicalObject).dist}");
                    }
                    else
                    {
                        MonklandSteamManager.Log($"[Entity] Incoming hit: {AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject).dist} hit something");
                    }
                    if (obj is Rock || obj is Spear)
                    {
                        RockHK.SetNet();
                        if (hit != null && chunk != -1)
                        { 
                            (obj as Weapon).HitSomething(new SharedPhysics.CollisionResult(hit, hit.bodyChunks[chunk], null, true, default), RainWorldGameHK.mainGame.evenUpdate); 
                        }
                        else
                        { 
                            (obj as Weapon).HitSomething(new SharedPhysics.CollisionResult(hit, hit is Creature ? hit.firstChunk : null, null, true, default), RainWorldGameHK.mainGame.evenUpdate); 
                        }
                    }
                    /*
                    else if (obj is Spear s)
                    {
                        WeaponHK.SetNet();
                        if (hit != null && chunk != -1)
                        { s.HitSomething(new SharedPhysics.CollisionResult(hit, hit.bodyChunks[chunk], null, true, default), RainWorldGameHK.mainGame.evenUpdate); }
                        else
                        { s.HitSomething(new SharedPhysics.CollisionResult(hit, hit is Creature ? hit.firstChunk : null, null, true, default), RainWorldGameHK.mainGame.evenUpdate); }
                    }
                    */
                }
            }
        }

        public void ReadThrow(BinaryReader br, CSteamID sentPlayer)
        {
            int roomName = br.ReadInt32();//Read Room Name
            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || sentPlayer.m_SteamID == NetworkGameManager.playerID)
            {
                return;
            }

            if (abstractRoom.realizedRoom != null)
            {
                PhysicalObject obj = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);
                Creature thrownBy = DistHandler.ReadCreature(ref br, abstractRoom.realizedRoom);
                Vector2 thrownPos = Vector2Handler.Read(ref br);
                Vector2? firstFrameTraceFromPos = Vector2NHandler.Read(ref br);
                IntVector2 throwDir = IntVector2Handler.Read(ref br);
                float frc = br.ReadSingle();
                if (obj != null && thrownBy != null)
                {
                    MonklandSteamManager.Log($"[Entity] Incoming Throw: {AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject).dist} thrownby {AbstractPhysicalObjectHK.GetField(thrownBy.abstractPhysicalObject).dist}");
                    if (obj is Rock || obj is Spear)
                    {
                        WeaponHK.SetNet();
                        (obj as Weapon).Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, RainWorldGameHK.mainGame.evenUpdate);
                    }
                    /*
                    else if (weapon is Spear s)
                    {
                        SpearHK.SetNet();
                        s.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, RainWorldGameHK.mainGame.evenUpdate);
                    }
                    */
                }
            }
        }

        public void ReadGrab(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning || RainWorldGameHK.mainGame == null || RainWorldGameHK.mainGame.world == null)
            { return; }
            int roomName = br.ReadInt32();//Read Room Name
            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null) { return; }
            if (abstractRoom.realizedRoom != null)
            {
                Creature grabber = DistHandler.ReadCreature(ref br, abstractRoom.realizedRoom);
                PhysicalObject grabbed = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);
                AbsPhyObjFields grabberSub = AbstractPhysicalObjectHK.GetField(grabber.abstractPhysicalObject);
                AbsPhyObjFields grabbedSub = AbstractPhysicalObjectHK.GetField(grabbed.abstractPhysicalObject);
                int graspUsed = br.ReadInt32();
                int grabbedChunk = br.ReadInt32();
                Creature.Grasp.Shareability shareability = (Creature.Grasp.Shareability)br.ReadInt32();
                float dominance = br.ReadSingle();
                bool pacifying = br.ReadBoolean();
                if (grabbed != null)
                {
                    if (grabbed is Player && grabber != null && !(grabber is Player))
                    {
                        grabberSub.owner = grabbedSub.owner;
                    }
                    else if (!(grabbed is Player))
                    {
                        grabbedSub.owner = sentPlayer.m_SteamID;
                    }
                }
                if (grabber == null || grabbed == null)
                {
                    MonklandSteamManager.Log("[Entity] Incomming grab: One or more targets not found!");
                    return;
                }
                if (grabber.grasps[graspUsed] != null && grabber.grasps[graspUsed].grabbed != null && AbstractPhysicalObjectHK.GetField(grabber.grasps[graspUsed].grabbed.abstractPhysicalObject).dist == grabbedSub.dist)
                {
                    MonklandSteamManager.Log($"[Entity] Incomming grab: Grab already satisfied, {grabberSub.dist} grabbed {grabbedSub.dist}");
                    return;
                }
                int preexisting = -1;
                for (int i = 0; i < grabber.grasps.Length; i++)
                {
                    if (grabber.grasps[i] != null && grabber.grasps[i].grabbed != null && AbstractPhysicalObjectHK.GetField(grabber.grasps[i].grabbed.abstractPhysicalObject).dist == grabbedSub.dist)
                    { preexisting = i; }
                }
                if (preexisting != -1 && preexisting != graspUsed)
                {
                    MonklandSteamManager.Log($"[Entity] Incomming grab: Found preexisting satifying grab, {grabberSub.dist} grabbed {grabbedSub.dist}");
                    CreatureHK.SetNet();
                    grabber.SwitchGrasps(preexisting, graspUsed); //NetSwitch
                    return;
                }
                MonklandSteamManager.Log($"[Entity] Incomming grab: GRAB! {grabberSub.dist} grabbed {grabbedSub.dist}");
                CreatureHK.SetNet();
                grabber.Grab(grabbed, graspUsed, grabbedChunk, shareability, dominance, false, pacifying); //NetGrab
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
                    if (abstractRoom.entities[i] != null && abstractRoom.entities[i] is AbstractPhysicalObject apo)
                    {
                        if (AbstractPhysicalObjectHK.GetField(apo).dist == grabber && (abstractRoom.entities[i] is AbstractCreature))
                        {
                            abstractGrabber = (abstractRoom.entities[i] as AbstractCreature);
                            for (int j = 0; j < (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects.Count; j++)
                            {
                                if ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j] != null && apo.stuckObjects[j].A != null && apo.stuckObjects[j].B != null && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].A).dist == grabber && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].B).dist == grabbed)
                                {
                                    MonklandSteamManager.Log($"[Entity] Incomming grab: Grab already satisfyed. {grabber} grabbed {grabbed}");
                                    abstractGrabbed = (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B;
                                    if (!(abstractGrabbed is AbstractCreature) || (abstractGrabbed as AbstractCreature).creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Slugcat)
                                        AbstractPhysicalObjectHK.GetField(abstractGrabbed).owner = sentPlayer.m_SteamID;
                                    return;
                                }
                            }
                        }
                        else if (AbstractPhysicalObjectHK.GetField(apo).dist == grabbed)
                        {
                            abstractGrabbed = (abstractRoom.entities[i] as AbstractPhysicalObject);
                            if (!(abstractGrabbed is AbstractCreature) || (abstractGrabbed as AbstractCreature).creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Slugcat)
                            { AbstractPhysicalObjectHK.GetField(abstractGrabbed).owner = sentPlayer.m_SteamID; }
                            for (int j = 0; j < apo.stuckObjects.Count; j++)
                            {
                                if (apo.stuckObjects[j] != null && apo.stuckObjects[j].A != null && apo.stuckObjects[j].B != null && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].A).dist == grabber && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].B).dist == grabbed)
                                {
                                    MonklandSteamManager.Log($"[Entity] Incomming grab: Grab already satisfyed. {grabber} grabbed {grabbed}");
                                    return;
                                }
                            }
                        }
                    }
                }
                if (abstractGrabber != null && abstractGrabbed != null)
                {
                    MonklandSteamManager.Log($"[Entity] Incomming grab: Abstract GRAB! {grabber} grabbed {grabbed}");
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
            if (!MonklandSteamManager.WorldManager.gameRunning || RainWorldGameHK.mainGame == null)
                return;
            int roomName = br.ReadInt32();//Read Room Name
            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
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
                    if (grabber.grasps[i] != null && grabber.grasps[i].grabbed != null && AbstractPhysicalObjectHK.GetField(grabber.grasps[i].grabbed.abstractPhysicalObject).dist == grabbed)
                    {
                        MonklandSteamManager.Log($"[Entity] Incomming release grasp: RELEASE! {AbstractPhysicalObjectHK.GetField(grabber.abstractPhysicalObject).dist} released {grabbed}");
                        CreatureHK.SetNet();
                        grabber.ReleaseGrasp(i); // NetRelease
                    }
                }
            }
            else
            {
                int grabber = br.ReadInt32();
                int grabbed = br.ReadInt32();
                for (int i = 0; i < abstractRoom.entities.Count; i++)
                {
                    if (abstractRoom.entities[i] != null && abstractRoom.entities[i] is AbstractPhysicalObject apo)
                    {
                        for (int j = 0; j < apo.stuckObjects.Count; j++)
                        {
                            if (apo.stuckObjects[j] != null && apo.stuckObjects[j].A != null && apo.stuckObjects[j].B != null
                                && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].A).dist == grabber && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].B).dist == grabbed)
                            {
                                MonklandSteamManager.Log($"[Entity] Incomming release grasp: Abstract RELEASE! {grabber} released {grabbed}");
                                apo.stuckObjects[j].Deactivate();
                            }
                        }
                    }
                }
                MonklandSteamManager.Log("[Entity] Incomming release grasp: One or more abstract targets not found!");
            }
        }

        public void ReadSwitch(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning || RainWorldGameHK.mainGame == null)
            { return; }

            //Read Room Name
            int roomName = br.ReadInt32();

            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
            { return; }

            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
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
            MonklandSteamManager.Log($"[Entity] Incomming switch: SWITCH! {AbstractPhysicalObjectHK.GetField(grabber.abstractPhysicalObject).dist}");
            CreatureHK.SetNet();
            grabber.SwitchGrasps(from, to);
        }

        public void ReadStick(BinaryReader br, CSteamID sentPlayer)
        {
            StickType type = (StickType)br.ReadByte();
            if (!MonklandSteamManager.WorldManager.gameRunning || RainWorldGameHK.mainGame == null)
                return;

            //Read Room Name
            int roomName = br.ReadInt32();

            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
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
                    AbsPhyObjFields sub = AbstractPhysicalObjectHK.GetField(abstractRoom.entities[i] as AbstractPhysicalObject);
                    if (sub.dist == distA) { A = abstractRoom.entities[i] as AbstractPhysicalObject; }
                    else if (sub.dist == distB) { B = abstractRoom.entities[i] as AbstractPhysicalObject; }
                }
            }
            if (A == null || B == null)
            {
                MonklandSteamManager.Log("[Entity] Incomming stick: Targets not found!");
                return;
            }
            for (int i = 0; i < A.stuckObjects.Count; i++)
            {
                if (AbstractPhysicalObjectHK.GetField(A.stuckObjects[i].B).dist == distB)
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
            MonklandSteamManager.Log($"[Entity] Incomming stick: STICK! {distA}, {distB}");
        }

        public void ReadDeactivate(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning || RainWorldGameHK.mainGame == null)
            { return; }
            int roomName = br.ReadInt32();//Read Room Name
            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null)
            { return; }
            int A = br.ReadInt32();//Read Grabber dist
            int B = br.ReadInt32();//Read Grabbed dist
            for (int i = 0; i < abstractRoom.entities.Count; i++)
            {
                if (abstractRoom.entities[i] != null && abstractRoom.entities[i] is AbstractPhysicalObject apo)
                {
                    for (int j = 0; j < apo.stuckObjects.Count; j++)
                    {
                        if (apo.stuckObjects[j] != null && apo.stuckObjects[j].A != null && apo.stuckObjects[j].B != null
                            && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].A).dist == A && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].B).dist == B)
                        {
                            MonklandSteamManager.Log($"[Entity] Incomming deactivate: DEACTIVATE! {A} released {B}");
                            apo.stuckObjects[j].Deactivate();
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

        #endregion Grasps and Sticks

        #endregion Incomming Packets
    }
}
