using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace ProtoBuf.Compiler
{
	internal sealed class CompilerContext
	{
		private sealed class UsingBlock : IDisposable
		{
			private Local local;

			private CompilerContext ctx;

			private CodeLabel label;

			public UsingBlock(CompilerContext ctx, Local local)
			{
				if (ctx == null)
				{
					throw new ArgumentNullException("ctx");
				}
				if (local == null)
				{
					throw new ArgumentNullException("local");
				}
				Type type = local.Type;
				if ((!Helpers.IsValueType(type) && !Helpers.IsSealed(type)) || ctx.MapType(typeof(IDisposable)).IsAssignableFrom(type))
				{
					this.local = local;
					this.ctx = ctx;
					label = ctx.BeginTry();
				}
			}

			public void Dispose()
			{
				if (this.local == null || ctx == null)
				{
					return;
				}
				ctx.EndTry(label, @short: false);
				ctx.BeginFinally();
				Type type = ctx.MapType(typeof(IDisposable));
				MethodInfo method = type.GetMethod("Dispose");
				Type type2 = this.local.Type;
				if (Helpers.IsValueType(type2))
				{
					ctx.LoadAddress(this.local, type2);
					if (ctx.MetadataVersion == ILVersion.Net1)
					{
						ctx.LoadValue(this.local);
						ctx.CastToObject(type2);
					}
					else
					{
						ctx.Constrain(type2);
					}
					ctx.EmitCall(method);
				}
				else
				{
					CodeLabel codeLabel = ctx.DefineLabel();
					if (type.IsAssignableFrom(type2))
					{
						ctx.LoadValue(this.local);
						ctx.BranchIfFalse(codeLabel, @short: true);
						ctx.LoadAddress(this.local, type2);
					}
					else
					{
						using (Local local = new Local(ctx, type))
						{
							ctx.LoadValue(this.local);
							ctx.TryCast(type);
							ctx.CopyValue();
							ctx.StoreValue(local);
							ctx.BranchIfFalse(codeLabel, @short: true);
							ctx.LoadAddress(local, type);
						}
					}
					ctx.EmitCall(method);
					ctx.MarkLabel(codeLabel);
				}
				ctx.EndFinally();
				this.local = null;
				ctx = null;
				label = default(CodeLabel);
			}
		}

		public enum ILVersion
		{
			Net1 = 0,
			Net2 = 1
		}

		private readonly DynamicMethod method;

		private static int next;

		private readonly bool isStatic;

		private readonly RuntimeTypeModel.SerializerPair[] methodPairs;

		private readonly bool isWriter;

		private readonly bool nonPublic;

		private readonly Local inputValue;

		private readonly string assemblyName;

		private readonly ILGenerator il;

		private MutableList locals = new MutableList();

		private int nextLabel;

		private BasicList knownTrustedAssemblies;

		private BasicList knownUntrustedAssemblies;

		private readonly TypeModel model;

		private readonly ILVersion metadataVersion;

		public TypeModel Model => model;

		internal bool NonPublic => nonPublic;

		public Local InputValue => inputValue;

		public ILVersion MetadataVersion => metadataVersion;

		internal CodeLabel DefineLabel()
		{
			return new CodeLabel(il.DefineLabel(), nextLabel++);
		}

		[Conditional("DEBUG_COMPILE")]
		private void TraceCompile(string value)
		{
		}

		internal void MarkLabel(CodeLabel label)
		{
			il.MarkLabel(label.Value);
		}

		public static ProtoSerializer BuildSerializer(IProtoSerializer head, TypeModel model)
		{
			Type expectedType = head.ExpectedType;
			try
			{
				CompilerContext compilerContext = new CompilerContext(expectedType, isWriter: true, isStatic: true, model, typeof(object));
				compilerContext.LoadValue(compilerContext.InputValue);
				compilerContext.CastFromObject(expectedType);
				compilerContext.WriteNullCheckedTail(expectedType, head, null);
				compilerContext.Emit(OpCodes.Ret);
				return (ProtoSerializer)compilerContext.method.CreateDelegate(typeof(ProtoSerializer));
			}
			catch (Exception innerException)
			{
				string text = expectedType.FullName;
				if (string.IsNullOrEmpty(text))
				{
					text = expectedType.Name;
				}
				throw new InvalidOperationException("It was not possible to prepare a serializer for: " + text, innerException);
			}
		}

		public static ProtoDeserializer BuildDeserializer(IProtoSerializer head, TypeModel model)
		{
			Type expectedType = head.ExpectedType;
			CompilerContext compilerContext = new CompilerContext(expectedType, isWriter: false, isStatic: true, model, typeof(object));
			using (Local local = new Local(compilerContext, expectedType))
			{
				if (!Helpers.IsValueType(expectedType))
				{
					compilerContext.LoadValue(compilerContext.InputValue);
					compilerContext.CastFromObject(expectedType);
					compilerContext.StoreValue(local);
				}
				else
				{
					compilerContext.LoadValue(compilerContext.InputValue);
					CodeLabel label = compilerContext.DefineLabel();
					CodeLabel label2 = compilerContext.DefineLabel();
					compilerContext.BranchIfTrue(label, @short: true);
					compilerContext.LoadAddress(local, expectedType);
					compilerContext.EmitCtor(expectedType);
					compilerContext.Branch(label2, @short: true);
					compilerContext.MarkLabel(label);
					compilerContext.LoadValue(compilerContext.InputValue);
					compilerContext.CastFromObject(expectedType);
					compilerContext.StoreValue(local);
					compilerContext.MarkLabel(label2);
				}
				head.EmitRead(compilerContext, local);
				if (head.ReturnsValue)
				{
					compilerContext.StoreValue(local);
				}
				compilerContext.LoadValue(local);
				compilerContext.CastToObject(expectedType);
			}
			compilerContext.Emit(OpCodes.Ret);
			return (ProtoDeserializer)compilerContext.method.CreateDelegate(typeof(ProtoDeserializer));
		}

		internal void Return()
		{
			Emit(OpCodes.Ret);
		}

		private static bool IsObject(Type type)
		{
			return type == typeof(object);
		}

		internal void CastToObject(Type type)
		{
			if (!IsObject(type))
			{
				if (Helpers.IsValueType(type))
				{
					il.Emit(OpCodes.Box, type);
				}
				else
				{
					il.Emit(OpCodes.Castclass, MapType(typeof(object)));
				}
			}
		}

		internal void CastFromObject(Type type)
		{
			if (IsObject(type))
			{
				return;
			}
			if (Helpers.IsValueType(type))
			{
				if (MetadataVersion == ILVersion.Net1)
				{
					il.Emit(OpCodes.Unbox, type);
					il.Emit(OpCodes.Ldobj, type);
				}
				else
				{
					il.Emit(OpCodes.Unbox_Any, type);
				}
			}
			else
			{
				il.Emit(OpCodes.Castclass, type);
			}
		}

		internal MethodBuilder GetDedicatedMethod(int metaKey, bool read)
		{
			if (methodPairs == null)
			{
				return null;
			}
			for (int i = 0; i < methodPairs.Length; i++)
			{
				if (methodPairs[i].MetaKey == metaKey)
				{
					if (!read)
					{
						return methodPairs[i].Serialize;
					}
					return methodPairs[i].Deserialize;
				}
			}
			throw new ArgumentException("Meta-key not found", "metaKey");
		}

		internal int MapMetaKeyToCompiledKey(int metaKey)
		{
			if (metaKey < 0 || methodPairs == null)
			{
				return metaKey;
			}
			for (int i = 0; i < methodPairs.Length; i++)
			{
				if (methodPairs[i].MetaKey == metaKey)
				{
					return i;
				}
			}
			throw new ArgumentException("Key could not be mapped: " + metaKey, "metaKey");
		}

		internal CompilerContext(ILGenerator il, bool isStatic, bool isWriter, RuntimeTypeModel.SerializerPair[] methodPairs, TypeModel model, ILVersion metadataVersion, string assemblyName, Type inputType, string traceName)
		{
			if (il == null)
			{
				throw new ArgumentNullException("il");
			}
			if (methodPairs == null)
			{
				throw new ArgumentNullException("methodPairs");
			}
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}
			if (Helpers.IsNullOrEmpty(assemblyName))
			{
				throw new ArgumentNullException("assemblyName");
			}
			this.assemblyName = assemblyName;
			this.isStatic = isStatic;
			this.methodPairs = methodPairs;
			this.il = il;
			this.isWriter = isWriter;
			this.model = model;
			this.metadataVersion = metadataVersion;
			if (inputType != null)
			{
				inputValue = new Local(null, inputType);
			}
		}

		private CompilerContext(Type associatedType, bool isWriter, bool isStatic, TypeModel model, Type inputType)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}
			metadataVersion = ILVersion.Net2;
			this.isStatic = isStatic;
			this.isWriter = isWriter;
			this.model = model;
			nonPublic = true;
			Type typeFromHandle;
			Type[] parameterTypes;
			if (isWriter)
			{
				typeFromHandle = typeof(void);
				parameterTypes = new Type[2]
				{
					typeof(object),
					typeof(ProtoWriter)
				};
			}
			else
			{
				typeFromHandle = typeof(object);
				parameterTypes = new Type[2]
				{
					typeof(object),
					typeof(ProtoReader)
				};
			}
			method = new DynamicMethod("proto_" + Interlocked.Increment(ref next), typeFromHandle, parameterTypes, associatedType.IsInterface ? typeof(object) : associatedType, skipVisibility: true);
			il = method.GetILGenerator();
			if (inputType != null)
			{
				inputValue = new Local(null, inputType);
			}
		}

		private void Emit(OpCode opcode)
		{
			il.Emit(opcode);
		}

		public void LoadValue(string value)
		{
			if (value == null)
			{
				LoadNullRef();
			}
			else
			{
				il.Emit(OpCodes.Ldstr, value);
			}
		}

		public void LoadValue(float value)
		{
			il.Emit(OpCodes.Ldc_R4, value);
		}

		public void LoadValue(double value)
		{
			il.Emit(OpCodes.Ldc_R8, value);
		}

		public void LoadValue(long value)
		{
			il.Emit(OpCodes.Ldc_I8, value);
		}

		public void LoadValue(int value)
		{
			switch (value)
			{
			case 0:
				Emit(OpCodes.Ldc_I4_0);
				return;
			case 1:
				Emit(OpCodes.Ldc_I4_1);
				return;
			case 2:
				Emit(OpCodes.Ldc_I4_2);
				return;
			case 3:
				Emit(OpCodes.Ldc_I4_3);
				return;
			case 4:
				Emit(OpCodes.Ldc_I4_4);
				return;
			case 5:
				Emit(OpCodes.Ldc_I4_5);
				return;
			case 6:
				Emit(OpCodes.Ldc_I4_6);
				return;
			case 7:
				Emit(OpCodes.Ldc_I4_7);
				return;
			case 8:
				Emit(OpCodes.Ldc_I4_8);
				return;
			case -1:
				Emit(OpCodes.Ldc_I4_M1);
				return;
			}
			if (value >= -128 && value <= 127)
			{
				il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
			}
			else
			{
				il.Emit(OpCodes.Ldc_I4, value);
			}
		}

		internal LocalBuilder GetFromPool(Type type)
		{
			int count = locals.Count;
			for (int i = 0; i < count; i++)
			{
				LocalBuilder localBuilder = (LocalBuilder)locals[i];
				if (localBuilder != null && localBuilder.LocalType == type)
				{
					locals[i] = null;
					return localBuilder;
				}
			}
			return il.DeclareLocal(type);
		}

		internal void ReleaseToPool(LocalBuilder value)
		{
			int count = locals.Count;
			for (int i = 0; i < count; i++)
			{
				if (locals[i] == null)
				{
					locals[i] = value;
					return;
				}
			}
			locals.Add(value);
		}

		public void LoadReaderWriter()
		{
			Emit(isStatic ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2);
		}

		public void StoreValue(Local local)
		{
			if (local == InputValue)
			{
				byte arg = ((!isStatic) ? ((byte)1) : ((byte)0));
				il.Emit(OpCodes.Starg_S, arg);
				return;
			}
			switch (local.Value.LocalIndex)
			{
			case 0:
				Emit(OpCodes.Stloc_0);
				break;
			case 1:
				Emit(OpCodes.Stloc_1);
				break;
			case 2:
				Emit(OpCodes.Stloc_2);
				break;
			case 3:
				Emit(OpCodes.Stloc_3);
				break;
			default:
			{
				OpCode opcode = (UseShortForm(local) ? OpCodes.Stloc_S : OpCodes.Stloc);
				il.Emit(opcode, local.Value);
				break;
			}
			}
		}

		public void LoadValue(Local local)
		{
			if (local == null)
			{
				return;
			}
			if (local == InputValue)
			{
				Emit(isStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
				return;
			}
			switch (local.Value.LocalIndex)
			{
			case 0:
				Emit(OpCodes.Ldloc_0);
				break;
			case 1:
				Emit(OpCodes.Ldloc_1);
				break;
			case 2:
				Emit(OpCodes.Ldloc_2);
				break;
			case 3:
				Emit(OpCodes.Ldloc_3);
				break;
			default:
			{
				OpCode opcode = (UseShortForm(local) ? OpCodes.Ldloc_S : OpCodes.Ldloc);
				il.Emit(opcode, local.Value);
				break;
			}
			}
		}

		public Local GetLocalWithValue(Type type, Local fromValue)
		{
			if (fromValue != null)
			{
				if (fromValue.Type == type)
				{
					return fromValue.AsCopy();
				}
				LoadValue(fromValue);
				if (!Helpers.IsValueType(type) && (fromValue.Type == null || !type.IsAssignableFrom(fromValue.Type)))
				{
					Cast(type);
				}
			}
			Local local = new Local(this, type);
			StoreValue(local);
			return local;
		}

		internal void EmitBasicRead(string methodName, Type expectedType)
		{
			MethodInfo methodInfo = MapType(typeof(ProtoReader)).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo == null || methodInfo.ReturnType != expectedType || methodInfo.GetParameters().Length != 0)
			{
				throw new ArgumentException("methodName");
			}
			LoadReaderWriter();
			EmitCall(methodInfo);
		}

		internal void EmitBasicRead(Type helperType, string methodName, Type expectedType)
		{
			MethodInfo methodInfo = helperType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo == null || methodInfo.ReturnType != expectedType || methodInfo.GetParameters().Length != 1)
			{
				throw new ArgumentException("methodName");
			}
			LoadReaderWriter();
			EmitCall(methodInfo);
		}

		internal void EmitBasicWrite(string methodName, Local fromValue)
		{
			if (Helpers.IsNullOrEmpty(methodName))
			{
				throw new ArgumentNullException("methodName");
			}
			LoadValue(fromValue);
			LoadReaderWriter();
			EmitCall(GetWriterMethod(methodName));
		}

		private MethodInfo GetWriterMethod(string methodName)
		{
			Type type = MapType(typeof(ProtoWriter));
			MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (!(methodInfo.Name != methodName))
				{
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (parameters.Length == 2 && parameters[1].ParameterType == type)
					{
						return methodInfo;
					}
				}
			}
			throw new ArgumentException("No suitable method found for: " + methodName, "methodName");
		}

		internal void EmitWrite(Type helperType, string methodName, Local valueFrom)
		{
			if (Helpers.IsNullOrEmpty(methodName))
			{
				throw new ArgumentNullException("methodName");
			}
			MethodInfo methodInfo = helperType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo == null || methodInfo.ReturnType != MapType(typeof(void)))
			{
				throw new ArgumentException("methodName");
			}
			LoadValue(valueFrom);
			LoadReaderWriter();
			EmitCall(methodInfo);
		}

		public void EmitCall(MethodInfo method)
		{
			EmitCall(method, null);
		}

		public void EmitCall(MethodInfo method, Type targetType)
		{
			CheckAccessibility(method);
			OpCode opcode;
			if (method.IsStatic || Helpers.IsValueType(method.DeclaringType))
			{
				opcode = OpCodes.Call;
			}
			else
			{
				opcode = OpCodes.Callvirt;
				if (targetType != null && Helpers.IsValueType(targetType) && !Helpers.IsValueType(method.DeclaringType))
				{
					Constrain(targetType);
				}
			}
			il.EmitCall(opcode, method, null);
		}

		public void LoadNullRef()
		{
			Emit(OpCodes.Ldnull);
		}

		internal void WriteNullCheckedTail(Type type, IProtoSerializer tail, Local valueFrom)
		{
			if (Helpers.IsValueType(type))
			{
				if (!(Helpers.GetUnderlyingType(type) == null))
				{
					using (Local local = GetLocalWithValue(type, valueFrom))
					{
						LoadAddress(local, type);
						LoadValue(type.GetProperty("HasValue"));
						CodeLabel label = DefineLabel();
						BranchIfFalse(label, @short: false);
						LoadAddress(local, type);
						EmitCall(type.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
						tail.EmitWrite(this, null);
						MarkLabel(label);
						return;
					}
				}
				tail.EmitWrite(this, valueFrom);
			}
			else
			{
				LoadValue(valueFrom);
				CopyValue();
				CodeLabel label2 = DefineLabel();
				CodeLabel label3 = DefineLabel();
				BranchIfTrue(label2, @short: true);
				DiscardValue();
				Branch(label3, @short: false);
				MarkLabel(label2);
				tail.EmitWrite(this, null);
				MarkLabel(label3);
			}
		}

		internal void ReadNullCheckedTail(Type type, IProtoSerializer tail, Local valueFrom)
		{
			Type underlyingType;
			if (Helpers.IsValueType(type) && (underlyingType = Helpers.GetUnderlyingType(type)) != null)
			{
				if (tail.RequiresOldValue)
				{
					using (Local local = GetLocalWithValue(type, valueFrom))
					{
						LoadAddress(local, type);
						EmitCall(type.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
					}
				}
				tail.EmitRead(this, null);
				if (tail.ReturnsValue)
				{
					EmitCtor(type, underlyingType);
				}
			}
			else
			{
				tail.EmitRead(this, valueFrom);
			}
		}

		public void EmitCtor(Type type)
		{
			EmitCtor(type, Helpers.EmptyTypes);
		}

		public void EmitCtor(ConstructorInfo ctor)
		{
			if (ctor == null)
			{
				throw new ArgumentNullException("ctor");
			}
			CheckAccessibility(ctor);
			il.Emit(OpCodes.Newobj, ctor);
		}

		public void EmitCtor(Type type, params Type[] parameterTypes)
		{
			if (Helpers.IsValueType(type) && parameterTypes.Length == 0)
			{
				il.Emit(OpCodes.Initobj, type);
				return;
			}
			ConstructorInfo constructor = Helpers.GetConstructor(type, parameterTypes, nonPublic: true);
			if (constructor == null)
			{
				throw new InvalidOperationException("No suitable constructor found for " + type.FullName);
			}
			EmitCtor(constructor);
		}

		private bool InternalsVisible(Assembly assembly)
		{
			if (Helpers.IsNullOrEmpty(assemblyName))
			{
				return false;
			}
			if (knownTrustedAssemblies != null && knownTrustedAssemblies.IndexOfReference(assembly) >= 0)
			{
				return true;
			}
			if (knownUntrustedAssemblies != null && knownUntrustedAssemblies.IndexOfReference(assembly) >= 0)
			{
				return false;
			}
			bool flag = false;
			Type type = MapType(typeof(InternalsVisibleToAttribute));
			if (type == null)
			{
				return false;
			}
			object[] customAttributes = assembly.GetCustomAttributes(type, inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				InternalsVisibleToAttribute internalsVisibleToAttribute = (InternalsVisibleToAttribute)customAttributes[i];
				if (internalsVisibleToAttribute.AssemblyName == assemblyName || internalsVisibleToAttribute.AssemblyName.StartsWith(assemblyName + ","))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (knownTrustedAssemblies == null)
				{
					knownTrustedAssemblies = new BasicList();
				}
				knownTrustedAssemblies.Add(assembly);
			}
			else
			{
				if (knownUntrustedAssemblies == null)
				{
					knownUntrustedAssemblies = new BasicList();
				}
				knownUntrustedAssemblies.Add(assembly);
			}
			return flag;
		}

		internal void CheckAccessibility(MemberInfo member)
		{
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			if (NonPublic)
			{
				return;
			}
			MemberTypes memberType = member.MemberType;
			bool flag;
			switch (memberType)
			{
			case MemberTypes.TypeInfo:
			{
				Type type = (Type)member;
				flag = type.IsPublic || InternalsVisible(type.Assembly);
				break;
			}
			case MemberTypes.NestedType:
			{
				Type type = (Type)member;
				do
				{
					flag = type.IsNestedPublic || type.IsPublic || ((type.DeclaringType == null || type.IsNestedAssembly || type.IsNestedFamORAssem) && InternalsVisible(type.Assembly));
				}
				while (flag && (type = type.DeclaringType) != null);
				break;
			}
			case MemberTypes.Field:
			{
				FieldInfo fieldInfo = (FieldInfo)member;
				flag = fieldInfo.IsPublic || ((fieldInfo.IsAssembly || fieldInfo.IsFamilyOrAssembly) && InternalsVisible(fieldInfo.DeclaringType.Assembly));
				break;
			}
			case MemberTypes.Constructor:
			{
				ConstructorInfo constructorInfo = (ConstructorInfo)member;
				flag = constructorInfo.IsPublic || ((constructorInfo.IsAssembly || constructorInfo.IsFamilyOrAssembly) && InternalsVisible(constructorInfo.DeclaringType.Assembly));
				break;
			}
			case MemberTypes.Method:
			{
				MethodInfo methodInfo = (MethodInfo)member;
				flag = methodInfo.IsPublic || ((methodInfo.IsAssembly || methodInfo.IsFamilyOrAssembly) && InternalsVisible(methodInfo.DeclaringType.Assembly));
				if (!flag && (member is MethodBuilder || member.DeclaringType == MapType(typeof(TypeModel))))
				{
					flag = true;
				}
				break;
			}
			case MemberTypes.Property:
				flag = true;
				break;
			default:
				throw new NotSupportedException(memberType.ToString());
			}
			if (!flag)
			{
				if (memberType == MemberTypes.TypeInfo || memberType == MemberTypes.NestedType)
				{
					throw new InvalidOperationException("Non-public type cannot be used with full dll compilation: " + ((Type)member).FullName);
				}
				throw new InvalidOperationException("Non-public member cannot be used with full dll compilation: " + member.DeclaringType.FullName + "." + member.Name);
			}
		}

		public void LoadValue(FieldInfo field)
		{
			CheckAccessibility(field);
			OpCode opcode = (field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld);
			il.Emit(opcode, field);
		}

		public void StoreValue(FieldInfo field)
		{
			CheckAccessibility(field);
			OpCode opcode = (field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld);
			il.Emit(opcode, field);
		}

		public void LoadValue(PropertyInfo property)
		{
			CheckAccessibility(property);
			EmitCall(Helpers.GetGetMethod(property, nonPublic: true, allowInternal: true));
		}

		public void StoreValue(PropertyInfo property)
		{
			CheckAccessibility(property);
			EmitCall(Helpers.GetSetMethod(property, nonPublic: true, allowInternal: true));
		}

		internal static void LoadValue(ILGenerator il, int value)
		{
			switch (value)
			{
			case 0:
				il.Emit(OpCodes.Ldc_I4_0);
				break;
			case 1:
				il.Emit(OpCodes.Ldc_I4_1);
				break;
			case 2:
				il.Emit(OpCodes.Ldc_I4_2);
				break;
			case 3:
				il.Emit(OpCodes.Ldc_I4_3);
				break;
			case 4:
				il.Emit(OpCodes.Ldc_I4_4);
				break;
			case 5:
				il.Emit(OpCodes.Ldc_I4_5);
				break;
			case 6:
				il.Emit(OpCodes.Ldc_I4_6);
				break;
			case 7:
				il.Emit(OpCodes.Ldc_I4_7);
				break;
			case 8:
				il.Emit(OpCodes.Ldc_I4_8);
				break;
			case -1:
				il.Emit(OpCodes.Ldc_I4_M1);
				break;
			default:
				il.Emit(OpCodes.Ldc_I4, value);
				break;
			}
		}

		private bool UseShortForm(Local local)
		{
			return local.Value.LocalIndex < 256;
		}

		internal void LoadAddress(Local local, Type type)
		{
			if (Helpers.IsValueType(type))
			{
				if (local == null)
				{
					throw new InvalidOperationException("Cannot load the address of a struct at the head of the stack");
				}
				if (local == InputValue)
				{
					il.Emit(OpCodes.Ldarga_S, (!isStatic) ? ((byte)1) : ((byte)0));
					return;
				}
				OpCode opcode = (UseShortForm(local) ? OpCodes.Ldloca_S : OpCodes.Ldloca);
				il.Emit(opcode, local.Value);
			}
			else
			{
				LoadValue(local);
			}
		}

		internal void Branch(CodeLabel label, bool @short)
		{
			OpCode opcode = (@short ? OpCodes.Br_S : OpCodes.Br);
			il.Emit(opcode, label.Value);
		}

		internal void BranchIfFalse(CodeLabel label, bool @short)
		{
			OpCode opcode = (@short ? OpCodes.Brfalse_S : OpCodes.Brfalse);
			il.Emit(opcode, label.Value);
		}

		internal void BranchIfTrue(CodeLabel label, bool @short)
		{
			OpCode opcode = (@short ? OpCodes.Brtrue_S : OpCodes.Brtrue);
			il.Emit(opcode, label.Value);
		}

		internal void BranchIfEqual(CodeLabel label, bool @short)
		{
			OpCode opcode = (@short ? OpCodes.Beq_S : OpCodes.Beq);
			il.Emit(opcode, label.Value);
		}

		internal void CopyValue()
		{
			Emit(OpCodes.Dup);
		}

		internal void BranchIfGreater(CodeLabel label, bool @short)
		{
			OpCode opcode = (@short ? OpCodes.Bgt_S : OpCodes.Bgt);
			il.Emit(opcode, label.Value);
		}

		internal void BranchIfLess(CodeLabel label, bool @short)
		{
			OpCode opcode = (@short ? OpCodes.Blt_S : OpCodes.Blt);
			il.Emit(opcode, label.Value);
		}

		internal void DiscardValue()
		{
			Emit(OpCodes.Pop);
		}

		public void Subtract()
		{
			Emit(OpCodes.Sub);
		}

		public void Switch(CodeLabel[] jumpTable)
		{
			if (jumpTable.Length <= 128)
			{
				Label[] array = new Label[jumpTable.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = jumpTable[i].Value;
				}
				il.Emit(OpCodes.Switch, array);
				return;
			}
			using (Local local = GetLocalWithValue(MapType(typeof(int)), null))
			{
				int num = jumpTable.Length;
				int num2 = 0;
				int num3 = num / 128;
				if (num % 128 != 0)
				{
					num3++;
				}
				Label[] array2 = new Label[num3];
				for (int j = 0; j < num3; j++)
				{
					array2[j] = il.DefineLabel();
				}
				CodeLabel label = DefineLabel();
				LoadValue(local);
				LoadValue(128);
				Emit(OpCodes.Div);
				il.Emit(OpCodes.Switch, array2);
				Branch(label, @short: false);
				Label[] array3 = new Label[128];
				for (int k = 0; k < num3; k++)
				{
					il.MarkLabel(array2[k]);
					int num4 = Math.Min(128, num);
					num -= num4;
					if (array3.Length != num4)
					{
						array3 = new Label[num4];
					}
					int num5 = num2;
					for (int l = 0; l < num4; l++)
					{
						array3[l] = jumpTable[num2++].Value;
					}
					LoadValue(local);
					if (num5 != 0)
					{
						LoadValue(num5);
						Emit(OpCodes.Sub);
					}
					il.Emit(OpCodes.Switch, array3);
					if (num != 0)
					{
						Branch(label, @short: false);
					}
				}
				MarkLabel(label);
			}
		}

		internal void EndFinally()
		{
			il.EndExceptionBlock();
		}

		internal void BeginFinally()
		{
			il.BeginFinallyBlock();
		}

		internal void EndTry(CodeLabel label, bool @short)
		{
			OpCode opcode = (@short ? OpCodes.Leave_S : OpCodes.Leave);
			il.Emit(opcode, label.Value);
		}

		internal CodeLabel BeginTry()
		{
			return new CodeLabel(il.BeginExceptionBlock(), nextLabel++);
		}

		internal void Constrain(Type type)
		{
			il.Emit(OpCodes.Constrained, type);
		}

		internal void TryCast(Type type)
		{
			il.Emit(OpCodes.Isinst, type);
		}

		internal void Cast(Type type)
		{
			il.Emit(OpCodes.Castclass, type);
		}

		public IDisposable Using(Local local)
		{
			return new UsingBlock(this, local);
		}

		internal void Add()
		{
			Emit(OpCodes.Add);
		}

		internal void LoadLength(Local arr, bool zeroIfNull)
		{
			if (zeroIfNull)
			{
				CodeLabel label = DefineLabel();
				CodeLabel label2 = DefineLabel();
				LoadValue(arr);
				CopyValue();
				BranchIfTrue(label, @short: true);
				DiscardValue();
				LoadValue(0);
				Branch(label2, @short: true);
				MarkLabel(label);
				Emit(OpCodes.Ldlen);
				Emit(OpCodes.Conv_I4);
				MarkLabel(label2);
			}
			else
			{
				LoadValue(arr);
				Emit(OpCodes.Ldlen);
				Emit(OpCodes.Conv_I4);
			}
		}

		internal void CreateArray(Type elementType, Local length)
		{
			LoadValue(length);
			il.Emit(OpCodes.Newarr, elementType);
		}

		internal void LoadArrayValue(Local arr, Local i)
		{
			Type type = arr.Type;
			type = type.GetElementType();
			LoadValue(arr);
			LoadValue(i);
			switch (Helpers.GetTypeCode(type))
			{
			case ProtoTypeCode.SByte:
				Emit(OpCodes.Ldelem_I1);
				return;
			case ProtoTypeCode.Int16:
				Emit(OpCodes.Ldelem_I2);
				return;
			case ProtoTypeCode.Int32:
				Emit(OpCodes.Ldelem_I4);
				return;
			case ProtoTypeCode.Int64:
				Emit(OpCodes.Ldelem_I8);
				return;
			case ProtoTypeCode.Byte:
				Emit(OpCodes.Ldelem_U1);
				return;
			case ProtoTypeCode.UInt16:
				Emit(OpCodes.Ldelem_U2);
				return;
			case ProtoTypeCode.UInt32:
				Emit(OpCodes.Ldelem_U4);
				return;
			case ProtoTypeCode.UInt64:
				Emit(OpCodes.Ldelem_I8);
				return;
			case ProtoTypeCode.Single:
				Emit(OpCodes.Ldelem_R4);
				return;
			case ProtoTypeCode.Double:
				Emit(OpCodes.Ldelem_R8);
				return;
			}
			if (Helpers.IsValueType(type))
			{
				il.Emit(OpCodes.Ldelema, type);
				il.Emit(OpCodes.Ldobj, type);
			}
			else
			{
				Emit(OpCodes.Ldelem_Ref);
			}
		}

		internal void LoadValue(Type type)
		{
			il.Emit(OpCodes.Ldtoken, type);
			EmitCall(MapType(typeof(Type)).GetMethod("GetTypeFromHandle"));
		}

		internal void ConvertToInt32(ProtoTypeCode typeCode, bool uint32Overflow)
		{
			switch (typeCode)
			{
			case ProtoTypeCode.SByte:
			case ProtoTypeCode.Byte:
			case ProtoTypeCode.Int16:
			case ProtoTypeCode.UInt16:
				Emit(OpCodes.Conv_I4);
				break;
			case ProtoTypeCode.Int64:
				Emit(OpCodes.Conv_Ovf_I4);
				break;
			case ProtoTypeCode.UInt32:
				Emit(uint32Overflow ? OpCodes.Conv_Ovf_I4_Un : OpCodes.Conv_Ovf_I4);
				break;
			case ProtoTypeCode.UInt64:
				Emit(OpCodes.Conv_Ovf_I4_Un);
				break;
			default:
				throw new InvalidOperationException("ConvertToInt32 not implemented for: " + typeCode);
			case ProtoTypeCode.Int32:
				break;
			}
		}

		internal void ConvertFromInt32(ProtoTypeCode typeCode, bool uint32Overflow)
		{
			switch (typeCode)
			{
			case ProtoTypeCode.SByte:
				Emit(OpCodes.Conv_Ovf_I1);
				break;
			case ProtoTypeCode.Byte:
				Emit(OpCodes.Conv_Ovf_U1);
				break;
			case ProtoTypeCode.Int16:
				Emit(OpCodes.Conv_Ovf_I2);
				break;
			case ProtoTypeCode.UInt16:
				Emit(OpCodes.Conv_Ovf_U2);
				break;
			case ProtoTypeCode.UInt32:
				Emit(uint32Overflow ? OpCodes.Conv_Ovf_U4 : OpCodes.Conv_U4);
				break;
			case ProtoTypeCode.Int64:
				Emit(OpCodes.Conv_I8);
				break;
			case ProtoTypeCode.UInt64:
				Emit(OpCodes.Conv_U8);
				break;
			default:
				throw new InvalidOperationException();
			case ProtoTypeCode.Int32:
				break;
			}
		}

		internal void LoadValue(decimal value)
		{
			if (value == 0m)
			{
				LoadValue(typeof(decimal).GetField("Zero"));
				return;
			}
			int[] bits = decimal.GetBits(value);
			LoadValue(bits[0]);
			LoadValue(bits[1]);
			LoadValue(bits[2]);
			LoadValue((int)((uint)bits[3] >> 31));
			LoadValue((bits[3] >> 16) & 0xFF);
			EmitCtor(MapType(typeof(decimal)), MapType(typeof(int)), MapType(typeof(int)), MapType(typeof(int)), MapType(typeof(bool)), MapType(typeof(byte)));
		}

		internal void LoadValue(Guid value)
		{
			if (value == Guid.Empty)
			{
				LoadValue(typeof(Guid).GetField("Empty"));
				return;
			}
			byte[] array = value.ToByteArray();
			int value2 = array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
			LoadValue(value2);
			short value3 = (short)(array[4] | (array[5] << 8));
			LoadValue(value3);
			value3 = (short)(array[6] | (array[7] << 8));
			LoadValue(value3);
			for (value2 = 8; value2 <= 15; value2++)
			{
				LoadValue(array[value2]);
			}
			EmitCtor(MapType(typeof(Guid)), MapType(typeof(int)), MapType(typeof(short)), MapType(typeof(short)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)));
		}

		internal void LoadSerializationContext()
		{
			LoadReaderWriter();
			LoadValue((isWriter ? typeof(ProtoWriter) : typeof(ProtoReader)).GetProperty("Context"));
		}

		internal Type MapType(Type type)
		{
			return model.MapType(type);
		}

		internal bool AllowInternal(PropertyInfo property)
		{
			if (!NonPublic)
			{
				return InternalsVisible(Helpers.GetAssembly(property.DeclaringType));
			}
			return true;
		}
	}
}
