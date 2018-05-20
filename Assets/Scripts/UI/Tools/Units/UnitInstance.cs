using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstance : MonoBehaviour {


	public GetGamedataFile.UnitObject Owner;

	public Matrix4x4 LocalToWorldMatrix;

	Vector3 Position;
	Vector3 Scale;

	public void SetMatrix(Vector3 Position, Quaternion Rotation)
	{
		transform.localPosition = Position;
		this.Position = Position;
		transform.localRotation = Rotation;

		Scale = (Owner.BP.UniformScale.x * 0.1f) * Owner.BP.Size;

		LocalToWorldMatrix = Matrix4x4.TRS(Position, transform.localRotation, Owner.BP.UniformScale * 0.1f);
	}

	public void UpdateMatrix()
	{
		Position = transform.localPosition;
		//Scale = Owner.BP.UniformScale.x * Owner.BP.Size;

		LocalToWorldMatrix = Matrix4x4.TRS(Position, transform.localRotation, Owner.BP.UniformScale * 0.1f);
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
		if (Owner)
		{
			UpdateMatrix();
			Owner.AddInstance(this);

		}
	}

	private void OnDisable()
	{
		if (Owner)
			Owner.RemoveInstance(this);
	}
}
