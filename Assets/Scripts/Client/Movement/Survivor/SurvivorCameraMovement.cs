using UnityEngine;

public class SurvivorCameraMovement : MonoBehaviour
{
    // GameObject that serves as the main character or whatever the camera should follow
    public GameObject character;
    /*
	An Empty GameObject, that serves as a parent to the camera, 
	and it's rotation will rotate the camera as we want since the camera is a child of it.
	*/
    public GameObject cameraCenter;
    /*
	this variable is a bonus height to the CameraCenter, because my character had 
	its origin at its feet, which is not where i want the center of camera rotation to be.
	*/
    public float yOffset;
    // Sensitivity = speed of rotation of camera (bigger it is, more sensitivethe camera is to input)      
    public float sensitivity;
    // The Camera (child of CameraCenter)
    public Camera cam;

    private RaycastHit _camHit;
    // This one is public but no need to input any values for it
    public Vector3 camDist;
    public float scrollSensitivity = 2f;
    public float scrollDampening = 6f;
    public float zoomMin = 3.5f;
    public float zoomMax = 15f;
    public float zoomDefault = 10f;
    public float zoomDistance;
    public float collisionSensitivity = 2.5f;

    // Start is called before the first frame update
    void Start()
    {
        /*
		= The initial local position of the camera (relative to the CameraCenter),
		*/
        camDist = cam.transform.localPosition;

        // Set default zoom value
        zoomDistance = zoomDefault;

        // Apply default zoom
        camDist.z = zoomDistance;
    }

    // Update is called once per frame
    void Update()
    {
        // The CameraCenter (empty gameobject) follows always the character's position:
        var position1 = character.transform.position;
        cameraCenter.transform.position = new Vector3(position1.x, position1.y + yOffset, position1.z);

        // Rotation of CameraCenter, and thus the camera, depending on Mouse Input:
        var rotation = cameraCenter.transform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * sensitivity / 2,
            rotation.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity, rotation.eulerAngles.z);
        cameraCenter.transform.rotation = rotation;


        // Zooming Input from our Mouse Scroll Wheel
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            var scrollAmount = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
            scrollAmount *= (zoomDistance * 0.3f);
            zoomDistance += scrollAmount * -1f;
            zoomDistance = Mathf.Clamp(zoomDistance, zoomMin, zoomMax);
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (camDist.z != zoomDistance * -1f)
        {
            camDist.z = Mathf.Lerp(camDist.z, -zoomDistance, Time.deltaTime * scrollDampening);
        }


        // Apply calculated camera position
        var transform2 = cam.transform;
        transform2.localPosition = camDist;

        // Check and handle Collision
        GameObject obj = new GameObject();
        obj.transform.SetParent(transform2.parent);
        var position = cam.transform.localPosition;
        obj.transform.localPosition = new Vector3(position.x, position.y, position.z - collisionSensitivity);
        /*
		Linecast is an alternative to Raycast, using it to cast a ray between the CameraCenter 
		and a point directly behind the camera (to smooth things, that's why there's an "obj"
		GameObject, that is directly behind cam)
		*/
        if (Physics.Linecast(cameraCenter.transform.position, obj.transform.position, out _camHit))
        {
            //This gets executed if there's any collider in the way
            var transform1 = cam.transform;
            transform1.position = _camHit.point;
            var localPosition = transform1.localPosition;
            localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z + collisionSensitivity);
            transform1.localPosition = localPosition;
        }
        // Clean up
        Destroy(obj);

        // Make sure camera can't clip into player because of collision
        if (cam.transform.localPosition.z > -1f)
        {
            cam.transform.localPosition =
                new Vector3(cam.transform.localPosition.x, cam.transform.localPosition.y, -1f);
        }
    }
}