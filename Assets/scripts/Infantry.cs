using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class Infantry : Army
{
    private static List<int> attackDamageLevels = new List<int> { 1, 2, 4, 8 };
    private static List<int> hpLevels = new List<int> { 4, 8, 12, 16 };
    private static List<float> speedLevels = new List<float> { 0.02f, 0.03f};
    private static List<float> rangeLevels = new List<float> { 0.5f, 0.75f, 1f, 1.25f };

    public override string GetDescriptor()
    {
        if (UpgradeMaxed())
            return "I MAX";
        return "I" + (rank + 1) + "-$" + UpgradeCost(rank + 1);
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

    internal override bool IsType(ArmyType type)
    {
        return ArmyType.Infantry == type;
    }
}
