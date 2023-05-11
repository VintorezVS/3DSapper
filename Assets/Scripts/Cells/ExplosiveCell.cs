using UnityEngine;

public class ExplosiveCell : Cell
{
    private GameObject dynamite;

    protected override void Awake()
    {
        base.Awake();
        type = CellType.Explosive;
        dynamite = transform.Find("Dynamite").gameObject;
    }

    public override void OnHit()
    {
        dynamite.SetActive(true);
        GameManager.Instance.GameOver();
    }
}