using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace EditMap
{
	public class MarkersInfo : MonoBehaviour
	{

		public static MarkersInfo Current;

		private void Awake()
		{
			Current = this;
		}

		public GameObject[] PageSelection;
		public GameObject[] Page;
		public ChainsList ChainsInfo;
		public MarkersList MarkerList;

		void OnEnable()
		{
			ChangePage(CurrentPage);

		}

		void OnDisable()
		{
			Selection.SelectionManager.Current.ClearAffectedGameObjects();
			MarkerList.Clean();
		}

		public int GetCurrentPage()
		{
			return CurrentPage;
		}

		public int PreviousCurrentPage()
		{
			return PreviousPage;
		}

		int PreviousPage = 0;
		int CurrentPage = 0;
		public static bool MarkerPageChange = false;
		public void ChangePage(int PageId)
		{
			if (CurrentPage == PageId && Page[CurrentPage].activeSelf && PageSelection[CurrentPage].activeSelf)
				return;
			MarkerPageChange = true;

			PreviousPage = CurrentPage;
			CurrentPage = PageId;

			for(int i = 0; i < Page.Length; i++)
			{
				Page[i].SetActive(false);
				PageSelection[i].SetActive(false);
			}

			Page[CurrentPage].SetActive(true);
			PageSelection[CurrentPage].SetActive(true);
			MarkerPageChange = false;
		}

	}
}
