using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{

		public static SelectionManager Current;

		[Header("Core")]
		public Camera Cam;
		public RectTransform SelBox;
		public int DisableLayer;
		public int UsedLayer;
		public bool SnapToGrid;

		public GameObject[] AffectedGameObjects;
		public bool Active = false;


		void Awake()
		{
			Current = this;
		}

		public bool AllowUp;
		public bool AllowRotation;
		public bool AllowScale;

		public void SetAffectedGameObjects(GameObject[] GameObjects, bool _AllowUp = false, bool _AllowRotation = false, bool _AllowScale = false)
		{
			AllowUp = _AllowUp;
			AllowRotation = _AllowRotation;
			AllowScale = _AllowScale;

			Controls_Up.SetActive(AllowUp);
			Controls_Rotate.SetActive(AllowRotation);
			Controls_Scale.SetActive(AllowScale);

			if (AffectedGameObjects.Length > 0)
			{
				// Clean
				for(int i = 0; i < AffectedGameObjects.Length; i++)
				{
					if(AffectedGameObjects[i])
						AffectedGameObjects[i].layer = DisableLayer;
				}
			}

			AffectedGameObjects = GameObjects;

			if (AffectedGameObjects.Length > 0)
			{
				// activate settings
				for (int i = 0; i < AffectedGameObjects.Length; i++)
				{
					if (AffectedGameObjects[i])
						AffectedGameObjects[i].layer = UsedLayer;
				}
			}


			Active = AffectedGameObjects.Length > 0;

		}





	}
}
