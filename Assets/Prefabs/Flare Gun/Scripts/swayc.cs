using UnityEngine;
using System.Collections;

public class swayc : MonoBehaviour {
	public float moveAmount = 1f;
	public float moveSpeed = 2f;
	public GameObject gun;
	private float moveOnX;
	private float moveOnY;
	public Vector3 defaultPosition;
	public Vector3 newGunPosition;
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		moveOnX = Input.GetAxis("Mouse X") * Time.deltaTime * moveAmount;
 
		moveOnY = Input.GetAxis("Mouse Y") * Time.deltaTime * moveAmount;
 
		newGunPosition = new Vector3 (defaultPosition.x+moveOnX, defaultPosition.y+moveOnY, defaultPosition.z);
 
		gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, newGunPosition, moveSpeed*Time.deltaTime);
	
	}
	
	void OnGUI()	
	{
		
		GUI.Label(new Rect(10,10,150,30),"Press 'R' to reload");
		
		
	}
}
