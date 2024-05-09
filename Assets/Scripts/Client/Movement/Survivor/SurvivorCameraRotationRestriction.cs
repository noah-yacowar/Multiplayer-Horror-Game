using UnityEngine;

public class SurvivorCameraRotationRestriction : MonoBehaviour
{
    public float restrictionAngle = -50f;

    void Update()
    {
        // Get current rotation
        Vector3 rotation = transform.localEulerAngles;

        float rotationToCompare = rotation.x;

        if(rotationToCompare > 270)
        {
            rotationToCompare -= 360;
        }

        // Make sure camera can't clip into player
        if (rotationToCompare < restrictionAngle)
        {
            // Set x rotation to restriction angle
            rotation.x = restrictionAngle;
            // Apply the updated rotation
            transform.localEulerAngles = rotation;
        }
    }
}