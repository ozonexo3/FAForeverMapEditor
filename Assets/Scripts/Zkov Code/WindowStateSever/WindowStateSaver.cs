using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace WindowStateSever
{
    public static class WindowStateSaver
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect);
        
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        
        [DllImport("USER32.DLL")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("USER32.DLL")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);
        
        private const string playerPrefsKey = "WindowModelData";

        private static string WindowName
        {
            get { return Application.productName; }
        }

        public static bool Save()
        {
            WindowModel wm = new WindowModel();

            IntPtr windowPtr = FindWindow(null, WindowName);

            Rect windowrect = new Rect();
            if (!GetWindowRect(windowPtr, out windowrect))
            {
                Debug.LogFormat("Save window is fail: {0}", "");
                return false;
            }
            wm.WindowRect = windowrect.ToURect();

            long styles = GetWindowLong(windowPtr, WindowLong.GWL_STYLE);
            if (styles == 0)
            {
                Debug.LogFormat("Save window is fail: {0}", "");
                return false;
            }
            wm.WindowStyles = styles;

            string jsonStr = JsonUtility.ToJson(wm);
            Debug.LogFormat("Saving values: {0}", jsonStr);
            PlayerPrefs.SetString(playerPrefsKey, jsonStr);

            return true;
        }

        public static bool Restore()
        {
            if (!PlayerPrefs.HasKey(playerPrefsKey))
            {
                Debug.LogFormat("Restore window is fail: {0}", "");
                return false;
            }

            string jsonStr = PlayerPrefs.GetString(playerPrefsKey);

            Debug.LogFormat("Restoring values: {0}", jsonStr);

            WindowModel wm = JsonUtility.FromJson<WindowModel>(jsonStr);
            
            if (wm == null)
            {
                Debug.LogFormat("Restore window is fail: {0}", "Saved data is corupted");
                return false;
            }

            IntPtr windowPtr = FindWindow(null, WindowName);

            if (SetWindowLong(windowPtr, WindowLong.GWL_STYLE, wm.WindowStyles) == 0)
            {
                Debug.LogFormat("Restore window is fail: {0}", "Can`t set window styles");
                return false;
            }

            if (SetWindowPos(windowPtr, 0, (int) wm.WindowRect.x, (int) wm.WindowRect.y, (int) wm.WindowRect.width, (int) wm.WindowRect.height, 0x0040).ToInt32() == 0)
            {
                Debug.LogFormat("Restore window is fail: {0}", "Can`t set window position");
                return false;
            }
            
            return true;
        }
    }
}