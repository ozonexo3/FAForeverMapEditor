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

		public GameObject[] Selection;
		public GameObject[] Page;
		public ChainsList ChainsInfo;
		public MarkersList MarkerList;

		void OnEnable()
		{
			ChangePage(CurrentPage);

		}

		void OnDisable()
		{
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
			MarkerPageChange = true;
			if (CurrentPage == PageId)
				return;
			PreviousPage = CurrentPage;
			CurrentPage = PageId;

			for(int i = 0; i < Page.Length; i++)
			{
				Page[i].SetActive(false);
				Selection[i].SetActive(false);
			}

			Page[CurrentPage].SetActive(true);
			Selection[CurrentPage].SetActive(true);
			MarkerPageChange = false;
		}

	}
}
