using Monkland.Hooks;
using Monkland.UI;
using Partiality.Modloader;

namespace Monkland
{
    public class Monkland : PartialityMod
    {
        public Monkland()
        {
            this.Version = MonklandUI.VERSION;
            this.ModID = "Monkland";
        }

        // public static Monkland instance;

        public override void OnEnable()
        {
            base.OnEnable();
            RainWorldHK.Patch();
        }
    }
}