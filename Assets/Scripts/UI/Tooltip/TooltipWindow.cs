using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TooltipWindow : MonoBehaviour
{
	public static TooltipWindow Instance { get; private set; }

	public RectTransform window;
	public Text field;
	public LayoutGroup layout;
	public LayoutElement layoutElement;
	public int characterWrapLimit = 80;

	private void Awake()
	{
		Instance = this;
	}

	static Tooltip displayedTip = null;

	static Vector3[] WorldCorners = new Vector3[4];

	public static void Show(Tooltip tip)
	{
		displayedTip = tip;

		RectTransform rt = tip.GetComponent<RectTransform>();
		rt.GetWorldCorners(WorldCorners);

		Instance.window.position = WorldCorners[0] + new Vector3(4, -4, 0);


		Instance.window.gameObject.SetActive(true);
		Instance.field.text = tip.text;
		int textLength = tip.text.Length;
		Instance.layoutElement.enabled = tip.text.Length > Instance.characterWrapLimit;

		Instance.layout.enabled = false;
		Instance.layout.enabled = true;
		LayoutRebuilder.ForceRebuildLayoutImmediate(Instance.layout.GetComponent<RectTransform>());

		Instance.layoutElement.enabled = tip.text.Length > Instance.characterWrapLimit;
		Instance.layoutElement.preferredWidth = Mathf.Min(500f, Instance.field.preferredWidth + 18f);
	}

	public static void Hide(Tooltip tip)
	{
		if(displayedTip == tip)
		{
			displayedTip = null;
			Instance.window.gameObject.SetActive(false);
		}
	}

}
