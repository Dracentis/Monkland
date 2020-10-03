using HUD;
using Menu;
using UnityEngine;

namespace Monkland.Hooks.Menus
{
    internal static class HUDHK
    {
        public static void SubPatch()
        {
            On.HUD.HUD.InitSleepHud += new On.HUD.HUD.hook_InitSleepHud(InitSleepHudHK);
        }

        private static void InitSleepHudHK(On.HUD.HUD.orig_InitSleepHud orig, HUD.HUD self, SleepAndDeathScreen sleepAndDeathScreen, Map.MapData mapData, SlugcatStats charStats)
        {
            if (!(sleepAndDeathScreen is MultiplayerSleepAndDeathScreen)) { orig.Invoke(self, sleepAndDeathScreen, mapData, charStats); return; }
            self.AddPart(new FoodMeter(self, charStats.maxFood, charStats.foodToHibernate));
            //if (mapData != null)
            //{
            //this.AddPart(new Map(this, mapData));
            //}
            self.foodMeter.pos = new Vector2(sleepAndDeathScreen.FoodMeterXPos((sleepAndDeathScreen.ID != ProcessManager.ProcessID.SleepScreen) ? 1f : 0f), 0f);
            self.foodMeter.lastPos = self.foodMeter.pos;

            self.AddPart(new RainMeter(self, self.fContainers[1]));
            self.rainMeter.pos = new Vector2(self.rainWorld.options.ScreenSize.x - 335f, self.rainWorld.options.ScreenSize.y - 70f);
            self.rainMeter.lastPos = self.rainMeter.pos;
            self.rainMeter.fade = 1f;
        }
    }
}