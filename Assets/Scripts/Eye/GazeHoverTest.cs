using UnityEngine;

public class GazeHoverTest : MonoBehaviour
{
    public void GazeEnter()
    {
        Debug.Log("眼睛正在看 Cube");
    }

    public void GazeExit()
    {
        Debug.Log("眼睛离开 Cube");
    }
}