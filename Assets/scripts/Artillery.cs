using UnityEngine;
using System.Collections.Generic;
using System;

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

    private const float MIN_SWIPE_TIME = 0.5f;
    private const float SHELL_LIFETIME = 0.7f;

    private Vector2 invalidPos = new Vector2(-1, -1);
    private Vector2 bombardTarget;
    private float swipeStartTime = -1;
    private float remainingReloadTime;
    private bool acceptNewPath;
    private bool inBombardMode;

    protected override void Awake()
    {
        base.Awake();
        bombardTarget = invalidPos;
    }

    void Update()
    {
        if (IsReloading())
        {
            remainingReloadTime -= Time.deltaTime;
        }
        else if (IsBombaring())
        {
            Bombard();
            remainingReloadTime = reloadTime;
        }
    }

    private bool IsBombaring()
    {
        return inBombardMode && bombardTarget != invalidPos;
    }

    private bool IsReloading()
    {
        return remainingReloadTime > 0;
    }

    private bool DidLongGesture()
    {
        return swipeStartTime >= 0 && swipeStartTime + MIN_SWIPE_TIME <= Time.time;
    }

    private void Bombard()
    {
        var shell = Instantiate(shellPrefab);
        var sr = shell.GetComponent<SpriteRenderer>();
        shell.transform.position = bombardTarget;
        var color = sr.color;
        color.a = 0.5f;
        sr.color = color;
        Destroy(shell, SHELL_LIFETIME);

        var colls = Physics2D.OverlapCircleAll(bombardTarget, shellDamageRadius);
        foreach (var coll in colls)
        {
            var army = coll.gameObject.GetComponent<Army>();
            if (army != null && IsEnemy(army))
            {
                var rb = army.GetComponent<Rigidbody2D>();
                var forceDirection = (Vector2)coll.transform.position - bombardTarget;
                rb.AddForce(forceDirection * 100);
                army.TakeDamage(bombardDamge);
            }
        }
    }

    protected override void Attack(Army enemy)
    {
        // While bombarding, targets in range but not in bombardment area are ignored.
        if (!inBombardMode)
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

    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        acceptNewPath = false;
        swipeStartTime = Time.time;
    }

    protected void OnMouseUp()
    {
        if (inBombardMode)
        {
            bombardTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            swipeStartTime = -1;
            return;
        }
        if (DidLongGesture())
        {
            if (IsStationary())
                SetBombardMode(true);
        }
        swipeStartTime = -1;
    }
    
    private void OnMouseExit()
    {
        acceptNewPath = true;
    }

    public override void ChangeTravelPath(List<Vector2> swipePath)
    {
        if (acceptNewPath)
        {
            base.ChangeTravelPath(swipePath);
            SetBombardMode(false);
            bombardTarget = invalidPos;
        }
    }
}
