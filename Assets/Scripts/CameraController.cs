using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        UpdatePosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
    }
}
