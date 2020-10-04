using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Monkland.UI
{
    public class MUILabel : MUIHUD
    {
        public FLabel label;
        public Color color;
        public MultiplayerHUD owner;
        public Vector2 pos;
        public float alpha;

        public MUILabel(MultiplayerHUD owner, string labelName, Color color, Vector2 pos)
        {
            this.owner = owner;
            this.label = new FLabel("font", labelName);
            this.color = color;
            this.label.color = color;
            this.owner.frontContainer.AddChild(this.label);
            this.label.alpha = 0f;
            this.label.x = -1000f;
            this.pos = pos;

        }

        public RoomCamera Camera
        {
            get
            {
                return (this.owner.hud.owner as Player).abstractCreature.world.game.cameras[0];
            }
        }

        public override void Update()
        {

        }

        public override void Draw(float timeStacker)
        {
            this.label.x = pos.x;
            this.label.y = pos.y + 20f;
            this.label.color = this.color;
            this.label.alpha = alpha;
        }

        public override void ClearSprites()
        {
            this.label.RemoveFromContainer();
        }
    }
}
