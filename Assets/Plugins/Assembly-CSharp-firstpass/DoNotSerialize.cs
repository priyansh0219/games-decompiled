using System;

[Obsolete("Use System.NonSerialized instead.")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
public class DoNotSerialize : Attribute
{
}
