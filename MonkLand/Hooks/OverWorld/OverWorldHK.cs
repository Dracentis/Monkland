using Monkland.SteamManagement;

namespace Monkland.Hooks.OverWorld
{
    public static class OverWorldHK
    {
        public static void Patch()
        {
            AbstractRoomHK.SubPatch();
            ShortcutHandlerHK.SubPatch();

            On.OverWorld.LoadFirstWorld += new On.OverWorld.hook_LoadFirstWorld(LoadFirstWorldHK);
        }

        private static void LoadFirstWorldHK(On.OverWorld.orig_LoadFirstWorld orig, global::OverWorld self)
        {
            orig.Invoke(self);
            if (MonklandSteamManager.isInGame)
            { MonklandSteamManager.WorldManager.GameStart(); }
        }
    }
}