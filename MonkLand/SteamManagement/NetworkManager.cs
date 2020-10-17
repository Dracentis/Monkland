using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkManager
    {

        public const int GAME_CHANNEL = 0;
        public const int WORLD_CHANNEL = 1;
        public const int ENTITY_CHANNEL = 2;
        public const int GRASPSTICK_CHANNEL = 3;

        /*
        public bool isSynced(PhysicalObject obj)
        {
            if (obj == null)
                return false;
            if (obj is Player)
                return true;
            if (obj is Rock)
                return true;
            if (obj is Spear)
                return true;
            return false;
        }*/

        public bool isSynced(AbstractPhysicalObject obj)
        {
            if (obj == null)
            { return false; }
            if (obj is AbstractCreature && (obj as AbstractCreature).creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Slugcat)
            { return true; }
            if (obj.type == AbstractPhysicalObject.AbstractObjectType.Rock)
            { return true; }
            if (obj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            { return true; }

            return false;
        }


        public virtual void Reset()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void RegisterHandlers()
        {
        }

        public virtual void PlayerJoined(ulong steamID)
        {
        }

        public virtual void PlayerLeft(ulong steamID)
        {
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }
    }
}