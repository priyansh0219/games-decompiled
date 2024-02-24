using System;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class CompiledSerializer : IProtoTypeSerializer, IProtoSerializer
	{
		private readonly IProtoTypeSerializer head;

		private readonly ProtoSerializer serializer;

		private readonly ProtoDeserializer deserializer;

		bool IProtoSerializer.RequiresOldValue => head.RequiresOldValue;

		bool IProtoSerializer.ReturnsValue => head.ReturnsValue;

		Type IProtoSerializer.ExpectedType => head.ExpectedType;

		bool IProtoTypeSerializer.HasCallbacks(TypeModel.CallbackType callbackType)
		{
			return head.HasCallbacks(callbackType);
		}

		bool IProtoTypeSerializer.CanCreateInstance()
		{
			return head.CanCreateInstance();
		}

		object IProtoTypeSerializer.CreateInstance(ProtoReader source)
		{
			return head.CreateInstance(source);
		}

		public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
		{
			head.Callback(value, callbackType, context);
		}

		public static CompiledSerializer Wrap(IProtoTypeSerializer head, TypeModel model)
		{
			CompiledSerializer compiledSerializer = head as CompiledSerializer;
			if (compiledSerializer == null)
			{
				compiledSerializer = new CompiledSerializer(head, model);
			}
			return compiledSerializer;
		}

		private CompiledSerializer(IProtoTypeSerializer head, TypeModel model)
		{
			this.head = head;
			serializer = CompilerContext.BuildSerializer(head, model);
			deserializer = CompilerContext.BuildDeserializer(head, model);
		}

		void IProtoSerializer.Write(object value, ProtoWriter dest)
		{
			serializer(value, dest);
		}

		object IProtoSerializer.Read(object value, ProtoReader source)
		{
			return deserializer(value, source);
		}

		void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			head.EmitWrite(ctx, valueFrom);
		}

		void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
		{
			head.EmitRead(ctx, valueFrom);
		}

		void IProtoTypeSerializer.EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
		{
			head.EmitCallback(ctx, valueFrom, callbackType);
		}

		void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
		{
			head.EmitCreateInstance(ctx);
		}
	}
}
