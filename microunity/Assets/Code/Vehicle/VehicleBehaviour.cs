using UnityEngine;
using System.Collections;

public class VehicleBehaviour : MonoBehaviour
{
    public string prefabPath = "Prefabs/Car";

    private GameObject meshPrefab = null;
    private GameObject targetObject = null;


    public void Setup(Color vehicleColor)
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

        VehicleColorModifier[] modifier = targetObject.GetComponentsInChildren<VehicleColorModifier>();

        for (int i = 0; i < modifier.Length; ++i)
        {
            modifier[i].Setup(vehicleColor);
        }
    }

    public void SetState(VehicleState state)
    {
        transform.position = state.Position;
        transform.rotation = state.Rotation;
    }
}
