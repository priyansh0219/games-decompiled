using System;
using System.Reflection;
using ProtoBuf.Compiler;

namespace ProtoBuf.Serializers
{
	internal sealed class FieldDecorator : ProtoDecoratorBase
	{
		private readonly FieldInfo field;

		private readonly Type forType;

		public override Type ExpectedType => forType;

		public override bool RequiresOldValue => true;

		public override bool ReturnsValue => false;

		public FieldDecorator(Type forType, FieldInfo field, IProtoSerializer tail)
			: base(tail)
		{
			this.forType = forType;
			this.field = field;
		}

		public override void Write(object value, ProtoWriter dest)
		{
			value = field.GetValue(value);
			if (value != null)
			{
				Tail.Write(value, dest);
			}
		}

		public override object Read(object value, ProtoReader source)
		{
			object obj = Tail.Read(Tail.RequiresOldValue ? field.GetValue(value) : null, source);
			if (obj != null)
			{
				field.SetValue(value, obj);
			}
			return null;
		}

		protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			ctx.LoadAddress(valueFrom, ExpectedType);
			ctx.LoadValue(field);
			ctx.WriteNullCheckedTail(field.FieldType, Tail, null);
		}

		protected override void EmitRead(CompilerContext ctx, Local valueFrom)
		{
			using (Local local = ctx.GetLocalWithValue(ExpectedType, valueFrom))
			{
				if (Tail.RequiresOldValue)
				{
					ctx.LoadAddress(local, ExpectedType);
					ctx.LoadValue(field);
				}
				ctx.ReadNullCheckedTail(field.FieldType, Tail, null);
				if (!Tail.ReturnsValue)
				{
					return;
				}
				using (Local local2 = new Local(ctx, field.FieldType))
				{
					ctx.StoreValue(local2);
					if (Helpers.IsValueType(field.FieldType))
					{
						ctx.LoadAddress(local, ExpectedType);
						ctx.LoadValue(local2);
						ctx.StoreValue(field);
						return;
					}
					CodeLabel label = ctx.DefineLabel();
					ctx.LoadValue(local2);
					ctx.BranchIfFalse(label, @short: true);
					ctx.LoadAddress(local, ExpectedType);
					ctx.LoadValue(local2);
					ctx.StoreValue(field);
					ctx.MarkLabel(label);
				}
			}
		}
	}
}
