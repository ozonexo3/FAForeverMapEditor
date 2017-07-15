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

		void OnEnable()
		{
			ChangePage(CurrentPage);

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
		public void ChangePage(int PageId)
		{
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

		}

	}
}
