using System;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class UriDecorator : ProtoDecoratorBase
	{
		private static readonly Type expectedType = typeof(Uri);

		public override Type ExpectedType => expectedType;

		public override bool RequiresOldValue => false;

		public override bool ReturnsValue => true;

		public UriDecorator(TypeModel model, IProtoSerializer tail)
			: base(tail)
		{
		}

		public override void Write(object value, ProtoWriter dest)
		{
			Tail.Write(((Uri)value).AbsoluteUri, dest);
		}

		public override object Read(object value, ProtoReader source)
		{
			string text = (string)Tail.Read(null, source);
			if (text.Length != 0)
			{
				return new Uri(text);
			}
			return null;
		}

		protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			ctx.LoadValue(valueFrom);
			ctx.LoadValue(typeof(Uri).GetProperty("AbsoluteUri"));
			Tail.EmitWrite(ctx, null);
		}

		protected override void EmitRead(CompilerContext ctx, Local valueFrom)
		{
			Tail.EmitRead(ctx, valueFrom);
			ctx.CopyValue();
			CodeLabel label = ctx.DefineLabel();
			CodeLabel label2 = ctx.DefineLabel();
			ctx.LoadValue(typeof(string).GetProperty("Length"));
			ctx.BranchIfTrue(label, @short: true);
			ctx.DiscardValue();
			ctx.LoadNullRef();
			ctx.Branch(label2, @short: true);
			ctx.MarkLabel(label);
			ctx.EmitCtor(ctx.MapType(typeof(Uri)), ctx.MapType(typeof(string)));
			ctx.MarkLabel(label2);
		}
	}
}
