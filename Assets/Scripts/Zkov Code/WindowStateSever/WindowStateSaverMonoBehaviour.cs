using UnityEngine;

namespace WindowStateSever
{
    public class WindowStateSaverMonoBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            WindowStateSaver.Restore();
        }

        private void OnApplicationQuit()
        {
            WindowStateSaver.Save();
        }
    }
}