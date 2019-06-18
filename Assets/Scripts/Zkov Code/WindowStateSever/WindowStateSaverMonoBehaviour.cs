using UnityEngine;

namespace WindowStateSever
{
    public class WindowStateSaverMonoBehaviour : MonoBehaviour
    {
        public void Init()
        {
			DontDestroyOnLoad(gameObject);
			//Screen.SetResolution(1280, 768, false);
			//Screen.fullScreenMode = FullScreenMode.Windowed;
			if (Screen.fullScreen)
				Screen.fullScreen = false;
			
			if(!Screen.fullScreen)
				WindowStateSaver.Restore();
        }

        private void OnApplicationQuit()
        {
			if (!Screen.fullScreen)
				WindowStateSaver.Save();
        }
    }
}