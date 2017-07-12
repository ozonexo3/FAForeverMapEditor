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

		public GameObject[] AfectedGameObjects;
		private bool Active = false;


		void Awake()
		{
			Current = this;
		}

		public void SetAfectedGameObjects(GameObject[] GameObjects)
		{
			if(AfectedGameObjects.Length == 0)
			{
				// Clean
				for(int i = 0; i < AfectedGameObjects.Length; i++)
				{
					if(AfectedGameObjects[i])
						AfectedGameObjects[i].layer = DisableLayer;
				}
			}

			AfectedGameObjects = GameObjects;

			if (AfectedGameObjects.Length == 0)
			{
				// activate settings
				for (int i = 0; i < AfectedGameObjects.Length; i++)
				{
					if (AfectedGameObjects[i])
						AfectedGameObjects[i].layer = UsedLayer;
				}
			}


			Active = AfectedGameObjects.Length > 0;

		}





	}
}
