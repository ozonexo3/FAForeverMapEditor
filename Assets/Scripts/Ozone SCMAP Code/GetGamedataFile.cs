using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;

public class GetGamedataFile : MonoBehaviour {

	public static	string			GameDataPath;
	public			ScmapEditor		Scmap;
	public			Texture2D		EmptyTexture;

	void Start () {

	}


	public void SetPath(){
		GameDataPath = PlayerPrefs.GetString("GameDataPath", "gamedata/");
	}

	public void LoadTextureFromGamedata(string scd, string LocalPath, int Id, bool NormalMap = false){
		SetPath();

		if(!Directory.Exists(GameDataPath)){
			Debug.LogError("Gamedata path not exist!");
			return;
		}

		if(!Directory.Exists("temfiles")) Directory.CreateDirectory("temfiles");

		ZipFile zf = null;
		try {
			FileStream fs = File.OpenRead(GameDataPath + scd);
			zf = new ZipFile(fs);



			char[] sep = ("/").ToCharArray();
			string[] LocalSepPath = LocalPath.Split(sep);
			string FileName = LocalSepPath[LocalSepPath.Length - 1];


			foreach (ZipEntry zipEntry in zf) {
				if (!zipEntry.IsFile) {
					continue;
				}
				if(zipEntry.Name.ToLower() == LocalPath.ToLower() || zipEntry.Name == LocalPath.ToLower()){
					//Debug.LogWarning("File found!");

					byte[] buffer = new byte[4096]; // 4K is optimum
					Stream zipStream = zf.GetInputStream(zipEntry);
					int size = 4096;
		

					using (FileStream streamWriter = File.Create("temfiles/" + FileName))
					{
						while (true)
							{
							size = zipStream.Read(buffer, 0, buffer.Length);
							if (size > 0)
							{
								streamWriter.Write(buffer, 0, size);
							}
							else
							{
								break;
							}
						}
						streamWriter.Close();
					}



					byte[] FinalTextureData = System.IO.File.ReadAllBytes("temfiles/" + FileName);

					byte ddsSizeCheck = FinalTextureData[4];
					if (ddsSizeCheck != 124)
						throw new Exception("Invalid DDS DXTn texture. Unable to read"); //this header byte should be 124 for DDS image files

					int height = FinalTextureData[13] * 256 + FinalTextureData[12];
					int width = FinalTextureData[17] * 256 + FinalTextureData[16];

					TextureFormat format;;
					// Now this is made realy bad. I don't know how to check DDS texture format, so i check texture size and its all bytes length to check if there are 3 or 4 channels
					float FormatFileSize = (float)FinalTextureData.Length / ((float)(width * height));
					//Debug.LogWarning("Size: " +  FormatFileSize);
					if(FormatFileSize < 1){
						format = TextureFormat.DXT1;
						Debug.LogWarning(FileName + " is DXT1"); 
					}
					else if(FormatFileSize > 4){
						format = TextureFormat.RGB24;
						Debug.LogWarning(FileName + " is RGB24"); 
					}
					else{
						format = TextureFormat.DXT5;
						Debug.LogWarning(FileName + " is DXT5"); 
					}

					format = GetFormatOfDds("temfiles/" + FileName);
					Debug.Log(width +", " + height +", "+ format);

					Texture2D texture = new Texture2D(width, height, format, true);

					if(FileName == "snow001_normals.dds"){
						//Debug.Log("Change format");
						//texture = new Texture2D(width, height, TextureFormat.DXT5, true);
					}

					int DDS_HEADER_SIZE = 128;
					byte[] dxtBytes = new byte[FinalTextureData.Length - DDS_HEADER_SIZE];
					Buffer.BlockCopy(FinalTextureData, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData.Length - DDS_HEADER_SIZE);
					//texture.LoadImage(FinalTextureData);
					texture.LoadRawTextureData(dxtBytes);
					texture.Apply();

					if(NormalMap){
						Texture2D normalTexture = new Texture2D(height, width, TextureFormat.ARGB32, true);

						Color theColour = new Color();
						for (int x=0; x<texture.width; x++){
							for (int y=0; y<texture.height; y++){
								theColour.r = texture.GetPixel(x,y).r;
								theColour.g = texture.GetPixel(x,y).g;
								theColour.b = 1;
								theColour.a = texture.GetPixel(x,y).g;
								normalTexture.SetPixel(x,y, theColour);
							}
						}

						normalTexture.Apply();

						Scmap.Textures[Id].Normal = normalTexture;
						Scmap.Textures[Id].Normal.mipMapBias = -0.0f;
						Scmap.Textures[Id].Normal.filterMode = FilterMode.Trilinear;
						Scmap.Textures[Id].Normal.anisoLevel = 6;
					}
					else{
						Scmap.Textures[Id].Albedo = texture;
						Scmap.Textures[Id].Albedo.mipMapBias = -0.0f;
						Scmap.Textures[Id].Albedo.filterMode = FilterMode.Trilinear;
						Scmap.Textures[Id].Albedo.anisoLevel = 6;
					}

				}
			}
		} finally {
			if (zf != null) {
				zf.IsStreamOwner = true; // Makes close also shut the underlying stream
				zf.Close(); // Ensure we release resources
			}
		}
	}


	//************************************* SIMPLE LOAD
	public Texture2D LoadSimpleTextureFromGamedata(string scd, string LocalPath, bool NormalMap = false){
		SetPath();
		
		if(!Directory.Exists(GameDataPath)){
			Debug.LogError("Gamedata path not exist!");
			return null;
		}
		
		if(!Directory.Exists("temfiles")) Directory.CreateDirectory("temfiles");
		Texture2D texture = null;

		Debug.LogWarning("Load texture: " + GameDataPath + scd + LocalPath);
		ZipFile zf = null;
		try {
			FileStream fs = File.OpenRead(GameDataPath + scd);
			zf = new ZipFile(fs);

			
			char[] sep = ("/").ToCharArray();
			string[] LocalSepPath = LocalPath.Split(sep);
			string FileName = LocalSepPath[LocalSepPath.Length - 1];
			
			
			foreach (ZipEntry zipEntry in zf) {
				if (!zipEntry.IsFile) {
					continue;
				}
				Debug.Log(zipEntry.Name.ToLower() + " - " + LocalPath.ToLower());

				if(zipEntry.Name.ToLower() == LocalPath.ToLower() || zipEntry.Name == LocalPath.ToLower() || ("/" + zipEntry.Name).ToLower() == LocalPath.ToLower()){
					Debug.LogWarning("File found!");
					
					byte[] buffer = new byte[4096]; // 4K is optimum
					Stream zipStream = zf.GetInputStream(zipEntry);
					int size = 4096;
					using (FileStream streamWriter = File.Create("temfiles/" + FileName))
					{
						while (true)
						{
							size = zipStream.Read(buffer, 0, buffer.Length);
							if (size > 0)
							{
								streamWriter.Write(buffer, 0, size);
							}
							else
							{
								break;
							}
						}
					}
					
					
					
					byte[] FinalTextureData = System.IO.File.ReadAllBytes("temfiles/" + FileName);
					
					byte ddsSizeCheck = FinalTextureData[4];
					if (ddsSizeCheck != 124)
						throw new Exception("Invalid DDS DXTn texture. Unable to read"); //this header byte should be 124 for DDS image files
					
					int height = FinalTextureData[13] * 256 + FinalTextureData[12];
					int width = FinalTextureData[17] * 256 + FinalTextureData[16];
					
					TextureFormat format;;

					// Now this is made realy bad. I don't know how to check DDS texture format, so i check texture size and its all bytes length to check if there are 3 or 4 channels and how many bits
					float FormatFileSize = (float)FinalTextureData.Length / ((float)(width * height));
					if(FormatFileSize < 1){
						Debug.Log("Dxt1");
						//format = TextureFormat.DXT1;
					}
					else if(FormatFileSize > 4){
						Debug.Log("rgb24");
						//format = TextureFormat.RGB24;
					}
					else{
						Debug.Log("Dxt5");
						//format = TextureFormat.DXT5;
					}


					format = GetFormatOfDds("temfiles/" + FileName);
					
					texture = new Texture2D(width, height, format, true);
					int DDS_HEADER_SIZE = 128;
					byte[] dxtBytes = new byte[FinalTextureData.Length - DDS_HEADER_SIZE];
					Buffer.BlockCopy(FinalTextureData, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData.Length - DDS_HEADER_SIZE);
					texture.LoadRawTextureData(dxtBytes);
					texture.Apply();
					
					if(NormalMap){
						Texture2D normalTexture = new Texture2D(height, width, TextureFormat.ARGB32, true);
						
						Color theColour = new Color();
						for (int x=0; x<texture.width; x++){
							for (int y=0; y<texture.height; y++){
								theColour.r = texture.GetPixel(x,y).r;
								theColour.g = texture.GetPixel(x,y).g;
								theColour.b = 1;
								theColour.a = texture.GetPixel(x,y).g;
								normalTexture.SetPixel(x,y, theColour);
							}
						}
						
						normalTexture.Apply();

						normalTexture.mipMapBias = -0.4f;
						normalTexture.anisoLevel = 6;
						normalTexture.filterMode = FilterMode.Trilinear;

						texture = normalTexture;
					}
					else{
						texture.mipMapBias = -0.4f;
						texture.filterMode = FilterMode.Trilinear;
						texture.anisoLevel = 6;

					}
					
				}
			}
		} finally {
			if (zf != null) {
				zf.IsStreamOwner = true; // Makes close also shut the underlying stream
				zf.Close(); // Ensure we release resources
			}
		}

		Debug.LogError("Gamedata path not exist!");
		return texture;
	}

	public		HeaderClass			LoadDDsHeader;
	
	[System.Serializable]
	public class HeaderClass{
		public		uint size;
		public		uint flags;
		public		uint height;
		public		uint width;
		public		uint sizeorpitch;
		public		uint depth;
		public		uint mipmapcount;
		public		uint alphabitdepth;
		public		uint[] reserved;
		
		public		uint pixelformatSize;
		public		uint pixelformatflags;
		public		uint pixelformatFourcc;
		public		uint pixelformatRgbBitCount;
		public		uint pixelformatRbitMask;
		public		uint pixelformatGbitMask;
		public		uint pixelformatBbitMask;
		public		uint pixelformatAbitMask;
	}

	public TextureFormat GetFormatOfDds(string FinalImagePath){

		if(!File.Exists(FinalImagePath)){
			Debug.LogError("File not exist!");
			return TextureFormat.DXT5;
		}

		// Load DDS Header
		System.IO.FileStream fs = new System.IO.FileStream(FinalImagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
		BinaryReader Stream = new BinaryReader(fs);
		LoadDDsHeader = new HeaderClass();
		
		byte[] signature = Stream.ReadBytes(4);
		LoadDDsHeader.size = Stream.ReadUInt32();
		LoadDDsHeader.flags = Stream.ReadUInt32();
		LoadDDsHeader.height = Stream.ReadUInt32();
		LoadDDsHeader.width = Stream.ReadUInt32();
		LoadDDsHeader.sizeorpitch = Stream.ReadUInt32();
		LoadDDsHeader.depth = Stream.ReadUInt32();
		LoadDDsHeader.mipmapcount = Stream.ReadUInt32();
		LoadDDsHeader.alphabitdepth = Stream.ReadUInt32();
		
		
		LoadDDsHeader.reserved = new uint[10];
		for (int i = 0; i < 10; i++)
		{
			LoadDDsHeader.reserved[i] = Stream.ReadUInt32();
		}
		
		LoadDDsHeader.pixelformatSize = Stream.ReadUInt32();
		LoadDDsHeader.pixelformatflags = Stream.ReadUInt32();
		LoadDDsHeader.pixelformatFourcc = Stream.ReadUInt32();
		LoadDDsHeader.pixelformatRgbBitCount = Stream.ReadUInt32();
		LoadDDsHeader.pixelformatRbitMask = Stream.ReadUInt32();
		LoadDDsHeader.pixelformatGbitMask = Stream.ReadUInt32();
		LoadDDsHeader.pixelformatBbitMask = Stream.ReadUInt32();
		LoadDDsHeader.pixelformatAbitMask = Stream.ReadUInt32();

		return ReadFourcc(LoadDDsHeader.pixelformatFourcc);

	}


	public TextureFormat ReadFourcc(uint fourcc){
		/*
		uint FOURCC_DXT1 = 0x31545844;
		uint FOURCC_DXT2 = 0x32545844;
		uint FOURCC_DXT3 = 0x33545844;
		uint FOURCC_DXT4 = 0x34545844;
		uint FOURCC_DXT5 = 0x35545844;
		uint FOURCC_ATI1 = 0x31495441;
		uint FOURCC_ATI2 = 0x32495441;
		uint FOURCC_RXGB = 0x42475852;
		uint FOURCC_DOLLARNULL = 0x24;
		uint FOURCC_oNULL = 0x6f;
		uint FOURCC_pNULL = 0x70;
		uint FOURCC_qNULL = 0x71;
		uint FOURCC_rNULL = 0x72;
		uint FOURCC_sNULL = 0x73;
		uint FOURCC_tNULL = 0x74;
		*/

		int mask0 = 0;
		int mask1 = 255;
		int mask2 = 65280;
		int mask3 = 16711680;
		
		Debug.Log("Fourcc: " + fourcc + ", Count: " + LoadDDsHeader.pixelformatRgbBitCount +", Mask:" + LoadDDsHeader.pixelformatRbitMask + ", " + LoadDDsHeader.pixelformatGbitMask + ", " + LoadDDsHeader.pixelformatBbitMask
			+", AbitMask: "+ LoadDDsHeader.pixelformatAbitMask +", AlphaBitDepth: "+ 	LoadDDsHeader.alphabitdepth
			+", : "+ 	LoadDDsHeader.depth +", Flags: "+ 	LoadDDsHeader.flags +", : "+ 	LoadDDsHeader.pixelformatFourcc +", : "+ 	LoadDDsHeader.pixelformatSize);



		switch(fourcc){
		case 827611204:
			return TextureFormat.DXT1;
		case 894720068:
			return TextureFormat.DXT5;
		case 64:
			return TextureFormat.RGB24;
		case 0:
			if(LoadDDsHeader.flags == 528391){
				return TextureFormat.DXT5;
			}
			else if(LoadDDsHeader.pixelformatRgbBitCount == 24){
				return TextureFormat.RGB24;
			}
			else{
				return TextureFormat.BGRA32;
			}
		}
		
		return TextureFormat.DXT5;
	}
}
