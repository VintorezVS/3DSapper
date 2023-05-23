using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private int minZoom = 2;
    [SerializeField] private int maxZoom = 10;
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        UpdatePosition();
        mainCamera = transform.GetChild(0).GetComponent<Camera>();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameInProgress) return;

        float zoom = Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel") * 10, -1, 1);
        if (zoom != 0)
        {
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - zoom, minZoom, maxZoom);
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
    }
}
