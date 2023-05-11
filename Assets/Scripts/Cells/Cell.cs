using UnityEngine;

public abstract class Cell : MonoBehaviour
{
    protected CellType type;
    protected GameObject flag;
    public bool IsMarked { get; protected set; }
    public bool IsExplosive
    {
        get => type == CellType.Explosive;
    }
    public bool IsEmpty
    {
        get => type == CellType.Empty;
    }

    protected virtual void Awake()
    {
        flag = transform.Find("Flag")?.gameObject;
    }

    public virtual void OnHit()
    {
        return;
    }

    public virtual void ToggleMark()
    {
        if (IsEmpty) return;
        IsMarked = !IsMarked;
        flag.SetActive(IsMarked);
    }
}