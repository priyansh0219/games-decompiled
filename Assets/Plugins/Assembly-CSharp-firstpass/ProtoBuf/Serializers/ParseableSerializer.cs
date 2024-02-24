using System;
using System.Reflection;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class ParseableSerializer : IProtoSerializer
	{
		private readonly MethodInfo parse;

		public Type ExpectedType => parse.DeclaringType;

		bool IProtoSerializer.RequiresOldValue => false;

		bool IProtoSerializer.ReturnsValue => true;

		public static ParseableSerializer TryCreate(Type type, TypeModel model)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			MethodInfo method = type.GetMethod("Parse", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public, null, new Type[1] { model.MapType(typeof(string)) }, null);
			if (method != null && method.ReturnType == type)
			{
				if (Helpers.IsValueType(type))
				{
					MethodInfo customToString = GetCustomToString(type);
					if (customToString == null || customToString.ReturnType != model.MapType(typeof(string)))
					{
						return null;
					}
				}
				return new ParseableSerializer(method);
			}
			return null;
		}

		private static MethodInfo GetCustomToString(Type type)
		{
			return type.GetMethod("ToString", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public, null, Helpers.EmptyTypes, null);
		}

		private ParseableSerializer(MethodInfo parse)
		{
			this.parse = parse;
		}

		public object Read(object value, ProtoReader source)
		{
			return parse.Invoke(null, new object[1] { source.ReadString() });
		}

		public void Write(object value, ProtoWriter dest)
		{
			ProtoWriter.WriteString(value.ToString(), dest);
		}

		void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			Type expectedType = ExpectedType;
			if (Helpers.IsValueType(expectedType))
			{
				using (Local local = ctx.GetLocalWithValue(expectedType, valueFrom))
				{
					ctx.LoadAddress(local, expectedType);
					ctx.EmitCall(GetCustomToString(expectedType));
				}
			}
			else
			{
				ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("ToString"));
			}
			ctx.EmitBasicWrite("WriteString", valueFrom);
		}

		void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
		{
			ctx.EmitBasicRead("ReadString", ctx.MapType(typeof(string)));
			ctx.EmitCall(parse);
		}
	}
}
