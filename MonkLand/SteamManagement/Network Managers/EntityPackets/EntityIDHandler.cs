using System.IO;

namespace Monkland.SteamManagement
{
    internal static class EntityIDHandler
    {
        /// <summary>
        /// Writes EntityID packet 
        /// <para>[int number | int spawn]</para>
        /// <para>[8 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(EntityID ID, ref BinaryWriter writer)
        {
            writer.Write(ID.number);
            writer.Write(ID.spawner);
        }

        /// <summary>
        /// Reads EntityID packet 
        /// <para>[int number | int spawn]</para>
        /// <para>[8 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static EntityID Read(ref BinaryReader reader)
        {
            EntityID ID = new EntityID(0, 0);
            return Read(ID, ref reader);
        }

        public static EntityID Read(EntityID ID, ref BinaryReader reader)
        {
            ID.number = reader.ReadInt32();
            ID.spawner = reader.ReadInt32();
            return ID;
        }



    }
}