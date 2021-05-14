using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RWCustom;
using MonoMod;

namespace Monkland.Patches
{
    [MonoModPatch("global::BodyChunk")]
    class patch_BodyChunk : BodyChunk
    {
        [MonoModIgnore]
        public patch_BodyChunk(PhysicalObject owner, int index, Vector2 pos, float rad, float mass) : base(owner, index, pos, rad, mass)
        {
        }

        [MonoModIgnore]
        private IntVector2 contactPoint;

        public void Sync(IntVector2 contactPoint)
        {
            this.contactPoint = contactPoint;
        }
    }
}