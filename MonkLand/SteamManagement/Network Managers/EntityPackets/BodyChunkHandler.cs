using System.IO;
using RWCustom;
using UnityEngine;

namespace Monkland.SteamManagement
{
    static class BodyChunkHandler
    {
        public static BodyChunk Read(BodyChunk bodyChunk ,ref BinaryReader reader)
        {
            (bodyChunk as Patches.patch_BodyChunk).Sync(IntVector2Handler.Read(ref reader));
            bodyChunk.lastContactPoint = IntVector2Handler.Read(ref reader);

            bodyChunk.lastLastPos = Vector2Handler.Read(ref reader);
            bodyChunk.lastPos = Vector2Handler.Read(ref reader);
            bodyChunk.pos = Vector2Handler.Read(ref reader);
            return bodyChunk;
        }
        public static BodyChunk Read(ref BinaryReader reader)
        {
            BodyChunk bodyChunk = new BodyChunk(null, 0, new Vector2(0f,0f), 1f, 1f);

            (bodyChunk as Patches.patch_BodyChunk).Sync(IntVector2Handler.Read(ref reader));
            bodyChunk.lastContactPoint = IntVector2Handler.Read(ref reader);

            bodyChunk.lastLastPos = Vector2Handler.Read(ref reader);
            bodyChunk.lastPos = Vector2Handler.Read(ref reader);
            bodyChunk.pos = Vector2Handler.Read(ref reader);
            return bodyChunk;
        }

        public static void Write(BodyChunk bodyChunk, ref BinaryWriter writer)
        {
            IntVector2Handler.Write(bodyChunk.ContactPoint, ref writer);
            IntVector2Handler.Write(bodyChunk.lastContactPoint, ref writer);
            Vector2Handler.Write(bodyChunk.lastLastPos, ref writer);
            Vector2Handler.Write(bodyChunk.lastPos, ref writer);
            Vector2Handler.Write(bodyChunk.pos, ref writer);
        }

    }
}
