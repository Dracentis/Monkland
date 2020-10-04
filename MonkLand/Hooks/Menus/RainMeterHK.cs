using HUD;
using Menu;
using Monkland.SteamManagement;
using RWCustom;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Monkland.Hooks.Menus
{
    internal static class RainMeterHK
    {
        public static void ApplyHook()
        {
            On.HUD.RainMeter.ctor += new On.HUD.RainMeter.hook_ctor(CtorHK);
            On.HUD.RainMeter.Update += new On.HUD.RainMeter.hook_Update(UpdateHK);
        }

        private static void CtorHK(On.HUD.RainMeter.orig_ctor orig, RainMeter self, HUD.HUD hud, FContainer fContainer)
        {
            bool isMulti = true;
            if (!MonklandSteamManager.isInGame || MonklandSteamManager.WorldManager == null)
            {
                isMulti = false;
                if (hud.owner != null && hud.owner is Player p && p.room != null)
                {
                    orig(self, hud, fContainer);
                }
                else
                {
                    noOrigCtor(self, isMulti, hud, fContainer);
                }
            }
            else if (hud.owner != null && hud.owner is Player p && p.room != null)
            {
                orig(self, hud, fContainer);
                for (int i = 0; i < self.circles.Length; i++)
                {
                    // Remove Old Circles
                    self.circles[i].ClearSprite();
                }

                self.circles = new HUDCircle[MonklandSteamManager.WorldManager.cycleLength / 1200];
                for (int i = 0; i < self.circles.Length; i++)
                {
                    self.circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
                }
            }
            else
            {
                //orig ctor will cause NullRef
                noOrigCtor(self, isMulti, hud, fContainer);
            }
        }

        public static void noOrigCtor(RainMeter self, bool isMulti, HUD.HUD hud, FContainer fContainer)
        {
            Type[] constructorSignature = new Type[1];
            constructorSignature[0] = typeof(HUD.HUD);
            RuntimeMethodHandle handle = typeof(HudPart).GetConstructor(constructorSignature).MethodHandle;
            RuntimeHelpers.PrepareMethod(handle);
            IntPtr ptr = handle.GetFunctionPointer();
            Action<HUD.HUD> funct = (Action<HUD.HUD>)Activator.CreateInstance(typeof(Action<HUD.HUD>), self, ptr);
            funct(hud); //HudPart Constructor

            self.lastPos = self.pos;
            self.circles = new HUDCircle[(isMulti ? MonklandSteamManager.WorldManager.cycleLength : RainWorldGameHK.mainGame.world.rainCycle.cycleLength) / 1200];
            for (int i = 0; i < self.circles.Length; i++)
            {
                self.circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
            }
        }

        private static bool Show(RainMeter self)
        {
            return self.halfTimeBlink > 0 || self.hud.showKarmaFoodRain || self.hud.owner.RevealMap
                || (self.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.DeathScreen) || (self.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.SleepScreen);
        }

        private static bool NextCycleMeter(RainMeter self) => (self.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.DeathScreen) || (self.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.SleepScreen);

        private struct CircleBak
        {
            public CircleBak(HUDCircle orig) : this()
            {
                pos = orig.pos;
                rad = orig.rad;
                thickness = orig.thickness;
                fade = orig.fade;
                snapRad = orig.snapRad;
                snapThickness = orig.snapThickness;
                snapGraphic = orig.snapGraphic;
            }

            public void Apply(HUDCircle mod)
            {
                mod.pos = this.pos;
                mod.rad = this.rad;
                mod.thickness = this.thickness;
                mod.fade = this.fade;
                mod.snapRad = this.snapRad;
                mod.snapThickness = this.snapThickness;
                mod.snapGraphic = this.snapGraphic;
            }

            public Vector2 pos;
            public float rad, thickness, fade, snapRad, snapThickness;
            public HUDCircle.SnapToGraphic snapGraphic;
        }

        private static void UpdateHK(On.HUD.RainMeter.orig_Update orig, RainMeter self)
        {
            if (self.hud.owner != null && self.hud.owner is Player)
            {
                int oldHalfTimeBlink = self.halfTimeBlink;
                CircleBak[] cbk = new CircleBak[self.circles.Length];
                for (int i = 0; i < self.circles.Length; i++) { cbk[i] = new CircleBak(self.circles[i]); } // backup circles

                orig.Invoke(self);

                for (int i = 0; i < self.circles.Length; i++) { cbk[i].Apply(self.circles[i]); } // revert circles

                // update fade
                self.halfTimeBlink = oldHalfTimeBlink;
                self.fade = self.lastFade;
            }
            else
            { // orig_Update
                self.lastPos = self.pos;
                self.pos = self.hud.karmaMeter.pos;
                if (!self.halfTimeShown && RainWorldGameHK.mainGame.world.rainCycle.AmountLeft < 0.5f)
                {
                    self.halfTimeBlink = 220;
                    self.halfTimeShown = true;
                }
                self.lastFade = self.fade;
                if (self.remainVisibleCounter > 0) { self.remainVisibleCounter--; }
                self.lastPlop = self.plop;
                if (self.fade >= 0.7f) { self.plop = Mathf.Min(1f, self.plop + 0.05f); }
                else { self.plop = 0f; }
            }

            if (!NextCycleMeter(self))
            {
                if (self.halfTimeBlink > 0)
                {
                    self.halfTimeBlink--;
                    self.hud.karmaMeter.forceVisibleCounter = Math.Max(self.hud.karmaMeter.forceVisibleCounter, 10);
                }
                if ((self.hud.karmaMeter.fade > 0f && Show(self)) || self.remainVisibleCounter > 0)
                { self.fade = Mathf.Min(1f, self.fade + 0.0333333351f); }
                else
                { self.fade = Mathf.Max(0f, self.fade - 0.1f); }
                if (self.hud.HideGeneralHud) { self.fade = 0f; }
            }

            // replace fRain
            if (!NextCycleMeter(self))
            {
                self.fRain = RainWorldGameHK.mainGame.world.rainCycle.AmountLeft;
            }
            else if (NextCycleMeter(self) && MonklandSteamManager.isInGame && MonklandSteamManager.WorldManager != null)
            {
                self.fRain = (float)(MonklandSteamManager.WorldManager.cycleLength - MonklandSteamManager.WorldManager.timer) / (float)MonklandSteamManager.WorldManager.cycleLength;
                self.fade = 1f;
            }

            // update circles
            for (int i = 0; i < self.circles.Length; i++)
            {
                self.circles[i].Update();
                if (self.fade > 0f || self.lastFade > 0f)
                {
                    float num = (float)i / (float)(self.circles.Length - 1);
                    float value = Mathf.InverseLerp((float)i / (float)self.circles.Length, (float)(i + 1) / (float)self.circles.Length, self.fRain);
                    float num2 = Mathf.InverseLerp(0.5f, 0.475f, Mathf.Abs(0.5f - Mathf.InverseLerp(0.0333333351f, 1f, value)));
                    if (self.halfTimeBlink > 0)
                    {
                        num2 = Mathf.Max(num2, (self.halfTimeBlink % 15 >= 7) ? 1f : 0f);
                    }
                    self.circles[i].rad = ((2f + num2) * Mathf.Pow(self.fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - self.fRain) * self.fade - 0.075f, 1.075f, Mathf.Pow(self.plop, 0.85f)))) * 2f * self.fade) * Mathf.InverseLerp(0f, 0.0333333351f, value);
                    if (num2 == 0f)
                    {
                        self.circles[i].thickness = -1f;
                        self.circles[i].snapGraphic = HUDCircle.SnapToGraphic.Circle4;
                        self.circles[i].snapRad = 2f;
                        self.circles[i].snapThickness = -1f;
                    }
                    else
                    {
                        self.circles[i].thickness = Mathf.Lerp(3.5f, 1f, num2);
                        self.circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
                        self.circles[i].snapRad = 3f;
                        self.circles[i].snapThickness = 1f;
                    }
                    if (NextCycleMeter(self))
                    {
                        self.circles[i].pos = self.pos + Custom.DegToVec((1f - (float)i / (float)self.circles.Length) * 360f * Custom.SCurve(Mathf.Pow(self.fade, 1.5f - num), 0.6f)) * (22.5f + 8.5f + num2);
                    }
                    else
                    {
                        self.circles[i].pos = self.pos + Custom.DegToVec((1f - (float)i / (float)self.circles.Length) * 360f * Custom.SCurve(Mathf.Pow(self.fade, 1.5f - num), 0.6f)) * (self.hud.karmaMeter.Radius + 8.5f + num2);
                    }
                }
                else
                {
                    self.circles[i].rad = 0f;
                }
            }
        }
    }
}
