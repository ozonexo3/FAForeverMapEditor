using System.Linq;
using UnityEngine;

namespace ZkovCode.Utils
{
    public static class RectUtils
    {
        public static Vector2[] GetCoroners(Rect rect)
        {
            return new Vector2[4]{
                new Vector2(rect.xMin,rect.yMin),
                new Vector2(rect.xMin,rect.yMax),
                new Vector2(rect.xMax,rect.yMax),
                new Vector2(rect.xMax,rect.yMin)
            };
        }
        
        public static bool IsCrossed(Rect rectL, Rect rectR)
        {
            Vector2[] coroners = GetCoroners(rectR);
//            foreach (var coroner in coroners)
//            {
//                if (!rectL.Contains(coroner))
//                {
//                    return true;
//                }
//            }

//            return false;
            return coroners.Any(coroner => !rectL.Contains(coroner));
        }

        public static Rect Cross(Rect rectL, Rect rectR)
        {
            Rect rect = Rect.MinMaxRect(
                Mathf.Max(rectL.xMin, rectR.xMin),
                Mathf.Max(rectL.yMin, rectR.yMin),
                Mathf.Min(rectL.xMax, rectR.xMax),
                Mathf.Min(rectL.yMax, rectR.yMax)
                );
            return rect;
        }
    }
}