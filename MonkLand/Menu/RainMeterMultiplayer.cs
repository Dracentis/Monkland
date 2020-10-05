using HUD;
using Menu;
using Monkland.SteamManagement;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Monkland
{
    class RainMeterMultiplayer : HudPart
    {
		public Vector2 pos;
		public Vector2 lastPos;
		public int remainVisibleCounter;
		public float fade;
		public float lastFade;
		public float plop;
		public float lastPlop;
		public HUDCircle[] circles;
		public float fRain;
		public int halfTimeBlink;
		public bool halfTimeShown;

		public RainMeterMultiplayer(HUD.HUD hud, FContainer fContainer) : base(hud)
		{
            this.lastPos = this.pos;
            if (MonklandSteamManager.isInGame && MonklandSteamManager.WorldManager != null)
			{
				this.circles = new HUDCircle[MonklandSteamManager.WorldManager.cycleLength / 1200];
			}
			for (int i = 0; i < this.circles.Length; i++)
			{
				this.circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
			}
		}

		public bool Show
		{
			get
			{
				return this.halfTimeBlink > 0 || this.hud.showKarmaFoodRain || this.hud.owner.RevealMap;
			}
		}

		public override void Update()
		{
            this.lastPos = this.pos;

            this.lastFade = this.fade;
            if (this.remainVisibleCounter > 0)
            {
                this.remainVisibleCounter--;
            }

            this.lastPlop = this.plop;
            if (this.fade >= 0.7f)
            {
                this.plop = Mathf.Min(1f, this.plop + 0.05f);
            }
            else
            {
                this.plop = 0f;
            }

            
            if (MonklandSteamManager.isInGame && MonklandSteamManager.WorldManager != null)
            {
                this.fRain = (float)(MonklandSteamManager.WorldManager.cycleLength - MonklandSteamManager.WorldManager.timer) / (float)MonklandSteamManager.WorldManager.cycleLength;
                this.fade = 1f;
            }
            

            for (int i = 0; i < this.circles.Length; i++)
            {
                this.circles[i].Update();
                if (this.fade > 0f || this.lastFade > 0f)
                {
                    float num = (float)i / (float)(this.circles.Length - 1);
                    float value = Mathf.InverseLerp((float)i / (float)this.circles.Length, (float)(i + 1) / (float)this.circles.Length, this.fRain);
                    float num2 = Mathf.InverseLerp(0.5f, 0.475f, Mathf.Abs(0.5f - Mathf.InverseLerp(0.0333333351f, 1f, value)));
                    if (this.halfTimeBlink > 0)
                    {
                        num2 = Mathf.Max(num2, (this.halfTimeBlink % 15 >= 7) ? 1f : 0f);
                    }
                    this.circles[i].rad = ((2f + num2) * Mathf.Pow(this.fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - this.fRain) * this.fade - 0.075f, 1.075f, Mathf.Pow(this.plop, 0.85f)))) * 2f * this.fade) * Mathf.InverseLerp(0f, 0.0333333351f, value);
                    if (num2 == 0f)
                    {
                        this.circles[i].thickness = -1f;
                        this.circles[i].snapGraphic = HUDCircle.SnapToGraphic.Circle4;
                        this.circles[i].snapRad = 2f;
                        this.circles[i].snapThickness = -1f;
                    }
                    else
                    {
                        this.circles[i].thickness = Mathf.Lerp(3.5f, 1f, num2);
                        this.circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
                        this.circles[i].snapRad = 3f;
                        this.circles[i].snapThickness = 1f;
                    }

                        this.circles[i].pos = this.pos + Custom.DegToVec((1f - (float)i / (float)this.circles.Length) * 360f * Custom.SCurve(Mathf.Pow(this.fade, 1.5f - num), 0.6f)) * (22.5f + 8.5f + num2);
                }
                else
                {
                    this.circles[i].rad = 0f;
                }
            }
        }

		public Vector2 DrawPos(float timeStacker)
		{
			return Vector2.Lerp(this.lastPos, this.pos, timeStacker);
		}

		public override void Draw(float timeStacker)
		{
			float num = Mathf.Pow(Mathf.Lerp(this.lastFade, this.fade, timeStacker), 1.5f);
			for (int i = 0; i < this.circles.Length; i++)
			{
				this.circles[i].Draw(timeStacker);
			}
		}


	}
}

