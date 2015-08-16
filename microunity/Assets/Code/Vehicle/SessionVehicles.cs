using UnityEngine;
using System.Collections.Generic;

public class SessionVehicles : MonoBehaviour
{
	public VehicleParameters VehicleParameters;
	public VehicleBehaviour VehiclePrototype;

    private List<VehicleBehaviour> vehicles = new List<VehicleBehaviour>();
    private List<BaseVehicleController> vehicleControllers = new List<BaseVehicleController>();

    public List<VehicleBehaviour> Vehicles
    {
        get { return vehicles; }
    }

	public void Start()
	{
		VehiclePrototype.gameObject.SetActive(false);

        if (Session.Instance == null)
        {
            // Setup for testing in scene only
            SetupVehicleSlots(1);
            SpawnLocalVehicle(0);
        }
	}

    public void SetupVehicleSlots(int numSlots)
    {
        vehicles.Clear();
        vehicleControllers.Clear();

        for (int i = 0; i < numSlots; ++i)
        {
            vehicles.Add(null);
            vehicleControllers.Add(null);
        }
    }

    public void SpawnLocalVehicle(int slot)
    {
        SpawnVehicle(slot, new LocalVehicleController());
    }

    public void SpawnRemotevehicle(int slot)
    {
        SpawnVehicle(slot, new RemoteVehicleController());
    }

    private void SpawnVehicle(int slot, BaseVehicleController controller)
    {
        if (vehicles[slot] != null)
        {
            GameObject.Destroy(vehicles[slot]);
        }

        VehicleBehaviour vehicle = InstantiateVehicle(slot);
        vehicles[slot] = vehicle;

        controller.Target = vehicle;
        controller.Parameters = VehicleParameters;
        vehicleControllers[slot] = controller;
    }

    public VehicleState GetVehicleState(int slot)
    {
        return vehicleControllers[slot].GetState();
    }

    public void SetVehicleState(int slot, VehicleState state)
    {
        if (vehicleControllers[slot] == null)
            return;

        vehicleControllers[slot].HandleIncomingState(state);
    }

	public VehicleBehaviour InstantiateVehicle(int slot)
	{
		GameObject vehicleGo = GameObject.Instantiate(VehiclePrototype.gameObject) as GameObject;
		vehicleGo.SetActive(true);

        VehicleBehaviour behaviour = vehicleGo.GetComponent<VehicleBehaviour>();
        behaviour.Setup(PlayerIdentityGenerator.PlayerSlotToColor(slot));

        return behaviour;
	}

    public void Update()
    {
        for (int i = 0; i < vehicleControllers.Count; ++i)
        {
            if (vehicleControllers[i] == null)
                continue;

            vehicleControllers[i].Update(Time.deltaTime);
        }
    }
}
