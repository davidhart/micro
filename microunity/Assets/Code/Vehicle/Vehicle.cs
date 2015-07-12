using UnityEngine;
using System.Collections;

public class Vehicle : MonoBehaviour
{
    public string prefabPath = "Prefabs/Car";

    private GameObject meshPrefab = null;
    private GameObject targetObject = null;
    private Transform targetTransform = null;

    public void Spawn(VehicleState state)
    {
        if (meshPrefab == null)
        {
            meshPrefab = Resources.Load(prefabPath) as GameObject;
        }

        if (targetObject != null)
        {
            GameObject.Destroy(targetObject);

            targetObject = null;
            targetTransform = null;
        }

        targetObject = GameObject.Instantiate(meshPrefab) as GameObject;
        targetTransform = targetObject.GetComponent<Transform>();

        targetTransform.position = state.Position;
        targetTransform.rotation = state.Rotation;
    }

    public void SetState(VehicleState state)
    {
        targetTransform.position = state.Position;
        targetTransform.rotation = state.Rotation;
    }

    public Transform VehicleTransform
    {
        get
        {
            return targetTransform;
        }
    }
}
