using System;
using System.Reflection;
using ProtoBuf.Compiler;

namespace ProtoBuf.Serializers
{
	internal sealed class MemberSpecifiedDecorator : ProtoDecoratorBase
	{
		private readonly MethodInfo getSpecified;

		private readonly MethodInfo setSpecified;

		public override Type ExpectedType => Tail.ExpectedType;

		public override bool RequiresOldValue => Tail.RequiresOldValue;

		public override bool ReturnsValue => Tail.ReturnsValue;

		public MemberSpecifiedDecorator(MethodInfo getSpecified, MethodInfo setSpecified, IProtoSerializer tail)
			: base(tail)
		{
			if (getSpecified == null && setSpecified == null)
			{
				throw new InvalidOperationException();
			}
			this.getSpecified = getSpecified;
			this.setSpecified = setSpecified;
		}

		public override void Write(object value, ProtoWriter dest)
		{
			if (getSpecified == null || (bool)getSpecified.Invoke(value, null))
			{
				Tail.Write(value, dest);
			}
		}

		public override object Read(object value, ProtoReader source)
		{
			object result = Tail.Read(value, source);
			if (setSpecified != null)
			{
				setSpecified.Invoke(value, new object[1] { true });
			}
			return result;
		}

		protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			if (getSpecified == null)
			{
				Tail.EmitWrite(ctx, valueFrom);
				return;
			}
			using (Local local = ctx.GetLocalWithValue(ExpectedType, valueFrom))
			{
				ctx.LoadAddress(local, ExpectedType);
				ctx.EmitCall(getSpecified);
				CodeLabel label = ctx.DefineLabel();
				ctx.BranchIfFalse(label, @short: false);
				Tail.EmitWrite(ctx, local);
				ctx.MarkLabel(label);
			}
		}

		protected override void EmitRead(CompilerContext ctx, Local valueFrom)
		{
			if (setSpecified == null)
			{
				Tail.EmitRead(ctx, valueFrom);
				return;
			}
			using (Local local = ctx.GetLocalWithValue(ExpectedType, valueFrom))
			{
				Tail.EmitRead(ctx, local);
				ctx.LoadAddress(local, ExpectedType);
				ctx.LoadValue(1);
				ctx.EmitCall(setSpecified);
			}
		}
	}
}
