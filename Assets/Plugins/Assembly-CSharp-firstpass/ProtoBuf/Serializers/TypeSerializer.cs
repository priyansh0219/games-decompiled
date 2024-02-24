using System;
using System.Reflection;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal sealed class TypeSerializer : IProtoTypeSerializer, IProtoSerializer
	{
		private readonly Type forType;

		private readonly Type constructType;

		private readonly IProtoSerializer[] serializers;

		private readonly int[] fieldNumbers;

		private readonly bool isRootType;

		private readonly bool useConstructor;

		private readonly bool isExtensible;

		private readonly bool hasConstructor;

		private readonly CallbackSet callbacks;

		private readonly MethodInfo[] baseCtorCallbacks;

		private readonly MethodInfo factory;

		private static readonly Type iextensible = typeof(IExtensible);

		public Type ExpectedType => forType;

		private bool CanHaveInheritance
		{
			get
			{
				if (forType.IsClass || forType.IsInterface)
				{
					return !forType.IsSealed;
				}
				return false;
			}
		}

		bool IProtoSerializer.RequiresOldValue => true;

		bool IProtoSerializer.ReturnsValue => false;

		public bool HasCallbacks(TypeModel.CallbackType callbackType)
		{
			if (callbacks != null && callbacks[callbackType] != null)
			{
				return true;
			}
			for (int i = 0; i < serializers.Length; i++)
			{
				if (serializers[i].ExpectedType != forType && ((IProtoTypeSerializer)serializers[i]).HasCallbacks(callbackType))
				{
					return true;
				}
			}
			return false;
		}

		public TypeSerializer(TypeModel model, Type forType, int[] fieldNumbers, IProtoSerializer[] serializers, MethodInfo[] baseCtorCallbacks, bool isRootType, bool useConstructor, CallbackSet callbacks, Type constructType, MethodInfo factory)
		{
			Helpers.Sort(fieldNumbers, serializers);
			bool flag = false;
			for (int i = 1; i < fieldNumbers.Length; i++)
			{
				if (fieldNumbers[i] == fieldNumbers[i - 1])
				{
					throw new InvalidOperationException("Duplicate field-number detected; " + fieldNumbers[i] + " on: " + forType.FullName);
				}
				if (!flag && serializers[i].ExpectedType != forType)
				{
					flag = true;
				}
			}
			this.forType = forType;
			this.factory = factory;
			if (constructType == null)
			{
				constructType = forType;
			}
			else if (!forType.IsAssignableFrom(constructType))
			{
				throw new InvalidOperationException(forType.FullName + " cannot be assigned from " + constructType.FullName);
			}
			this.constructType = constructType;
			this.serializers = serializers;
			this.fieldNumbers = fieldNumbers;
			this.callbacks = callbacks;
			this.isRootType = isRootType;
			this.useConstructor = useConstructor;
			if (baseCtorCallbacks != null && baseCtorCallbacks.Length == 0)
			{
				baseCtorCallbacks = null;
			}
			this.baseCtorCallbacks = baseCtorCallbacks;
			if (Helpers.GetUnderlyingType(forType) != null)
			{
				throw new ArgumentException("Cannot create a TypeSerializer for nullable types", "forType");
			}
			if (model.MapType(iextensible).IsAssignableFrom(forType))
			{
				if (forType.IsValueType || !isRootType || flag)
				{
					throw new NotSupportedException("IExtensible is not supported in structs or classes with inheritance");
				}
				isExtensible = true;
			}
			hasConstructor = !constructType.IsAbstract && Helpers.GetConstructor(constructType, Helpers.EmptyTypes, nonPublic: true) != null;
			if (constructType != forType && useConstructor && !hasConstructor)
			{
				throw new ArgumentException("The supplied default implementation cannot be created: " + constructType.FullName, "constructType");
			}
		}

		bool IProtoTypeSerializer.CanCreateInstance()
		{
			return true;
		}

		object IProtoTypeSerializer.CreateInstance(ProtoReader source)
		{
			return CreateInstance(source, includeLocalCallback: false);
		}

		public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
		{
			if (callbacks != null)
			{
				InvokeCallback(callbacks[callbackType], value, context);
			}
			((IProtoTypeSerializer)GetMoreSpecificSerializer(value))?.Callback(value, callbackType, context);
		}

		private IProtoSerializer GetMoreSpecificSerializer(object value)
		{
			if (!CanHaveInheritance)
			{
				return null;
			}
			Type type = value.GetType();
			if (type == forType)
			{
				return null;
			}
			for (int i = 0; i < serializers.Length; i++)
			{
				IProtoSerializer protoSerializer = serializers[i];
				if (protoSerializer.ExpectedType != forType && Helpers.IsAssignableFrom(protoSerializer.ExpectedType, type))
				{
					return protoSerializer;
				}
			}
			if (type == constructType)
			{
				return null;
			}
			TypeModel.ThrowUnexpectedSubtype(forType, type);
			return null;
		}

		public void Write(object value, ProtoWriter dest)
		{
			if (isRootType)
			{
				Callback(value, TypeModel.CallbackType.BeforeSerialize, dest.Context);
			}
			GetMoreSpecificSerializer(value)?.Write(value, dest);
			for (int i = 0; i < serializers.Length; i++)
			{
				IProtoSerializer protoSerializer = serializers[i];
				if (protoSerializer.ExpectedType == forType)
				{
					protoSerializer.Write(value, dest);
				}
			}
			if (isExtensible)
			{
				ProtoWriter.AppendExtensionData((IExtensible)value, dest);
			}
			if (isRootType)
			{
				Callback(value, TypeModel.CallbackType.AfterSerialize, dest.Context);
			}
		}

		public object Read(object value, ProtoReader source)
		{
			if (isRootType && value != null)
			{
				Callback(value, TypeModel.CallbackType.BeforeDeserialize, source.Context);
			}
			int num = 0;
			int num2 = 0;
			int num3;
			while ((num3 = source.ReadFieldHeader()) > 0)
			{
				bool flag = false;
				if (num3 < num)
				{
					num = (num2 = 0);
				}
				for (int i = num2; i < fieldNumbers.Length; i++)
				{
					if (fieldNumbers[i] != num3)
					{
						continue;
					}
					IProtoSerializer protoSerializer = serializers[i];
					Type expectedType = protoSerializer.ExpectedType;
					if (value == null)
					{
						if (expectedType == forType)
						{
							value = CreateInstance(source, includeLocalCallback: true);
						}
					}
					else if (expectedType != forType && ((IProtoTypeSerializer)protoSerializer).CanCreateInstance() && expectedType.IsSubclassOf(value.GetType()))
					{
						value = ProtoReader.Merge(source, value, ((IProtoTypeSerializer)protoSerializer).CreateInstance(source));
					}
					if (protoSerializer.ReturnsValue)
					{
						value = protoSerializer.Read(value, source);
					}
					else
					{
						protoSerializer.Read(value, source);
					}
					num2 = i;
					num = num3;
					flag = true;
					break;
				}
				if (!flag)
				{
					if (value == null)
					{
						value = CreateInstance(source, includeLocalCallback: true);
					}
					if (isExtensible)
					{
						source.AppendExtensionData((IExtensible)value);
					}
					else
					{
						source.SkipField();
					}
				}
			}
			if (value == null)
			{
				value = CreateInstance(source, includeLocalCallback: true);
			}
			if (isRootType)
			{
				Callback(value, TypeModel.CallbackType.AfterDeserialize, source.Context);
			}
			return value;
		}

		private object InvokeCallback(MethodInfo method, object obj, SerializationContext context)
		{
			object result = null;
			if (method != null)
			{
				ParameterInfo[] parameters = method.GetParameters();
				object[] array;
				bool flag;
				if (parameters.Length == 0)
				{
					array = null;
					flag = true;
				}
				else
				{
					array = new object[parameters.Length];
					flag = true;
					for (int i = 0; i < array.Length; i++)
					{
						Type parameterType = parameters[i].ParameterType;
						object obj2;
						if (parameterType == typeof(SerializationContext))
						{
							obj2 = context;
						}
						else if (parameterType == typeof(Type))
						{
							obj2 = constructType;
						}
						else
						{
							obj2 = null;
							flag = false;
						}
						array[i] = obj2;
					}
				}
				if (!flag)
				{
					throw CallbackSet.CreateInvalidCallbackSignature(method);
				}
				result = method.Invoke(obj, array);
			}
			return result;
		}

		private object CreateInstance(ProtoReader source, bool includeLocalCallback)
		{
			object obj;
			if (factory != null)
			{
				obj = InvokeCallback(factory, null, source.Context);
			}
			else if (useConstructor)
			{
				if (!hasConstructor)
				{
					TypeModel.ThrowCannotCreateInstance(constructType);
				}
				obj = Activator.CreateInstance(constructType, nonPublic: true);
			}
			else
			{
				obj = BclHelpers.GetUninitializedObject(constructType);
			}
			ProtoReader.NoteObject(obj, source);
			if (baseCtorCallbacks != null)
			{
				for (int i = 0; i < baseCtorCallbacks.Length; i++)
				{
					InvokeCallback(baseCtorCallbacks[i], obj, source.Context);
				}
			}
			if (includeLocalCallback && callbacks != null)
			{
				InvokeCallback(callbacks.BeforeDeserialize, obj, source.Context);
			}
			return obj;
		}

		void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			Type expectedType = ExpectedType;
			using (Local local = ctx.GetLocalWithValue(expectedType, valueFrom))
			{
				EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.BeforeSerialize);
				CodeLabel label = ctx.DefineLabel();
				if (CanHaveInheritance)
				{
					for (int i = 0; i < serializers.Length; i++)
					{
						IProtoSerializer protoSerializer = serializers[i];
						Type expectedType2 = protoSerializer.ExpectedType;
						if (expectedType2 != forType)
						{
							CodeLabel label2 = ctx.DefineLabel();
							CodeLabel label3 = ctx.DefineLabel();
							ctx.LoadValue(local);
							ctx.TryCast(expectedType2);
							ctx.CopyValue();
							ctx.BranchIfTrue(label2, @short: true);
							ctx.DiscardValue();
							ctx.Branch(label3, @short: true);
							ctx.MarkLabel(label2);
							protoSerializer.EmitWrite(ctx, null);
							ctx.Branch(label, @short: false);
							ctx.MarkLabel(label3);
						}
					}
					if (constructType != null && constructType != forType)
					{
						using (Local local2 = new Local(ctx, ctx.MapType(typeof(Type))))
						{
							ctx.LoadValue(local);
							ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("GetType"));
							ctx.CopyValue();
							ctx.StoreValue(local2);
							ctx.LoadValue(forType);
							ctx.BranchIfEqual(label, @short: true);
							ctx.LoadValue(local2);
							ctx.LoadValue(constructType);
							ctx.BranchIfEqual(label, @short: true);
						}
					}
					else
					{
						ctx.LoadValue(local);
						ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("GetType"));
						ctx.LoadValue(forType);
						ctx.BranchIfEqual(label, @short: true);
					}
					ctx.LoadValue(forType);
					ctx.LoadValue(local);
					ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("GetType"));
					ctx.EmitCall(ctx.MapType(typeof(TypeModel)).GetMethod("ThrowUnexpectedSubtype", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
				}
				ctx.MarkLabel(label);
				for (int j = 0; j < serializers.Length; j++)
				{
					IProtoSerializer protoSerializer2 = serializers[j];
					if (protoSerializer2.ExpectedType == forType)
					{
						protoSerializer2.EmitWrite(ctx, local);
					}
				}
				if (isExtensible)
				{
					ctx.LoadValue(local);
					ctx.LoadReaderWriter();
					ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("AppendExtensionData"));
				}
				EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.AfterSerialize);
			}
		}

		private static void EmitInvokeCallback(CompilerContext ctx, MethodInfo method, bool copyValue, Type constructType, Type type)
		{
			if (!(method != null))
			{
				return;
			}
			if (copyValue)
			{
				ctx.CopyValue();
			}
			ParameterInfo[] parameters = method.GetParameters();
			bool flag = true;
			for (int i = 0; i < parameters.Length; i++)
			{
				Type parameterType = parameters[i].ParameterType;
				if (parameterType == ctx.MapType(typeof(SerializationContext)))
				{
					ctx.LoadSerializationContext();
				}
				else if (parameterType == ctx.MapType(typeof(Type)))
				{
					Type type2 = constructType;
					if (type2 == null)
					{
						type2 = type;
					}
					ctx.LoadValue(type2);
				}
				else
				{
					flag = false;
				}
			}
			if (flag)
			{
				ctx.EmitCall(method);
				if (constructType != null && method.ReturnType == ctx.MapType(typeof(object)))
				{
					ctx.CastFromObject(type);
				}
				return;
			}
			throw CallbackSet.CreateInvalidCallbackSignature(method);
		}

		private void EmitCallbackIfNeeded(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
		{
			if (isRootType && ((IProtoTypeSerializer)this).HasCallbacks(callbackType))
			{
				((IProtoTypeSerializer)this).EmitCallback(ctx, valueFrom, callbackType);
			}
		}

		void IProtoTypeSerializer.EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
		{
			bool flag = false;
			if (CanHaveInheritance)
			{
				for (int i = 0; i < serializers.Length; i++)
				{
					IProtoSerializer protoSerializer = serializers[i];
					if (protoSerializer.ExpectedType != forType && ((IProtoTypeSerializer)protoSerializer).HasCallbacks(callbackType))
					{
						flag = true;
					}
				}
			}
			MethodInfo methodInfo = ((callbacks == null) ? null : callbacks[callbackType]);
			if (methodInfo == null && !flag)
			{
				return;
			}
			ctx.LoadAddress(valueFrom, ExpectedType);
			EmitInvokeCallback(ctx, methodInfo, flag, null, forType);
			if (!flag)
			{
				return;
			}
			CodeLabel label = ctx.DefineLabel();
			for (int j = 0; j < serializers.Length; j++)
			{
				IProtoSerializer protoSerializer2 = serializers[j];
				Type expectedType = protoSerializer2.ExpectedType;
				IProtoTypeSerializer protoTypeSerializer;
				if (expectedType != forType && (protoTypeSerializer = (IProtoTypeSerializer)protoSerializer2).HasCallbacks(callbackType))
				{
					CodeLabel label2 = ctx.DefineLabel();
					CodeLabel label3 = ctx.DefineLabel();
					ctx.CopyValue();
					ctx.TryCast(expectedType);
					ctx.CopyValue();
					ctx.BranchIfTrue(label2, @short: true);
					ctx.DiscardValue();
					ctx.Branch(label3, @short: false);
					ctx.MarkLabel(label2);
					protoTypeSerializer.EmitCallback(ctx, null, callbackType);
					ctx.Branch(label, @short: false);
					ctx.MarkLabel(label3);
				}
			}
			ctx.MarkLabel(label);
			ctx.DiscardValue();
		}

		void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
		{
			Type expectedType = ExpectedType;
			using (Local local = ctx.GetLocalWithValue(expectedType, valueFrom))
			{
				using (Local local2 = new Local(ctx, ctx.MapType(typeof(int))))
				{
					if (HasCallbacks(TypeModel.CallbackType.BeforeDeserialize))
					{
						if (Helpers.IsValueType(ExpectedType))
						{
							EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.BeforeDeserialize);
						}
						else
						{
							CodeLabel label = ctx.DefineLabel();
							ctx.LoadValue(local);
							ctx.BranchIfFalse(label, @short: false);
							EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.BeforeDeserialize);
							ctx.MarkLabel(label);
						}
					}
					CodeLabel codeLabel = ctx.DefineLabel();
					CodeLabel label2 = ctx.DefineLabel();
					ctx.Branch(codeLabel, @short: false);
					ctx.MarkLabel(label2);
					int[] keys = fieldNumbers;
					object[] values = serializers;
					BasicList.NodeEnumerator enumerator = BasicList.GetContiguousGroups(keys, values).GetEnumerator();
					while (enumerator.MoveNext())
					{
						BasicList.Group group = (BasicList.Group)enumerator.Current;
						CodeLabel label3 = ctx.DefineLabel();
						int count = group.Items.Count;
						if (count == 1)
						{
							ctx.LoadValue(local2);
							ctx.LoadValue(group.First);
							CodeLabel codeLabel2 = ctx.DefineLabel();
							ctx.BranchIfEqual(codeLabel2, @short: true);
							ctx.Branch(label3, @short: false);
							WriteFieldHandler(ctx, expectedType, local, codeLabel2, codeLabel, (IProtoSerializer)group.Items[0]);
						}
						else
						{
							ctx.LoadValue(local2);
							ctx.LoadValue(group.First);
							ctx.Subtract();
							CodeLabel[] array = new CodeLabel[count];
							for (int i = 0; i < count; i++)
							{
								array[i] = ctx.DefineLabel();
							}
							ctx.Switch(array);
							ctx.Branch(label3, @short: false);
							for (int j = 0; j < count; j++)
							{
								WriteFieldHandler(ctx, expectedType, local, array[j], codeLabel, (IProtoSerializer)group.Items[j]);
							}
						}
						ctx.MarkLabel(label3);
					}
					EmitCreateIfNull(ctx, local);
					ctx.LoadReaderWriter();
					if (isExtensible)
					{
						ctx.LoadValue(local);
						ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("AppendExtensionData"));
					}
					else
					{
						ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("SkipField"));
					}
					ctx.MarkLabel(codeLabel);
					ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof(int)));
					ctx.CopyValue();
					ctx.StoreValue(local2);
					ctx.LoadValue(0);
					ctx.BranchIfGreater(label2, @short: false);
					EmitCreateIfNull(ctx, local);
					EmitCallbackIfNeeded(ctx, local, TypeModel.CallbackType.AfterDeserialize);
					if (valueFrom != null && !local.IsSame(valueFrom))
					{
						ctx.LoadValue(local);
						ctx.Cast(valueFrom.Type);
						ctx.StoreValue(valueFrom);
					}
				}
			}
		}

		private void WriteFieldHandler(CompilerContext ctx, Type expected, Local loc, CodeLabel handler, CodeLabel @continue, IProtoSerializer serializer)
		{
			ctx.MarkLabel(handler);
			Type expectedType = serializer.ExpectedType;
			if (expectedType == forType)
			{
				EmitCreateIfNull(ctx, loc);
				serializer.EmitRead(ctx, loc);
			}
			else
			{
				if (((IProtoTypeSerializer)serializer).CanCreateInstance())
				{
					CodeLabel label = ctx.DefineLabel();
					ctx.LoadValue(loc);
					ctx.BranchIfFalse(label, @short: false);
					ctx.LoadValue(loc);
					ctx.TryCast(expectedType);
					ctx.BranchIfTrue(label, @short: false);
					ctx.LoadReaderWriter();
					ctx.LoadValue(loc);
					((IProtoTypeSerializer)serializer).EmitCreateInstance(ctx);
					ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("Merge"));
					ctx.Cast(expected);
					ctx.StoreValue(loc);
					ctx.MarkLabel(label);
				}
				ctx.LoadValue(loc);
				ctx.Cast(expectedType);
				serializer.EmitRead(ctx, null);
			}
			if (serializer.ReturnsValue)
			{
				ctx.StoreValue(loc);
			}
			ctx.Branch(@continue, @short: false);
		}

		void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
		{
			bool flag = true;
			if (factory != null)
			{
				EmitInvokeCallback(ctx, factory, copyValue: false, constructType, forType);
			}
			else if (!useConstructor)
			{
				ctx.LoadValue(constructType);
				ctx.EmitCall(ctx.MapType(typeof(BclHelpers)).GetMethod("GetUninitializedObject"));
				ctx.Cast(forType);
			}
			else if (Helpers.IsClass(constructType) && hasConstructor)
			{
				ctx.EmitCtor(constructType);
			}
			else
			{
				ctx.LoadValue(ExpectedType);
				ctx.EmitCall(ctx.MapType(typeof(TypeModel)).GetMethod("ThrowCannotCreateInstance", BindingFlags.Static | BindingFlags.Public));
				ctx.LoadNullRef();
				flag = false;
			}
			if (flag)
			{
				ctx.CopyValue();
				ctx.LoadReaderWriter();
				ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("NoteObject", BindingFlags.Static | BindingFlags.Public));
			}
			if (baseCtorCallbacks != null)
			{
				for (int i = 0; i < baseCtorCallbacks.Length; i++)
				{
					EmitInvokeCallback(ctx, baseCtorCallbacks[i], copyValue: true, null, forType);
				}
			}
		}

		private void EmitCreateIfNull(CompilerContext ctx, Local storage)
		{
			if (!Helpers.IsValueType(ExpectedType))
			{
				CodeLabel label = ctx.DefineLabel();
				ctx.LoadValue(storage);
				ctx.BranchIfTrue(label, @short: false);
				((IProtoTypeSerializer)this).EmitCreateInstance(ctx);
				if (callbacks != null)
				{
					EmitInvokeCallback(ctx, callbacks.BeforeDeserialize, copyValue: true, null, forType);
				}
				ctx.StoreValue(storage);
				ctx.MarkLabel(label);
			}
		}
	}
}
