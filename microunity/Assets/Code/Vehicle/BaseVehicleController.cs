
public abstract class BaseVehicleController
{
    public VehicleBehaviour Target { get; set; }
    public VehicleParameters Parameters { get; set; }

    public abstract void Update(float dt);

    public abstract void HandleIncomingState(VehicleState state);
    public abstract VehicleState GetState();
}
