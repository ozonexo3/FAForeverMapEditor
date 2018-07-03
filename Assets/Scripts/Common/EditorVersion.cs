using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EditorVersion : MonoBehaviour
{

	public const string EditorBuildVersion = "v0.601-Alpha";
	public const float VersionOffset = 0f; // Release
	//public const float VersionOffset = -0.001f; // Prerelease
	//public const float VersionOffset = 0.523f - 0.600f; // Prerelease v0600
	public static string LatestTag = "";
	public static string FoundUrl;
	public bool SearchForNew = false;

	void Start()
	{
		GetComponent<Text>().text = EditorBuildVersion;
		if(SearchForNew)
			StartCoroutine(FindLatest());
	}

	public string url = "https://github.com/ozonexo3/FAForeverMapEditor/releases/latest";
	IEnumerator FindLatest()
	{
		using (WWW www = new WWW(url))
		{
			yield return www;
			if (www.responseHeaders.Count > 0)
			{
				/*
				foreach (KeyValuePair<string, string> entry in www.responseHeaders)
				{
					Debug.Log(entry.Key + " = " + entry.Value);
				}
				*/
			}
			string[] Tags = www.url.Replace("\\", "/").Split("/".ToCharArray());

			if (Tags.Length > 0)
			{
				LatestTag = Tags[Tags.Length - 1];
				FoundUrl = www.url;


				float Latest = BuildFloat(LatestTag);
				float Current = BuildFloat(EditorBuildVersion) + VersionOffset;
				if (Current < Latest)
				{
					Debug.Log("New version avaiable: " + Latest);
					GenericPopup.ShowPopup(GenericPopup.PopupTypes.TwoButton, "New version",
						"New version of Map Editor is avaiable.\nCurrent: " + EditorBuildVersion.ToLower() + "\t\tNew: " + LatestTag + "\nDo you want to download it now?",
						"Download", DownloadLatest,
						"Cancel", CancelDownload
						);
				}
				else
				{
					Debug.Log("Latest version: " + Mathf.Max(Latest, Current));
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
