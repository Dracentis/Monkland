namespace Monkland.Hooks.OverWorld
{
    public class ARMonkFields
    {
        public ARMonkFields(AbstractRoom self)
        {
            this.self = self;
        }

        public readonly AbstractRoom self;

        public int syncDelay = 20;
    }
}