using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.ObjectModel;

public class Artillery : Army
{
    public GameObject shellPrefab;
    //[SerializeField]
    //private float reloadTime;
    //[SerializeField]
    //private float shellDamageRadius;
    //[SerializeField]
    //private int bombardDamge;
    [SerializeField]
    private readonly float BOMBARD_MODE_BONUS = 0.5f;
    private float deplomentTimer = 0;
    private bool inBombardMode;
    //private const float SHELL_LIFETIME = 0.7f;
    private const float DEPLOY_TIME = 3f;

    private static List<int> attackDamageLevels = new List<int> { 2, 4, 8, 16 };
    private static List<int> defenseDamageLevels = new List<int> { 1, 2, 3, 4 };
    private static List<int> hpLevels = new List<int> { 3, 6, 9, 12 };
    private static List<float> speedLevels = new List<float> { 0.01f, 0.02f };
    private static List<float> rangeLevels = new List<float> { 1.25f, 1.5f, 1.75f, 2f };

    void Update()
    {
        if (Deploying())
        {
            deplomentTimer -= Time.deltaTime;
            if (deplomentTimer <= 0 && IsStationary())
            {
                SetBombardMode(true);
            }
        }
    }

    private bool Deploying()
    {
        return deplomentTimer > 0 && IsStationary();
    }

    private bool IsReloading()
    {
        return deplomentTimer > 0;
    }

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

    protected override void Attack(Army enemy)
    {
        if(!Deploying())
            base.Attack(enemy);
    }

    private void SetBombardMode(bool val)
    {
        inBombardMode = val;
        UpdateRange();
    }

    public override void ChangeTravelPath(List<Vector2> swipePath)
    {
        SetBombardMode(false);
        deplomentTimer = DEPLOY_TIME;
        base.ChangeTravelPath(swipePath);
    }

    protected override void UpdateRange()
    {
        float tmpRange = GetItemAtRankOrLast(GetRangeLevels());
        if (inBombardMode)
            tmpRange += BOMBARD_MODE_BONUS;
        tmpRange *= tileCombatMods.RangeMultiplier;
        range = tmpRange;
        OnRangeChanged();
    }

    public override string GetUpgradeDescriptor()
    {
        if (UpgradeMaxed())
            return "A MAX";
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
}
