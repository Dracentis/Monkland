using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
public class Monkland : PartialityMod
{
    public Monkland()
    {
        instance = this;
        this.ModID = "Monkland";
        this.Version = "0.4.0";
        this.author = "Dracentis, Garrakx, the1whoscreamsiguess, notfood";
    }
    public static Monkland instance; // for future Config Machine support
    public override void OnEnable()
    {
        base.OnEnable();
        // Hooking codes would go here
    }
}