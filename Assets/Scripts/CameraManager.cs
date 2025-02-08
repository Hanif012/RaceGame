using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera FirstCarCam;
    public CinemachineCamera PlayerCarCam;
    public CinemachineCamera POVCam;
    public CinemachineCamera MenuCam;

    private void Start()
    {
        SwitchToCamera(MenuCam);
    }

    public void SwitchToCamera(CinemachineCamera targetCamera)
    {
        FirstCarCam.Priority = 0;
        PlayerCarCam.Priority = 0;
        POVCam.Priority = 0;

        targetCamera.Priority = 10;
    }

    public IEnumerator PanToFirstCar(float delay = 3f)
    {
        var firstCar = RankingManager.Instance.FirstCar;
        if (firstCar == null)
        {
            Debug.LogError("First car not found in RankingManager!");
            yield break;
        }

        FirstCarCam.Follow = firstCar.transform;

        SwitchToCamera(FirstCarCam);
        Debug.Log("Switched to First Car Camera");
        yield return new WaitForSeconds(delay);
    }

    public IEnumerator PanToPlayerCar(float delay = 3f)
    {
        SwitchToCamera(PlayerCarCam);
        Debug.Log("Switched to Player Car Camera");
        yield return new WaitForSeconds(delay);
    }

    public IEnumerator SwitchToPOV(float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);
        SwitchToCamera(POVCam);
        Debug.Log("Switched to POV Camera");
    }

    public IEnumerator SwitchToMenuCam(float delay = 3f)
    {
        yield return new WaitForSeconds(delay);
        SwitchToCamera(MenuCam);
        Debug.Log("Switched to Menu Camera");
    }
}
