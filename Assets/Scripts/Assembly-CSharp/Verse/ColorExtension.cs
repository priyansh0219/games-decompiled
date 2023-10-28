using UnityEngine;

namespace Verse
{
	public static class ColorExtension
	{
		public static Color ToOpaque(this Color c)
		{
			c.a = 1f;
			return c;
		}

		public static Color ToTransparent(this Color c, float transparency)
		{
			c.a = transparency;
			return c;
		}
	}
}
