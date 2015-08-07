using UnityEngine;
using System.Collections.Generic;

public class VehicleFollowCamera : MonoBehaviour
{
    public SessionVehicles targetVehicles;

    public void Update()
    {
        Vector3 midPtAverage = Vector3.zero;

        List<VehicleBehaviour> vehicles = targetVehicles.Vehicles;
        int numActiveVehicles = 0;

        for (int i = 0; i < vehicles.Count; ++i)
        {
            if (vehicles[i] == null)
                continue;

            Transform t = vehicles[i].transform;
            numActiveVehicles++;

            if (t != null)
            {
                midPtAverage += t.position;
            }
        }

        if (numActiveVehicles > 0)
        {
            midPtAverage /= (float)numActiveVehicles;

            float height = transform.position.y;

            float pullBackDistance = -transform.forward.y * height;

            Vector3 newPosition = midPtAverage + transform.forward * pullBackDistance;
            newPosition.y = height;
            transform.position = newPosition;
        }
    }

}
