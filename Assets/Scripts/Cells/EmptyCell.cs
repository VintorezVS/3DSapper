using UnityEngine;
using TMPro;

public class EmptyCell : Cell
{
    [SerializeField] private TextMeshProUGUI textObject;

    public string Text
    {
        get => textObject.text;
        set => textObject.text = value;
    }

    protected override void Awake()
    {
        type = CellType.Empty;
    }

    public void SetNearExplosiveCount(int explosives)
    {
        textObject.text = explosives.ToString();
    }
}