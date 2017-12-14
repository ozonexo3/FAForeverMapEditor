using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ListObjectDecal : ListObject
{

	public Decal.DecalSharedSettings Setting;
	public RawImage Image;

	public Text VisibleText;
	public void SetHidden(bool Hidden)
	{
		VisibleText.text = Hidden ? ("H") : ("V");
	}

	public void SwitchVisible()
	{
		Setting.Hidden = !Setting.Hidden;
		SetHidden(Setting.Hidden);
	}
}
