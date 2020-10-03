using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;

namespace Monkland.Patches
{
    [MonoModPatch("global::PhysicalObject")]
    class patch_PhysicalObject : PhysicalObject
    {
        [MonoModIgnore]
        public patch_PhysicalObject(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {
        }

        [MonoModIgnore]
        public extern void OriginalConstructor(AbstractPhysicalObject abstractPhysicalObject);
        [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
        public void ctor_PhysicalObject(AbstractPhysicalObject abstractPhysicalObject)
        {
            OriginalConstructor(abstractPhysicalObject);
        }

        public void Sync(BodyChunk[] bodyChunks)
        {
            this.bodyChunks = bodyChunks;
        }
    }
}