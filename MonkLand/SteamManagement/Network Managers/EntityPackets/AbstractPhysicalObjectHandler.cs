using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RWCustom;
using UnityEngine;

namespace Monkland.SteamManagement
{
    class AbstractPhysicalObjectHandler
    {
        public static AbstractPhysicalObject Read(AbstractPhysicalObject physicalObject, ref BinaryReader reader)
        {
            physicalObject.ID = EntityIDHandler.Read(ref reader);
            physicalObject.pos = WorldCoordinateHandler.Read(ref reader);
            physicalObject.InDen = reader.ReadBoolean();
            physicalObject.timeSpentHere = reader.ReadInt32();
            physicalObject.type = (AbstractPhysicalObject.AbstractObjectType) reader.ReadByte();
            (physicalObject as Patches.patch_AbstractPhysicalObject).ID.number = reader.ReadInt32();
            physicalObject.destroyOnAbstraction = true;
            return physicalObject;
        }

        public static void Write(AbstractPhysicalObject physicalObject, ref BinaryWriter writer)
        {
            EntityIDHandler.Write(physicalObject.ID, ref writer);
            WorldCoordinateHandler.Write(physicalObject.pos, ref writer);
            writer.Write(physicalObject.InDen);
            writer.Write(physicalObject.timeSpentHere);
            writer.Write((byte)physicalObject.type);
            writer.Write((physicalObject as Patches.patch_AbstractPhysicalObject).dist);
        }
    }
}
