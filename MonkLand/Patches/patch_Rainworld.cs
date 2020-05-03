using Monkland.SteamManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Monkland.Patches {
    [MonoMod.MonoModPatch( "global::RainWorld" )]
    class patch_Rainworld : RainWorld {

        public static RainWorld mainRW;

        public extern void orig_Start();
        public void Start() {
            mainRW = this;
            try {
                if( MonklandSteamManager.instance == null ) {
                    MonklandSteamManager.CreateManager();
                }
            } catch( System.Exception e ) {
                Debug.Log( e );
            }


            orig_Start();
            this.buildType = BuildType.Development;
            setup.devToolsActive = true;
        }

    }
}
