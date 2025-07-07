using UnityEngine;

public class SceneCamera : MonoBehaviour
{
    public void Setup(Vector3 position, float orthographicSize)
    {
        var cameraComponent = GetComponent<Camera>();
        cameraComponent.transform.position = position;
        cameraComponent.orthographicSize = orthographicSize;
    }
}
