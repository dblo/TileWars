using UnityEngine;
using System.Collections.Generic;

public class Artillery : Army
{
    public GameObject shellPrefab;
    [SerializeField]
    private float reloadTime;
    [SerializeField]
    private float shellDamageRadius;
    [SerializeField]
    private int bombardDamge;
    [SerializeField]
    private float bombardRange;

    private const float SHELL_LIFETIME = 0.7f;

    private float remainingReloadTime;
    private bool inBombardMode;

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (IsReloading())
        {
            remainingReloadTime -= Time.deltaTime;
        }
        else if (inBombardMode)
        {
            //Bombard();
            remainingReloadTime = reloadTime;
        }
    }

    private bool IsReloading()
    {
        return remainingReloadTime > 0;
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
        // While bombarding, targets in range but not in bombardment area are ignored.
        //if (!inBombardMode)
        {
            base.Attack(enemy);
        }
    }

    private void SetBombardMode(bool val)
    {
        inBombardMode = val;

        if (inBombardMode)
            OnRangeChanged(bombardRange);
        else
            OnRangeChanged(range);
    }

    public override void ChangeTravelPath(List<Vector2> swipePath)
    {
        SetBombardMode(false);
        base.ChangeTravelPath(swipePath);
    }
}
