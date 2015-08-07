using UnityEngine;
using System.Collections.Generic;

public class SessionVehicles : MonoBehaviour
{
	public VehicleParameters VehicleParameters;
	public VehicleBehaviour VehiclePrototype;

    private List<VehicleBehaviour> Vehicles = new List<VehicleBehaviour>();
    private List<BaseVehicleController> VehicleControllers = new List<BaseVehicleController>();

	public void Start()
	{
		VehiclePrototype.gameObject.SetActive(false);

        if (Session.Instance == null)
        {
            // Setup for testing in scene only
            VehicleBehaviour vehicle = InstantiateVehicle();
            Vehicles.Add(vehicle);

            LocalVehicleController vehicleController = new LocalVehicleController();
            vehicleController.Target = vehicle;
            vehicleController.Parameters = VehicleParameters;
            VehicleControllers.Add(vehicleController);

            return;
        }

		RemotePlayerSet players = Session.Instance.Players;

		for (int i = 0; i < players.SlotsCount; ++i)
		{
			RemotePlayer player = players.GetPlayerInSlot(i);

            if (player == null)
            {
                Vehicles.Add(null);
                VehicleControllers.Add(null);

                continue; // empty slot;
            }

            VehicleBehaviour vehicle = InstantiateVehicle();
            Vehicles.Add(vehicle);

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
            VehicleControllers.Add(playerController);
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
        for (int i = 0; i < VehicleControllers.Count; ++i)
        {
            if (VehicleControllers[i] == null)
                continue;

            VehicleControllers[i].Update();
        }
    }
}
