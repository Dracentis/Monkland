using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Monkland.UI
{
    public abstract class MUIHUD
    {
        public Vector2 pos;
        public bool isVisible;
        public abstract void Update();

        public abstract void Draw(float timeStacker);

        public abstract void ClearSprites();

    }
}
