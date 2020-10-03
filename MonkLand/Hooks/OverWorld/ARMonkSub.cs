namespace Monkland.Hooks.OverWorld
{
    public class ARMonkSub
    {
        public ARMonkSub(AbstractRoom self)
        {
            this.self = self;
        }

        public readonly AbstractRoom self;

        public int syncDelay = 20;
    }
}