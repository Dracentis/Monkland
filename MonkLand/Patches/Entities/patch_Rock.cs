using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using Monkland.SteamManagement;
using MonoMod;

namespace Monkland.Patches
{
    [MonoModPatch("global::Rock")]
    class patch_Rock : Rock
    {
        [MonoModIgnore]
        public patch_Rock(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
        }

        [MonoModIgnore]
        public extern void OriginalConstructor(AbstractPhysicalObject abstractPhysicalObject, World world);
        [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
        public void ctor_Rock(AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            OriginalConstructor(abstractPhysicalObject, world);
            this.networkLife = 120;
        }

        public int networkLife = 120;

        public void Sync()
        {
            networkLife = 120;
        }

        public extern void orig_Update(bool eu);

        public void Update(bool eu)
        {
            orig_Update(eu);
            if ((this.abstractPhysicalObject as patch_AbstractPhysicalObject).networkObject)
            {
                if (this.networkLife > 0)
                {
                    this.networkLife--;
                }
                else
                {
                    networkLife = 120;
                    foreach (Creature.Grasp grasp in this.grabbedBy){
                        grasp.Release();
                    }
                    this.Destroy();
                }
            }
        }
    }
}
