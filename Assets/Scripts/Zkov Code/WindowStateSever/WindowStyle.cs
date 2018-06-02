using System;
using System.Linq;
using System.Reflection;

namespace WindowStateSever
{
    [Serializable]
    public static class WindowStyle
    {
        public static long WS_BORDER = 0x00800000L;
        public static long WS_CAPTION = 0x00C00000L;
        public static long WS_CHILD = 0x40000000L;
        public static long WS_CHILDWINDOW = 0x40000000L;
        public static long WS_CLIPCHILDREN = 0x02000000L;
        public static long WS_CLIPSIBLINGS = 0x04000000L;
        public static long WS_DISABLED = 0x08000000L;
        public static long WS_DLGFRAME = 0x00400000L;
        public static long WS_GROUP = 0x00020000L;
        public static long WS_HSCROLL = 0x00100000L;
        public static long WS_ICONIC = 0x20000000L;
        public static long WS_MAXIMIZE = 0x01000000L;
        public static long WS_MAXIMIZEBOX = 0x00010000L;
        public static long WS_MINIMIZE = 0x20000000L;
        public static long WS_MINIMIZEBOX = 0x00020000L;
        public static long WS_OVERLAPPED = 0x00000000L;
        public static long WS_POPUP = 0x80000000L;
        public static long WS_SIZEBOX = 0x00040000L;
        public static long WS_SYSMENU = 0x00080000L;
        public static long WS_TABSTOP = 0x00010000L;
        public static long WS_THICKFRAME = 0x00040000L;
        public static long WS_TILED = 0x00000000L;
        public static long WS_VISIBLE = 0x10000000L;
        public static long WS_VSCROLL = 0x00200000L;
        public static long WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public static long WS_TILEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public static long WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU);

        public static string[] Parce(long styles)
        {
            var fieldInfos = typeof(WindowStyle).GetFields(BindingFlags.Public | BindingFlags.Static);
            var array = fieldInfos.Where(info => (styles & (long) info.GetValue(null)) != 0).ToArray();
            return array.Select(info => info.Name).ToArray();
        }
    }
}