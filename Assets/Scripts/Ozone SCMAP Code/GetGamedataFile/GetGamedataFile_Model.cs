using UnityEngine;
using System.Collections;
using System;
using System.IO;

public partial struct GetGamedataFile
{

	#region Scm Objects
	class Scm
	{
		public ScmHeader Header;
		public Scm_Bone[] Bones;
		public string[] BoneNames;
		public Scm_Vert[] Verts;
		public Scm_Tris[] Tris;
		//public Vector4[] SkinWeights;
		public int[] WitchBone;

		public void LoadFromStream(BinaryReader Stream)
		{
			Header = new ScmHeader();
			Header.LoadFromStream(Stream);
			if (!Header.IsValid())
			{
				// File is not valid, cancel reading file
				Debug.LogError("Invalid header: " + Header.fourcc + ", " + Header.Version);
				return;
			}

			//Stream.ReadBytes(64);
			int i = 0;

			//Debug.Log(Header.BoneOffset);
			Stream.BaseStream.Seek(64, SeekOrigin.Begin);

			// Load BoneNames
			BoneNames = new string[Header.wBoneCount];
			for (i = 0; i < Header.wBoneCount; i++)
			{
				BoneNames[i] = Stream.ReadStringNull();
			}

			// Load Bones
			Stream.BaseStream.Seek(Header.BoneOffset, SeekOrigin.Begin);
			Bones = new Scm_Bone[Header.wBoneCount];
			for (i = 0; i < Bones.Length; i++)
			{
				Bones[i] = new Scm_Bone();
				Bones[i].LoadFromStream(Stream);
			}

			// Load Vers
			Stream.BaseStream.Seek(Header.VertOffset, SeekOrigin.Begin);

			Verts = new Scm_Vert[Header.VertCount];
			for (i = 0; i < Header.VertCount; i++)
			{
				Verts[i] = new Scm_Vert();
				Verts[i].LoadFromStream(Stream);
			}

			// Load Triangles
			Stream.BaseStream.Seek(Header.IndexOffset, SeekOrigin.Begin);
			Tris = new Scm_Tris[Header.IndexCount / 3];
			for (i = 0; i < Tris.Length; i++)
			{
				Tris[i] = new Scm_Tris();
				Tris[i].LoadFromStream(Stream);
			}
		}

		public Vector3[] GetVerts()
		{
			Vector3[] VertsArray = new Vector3[Verts.Length];

			for (int i = 0; i < VertsArray.Length; i++)
			{
				VertsArray[i] = Verts[i].Position;
			}
			return VertsArray;
		}

		public Vector3[] GetNormals()
		{
			Vector3[] VertsArray = new Vector3[Verts.Length];

			for (int i = 0; i < VertsArray.Length; i++)
			{
				VertsArray[i] = Verts[i].Normal;
			}
			return VertsArray;
		}

		public Vector2[] GetUv0()
		{
			Vector2[] VertsArray = new Vector2[Verts.Length];

			for (int i = 0; i < VertsArray.Length; i++)
			{
				VertsArray[i] = Verts[i].Uv0;
			}
			return VertsArray;
		}

		public Vector2[] GetUv1()
		{
			Vector2[] VertsArray = new Vector2[Verts.Length];

			for (int i = 0; i < VertsArray.Length; i++)
			{
				VertsArray[i] = Verts[i].Uv1;
			}
			return VertsArray;
		}

		public int[] GetTris()
		{
			int[] TrisArray = new int[Tris.Length * 3];

			for (int i = 0; i < Tris.Length; i++)
			{
				TrisArray[i * 3] = Tris[i].Vert0;
				TrisArray[i * 3 + 1] = Tris[i].Vert1;
				TrisArray[i * 3 + 2] = Tris[i].Vert2;
			}
			return TrisArray;

		}
	}


	class ScmHeader
	{
		public string fourcc; // "MODL"
		public int Version; // "5"
		public int BoneOffset;
		public int wBoneCount; // Number of bones influencing verts
		public int VertOffset;
		public int eVertOffset;
		public int VertCount;
		public int IndexOffset; // Indices
		public int IndexCount;
		public int InfoOffset; // Info?
		public int InfoCount;
		public int TotalBones;

		public void LoadFromStream(BinaryReader Stream)
		{
			fourcc = Stream.ReadString(4);

			Version = Stream.ReadInt32();
			BoneOffset = Stream.ReadInt32();
			wBoneCount = Stream.ReadInt32();
			VertOffset = Stream.ReadInt32();
			eVertOffset = Stream.ReadInt32();
			VertCount = Stream.ReadInt32();
			IndexOffset = Stream.ReadInt32();
			IndexCount = Stream.ReadInt32();
			InfoOffset = Stream.ReadInt32();
			InfoCount = Stream.ReadInt32();
			TotalBones = Stream.ReadInt32();
		}

		public bool IsValid()
		{
			if (fourcc != "MODL") return false;
			if (Version != 5)
			{
				Debug.LogError("Wrong SCM file format! Found " + Version + ", but should be 5");
				return false;
			}
			return true;
		}
	}

	class Scm_Vert
	{
		public Vector3 Position;
		// Tangent space
		public Vector3 Normal;
		public Vector3 Tangent; //?? why not vector4?
		public Vector3 Binormal;
		// supports 2 sets of UV
		public Vector2 Uv0;
		public Vector2 Uv1;
		public byte[] BoneIndex; // 4 connected bones
		public int NewIndex;
		public int OldInxed;

		public Scm_Vert()
		{
			BoneIndex = new byte[4];
		}

		public void LoadFromStream(BinaryReader Stream)
		{
			Position = Stream.ReadVector3();
			Normal = Stream.ReadVector3();
			Tangent = Stream.ReadVector3();
			Binormal = Stream.ReadVector3();

			Uv0 = Stream.ReadVector2();
			//Uv0.y = 1 - Uv0.y;
			Uv1 = Stream.ReadVector2();
			//Uv1.y = 1 - Uv1.y;
			//BoneIndex = new byte[4];
			BoneIndex[0] = Stream.ReadByte();
			BoneIndex[1] = Stream.ReadByte();
			BoneIndex[2] = Stream.ReadByte();
			BoneIndex[3] = Stream.ReadByte();
		}
	}

	class Scm_Tris
	{
		public short Vert0;
		public short Vert1;
		public short Vert2;

		public void LoadFromStream(BinaryReader Stream)
		{
			Vert0 = Stream.ReadInt16();
			Vert1 = Stream.ReadInt16();
			Vert2 = Stream.ReadInt16();
		}
	}

	class Scm_Bone
	{
		public Matrix4x4 InverseBindPose;
		public Vector3 Position;
		public Quaternion Rotation; // W, X, Y, Z
		public int NameOffset;
		public int ParentBoneIndex;
		public int Reserved0;
		public int Reserved1;
		public Matrix4x4 transform;

		public void LoadFromStream(BinaryReader Stream)
		{
			InverseBindPose.SetRow(0, Stream.ReadVector4());
			InverseBindPose.SetRow(1, Stream.ReadVector4());
			InverseBindPose.SetRow(2, Stream.ReadVector4());
			InverseBindPose.SetRow(3, Stream.ReadVector4());

			Position = Stream.ReadVector3();
			Rotation = new Quaternion();
			Rotation.w = Stream.ReadSingle();
			Rotation.x = Stream.ReadSingle();
			Rotation.y = Stream.ReadSingle();
			Rotation.z = Stream.ReadSingle();

			NameOffset = Stream.ReadInt32();
			ParentBoneIndex = Stream.ReadInt32();
			Reserved0 = Stream.ReadInt32();
			Reserved1 = Stream.ReadInt32();

			transform = InverseBindPose.inverse;
		}
	}

	class Sca_Frame
	{
		//TO DO Import animation
	}

	#endregion

	public static Mesh LoadModel(string scd, string LocalPath)
	{
		byte[] FinalMeshBytes = LoadBytes(scd, LocalPath);

		if (FinalMeshBytes == null || FinalMeshBytes.Length == 0)
			return null; // File is empty

		Mesh ToReturn = new Mesh();

		// Create stream from bytes, to read it as binary file
		BinaryReader Stream = new BinaryReader(new MemoryStream(FinalMeshBytes));

		Scm NewScmModel = new Scm();
		NewScmModel.LoadFromStream(Stream);

		// Create Unity Mesh from Scm data
		ToReturn.name = NewScmModel.BoneNames[0];

		ToReturn.vertices = NewScmModel.GetVerts();
		ToReturn.normals = NewScmModel.GetNormals();
		ToReturn.uv = NewScmModel.GetUv0();
		ToReturn.uv2 = NewScmModel.GetUv1();

		//ToReturn.subMeshCount = 1;
		ToReturn.subMeshCount = 1;
		ToReturn.SetIndices(NewScmModel.GetTris(), MeshTopology.Triangles, 0);

		ToReturn.RecalculateBounds();
		ToReturn.RecalculateNormals();

		return ToReturn;
	}
}
