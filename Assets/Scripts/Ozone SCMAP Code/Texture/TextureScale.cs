using System.Threading;
using UnityEngine;

public class TextureScale
{
	public class ThreadData
	{
		public int start;
		public int end;
		public ThreadData (int s, int e) {
			start = s;
			end = e;
		}
	}

	private static Color[] texColors;
	private static Color[] newColors;
	private static int w;
	private static float ratioX;
	private static float ratioY;
	private static int w2;
	private static int finishCount;
	private static Mutex mutex;

	public static void Point (Texture2D tex, int newWidth, int newHeight)
	{
		ThreadedScale (tex, newWidth, newHeight, false);
	}

	public static Texture2D Bilinear (Texture2D source, int targetWidth, int targetHeight)
	{
		return ThreadedScale (source, targetWidth, targetHeight, true);
		/*
		Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
		float incX = (1.0f / (float)targetWidth);
		float incY = (1.0f / (float)targetHeight);
		for (int i = 0; i < result.height; ++i)
		{
			for (int j = 0; j < result.width; ++j)
			{
				Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
				result.SetPixel(j, i, newColor);
			}
		}
		result.Apply();
		return result;
		*/
	}


	private static Texture2D ThreadedScale (Texture2D tex, int newWidth, int newHeight, bool useBilinear)
	{
		Texture2D NewTex = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, tex.mipmapCount > 1);

		int texWidth = tex.width;
		int texHeight = tex.height;

		//Debug.Log(tex.mipmapCount);

		for (int m = 0; m < tex.mipmapCount; m++)
		{

			texColors = tex.GetPixels(m);
			newColors = NewTex.GetPixels(m);

			if (m > 0) {
				if (newWidth == 1 || newHeight == 1)
					break;

				newWidth /= 2;
				newHeight /= 2;
				texWidth /= 2;
				texHeight /= 2;
			}

			//Debug.Log(m + " : " + texWidth + " > " + newWidth);

			if (texWidth == 1)
			{
				for (int i = 0; i < newColors.Length; i++)
					newColors[i] = texColors[0];


				NewTex.SetPixels(newColors, m);
				break;
			}


			if (useBilinear)
			{
				ratioX = 1.0f / ((float)newWidth / (texWidth - 1));
				ratioY = 1.0f / ((float)newHeight / (texHeight - 1));
			}
			else
			{
				ratioX = ((float)texWidth) / newWidth;
				ratioY = ((float)texHeight) / newHeight;
			}
			w = texWidth;
			w2 = newWidth;
			var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
			var slice = newHeight / cores;

			finishCount = 0;
			if (mutex == null)
			{
				mutex = new Mutex(false);
			}
			if (cores > 1)
			{
				int i = 0;
				ThreadData threadData;
				for (i = 0; i < cores - 1; i++)
				{
					threadData = new ThreadData(slice * i, slice * (i + 1));
					ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
					Thread thread = new Thread(ts);
					thread.Start(threadData);
				}
				threadData = new ThreadData(slice * i, newHeight);
				if (useBilinear)
				{
					BilinearScale(threadData);
				}
				else
				{
					PointScale(threadData);
				}
				while (finishCount < cores)
				{
					Thread.Sleep(1);
				}
			}
			else
			{
				ThreadData threadData = new ThreadData(0, newHeight);
				if (useBilinear)
				{
					BilinearScale(threadData);
				}
				else
				{
					PointScale(threadData);
				}
			}

			NewTex.SetPixels(newColors, m);
		}


		//tex.Resize(newWidth, newHeight);
		//tex.SetPixels(newColors);
		NewTex.Apply(true);
		return NewTex;
	}

	public static void BilinearScale (System.Object obj)
	{
		ThreadData threadData = (ThreadData) obj;
		for (var y = threadData.start; y < threadData.end; y++)
		{
			int yFloor = (int)Mathf.Floor(y * ratioY);
			var y1 = yFloor * w;
			var y2 = (yFloor+1) * w;
			var yw = y * w2;

			for (var x = 0; x < w2; x++) {
				int xFloor = (int)Mathf.Floor(x * ratioX);
				var xLerp = x * ratioX-xFloor;
				newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor+1], xLerp),
					ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor+1], xLerp),
					y*ratioY-yFloor);
			}
		}

		mutex.WaitOne();
		finishCount++;
		mutex.ReleaseMutex();
	}

	public static void PointScale (System.Object obj)
	{
		ThreadData threadData = (ThreadData) obj;
		for (var y = threadData.start; y < threadData.end; y++)
		{
			var thisY = (int)(ratioY * y) * w;
			var yw = y * w2;
			for (var x = 0; x < w2; x++) {
				newColors[yw + x] = texColors[(int)(thisY + ratioX*x)];
			}
		}

		mutex.WaitOne();
		finishCount++;
		mutex.ReleaseMutex();
	}

	private static Color ColorLerpUnclamped (Color c1, Color c2, float value)
	{
		return new Color (c1.r + (c2.r - c1.r)*value, 
			c1.g + (c2.g - c1.g)*value, 
			c1.b + (c2.b - c1.b)*value, 
			c1.a + (c2.a - c1.a)*value);
	}
}