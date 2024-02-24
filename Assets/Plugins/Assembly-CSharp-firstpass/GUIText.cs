using System;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Use UnityEngine.UI.Text instead.")]
public sealed class GUIText : Text
{
	[NonSerialized]
	public TextAnchor anchor;

	[NonSerialized]
	public Vector2 pixelOffset;

	[NonSerialized]
	public bool richText;

	[NonSerialized]
	public int tabSize;
}
