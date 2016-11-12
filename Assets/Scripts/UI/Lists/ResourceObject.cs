using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResourceObject : MonoBehaviour {

	public ResourceBrowser Controler;
	public int InstanceId;
	public Text NameField;
	public GameObject Selected;

	public void Clicked(){

	}
	public void OnBeginDrag(){
		ResourceBrowser.DragedObject = this;

		Cursor.SetCursor ((Texture2D)GetComponent<RawImage> ().texture, Vector2.zero, CursorMode.Auto);
	}
}
