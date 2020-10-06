using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Monkland.UI
{
    class MUIButton : HudPart
    {
        public MUIBox box;
        public MUILabel label;

        public MUIButton(HUD.HUD hud, Vector2 pos, string label) : base(hud)
        {

        }

    }
}
