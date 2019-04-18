using UnityEngine;

namespace WindowStateSever
{
    public class WindowStateSaverMonoBehaviour : MonoBehaviour
    {
        private void Start()
        {
			DontDestroyOnLoad(gameObject);
			Screen.SetResolution(1280, 768, false);
			Screen.fullScreenMode = FullScreenMode.Windowed;
			

			WindowStateSaver.Restore();
        }

        private void OnApplicationQuit()
        {
            WindowStateSaver.Save();
        }
    }
}