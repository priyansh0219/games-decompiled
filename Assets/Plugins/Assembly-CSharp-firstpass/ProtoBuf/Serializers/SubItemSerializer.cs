using System;
using System.Reflection.Emit;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class SubItemSerializer : IProtoTypeSerializer, IProtoSerializer
	{
		private readonly int key;

		private readonly Type type;

		private readonly ISerializerProxy proxy;

		private readonly bool recursionCheck;

		Type IProtoSerializer.ExpectedType => type;

		bool IProtoSerializer.RequiresOldValue => true;

		bool IProtoSerializer.ReturnsValue => true;

		bool IProtoTypeSerializer.HasCallbacks(TypeModel.CallbackType callbackType)
		{
			return ((IProtoTypeSerializer)proxy.Serializer).HasCallbacks(callbackType);
		}

		bool IProtoTypeSerializer.CanCreateInstance()
		{
			return ((IProtoTypeSerializer)proxy.Serializer).CanCreateInstance();
		}

		void IProtoTypeSerializer.EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
		{
			((IProtoTypeSerializer)proxy.Serializer).EmitCallback(ctx, valueFrom, callbackType);
		}

		void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
		{
			((IProtoTypeSerializer)proxy.Serializer).EmitCreateInstance(ctx);
		}

		void IProtoTypeSerializer.Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
		{
			((IProtoTypeSerializer)proxy.Serializer).Callback(value, callbackType, context);
		}

		object IProtoTypeSerializer.CreateInstance(ProtoReader source)
		{
			return ((IProtoTypeSerializer)proxy.Serializer).CreateInstance(source);
		}

		public SubItemSerializer(Type type, int key, ISerializerProxy proxy, bool recursionCheck)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (proxy == null)
			{
				throw new ArgumentNullException("proxy");
			}
			this.type = type;
			this.proxy = proxy;
			this.key = key;
			this.recursionCheck = recursionCheck;
		}

		void IProtoSerializer.Write(object value, ProtoWriter dest)
		{
			if (recursionCheck)
			{
				ProtoWriter.WriteObject(value, key, dest);
			}
			else
			{
				ProtoWriter.WriteRecursionSafeObject(value, key, dest);
			}
		}

		object IProtoSerializer.Read(object value, ProtoReader source)
		{
			return ProtoReader.ReadObject(value, key, source);
		}

		private bool EmitDedicatedMethod(CompilerContext ctx, Local valueFrom, bool read)
		{
			MethodBuilder dedicatedMethod = ctx.GetDedicatedMethod(key, read);
			if (dedicatedMethod == null)
			{
				return false;
			}
			using (Local local = new Local(ctx, ctx.MapType(typeof(SubItemToken))))
			{
				Type type = ctx.MapType(read ? typeof(ProtoReader) : typeof(ProtoWriter));
				ctx.LoadValue(valueFrom);
				if (!read)
				{
					if (Helpers.IsValueType(this.type) || !recursionCheck)
					{
						ctx.LoadNullRef();
					}
					else
					{
						ctx.CopyValue();
					}
				}
				ctx.LoadReaderWriter();
				ctx.EmitCall(Helpers.GetStaticMethod(type, "StartSubItem", (!read) ? new Type[2]
				{
					ctx.MapType(typeof(object)),
					type
				} : new Type[1] { type }));
				ctx.StoreValue(local);
				ctx.LoadReaderWriter();
				ctx.EmitCall(dedicatedMethod);
				if (read && this.type != dedicatedMethod.ReturnType)
				{
					ctx.Cast(this.type);
				}
				ctx.LoadValue(local);
				ctx.LoadReaderWriter();
				ctx.EmitCall(Helpers.GetStaticMethod(type, "EndSubItem", new Type[2]
				{
					ctx.MapType(typeof(SubItemToken)),
					type
				}));
			}
			return true;
		}

		void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			if (!EmitDedicatedMethod(ctx, valueFrom, read: false))
			{
				ctx.LoadValue(valueFrom);
				if (Helpers.IsValueType(type))
				{
					ctx.CastToObject(type);
				}
				ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(key));
				ctx.LoadReaderWriter();
				ctx.EmitCall(Helpers.GetStaticMethod(ctx.MapType(typeof(ProtoWriter)), recursionCheck ? "WriteObject" : "WriteRecursionSafeObject", new Type[3]
				{
					ctx.MapType(typeof(object)),
					ctx.MapType(typeof(int)),
					ctx.MapType(typeof(ProtoWriter))
				}));
			}
		}

		void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
		{
			if (!EmitDedicatedMethod(ctx, valueFrom, read: true))
			{
				ctx.LoadValue(valueFrom);
				if (Helpers.IsValueType(type))
				{
					ctx.CastToObject(type);
				}
				ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(key));
				ctx.LoadReaderWriter();
				ctx.EmitCall(Helpers.GetStaticMethod(ctx.MapType(typeof(ProtoReader)), "ReadObject"));
				ctx.CastFromObject(type);
			}
		}
	}
}
