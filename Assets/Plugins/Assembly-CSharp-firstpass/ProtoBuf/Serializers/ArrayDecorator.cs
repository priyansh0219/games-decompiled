using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class ArrayDecorator : ProtoDecoratorBase
	{
		private readonly int fieldNumber;

		private const byte OPTIONS_WritePacked = 1;

		private const byte OPTIONS_OverwriteList = 2;

		private const byte OPTIONS_SupportNull = 4;

		private readonly byte options;

		private readonly WireType packedWireType;

		private readonly Type arrayType;

		private readonly Type itemType;

		public override Type ExpectedType => arrayType;

		public override bool RequiresOldValue => AppendToCollection;

		public override bool ReturnsValue => true;

		private bool AppendToCollection => (options & 2) == 0;

		private bool SupportNull => (options & 4) != 0;

		public ArrayDecorator(TypeModel model, IProtoSerializer tail, int fieldNumber, bool writePacked, WireType packedWireType, Type arrayType, bool overwriteList, bool supportNull)
			: base(tail)
		{
			itemType = arrayType.GetElementType();
			if (!supportNull)
			{
				if ((object)Helpers.GetUnderlyingType(itemType) == null)
				{
					_ = itemType;
				}
			}
			else
			{
				_ = itemType;
			}
			if ((writePacked || packedWireType != WireType.None) && fieldNumber <= 0)
			{
				throw new ArgumentOutOfRangeException("fieldNumber");
			}
			if (!ListDecorator.CanPack(packedWireType))
			{
				if (writePacked)
				{
					throw new InvalidOperationException("Only simple data-types can use packed encoding");
				}
				packedWireType = WireType.None;
			}
			this.fieldNumber = fieldNumber;
			this.packedWireType = packedWireType;
			if (writePacked)
			{
				options |= 1;
			}
			if (overwriteList)
			{
				options |= 2;
			}
			if (supportNull)
			{
				options |= 4;
			}
			this.arrayType = arrayType;
		}

		protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			using (Local local = ctx.GetLocalWithValue(arrayType, valueFrom))
			{
				using (Local i = new Local(ctx, ctx.MapType(typeof(int))))
				{
					bool flag = (options & 1) != 0;
					bool flag2 = flag && CanUsePackedPrefix();
					using (Local local2 = ((flag && !flag2) ? new Local(ctx, ctx.MapType(typeof(SubItemToken))) : null))
					{
						Type type = ctx.MapType(typeof(ProtoWriter));
						if (flag)
						{
							ctx.LoadValue(fieldNumber);
							ctx.LoadValue(2);
							ctx.LoadReaderWriter();
							ctx.EmitCall(type.GetMethod("WriteFieldHeader"));
							if (flag2)
							{
								ctx.LoadLength(local, zeroIfNull: false);
								ctx.LoadValue((int)packedWireType);
								ctx.LoadReaderWriter();
								ctx.EmitCall(type.GetMethod("WritePackedPrefix"));
							}
							else
							{
								ctx.LoadValue(local);
								ctx.LoadReaderWriter();
								ctx.EmitCall(type.GetMethod("StartSubItem"));
								ctx.StoreValue(local2);
							}
							ctx.LoadValue(fieldNumber);
							ctx.LoadReaderWriter();
							ctx.EmitCall(type.GetMethod("SetPackedField"));
						}
						EmitWriteArrayLoop(ctx, i, local);
						if (flag)
						{
							if (flag2)
							{
								ctx.LoadValue(fieldNumber);
								ctx.LoadReaderWriter();
								ctx.EmitCall(type.GetMethod("ClearPackedField"));
							}
							else
							{
								ctx.LoadValue(local2);
								ctx.LoadReaderWriter();
								ctx.EmitCall(type.GetMethod("EndSubItem"));
							}
						}
					}
				}
			}
		}

		private bool CanUsePackedPrefix()
		{
			return CanUsePackedPrefix(packedWireType, itemType);
		}

		internal static bool CanUsePackedPrefix(WireType packedWireType, Type itemType)
		{
			if (packedWireType != WireType.Fixed64 && packedWireType != WireType.Fixed32)
			{
				return false;
			}
			if (!Helpers.IsValueType(itemType))
			{
				return false;
			}
			return Helpers.GetUnderlyingType(itemType) == null;
		}

		private void EmitWriteArrayLoop(CompilerContext ctx, Local i, Local arr)
		{
			ctx.LoadValue(0);
			ctx.StoreValue(i);
			CodeLabel label = ctx.DefineLabel();
			CodeLabel label2 = ctx.DefineLabel();
			ctx.Branch(label, @short: false);
			ctx.MarkLabel(label2);
			ctx.LoadArrayValue(arr, i);
			if (SupportNull)
			{
				Tail.EmitWrite(ctx, null);
			}
			else
			{
				ctx.WriteNullCheckedTail(itemType, Tail, null);
			}
			ctx.LoadValue(i);
			ctx.LoadValue(1);
			ctx.Add();
			ctx.StoreValue(i);
			ctx.MarkLabel(label);
			ctx.LoadValue(i);
			ctx.LoadLength(arr, zeroIfNull: false);
			ctx.BranchIfLess(label2, @short: false);
		}

		public override void Write(object value, ProtoWriter dest)
		{
			IList list = (IList)value;
			int count = list.Count;
			bool flag = (options & 1) != 0;
			bool flag2 = flag && CanUsePackedPrefix();
			SubItemToken token;
			if (flag)
			{
				ProtoWriter.WriteFieldHeader(fieldNumber, WireType.String, dest);
				if (flag2)
				{
					ProtoWriter.WritePackedPrefix(list.Count, packedWireType, dest);
					token = default(SubItemToken);
				}
				else
				{
					token = ProtoWriter.StartSubItem(value, dest);
				}
				ProtoWriter.SetPackedField(fieldNumber, dest);
			}
			else
			{
				token = default(SubItemToken);
			}
			bool flag3 = !SupportNull;
			for (int i = 0; i < count; i++)
			{
				object obj = list[i];
				if (flag3 && obj == null)
				{
					throw new NullReferenceException();
				}
				Tail.Write(obj, dest);
			}
			if (flag)
			{
				if (flag2)
				{
					ProtoWriter.ClearPackedField(fieldNumber, dest);
				}
				else
				{
					ProtoWriter.EndSubItem(token, dest);
				}
			}
		}

		public override object Read(object value, ProtoReader source)
		{
			int field = source.FieldNumber;
			BasicList basicList = new BasicList();
			if (packedWireType != WireType.None && source.WireType == WireType.String)
			{
				SubItemToken token = ProtoReader.StartSubItem(source);
				while (ProtoReader.HasSubValue(packedWireType, source))
				{
					basicList.Add(Tail.Read(null, source));
				}
				ProtoReader.EndSubItem(token, source);
			}
			else
			{
				do
				{
					basicList.Add(Tail.Read(null, source));
				}
				while (source.TryReadFieldHeader(field));
			}
			int num = (AppendToCollection ? ((value != null) ? ((Array)value).Length : 0) : 0);
			Array array = Array.CreateInstance(itemType, num + basicList.Count);
			if (num != 0)
			{
				((Array)value).CopyTo(array, 0);
			}
			basicList.CopyTo(array, num);
			return array;
		}

		protected override void EmitRead(CompilerContext ctx, Local valueFrom)
		{
			Type type = ctx.MapType(typeof(List<>)).MakeGenericType(itemType);
			Type expectedType = ExpectedType;
			using (Local local2 = (AppendToCollection ? ctx.GetLocalWithValue(expectedType, valueFrom) : null))
			{
				using (Local local4 = new Local(ctx, expectedType))
				{
					using (Local local = new Local(ctx, type))
					{
						ctx.EmitCtor(type);
						ctx.StoreValue(local);
						ListDecorator.EmitReadList(ctx, local, Tail, type.GetMethod("Add"), packedWireType, castListForAdd: false);
						using (Local local3 = (AppendToCollection ? new Local(ctx, ctx.MapType(typeof(int))) : null))
						{
							Type[] array = new Type[2]
							{
								ctx.MapType(typeof(Array)),
								ctx.MapType(typeof(int))
							};
							if (AppendToCollection)
							{
								ctx.LoadLength(local2, zeroIfNull: true);
								ctx.CopyValue();
								ctx.StoreValue(local3);
								ctx.LoadAddress(local, type);
								ctx.LoadValue(type.GetProperty("Count"));
								ctx.Add();
								ctx.CreateArray(itemType, null);
								ctx.StoreValue(local4);
								ctx.LoadValue(local3);
								CodeLabel label = ctx.DefineLabel();
								ctx.BranchIfFalse(label, @short: true);
								ctx.LoadValue(local2);
								ctx.LoadValue(local4);
								ctx.LoadValue(0);
								ctx.EmitCall(expectedType.GetMethod("CopyTo", array));
								ctx.MarkLabel(label);
								ctx.LoadValue(local);
								ctx.LoadValue(local4);
								ctx.LoadValue(local3);
							}
							else
							{
								ctx.LoadAddress(local, type);
								ctx.LoadValue(type.GetProperty("Count"));
								ctx.CreateArray(itemType, null);
								ctx.StoreValue(local4);
								ctx.LoadAddress(local, type);
								ctx.LoadValue(local4);
								ctx.LoadValue(0);
							}
							array[0] = expectedType;
							MethodInfo method = type.GetMethod("CopyTo", array);
							if (method == null)
							{
								array[1] = ctx.MapType(typeof(Array));
								method = type.GetMethod("CopyTo", array);
							}
							ctx.EmitCall(method);
						}
						ctx.LoadValue(local4);
					}
				}
			}
		}
	}
}
