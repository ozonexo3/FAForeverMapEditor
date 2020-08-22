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
using Ozone.UI;

namespace UI.Windows
{
	public class UploadMapPopUp : MonoBehaviour
	{
		//public InputField BaseUrlInputField;
		public UiTextField MapFolderPathInputField;
		public Toggle RankedToggle;
		public Text ErrorText;
		public UiTextField UsernameInputField;
		public UiTextField PasswordInputField;

		public UploadConfigData Config = new UploadConfigData();

		public class UploadConfigData
		{
			public string ClientIdentifier = "";
			public string ClientCredentialApplicator = "";
			public string ApiUrl = "";

			static string filePath {
				get {
					return MapLuaParser.GetDataPath() + "/Structure/MapUpload/config.txt";
				}
			}

			public void TryLoad()
			{
				string PathToSave = filePath;
				if (File.Exists(PathToSave))
				{
					string data = File.ReadAllText(PathToSave);

					UploadConfigData ucd = JsonUtility.FromJson<UploadConfigData>(data);
					ClientIdentifier = ucd.ClientIdentifier;
					ClientCredentialApplicator = ucd.ClientCredentialApplicator;
					ApiUrl = ucd.ApiUrl;
				}
				else
				{
					Debug.LogError("Map Upload Config file not found: " + PathToSave);
					SetDefaults();
				}
			}

			public void Save()
			{
				string PathToSave = filePath;
				string data = JsonUtility.ToJson(this, true);
				if (!Directory.Exists(Path.GetDirectoryName(PathToSave)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(PathToSave));
				}
				File.WriteAllText(PathToSave, data);
				Debug.Log("Config saved: " + PathToSave);
			}

			public void SetDefaults()
			{
				ClientIdentifier = "0db32c56-c43f-41f3-b875-322846a46dff";
				ClientCredentialApplicator = "d020a6d7-d518-4509-b43e-b2f577b5d45e";
				ApiUrl = "https://api.faforever.com";
			}

		}

		void Awake()
		{
			//Create default file
			//Config.SetDefaults();
			//Config.Save();

			if (Config == null)
				Config = new UploadConfigData();
			Config.TryLoad();
		}

		public void SetMapFolderClicked()
		{
			var extensions = new[]
			{
				new ExtensionFilter("Scenario", "lua")
			};

			var paths = StandaloneFileBrowser.OpenFilePanel("Select Scenario.lua of map to upload", EnvPaths.GetMapsPath(), extensions, false);

			if (paths.Length != 0 && paths[0] != null)
			{
				MapFolderPathInputField.SetValue(Path.GetDirectoryName(paths[0]));
			}
		}

		public void UploadMap()
		{
			ErrorText.enabled = false;

			if (!CheckUserInput()) return;
			string uniqueTempPathInProject = GetUniqueTempPath() + ".zip";
			string tempPathForWrapperFolder = GetUniqueTempPath();
			FileDirectory.DirectoryCopyWithSourceFolder(MapFolderPathInputField.text, tempPathForWrapperFolder, true);

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
				HttpResponseMessage res = httpClient.PostAsync(Config.ApiUrl + "/maps/upload", form).Result;
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
			if (string.IsNullOrEmpty(Config.ApiUrl))
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
				TokenEndpoint = new Uri(Config.ApiUrl + "/oauth/token"),
				ProtocolVersion = ProtocolVersion.V20
			};

			var client = new UserAgentClient(server)
			{
				//Those are the credentials for Downlords-FAF-Client
				ClientIdentifier = Config.ClientIdentifier,
				ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(Config.ClientCredentialApplicator)
			};

			var token = client.ExchangeUserCredentialForToken(
				UsernameInputField.text, PasswordInputField.text);

			return token;
		}

		private static string GetUniqueTempPath()
		{
			string BackupPath = Application.dataPath;
#if UNITY_EDITOR
			BackupPath = BackupPath.Replace("Assets/", "TempMapUpload/");
#endif
			return BackupPath + "/Structure/MapUpload/Temp/TempFile_" + MapLuaParser.GetUniqueId();

		}

	}
}
