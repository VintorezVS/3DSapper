using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector3 Coordinates { get; set; }
    [SerializeField] private bool isExplosive;
    public bool IsExplosive
    {
        get => isExplosive;
        set => isExplosive = value;
    }
}