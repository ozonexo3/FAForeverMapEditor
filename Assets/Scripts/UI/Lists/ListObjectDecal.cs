using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using EditMap;

public class ListObjectDecal : ListObject
{

	public Decal.DecalSharedSettings Setting;
	public RawImage Image;

	public Text VisibleText;
	bool LastHidden = false;
	public void SetHidden(bool Hidden)
	{
		if (LastHidden == Hidden)
			return;

		VisibleText.text = Hidden ? ("H") : ("V");
		LastHidden = Hidden;
	}

	public void SwitchVisible()
	{
		if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftShift))
		{
			DecalsInfo.Current.ToggleHideOther(Setting);
		}
		else
		{
			Setting.Hidden = !Setting.Hidden;
		}
		SetHidden(Setting.Hidden);
	}
}
