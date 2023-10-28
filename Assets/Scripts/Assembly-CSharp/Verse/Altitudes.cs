using UnityEngine;

namespace Verse
{
	public static class Altitudes
	{
		private const int NumAltitudeLayers = 36;

		private static readonly float[] Alts;

		private const float LayerSpacing = 0.4054054f;

		public const float AltInc = 3f / 74f;

		public static readonly Vector3 AltIncVect;

		static Altitudes()
		{
			Alts = new float[36];
			AltIncVect = new Vector3(0f, 3f / 74f, 0f);
			for (int i = 0; i < 36; i++)
			{
				Alts[i] = (float)i * 0.4054054f;
			}
		}

		public static float AltitudeFor(this AltitudeLayer alt)
		{
			return Alts[(uint)alt];
		}

		public static float AltitudeFor(this AltitudeLayer alt, float incOffset)
		{
			return alt.AltitudeFor() + incOffset * (3f / 74f);
		}
	}
}
