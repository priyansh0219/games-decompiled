using System;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class TimeSpanSerializer : IProtoSerializer
	{
		private static readonly Type expectedType = typeof(TimeSpan);

		public Type ExpectedType => expectedType;

		bool IProtoSerializer.RequiresOldValue => false;

		bool IProtoSerializer.ReturnsValue => true;

		public TimeSpanSerializer(TypeModel model)
		{
		}

		public object Read(object value, ProtoReader source)
		{
			return BclHelpers.ReadTimeSpan(source);
		}

		public void Write(object value, ProtoWriter dest)
		{
			BclHelpers.WriteTimeSpan((TimeSpan)value, dest);
		}

		void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)), "WriteTimeSpan", valueFrom);
		}

		void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
		{
			ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)), "ReadTimeSpan", ExpectedType);
		}
	}
}
