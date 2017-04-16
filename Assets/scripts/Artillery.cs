using UnityEngine;
using System.Collections.Generic;

public class Artillery : Army
{
    private const float MIN_SWIPE_TIME = 0.5f;
    private float swipeStartTime = -1;
    private Transform bombardDisplay;
    private Vector2 invalidPos = new Vector2(-1, -1);
    private Vector2 bombardTarget;
    [SerializeField]
    private float remainingReloadTime;
    [SerializeField]
    private float bombardDamageRadius;
    private float reloadTime = 1;
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
        if (remainingReloadTime > 0)
        {
            remainingReloadTime -= Time.deltaTime;
        }
        else if (bombardTarget != invalidPos)
        {
            Bombard();
            remainingReloadTime = reloadTime;
        }
    }

    private void Bombard()
    {
        var colls = Physics2D.OverlapCircleAll(bombardTarget, bombardDamageRadius);
        foreach (var coll in colls)
        {
            var enemy = coll.gameObject.GetComponent<Army>();
            if (enemy != null && enemy.GetTeam() != team)
            {
                enemy.TakeDamage(this);
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

    protected void OnMouseUp()
    {
        if (SelectingBombardTarget())
        {
            bombardTarget = Input.mousePosition;
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
