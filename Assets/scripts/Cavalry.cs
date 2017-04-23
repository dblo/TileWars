using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Cavalry : Army {
    private static List<int> attackDamageLevels = new List<int> { 1, 2, 4, 8 };
    private static List<int> hpLevels = new List<int> { 4, 8, 12, 16 };
    private static List<float> speedLevels = new List<float> { 0.04f, 0.05f, 0.06f };
    private static List<float> rangeLevels = new List<float> { 0.6f, 0.8f, 1f, 1.25f };

    public override string GetUpgradeDescriptor()
    {
        if (UpgradeMaxed())
            return "C MAX";
        return "C" + (rank + 1) + "-$" + UpgradeCost(rank);
    }

    internal override bool IsType(ArmyType type)
    {
        return ArmyType.Cavalry == type;
    }

    protected override ReadOnlyCollection<int> GetAttackDamageLevels()
    {
        return attackDamageLevels.AsReadOnly();
    }

    protected override ReadOnlyCollection<int> GetHPLevels()
    {
        return hpLevels.AsReadOnly();
    }

    protected override ReadOnlyCollection<float> GetSpeedLevels()
    {
        return speedLevels.AsReadOnly();
    }

    protected override ReadOnlyCollection<float> GetRangeLevels()
    {
        return rangeLevels.AsReadOnly();
    }
}
