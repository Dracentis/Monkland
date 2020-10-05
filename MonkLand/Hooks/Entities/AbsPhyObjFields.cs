using Monkland.SteamManagement;

namespace Monkland.Hooks.Entities
{
    public class AbsPhyObjFields
    {
        public AbsPhyObjFields(AbstractPhysicalObject self)
        {
            this.self = self;

            playerdist = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            if (MonklandSteamManager.isInGame)
            { 
                owner = NetworkGameManager.playerID;
            }
        }

        public int dist => self.ID.number == 0 ? playerdist : self.ID.number;
        public bool networkObject => (MonklandSteamManager.isInGame && owner != NetworkGameManager.playerID);

        public readonly AbstractPhysicalObject self;
        public readonly int playerdist;
        public ulong owner = 0;

        public int networkLife = 60;
    }
}