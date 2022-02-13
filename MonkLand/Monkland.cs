using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using Monkland.Hooks;
using Monkland.Hooks.Menus;

namespace Monkland
{
    public class Monkland : PartialityMod
    {
        public static readonly string VERSION = typeof(Monkland).Assembly.GetName().Version.ToString().Substring(0, typeof(Monkland).Assembly.GetName().Version.ToString().Length-2); // Version number
        public const bool DEVELOPMENT = true; // Is this build for development
        public static Monkland instance; // For future Config Machine support

        public Monkland()
        {
            instance = this;
            ModID = "Monkland";
            Version = VERSION;
            author = "Dracentis, Garrakx, the1whoscreamsiguess, notfood"; // other authors added
        }
        public override void OnEnable()
        {
            base.OnEnable();
            // Hooking is done here

            RainWorldHK.ApplyHook();
            RainWorldGameHK.ApplyHook();
            ProcessManagerHK.ApplyHook();
            SaveStateHK.ApplyHook();

            PlayerGraphicsHK.ApplyHook();

            #region User Interface
            MainMenuHK.ApplyHook();
            #endregion User Interface
        }
    }
}