using Monkland.SteamManagement;
using Steamworks;

namespace Monkland.Hooks.Entities
{
    public class AbstractObjFields
    {
        public AbstractObjFields(AbstractPhysicalObject self)
        {
            this.self = self;

            playerdist = UnityEngine.Random.Range(1, int.MaxValue);
            if (MonklandSteamManager.isInGame)
            {
                ownerID = NetworkGameManager.playerID;
            }
        }

        /// <summary>
        /// ID in the network.
        /// </summary>
        public int networkID => self.ID.number == 0 ? playerdist : self.ID.number;

        public string ownerName => SteamFriends.GetFriendPersonaName((CSteamID)ownerID) == string.Empty ? "Player" : SteamFriends.GetFriendPersonaName((CSteamID)ownerID);


        /// <summary>
        /// Returns if the object is local or network.
        /// </summary>
        public bool isNetworkObject => (MonklandSteamManager.isInGame && ownerID != NetworkGameManager.playerID);


        public readonly AbstractPhysicalObject self;
        public readonly int playerdist;
        public ulong ownerID = 0;

        public int networkLife = 100;
    }
}
