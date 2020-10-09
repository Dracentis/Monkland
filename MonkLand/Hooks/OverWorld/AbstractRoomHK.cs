using Monkland.SteamManagement;
using System.Collections.Generic;

namespace Monkland.Hooks.OverWorld
{
    internal static class AbstractRoomHK
    {
        public static void ApplyHook()
        {
            fields = new Dictionary<AbstractRoom, AbsRoomFields>();

            On.AbstractRoom.ctor += new On.AbstractRoom.hook_ctor(CtorHK);
            On.AbstractRoom.RealizeRoom += new On.AbstractRoom.hook_RealizeRoom(RealizeRoomHK);
            On.AbstractRoom.Abstractize += new On.AbstractRoom.hook_Abstractize(AbstractizeHK);
        }

        private static Dictionary<AbstractRoom, AbsRoomFields> fields;

        public static void ClearField() => fields.Clear();

        public static AbsRoomFields GetField(AbstractRoom self)
        {
            if (fields.TryGetValue(self, out AbsRoomFields field))
            {
                return field;
            }
            field = new AbsRoomFields(self);
            fields.Add(self, field);
            return field;
        }

        private static void CtorHK(On.AbstractRoom.orig_ctor orig, AbstractRoom self,
            string name, int[] connections, int index, int swarmRoomIndex, int shelterIndex, int gateIndex)
        {
            orig(self, name, connections, index, swarmRoomIndex, shelterIndex, gateIndex);
            GetField(self);
        }

        private static void RealizeRoomHK(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
        {
            orig(self, world, game);

            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.WorldManager.ActivateRoom(self.index);
            }
        }

        private static void AbstractizeHK(On.AbstractRoom.orig_Abstractize orig, AbstractRoom self)
        {
            orig(self);

            if (MonklandSteamManager.isInGame)
            {
                MonklandSteamManager.WorldManager.DeactivateRoom(self.index);
            }
        }
    }
}
