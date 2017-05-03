using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

public class Infantry : Army
{
    private static List<int> attackDamageLevels = new List<int> { 2, 4, 6, 8 };
    private static List<int> defenseDamageLevels = new List<int> { 1, 2, 3, 4 };
    private static List<int> hpLevels = new List<int> { 50, 10, 150, 200 };
    private static List<float> speedLevels = new List<float> { 0.01f, 0.013f, 0.016f, 0.019f };
    private static List<float> rangeLevels = new List<float> { 0.6f, 0.8f, 1f, 1.2f };

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
