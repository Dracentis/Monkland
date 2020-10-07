namespace Monkland.Hooks.OverWorld
{
    public class AbsRoomFields
    {
        public AbsRoomFields(AbstractRoom self)
        {
            this.self = self;
        }

        public readonly AbstractRoom self;

        public int syncDelay = 20;
    }
}
