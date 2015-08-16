using UnityEngine;

[RequireComponent(typeof(Renderer))]
class VehicleColorModifier : MonoBehaviour
{
    public void Setup(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }
}
