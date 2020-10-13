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
            switch (abstractPhysicalObject.type)
            {
                case AbstractPhysicalObject.AbstractObjectType.Spear:
                    AbstractSpearHandler.Write(abstractPhysicalObject as AbstractSpear, ref writer);
                    break;
                case AbstractPhysicalObject.AbstractObjectType.DataPearl:
                    break;
                case AbstractPhysicalObject.AbstractObjectType.VultureMask:
                    break;
                    /*...*/
           
            }
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
            switch(abstractPhysicalObject.type)
            {
                case AbstractPhysicalObject.AbstractObjectType.Spear:
                    AbstractSpearHandler.Read(abstractPhysicalObject as AbstractSpear, ref reader);
                    break;
                case AbstractPhysicalObject.AbstractObjectType.DataPearl:
                    break;
                case AbstractPhysicalObject.AbstractObjectType.VultureMask:
                    break;
                    /*...*/
            }
        }

        public static AbstractPhysicalObject InitializeAbstractObject(World world, AbstractPhysicalObject.AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
        {
            AbstractPhysicalObject abstractPhysical = new AbstractPhysicalObject(world, type, realizedObject, pos, ID);
            if (type == AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                abstractPhysical = new AbstractSpear(world, realizedObject as Spear, pos, ID, false);
            }
            return abstractPhysical;
        }
    }
}
