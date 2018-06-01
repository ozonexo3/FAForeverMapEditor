using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace FAF.MapEditor
{
	public class ResourceObject : MonoBehaviour, IBeginDragHandler, IDragHandler
	{

		//public ResourceBrowser Controler;
		public int InstanceId;
		public Text NameField;
		public GameObject Selected;

		public RawImage[] RawImages;

		public Text[] CustomTexts;
		public UI3DMesh MeshView;

		public UnityEvent BeginDrag;

		public void SetImages(Texture2D Tex)
		{
			//Tex.Resize (256, 256);

			foreach (RawImage Rsrc in RawImages)
			{
				Rsrc.texture = Tex;
			}
		}

		public void Clicked()
		{

		}

		public void OnDrag(PointerEventData eventData)
		{

		}

			public void OnBeginDrag(PointerEventData eventData)
		{
			BeginDrag.Invoke();
			ResourceBrowser.DragedObject = this;
			//Cursor.SetCursor ((Texture2D)GetComponent<RawImage> ().texture, Vector2.zero, CursorMode.Auto);
			Cursor.SetCursor(ResourceBrowser.Current.GetCursorImage(), Vector2.zero, CursorMode.Auto);
		}
	}
}