
public class RemoteVehicleController : BaseVehicleController
{
    VehicleState state;

    public override void Update(float dt)
    {
        state.Update(dt, Parameters);
    }

    public override void HandleIncomingState(VehicleState state)
    {
        this.state = state;

        Target.SetState(state);
    }

    public override VehicleState GetState()
    {
        return state;
    }
}
