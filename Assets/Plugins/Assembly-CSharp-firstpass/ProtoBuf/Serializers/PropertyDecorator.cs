using System;
using System.Reflection;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class PropertyDecorator : ProtoDecoratorBase
	{
		private readonly PropertyInfo property;

		private readonly Type forType;

		private readonly bool readOptionsWriteValue;

		private readonly MethodInfo shadowSetter;

		public override Type ExpectedType => forType;

		public override bool RequiresOldValue => true;

		public override bool ReturnsValue => false;

		public PropertyDecorator(TypeModel model, Type forType, PropertyInfo property, IProtoSerializer tail)
			: base(tail)
		{
			this.forType = forType;
			this.property = property;
			SanityCheck(model, property, tail, out readOptionsWriteValue, nonPublic: true, allowInternal: true);
			shadowSetter = GetShadowSetter(model, property);
		}

		private static void SanityCheck(TypeModel model, PropertyInfo property, IProtoSerializer tail, out bool writeValue, bool nonPublic, bool allowInternal)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			writeValue = tail.ReturnsValue && (GetShadowSetter(model, property) != null || (property.CanWrite && Helpers.GetSetMethod(property, nonPublic, allowInternal) != null));
			if (!property.CanRead || Helpers.GetGetMethod(property, nonPublic, allowInternal) == null)
			{
				throw new InvalidOperationException("Cannot serialize property without a get accessor");
			}
			if (!writeValue && (!tail.RequiresOldValue || Helpers.IsValueType(tail.ExpectedType)))
			{
				throw new InvalidOperationException("Cannot apply changes to property " + property.DeclaringType.FullName + "." + property.Name);
			}
		}

		private static MethodInfo GetShadowSetter(TypeModel model, PropertyInfo property)
		{
			MethodInfo instanceMethod = Helpers.GetInstanceMethod(property.ReflectedType, "Set" + property.Name, new Type[1] { property.PropertyType });
			if (instanceMethod == null || !instanceMethod.IsPublic || instanceMethod.ReturnType != model.MapType(typeof(void)))
			{
				return null;
			}
			return instanceMethod;
		}

		public override void Write(object value, ProtoWriter dest)
		{
			value = property.GetValue(value, null);
			if (value != null)
			{
				Tail.Write(value, dest);
			}
		}

		public override object Read(object value, ProtoReader source)
		{
			object value2 = (Tail.RequiresOldValue ? property.GetValue(value, null) : null);
			object obj = Tail.Read(value2, source);
			if (readOptionsWriteValue && obj != null)
			{
				if (shadowSetter == null)
				{
					property.SetValue(value, obj, null);
				}
				else
				{
					shadowSetter.Invoke(value, new object[1] { obj });
				}
			}
			return null;
		}

		protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			ctx.LoadAddress(valueFrom, ExpectedType);
			ctx.LoadValue(property);
			ctx.WriteNullCheckedTail(property.PropertyType, Tail, null);
		}

		protected override void EmitRead(CompilerContext ctx, Local valueFrom)
		{
			SanityCheck(ctx.Model, property, Tail, out var writeValue, ctx.NonPublic, ctx.AllowInternal(property));
			if (Helpers.IsValueType(ExpectedType) && valueFrom == null)
			{
				throw new InvalidOperationException("Attempt to mutate struct on the head of the stack; changes would be lost");
			}
			using (Local local = ctx.GetLocalWithValue(ExpectedType, valueFrom))
			{
				if (Tail.RequiresOldValue)
				{
					ctx.LoadAddress(local, ExpectedType);
					ctx.LoadValue(property);
				}
				Type propertyType = property.PropertyType;
				ctx.ReadNullCheckedTail(propertyType, Tail, null);
				if (writeValue)
				{
					using (Local local2 = new Local(ctx, property.PropertyType))
					{
						ctx.StoreValue(local2);
						CodeLabel label = default(CodeLabel);
						if (!Helpers.IsValueType(propertyType))
						{
							label = ctx.DefineLabel();
							ctx.LoadValue(local2);
							ctx.BranchIfFalse(label, @short: true);
						}
						ctx.LoadAddress(local, ExpectedType);
						ctx.LoadValue(local2);
						if (shadowSetter == null)
						{
							ctx.StoreValue(property);
						}
						else
						{
							ctx.EmitCall(shadowSetter);
						}
						if (!Helpers.IsValueType(propertyType))
						{
							ctx.MarkLabel(label);
						}
						return;
					}
				}
				if (Tail.ReturnsValue)
				{
					ctx.DiscardValue();
				}
			}
		}

		internal static bool CanWrite(TypeModel model, MemberInfo member)
		{
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			PropertyInfo propertyInfo = member as PropertyInfo;
			if (propertyInfo != null)
			{
				if (!propertyInfo.CanWrite)
				{
					return GetShadowSetter(model, propertyInfo) != null;
				}
				return true;
			}
			return member is FieldInfo;
		}
	}
}
