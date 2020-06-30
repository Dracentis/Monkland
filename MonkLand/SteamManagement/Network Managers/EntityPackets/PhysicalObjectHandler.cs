using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using RWCustom;

namespace Monkland.SteamManagement
{
    static class PhysicalObjectHandler
    {
        public static PhysicalObject Read(PhysicalObject physicalObject, ref BinaryReader reader)
        {
            physicalObject.abstractPhysicalObject = AbstractPhysicalObjectHandler.Read(physicalObject.abstractPhysicalObject, ref reader);
            //int numberOfAppendages = reader.ReadInt32();
            //if (physicalObject.appendages.Count < numberOfAppendages)
            //{
            //    physicalObject.appendages = new List<PhysicalObject.Appendage>();
            //}
            //for (int a = 0; a < numberOfAppendages; a++)
            //{
            //    physicalObject.appendages[a] = AppendageHandler.Read(physicalObject.appendages[a], ref reader);
            //}
            int numberOfChunks = reader.ReadInt32();
            BodyChunk[] chunks = physicalObject.bodyChunks;
            if (physicalObject.bodyChunks.Length < numberOfChunks)
            {
                chunks = new BodyChunk[numberOfChunks];
            }
            for (int a = 0; a < numberOfChunks; a++)
            {
                chunks[a] = BodyChunkHandler.Read(chunks[a], ref reader);
            }
            (physicalObject as PhysicalObject as Patches.patch_PhysicalObject).Sync(chunks);
            int numberOFConnections = reader.ReadInt32();
            if (physicalObject.bodyChunkConnections.Length < numberOFConnections)
            {
                physicalObject.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[numberOFConnections];
            }
            for (int a = 0; a < numberOFConnections; a++)
            {
                physicalObject.bodyChunkConnections[a] = BodyChunkConnectionHandler.Read(physicalObject.bodyChunkConnections[a], ref reader);
            }
            physicalObject.bounce = reader.ReadSingle();
            physicalObject.canBeHitByWeapons = reader.ReadBoolean();
            return physicalObject;
        }
        public static Spear Read(Spear physicalObject, ref BinaryReader reader)
        {
            physicalObject.abstractPhysicalObject = AbstractPhysicalObjectHandler.Read(physicalObject.abstractPhysicalObject, ref reader);
            //int numberOfAppendages = reader.ReadInt32();
            //if (physicalObject.appendages.Count < numberOfAppendages)
            //{
            //    physicalObject.appendages = new List<PhysicalObject.Appendage>();
            //}
            //for (int a = 0; a < numberOfAppendages; a++)
            //{
            //    physicalObject.appendages[a] = AppendageHandler.Read(physicalObject.appendages[a], ref reader);
            //}
            int numberOfChunks = reader.ReadInt32();
            BodyChunk[] chunks = physicalObject.bodyChunks;
            if (physicalObject.bodyChunks.Length < numberOfChunks)
            {
                chunks = new BodyChunk[numberOfChunks];
            }
            for (int a = 0; a < numberOfChunks; a++)
            {
                chunks[a] = BodyChunkHandler.Read(chunks[a], ref reader);
            }
            (physicalObject as PhysicalObject as Patches.patch_PhysicalObject).Sync(chunks);
            int numberOFConnections = reader.ReadInt32();
            if (physicalObject.bodyChunkConnections.Length < numberOFConnections)
            {
                physicalObject.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[numberOFConnections];
            }
            for (int a = 0; a < numberOFConnections; a++)
            {
                physicalObject.bodyChunkConnections[a] = BodyChunkConnectionHandler.Read(physicalObject.bodyChunkConnections[a], ref reader);
            }
            physicalObject.bounce = reader.ReadSingle();
            physicalObject.canBeHitByWeapons = reader.ReadBoolean();
            return physicalObject;
        }
        public static Rock Read(Rock physicalObject, ref BinaryReader reader)
        {
            physicalObject.abstractPhysicalObject = AbstractPhysicalObjectHandler.Read(physicalObject.abstractPhysicalObject, ref reader);
            //int numberOfAppendages = reader.ReadInt32();
            //if (physicalObject.appendages.Count < numberOfAppendages)
            //{
            //    physicalObject.appendages = new List<PhysicalObject.Appendage>();
            //}
            //for (int a = 0; a < numberOfAppendages; a++)
            //{
            //    physicalObject.appendages[a] = AppendageHandler.Read(physicalObject.appendages[a], ref reader);
            //}
            int numberOfChunks = reader.ReadInt32();
            BodyChunk[] chunks = physicalObject.bodyChunks;
            if (physicalObject.bodyChunks.Length < numberOfChunks)
            {
                chunks = new BodyChunk[numberOfChunks];
            }
            for (int a = 0; a < numberOfChunks; a++)
            {
                chunks[a] = BodyChunkHandler.Read(chunks[a], ref reader);
            }
            (physicalObject as PhysicalObject as Patches.patch_PhysicalObject).Sync(chunks);
            int numberOFConnections = reader.ReadInt32();
            if (physicalObject.bodyChunkConnections.Length < numberOFConnections)
            {
                physicalObject.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[numberOFConnections];
            }
            for (int a = 0; a < numberOFConnections; a++)
            {
                physicalObject.bodyChunkConnections[a] = BodyChunkConnectionHandler.Read(physicalObject.bodyChunkConnections[a], ref reader);
            }
            physicalObject.bounce = reader.ReadSingle();
            physicalObject.canBeHitByWeapons = reader.ReadBoolean();
            return physicalObject;
        }
        public static Weapon Read(Weapon physicalObject, ref BinaryReader reader)
        {
            physicalObject.abstractPhysicalObject = AbstractPhysicalObjectHandler.Read(physicalObject.abstractPhysicalObject, ref reader);
            //int numberOfAppendages = reader.ReadInt32();
            //if (physicalObject.appendages.Count < numberOfAppendages)
            //{
            //    physicalObject.appendages = new List<PhysicalObject.Appendage>();
            //}
            //for (int a = 0; a < numberOfAppendages; a++)
            //{
            //    physicalObject.appendages[a] = AppendageHandler.Read(physicalObject.appendages[a], ref reader);
            //}
            int numberOfChunks = reader.ReadInt32();
            BodyChunk[] chunks = physicalObject.bodyChunks;
            if (physicalObject.bodyChunks.Length < numberOfChunks)
            {
                chunks = new BodyChunk[numberOfChunks];
            }
            for (int a = 0; a < numberOfChunks; a++)
            {
                chunks[a] = BodyChunkHandler.Read(chunks[a], ref reader);
            }
            (physicalObject as PhysicalObject as Patches.patch_PhysicalObject).Sync(chunks);
            int numberOFConnections = reader.ReadInt32();
            if (physicalObject.bodyChunkConnections.Length < numberOFConnections)
            {
                physicalObject.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[numberOFConnections];
            }
            for (int a = 0; a < numberOFConnections; a++)
            {
                physicalObject.bodyChunkConnections[a] = BodyChunkConnectionHandler.Read(physicalObject.bodyChunkConnections[a], ref reader);
            }
            physicalObject.bounce = reader.ReadSingle();
            physicalObject.canBeHitByWeapons = reader.ReadBoolean();
            return physicalObject;
        }
        public static Creature Read(Creature physicalObject, ref BinaryReader reader)
        {
            physicalObject.abstractPhysicalObject = AbstractPhysicalObjectHandler.Read(physicalObject.abstractPhysicalObject, ref reader);
            //int numberOfAppendages = reader.ReadInt32();
            //if (physicalObject.appendages.Count < numberOfAppendages)
            //{
            //    physicalObject.appendages = new List<PhysicalObject.Appendage>();
            //}
            //for (int a = 0; a < numberOfAppendages; a++)
            //{
            //    physicalObject.appendages[a] = AppendageHandler.Read(physicalObject.appendages[a], ref reader);
            //}
            int numberOfChunks = reader.ReadInt32();
            BodyChunk[] chunks = physicalObject.bodyChunks;
            if (physicalObject.bodyChunks.Length < numberOfChunks)
            {
                chunks = new BodyChunk[numberOfChunks];
            }
            for (int a = 0; a < numberOfChunks; a++)
            {
                chunks[a] = BodyChunkHandler.Read(chunks[a], ref reader);
            }
            (physicalObject as PhysicalObject as Patches.patch_PhysicalObject).Sync(chunks);
            int numberOFConnections = reader.ReadInt32();
            if (physicalObject.bodyChunkConnections.Length < numberOFConnections)
            {
                physicalObject.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[numberOFConnections];
            }
            for (int a = 0; a < numberOFConnections; a++)
            {
                physicalObject.bodyChunkConnections[a] = BodyChunkConnectionHandler.Read(physicalObject.bodyChunkConnections[a], ref reader);
            }
            physicalObject.bounce = reader.ReadSingle();
            physicalObject.canBeHitByWeapons = reader.ReadBoolean();
            return physicalObject;
        }
        public static Player Read(Player physicalObject, ref BinaryReader reader)
        {
            physicalObject.abstractPhysicalObject = AbstractPhysicalObjectHandler.Read(physicalObject.abstractPhysicalObject, ref reader);
            //int numberOfAppendages = reader.ReadInt32();
            //if (physicalObject.appendages.Count < numberOfAppendages)
            //{
            //    physicalObject.appendages = new List<PhysicalObject.Appendage>();
            //}
            //for (int a = 0; a < numberOfAppendages; a++)
            //{
            //    physicalObject.appendages[a] = AppendageHandler.Read(physicalObject.appendages[a], ref reader);
            //}
            int numberOfChunks = reader.ReadInt32();
            BodyChunk[] chunks = physicalObject.bodyChunks;
            if (physicalObject.bodyChunks.Length < numberOfChunks)
            {
                chunks = new BodyChunk[numberOfChunks];
            }
            for (int a = 0; a < numberOfChunks; a++)
            {
                chunks[a] = BodyChunkHandler.Read(chunks[a], ref reader);
            }
            (physicalObject as PhysicalObject as Patches.patch_PhysicalObject).Sync(chunks);
            int numberOFConnections = reader.ReadInt32();
            if (physicalObject.bodyChunkConnections.Length < numberOFConnections)
            {
                physicalObject.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[numberOFConnections];
            }
            for (int a = 0; a < numberOFConnections; a++)
            {
                physicalObject.bodyChunkConnections[a] = BodyChunkConnectionHandler.Read(physicalObject.bodyChunkConnections[a], ref reader);
            }
            physicalObject.bounce = reader.ReadSingle();
            physicalObject.canBeHitByWeapons = reader.ReadBoolean();
            return physicalObject;
        }

        public static void Write(PhysicalObject physicalObject, ref BinaryWriter writer)
        {
            AbstractPhysicalObjectHandler.Write(physicalObject.abstractPhysicalObject, ref writer);
            //writer.Write(physicalObject.appendages.Count);
            //foreach (PhysicalObject.Appendage app in physicalObject.appendages)
            //{
            //    AppendageHandler.Write(app, ref writer);
            //}
            writer.Write(physicalObject.bodyChunks.Length);
            foreach (BodyChunk chunk in physicalObject.bodyChunks)
            {
                BodyChunkHandler.Write(chunk, ref writer);
            }
            writer.Write(physicalObject.bodyChunkConnections.Length);
            foreach (PhysicalObject.BodyChunkConnection con in physicalObject.bodyChunkConnections)
            {
                BodyChunkConnectionHandler.Write(con, ref writer);
            }
            writer.Write(physicalObject.bounce);
            writer.Write(physicalObject.canBeHitByWeapons);
        }

        static class AppendageHandler
        {
            public static PhysicalObject.Appendage Read(PhysicalObject.Appendage appendage, ref BinaryReader reader)
            {
                appendage.appIndex = reader.ReadInt32();
                appendage.canBeHit = reader.ReadBoolean();
                int numberOfSegments = reader.ReadInt32();
                appendage.segments = new Vector2[numberOfSegments]; 
                for (int a = 0; a < numberOfSegments; a++)
                {
                    appendage.segments[a] = Vector2Handler.Read(ref reader);
                }
                appendage.totalLength = reader.ReadSingle();
                return appendage;
            }

            public static void Write(PhysicalObject.Appendage appendage, ref BinaryWriter writer)
            {
                writer.Write(appendage.appIndex);
                writer.Write(appendage.canBeHit);
                writer.Write(appendage.segments.Length);
                for (int a = 0; a < appendage.segments.Length; a++)
                {
                    Vector2Handler.Write(appendage.segments[a], ref writer);
                }
                writer.Write(appendage.totalLength);
            }
        }

        static class BodyChunkConnectionHandler
        {
            public static PhysicalObject.BodyChunkConnection Read(PhysicalObject.BodyChunkConnection bodyChunkConnection, ref BinaryReader reader)
            {
                bodyChunkConnection.active = reader.ReadBoolean();
                bodyChunkConnection.type = (PhysicalObject.BodyChunkConnection.Type)reader.ReadByte();
                bodyChunkConnection.distance = reader.ReadSingle();
                bodyChunkConnection.weightSymmetry = reader.ReadSingle();
                bodyChunkConnection.elasticity = reader.ReadSingle();
                return bodyChunkConnection;
            }

            public static void Write(PhysicalObject.BodyChunkConnection bodyChunkConnection, ref BinaryWriter writer)
            {
                writer.Write(bodyChunkConnection.active);
                writer.Write((byte)bodyChunkConnection.type);
                writer.Write(bodyChunkConnection.distance);
                writer.Write(bodyChunkConnection.weightSymmetry);
                writer.Write(bodyChunkConnection.elasticity);
            }
        }
    }
}
