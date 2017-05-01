using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

public class Infantry : Army
{
    private static List<int> attackDamageLevels = new List<int> { 3, 6, 9, 12 };
    private static List<int> defenseDamageLevels = new List<int> { 1, 2, 3, 4 };
    private static List<int> hpLevels = new List<int> { 10, 20, 30, 40 };
    private static List<float> speedLevels = new List<float> { 0.01f, 0.02f, 0.03f};
    private static List<float> rangeLevels = new List<float> { 0.75f, 1f, 1.25f, 1.55f };

    public override string GetUpgradeDescriptor()
    {
        if (UpgradeMaxed())
            return null;
        return "Inf $" + UpgradeCost(rank);
    }

    internal override bool IsType(ArmyType type)
    {
        return ArmyType.Infantry == type;
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
        if (enemy is Cavalry)
            return 1.3f;
        return 1;
    }
}
