using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu;
using UnityEngine;
using Steamworks;
using Monkland.SteamManagement;

namespace Monkland {
    class MultiplayerDisplayMenu: RectangularMenuObject, Slider.ISliderOwner {

        public float scrollValue = 0;
        public RoundedRect backgroundRect;

        public static List<DisplayElement> displayElements = new List<DisplayElement>();
        public Vector2 displayElementSize;
        public float displayFadeStart = 10;

        public VerticalSlider slider;

        public MultiplayerDisplayMenu(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, Vector2 displayElementSize, float displayElementFade = 10) : base( menu, owner, pos, size ) {
            backgroundRect = new RoundedRect( menu, owner, pos, size, true );
            this.subObjects.Add( backgroundRect );

            slider = new VerticalSlider( menu, this, "Slider", new Vector2( -20f, 9f ), new Vector2( 30f, this.size.y - 40f ), Slider.SliderID.LevelsListScroll, true );
            this.subObjects.Add( slider );

            this.displayElementSize = displayElementSize;
            this.displayFadeStart = displayElementFade;
        }

        public void SliderSetValue(Slider slider, float setValue) {
            this.scrollValue = setValue;
        }

        public float ValueOfSlider(Slider slider) {
            return scrollValue;
        }

        public override void Update() {
            base.Update();

            //Update stuff
            {
                //The total height in pixels that the players take up on the scroll menu
                float messagePixelHeight = 5 + ( displayElements.Count * displayElementSize.y );
                //The max height the scrollbar can display
                float maxDisplayHeight = this.size.y - (displayElementSize.y * 2f);
                float maxDisplayTransition = this.size.y - ( (displayElementSize.y * 2f) + displayFadeStart );

                float difference = messagePixelHeight - maxDisplayHeight;

                if( difference < 0 ) {
                    scrollValue = 0;
                }

                int i = 0;

                foreach( DisplayElement item in displayElements ) {
                    float actualY = 5 + ( ( displayElements.Count - i - 1 ) * displayElementSize.y ) - ( difference * scrollValue );
                    item.pos = new Vector2( item.pos.x, actualY );

                    float targetAlpha = 1;

                    if( actualY < displayFadeStart ) {
                        targetAlpha = actualY / displayFadeStart;
                    } else if( actualY > maxDisplayHeight ) {
                        targetAlpha = 0;
                    } else if( actualY > maxDisplayTransition ) {
                        targetAlpha = 1 - ( ( actualY - maxDisplayTransition ) / displayFadeStart );
                    } else {
                        targetAlpha = 1;
                    }

                    item.SetAlpha(targetAlpha);

                    i++;
                }
            }

        }

        public class DisplayElement : RectangularMenuObject{
            public DisplayElement(Menu.Menu menu, MenuObject owner) : base( menu, owner, Vector2.zero, Vector2.one * 100 ) {}

            public virtual void SetAlpha(float f) { }
        }
    }
}