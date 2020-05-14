using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using RWCustom;

namespace Monkland.Patches
{
    [MonoModPatch("global::Creature")]
    class patch_Creature : Creature
    {
        [MonoModIgnore]
        public patch_Creature(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
        }

        public void Sync(bool dead)
        {
            this.dead = dead;
        }

    }
}
