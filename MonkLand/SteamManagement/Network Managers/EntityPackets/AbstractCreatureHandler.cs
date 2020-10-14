using Monkland.Hooks.Entities;
using System.IO;

namespace Monkland.SteamManagement
{
    internal static class AbstractCreatureHandler
    {
        public static void Write(AbstractCreature creature, ref BinaryWriter writer)
        {
            AbstractPhysicalObjectHandler.Write(creature, ref writer);
            writer.Write((byte)creature.creatureTemplate.type);

            //Additional personality and relationship traits should be synced here!
            writer.Write(creature.remainInDenCounter);
            WorldCoordinateHandler.Write(creature.spawnDen, ref writer);
            //creature state should also be synced here!!
        }

        public static void Read(AbstractCreature creature, ref BinaryReader reader)
        {
            AbstractPhysicalObjectHandler.Read(creature, ref reader);
            creature.creatureTemplate.type = (CreatureTemplate.Type)reader.ReadByte();

            //Additional personality and relationship traits should be synced here!
            creature.remainInDenCounter = reader.ReadInt32();
            creature.spawnDen = WorldCoordinateHandler.Read(ref reader);
            //creature state should also be synced here!!
        }


        /*
        public static void Read(AbstractCreature creature, ref BinaryReader reader)
        {
            creature.ID = EntityIDHandler.Read(ref reader);
            creature.pos = WorldCoordinateHandler.Read(ref reader);
            creature.InDen = reader.ReadBoolean();
            creature.timeSpentHere = reader.ReadInt32();
            //creature.type = (AbstractPhysicalObject.AbstractObjectType)reader.ReadByte();
            creature.ID.number = reader.ReadInt32();
            creature.destroyOnAbstraction = true;
            //creature.abstractAI = AbstractCreatureAIHandler.Read(creature.abstractAI, ref reader);
            //Additional personality and relationship traits should be synced here!
            creature.remainInDenCounter = reader.ReadInt32();
            creature.spawnDen = WorldCoordinateHandler.Read(ref reader);
            //creature state should also be synced here!!
            //return creature;
        }

        public static void Read(Creature creature, ref BinaryReader reader)
        {
            if (creature.abstractCreature == null) { return; }
            creature.abstractCreature.ID = EntityIDHandler.Read(ref reader);
            creature.abstractCreature.pos = WorldCoordinateHandler.Read(ref reader);
            creature.abstractCreature.InDen = reader.ReadBoolean();
            creature.abstractCreature.timeSpentHere = reader.ReadInt32();
            //creature.type = (AbstractPhysicalObject.AbstractObjectType)reader.ReadByte();
            creature.abstractCreature.ID.number = reader.ReadInt32();
            creature.abstractCreature.destroyOnAbstraction = true;
            //creature.abstractAI = AbstractCreatureAIHandler.Read(creature.abstractAI, ref reader);
            //Additional personality and relationship traits should be synced here!
            creature.abstractCreature.remainInDenCounter = reader.ReadInt32();
            creature.abstractCreature.spawnDen = WorldCoordinateHandler.Read(ref reader);
            //creature state should also be synced here!!
            //return creature;
        }

        public static void Write(AbstractCreature creature, ref BinaryWriter writer)
        {
            EntityIDHandler.Write(creature.ID, ref writer);
            WorldCoordinateHandler.Write(creature.pos, ref writer);
            writer.Write(creature.InDen);
            writer.Write(creature.timeSpentHere);
            //writer.Write((byte)creature.type);
            writer.Write(AbstractPhysicalObjectHK.GetField(creature).dist);
            //AbstractCreatureAIHandler.Write(creature.abstractAI, ref writer);
            //Additional personality and relationship traits should be synced here!
            writer.Write(creature.remainInDenCounter);
            WorldCoordinateHandler.Write(creature.spawnDen, ref writer);
            //creature state should also be synced here!!
        }
        */

        private static class AbstractCreatureAIHandler
        {
            public static AbstractCreatureAI Read(AbstractCreatureAI abstractCreatureAI, ref BinaryReader reader)
            {
                int numberOfNodes = reader.ReadInt32();
                abstractCreatureAI.path.Clear();
                for (int a = 0; a < numberOfNodes; a++)
                { abstractCreatureAI.path.Add(WorldCoordinateHandler.Read(ref reader)); }
                return abstractCreatureAI;
            }

            public static void Write(AbstractCreatureAI abstractCreatureAI, ref BinaryWriter writer)
            {
                writer.Write(abstractCreatureAI.path.Count);
                for (int a = 0; a < abstractCreatureAI.path.Count; a++)
                { WorldCoordinateHandler.Write(abstractCreatureAI.path[a], ref writer); }
            }
        }
    }
}
