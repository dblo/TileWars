using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Cavalry : Army {
    private static List<int> attackDamageLevels = new List<int> { 5, 10, 15, 20 };
    private static List<int> defenseDamageLevels = new List<int> { 1, 2, 3, 4 };
    private static List<int> hpLevels = new List<int> { 8, 16, 24, 32 };
    private static List<float> speedLevels = new List<float> { 0.02f, 0.03f, 0.04f };
    private static List<float> rangeLevels = new List<float> { 0.6f, 0.8f, 1f, 1.25f };

    public override string GetUpgradeDescriptor()
    {
        if (UpgradeMaxed())
            return null;
        return "Cav $" + UpgradeCost(rank);
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

    protected override ReadOnlyCollection<int> GetDefenseLevels()
    {
        return defenseDamageLevels.AsReadOnly();
    }

    protected override float GetAttackMultiplier(Army enemy)
    {
        if (enemy is Artillery)
            return 1.3f;
        return 1;
    }
}
