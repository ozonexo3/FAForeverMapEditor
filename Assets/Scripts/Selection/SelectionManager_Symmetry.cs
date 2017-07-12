using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{

		private SelectedObjects Selection = new SelectedObjects();
		SelectedObjects[] SymetrySelection;

		public class SelectedObjects
		{
			public List<int> Ids;
		}

		private int GetIdOfObject(GameObject Obj)
		{
			for(int i = 0; i < AfectedGameObjects.Length; i++)
			{
				if (AfectedGameObjects[i] == Obj)
					return i;
			}
			return -1;
		}


		private void FinishSelectionChange()
		{


		}

		void GenerateSymmetry()
		{
			SymetrySelection = new SelectedObjects[0];

		}

	}
}