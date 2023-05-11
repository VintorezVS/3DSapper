public class RockCell : Cell
{
    protected override void Awake()
    {
        base.Awake();
        type = CellType.Rock;
    }

    public override void OnHit()
    {
        Destroy(gameObject);
    }
}