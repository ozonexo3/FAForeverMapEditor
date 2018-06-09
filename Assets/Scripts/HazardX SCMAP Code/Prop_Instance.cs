using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditMap;

public partial class Prop
{

	public PropGameObject Obj;
	public int GroupId;

	public PropsInfo.PropTypeGroup Group
	{
		get
		{
			return PropsInfo.AllPropsTypes[GroupId];
		}
		set
		{
			GroupId = PropsInfo.AllPropsTypes.IndexOf(value);
		}
	}

	public void Bake()
	{
		BlueprintPath = Group.Blueprint.Replace("\\", "/");

		if (!BlueprintPath.StartsWith("/"))
			BlueprintPath = "/" + BlueprintPath;

		BlueprintPath = GetGamedataFile.FixMapsPath(BlueprintPath);

		Position = ScmapEditor.WorldPosToScmap(Obj.Tr.position);

		RotationX = Vector3.zero;
		RotationY = Vector3.zero;
		RotationZ = Vector3.zero;
		MassMath.QuaternionToRotationMatrix(Obj.Tr.localRotation, ref RotationX, ref RotationY, ref RotationZ);

		Scale = Obj.Tr.localScale;
		Scale.x /= Group.PropObject.BP.LocalScale.x;
		Scale.y /= Group.PropObject.BP.LocalScale.y;
		Scale.z /= Group.PropObject.BP.LocalScale.z;
	}

	public void CreateObject(Vector3 WorldPosition, Quaternion WorldRotation, Vector3 WorldScale, bool AllowFarLod = true)
	{
		Obj = Group.PropObject.CreatePropGameObject(
						WorldPosition,
						WorldRotation,
						WorldScale, AllowFarLod
						);

		Obj.Connected = this;

		Bake();
	}

	public void CreateObject(bool AllowFarLod = true)
	{
		Obj = Group.PropObject.CreatePropGameObject(
						ScmapEditor.ScmapPosToWorld(Position),
						MassMath.QuaternionFromRotationMatrix(RotationX, RotationY, RotationZ),
						Scale, AllowFarLod
						);

		Obj.Connected = this;
	}
	
}
