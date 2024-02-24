using ProtoBuf.Meta;

public static class ProtobufExtensions
{
	private static int GetNextFieldNumber(MetaType metaType)
	{
		int num = 0;
		ValueMember[] fields = metaType.GetFields();
		foreach (ValueMember valueMember in fields)
		{
			if (valueMember.FieldNumber > num)
			{
				num = valueMember.FieldNumber;
			}
		}
		SubType[] subtypes = metaType.GetSubtypes();
		foreach (SubType subType in subtypes)
		{
			if (subType.FieldNumber > num)
			{
				num = subType.FieldNumber;
			}
		}
		return num + 1;
	}

	public static MetaType AddList(this MetaType metaType, string memberName)
	{
		int nextFieldNumber = GetNextFieldNumber(metaType);
		metaType.AddField(nextFieldNumber, memberName).OverwriteList = true;
		return metaType;
	}
}
