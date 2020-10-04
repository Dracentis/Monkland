using RWCustom;
using HUD;
using UnityEngine;
using MonoMod;
using System;
using System.Runtime.CompilerServices;
using Monkland.SteamManagement;
using Menu;
using HUD;

namespace Monkland.Patches
{
    [MonoModPatch("global::HUD.RainMeter")]
    class patch_RainMeter : RainMeter
    {
        [MonoModIgnore]
        public patch_RainMeter(HUD.HUD hud, FContainer fContainer) : base(hud, fContainer)
        {
        }

        public extern void orig_ctor(HUD.HUD hud, FContainer fContainer);

        [MonoModConstructor]
        public void ctor(HUD.HUD hud, FContainer fContainer)
        {
            Type[] constructorSignature = new Type[1];
            constructorSignature[0] = typeof(HUD.HUD);
            RuntimeMethodHandle handle = typeof(HudPart).GetConstructor(constructorSignature).MethodHandle;
            RuntimeHelpers.PrepareMethod(handle);
            IntPtr ptr = handle.GetFunctionPointer();
            Action<HUD.HUD> funct = (Action<HUD.HUD>)Activator.CreateInstance(typeof(Action<HUD.HUD>), this, ptr);
            funct(hud);//HudPart Constructor
            this.lastPos = this.pos;

            if (hud.owner != null & hud.owner is Player)
            {
                if (MonklandSteamManager.isInGame)
                {
                    this.circles = new HUDCircle[MonklandSteamManager.WorldManager.cycleLength / 1200];
                }
                else
                {
                    this.circles = new HUDCircle[(hud.owner as Player).room.world.rainCycle.cycleLength / 1200];
                }
            }
            else if (hud.owner != null & (hud.owner is MultiplayerSleepAndDeathScreen) && MonklandSteamManager.isInGame && MonklandSteamManager.WorldManager != null)
            {
                this.circles = new HUDCircle[MonklandSteamManager.WorldManager.cycleLength / 1200];
            }
            for (int i = 0; i < this.circles.Length; i++)
            {
                this.circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
            }
        }

        private float rainMeterXPositionOnSleepScreen
        {
            get
            {
                return this.hud.rainWorld.options.ScreenSize.x - 100f;
            }
        }

        private float rainMeterYPositionOnSleepScreen
        {
            get
            {
                return this.hud.rainWorld.options.ScreenSize.y - 100f;
            }
        }

        [MonoModIgnore]
        private bool halfTimeShown;

        [MonoModIgnore]
        private float plop;

        [MonoModIgnore]
        private float lastPlop;

        [MonoModIgnore]
        private float fRain;

        public override void Update()
        {
            this.lastPos = this.pos;
            if (!NextCycleMeter)
            {
                this.pos = this.hud.karmaMeter.pos;
                if (!this.halfTimeShown && (this.hud.owner as Player).room != null && (this.hud.owner as Player).room.world.rainCycle.AmountLeft < 0.5f && (this.hud.owner as Player).room.roomSettings.DangerType != RoomRain.DangerType.None)
                {
                    this.halfTimeBlink = 220;
                    this.halfTimeShown = true;
                }
            }
            this.lastFade = this.fade;
            if (this.remainVisibleCounter > 0)
            {
                this.remainVisibleCounter--;
            }
            if (!NextCycleMeter)
            {
                if (this.halfTimeBlink > 0)
                {
                    this.halfTimeBlink--;
                    this.hud.karmaMeter.forceVisibleCounter = Math.Max(this.hud.karmaMeter.forceVisibleCounter, 10);
                }
                if ((this.hud.karmaMeter.fade > 0f && this.Show) || this.remainVisibleCounter > 0)
                {
                    this.fade = Mathf.Min(1f, this.fade + 0.0333333351f);
                }
                else
                {
                    this.fade = Mathf.Max(0f, this.fade - 0.1f);
                }
                if (this.hud.HideGeneralHud)
                {
                    this.fade = 0f;
                }
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
            if (!NextCycleMeter && (this.hud.owner as Player).room != null)
            {
                this.fRain = (this.hud.owner as Player).room.world.rainCycle.AmountLeft;
            }
            else if (NextCycleMeter && MonklandSteamManager.isInGame && MonklandSteamManager.WorldManager != null)
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
                    if (NextCycleMeter)
                    {
                        this.circles[i].pos = this.pos + Custom.DegToVec((1f - (float)i / (float)this.circles.Length) * 360f * Custom.SCurve(Mathf.Pow(this.fade, 1.5f - num), 0.6f)) * (22.5f + 8.5f + num2);
                    }
                    else
                    {
                        this.circles[i].pos = this.pos + Custom.DegToVec((1f - (float)i / (float)this.circles.Length) * 360f * Custom.SCurve(Mathf.Pow(this.fade, 1.5f - num), 0.6f)) * (this.hud.karmaMeter.Radius + 8.5f + num2);
                    }
                }
                else
                {
                    this.circles[i].rad = 0f;
                }
            }
        }

        private bool NextCycleMeter
        {
            get
            {
                return (this.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.DeathScreen) || (this.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.SleepScreen);
            }
        }

        private bool Show
        {
            get
            {
                return this.halfTimeBlink > 0 || this.hud.showKarmaFoodRain || this.hud.owner.RevealMap || (this.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.DeathScreen) || (this.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.SleepScreen);
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