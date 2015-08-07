using UnityEngine;
using System.Collections;

public class VehicleBehaviour : MonoBehaviour
{
    public string prefabPath = "Prefabs/Car";

    private GameObject meshPrefab = null;
    private GameObject targetObject = null;

    public void Setup()
    {
        if (meshPrefab == null)
        {
            meshPrefab = Resources.Load(prefabPath) as GameObject;
        }

        if (targetObject != null)
        {
            GameObject.Destroy(targetObject);
            targetObject = null;
        }

        targetObject = GameObject.Instantiate(meshPrefab) as GameObject;

        Transform targetTransform = targetObject.GetComponent<Transform>();
        targetTransform.parent = transform;
        targetTransform.localPosition = Vector3.zero;
        targetTransform.localRotation = Quaternion.identity;
        targetTransform.localScale = Vector3.one;
    }

    public void SetState(VehicleState state)
    {
        transform.position = state.Position;
        transform.rotation = state.Rotation;
    }
}
