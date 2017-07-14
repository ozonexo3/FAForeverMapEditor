using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace EditMap
{
	public class MarkersInfo : MonoBehaviour
	{

		public GameObject[] Selection;
		public GameObject[] Page;

		void OnEnable()
		{
			ChangePage(CurrentPage);

		}

		int CurrentPage = 0;
		public void ChangePage(int PageId)
		{
			if (CurrentPage == PageId)
				return;
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
