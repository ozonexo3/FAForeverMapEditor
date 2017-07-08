using UnityEngine;
using System.Collections;

public struct MassMath {

	public static float EasyInOut(float lerp){
		return 0;
	}

	public static float EasyIn(float lerp){
		return 0;
	}

	public static float EasyOut(float lerp){
		return 0;
	}


	public static float StringToFloat(string value)
	{
		float ToReturn = 0f;

		if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out ToReturn))
		{

		}

		return ToReturn;
	}


	public static Quaternion QuaternionFromRotationMatrix(Vector3 VecX, Vector3 VecY, Vector3 VecZ)
	{
		//Quaternion ToReturn = new Quaternion();
		//ToReturn.w = Mathf.Sqrt(1 + VecX.x + VecY.y + VecZ.z) / 2f;
		//ToReturn.x = (VecZ.y - VecY.z) / (4 * ToReturn.w);
		//ToReturn.x = (VecX.z - VecZ.x) / (4 * ToReturn.w);
		//ToReturn.x = (VecY.x - VecY.y) / (4 * ToReturn.w);

		Matrix4x4 NewMatrix = new Matrix4x4();
		NewMatrix.SetRow(0, new Vector4(VecX.x, VecX.y, VecX.z, 0));
		NewMatrix.SetRow(1, new Vector4(VecY.x, VecY.y, VecY.z, 0));
		NewMatrix.SetRow(2, new Vector4(VecZ.x, VecZ.y, VecZ.z, 0));

		//NewMatrix.SetRow(0, new Vector4(VecX.x, VecY.x, VecZ.x, 0));
		//NewMatrix.SetRow(1, new Vector4(VecX.y, VecY.y, VecZ.y, 0));
		//NewMatrix.SetRow(2, new Vector4(VecX.z, VecY.z, VecZ.z, 0));

		NewMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

		return QuaternionFromMatrix(NewMatrix);

		//return ToReturn;
	}

	public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
		q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
		q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
		q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
		q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
		q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
		q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
		return q;
	}

	public static void QuaternionToRotationMatrix(Quaternion Rotation, ref Vector3 VecX, ref Vector3 VecY, ref Vector3 VecZ)
	{
		Matrix4x4 NewMatrix = Matrix4x4.TRS(Vector3.zero, Rotation, Vector3.one);

		Vector4 Row = NewMatrix.GetRow(0);
		VecX.x = Row.x;
		VecX.y = Row.y;
		VecX.z = Row.z;

		Row = NewMatrix.GetRow(1);
		VecY.x = Row.x;
		VecY.y = Row.y;
		VecY.z = Row.z;

		Row = NewMatrix.GetRow(2);
		VecZ.x = Row.x;
		VecZ.y = Row.y;
		VecZ.z = Row.z;
	}

	public static Quaternion MirrorQuaternionX(Quaternion rotation)
	{

		rotation.x *= -1;
		//rotation.z *= -1;
		rotation.w *= -1;

		//rotation *= Quaternion.Euler(Vector3.up * 180);


		return rotation;
	}

	public static Quaternion MirrorQuaternionZ(Quaternion rotation)
	{
		rotation.z *= -1;
		rotation.w *= -1;
		return rotation;
	}

}
