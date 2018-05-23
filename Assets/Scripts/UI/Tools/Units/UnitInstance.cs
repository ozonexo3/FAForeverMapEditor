using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstance : MonoBehaviour
{

	public BoxCollider Col;

	public string orders;
	public string platoon;

	public GetGamedataFile.UnitObject UnitRenderer;
	public MapLua.SaveLua.Army.UnitsGroup Group;


	public Matrix4x4 LocalToWorldMatrix;

	Vector3 Position;
	Vector3 Scale;

	public static HashSet<GameObject> AllUnitInstances = new HashSet<GameObject>();
	public static GameObject[] GetAllUnitGo(out int[] Types)
	{
		GameObject[] ToReturn = new GameObject[AllUnitInstances.Count];
		Types = new int[ToReturn.Length];
		AllUnitInstances.CopyTo(ToReturn);
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

	public BoundingSphere Sphere
	{
		get
		{
			return new BoundingSphere(Position, Mathf.Max(Scale.x, Scale.y, Scale.z));
		}
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
}
