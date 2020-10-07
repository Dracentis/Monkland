using Monkland.Hooks.Entities;
using System.IO;

namespace Monkland.SteamManagement
{
    internal class AbstractPhysicalObjectHandler
    {
        /* public static AbstractPhysicalObject Read(AbstractPhysicalObject physicalObject, ref BinaryReader reader)
        {
            physicalObject.ID = EntityIDHandler.Read(ref reader);
            physicalObject.pos = WorldCoordinateHandler.Read(ref reader);
            physicalObject.InDen = reader.ReadBoolean();
            physicalObject.timeSpentHere = reader.ReadInt32();
            physicalObject.type = (AbstractPhysicalObject.AbstractObjectType)reader.ReadByte();
            physicalObject.ID.number = reader.ReadInt32();
            physicalObject.destroyOnAbstraction = true;
            return physicalObject;
        } */

        public static void Read(AbstractPhysicalObject physicalObject, ref BinaryReader reader)
        {
            physicalObject.ID = EntityIDHandler.Read(ref reader);
            physicalObject.pos = WorldCoordinateHandler.Read(ref reader);
            physicalObject.InDen = reader.ReadBoolean();
            physicalObject.timeSpentHere = reader.ReadInt32();
            physicalObject.type = (AbstractPhysicalObject.AbstractObjectType)reader.ReadByte();
            physicalObject.ID.number = reader.ReadInt32();
            physicalObject.destroyOnAbstraction = true;
        }

        public static void Write(AbstractPhysicalObject physicalObject, ref BinaryWriter writer)
        {
            EntityIDHandler.Write(physicalObject.ID, ref writer);
            WorldCoordinateHandler.Write(physicalObject.pos, ref writer);
            writer.Write(physicalObject.InDen);
            writer.Write(physicalObject.timeSpentHere);
            writer.Write((byte)physicalObject.type);
            writer.Write(AbstractPhysicalObjectHK.GetField(physicalObject).dist);
        }
    }
}
