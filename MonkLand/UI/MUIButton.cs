using UnityEngine;

namespace Monkland.UI
{
    internal class MUIButton : MUIHUD
    {
        public MUIBox box;
        public MUILabel label;
        public Vector2 size;
        public string signalString;

        public MUIButton(MultiplayerHUD owner, Vector2 pos, string signalString, string labelString) : base(owner, pos)
        {
            Debug.Log($"Monkland) Creating MUIButton {pos}");

            this.signalString = signalString;

            this.box = new MUIBox(owner, pos, new Vector2(110f, 30f));

            this.label = new MUILabel(owner, labelString, Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey), pos + new Vector2(0, -MUIBox.lineHeight - 5f));
            this.size = box.drawSize;
            this.pos.x -= box.drawSize.x;
        }

        public override void ClearSprites()
        {
            label.ClearSprites();
            box.ClearSprites();
        }

        public override void Draw(float timeStacker)
        {
            label.Draw(timeStacker);
            box.Draw(timeStacker);
        }

        private bool mouseDown;

        public override void Update()
        {
            if (MouseOver)
            {
                if (Input.GetMouseButton(0))
                { mouseDown = true; }
                else if (mouseDown)
                {
                    mouseDown = false;
                    this.owner.Signal(this, signalString);
                }
            }
            else if (!Input.GetMouseButton(0))
            { mouseDown = false; }

            label.isVisible = this.isVisible;
            box.isVisible = this.isVisible;

            label.Update();
            box.Update();
        }

        public bool MouseOver
        {
            get
            {
                return this.MousePos.x > this.pos.x
                    && this.MousePos.x < this.pos.x + this.size.x
                    && this.MousePos.y > this.pos.y
                    && this.MousePos.y < this.pos.y + this.size.y;
            }
        }
    }
}
