using UnityEngine;

public class RotateItem : MonoBehaviour
{
    public float rotateSpeed = 50f;

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}