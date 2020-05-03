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
		}
	}
}
