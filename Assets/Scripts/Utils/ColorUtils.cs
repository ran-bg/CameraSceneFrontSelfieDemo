using UnityEngine;

namespace Language
{
    public static class ColorPalette
    {
        public static Color GetGyroOk()
        {
            return new Color(
                92f / 255f,
                198f / 255f,
                137f / 255f);
        }

        public static Color GetGyroNg()
        {
            return new Color(
                238f / 255f,
                82f / 255f,
                82f / 255f);
        }
    }
}