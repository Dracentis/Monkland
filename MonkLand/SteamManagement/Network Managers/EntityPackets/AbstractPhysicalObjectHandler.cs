using Monkland.Hooks.Entities;
using System.IO;

namespace Monkland.SteamManagement
{
    internal class AbstractPhysicalObjectHandler
    {
        /* **************
        * AbstractPhysicalObject packet ()
        * 
        * (byte) type          (1 byte)
        * (int)  distinguisher (1~5 byte)

        * **************/

        /// <summary>
        /// Writes physicalObject packet 
        /// <para>[ABSTRACTENTITYOBJ | byte type]</para>
        /// <para>[30 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(AbstractPhysicalObject abstractPhysicalObject, ref BinaryWriter writer)
        {
            AbstractWorldEntityHandler.Write(abstractPhysicalObject, ref writer);
            writer.Write((byte)abstractPhysicalObject.type);
        }

        /// <summary>
        /// Writes physicalObject packet 
        /// <para>[ABSTRACTENTITYOBJ | byte type]</para>
        /// <para>[30 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Read(AbstractPhysicalObject abstractPhysicalObject, ref BinaryReader reader)
        {
            AbstractWorldEntityHandler.Read(abstractPhysicalObject, ref reader);
            abstractPhysicalObject.type = (AbstractPhysicalObject.AbstractObjectType)reader.ReadByte();
            abstractPhysicalObject.destroyOnAbstraction = true;
        }
    }
}
