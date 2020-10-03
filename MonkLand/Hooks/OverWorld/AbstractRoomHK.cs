using Monkland.SteamManagement;
using System.Collections.Generic;

namespace Monkland.Hooks.OverWorld
{
    internal static class AbstractRoomHK
    {
        public static void SubPatch()
        {
            Subs = new Dictionary<AbstractRoom, ARMonkSub>();

            On.AbstractRoom.ctor += new On.AbstractRoom.hook_ctor(CtorHK);
            On.AbstractRoom.RealizeRoom += new On.AbstractRoom.hook_RealizeRoom(RealizeRoomHK);
            On.AbstractRoom.Abstractize += new On.AbstractRoom.hook_Abstractize(AbstractizeHK);
        }

        private static Dictionary<AbstractRoom, ARMonkSub> Subs;

        public static void ClearSub() => Subs.Clear();

        public static ARMonkSub GetSub(AbstractRoom self)
        {
            if (Subs.TryGetValue(self, out ARMonkSub sub)) { return sub; }
            sub = new ARMonkSub(self);
            Subs.Add(self, sub);
            return sub;
        }

        private static void CtorHK(On.AbstractRoom.orig_ctor orig, AbstractRoom self,
            string name, int[] connections, int index, int swarmRoomIndex, int shelterIndex, int gateIndex)
        {
            orig.Invoke(self, name, connections, index, swarmRoomIndex, shelterIndex, gateIndex);
            GetSub(self);
        }

        private static void RealizeRoomHK(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
        {
            orig.Invoke(self, world, game);
            if (MonklandSteamManager.isInGame) { MonklandSteamManager.WorldManager.ActivateRoom(self.name); }
        }

        private static void AbstractizeHK(On.AbstractRoom.orig_Abstractize orig, AbstractRoom self)
        {
            orig.Invoke(self);
            if (MonklandSteamManager.isInGame) { MonklandSteamManager.WorldManager.DeactivateRoom(self.name); }
        }
    }
}