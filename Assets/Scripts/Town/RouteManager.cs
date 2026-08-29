using UnityEngine;

public class RouteManager : MonoBehaviour
{
    public static RouteManager Instance;

    public bool helpedCar = false;

    private void Awake()
    {
        Instance = this;
    }

    public void SetHelpedCar()
    {
        helpedCar = true;
    }
}