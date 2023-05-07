using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float changeProjectionSpeed = 300;
    private bool isChangingProjection = false;

    public void ChangeProjectionToLeft()
    {
        ChangeProjection(Projection.Left);
    }

    public void ChangeProjectionToRight()
    {
        ChangeProjection(Projection.Right);
    }

    private void ChangeProjection(Projection projection)
    {
        if (isChangingProjection) return;
        isChangingProjection = true;
        StartCoroutine(RotateY((transform.rotation.eulerAngles.y + (int)projection)));
    }

    private IEnumerator RotateY(float targetAngle)
    {
        while (Mathf.Abs(transform.rotation.eulerAngles.y - targetAngle) % 360 > 5)
        {
            Debug.Log(Mathf.Abs(transform.rotation.eulerAngles.y - targetAngle));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, targetAngle, 0f), changeProjectionSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        isChangingProjection = false;
    }
}
