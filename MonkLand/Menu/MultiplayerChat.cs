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
    class MultiplayerChat: RectangularMenuObject, Slider.ISliderOwner {
        public float scrollValue = 0;
        public RoundedRect backgroundRect;

        public static List<string> chatStrings = new List<string>();
        public List<MenuLabel> chatMessages = new List<MenuLabel>();
        private HashSet<string> chatHash = new HashSet<string>();

        public VerticalSlider slider;

        public MultiplayerChat(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base( menu, owner, pos, size ) {
            backgroundRect = new RoundedRect( menu, owner, pos, size, true );
            this.subObjects.Add( backgroundRect );

            slider = new VerticalSlider( menu, this, "Slider", new Vector2( -20f, 9f ), new Vector2( 30f, this.size.y - 40f ), Slider.SliderID.LevelsListScroll, true );
            this.subObjects.Add( slider );
        }

        public void SliderSetValue(Slider slider, float setValue) {
            this.scrollValue = setValue;
        }

        public float ValueOfSlider(Slider slider) {
            return scrollValue;
        }

        public void ClearMessages() {

            foreach( MenuLabel ml in chatMessages )
                this.subObjects.Remove( ml );
            chatStrings.Clear();
            chatMessages.Clear();
            chatHash.Clear();
        }

        public override void Update() {
            base.Update();

            foreach( string s in chatStrings ) {
                if( chatHash.Contains( s ) )
                    continue;

                MenuLabel newLabel = new MenuLabel( this.menu, this, s, new Vector2( 5, 0 ), new Vector2( this.size.x - 10, 20 ), false );
                chatHash.Add( s );
                chatMessages.Add( newLabel );
                this.subObjects.Add( newLabel );
            }

            //Update stuff
            {
                //The total height in pixels that the players take up on the scroll menu
                float messagePixelHeight = 5 + ( chatMessages.Count * 25 );
                //The max height the scrollbar can display
                float maxDisplayHeight = this.size.y - 30;
                float maxDisplayTransition = this.size.y - 40;

                float difference = messagePixelHeight - maxDisplayHeight;

                if( difference < 0 ) {
                    scrollValue = 0;
                }

                int i = 0;

                foreach( MenuLabel item in chatMessages ) {
                    float actualY = 5 + ( (chatMessages.Count - i - 1) * 25 ) - ( difference * scrollValue );
                    item.pos = new Vector2( item.pos.x, actualY );

                    float targetAlpha = 1;

                    if( actualY < 10 ) {
                        targetAlpha = actualY / 10f;
                    } else if( actualY > maxDisplayHeight ) {
                        targetAlpha = 0;
                    } else if( actualY > maxDisplayTransition ) {
                        targetAlpha = 1 - ( ( actualY - maxDisplayTransition ) / 10f );
                    } else {
                        targetAlpha = 1;
                    }

                    item.label.alpha = targetAlpha;

                    i++;
                }
            }

        }
    }
}