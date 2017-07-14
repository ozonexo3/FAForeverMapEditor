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

		void Update()
		{
			if (!Active)
				return;

			if (Input.GetKeyDown(KeyCode.Delete))
			{
				DestroySelectedObjects();
			}

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

			CleanIfInactive();

		}

		void CleanIfInactive()
		{
			if (!Active)
			{
				AffectedGameObjects = new GameObject[0];
				Selection.Ids = new List<int>();
				FinishSelectionChange();
			}
		}


		private System.Action<List<GameObject>> RemoveAction;
		public void SetRemoveAction(System.Action<List<GameObject>> Action)
		{
			RemoveAction = Action;

		}


		void DestroySelectedObjects()
		{
			int Count = Selection.Ids.Count;
			if (Count > 0)
			{
				List<GameObject> SelectedObjectsList = new List<GameObject>();
				for(int i = 0; i < Count; i++)
				{
					SelectedObjectsList.Add(AffectedGameObjects[Selection.Ids[i]]);
				}


				for(int s = 0; s < SymetrySelection.Length; s++)
				{
					Count = SymetrySelection[s].Ids.Count;

					for (int i = 0; i < Count; i++)
					{
						SelectedObjectsList.Add(AffectedGameObjects[SymetrySelection[s].Ids[i]]);
					}
				}

				RemoveAction(SelectedObjectsList);

				Active = false;

				CleanIfInactive();
			}
		}




	}
}
