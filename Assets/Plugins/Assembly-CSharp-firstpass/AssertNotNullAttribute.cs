using System;

[AttributeUsage(AttributeTargets.Field)]
public class AssertNotNullAttribute : Attribute
{
	[Flags]
	public enum Options
	{
		None = 0,
		IgnorePrefabs = 1,
		IgnoreScenes = 2,
		AllowEmptyCollection = 4
	}

	public readonly Options options;

	public bool ignorePrefabs => HasOption(Options.IgnorePrefabs);

	public bool ignoreScenes => HasOption(Options.IgnoreScenes);

	public bool allowEmptyCollection => HasOption(Options.AllowEmptyCollection);

	public AssertNotNullAttribute()
	{
	}

	public AssertNotNullAttribute(Options options)
	{
		this.options = options;
	}

	public bool HasOption(Options option)
	{
		return (options & option) != 0;
	}
}
