using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using RWCustom;

namespace Monkland.SteamManagement
{
    static class PlayerCarryableItemHandler
    {
        public static PlayerCarryableItem Read(PlayerCarryableItem item, ref BinaryReader reader)
        {
            item.abstractPhysicalObject = AbstractPhysicalObjectHandler.Read(item.abstractPhysicalObject, ref reader);
            item = PhysicalObjectHandler.Read(item, ref reader);
            return item;
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

        public static void Write(PlayerCarryableItem item, ref BinaryWriter writer)
        {
        }


    }
}
