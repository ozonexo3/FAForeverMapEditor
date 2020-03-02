using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EditorVersion : MonoBehaviour
{

	public const string EditorBuildVersion = "v0.606-Alpha";

	//Release
	//public const string EditorBuildTag = "HF1";
	//public const float VersionOffset = 0f; // Release

	// Prerelease
	public const string EditorBuildTag = "WIP1";
	public const double VersionOffset = -0.001f; // Prerelease

	public static string LatestTag = "";
	public static string FoundUrl;
	public bool SearchForNew = false;

	string TagString
	{
		get
		{
			if (EditorBuildTag.Length == 0)
				return "";

			return " " + EditorBuildTag;
		}
	}

	void Start()
	{
		GetComponent<Text>().text = EditorBuildVersion + TagString;
		if(SearchForNew)
			StartCoroutine(FindLatest());
	}

	public string url = "https://github.com/ozonexo3/FAForeverMapEditor/releases/latest";
	IEnumerator FindLatest()
	{

		//using (WWW www = new WWW(url))
		DownloadHandler dh = new DownloadHandlerBuffer();
		using (UnityWebRequest www = new UnityWebRequest(url,"GET", dh, null))
		{
			yield return www.SendWebRequest();

			//yield return www;
			/*if (www.responseHeaders.Count > 0)
			{
				foreach (KeyValuePair<string, string> entry in www.responseHeaders)
				{
					Debug.Log(entry.Key + " = " + entry.Value);
				}
			}*/
			string[] Tags = www.url.Replace("\\", "/").Split("/".ToCharArray());

			if (Tags.Length > 0)
			{
				LatestTag = Tags[Tags.Length - 1];
				FoundUrl = www.url;

				double Latest = System.Math.Round(BuildFloat(LatestTag), 3);
				double Current = System.Math.Round(BuildFloat(EditorBuildVersion), 3);
				double CurrentWithOffset = System.Math.Round(Current + VersionOffset, 3);
				if (CurrentWithOffset < Latest)
				{
					Debug.Log("New version avaiable: " + Latest + "\n" + (Current + VersionOffset));
					GenericPopup.ShowPopup(GenericPopup.PopupTypes.TwoButton, "New version",
						"New version of Map Editor is avaiable.\nCurrent: " + EditorBuildVersion.ToLower() + TagString + "\t\tNew: " + LatestTag + "\nDo you want to download it now?",
						"Download", DownloadLatest,
						"Cancel", CancelDownload
						);
				}
				else
				{
					Debug.Log("Latest version: " + System.Math.Max(Latest, Current) + " " + EditorBuildTag);
				}

			}
		}
	}

	static string CleanBuildVersion(string tag)
	{
		return tag.ToLower().Replace(" ", "").Replace("-alpha", "").Replace("-beta", "");
	}

	static float BuildFloat(string tag)
	{
		float Found = 0.5f;
		string ToParse = CleanBuildVersion(tag).Replace("v", "");

		if (float.TryParse(ToParse, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Found))
		{
			return Found;
		}
		else
		{
			Debug.LogWarning("Wrong tag! Cant parse build version to float! Tag: " + ToParse);
			return 0;
		}
	}

	public void DownloadLatest()
	{
		Application.OpenURL(FoundUrl);
	}

	public void CancelDownload()
	{

	}
}
