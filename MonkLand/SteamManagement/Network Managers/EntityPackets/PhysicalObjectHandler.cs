using Monkland.Hooks.Entities;
using System;
using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal static class PhysicalObjectHandler
    {
        //public static void SyncPhysicalObject(PhysicalObject self, BodyChunk[] bodyChunks) => self.bodyChunks = bodyChunks;


        /* **************
        * PhysicalObject packet (6 bytes + bodyChunkPacket + bodyChunkConnections)
        * 
        * (float) bounce                  (4 byte)
        * (bool)  canBeHitByWeapons       (1 byte)
        * (byte)  numberOfChunks          (1 byte)
        * (BODYCHUNKS PACKET)
        * (byte)  numberOfChunkConnectino (1 byte)
        * (BODYCHUNKSCONNECTION PACKET)
        * **************/

        /// <summary>
        /// Writes physicalObject packet 
        /// <para>[ABSTRACTPHYSICALOBJ | float bounce | bool canBeHitByWeapons | byte numberOfChunks | BODYCHUNK(s) | byte numberOfConnections | BODYCHUNKCONN(s)]</para>
        /// <para>[min 74 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(PhysicalObject physicalObject, ref BinaryWriter writer)
        {
            //AbstractPhysicalObjectHandler.Write(physicalObject.abstractPhysicalObject, ref writer);

            //writer.Write(physicalObject.appendages.Count);
            //foreach (PhysicalObject.Appendage app in physicalObject.appendages)
            //{
            //    AppendageHandler.Write(app, ref writer);
            //}

            //DistHandler.Write(physicalObject, ref writer);

            
            writer.Write(physicalObject.bounce);
            writer.Write(physicalObject.canBeHitByWeapons);

            writer.Write((byte)physicalObject.bodyChunks.Length);
            for(int i = 0; i < physicalObject.bodyChunks.Length; i++)
            {
                BodyChunkHandler.Write(physicalObject.bodyChunks[i], ref writer);
            }

            /*
            writer.Write((byte)physicalObject.bodyChunkConnections.Length);

            foreach (PhysicalObject.BodyChunkConnection con in physicalObject.bodyChunkConnections)
            {
                BodyChunkConnectionHandler.Write(con, ref writer);
            }
            */
        }

        public static void Read(PhysicalObject physicalObject, ref BinaryReader reader)
        {
            physicalObject.bounce = reader.ReadSingle();
            physicalObject.canBeHitByWeapons = reader.ReadBoolean();

            int numberOfChunks = reader.ReadByte();
            BodyChunk[] chunks = physicalObject.bodyChunks;

            for (int a = 0; a < numberOfChunks; a++)
            {
                BodyChunkHandler.Read(chunks[a], ref reader);
            }
            /*
            int numberOFConnections = reader.ReadInt32();
            for (int a = 0; a < numberOFConnections; a++)
            {
                if (physicalObject.bodyChunkConnections[a] != null)
                {
                    physicalObject.bodyChunkConnections[a] = BodyChunkConnectionHandler.Read(physicalObject.bodyChunkConnections[a], ref reader);
                }
            }
            */


            // Alternative
            /*
            if (physicalObject.bodyChunks.Length < numberOfChunks)
            {
                chunks = new BodyChunk[numberOfChunks];
            }
            for (int a = 0; a < chunks.Length; a++)
            {
                if (chunks[a] == null)
                {
                    chunks[a] = new BodyChunk(physicalObject, a, new Vector2(0, 0), 1, 1);
                }
                chunks[a] = BodyChunkHandler.Read(chunks[a], ref reader);
            }

            physicalObject.bodyChunks = chunks;

            int numberOfConnections = reader.ReadByte();
            PhysicalObject.BodyChunkConnection[] bodyChunkConnections = physicalObject.bodyChunkConnections;
            if (physicalObject.bodyChunkConnections.Length < numberOfConnections)
            {
                bodyChunkConnections = new PhysicalObject.BodyChunkConnection[numberOfConnections];
            }
            for (int a = 0; a < physicalObject.bodyChunks.Length -1; a++)
            {
                if (bodyChunkConnections[a] == null)
                {
                    bodyChunkConnections[a] = new PhysicalObject.BodyChunkConnection(physicalObject.bodyChunks[a], physicalObject.bodyChunks[a+1], 1, )
                }
                bodyChunkConnections[a] = BodyChunkConnectionHandler.Read(physicalObject.bodyChunkConnections[a], ref reader);
            }
            physicalObject.bodyChunkConnections = body
            */
        }



        public static void ReadSpecificPhysicalObject(PhysicalObject physicalObject, ref BinaryReader reader)
        {
            AbstractPhysicalObject.AbstractObjectType type = physicalObject.abstractPhysicalObject.type;
            switch (type)
            {
                case AbstractPhysicalObject.AbstractObjectType.Creature:
                    break;
                case AbstractPhysicalObject.AbstractObjectType.Rock:
                    WeaponHandler.Read(physicalObject as Rock, ref reader);
                    break;
                case AbstractPhysicalObject.AbstractObjectType.Spear:
                    SpearHandler.Read(physicalObject as Spear, ref reader);
                    break;
                case AbstractPhysicalObject.AbstractObjectType.FlareBomb:
                    break;
                case AbstractPhysicalObject.AbstractObjectType.VultureMask:
                    break;
                case AbstractPhysicalObject.AbstractObjectType.PuffBall:
                    break;
                    /* ... */
            }

        }

        public static void WriteSpecificPhysicalObject(PhysicalObject physicalObject, ref BinaryWriter writer)
        {
            AbstractPhysicalObject.AbstractObjectType type = physicalObject.abstractPhysicalObject.type;
            switch (type)
            {
                case AbstractPhysicalObject.AbstractObjectType.Creature:
                    break;
                case AbstractPhysicalObject.AbstractObjectType.Rock:
                    WeaponHandler.Write(physicalObject as Rock, ref writer);
                    break;
                case AbstractPhysicalObject.AbstractObjectType.Spear:
                    SpearHandler.Write(physicalObject as Spear, ref writer);
                    break;
                case AbstractPhysicalObject.AbstractObjectType.FlareBomb:
                    break;
                case AbstractPhysicalObject.AbstractObjectType.VultureMask:
                    break;
                    /* ... */
            }
        }

        internal static void WriteSpecificCreature(Creature creature, ref BinaryWriter writer)
        {
            switch (creature.abstractCreature.creatureTemplate.TopAncestor().type)
            {
                case CreatureTemplate.Type.StandardGroundCreature:
                    break;
                case CreatureTemplate.Type.Slugcat:
                    PlayerHandler.Write(creature as Player, ref writer);
                    break;
                case CreatureTemplate.Type.LizardTemplate:
                    break;
                    /* ... */

            }
        }
        internal static void ReadSpecificCreature(Creature creature, ref BinaryReader reader)
        {
            switch (creature.abstractCreature.creatureTemplate.TopAncestor().type)
            {
                case CreatureTemplate.Type.StandardGroundCreature:
                    break;
                case CreatureTemplate.Type.Slugcat:
                    PlayerHandler.Read(creature as Player, ref reader);
                    break;
                case CreatureTemplate.Type.LizardTemplate:
                    break;
                case CreatureTemplate.Type.PinkLizard:
                    break;
                    /* ... */

            }

        }

        // Unused
        private static class AppendageHandler
        {
            public static PhysicalObject.Appendage Read(PhysicalObject.Appendage appendage, ref BinaryReader reader)
            {
                appendage.appIndex = reader.ReadInt32();
                appendage.canBeHit = reader.ReadBoolean();
                int numberOfSegments = reader.ReadInt32();
                appendage.segments = new Vector2[numberOfSegments];
                for (int a = 0; a < numberOfSegments; a++)
                { appendage.segments[a] = Vector2Handler.Read(ref reader); }
                appendage.totalLength = reader.ReadSingle();
                return appendage;
            }

            public static void Write(PhysicalObject.Appendage appendage, ref BinaryWriter writer)
            {
                writer.Write(appendage.appIndex);
                writer.Write(appendage.canBeHit);
                writer.Write(appendage.segments.Length);
                for (int a = 0; a < appendage.segments.Length; a++)
                { Vector2Handler.Write(appendage.segments[a], ref writer); }
                writer.Write(appendage.totalLength);
            }
        }

    }
}
