using System;
using System.Collections.Generic;
using Menu;
using Monkland;
using RWCustom;
using HUD;
using UnityEngine;
using MonoMod;

namespace Monkland.Patches.Menus
{
    [MonoModPatch("global::HUD.HUD")]
    class patch_HUD : HUD.HUD
    {
        [MonoModIgnore]
        public patch_HUD(FContainer[] fContainers, RainWorld rainWorld, IOwnAHUD owner) : base(fContainers, rainWorld, owner)
        {
        }

        [MonoModIgnore]
        public abstract class HudPart
        {
        }

        [MonoModIgnore]
        public void AddPart(HudPart part)
        {
        }

        public void InitSleepHud(MultiplayerSleepAndDeathScreen sleepAndDeathScreen, Map.MapData mapData, SlugcatStats charStats)
        {
            this.AddPart(new FoodMeter(this, charStats.maxFood, charStats.foodToHibernate));
            //if (mapData != null)
            //{
            //this.AddPart(new Map(this, mapData));
            //}
            this.foodMeter.pos = new Vector2(sleepAndDeathScreen.FoodMeterXPos((sleepAndDeathScreen.ID != ProcessManager.ProcessID.SleepScreen) ? 1f : 0f), 0f);
            this.foodMeter.lastPos = this.foodMeter.pos;

            this.AddPart(new RainMeter(this, this.fContainers[1]));
            this.rainMeter.pos = new Vector2(this.rainWorld.options.ScreenSize.x - 335f, this.rainWorld.options.ScreenSize.y - 70f);
            this.rainMeter.lastPos = this.rainMeter.pos;
            this.rainMeter.fade = 1f;
        }
    }
}