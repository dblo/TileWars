using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Cavalry : Army
{
    private static List<int> attackDamageLevels = new List<int> { 8,10,12,14 };
    private static List<int> defenseDamageLevels = new List<int> { 0,0,0,0 };
    private static List<int> hpLevels = new List<int> { 30, 40, 50, 60 };
    private static List<float> speedLevels = new List<float> { 0.015f, 0.02f, 0.025f, 0.03f };
    private static List<float> rangeLevels = new List<float> { 0.5f, 0.55f, 0.6f, 0.65f };

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
            return 2;
        return 1;
    }
}
