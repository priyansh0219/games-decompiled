using UnityEngine;

namespace UWE
{
	public struct Color32ARGB
	{
		public byte a;

		public byte r;

		public byte g;

		public byte b;

		public byte this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return a;
				case 1:
					return r;
				case 2:
					return g;
				case 3:
					return b;
				default:
					return 0;
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					a = value;
					break;
				case 1:
					r = value;
					break;
				case 2:
					g = value;
					break;
				case 3:
					b = value;
					break;
				}
			}
		}

		public Color32ARGB(byte a, byte r, byte g, byte b)
		{
			this.a = a;
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public static implicit operator Color32(Color32ARGB argb)
		{
			return new Color32(argb.r, argb.g, argb.b, argb.a);
		}
	}
}
