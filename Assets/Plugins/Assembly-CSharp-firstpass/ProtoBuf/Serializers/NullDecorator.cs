using System;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class NullDecorator : ProtoDecoratorBase
	{
		private readonly Type expectedType;

		public const int Tag = 1;

		public override Type ExpectedType => expectedType;

		public override bool ReturnsValue => true;

		public override bool RequiresOldValue => true;

		public NullDecorator(TypeModel model, IProtoSerializer tail)
			: base(tail)
		{
			if (!tail.ReturnsValue)
			{
				throw new NotSupportedException("NullDecorator only supports implementations that return values");
			}
			Type type = tail.ExpectedType;
			if (Helpers.IsValueType(type))
			{
				expectedType = model.MapType(typeof(Nullable<>)).MakeGenericType(type);
			}
			else
			{
				expectedType = type;
			}
		}

		protected override void EmitRead(CompilerContext ctx, Local valueFrom)
		{
			using (Local local3 = ctx.GetLocalWithValue(expectedType, valueFrom))
			{
				using (Local local = new Local(ctx, ctx.MapType(typeof(SubItemToken))))
				{
					using (Local local2 = new Local(ctx, ctx.MapType(typeof(int))))
					{
						ctx.LoadReaderWriter();
						ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("StartSubItem"));
						ctx.StoreValue(local);
						CodeLabel label = ctx.DefineLabel();
						CodeLabel label2 = ctx.DefineLabel();
						CodeLabel label3 = ctx.DefineLabel();
						ctx.MarkLabel(label);
						ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof(int)));
						ctx.CopyValue();
						ctx.StoreValue(local2);
						ctx.LoadValue(1);
						ctx.BranchIfEqual(label2, @short: true);
						ctx.LoadValue(local2);
						ctx.LoadValue(1);
						ctx.BranchIfLess(label3, @short: false);
						ctx.LoadReaderWriter();
						ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("SkipField"));
						ctx.Branch(label, @short: true);
						ctx.MarkLabel(label2);
						if (Tail.RequiresOldValue)
						{
							if (Helpers.IsValueType(expectedType))
							{
								ctx.LoadAddress(local3, expectedType);
								ctx.EmitCall(expectedType.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
							}
							else
							{
								ctx.LoadValue(local3);
							}
						}
						Tail.EmitRead(ctx, null);
						if (Helpers.IsValueType(expectedType))
						{
							ctx.EmitCtor(expectedType, Tail.ExpectedType);
						}
						ctx.StoreValue(local3);
						ctx.Branch(label, @short: false);
						ctx.MarkLabel(label3);
						ctx.LoadValue(local);
						ctx.LoadReaderWriter();
						ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("EndSubItem"));
						ctx.LoadValue(local3);
					}
				}
			}
		}

		protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			using (Local local2 = ctx.GetLocalWithValue(expectedType, valueFrom))
			{
				using (Local local = new Local(ctx, ctx.MapType(typeof(SubItemToken))))
				{
					ctx.LoadNullRef();
					ctx.LoadReaderWriter();
					ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("StartSubItem"));
					ctx.StoreValue(local);
					if (Helpers.IsValueType(expectedType))
					{
						ctx.LoadAddress(local2, expectedType);
						ctx.LoadValue(expectedType.GetProperty("HasValue"));
					}
					else
					{
						ctx.LoadValue(local2);
					}
					CodeLabel label = ctx.DefineLabel();
					ctx.BranchIfFalse(label, @short: false);
					if (Helpers.IsValueType(expectedType))
					{
						ctx.LoadAddress(local2, expectedType);
						ctx.EmitCall(expectedType.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
					}
					else
					{
						ctx.LoadValue(local2);
					}
					Tail.EmitWrite(ctx, null);
					ctx.MarkLabel(label);
					ctx.LoadValue(local);
					ctx.LoadReaderWriter();
					ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("EndSubItem"));
				}
			}
		}

		public override object Read(object value, ProtoReader source)
		{
			SubItemToken token = ProtoReader.StartSubItem(source);
			int num;
			while ((num = source.ReadFieldHeader()) > 0)
			{
				if (num == 1)
				{
					value = Tail.Read(value, source);
				}
				else
				{
					source.SkipField();
				}
			}
			ProtoReader.EndSubItem(token, source);
			return value;
		}

		public override void Write(object value, ProtoWriter dest)
		{
			SubItemToken token = ProtoWriter.StartSubItem(null, dest);
			if (value != null)
			{
				Tail.Write(value, dest);
			}
			ProtoWriter.EndSubItem(token, dest);
		}
	}
}
