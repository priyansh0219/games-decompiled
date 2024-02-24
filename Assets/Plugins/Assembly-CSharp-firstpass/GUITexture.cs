using System;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Use UnityEngine.UI.RawImage instead.")]
public sealed class GUITexture : RawImage
{
	[NonSerialized]
	public Rect pixelInset;
}
