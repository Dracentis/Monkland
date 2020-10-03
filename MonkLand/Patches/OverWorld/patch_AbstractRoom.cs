using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using Monkland.SteamManagement;

namespace Monkland.Patches
{
    [MonoMod.MonoModPatch("global::AbstractRoom")]
    class patch_AbstractRoom : AbstractRoom
    {
        [MonoModIgnore]
        public patch_AbstractRoom(string name, int[] connections, int index, int swarmRoomIndex, int shelterIndex, int gateIndex) : base(name, connections, index, swarmRoomIndex, shelterIndex, gateIndex)
        {
        }

        public extern void orig_RealizeRoom(World world, RainWorldGame game);

        public void RealizeRoom(World world, RainWorldGame game)
        {
            orig_RealizeRoom(world, game);
            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.WorldManager.ActivateRoom(this.name);
            }
        }

        public extern void orig_Abstractize();

        public void Abstractize()
        {
            orig_Abstractize();
            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.WorldManager.DeactivateRoom(this.name);
            }
        }
    }
}