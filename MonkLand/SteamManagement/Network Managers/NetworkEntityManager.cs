using Monkland.Hooks;
using Monkland.Hooks.Entities;
using Monkland.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkEntityManager : NetworkManager
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

 
        public byte EntityHandler = 0;

        public enum EntityPacketType
        {
            Player,
            Creature,
            PhysicalObject
        }

        #region Packet Handler

        public override void RegisterHandlers()
        {
            EntityHandler = MonklandSteamManager.instance.RegisterHandler(ENTITY_CHANNEL, HandlePackets);
        }

        public void HandlePackets(BinaryReader reader, CSteamID sentPlayer)
        {
            EntityPacketType messageType = (EntityPacketType)reader.ReadByte();
            // up to 256 message types
            switch (messageType)
            {
                
                case EntityPacketType.Creature:
                    if (RainWorldGameHK.mainGame != null)
                    { GetCreature(reader, sentPlayer); }
                    return;

                case EntityPacketType.PhysicalObject:
                    if (RainWorldGameHK.mainGame != null)
                    { GetPhysicalObject(reader, sentPlayer); }
                    return;
            }
        }


        #endregion Packet Handler



        #region Outgoing Packets

       
        internal void SendCreature(Creature creature, List<ulong> targets, bool reliable = false)
        {
            /* Creature Packet */

            /* **************
             * packetType (1 byte)
             * roomName (~ byte)
             * distinguisher
             * type
             * PhysicalObject Packet
             * 
             * extra...
             * **************/

            if (creature == null || !(creature is Creature) || targets == null || targets.Count == 0 || creature.room == null || creature.room.abstractRoom == null || !isSynced(creature))
            {
                return;
            }

            // Prepare packet
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, EntityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            AbstractObjFields field = AbstractPhysicalObjectHK.GetField(creature.abstractPhysicalObject);

            // Packet Type
            writer.Write((byte)EntityPacketType.Creature);

            // RoomName
            writer.Write(creature.room.abstractRoom.name);

            // Distinguisher
            writer.Write(field.networkID);
            creature.abstractCreature.ID.number = field.networkID;

            writer.Write((byte)creature.abstractCreature.creatureTemplate.TopAncestor().type);

            // AbstractCreatureHandler
            AbstractCreatureHandler.Write(creature.abstractCreature, ref writer);
            // PhysicalObjectPacket
            PhysicalObjectHandler.Write(creature, ref writer);
            PhysicalObjectHandler.WriteSpecificCreature(creature, ref writer);

            reliable = true;

            MonklandUI.UpdateMessage($"{creature.abstractCreature.creatureTemplate.TopAncestor().type}\nO: {field.ownerName}\nID: {field.networkID % 10000:0000}", 1, creature.bodyChunks[0].pos, field.networkID, creature.room.abstractRoom.index, creature.ShortCutColor());


            // Finalize acket
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            // Send packet
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public void SendPhysicalObject(PhysicalObject physicalObject, List<ulong> targets, bool reliable = false)
        {
            /* PhysicalObject Packet */

            /* **************
             * packetType (1 byte)
             * roomName (~ byte)
             * distinguisher
             * type
             * PhysicalObject Packet
             * 
             * extra...
             * **************/

            if (physicalObject == null || physicalObject is Creature || targets == null || targets.Count == 0 || physicalObject.room == null || physicalObject.room.abstractRoom == null || !isSynced(physicalObject))
            {
                return;
            }
            string buildingPacket = "[--->Sending Physical Object] ";

            // Prepare packet
            MonklandSteamManager.DataPacket packet = MonklandSteamManager.instance.GetNewPacket(ENTITY_CHANNEL, EntityHandler);
            BinaryWriter writer = MonklandSteamManager.instance.GetWriterForPacket(packet);
            AbstractObjFields field = AbstractPhysicalObjectHK.GetField(physicalObject.abstractPhysicalObject);
            
            // Packet Type
            writer.Write((byte)EntityPacketType.PhysicalObject);

            // RoomName
            writer.Write(physicalObject.room.abstractRoom.name);
            buildingPacket += $" room[{physicalObject.room.abstractRoom.name}]";

            // Distinguisher
            //physicalObject.abstractPhysicalObject.ID.number = field.networkID;
            writer.Write(field.networkID);
            buildingPacket += $" networkID[{field.networkID}] || IDNumber [{physicalObject.abstractPhysicalObject.ID.number}]";

            AbstractPhysicalObject.AbstractObjectType absType = physicalObject.abstractPhysicalObject.type;
            writer.Write((byte)absType);
            buildingPacket += $" type[{absType}]";

            // AbstractPhysicalObjectPacket
            AbstractPhysicalObjectHandler.Write(physicalObject.abstractPhysicalObject, ref writer);
            // PhysicalObjectPacket
            PhysicalObjectHandler.Write(physicalObject, ref writer);
            PhysicalObjectHandler.WriteSpecificPhysicalObject(physicalObject, ref writer);

            buildingPacket += $" ID[{physicalObject.abstractPhysicalObject.ID}]";

            reliable = true;

            MonklandUI.UpdateMessage($"{absType}\nO:{field.ownerName}\nID: {field.networkID}", 1, physicalObject.bodyChunks[0].pos, field.networkID, physicalObject.room.abstractRoom.index, Color.white);

            MonklandUI.PacketLog(buildingPacket);

            // Finalize acket
            MonklandSteamManager.instance.FinalizeWriterToPacket(writer, packet);

            // Send packet
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


        #endregion Outgoing Packets

        #region Incomming Packets

        public void GetPhysicalObject(BinaryReader reader, CSteamID sentPlayer)
        {
            /* PhysicalObject Packet */

            /* **************
             * packetType (1 byte)
             * string roomName (~ byte)
             * distinguisher
             * type
             * PhysicalObject Packet
             * 
             * extra...
             * **************/

            if (!MonklandSteamManager.WorldManager.gameRunning)
            { return; }

            // Read Room Name
            string roomName = reader.ReadString();

            // No Rooms in Common
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
            { return; }

            // Room is null or unloaded
            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null || abstractRoom.realizedRoom.physicalObjects == null)
            { return; }
            string buildingPacket = "[<--- Receiving Physical Object] ";

            int networkID = reader.ReadInt32();
            buildingPacket += $" ID[{networkID}]";

            AbstractPhysicalObject.AbstractObjectType type = (AbstractPhysicalObject.AbstractObjectType)reader.ReadByte();
            buildingPacket += $" type[{type}]";
            
            foreach (AbstractWorldEntity entity in abstractRoom.entities)
            {
                if (entity is AbstractPhysicalObject absObject && absObject != null)
                {
                    AbstractObjFields field = AbstractPhysicalObjectHK.GetField(absObject);
                    // Match ID and Owner
                    if (field.networkID == networkID )
                    {
                        if (field.ownerID == sentPlayer.m_SteamID)
                        {
                            AbstractPhysicalObjectHandler.Read(absObject, ref reader);
                            buildingPacket += $" Found existing!!!!";
                            buildingPacket += $" (abs)RoomInt[{absObject.pos.room}]";
                            // This never happens ?
                            if (absObject.realizedObject == null)
                            {
                                // Making sure the object ins in the intended room
                                absObject.pos.room = abstractRoom.index;
                                absObject.RealizeInRoom();
                                buildingPacket += $" [realized]";

                            }
                            PhysicalObjectHandler.Read(absObject.realizedObject, ref reader);
                            PhysicalObjectHandler.ReadSpecificPhysicalObject(absObject.realizedObject, ref reader);
                            MonklandUI.UpdateMessage($"{absObject.type}\nO: {field.ownerName}\nID: {networkID % 10000:1000}", 2, absObject.realizedObject.bodyChunks[0].pos, networkID, abstractRoom.index, Color.white);
                            absObject.pos.room = abstractRoom.index;
                            buildingPacket += $" (realized)RoomName[{absObject.realizedObject.room.abstractRoom.name}]";

                            MonklandUI.PacketLog(buildingPacket);
                        }
                        return;
                    }
                }
            }

            /*
            for (int i = 0; i < abstractRoom.realizedRoom.physicalObjects.Length; i++)
            {
                for (int j = 0; j < abstractRoom.realizedRoom.physicalObjects[i].Count; j++)
                {
                    if (abstractRoom.realizedRoom.physicalObjects[i][j] != null)
                    {
                        try
                        {
                            AbstractObjFields field = AbstractPhysicalObjectHK.GetField(abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject);

                            if (field.networkID == networkID && field.ownerID == sentPlayer.m_SteamID)
                            {
                                PhysicalObject foundObject = abstractRoom.realizedRoom.physicalObjects[i][j];
                                try
                                {
                                    // ALWAYS FOLLOW THIS ORDER WHEN READING / WRITING PACKERS
                                    AbstractPhysicalObjectHandler.Read(foundObject.abstractPhysicalObject, ref reader);
                                    PhysicalObjectHandler.ReadSpecificPhysicalObject(foundObject, ref reader);
                                }
                                catch (Exception e) { Debug.Log("Read failed \n" + e); }
                                try
                                {
                                    MonklandUI.UpdateMessage($"{foundObject.abstractPhysicalObject.type}\nO: {sentPlayer.m_SteamID % 10000:0000}\nID: {networkID % 10000:1000}", 2, foundObject.bodyChunks[0].pos, networkID, abstractRoom.index, Color.white);
                                }
                                catch (Exception e) { Debug.Log("Log failed \n" + e); }

                                // Make sure it is in correct room (probably not necessary)
                                foundObject.abstractPhysicalObject.pos.room = abstractRoom.index;
                                MonklandUI.PacketLog(buildingPacket);
                                return;
                            }
                        }
                        catch (Exception e) { Debug.Log("Field failed \n" + e); }
                    }
                }
            }
            */
            buildingPacket += $" New Object!!!! ]";
            // PhysicalObject not found, realize new one.
            //AbstractPhysicalObject abstractObject = new AbstractPhysicalObject(RainWorldGameHK.mainGame.world, 0, null, new WorldCoordinate(abstractRoom.index, 0, 0, -1), new EntityID(-20, -20));
            AbstractPhysicalObject abstractObject = AbstractPhysicalObjectHandler.InitializeAbstractObject(RainWorldGameHK.mainGame.world, type, null, new WorldCoordinate(abstractRoom.index, 15, 25, -1), new EntityID(-1, networkID));

            // Filling the fields
            AbstractPhysicalObjectHandler.Read(abstractObject, ref reader);
            buildingPacket += $" (abs)type[{abstractObject.type}]";

            // Fill ID number before calling GetField()
            //abstractObject.ID.number = networkID;
            AbstractPhysicalObjectHK.GetField(abstractObject).ownerID = sentPlayer.m_SteamID;
            buildingPacket += $" (abs)IDNumber[{abstractObject.ID.number}]";

            // Realizing in room
            RainWorldGameHK.mainGame.world.GetAbstractRoom(abstractRoom.index).AddEntity(abstractObject);
            // Making sure the object ins in the intended room
            abstractObject.pos.room = abstractRoom.index;
            abstractObject.realizedObject = null;
            abstractObject.RealizeInRoom();
            buildingPacket += $" (realized)in Room[{abstractObject.realizedObject.room.abstractRoom.name}]";

            // Filling the fields
            PhysicalObjectHandler.Read(abstractObject.realizedObject, ref reader);
            PhysicalObjectHandler.ReadSpecificPhysicalObject(abstractObject.realizedObject, ref reader);
            
            buildingPacket += $"Owner {AbstractPhysicalObjectHK.GetField(abstractObject).ownerName}";
            MonklandUI.PacketLog(buildingPacket);

            //Debug.Log($"Creating new Obj {abstractObject.type}. Owner {SteamFriends.GetFriendPersonaName((CSteamID)AbstractPhysicalObjectHK.GetField(abstractObject).ownerID)}. Distinguisher {networkID} | ID number {abstractObject.ID.number} | { AbstractPhysicalObjectHK.GetField(abstractObject).networkID}. Room {RainWorldGameHK.mainGame.world.GetAbstractRoom(abstractObject.pos.room).name}");

        }

        public void GetCreature(BinaryReader reader, CSteamID sentPlayer)
        {
            /* Creature Packet */

            /* **************
             * packetType (1 byte)
             * string roomName (~ byte)
             * distinguisher
             * PhysicalObject Packet
             * 
             * extra...
             * **************/

            if (!MonklandSteamManager.WorldManager.gameRunning)
            { return; }

            // Read Room Name
            string roomName = reader.ReadString();

            // No Rooms in Common
            if (!MonklandSteamManager.WorldManager.commonRooms.ContainsKey(roomName))
            { return; }

            // Room is null or unloaded
            AbstractRoom abstractRoom = RainWorldGameHK.mainGame.world.GetAbstractRoom(roomName);
            if (abstractRoom == null || abstractRoom.realizedRoom == null || abstractRoom.realizedRoom.physicalObjects == null)
            { return; }

            int networkID = reader.ReadInt32();
            CreatureTemplate.Type type = (CreatureTemplate.Type)reader.ReadByte();

            foreach (AbstractCreature absCreature in abstractRoom.creatures)
            {
                AbstractObjFields field = AbstractPhysicalObjectHK.GetField(absCreature);
                if (field.networkID == networkID && absCreature.realizedCreature != null)
                {
                    if (field.ownerID == sentPlayer.m_SteamID)
                    {
                        //Debug.Log($"Read creature matches. Owner {SteamFriends.GetFriendPersonaName((CSteamID)AbstractPhysicalObjectHK.GetField(absCreature).owner)}. Distinguisher read {distinguisher} |  getfield {AbstractPhysicalObjectHK.GetField(absCreature).dist}");

                        // Read Player
                        Creature realizedCreature = absCreature.realizedCreature;

                        // ALWAYS FOLLOW THIS ORDER WHEN READING / WRITING PACKERS
                        AbstractCreatureHandler.Read(absCreature, ref reader);
                        PhysicalObjectHandler.Read(realizedCreature, ref reader);
                        PhysicalObjectHandler.ReadSpecificCreature(realizedCreature, ref reader);

                        MonklandUI.UpdateMessage($"{absCreature.creatureTemplate.type}\nO: {field.ownerName}\nID: {networkID % 10000:1000}", 2, realizedCreature.bodyChunks[0].pos, networkID, abstractRoom.index, Color.white); 
                        absCreature.pos.room = abstractRoom.index;
                    }

                    return;
                }
            }

            // PhysicalObject not found, realize new one.
            AbstractCreature abstractCreature = new AbstractCreature(RainWorldGameHK.mainGame.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(abstractRoom.index, 15, 25, -1), new EntityID(-1, 0));
            // Filling the fields
            AbstractCreatureHandler.Read(abstractCreature, ref reader);
            // Create PlayerState if it is player (should be moved elsewhere probably)
            if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            {
                abstractCreature.state = new PlayerState(abstractCreature, 1, 0, false);
            }
            // Fill ID number before calling GetField()
            //abstractCreature.ID.number = networkID;
            AbstractPhysicalObjectHK.GetField(abstractCreature).ownerID = sentPlayer.m_SteamID;

            // Realizing Creature in Room
            RainWorldGameHK.mainGame.world.GetAbstractRoom(abstractRoom.index).AddEntity(abstractCreature);
            // Making sure player is in the intended room
            abstractCreature.pos.room = abstractRoom.index;
            abstractCreature.RealizeInRoom();

            // Filling the fields
            PhysicalObjectHandler.Read(abstractCreature.realizedObject, ref reader);
            PhysicalObjectHandler.ReadSpecificCreature(abstractCreature.realizedCreature, ref reader);

            Debug.Log($"Creating new Creature. Owner {AbstractPhysicalObjectHK.GetField(abstractCreature).ownerName}. Distinguisher {networkID} | ID number {abstractCreature.ID.number} | { AbstractPhysicalObjectHK.GetField(abstractCreature).networkID}. Room {RainWorldGameHK.mainGame.world.GetAbstractRoom(abstractCreature.pos.room).name}");

        }


        #endregion Incomming Packets
    }
}
