using System;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class DateTimeSerializer : IProtoSerializer
	{
		private static readonly Type expectedType = typeof(DateTime);

		private readonly bool includeKind;

		public Type ExpectedType => expectedType;

		bool IProtoSerializer.RequiresOldValue => false;

		bool IProtoSerializer.ReturnsValue => true;

		public DateTimeSerializer(TypeModel model)
		{
			includeKind = model?.SerializeDateTimeKind() ?? false;
		}

		public object Read(object value, ProtoReader source)
		{
			return BclHelpers.ReadDateTime(source);
		}

		public void Write(object value, ProtoWriter dest)
		{
			if (includeKind)
			{
				BclHelpers.WriteDateTimeWithKind((DateTime)value, dest);
			}
			else
			{
				BclHelpers.WriteDateTime((DateTime)value, dest);
			}
		}

		void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)), includeKind ? "WriteDateTimeWithKind" : "WriteDateTime", valueFrom);
		}

		void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
		{
			ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)), "ReadDateTime", ExpectedType);
		}
	}
}
