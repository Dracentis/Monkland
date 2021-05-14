using UnityEngine;

namespace Monkland.UI
{
    public abstract class MUIHUD
    {
        public Vector2 pos;
        public bool isVisible;
        public MultiplayerHUD owner;

        protected MUIHUD(MultiplayerHUD owner, Vector2 pos)
        {
            this.pos = pos;
            this.owner = owner;
        }

        public abstract void Update();

        public abstract void Draw(float timeStacker);

        public abstract void ClearSprites();

        internal Vector2 ScreenPos
        {
            get
            {
                if (this.owner == null) { return Vector2.zero; }
                return this.owner.screenPos;
            }
        }

        internal Vector2 MousePos
        {
            get
            {
                return new Vector2(this.owner.mousePos.x - this.ScreenPos.x, this.owner.mousePos.y - this.ScreenPos.y);
            }
        }
    }
}
