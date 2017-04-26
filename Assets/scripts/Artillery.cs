using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.ObjectModel;

public class Artillery : Army
{
    private enum DeployStatus { NOT_STARTED, STARTED, DONE };

    public GameObject shellPrefab;
    //[SerializeField]
    //private float reloadTime;
    //[SerializeField]
    //private float shellDamageRadius;
    //[SerializeField]
    //private int bombardDamge;
    [SerializeField]
    private readonly float BOMBARD_MODE_BONUS = 0.5f;
    //private const float SHELL_LIFETIME = 0.7f;
    private const float DEPLOY_TIME = 3f;

    private static List<int> attackDamageLevels = new List<int> { 2, 4, 8, 16 };
    private static List<int> defenseDamageLevels = new List<int> { 1, 2, 3, 4 };
    private static List<int> hpLevels = new List<int> { 3, 6, 9, 12 };
    private static List<float> speedLevels = new List<float> { 0.01f, 0.02f };
    private static List<float> rangeLevels = new List<float> { 1.25f, 1.5f, 1.75f, 2f };

    DeployStatus deployStatus = DeployStatus.NOT_STARTED;

    void Update()
    {
        if (IsStationary())
        {
            if (deployStatus == DeployStatus.NOT_STARTED)
            {
                UpdateRange();
                deployStatus = DeployStatus.STARTED;
            }
            else if (deployStatus == DeployStatus.STARTED)
                UpdateRange();
        }
    }

    //private bool IsReloading()
    //{
    //    return deplomentTimer > 0;
    //}

    //private void Bombard()
    //{
    //    var shell = Instantiate(shellPrefab);
    //    var sr = shell.GetComponent<SpriteRenderer>();
    //    shell.transform.position = bombardTarget;
    //    var color = sr.color;
    //    color.a = 0.5f;
    //    sr.color = color;
    //    Destroy(shell, SHELL_LIFETIME);

    //    var colls = Physics2D.OverlapCircleAll(bombardTarget, shellDamageRadius);
    //    foreach (var coll in colls)
    //    {
    //        var army = coll.gameObject.GetComponent<Army>();
    //        if (army != null && IsEnemy(army))
    //        {
    //            var rb = army.GetComponent<Rigidbody2D>();
    //            var forceDirection = (Vector2)coll.transform.position - bombardTarget;
    //            rb.AddForce(forceDirection * 100);
    //            army.TakeDamage(bombardDamge);
    //        }
    //    }
    //}

    internal override void AttackIfAble()
    {
        base.AttackIfAble();
    }

    public override void ChangeTravelPath(List<Vector2> swipePath)
    {
        if(deployStatus != DeployStatus.NOT_STARTED)
        {
            deployStatus = DeployStatus.NOT_STARTED;
            UpdateRange();
        }
        base.ChangeTravelPath(swipePath);
    }

    private float GetMaxRange()
    {
        return CalculateRange(true);
    }

    private float GetMinRange()
    {
        // Must multiply with tileBonus since mine will reduce below base
        return rangeLevels.First() * tileCombatMods.RangeMultiplier;
    }

    protected override void UpdateRange()
    {
        if (deployStatus == DeployStatus.STARTED)
        {
            range += ((GetMaxRange() - GetMinRange()) / DEPLOY_TIME) * Time.deltaTime;
            if (range >= GetMaxRange())
            {
                range = GetMaxRange();
                deployStatus = DeployStatus.DONE;
            }
        }
        else
        {
            range = CalculateRange(deployStatus == DeployStatus.DONE);
        }
        OnRangeChanged();
    }

    private float CalculateRange(bool withBombardBonus)
    {
        float tmpRange = GetItemAtRankOrLast(GetRangeLevels());
        if (withBombardBonus)
            tmpRange += BOMBARD_MODE_BONUS;
        if(tileCombatMods != null)
            tmpRange *= tileCombatMods.RangeMultiplier;
        return tmpRange;
    }

    public override string GetUpgradeDescriptor()
    {
        if (UpgradeMaxed())
            return null;
        return "A" + (rank + 1) + "-$" + UpgradeCost(rank);
    }

    internal override bool IsType(ArmyType type)
    {
        return ArmyType.Artillery == type;
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
        if (enemy is Infantry)
            return 1.3f;
        return 1;
    }
}
