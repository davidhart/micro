using UnityEngine;

class CarGameModeSetup : MonoBehaviour
{
    public SessionVehicles vehicles = null;

    private Gamemodes.Cars.CarsClientGameMode gamemode;

    public void Awake()
    {
        // Testing in scene
        if (Session.Instance == null)
            return;

        gamemode = new Gamemodes.Cars.CarsClientGameMode(vehicles);

        Session.Instance.AttachClientGameMode(gamemode);
    }
}
