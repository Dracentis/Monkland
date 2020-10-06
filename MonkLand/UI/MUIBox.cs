using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Monkland.UI
{
    public class MUIBox : MUIHUD
    {
        public static float meanCharWidth;
        public static float lineHeight;
        public static float heightMargin;
        public static float widthMargin;
        public FSprite[] sprites;

        MultiplayerHUD owner;

        private int longestLineX;
        private int numberLines;

        public MUIBox(MultiplayerHUD owner, Vector2 pos, int longestLineX, int numberLines)
        {
            //this.messages = new List<DialogBox.Message>();
            this.pos = pos;
            this.owner = owner;
            this.longestLineX = longestLineX;
            this.numberLines = numberLines;
            this.InitiateSprites();
        }

        static MUIBox()
        {
            MUIBox.meanCharWidth = 6f;
            MUIBox.lineHeight = 15f;
            MUIBox.heightMargin = 25f;
            MUIBox.widthMargin = 30f;
        }


        public int SideSprite(int side)
        {
            return 9 + side;
        }

        public int CornerSprite(int corner)
        {
            return 13 + corner;
        }


        public int FillSideSprite(int side)
        {
            return side;
        }

        public int FillCornerSprite(int corner)
        {
            return 4 + corner;
        }
        public int MainFillSprite
        {
            get
            {
                return 8;
            }
        }

        private void InitiateSprites()
        {
            this.sprites = new FSprite[17];
            for (int i = 0; i < 4; i++)
            {
                this.sprites[this.SideSprite(i)] = new FSprite("pixel", true);
                this.sprites[this.SideSprite(i)].scaleY = 2f;
                this.sprites[this.SideSprite(i)].scaleX = 2f;
                this.sprites[this.CornerSprite(i)] = new FSprite("UIroundedCorner", true);
                this.sprites[this.FillSideSprite(i)] = new FSprite("pixel", true);
                this.sprites[this.FillSideSprite(i)].scaleY = 6f;
                this.sprites[this.FillSideSprite(i)].scaleX = 6f;
                this.sprites[this.FillCornerSprite(i)] = new FSprite("UIroundedCornerInside", true);
            }
            this.sprites[this.SideSprite(0)].anchorY = 0f;
            this.sprites[this.SideSprite(2)].anchorY = 0f;
            this.sprites[this.SideSprite(1)].anchorX = 0f;
            this.sprites[this.SideSprite(3)].anchorX = 0f;
            this.sprites[this.CornerSprite(0)].scaleY = -1f;
            this.sprites[this.CornerSprite(2)].scaleX = -1f;
            this.sprites[this.CornerSprite(3)].scaleY = -1f;
            this.sprites[this.CornerSprite(3)].scaleX = -1f;
            this.sprites[this.MainFillSprite] = new FSprite("pixel", true);
            this.sprites[this.MainFillSprite].anchorY = 0f;
            this.sprites[this.MainFillSprite].anchorX = 0f;
            this.sprites[this.FillSideSprite(0)].anchorY = 0f;
            this.sprites[this.FillSideSprite(2)].anchorY = 0f;
            this.sprites[this.FillSideSprite(1)].anchorX = 0f;
            this.sprites[this.FillSideSprite(3)].anchorX = 0f;
            this.sprites[this.FillCornerSprite(0)].scaleY = -1f;
            this.sprites[this.FillCornerSprite(2)].scaleX = -1f;
            this.sprites[this.FillCornerSprite(3)].scaleY = -1f;
            this.sprites[this.FillCornerSprite(3)].scaleX = -1f;
            for (int j = 0; j < 9; j++)
            {
                this.sprites[j].color = new Color(0f, 0f, 0f);
                this.sprites[j].alpha = 0.75f;
            }

            /*
            this.label = new FLabel("font", string.Empty);
            this.label.alignment = FLabelAlignment.Left;
            this.label.anchorX = 0f;
            this.label.anchorY = 1f;
            */

            for (int k = 0; k < this.sprites.Length; k++)
            {
                this.owner.container.AddChild(this.sprites[k]);
            }
            //this.owner.container.AddChild(this.label);
        }

        public override void ClearSprites()
        {
            for (int i = 0; i < this.sprites.Length; i++)
            {
                this.sprites[i].RemoveFromContainer();
            }
        }

        public override void Draw(float timeStacker)
        {
            //base.Draw(timeStacker);
            for (int i = 0; i < this.sprites.Length; i++)
            {
                this.sprites[i].isVisible = this.isVisible;
            }

            /*
            this.label.isVisible = (this.CurrentMessage != null);
            if (this.CurrentMessage == null)
            {
                return;
            }
            */
            Vector2 vecPos = pos;
            Vector2 drawSize = new Vector2(0f, 0);//DialogBox.heightMargin + DialogBox.lineHeight * (float)this.CurrentMessage.lines);

            drawSize.x = MUIBox.widthMargin + (float)this.longestLineX + MUIBox.widthMargin;  //* MUIBox.meanCharWidth;
            drawSize.y = MUIBox.heightMargin + MUIBox.lineHeight * (float)this.numberLines;

            //drawSize.x = Mathf.Lerp(40f, drawSize.x, Mathf.Pow(Mathf.Lerp(this.lastSizeFac, this.sizeFac, timeStacker), 0.5f));

            //drawSize.y *= 0.5f + 0.5f * Mathf.Lerp(this.lastSizeFac, this.sizeFac, timeStacker);
            vecPos.x -= 0.333333343f;
            vecPos.y -= 0.333333343f;
            /*
            this.label.x = vector.x - (float)this.CurrentMessage.longestLine * DialogBox.meanCharWidth * 0.5f;
            this.label.y = vector.y + vector2.y / 2f - DialogBox.lineHeight * 0.6666f;
            this.label.text = this.showText;
            */
            vecPos.x -= drawSize.x / 2f;
            vecPos.y -= drawSize.y / 2f;
            this.sprites[this.SideSprite(0)].x = vecPos.x + 1f;
            this.sprites[this.SideSprite(0)].y = vecPos.y + 6f;
            this.sprites[this.SideSprite(0)].scaleY = drawSize.y - 12f;
            this.sprites[this.SideSprite(1)].x = vecPos.x + 6f;
            this.sprites[this.SideSprite(1)].y = vecPos.y + drawSize.y - 1f;
            this.sprites[this.SideSprite(1)].scaleX = drawSize.x - 12f;
            this.sprites[this.SideSprite(2)].x = vecPos.x + drawSize.x - 1f;
            this.sprites[this.SideSprite(2)].y = vecPos.y + 6f;
            this.sprites[this.SideSprite(2)].scaleY = drawSize.y - 12f;
            this.sprites[this.SideSprite(3)].x = vecPos.x + 6f;
            this.sprites[this.SideSprite(3)].y = vecPos.y + 1f;
            this.sprites[this.SideSprite(3)].scaleX = drawSize.x - 12f;
            this.sprites[this.CornerSprite(0)].x = vecPos.x + 3.5f;
            this.sprites[this.CornerSprite(0)].y = vecPos.y + 3.5f;
            this.sprites[this.CornerSprite(1)].x = vecPos.x + 3.5f;
            this.sprites[this.CornerSprite(1)].y = vecPos.y + drawSize.y - 3.5f;
            this.sprites[this.CornerSprite(2)].x = vecPos.x + drawSize.x - 3.5f;
            this.sprites[this.CornerSprite(2)].y = vecPos.y + drawSize.y - 3.5f;
            this.sprites[this.CornerSprite(3)].x = vecPos.x + drawSize.x - 3.5f;
            this.sprites[this.CornerSprite(3)].y = vecPos.y + 3.5f;
            Color color = new Color(1f, 1f, 1f);
            for (int j = 0; j < 4; j++)
            {
                this.sprites[this.SideSprite(j)].color = color;
                this.sprites[this.CornerSprite(j)].color = color;
            }
            this.sprites[this.FillSideSprite(0)].x = vecPos.x + 4f;
            this.sprites[this.FillSideSprite(0)].y = vecPos.y + 7f;
            this.sprites[this.FillSideSprite(0)].scaleY = drawSize.y - 14f;
            this.sprites[this.FillSideSprite(1)].x = vecPos.x + 7f;
            this.sprites[this.FillSideSprite(1)].y = vecPos.y + drawSize.y - 4f;
            this.sprites[this.FillSideSprite(1)].scaleX = drawSize.x - 14f;
            this.sprites[this.FillSideSprite(2)].x = vecPos.x + drawSize.x - 4f;
            this.sprites[this.FillSideSprite(2)].y = vecPos.y + 7f;
            this.sprites[this.FillSideSprite(2)].scaleY = drawSize.y - 14f;
            this.sprites[this.FillSideSprite(3)].x = vecPos.x + 7f;
            this.sprites[this.FillSideSprite(3)].y = vecPos.y + 4f;
            this.sprites[this.FillSideSprite(3)].scaleX = drawSize.x - 14f;
            this.sprites[this.FillCornerSprite(0)].x = vecPos.x + 3.5f;
            this.sprites[this.FillCornerSprite(0)].y = vecPos.y + 3.5f;
            this.sprites[this.FillCornerSprite(1)].x = vecPos.x + 3.5f;
            this.sprites[this.FillCornerSprite(1)].y = vecPos.y + drawSize.y - 3.5f;
            this.sprites[this.FillCornerSprite(2)].x = vecPos.x + drawSize.x - 3.5f;
            this.sprites[this.FillCornerSprite(2)].y = vecPos.y + drawSize.y - 3.5f;
            this.sprites[this.FillCornerSprite(3)].x = vecPos.x + drawSize.x - 3.5f;
            this.sprites[this.FillCornerSprite(3)].y = vecPos.y + 3.5f;
            this.sprites[this.MainFillSprite].x = vecPos.x + 7f;
            this.sprites[this.MainFillSprite].y = vecPos.y + 7f;
            this.sprites[this.MainFillSprite].scaleX = drawSize.x - 14f;
            this.sprites[this.MainFillSprite].scaleY = drawSize.y - 14f;
        }

        public override void Update()
        {
            //

            // throw new NotImplementedException();
        }
    }
}
