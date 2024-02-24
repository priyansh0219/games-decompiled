using System;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class EnumSerializer : IProtoSerializer
	{
		public struct EnumPair
		{
			public readonly object RawValue;

			public readonly Enum TypedValue;

			public readonly int WireValue;

			public EnumPair(int wireValue, object raw, Type type)
			{
				WireValue = wireValue;
				RawValue = raw;
				TypedValue = (Enum)Enum.ToObject(type, raw);
			}
		}

		private readonly Type enumType;

		private readonly EnumPair[] map;

		public Type ExpectedType => enumType;

		bool IProtoSerializer.RequiresOldValue => false;

		bool IProtoSerializer.ReturnsValue => true;

		public EnumSerializer(Type enumType, EnumPair[] map)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			this.enumType = enumType;
			this.map = map;
			if (map == null)
			{
				return;
			}
			for (int i = 1; i < map.Length; i++)
			{
				for (int j = 0; j < i; j++)
				{
					if (map[i].WireValue == map[j].WireValue && !object.Equals(map[i].RawValue, map[j].RawValue))
					{
						throw new ProtoException("Multiple enums with wire-value " + map[i].WireValue);
					}
					if (object.Equals(map[i].RawValue, map[j].RawValue) && map[i].WireValue != map[j].WireValue)
					{
						throw new ProtoException("Multiple enums with deserialized-value " + map[i].RawValue);
					}
				}
			}
		}

		private ProtoTypeCode GetTypeCode()
		{
			Type underlyingType = Helpers.GetUnderlyingType(enumType);
			if (underlyingType == null)
			{
				underlyingType = enumType;
			}
			return Helpers.GetTypeCode(underlyingType);
		}

		private int EnumToWire(object value)
		{
			switch (GetTypeCode())
			{
			case ProtoTypeCode.Byte:
				return (byte)value;
			case ProtoTypeCode.SByte:
				return (sbyte)value;
			case ProtoTypeCode.Int16:
				return (short)value;
			case ProtoTypeCode.Int32:
				return (int)value;
			case ProtoTypeCode.Int64:
				return (int)(long)value;
			case ProtoTypeCode.UInt16:
				return (ushort)value;
			case ProtoTypeCode.UInt32:
				return (int)(uint)value;
			case ProtoTypeCode.UInt64:
				return (int)(ulong)value;
			default:
				throw new InvalidOperationException();
			}
		}

		private object WireToEnum(int value)
		{
			switch (GetTypeCode())
			{
			case ProtoTypeCode.Byte:
				return Enum.ToObject(enumType, (byte)value);
			case ProtoTypeCode.SByte:
				return Enum.ToObject(enumType, (sbyte)value);
			case ProtoTypeCode.Int16:
				return Enum.ToObject(enumType, (short)value);
			case ProtoTypeCode.Int32:
				return Enum.ToObject(enumType, value);
			case ProtoTypeCode.Int64:
				return Enum.ToObject(enumType, (long)value);
			case ProtoTypeCode.UInt16:
				return Enum.ToObject(enumType, (ushort)value);
			case ProtoTypeCode.UInt32:
				return Enum.ToObject(enumType, (uint)value);
			case ProtoTypeCode.UInt64:
				return Enum.ToObject(enumType, (ulong)value);
			default:
				throw new InvalidOperationException();
			}
		}

		public object Read(object value, ProtoReader source)
		{
			int num = source.ReadInt32();
			if (map == null)
			{
				return WireToEnum(num);
			}
			for (int i = 0; i < map.Length; i++)
			{
				if (map[i].WireValue == num)
				{
					return map[i].TypedValue;
				}
			}
			source.ThrowEnumException(ExpectedType, num);
			return null;
		}

		public void Write(object value, ProtoWriter dest)
		{
			if (map == null)
			{
				ProtoWriter.WriteInt32(EnumToWire(value), dest);
				return;
			}
			for (int i = 0; i < map.Length; i++)
			{
				if (object.Equals(map[i].TypedValue, value))
				{
					ProtoWriter.WriteInt32(map[i].WireValue, dest);
					return;
				}
			}
			ProtoWriter.ThrowEnumException(dest, value);
		}

		void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			ProtoTypeCode typeCode = GetTypeCode();
			if (map == null)
			{
				ctx.LoadValue(valueFrom);
				ctx.ConvertToInt32(typeCode, uint32Overflow: false);
				ctx.EmitBasicWrite("WriteInt32", null);
				return;
			}
			using (Local local = ctx.GetLocalWithValue(ExpectedType, valueFrom))
			{
				CodeLabel label = ctx.DefineLabel();
				for (int i = 0; i < map.Length; i++)
				{
					CodeLabel label2 = ctx.DefineLabel();
					CodeLabel label3 = ctx.DefineLabel();
					ctx.LoadValue(local);
					WriteEnumValue(ctx, typeCode, map[i].RawValue);
					ctx.BranchIfEqual(label3, @short: true);
					ctx.Branch(label2, @short: true);
					ctx.MarkLabel(label3);
					ctx.LoadValue(map[i].WireValue);
					ctx.EmitBasicWrite("WriteInt32", null);
					ctx.Branch(label, @short: false);
					ctx.MarkLabel(label2);
				}
				ctx.LoadReaderWriter();
				ctx.LoadValue(local);
				ctx.CastToObject(ExpectedType);
				ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("ThrowEnumException"));
				ctx.MarkLabel(label);
			}
		}

		void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
		{
			ProtoTypeCode typeCode = GetTypeCode();
			if (map == null)
			{
				ctx.EmitBasicRead("ReadInt32", ctx.MapType(typeof(int)));
				ctx.ConvertFromInt32(typeCode, uint32Overflow: false);
				return;
			}
			int[] array = new int[map.Length];
			object[] array2 = new object[map.Length];
			for (int i = 0; i < map.Length; i++)
			{
				array[i] = map[i].WireValue;
				array2[i] = map[i].RawValue;
			}
			using (Local local2 = new Local(ctx, ExpectedType))
			{
				using (Local local = new Local(ctx, ctx.MapType(typeof(int))))
				{
					ctx.EmitBasicRead("ReadInt32", ctx.MapType(typeof(int)));
					ctx.StoreValue(local);
					CodeLabel codeLabel = ctx.DefineLabel();
					BasicList.NodeEnumerator enumerator = BasicList.GetContiguousGroups(array, array2).GetEnumerator();
					while (enumerator.MoveNext())
					{
						BasicList.Group group = (BasicList.Group)enumerator.Current;
						CodeLabel label = ctx.DefineLabel();
						int count = group.Items.Count;
						if (count == 1)
						{
							ctx.LoadValue(local);
							ctx.LoadValue(group.First);
							CodeLabel codeLabel2 = ctx.DefineLabel();
							ctx.BranchIfEqual(codeLabel2, @short: true);
							ctx.Branch(label, @short: false);
							WriteEnumValue(ctx, typeCode, codeLabel2, codeLabel, group.Items[0], local2);
						}
						else
						{
							ctx.LoadValue(local);
							ctx.LoadValue(group.First);
							ctx.Subtract();
							CodeLabel[] array3 = new CodeLabel[count];
							for (int j = 0; j < count; j++)
							{
								array3[j] = ctx.DefineLabel();
							}
							ctx.Switch(array3);
							ctx.Branch(label, @short: false);
							for (int k = 0; k < count; k++)
							{
								WriteEnumValue(ctx, typeCode, array3[k], codeLabel, group.Items[k], local2);
							}
						}
						ctx.MarkLabel(label);
					}
					ctx.LoadReaderWriter();
					ctx.LoadValue(ExpectedType);
					ctx.LoadValue(local);
					ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("ThrowEnumException"));
					ctx.MarkLabel(codeLabel);
					ctx.LoadValue(local2);
				}
			}
		}

		private static void WriteEnumValue(CompilerContext ctx, ProtoTypeCode typeCode, object value)
		{
			switch (typeCode)
			{
			case ProtoTypeCode.Byte:
				ctx.LoadValue((byte)value);
				break;
			case ProtoTypeCode.SByte:
				ctx.LoadValue((sbyte)value);
				break;
			case ProtoTypeCode.Int16:
				ctx.LoadValue((short)value);
				break;
			case ProtoTypeCode.Int32:
				ctx.LoadValue((int)value);
				break;
			case ProtoTypeCode.Int64:
				ctx.LoadValue((long)value);
				break;
			case ProtoTypeCode.UInt16:
				ctx.LoadValue((ushort)value);
				break;
			case ProtoTypeCode.UInt32:
				ctx.LoadValue((int)(uint)value);
				break;
			case ProtoTypeCode.UInt64:
				ctx.LoadValue((long)(ulong)value);
				break;
			default:
				throw new InvalidOperationException();
			}
		}

		private static void WriteEnumValue(CompilerContext ctx, ProtoTypeCode typeCode, CodeLabel handler, CodeLabel @continue, object value, Local local)
		{
			ctx.MarkLabel(handler);
			WriteEnumValue(ctx, typeCode, value);
			ctx.StoreValue(local);
			ctx.Branch(@continue, @short: false);
		}
	}
}
