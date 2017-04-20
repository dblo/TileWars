using UnityEngine;
using System.Collections.Generic;
using System;

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
    private float bombardRange;
    private float deplomentTimer = 0;

    private const float SHELL_LIFETIME = 0.7f;
    private const float DEPLOY_TIME = 4f;
    private bool inBombardMode;

    protected override void Awake()
    {
        base.Awake();
    }

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
        OnRangeChanged();
    }

    public override void ChangeTravelPath(List<Vector2> swipePath)
    {
        SetBombardMode(false);
        deplomentTimer = DEPLOY_TIME;
        base.ChangeTravelPath(swipePath);
    }

    protected override float GetEffectiveRange()
    {
        float eRange;
        eRange = inBombardMode ? bombardRange : range;
        eRange = inHill ? eRange * 1.5f : eRange;
        return eRange;
    }
}
