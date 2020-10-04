using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System;
using Menu;
using MonoMod;
using UnityEngine;

namespace Monkland.Patches
{
    [MonoMod.MonoModPatch("global::Menu.Slider")]
    class patch_Slider : Menu.Slider
    {
        [MonoMod.MonoModIgnore]
        public patch_Slider(Menu.Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, Slider.SliderID ID, bool subtleSlider) : base(menu, owner, text, pos, size, ID, subtleSlider) { }

        [MonoMod.MonoModIgnore]
        public extern void OriginalConstructor(Menu.Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, Slider.SliderID ID, bool subtleSlider);

        [MonoMod.MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
        public void ctor_MainMenu(Menu.Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, Slider.SliderID ID, bool subtleSlider)
        {
            OriginalConstructor(menu, owner, text, pos, size, ID, subtleSlider);
        }

        public enum SliderID//created new SliderID's for the multiplayer settings
        {
            SfxVol,

            MusicVol,

            ArenaMusicVolume,

            LevelsListScroll,

            BodyRed,

            BodyGreen,

            BodyBlue,

            EyesRed,

            EyesGreen,

            EyesBlue,

            MaxPlayers
        }
    }
}