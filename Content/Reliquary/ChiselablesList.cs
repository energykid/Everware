using System.Collections.Generic;

namespace Everware.Content.Reliquary;

public class ChiselablesList : ModSystem
{
    public static List<Chiselable> AllChiselables = [];
    public override void Load()
    {

    }
    public override void Unload()
    {
        AllChiselables.Clear();
    }
}
public class Chiselable
{
    public int BaseStatue;
    public int UpgradeMaterial;
    public int UpgradeStack = 1;
    public int UpgradedStatue;
    public Chiselable(int Base, int Upgrade, int Material, int Stack)
    {
        BaseStatue = Base;
        UpgradeMaterial = Material;
        UpgradeStack = Stack;
        UpgradedStatue = Upgrade;
    }
    public Chiselable(int Base, int Upgrade)
    {
        BaseStatue = Base;
        UpgradedStatue = Upgrade;
    }
}
