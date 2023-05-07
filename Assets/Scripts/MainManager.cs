using UnityEngine;

public class MainManager : MonoBehaviour
{
    [SerializeField] private GameObject fieldObject;
    [SerializeField] private GameObject player;
    private Field field;

    void Start()
    {
        field = fieldObject.GetComponent<Field>();
        field.PlayerPosition = player.transform.position;
        field.Generate();
    }
}
