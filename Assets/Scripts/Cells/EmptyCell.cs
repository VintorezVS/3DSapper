using UnityEngine;
using TMPro;

// INHERITANCE
public class EmptyCell : Cell
{
    [SerializeField] private TextMeshProUGUI textObject;

    public string Text
    {
        get => textObject.text;
        set => textObject.text = value;
    }

    // POLYMORPHISM
    protected override void Awake()
    {
        type = CellType.Empty;
    }

    public void SetNearExplosiveCount(int explosives)
    {
        textObject.text = explosives.ToString();
    }
}