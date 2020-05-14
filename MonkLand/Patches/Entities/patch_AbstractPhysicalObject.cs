using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using Monkland.SteamManagement;
using UnityEngine;

namespace Monkland.Patches
{
    [MonoModPatch("global::AbstractPhysicalObject")]
    class patch_AbstractPhysicalObject : AbstractPhysicalObject
    {
        [MonoModIgnore]
        public patch_AbstractPhysicalObject(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID) : base(world, type, realizedObject, pos, ID)
        {
        }

        public int dist = 0;
        public ulong owner = 0;

        public bool networkObject
        {
            get
            {
                return (MonklandSteamManager.isInGame && owner != NetworkGameManager.playerID);
            }
        }

        [MonoModIgnore]
        public extern void OriginalConstructor(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID);
        [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
        public void ctor_AbstractPhysicalObject(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
        {
            OriginalConstructor(world, type, realizedObject, pos, ID);
            if (MonklandSteamManager.isInGame)
            {
                owner = NetworkGameManager.playerID;
                dist = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
        }
    }
}
