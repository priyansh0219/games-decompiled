using System;
using System.Text;

public static class StringBuilderExtensions
{
	public static int LastIndexOfAny(this StringBuilder sb, char[] anyOf, int startIndex)
	{
		if (anyOf == null)
		{
			throw new ArgumentNullException("anyOf");
		}
		if (startIndex < 0 || startIndex >= sb.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		for (int num = startIndex; num >= 0; num--)
		{
			char c = sb[num];
			for (int i = 0; i < anyOf.Length; i++)
			{
				if (c == anyOf[i])
				{
					return num;
				}
			}
		}
		return -1;
	}

	public static int LastIndexOfAny(this StringBuilder sb, char[] anyOf, int startIndex, char requiredNextCharacter)
	{
		if (anyOf == null)
		{
			throw new ArgumentNullException("anyOf");
		}
		if (startIndex < 0 || startIndex >= sb.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		for (int num = startIndex; num >= 0; num--)
		{
			char c = sb[num];
			for (int i = 0; i < anyOf.Length; i++)
			{
				if (c == anyOf[i])
				{
					if (num + 1 >= sb.Length)
					{
						return num;
					}
					if (sb[num + 1] == requiredNextCharacter)
					{
						return num;
					}
				}
			}
		}
		return -1;
	}

	public static int LastIndexOf(this StringBuilder sb, char value, int startIndex)
	{
		if (startIndex < 0 || startIndex >= sb.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		for (int num = startIndex; num >= 0; num--)
		{
			if (sb[num] == value)
			{
				return num;
			}
		}
		return -1;
	}

	public static void Substring(StringBuilder input, int start, int length, StringBuilder output)
	{
		if (start < 0 || start >= input.Length || length < 0 || input == output)
		{
			output.Length = 0;
			return;
		}
		length = Math.Min(length, input.Length - start);
		output.Length = length;
		for (int i = 0; i < length; i++)
		{
			output[i] = input[start + i];
		}
	}

	public static StringBuilder AppendSize(this StringBuilder sb, long size)
	{
		MathExtensions.GetSizeRank(size, out var divisor, out var metric);
		return sb.Append((double)size / divisor).Append(' ').Append(metric);
	}
}
