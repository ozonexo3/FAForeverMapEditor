using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Prop
{

	public PropGameObject Obj;
	public int GroupId;

	public void Bake()
	{

	}

	public void CreateObject(bool AllowFarLod = true)
	{
		Obj = EditMap.PropsInfo.AllPropsTypes[GroupId].PropObject.CreatePropGameObject(
						ScmapEditor.ScmapPosToWorld(Position),
						MassMath.QuaternionFromRotationMatrix(RotationX, RotationY, RotationZ),
						Scale, AllowFarLod
						);
	}
	
}
