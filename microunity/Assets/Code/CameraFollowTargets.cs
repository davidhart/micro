using UnityEngine;
using System.Collections;

public class CameraFollowTargets : MonoBehaviour
{

    public void Update()
    {
        VehicleBehaviour[] vehicles = GameObject.FindObjectsOfType<VehicleBehaviour>();

        Vector3 midPtAverage = Vector3.zero;

        for (int i = 0; i < vehicles.Length; ++i)
        {
            Transform t = vehicles[i].transform;

            if (t != null)
            {
                midPtAverage += t.position;
            }
        }

        if (vehicles.Length > 0)
        {
            midPtAverage /= (float)vehicles.Length;

            float height = transform.position.y;

            float pullBackDistance = -transform.forward.y * height;

            Vector3 newPosition = midPtAverage + transform.forward * pullBackDistance;
            newPosition.y = height;
            transform.position = newPosition;
        }
    }

}
