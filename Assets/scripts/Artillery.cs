using UnityEngine;
using System.Collections.Generic;
using System;

public class Artillery : Army
{
    public GameObject shellPrefab;
    [SerializeField]
    private float reloadTime = 2;
    [SerializeField]
    private float bombardDamageRadius;
    [SerializeField]
    private int bombardDamge;

    private const float MIN_SWIPE_TIME = 0.5f;
    private float swipeStartTime = -1;
    private Transform bombardDisplay;
    private Vector2 invalidPos = new Vector2(-1, -1);
    private Vector2 bombardTarget;
    private float remainingReloadTime;
    private bool acceptNewPath;
    
    protected override void Awake()
    {
        base.Awake();
        foreach (Transform trans in transform)
        {
            if (trans.name == "BombardDisplay")
            {
                bombardDisplay = trans;
                break;
            }
        }
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

    private bool IsReloading()
    {
        return remainingReloadTime > 0;
    }

    private void Bombard()
    {
        var shell = Instantiate(shellPrefab);
        var sr = shell.GetComponent<SpriteRenderer>();
        shell.transform.position = bombardTarget;
        var color = sr.color;
        color.a = 0.5f;
        sr.color = color;
        Destroy(shell, 1.5f);

        var colls = Physics2D.OverlapCircleAll(bombardTarget, bombardDamageRadius);
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

    protected override void OnRangeChanged()
    {
        base.OnRangeChanged();
        bombardDisplay.localScale = new Vector3(range * 1.5f, range * 1.5f);
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        acceptNewPath = false;
        swipeStartTime = Time.time;
    }

    protected override void Attack(Army enemy)
    {
        // While bombarding, targets in range but not in bombardment area are ignored.
        if (!IsBombaring())
        {
            base.Attack(enemy);
        }
    }

    private bool IsBombaring()
    {
        return bombardTarget != invalidPos;
    }

    protected void OnMouseUp()
    {
        if (SelectingBombardTarget())
        {
            bombardTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            remainingReloadTime = 0;
            SetShowBombardDisplay(false);
            swipeStartTime = -1;
            return;
        }
        if (swipeStartTime  >= 0 && swipeStartTime + MIN_SWIPE_TIME <= Time.time)
        {
            if(IsStationary())
                SetShowBombardDisplay(true);
        }
        swipeStartTime = -1;
    }
    
    private void OnMouseExit()
    {
        acceptNewPath = true;
    }

    private bool SelectingBombardTarget()
    {
        return bombardDisplay.gameObject.activeSelf && bombardTarget == invalidPos;
    }

    public override void GiveNewPath(List<Vector2> swipePath)
    {
        if(acceptNewPath)
        {
            base.GiveNewPath(swipePath);
            SetShowBombardDisplay(false);
            bombardTarget = invalidPos;
        }
    }

    public void SetShowBombardDisplay(bool val)
    {
        bombardDisplay.gameObject.SetActive(val);
    }

    public override void SetShowRangeDisplay(bool val)
    {
        base.SetShowRangeDisplay(val);
        if(!val)
            SetShowBombardDisplay(false);
    }
}
