using System.IO;
using UnityEngine;

namespace Monkland.SteamManagement
{
    internal static class BodyChunkHandler
    {

        /* BodyChunksConnection Packet (18 bytes) */

        /* **************
         * (Vector2)  pos         (1 byte)
         * (Vector2)  vel         (1 byte)
         * **************/


        /// <summary>
        /// Writes bodyChunk packet 
        /// <para>[vec2 pos | vec2 vel]</para>
        /// <para>[16 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(BodyChunk bodyChunk, ref BinaryWriter writer)
        {
            Vector2Handler.Write(bodyChunk.pos, ref writer);
            Vector2Handler.Write(bodyChunk.vel, ref writer);
            //writer.Write(bodyChunk.mass);
            //writer.Write(bodyChunk.rad);
        }

        /// <summary>
        /// Reads bodyChunk packet 
        /// <para>[vec2 pos | vec2 vel]</para>
        /// <para>[16 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Read(BodyChunk bodyChunk, ref BinaryReader reader)
        {
            if (bodyChunk == null)
            {
                BodyChunk dummy = new BodyChunk(null, 0, new Vector2(0,0), 0, 0);
                dummy.pos = Vector2Handler.Read(ref reader);
                dummy.vel = Vector2Handler.Read(ref reader);
            }
            else
            {
                bodyChunk.pos = Vector2Handler.Read(ref reader);
                bodyChunk.vel = Vector2Handler.Read(ref reader);
                //bodyChunk.mass = reader.ReadSingle();
                //bodyChunk.rad = reader.ReadSingle();
                //return bodyChunk;
            }
        }


    }

    internal static class BodyChunkConnectionHandler
    {

        /* BodyChunksConnection Packet (18 bytes) */

        /* **************
         * (bool)  active         (1 byte)
         * (byte)  Type           (1 byte)
         * (float) distance       (4 bytes)
         * (float) weightSymmetry (4 bytes)
         * (float) elasticity     (4 bytes)
         * **************/

        /// <summary>
        /// Writes bodyChunk connection packet 
        /// <para>[bool active | byte type | float distance | float weightSymmetry | float elasticity]</para>
        /// <para>[18 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static void Write(PhysicalObject.BodyChunkConnection bodyChunkConnection, ref BinaryWriter writer)
        {
            writer.Write(bodyChunkConnection.active);
            writer.Write((byte)bodyChunkConnection.type);
            writer.Write(bodyChunkConnection.distance);
            writer.Write(bodyChunkConnection.weightSymmetry);
            writer.Write(bodyChunkConnection.elasticity);
        }

        /// <summary>
        /// Reads bodyChunk connection packet 
        /// <para>[bool active | byte type | float distance | float weightSymmetry | float elasticity]</para>
        /// <para>[18 bytes]</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>void</returns>
        public static PhysicalObject.BodyChunkConnection Read(PhysicalObject.BodyChunkConnection bodyChunkConnection, ref BinaryReader reader)
        {
            bodyChunkConnection.active = reader.ReadBoolean();
            bodyChunkConnection.type = (PhysicalObject.BodyChunkConnection.Type)reader.ReadByte();
            bodyChunkConnection.distance = reader.ReadSingle();
            bodyChunkConnection.weightSymmetry = reader.ReadSingle();
            bodyChunkConnection.elasticity = reader.ReadSingle();
            return bodyChunkConnection;
        }

    }
}