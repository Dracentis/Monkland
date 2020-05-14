using System;
using System.Collections.Generic;
using System.Text;
using HUD;
using MonoMod;
using Menu;
using RWCustom;
using UnityEngine;

namespace Monkland.Patches.Menus
{
	[MonoModPatch("global::HUD.FoodMeter")]
	class patch_FoodMeter : HUD.FoodMeter
    {
        [MonoModIgnore]
        public patch_FoodMeter(HUD.HUD hud, int maxFood, int survivalLimit) : base(hud, maxFood, survivalLimit)
        {
        }

		[MonoModIgnore]
		private float showSurvLim;

		[MonoModIgnore]
		private float survLimTo;

		[MonoModIgnore]
		public float forceSleep;

		[MonoModIgnore]
		private bool showKarmaChange;

		private extern void orig_SleepUpdate(); 

		private void SleepUpdate()
		{
			if (this.hud.owner is SleepAndDeathScreen)
			{
				orig_SleepUpdate();
			}
			else
			{
				int num = this.eatCircleDelay;
				if ((this.hud.owner as MultiplayerSleepAndDeathScreen).AllowFoodMeterTick && this.showSurvLim == this.survLimTo)
				{
					this.eatCircleDelay--;
				}
				switch (this.sleepScreenPhase)
				{
					case 0:
						if (this.eatCircles > 0 && this.showCount - 1 >= 0)
						{
							if (this.eatCircleDelay < 1)
							{
								this.circles[this.showCount - 1].EatFade();
								this.eatCircles--;
								this.showCount--;
								this.eatCircleDelay = ((this.maxFood <= 7) ? 40 : 20);
							}
						}
						else
						{
							if ((this.hud.owner as MultiplayerSleepAndDeathScreen).startMalnourished || (this.hud.owner as MultiplayerSleepAndDeathScreen).goalMalnourished)
							{
								this.sleepScreenPhase = 1;
							}
							else
							{
								this.sleepScreenPhase = 3;
							}
							this.eatCircleDelay = 80;
						}
						this.fade = Custom.LerpAndTick(this.fade, 0.5f, 0.04f, 0.0333333351f);
						break;
					case 1:
						if (this.eatCircleDelay <= 0 && num > 0)
						{
							if ((this.hud.owner as MultiplayerSleepAndDeathScreen).goalMalnourished)
							{
								this.MoveSurvivalLimit((float)this.maxFood, true);
							}
							else
							{
								this.MoveSurvivalLimit((float)this.survivalLimit, true);
							}
							this.sleepScreenPhase = 2;
						}
						this.fade = Custom.LerpAndTick(this.fade, 0.5f, 0.04f, 0.0333333351f);
						break;
					case 2:
						if (this.showSurvLim == this.survLimTo)
						{
							this.sleepScreenPhase = 3;
							this.eatCircleDelay = 80;
						}
						this.fade = Custom.LerpAndTick(this.fade, 0.5f, 0.04f, 0.0333333351f);
						break;
					case 3:
						if (this.eatCircleDelay <= 0 && num > 0)
						{
							this.hud.owner.FoodCountDownDone();
						}
						this.fade = Custom.LerpAndTick(this.fade, Custom.LerpMap((float)this.eatCircleDelay, 80f, 20f, 0.5f, 0.1f * (1f - (this.hud.owner as MultiplayerSleepAndDeathScreen).StarveLabelAlpha(1f))), 0.04f, 0.0333333351f);
						break;
				}
				float num2 = Custom.SCurve(Mathf.InverseLerp(-30f, -60f, (float)this.eatCircleDelay), 0.5f);
				this.pos.y = Mathf.Lerp(450f, 33f, num2);
				this.pos.x = (this.hud.owner as MultiplayerSleepAndDeathScreen).FoodMeterXPos(num2);
			}
		}

		private extern void orig_DeathUpdate();

		private void DeathUpdate()
		{
			if (this.hud.owner is SleepAndDeathScreen)
			{
				orig_DeathUpdate();
			}
			else
			{
				if (!(this.hud.owner as MultiplayerSleepAndDeathScreen).AllowFoodMeterTick)
				{
					return;
				}
				this.pos.y = 33f;
				this.pos.x = (this.hud.owner as MultiplayerSleepAndDeathScreen).FoodMeterXPos(1f);
				this.fade = Custom.LerpAndTick(this.fade, 0.1f, 0.04f, 0.0333333351f);
				this.eatCircleDelay--;
				if (this.eatCircleDelay < 0 && !this.showKarmaChange)
				{
					this.showKarmaChange = true;
					this.hud.owner.FoodCountDownDone();
				}
			}
		}

		public class MeterCircle : FoodMeter.MeterCircle
		{
			[MonoModIgnore]
			public MeterCircle(FoodMeter meter, int number) : base(meter, number)
			{
				this.meter = meter;
				this.number = number;
				this.slowXAdd = this.XAdd(1f);
				this.lastSlowXAdd = this.slowXAdd;
			}

			public void Update()
			{
				this.lastSlowXAdd = this.slowXAdd;
				this.slowXAdd = Custom.LerpAndTick(this.slowXAdd, this.XAdd(1f), 0.06f, 2f);
				for (int i = 0; i < this.rads.GetLength(0); i++)
				{
					if (!this.plopped)
					{
						this.rads[i, 0] = this.circles[i].snapRad / 2f;
						this.rads[i, 1] = 0f;
					}
					else
					{
						this.rads[i, 0] += this.rads[i, 1];
						this.rads[i, 1] *= ((this.rads[i, 0] >= this.circles[i].snapRad) ? 0.95f : 0.8f);
						this.rads[i, 1] += (this.circles[i].snapRad - this.rads[i, 0]) * 0.2f;
						this.rads[i, 0] = Custom.LerpAndTick(this.rads[i, 0], this.circles[i].snapRad, 0.0001f, 0.2f);
					}
				}
				float num = 1f;
				this.circles[0].color = 0;
				if (this.number < this.meter.ShowSurvivalLimit && this.number >= this.meter.showCount && !this.eaten && ((this.meter.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player && (this.meter.hud.owner as Player).room != null && (this.meter.hud.owner as Player).room.abstractRoom.shelter && !(this.meter.hud.owner as Player).room.world.brokenShelters[(this.meter.hud.owner as Player).room.abstractRoom.shelterIndex] && !(this.meter.hud.owner as Player).stillInStartShelter && !(this.meter.hud.owner as Player).readyForWin) || (((this.meter.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.SleepScreen && (this.meter.hud.owner is SleepAndDeathScreen) && (this.meter.hud.owner as SleepAndDeathScreen).goalMalnourished) || ((this.meter.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.SleepScreen && (this.meter.hud.owner is MultiplayerSleepAndDeathScreen) && (this.meter.hud.owner as MultiplayerSleepAndDeathScreen).goalMalnourished))) && this.meter.sleepScreenPhase < 3)))
				{
					if (this.meter.timeCounter % 20 > 10)
					{
						this.rads[0, 0] *= 0.96f;
						this.circles[0].color = 1;
					}
					num = 0.65f + 0.35f * Mathf.Sin((float)this.meter.timeCounter / 20f * 3.14159274f * 2f);
				}
				for (int j = 0; j < this.circles.Length; j++)
				{
					this.circles[j].Update();
					this.circles[j].pos = this.DrawPos(1f);
					this.circles[j].rad = this.rads[j, 0];
				}
				this.circles[0].fade = ((!this.plopped) ? 0f : (this.meter.fade * num));
				if (this.eaten)
				{
					this.eatCounter--;
					if (this.eatCounter < 1)
					{
						this.foodPlopped = false;
						this.eaten = false;
					}
					this.circles[1].fade = Mathf.Pow(Mathf.InverseLerp(0f, 35f, (float)this.eatCounter), 1.2f) * this.meter.fade;
					if (this.eatCounter > 30)
					{
						this.rads[0, 0] = Custom.LerpAndTick(this.rads[0, 0], 13f + 0.5f * Mathf.Sin(Mathf.InverseLerp(30f, 50f, (float)this.eatCounter) * 3.14159274f), 0.02f, 1.5f + 0.5f * Mathf.InverseLerp(30f, 50f, (float)this.eatCounter));
						this.rads[0, 1] *= 0f;
					}
					else if (this.eatCounter == 29)
					{
						this.rads[0, 1] = -1.5f;
						this.meter.hud.PlaySound(SoundID.HUD_Food_Meter_Deplete_Plop_B);
					}
					this.rads[1, 0] = Custom.LerpMap((float)this.eatCounter, 40f, 0f, this.circles[1].snapRad, this.circles[1].snapRad / 2f);
				}
				else
				{
					if (this.meter.refuseCounter > 0)
					{
						this.rads[0, 0] += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.5f;
						this.rads[1, 0] += Mathf.Lerp(-0.25f, 1f, UnityEngine.Random.value);
						this.rads[1, 1] += UnityEngine.Random.value * 0.4f;
						if (this.rads[1, 0] + 1f > this.rads[0, 0])
						{
							this.rads[0, 0] = this.rads[1, 0] + 1f;
							this.rads[0, 1] += 0.2f + UnityEngine.Random.value * 0.4f;
						}
					}
					this.circles[1].fade = ((!this.plopped || !this.foodPlopped) ? 0f : this.meter.fade);
				}
				if (this.foodPlopDelay > 0)
				{
					this.foodPlopDelay--;
					if (this.foodPlopDelay == 12)
					{
						this.rads[0, 0] = this.circles[0].snapRad + 2f;
						this.rads[0, 1] += 1f;
						this.meter.hud.PlaySound(SoundID.HUD_Food_Meter_Fill_Plop_B);
					}
					else if (this.foodPlopDelay == 0)
					{
						this.meter.hud.fadeCircles.Add(new FadeCircle(this.meter.hud, 10f, 10f, 0.82f, 30f, 4f, this.DrawPos(1f), this.meter.fContainer));
						this.meter.hud.PlaySound(SoundID.HUD_Food_Meter_Fill_Fade_Circle);
					}
				}
				if (this.meter.lastFade == 0f)
				{
					this.plopped = false;
				}
				this.circles[0].visible = this.plopped;
				this.circles[1].visible = (this.plopped && this.foodPlopped);
			}
		}

	}
}
