using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	public float speed = 1f;
	public float zoomSpeed = 1f;
	public float horRotationSpeed = 1f;
	public float vertRotationSpeed = 1f;
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
		float z = Input.GetAxis("Vertical") * speed * Time.deltaTime;
		float y = -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
		float rotY = transform.eulerAngles.y;

    Vector3 pan = new Vector3(
        Mathf.Cos(rotY*Mathf.Deg2Rad)*x + Mathf.Sin(rotY*Mathf.Deg2Rad)*z, y,
        Mathf.Cos(rotY*Mathf.Deg2Rad)*z - Mathf.Sin(rotY*Mathf.Deg2Rad)*x
    );
		if(Input.GetMouseButton(2))
		{
			float mouseX = Input.GetAxis("Mouse X") * horRotationSpeed * Time.deltaTime;
			float mouseY = Input.GetAxis("Mouse Y") * vertRotationSpeed * Time.deltaTime;
			//this.transform.RotateAroundLocal(this.transform.position,mouseX);
			this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + mouseX, this.transform.eulerAngles.z);
			this.transform.Rotate(mouseY,0,0);
		}
		this.transform.Translate(pan, Space.World);
	}
}
