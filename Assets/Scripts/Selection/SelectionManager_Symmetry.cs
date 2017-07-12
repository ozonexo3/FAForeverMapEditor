using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selection
{
	public partial class SelectionManager : MonoBehaviour
	{

		SelectedObjects Selection = new SelectedObjects();
		SelectedObjects[] SymetrySelection;

		public class SelectedObjects
		{
			public List<int> Ids;
		}

		void GenerateSymmetry()
		{
			SymetrySelection = new SelectedObjects[0];

		}

	}
}