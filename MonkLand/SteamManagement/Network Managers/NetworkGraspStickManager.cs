using Monkland.Hooks;
using Monkland.Hooks.Entities;
using RWCustom;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static Monkland.SteamManagement.NetworkEntityManager;

namespace Monkland.SteamManagement
{
    class NetworkGraspStickManager : NetworkManager
    {
        public byte GraspStickHandler = 0;

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

        private enum GraspStickPacketType
        {
            EstablishGrasp,
            SwitchGrasps,
            ReleaseGrasp,
            Hit,
            Throw,
            Stick,
            Deactivate,
            Violence,
            WorldLoadOrExit,
            RainSync,
            RealizeRoom,
            AbstractizeRoom

        }

        private static bool isManager { get { return playerID == managerID; } }

        public override void RegisterHandlers()
        {
            GraspStickHandler = MonklandSteamManager.instance.RegisterHandler(ENTITY_CHANNEL, HandlePackets);
        }

        public void HandlePackets(BinaryReader reader, CSteamID sentPlayer)
        {
            GraspStickPacketType messageType = (GraspStickPacketType)reader.ReadByte();

            switch (messageType)
            {
                case GraspStickPacketType.EstablishGrasp:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadGrab(reader, sentPlayer); }
                    return;

                case GraspStickPacketType.SwitchGrasps:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadSwitch(reader, sentPlayer); }
                    return;

                case GraspStickPacketType.ReleaseGrasp:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadRelease(reader, sentPlayer); }
                    return;

                case GraspStickPacketType.Hit:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadHit(reader, sentPlayer); }
                    return;

                case GraspStickPacketType.Throw:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadThrow(reader, sentPlayer); }
                    return;

                case GraspStickPacketType.Stick:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadStick(reader, sentPlayer); }
                    return;

                case GraspStickPacketType.Deactivate:
                    if (RainWorldGameHK.mainGame != null)
                    { ReadDeactivate(reader, sentPlayer); }
                    return;
            }
        }

        #region Outgoing Packets
        #region Grasps and Sticks

        public void SendGrab(Creature.Grasp grasp)
        {
            if (grasp == null || grasp.grabber == null || grasp.grabbed == null || !isSynced(grasp.grabber) || !isSynced(grasp.grabbed))
            { return; }

            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grasp.grabber.room.abstractRoom.name)) { return; }

            AbstractObjFields grabberField = AbstractPhysicalObjectHK.GetField(grasp.grabber.abstractPhysicalObject);

            if (grabberField.ownerID != NetworkGameManager.playerID) { return; }

            AbstractObjFields grabbedField = AbstractPhysicalObjectHK.GetField(grasp.grabbed.abstractPhysicalObject);

            MonklandSteamManager.Log($"[Entity] Sending Grab: {grabberField.networkID} grabbed {grabbedField.networkID}");
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)GraspStickPacketType.EstablishGrasp);
            writer.Write(grasp.grabber.room.abstractRoom.name);
            writer.Write(grabberField.networkID);
            writer.Write(grabbedField.networkID);
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
                    if (target != playerID) { MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendUnreliableNoDelay); }
                }
            }
            //One of our creatures is grabbing another player so the grabber should belong to the grabbed
            if ((grasp.grabbed is Player) && !(grasp.grabber is Player))
            {
                grabberField.ownerID = grabbedField.ownerID;
            }
            //One of our creatures is the grabber so we should take ownership of the grabbed
            else if (!(grasp.grabbed is Player))
            {
                grabbedField.ownerID = grabberField.ownerID;
            }
        }

        public void SendSwitch(Creature grabber, int from, int to)
        {
            if (grabber == null || !isSynced(grabber)) { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grabber.room.abstractRoom.name)) { return; }
            AbstractObjFields grabberSub = AbstractPhysicalObjectHK.GetField(grabber.abstractPhysicalObject);
            if (grabberSub.ownerID != NetworkGameManager.playerID) { return; }
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)GraspStickPacketType.SwitchGrasps);
            writer.Write(grabber.room.abstractRoom.name);
            writer.Write(grabberSub.networkID);
            writer.Write(from);
            writer.Write(to);
            MonklandSteamManager.Log($"[Entity] Sending Switch: {grabberSub.networkID} switched {from} => {to}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[grabber.room.abstractRoom.name])
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
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(grasp.grabber.room.abstractRoom.name)) { return; }
            AbstractObjFields grabberSub = AbstractPhysicalObjectHK.GetField(grasp.grabber.abstractPhysicalObject);
            if (grabberSub.ownerID != NetworkGameManager.playerID) { return; }
            AbstractObjFields grabbedSub = AbstractPhysicalObjectHK.GetField(grasp.grabbed.abstractPhysicalObject);
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)GraspStickPacketType.ReleaseGrasp);
            writer.Write(grasp.grabber.room.abstractRoom.name);
            writer.Write(grabberSub.networkID);
            writer.Write(grabbedSub.networkID);
            MonklandSteamManager.Log($"[Entity] Sending Release: {grabberSub.networkID} released {grabbedSub.networkID}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[grasp.grabber.room.abstractRoom.name])
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
            if (hit == null || obj == null || !isSynced(obj.abstractPhysicalObject) || !isSynced(hit.abstractPhysicalObject)) { return; }
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(obj.room.abstractRoom.name)) { return; }

            AbstractObjFields objField = AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject);
            AbstractObjFields hitField = AbstractPhysicalObjectHK.GetField(hit.abstractPhysicalObject);

            if (objField.ownerID != NetworkGameManager.playerID) { return; }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)GraspStickPacketType.Hit);
            writer.Write(obj.room.abstractRoom.name);
            writer.Write(objField.networkID);
            writer.Write(hitField.networkID);

            if (chunk != null) { writer.Write(chunk.index); }
            else { writer.Write(-1); }

            MonklandSteamManager.Log($"[Entity] Sending Hit: {objField.networkID} hit {hitField.networkID}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[obj.room.abstractRoom.name])
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
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(thrown.room.abstractRoom.name))
            {
                return;
            }

            AbstractObjFields thrownField = AbstractPhysicalObjectHK.GetField(thrown.abstractPhysicalObject);

            if (thrownField.ownerID != NetworkGameManager.playerID)
            {
                return;
            }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            AbstractObjFields thrownBySub = AbstractPhysicalObjectHK.GetField(thrownBy.abstractPhysicalObject);

            writer.Write((byte)GraspStickPacketType.Throw);
            writer.Write(thrown.room.abstractRoom.name);
            writer.Write(thrownField.networkID);
            writer.Write(thrownBySub.networkID);
            Vector2Handler.Write(thrownPos, ref writer);
            Vector2NHandler.Write(firstFrameTraceFromPos, ref writer);
            IntVector2Handler.Write(throwDir, ref writer);
            writer.Write(frc);

            MonklandSteamManager.Log($"[Entity] Sending Throw: {thrownField.networkID} thrown by {thrownBySub.networkID}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[thrown.room.abstractRoom.name])

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
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.name)) { return; }

            AbstractObjFields AField = AbstractPhysicalObjectHK.GetField(A);
            AbstractObjFields BField = AbstractPhysicalObjectHK.GetField(B);

            if (AField.ownerID != NetworkGameManager.playerID && BField.ownerID != NetworkGameManager.playerID) { return; }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)GraspStickPacketType.Stick);
            writer.Write((byte)StickType.Spear);
            writer.Write(room.name);
            writer.Write(AField.networkID);
            writer.Write(BField.networkID);
            writer.Write(chunk);
            writer.Write(bodyPart);
            writer.Write(angle);

            MonklandSteamManager.Log($"[Entity] Sending Spear Stick: {AField.networkID}, {BField.networkID}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.name])
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
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.name)) { return; }

            AbstractObjFields AField = AbstractPhysicalObjectHK.GetField(A);
            AbstractObjFields BField = AbstractPhysicalObjectHK.GetField(B);

            if (AField.ownerID != NetworkGameManager.playerID && BField.ownerID != NetworkGameManager.playerID) { return; }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)GraspStickPacketType.Stick);
            writer.Write((byte)StickType.SpearAppendage);
            writer.Write(room.name);
            writer.Write(AField.networkID);
            writer.Write(BField.networkID);
            writer.Write(appendage);
            writer.Write(prevSeg);
            writer.Write(distanceToNext);
            writer.Write(angle);

            MonklandSteamManager.Log($"[Entity] Sending Spear Appendage Stick: {AField.networkID}, {BField.networkID}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.name])
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
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.name)) { return; }

            AbstractObjFields AField = AbstractPhysicalObjectHK.GetField(A);
            AbstractObjFields BField = AbstractPhysicalObjectHK.GetField(B);

            if (AField.ownerID != NetworkGameManager.playerID && BField.ownerID != NetworkGameManager.playerID) { return; }

            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);

            writer.Write((byte)GraspStickPacketType.Stick);
            writer.Write((byte)StickType.SpearImpaled);
            writer.Write(room.name);
            writer.Write(AField.networkID);
            writer.Write(BField.networkID);
            writer.Write(chunk);
            writer.Write(onSpearPosition);

            MonklandSteamManager.Log($"[Entity] Sending Spear Impaled Stick: {AField.networkID}, {BField.networkID}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.name])
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
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(room.name)) { return; }
            AbstractObjFields ASub = AbstractPhysicalObjectHK.GetField(A);
            AbstractObjFields BSub = AbstractPhysicalObjectHK.GetField(B);
            if (ASub.ownerID != NetworkGameManager.playerID && BSub.ownerID != NetworkGameManager.playerID) { return; }
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, GraspStickHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            writer.Write((byte)GraspStickPacketType.Deactivate);
            writer.Write(room.name);
            writer.Write(ASub.networkID);
            writer.Write(BSub.networkID);
            MonklandSteamManager.Log($"[Entity] Sending Stick Deactivate: {ASub.networkID}, {BSub.networkID}");
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);
            foreach (ulong target in MonklandSteamManager.WorldManager.commonRooms[room.name])
            {
                if (MonklandSteamManager.WorldManager.ingamePlayers.Contains(target))
                {
                    if (target != playerID)
                    { MonklandSteamManager.instance.SendPacket(packet, (CSteamID)target, EP2PSend.k_EP2PSendReliable); }
                }
            }
        }

        #endregion Grasps and Sticks
        #endregion Outgoing Packets

        #region Incomming Packets

        #region Grasps and Sticks

        public void ReadHit(BinaryReader br, CSteamID sentPlayer)
        {
            //Read Room Name
            string roomName = br.ReadString();
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
                        MonklandSteamManager.Log($"[Entity] Incoming hit: {AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject).networkID} hit {hit.abstractPhysicalObject.type} [{AbstractPhysicalObjectHK.GetField(hit.abstractPhysicalObject).networkID}]");
                    }
                    else
                    {
                        MonklandSteamManager.Log($"[Entity] Incoming hit: {AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject).networkID} hit something");
                    }
                    if (obj is Rock || obj is Spear)
                    {
                        WeaponHK.SetNet();
                        if (hit != null && chunk != -1)
                        {
                            (obj as Weapon).HitSomething(new SharedPhysics.CollisionResult(hit, hit.bodyChunks[chunk], null, true, default), RainWorldGameHK.mainGame.evenUpdate);
                        }
                        else
                        {
                            (obj as Weapon).HitSomething(new SharedPhysics.CollisionResult(hit, hit is Creature ? hit.firstChunk : null, null, true, default), RainWorldGameHK.mainGame.evenUpdate);
                        }
                    }
                }
            }
        }

        public void ReadThrow(BinaryReader br, CSteamID sentPlayer)
        {
            //Read Room Name
            string roomName = br.ReadString();
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
                    MonklandSteamManager.Log($"[Entity] Incoming Throw: {obj.abstractPhysicalObject.type}[{AbstractPhysicalObjectHK.GetField(obj.abstractPhysicalObject).networkID}] thrownby {thrownBy.abstractCreature.creatureTemplate.TopAncestor().type} [{AbstractPhysicalObjectHK.GetField(thrownBy.abstractPhysicalObject).networkID}]");
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

            string roomName = br.ReadString();//Read Room Name

            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null) { return; }
            if (abstractRoom.realizedRoom != null)
            {
                Creature grabber = DistHandler.ReadCreature(ref br, abstractRoom.realizedRoom);
                PhysicalObject grabbed = DistHandler.ReadPhysicalObject(ref br, abstractRoom.realizedRoom);

                AbstractObjFields grabberField = AbstractPhysicalObjectHK.GetField(grabber.abstractPhysicalObject);
                AbstractObjFields grabbedField = AbstractPhysicalObjectHK.GetField(grabbed.abstractPhysicalObject);

                int graspUsed = br.ReadInt32();
                int grabbedChunk = br.ReadInt32();
                Creature.Grasp.Shareability shareability = (Creature.Grasp.Shareability)br.ReadInt32();
                float dominance = br.ReadSingle();
                bool pacifying = br.ReadBoolean();

                if (grabbed != null)
                {
                    if (grabbed is Player && grabber != null && !(grabber is Player))
                    {
                        grabberField.ownerID = grabbedField.ownerID;
                    }
                    else if (!(grabbed is Player))
                    {
                        grabbedField.ownerID = sentPlayer.m_SteamID;
                    }
                }
                if (grabber == null || grabbed == null)
                {
                    MonklandSteamManager.Log("[Entity] Incoming grab: One or more targets not found!");
                    return;
                }
                if (grabber.grasps[graspUsed] != null && grabber.grasps[graspUsed].grabbed != null && AbstractPhysicalObjectHK.GetField(grabber.grasps[graspUsed].grabbed.abstractPhysicalObject).networkID == grabbedField.networkID)
                {
                    MonklandSteamManager.Log($"[Entity] Incoming grab: Grab already satisfied, {grabber.abstractPhysicalObject.type}[{grabberField.networkID}] grabbed {grabbed.abstractPhysicalObject.type}[{grabbedField.networkID}]");
                    return;
                }
                int preexisting = -1;
                for (int i = 0; i < grabber.grasps.Length; i++)
                {
                    if (grabber.grasps[i] != null && grabber.grasps[i].grabbed != null && AbstractPhysicalObjectHK.GetField(grabber.grasps[i].grabbed.abstractPhysicalObject).networkID == grabbedField.networkID)
                    { preexisting = i; }
                }
                if (preexisting != -1 && preexisting != graspUsed)
                {
                    MonklandSteamManager.Log($"[Entity] Incoming grab: Found preexisting satifying grab, {grabberField.networkID} grabbed {grabbedField.networkID}");
                    CreatureHK.SetNet();
                    grabber.SwitchGrasps(preexisting, graspUsed); //NetSwitch
                    return;
                }
                MonklandSteamManager.Log($"[Entity] Incoming grab: GRAB! {grabber.abstractPhysicalObject.type}[{grabberField.networkID}] grabbed {grabbed.abstractPhysicalObject.type}[{grabbedField.networkID}]");
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
                        if (AbstractPhysicalObjectHK.GetField(apo).networkID == grabber && (abstractRoom.entities[i] is AbstractCreature))
                        {
                            abstractGrabber = (abstractRoom.entities[i] as AbstractCreature);
                            for (int j = 0; j < (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects.Count; j++)
                            {
                                if ((abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j] != null && apo.stuckObjects[j].A != null && apo.stuckObjects[j].B != null && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].A).networkID == grabber && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].B).networkID == grabbed)
                                {
                                    MonklandSteamManager.Log($"[Entity] Incoming grab: Grab already satisfyed. {grabber} grabbed {grabbed}");
                                    abstractGrabbed = (abstractRoom.entities[i] as AbstractPhysicalObject).stuckObjects[j].B;
                                    if (!(abstractGrabbed is AbstractCreature) || (abstractGrabbed as AbstractCreature).creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Slugcat)
                                        AbstractPhysicalObjectHK.GetField(abstractGrabbed).ownerID = sentPlayer.m_SteamID;
                                    return;
                                }
                            }
                        }
                        else if (AbstractPhysicalObjectHK.GetField(apo).networkID == grabbed)
                        {
                            abstractGrabbed = (abstractRoom.entities[i] as AbstractPhysicalObject);
                            if (!(abstractGrabbed is AbstractCreature) || (abstractGrabbed as AbstractCreature).creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Slugcat)
                            { AbstractPhysicalObjectHK.GetField(abstractGrabbed).ownerID = sentPlayer.m_SteamID; }
                            for (int j = 0; j < apo.stuckObjects.Count; j++)
                            {
                                if (apo.stuckObjects[j] != null && apo.stuckObjects[j].A != null && apo.stuckObjects[j].B != null && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].A).networkID == grabber && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].B).networkID == grabbed)
                                {
                                    MonklandSteamManager.Log($"[Entity] Incoming grab: Grab already satisfyed. {grabber} grabbed {grabbed}");
                                    return;
                                }
                            }
                        }
                    }
                }
                if (abstractGrabber != null && abstractGrabbed != null)
                {
                    MonklandSteamManager.Log($"[Entity] Incoming grab: Abstract GRAB! {grabber} grabbed {grabbed}");
                    new AbstractPhysicalObject.CreatureGripStick(abstractGrabber, abstractGrabbed, graspUsed, true);
                }
                else
                {
                    MonklandSteamManager.Log("[Entity] Incoming grab: One or more abstract targets not found!");
                }
            }
        }

        public void ReadRelease(BinaryReader br, CSteamID sentPlayer)
        {
            if (!MonklandSteamManager.WorldManager.gameRunning || RainWorldGameHK.mainGame == null)
                return;
            string roomName = br.ReadString();//Read Room Name
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
                    if (grabber.grasps[i] != null && grabber.grasps[i].grabbed != null && AbstractPhysicalObjectHK.GetField(grabber.grasps[i].grabbed.abstractPhysicalObject).networkID == grabbed)
                    {
                        MonklandSteamManager.Log($"[Entity] Incomming release grasp: RELEASE! {AbstractPhysicalObjectHK.GetField(grabber.abstractPhysicalObject).networkID} released {grabbed}");
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
                                && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].A).networkID == grabber && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].B).networkID == grabbed)
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
            string roomName = br.ReadString();

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
            MonklandSteamManager.Log($"[Entity] Incomming switch: SWITCH! {AbstractPhysicalObjectHK.GetField(grabber.abstractPhysicalObject).networkID}");
            CreatureHK.SetNet();
            grabber.SwitchGrasps(from, to);
        }

        public void ReadStick(BinaryReader br, CSteamID sentPlayer)
        {
            StickType type = (StickType)br.ReadByte();
            if (!MonklandSteamManager.WorldManager.gameRunning || RainWorldGameHK.mainGame == null)
                return;

            //Read Room Name
            string roomName = br.ReadString();

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
                    AbstractObjFields sub = AbstractPhysicalObjectHK.GetField(abstractRoom.entities[i] as AbstractPhysicalObject);
                    if (sub.networkID == distA) { A = abstractRoom.entities[i] as AbstractPhysicalObject; }
                    else if (sub.networkID == distB) { B = abstractRoom.entities[i] as AbstractPhysicalObject; }
                }
            }
            if (A == null || B == null)
            {
                MonklandSteamManager.Log("[Entity] Incomming stick: Targets not found!");
                return;
            }
            for (int i = 0; i < A.stuckObjects.Count; i++)
            {
                if (AbstractPhysicalObjectHK.GetField(A.stuckObjects[i].B).networkID == distB)
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
            //Read Room Name
            string roomName = br.ReadString();
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
                            && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].A).networkID == A && AbstractPhysicalObjectHK.GetField(apo.stuckObjects[j].B).networkID == B)
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
