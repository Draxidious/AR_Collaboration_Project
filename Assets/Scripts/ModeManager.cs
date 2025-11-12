using UnityEngine;

public class ModeManager : MonoBehaviour
{

    public MapPinPlacer mapPinPlacer;
    public RotateTarget rotateTarget;
    public void EnableRotate()
    {
        rotateTarget.enabled = true;
        mapPinPlacer.enabled = false;
    }
    public void EnablePlacePin()
    {
        rotateTarget.enabled = false;
        mapPinPlacer.enabled = true;
    }
}
