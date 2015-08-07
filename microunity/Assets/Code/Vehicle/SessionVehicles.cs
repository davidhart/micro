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
            VehicleBehaviour vehicle = InstantiateVehicle();
            vehicles.Add(vehicle);

            LocalVehicleController vehicleController = new LocalVehicleController();
            vehicleController.Target = vehicle;
            vehicleController.Parameters = VehicleParameters;
            vehicleControllers.Add(vehicleController);

            return;
        }

		RemotePlayerSet players = Session.Instance.Players;

		for (int i = 0; i < players.SlotsCount; ++i)
		{
			RemotePlayer player = players.GetPlayerInSlot(i);

            if (player == null)
            {
                vehicles.Add(null);
                vehicleControllers.Add(null);

                continue; // empty slot;
            }

            VehicleBehaviour vehicle = InstantiateVehicle();
            vehicles.Add(vehicle);

            BaseVehicleController playerController = null;

            if (player == Session.Instance.LocalPlayer)
            {
                playerController = new LocalVehicleController();
            }
            else
            {
                playerController = new RemoteVehicleController();
            }

            playerController.Target = vehicle;
            playerController.Parameters = VehicleParameters;
            vehicleControllers.Add(playerController);
		}
	}

	public VehicleBehaviour InstantiateVehicle()
	{
		GameObject vehicleGo = GameObject.Instantiate(VehiclePrototype.gameObject) as GameObject;
		vehicleGo.SetActive(true);

        VehicleBehaviour behaviour = vehicleGo.GetComponent<VehicleBehaviour>();
        behaviour.Setup();

        return behaviour;
	}

    public void Update()
    {
        for (int i = 0; i < vehicleControllers.Count; ++i)
        {
            if (vehicleControllers[i] == null)
                continue;

            vehicleControllers[i].Update();
        }
    }
}
