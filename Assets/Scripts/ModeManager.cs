using UnityEngine;

public class ModeManager : MonoBehaviour
{
    public MapPinPlacer mapPinPlacer;
    public RotateTarget rotateTarget;
    public MoveMap moveMap; // NEW
    public void EnableRotate()
    {
        rotateTarget.enabled = true;
        mapPinPlacer.enabled = false;
        if (moveMap != null) moveMap.enabled = false;
    }

    public void EnablePlacePin()
    {
        rotateTarget.enabled = false;
        mapPinPlacer.enabled = true;
        if (moveMap != null) moveMap.enabled = false;
    }

    public void EnableMove()
    {
        rotateTarget.enabled = false;
        mapPinPlacer.enabled = false;
        if (moveMap != null) moveMap.enabled = true;
    }
}
