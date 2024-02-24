using System;

[AttributeUsage(AttributeTargets.Field)]
public class AssertLocalizationAttribute : Attribute
{
	[Flags]
	public enum Options
	{
		None = 0,
		AllowEmptyString = 1
	}

	public readonly Options options;

	public readonly int formatItems;

	public bool allowEmptyString => HasOption(Options.AllowEmptyString);

	public AssertLocalizationAttribute()
	{
	}

	public AssertLocalizationAttribute(int formatItems)
	{
		this.formatItems = formatItems;
	}

	public AssertLocalizationAttribute(Options options)
	{
		this.options = options;
	}

	public AssertLocalizationAttribute(Options options, int formatItems)
	{
		this.options = options;
		this.formatItems = formatItems;
	}

	public bool HasOption(Options option)
	{
		return (options & option) != 0;
	}
}
