using System;
using System.Reflection;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class TupleSerializer : IProtoTypeSerializer, IProtoSerializer
	{
		private readonly MemberInfo[] members;

		private readonly ConstructorInfo ctor;

		private IProtoSerializer[] tails;

		public Type ExpectedType => ctor.DeclaringType;

		public bool RequiresOldValue => true;

		public bool ReturnsValue => false;

		public TupleSerializer(RuntimeTypeModel model, ConstructorInfo ctor, MemberInfo[] members)
		{
			if (ctor == null)
			{
				throw new ArgumentNullException("ctor");
			}
			if (members == null)
			{
				throw new ArgumentNullException("members");
			}
			this.ctor = ctor;
			this.members = members;
			tails = new IProtoSerializer[members.Length];
			ParameterInfo[] parameters = ctor.GetParameters();
			for (int i = 0; i < members.Length; i++)
			{
				Type parameterType = parameters[i].ParameterType;
				Type itemType = null;
				Type defaultType = null;
				MetaType.ResolveListTypes(model, parameterType, ref itemType, ref defaultType);
				Type type = ((itemType == null) ? parameterType : itemType);
				bool asReference = false;
				if (model.FindOrAddAuto(type, demand: false, addWithContractOnly: true, addEvenIfAutoDisabled: false) >= 0)
				{
					asReference = model[type].AsReferenceDefault;
				}
				IProtoSerializer protoSerializer = ValueMember.TryGetCoreSerializer(model, DataFormat.Default, type, out var defaultWireType, asReference, dynamicType: false, overwriteList: false, allowComplexTypes: true);
				if (protoSerializer == null)
				{
					throw new InvalidOperationException("No serializer defined for type: " + type.FullName);
				}
				protoSerializer = new TagDecorator(i + 1, defaultWireType, strict: false, protoSerializer);
				IProtoSerializer protoSerializer2 = ((!(itemType == null)) ? ((!parameterType.IsArray) ? ((ProtoDecoratorBase)ListDecorator.Create(model, parameterType, defaultType, protoSerializer, i + 1, writePacked: false, defaultWireType, returnList: true, overwriteList: false, supportNull: false)) : ((ProtoDecoratorBase)new ArrayDecorator(model, protoSerializer, i + 1, writePacked: false, defaultWireType, parameterType, overwriteList: false, supportNull: false))) : protoSerializer);
				tails[i] = protoSerializer2;
			}
		}

		public bool HasCallbacks(TypeModel.CallbackType callbackType)
		{
			return false;
		}

		public void EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
		{
		}

		void IProtoTypeSerializer.Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
		{
		}

		object IProtoTypeSerializer.CreateInstance(ProtoReader source)
		{
			throw new NotSupportedException();
		}

		private object GetValue(object obj, int index)
		{
			PropertyInfo propertyInfo;
			if ((propertyInfo = members[index] as PropertyInfo) != null)
			{
				if (obj == null)
				{
					if (!Helpers.IsValueType(propertyInfo.PropertyType))
					{
						return null;
					}
					return Activator.CreateInstance(propertyInfo.PropertyType);
				}
				return propertyInfo.GetValue(obj, null);
			}
			FieldInfo fieldInfo;
			if ((fieldInfo = members[index] as FieldInfo) != null)
			{
				if (obj == null)
				{
					if (!Helpers.IsValueType(fieldInfo.FieldType))
					{
						return null;
					}
					return Activator.CreateInstance(fieldInfo.FieldType);
				}
				return fieldInfo.GetValue(obj);
			}
			throw new InvalidOperationException();
		}

		public object Read(object value, ProtoReader source)
		{
			object[] array = new object[members.Length];
			bool flag = false;
			if (value == null)
			{
				flag = true;
			}
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GetValue(value, i);
			}
			int num;
			while ((num = source.ReadFieldHeader()) > 0)
			{
				flag = true;
				if (num <= tails.Length)
				{
					IProtoSerializer protoSerializer = tails[num - 1];
					array[num - 1] = tails[num - 1].Read(protoSerializer.RequiresOldValue ? array[num - 1] : null, source);
				}
				else
				{
					source.SkipField();
				}
			}
			if (!flag)
			{
				return value;
			}
			return ctor.Invoke(array);
		}

		public void Write(object value, ProtoWriter dest)
		{
			for (int i = 0; i < tails.Length; i++)
			{
				object value2 = GetValue(value, i);
				if (value2 != null)
				{
					tails[i].Write(value2, dest);
				}
			}
		}

		private Type GetMemberType(int index)
		{
			Type memberType = Helpers.GetMemberType(members[index]);
			if (memberType == null)
			{
				throw new InvalidOperationException();
			}
			return memberType;
		}

		bool IProtoTypeSerializer.CanCreateInstance()
		{
			return false;
		}

		public void EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			using (Local local = ctx.GetLocalWithValue(ctor.DeclaringType, valueFrom))
			{
				for (int i = 0; i < tails.Length; i++)
				{
					Type memberType = GetMemberType(i);
					ctx.LoadAddress(local, ExpectedType);
					if (members[i] is FieldInfo)
					{
						ctx.LoadValue((FieldInfo)members[i]);
					}
					else if (members[i] is PropertyInfo)
					{
						ctx.LoadValue((PropertyInfo)members[i]);
					}
					ctx.WriteNullCheckedTail(memberType, tails[i], null);
				}
			}
		}

		void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
		{
			throw new NotSupportedException();
		}

		public void EmitRead(CompilerContext ctx, Local incoming)
		{
			using (Local local = ctx.GetLocalWithValue(ExpectedType, incoming))
			{
				Local[] array = new Local[members.Length];
				try
				{
					for (int i = 0; i < array.Length; i++)
					{
						Type memberType = GetMemberType(i);
						bool flag = true;
						array[i] = new Local(ctx, memberType);
						if (Helpers.IsValueType(ExpectedType))
						{
							continue;
						}
						if (Helpers.IsValueType(memberType))
						{
							switch (Helpers.GetTypeCode(memberType))
							{
							case ProtoTypeCode.Boolean:
							case ProtoTypeCode.SByte:
							case ProtoTypeCode.Byte:
							case ProtoTypeCode.Int16:
							case ProtoTypeCode.UInt16:
							case ProtoTypeCode.Int32:
							case ProtoTypeCode.UInt32:
								ctx.LoadValue(0);
								break;
							case ProtoTypeCode.Int64:
							case ProtoTypeCode.UInt64:
								ctx.LoadValue(0L);
								break;
							case ProtoTypeCode.Single:
								ctx.LoadValue(0f);
								break;
							case ProtoTypeCode.Double:
								ctx.LoadValue(0.0);
								break;
							case ProtoTypeCode.Decimal:
								ctx.LoadValue(0m);
								break;
							case ProtoTypeCode.Guid:
								ctx.LoadValue(Guid.Empty);
								break;
							default:
								ctx.LoadAddress(array[i], memberType);
								ctx.EmitCtor(memberType);
								flag = false;
								break;
							}
						}
						else
						{
							ctx.LoadNullRef();
						}
						if (flag)
						{
							ctx.StoreValue(array[i]);
						}
					}
					CodeLabel label = (Helpers.IsValueType(ExpectedType) ? default(CodeLabel) : ctx.DefineLabel());
					if (!Helpers.IsValueType(ExpectedType))
					{
						ctx.LoadAddress(local, ExpectedType);
						ctx.BranchIfFalse(label, @short: false);
					}
					for (int j = 0; j < members.Length; j++)
					{
						ctx.LoadAddress(local, ExpectedType);
						if (members[j] is FieldInfo)
						{
							ctx.LoadValue((FieldInfo)members[j]);
						}
						else if (members[j] is PropertyInfo)
						{
							ctx.LoadValue((PropertyInfo)members[j]);
						}
						ctx.StoreValue(array[j]);
					}
					if (!Helpers.IsValueType(ExpectedType))
					{
						ctx.MarkLabel(label);
					}
					using (Local local2 = new Local(ctx, ctx.MapType(typeof(int))))
					{
						CodeLabel label2 = ctx.DefineLabel();
						CodeLabel label3 = ctx.DefineLabel();
						CodeLabel label4 = ctx.DefineLabel();
						ctx.Branch(label2, @short: false);
						CodeLabel[] array2 = new CodeLabel[members.Length];
						for (int k = 0; k < members.Length; k++)
						{
							array2[k] = ctx.DefineLabel();
						}
						ctx.MarkLabel(label3);
						ctx.LoadValue(local2);
						ctx.LoadValue(1);
						ctx.Subtract();
						ctx.Switch(array2);
						ctx.Branch(label4, @short: false);
						for (int l = 0; l < array2.Length; l++)
						{
							ctx.MarkLabel(array2[l]);
							IProtoSerializer protoSerializer = tails[l];
							Local valueFrom = (protoSerializer.RequiresOldValue ? array[l] : null);
							ctx.ReadNullCheckedTail(array[l].Type, protoSerializer, valueFrom);
							if (protoSerializer.ReturnsValue)
							{
								if (Helpers.IsValueType(array[l].Type))
								{
									ctx.StoreValue(array[l]);
								}
								else
								{
									CodeLabel label5 = ctx.DefineLabel();
									CodeLabel label6 = ctx.DefineLabel();
									ctx.CopyValue();
									ctx.BranchIfTrue(label5, @short: true);
									ctx.DiscardValue();
									ctx.Branch(label6, @short: true);
									ctx.MarkLabel(label5);
									ctx.StoreValue(array[l]);
									ctx.MarkLabel(label6);
								}
							}
							ctx.Branch(label2, @short: false);
						}
						ctx.MarkLabel(label4);
						ctx.LoadReaderWriter();
						ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("SkipField"));
						ctx.MarkLabel(label2);
						ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof(int)));
						ctx.CopyValue();
						ctx.StoreValue(local2);
						ctx.LoadValue(0);
						ctx.BranchIfGreater(label3, @short: false);
					}
					for (int m = 0; m < array.Length; m++)
					{
						ctx.LoadValue(array[m]);
					}
					ctx.EmitCtor(ctor);
					ctx.StoreValue(local);
				}
				finally
				{
					for (int n = 0; n < array.Length; n++)
					{
						if (array[n] != null)
						{
							array[n].Dispose();
						}
					}
				}
			}
		}
	}
}
