using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using RWCustom;
using UnityEngine;
using Monkland.SteamManagement;
using System.IO;


namespace Monkland.Patches
{
    [MonoModPatch("global::Player")]
    class patch_Player : Player
    {
        [MonoModIgnore]
        public patch_Player(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
        }

        [MonoModIgnore]
        bool corridorDrop;
        [MonoModIgnore]
        int corridorTurnCounter;
        [MonoModIgnore]
        IntVector2? corridorTurnDir;
        [MonoModIgnore]
        int crawlTurnDelay;


        public override Color ShortCutColor()
        {
            if (MonklandSteamManager.isInGame)
            {
                return MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf((this.abstractPhysicalObject as patch_AbstractPhysicalObject).owner)];
            }
            return PlayerGraphics.SlugcatColor((base.State as PlayerState).slugcatCharacter);
        }

        public void Sync(bool dead)
        {
            this.dead = dead;
        }

        public void Sync(bool corridorDrop, int corridorTurnCounter, IntVector2? corridorTurnDir, int crawlTurnDelay)
        {
            this.corridorDrop = corridorDrop;
            this.corridorTurnCounter = corridorTurnCounter;
            this.corridorTurnDir = corridorTurnDir;
            this.crawlTurnDelay = crawlTurnDelay;
        }

        public void Write(ref BinaryWriter writer)
        {
            writer.Write(corridorDrop);
            writer.Write(corridorTurnCounter);
            IntVector2Handler.Write(corridorTurnDir, ref writer);
            writer.Write(crawlTurnDelay);
        }

        public bool MapDiscoveryActive
        {
            get
            {
                if (MonklandSteamManager.isInGame)
                {
                    return base.Consious && base.abstractCreature.Room.realizedRoom != null && this.dangerGrasp == null && ((this.abstractCreature as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner == NetworkGameManager.playerID && base.mainBodyChunk != null && base.mainBodyChunk.pos.x > 0f && base.mainBodyChunk.pos.x < base.abstractCreature.Room.realizedRoom.PixelWidth && base.mainBodyChunk.pos.y > 0f && base.mainBodyChunk.pos.y < base.abstractCreature.Room.realizedRoom.PixelHeight;
                }
                else
                {
                    return base.Consious && base.abstractCreature.Room.realizedRoom != null && this.dangerGrasp == null && base.mainBodyChunk.pos.x > 0f && base.mainBodyChunk.pos.x < base.abstractCreature.Room.realizedRoom.PixelWidth && base.mainBodyChunk.pos.y > 0f && base.mainBodyChunk.pos.y < base.abstractCreature.Room.realizedRoom.PixelHeight;
                }
            }
        }

    }
}