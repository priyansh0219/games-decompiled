using System;
using Gendarme;
using UnityEngine;

public struct TooltipIcon : IEquatable<TooltipIcon>
{
	public Sprite sprite;

	public string text;

	[SuppressMessage("Gendarme.Rules.Maintainability", "VariableNamesShouldNotMatchFieldNamesRule")]
	public TooltipIcon(Sprite sprite, string text)
	{
		this.sprite = sprite;
		this.text = text;
	}

	public override int GetHashCode()
	{
		int num = 1039;
		num = 31 * num + sprite.GetHashCode();
		return 31 * num + text.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is TooltipIcon)
		{
			return Equals((TooltipIcon)obj);
		}
		return false;
	}

	public bool Equals(TooltipIcon other)
	{
		if (sprite == other.sprite)
		{
			return text == other.text;
		}
		return false;
	}

	public static bool operator ==(TooltipIcon lhs, TooltipIcon rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(TooltipIcon lhs, TooltipIcon rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override string ToString()
	{
		return $"[TooltipIcon icon:{sprite} text:\"{text}\"]";
	}
}
