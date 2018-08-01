using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstance : MonoBehaviour
{

	public BoxCollider Col;

	public string orders;
	public string platoon;

	public UnitSource UnitRenderer;
	public MapLua.SaveLua.Army.Unit Owner;

	public Color ArmyColor;
	public Matrix4x4 LocalToWorldMatrix;
	public float IsWreckage;

	Vector3 Position;
	Vector3 Scale;

	public static HashSet<GameObject> AllUnitInstances = new HashSet<GameObject>();
	public static GameObject[] GetAllUnitGo(out int[] Types)
	{
		AllUnitInstances.Remove(null);

		GameObject[] ToReturn = new GameObject[AllUnitInstances.Count];
		Types = new int[ToReturn.Length];
		AllUnitInstances.CopyTo(ToReturn);

		for (int i = 0; i < Types.Length; i++)
		{
			if (ToReturn[i] == null)
				Types[i] = -1;
			else
				Types[i] = ToReturn[i].GetComponent<UnitInstance>().UnitRenderer.BP.TypeId;
		}

		return ToReturn;
	}

	public void SetMatrix(Vector3 Position, Quaternion Rotation)
	{
		transform.localPosition = Position;
		this.Position = Position;
		transform.localRotation = Rotation;

		Scale = (UnitRenderer.BP.UniformScale.x * 0.1f) * UnitRenderer.BP.Size;

		LocalToWorldMatrix = Matrix4x4.TRS(Position, transform.localRotation, UnitRenderer.BP.UniformScale * 0.1f);
	}

	public void UpdateMatrix()
	{
		Position = transform.localPosition;
		LocalToWorldMatrix = Matrix4x4.TRS(Position, transform.localRotation, UnitRenderer.BP.UniformScale * 0.1f);
	}

	public bool SphereModified = false;
	public void UpdateMatrixTranslated()
	{
		UnitRenderer.RemoveInstance(this);
		UpdateMatrix();
		SphereModified = true;
		UnitRenderer.AddInstance(this, true);
	}

	const float SubDepth = 0.2f;
	public Vector3 GetSnapPosition(Vector3 Pos)
	{
		if (UnitRenderer.BP.PhysicsLayerSub)
		{
			Vector3 PositionOnWater = ScmapEditor.SnapToTerrain(Pos, true);
			if (PositionOnWater.y > ScmapEditor.GetWaterLevel())
				return PositionOnWater;

			Vector3 PositionUnderWater = ScmapEditor.SnapToTerrain(Pos, false);

			if (Mathf.Abs(PositionOnWater.y - PositionUnderWater.y) < UnitRenderer.BP.PhysicsElevation * 0.2f)
				return Vector3.Lerp(PositionOnWater, PositionUnderWater, 0.5f);

			PositionOnWater.y += UnitRenderer.BP.PhysicsElevation * 0.1f;

			return PositionOnWater;

		}
		return ScmapEditor.SnapToTerrain(Pos, UnitRenderer.BP.PhysicsLayerWater);
	}

	public void SnapToTerrain(bool UpdateMatrixes = true)
	{
		transform.localPosition = GetSnapPosition(transform.localPosition);
		if(UpdateMatrixes)
			UpdateMatrixTranslated();
	}

	public bool UpdateAfterTerrainChange()
	{
		Transform tr = transform;

		if (tr == null)
		{
			Debug.LogWarning("Transform component was null! Skip Unit");
			return false;
		}

		Vector3 NewPos = GetSnapPosition(tr.localPosition);

		if (NewPos.y == tr.localPosition.y)
			return false;

		tr.localPosition = NewPos;
		UpdateMatrixTranslated();

		return true;
	}

	public BoundingSphere Sphere
	{
		get
		{
			return new BoundingSphere(Position, Mathf.Max(Scale.x, Scale.y, Scale.z));
		}
	}

	public Vector3 GetScmapRotation()
	{
		return ScmapRotationFromRotation(transform.localRotation);
	}

	public static Quaternion RotationFromScmapRotation(Vector3 Rot)
	{
		Rot.y *= -1;
		return Quaternion.Euler(Rot * Mathf.Rad2Deg);
	}

	public static Vector3 ScmapRotationFromRotation(Quaternion Rot)
	{
		Vector3 ToReturn = Rot.eulerAngles;
		ToReturn.y *= -1;
		return ToReturn * Mathf.Deg2Rad;
	}

	private void OnEnable()
	{
		AllUnitInstances.Add(gameObject);
		if (UnitRenderer)
		{
			UpdateMatrix();
			UnitRenderer.AddInstance(this);
		}
	}

	private void OnDisable()
	{
		AllUnitInstances.Remove(gameObject);
		if (UnitRenderer)
			UnitRenderer.RemoveInstance(this);
	}

	private void OnDestroy()
	{
		AllUnitInstances.Remove(gameObject);
	}
}
