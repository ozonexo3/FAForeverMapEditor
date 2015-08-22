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

	private			string			GameDataPath;
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
						//Debug.LogWarning(FileName + " is DXT1"); 
					}
					else if(FormatFileSize > 4){
						format = TextureFormat.RGB24;
						//Debug.LogWarning(FileName + " is RGB24"); 
					}
					else{
						format = TextureFormat.DXT5;
						//Debug.LogWarning(FileName + " is DXT5"); 
					}

					Texture2D texture = new Texture2D(width, height, format, true);
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
					//Debug.LogWarning("Size: " +  FormatFileSize);
					if(FormatFileSize < 1){
						format = TextureFormat.DXT1;
						//Debug.LogWarning(FileName + " is DXT1"); 
					}
					else if(FormatFileSize > 4){
						format = TextureFormat.RGB24;
						//Debug.LogWarning(FileName + " is RGB24"); 
					}
					else{
						format = TextureFormat.DXT5;
						//Debug.LogWarning(FileName + " is DXT5"); 
					}
					
					texture = new Texture2D(width, height, format, true);
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
	
}
