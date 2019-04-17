using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DotNetOpenAuth.OAuth2;
using Ozone;
using SFB;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Windows
{
	public class UploadMapPopUp : MonoBehaviour
	{
		public InputField BaseUrlInputField;
		public InputField MapFolderPathInputField;
		public Toggle RankedToggle;
		public Text ErrorText;
		public InputField UsernameInputField;
		public InputField PasswordInputField;

		public void SetMapFolderClicked()
		{
			var extensions = new[]
			{
				new ExtensionFilter("Scenario", "lua")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Select Scenario.lua of map to upload", EnvPaths.GetMapsPath(), extensions, false);

			if (paths.Length != 0 && paths[0] != null)
			{
				MapFolderPathInputField.text = Path.GetDirectoryName(paths[0]);
			}
		}

		public void UploadMap()
		{
			ErrorText.enabled = false;

			if (!CheckUserInput()) return;
			string uniqueTempPathInProject = GetUniqueTempPath() + ".zip";
			string tempPathForWrapperFolder = GetUniqueTempPath();
			DirectoryCopyWithSourceFolder(MapFolderPathInputField.text, tempPathForWrapperFolder, true);

			//Move Map Folder into a wrapper folder so map folder lies within the zip
			try
			{
				//Zip everything
				ZipUtil.CreateZipFromFolder(uniqueTempPathInProject, null, tempPathForWrapperFolder);
			}
			catch (Exception e)
			{
				OnError("Could not package map. Is the path you entered correct? Exception is:\n" + e.Message);
				return;
			}

			Debug.Log("Writing map to temp file: " + uniqueTempPathInProject);
			HttpClient httpClient = new HttpClient();
			MultipartFormDataContent form = new MultipartFormDataContent();
			form.Add(new StringContent("{\"isRanked\":" + (RankedToggle.isOn ? "true" : "false") + " }", Encoding.UTF8, "application/json"), "metadata");
			FileStream fileStream = new FileStream(uniqueTempPathInProject, FileMode.Open);
			StreamContent streamContent = new StreamContent(fileStream);
			streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
			form.Add(streamContent, "file", Path.GetFileName(uniqueTempPathInProject));
			try
			{
				var token = GetAccessTokenFromOwnAuthSvr();
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.AccessToken);
			}
			catch (Exception e)
			{
				OnError("Login failed, please check connection and login data. Exception is : \n" + e.Message);
				return;
			}

			httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			try
			{
				HttpResponseMessage res = httpClient.PostAsync(BaseUrlInputField.text + "/maps/upload", form).Result;
				try
				{
					res.EnsureSuccessStatusCode();
				}
				catch (Exception e)
				{
					OnError(
						"JAVA-API complains: \n" + res.Content.ReadAsStringAsync().Result + "\n\nException is: \n" + e.Message);
				}
			}
			catch (Exception e)
			{
				OnError("Sending failed. Maybe map is too big... Exception was: \n" + e.Message);
			}

			httpClient.Dispose();
			form.Dispose();
			GenericInfoPopup.ShowInfo("Uploaded Successful... Check the vault to see ;)");
		}

		private bool CheckUserInput()
		{
			if (string.IsNullOrEmpty(MapFolderPathInputField.text))
			{
				OnError("Please select a map to upload");
				return false;
			}
			if (string.IsNullOrEmpty(BaseUrlInputField.text))
			{
				OnError("Please enter an upload target e.g. https://api.faforever.com");
				return false;
			}
			if (string.IsNullOrEmpty(UsernameInputField.text))
			{
				OnError("Please enter a username");
				return false;
			}
			if (string.IsNullOrEmpty(PasswordInputField.text))
			{
				OnError("Please enter a password");
				return false;
			}

			return true;
		}

		private void OnError(string errorMessage)
		{
			ErrorText.enabled = true;
			var error = ErrorText.text = errorMessage;
			Debug.LogError(error);
		}

		private IAuthorizationState GetAccessTokenFromOwnAuthSvr()
		{
			var server = new AuthorizationServerDescription
			{
				TokenEndpoint = new Uri(BaseUrlInputField.text + "/oauth/token"),
				ProtocolVersion = ProtocolVersion.V20
			};

			var client = new UserAgentClient(server)
			{
				//Those are the credentials for Downlords-FAF-Client
				ClientIdentifier = "0db32c56-c43f-41f3-b875-322846a46dff",
				ClientCredentialApplicator = ClientCredentialApplicator.PostParameter("d020a6d7-d518-4509-b43e-b2f577b5d45e")
			};

			var token = client.ExchangeUserCredentialForToken(
				UsernameInputField.text, PasswordInputField.text);

			return token;
		}

		//Copied from StackOverFlow
		private static void DirectoryCopyWithSourceFolder(string sourceDirName, string destDirName, bool copySubDirs)
		{
			var newDest = Path.Combine(destDirName, Path.GetFileName(sourceDirName));
			Directory.CreateDirectory(newDest);
			DirectoryCopy(sourceDirName, newDest, copySubDirs);
		}

		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string tempPath = Path.Combine(destDirName, file.Name);
				file.CopyTo(tempPath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs)
			{
				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}
		static string GetUniqueTempPath()
		{
#if UNITY_EDITOR
			return FileUtil.GetUniqueTempPathInProject();
#else
			return Application.dataPath + "/Structure/MapUpload/Temp/TempFile";
#endif
		}

	}
}
