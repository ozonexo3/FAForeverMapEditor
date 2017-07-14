using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Markers
{
	public class MarkerConnected : MonoBehaviour
	{
		public static MarkerConnected Current;
		public LineRenderer Line;

		public Transform[] Connected;
		Vector3[] Positions;

		void Awake()
		{
			Current = this;
		}

		public static void RenderConnection(string[] MarkerNames, string parent = "")
		{
			if (Current == null)
				return;

			Transform[] Trans = Current.gameObject.GetComponentsInChildren<Transform>();
			List<Transform> Found = new List<Transform>();
			for (int i = 0; i < Trans.Length; i++)
			{
				if (Trans[i].name == parent)
				{
					Found.Add(Trans[i]);
					break;
				}
			}

			if (Found.Count == 0)
				return;

			for (int n = 0; n < MarkerNames.Length; n++)
			{
				for (int i = 0; i < Trans.Length; i++)
				{
					if(Trans[i].name == MarkerNames[n])
					{
						Found.Add(Trans[i]);
						break;
					}
				}
			}

			Current.Connected = Found.ToArray();
			Current.Positions = new Vector3[(Current.Connected.Length) * 2];

			Current.Line.enabled = Current.Connected.Length > 0;
			Current.Line.positionCount = Current.Positions.Length;
		}

		public static void Clear()
		{
			if (Current == null)
				return;
			Current.Connected = new Transform[0];
			if(Current.Line)
				Current.Line.enabled = false;
		}


		void Update()
		{
			if(Connected.Length > 0)
			{
				for (int i = 0; i < Positions.Length; i += 2)
				{
					Positions[i] = Connected[0].localPosition;
				}

				for (int i = 1; i < Positions.Length; i+=2)
				{
					Positions[i] = Connected[(i - 1) / 2].localPosition;
				}

				Current.Line.SetPositions(Positions);
			}
		}

	}
}
