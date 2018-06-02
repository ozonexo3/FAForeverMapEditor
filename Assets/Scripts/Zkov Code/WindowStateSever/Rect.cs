using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace WindowStateSever
{
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct Rect
    {
        public int Left; // x position of upper-left corner
        public int Top; // y position of upper-left corner
        public int Right; // x position of lower-right corner
        public int Bottom; // y position of lower-right corner

        public Vector2Int GetSize()
        {
            return new Vector2Int(this.Right - this.Left, this.Bottom - this.Top);
        }

        public UnityEngine.Rect ToURect()
        {
            return UnityEngine.Rect.MinMaxRect(
                Left,
                Top,
                Right,
                Bottom
            );
        }

        public Vector4 ToVector4()
        {
            return new Vector4(
                Left,
                Bottom,
                Right,
                Top
            );
        }

        public static Rect FromURect(UnityEngine.Rect rect)
        {
            return new Rect()
            {
                Left = (int)(rect.xMin),
                Top = (int)(rect.yMin),
                Right = (int)(rect.xMax),
                Bottom = (int)(rect.yMax)
            };
        }

        public override string ToString()
        {
            return this.ToURect().ToString();
        }
    }
}