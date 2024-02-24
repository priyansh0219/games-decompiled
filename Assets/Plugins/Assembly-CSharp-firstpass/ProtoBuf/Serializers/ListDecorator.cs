using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf.Compiler;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
	internal class ListDecorator : ProtoDecoratorBase
	{
		private readonly byte options;

		private const byte OPTIONS_IsList = 1;

		private const byte OPTIONS_SuppressIList = 2;

		private const byte OPTIONS_WritePacked = 4;

		private const byte OPTIONS_ReturnList = 8;

		private const byte OPTIONS_OverwriteList = 16;

		private const byte OPTIONS_SupportNull = 32;

		private readonly Type declaredType;

		private readonly Type concreteType;

		private readonly MethodInfo add;

		private readonly int fieldNumber;

		protected readonly WireType packedWireType;

		private static readonly Type ienumeratorType = typeof(IEnumerator);

		private static readonly Type ienumerableType = typeof(IEnumerable);

		private bool IsList => (options & 1) != 0;

		private bool SuppressIList => (options & 2) != 0;

		private bool WritePacked => (options & 4) != 0;

		private bool SupportNull => (options & 0x20) != 0;

		private bool ReturnList => (options & 8) != 0;

		protected virtual bool RequireAdd => true;

		public override Type ExpectedType => declaredType;

		public override bool RequiresOldValue => AppendToCollection;

		public override bool ReturnsValue => ReturnList;

		protected bool AppendToCollection => (options & 0x10) == 0;

		internal static bool CanPack(WireType wireType)
		{
			if ((uint)wireType <= 1u || wireType == WireType.Fixed32 || wireType == WireType.SignedVariant)
			{
				return true;
			}
			return false;
		}

		internal static ListDecorator Create(TypeModel model, Type declaredType, Type concreteType, IProtoSerializer tail, int fieldNumber, bool writePacked, WireType packedWireType, bool returnList, bool overwriteList, bool supportNull)
		{
			if (returnList && ImmutableCollectionDecorator.IdentifyImmutable(model, declaredType, out var builderFactory, out var methodInfo, out var addRange, out var finish))
			{
				return new ImmutableCollectionDecorator(model, declaredType, concreteType, tail, fieldNumber, writePacked, packedWireType, returnList, overwriteList, supportNull, builderFactory, methodInfo, addRange, finish);
			}
			return new ListDecorator(model, declaredType, concreteType, tail, fieldNumber, writePacked, packedWireType, returnList, overwriteList, supportNull);
		}

		protected ListDecorator(TypeModel model, Type declaredType, Type concreteType, IProtoSerializer tail, int fieldNumber, bool writePacked, WireType packedWireType, bool returnList, bool overwriteList, bool supportNull)
			: base(tail)
		{
			if (returnList)
			{
				options |= 8;
			}
			if (overwriteList)
			{
				options |= 16;
			}
			if (supportNull)
			{
				options |= 32;
			}
			if ((writePacked || packedWireType != WireType.None) && fieldNumber <= 0)
			{
				throw new ArgumentOutOfRangeException("fieldNumber");
			}
			if (!CanPack(packedWireType))
			{
				if (writePacked)
				{
					throw new InvalidOperationException("Only simple data-types can use packed encoding");
				}
				packedWireType = WireType.None;
			}
			this.fieldNumber = fieldNumber;
			if (writePacked)
			{
				options |= 4;
			}
			this.packedWireType = packedWireType;
			if (declaredType == null)
			{
				throw new ArgumentNullException("declaredType");
			}
			if (declaredType.IsArray)
			{
				throw new ArgumentException("Cannot treat arrays as lists", "declaredType");
			}
			this.declaredType = declaredType;
			this.concreteType = concreteType;
			if (!RequireAdd)
			{
				return;
			}
			add = TypeModel.ResolveListAdd(model, declaredType, tail.ExpectedType, out var isList);
			if (isList)
			{
				options |= 1;
				string fullName = declaredType.FullName;
				if (fullName != null && fullName.StartsWith("System.Data.Linq.EntitySet`1[["))
				{
					options |= 2;
				}
			}
			if (add == null)
			{
				throw new InvalidOperationException("Unable to resolve a suitable Add method for " + declaredType.FullName);
			}
		}

		protected override void EmitRead(CompilerContext ctx, Local valueFrom)
		{
			bool returnList = ReturnList;
			using (Local local = (AppendToCollection ? ctx.GetLocalWithValue(ExpectedType, valueFrom) : new Local(ctx, declaredType)))
			{
				using (Local local2 = ((returnList && AppendToCollection && !Helpers.IsValueType(ExpectedType)) ? new Local(ctx, ExpectedType) : null))
				{
					if (!AppendToCollection)
					{
						ctx.LoadNullRef();
						ctx.StoreValue(local);
					}
					else if (returnList && local2 != null)
					{
						ctx.LoadValue(local);
						ctx.StoreValue(local2);
					}
					if (concreteType != null)
					{
						ctx.LoadValue(local);
						CodeLabel label = ctx.DefineLabel();
						ctx.BranchIfTrue(label, @short: true);
						ctx.EmitCtor(concreteType);
						ctx.StoreValue(local);
						ctx.MarkLabel(label);
					}
					bool castListForAdd = !add.DeclaringType.IsAssignableFrom(declaredType);
					EmitReadList(ctx, local, Tail, add, packedWireType, castListForAdd);
					if (returnList)
					{
						if (AppendToCollection && local2 != null)
						{
							ctx.LoadValue(local2);
							ctx.LoadValue(local);
							CodeLabel label2 = ctx.DefineLabel();
							CodeLabel label3 = ctx.DefineLabel();
							ctx.BranchIfEqual(label2, @short: true);
							ctx.LoadValue(local);
							ctx.Branch(label3, @short: true);
							ctx.MarkLabel(label2);
							ctx.LoadNullRef();
							ctx.MarkLabel(label3);
						}
						else
						{
							ctx.LoadValue(local);
						}
					}
				}
			}
		}

		internal static void EmitReadList(CompilerContext ctx, Local list, IProtoSerializer tail, MethodInfo add, WireType packedWireType, bool castListForAdd)
		{
			using (Local local = new Local(ctx, ctx.MapType(typeof(int))))
			{
				CodeLabel label = ((packedWireType == WireType.None) ? default(CodeLabel) : ctx.DefineLabel());
				if (packedWireType != WireType.None)
				{
					ctx.LoadReaderWriter();
					ctx.LoadValue(typeof(ProtoReader).GetProperty("WireType"));
					ctx.LoadValue(2);
					ctx.BranchIfEqual(label, @short: false);
				}
				ctx.LoadReaderWriter();
				ctx.LoadValue(typeof(ProtoReader).GetProperty("FieldNumber"));
				ctx.StoreValue(local);
				CodeLabel label2 = ctx.DefineLabel();
				ctx.MarkLabel(label2);
				EmitReadAndAddItem(ctx, list, tail, add, castListForAdd);
				ctx.LoadReaderWriter();
				ctx.LoadValue(local);
				ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("TryReadFieldHeader"));
				ctx.BranchIfTrue(label2, @short: false);
				if (packedWireType != WireType.None)
				{
					CodeLabel label3 = ctx.DefineLabel();
					ctx.Branch(label3, @short: false);
					ctx.MarkLabel(label);
					ctx.LoadReaderWriter();
					ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("StartSubItem"));
					CodeLabel label4 = ctx.DefineLabel();
					CodeLabel label5 = ctx.DefineLabel();
					ctx.MarkLabel(label4);
					ctx.LoadValue((int)packedWireType);
					ctx.LoadReaderWriter();
					ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("HasSubValue"));
					ctx.BranchIfFalse(label5, @short: false);
					EmitReadAndAddItem(ctx, list, tail, add, castListForAdd);
					ctx.Branch(label4, @short: false);
					ctx.MarkLabel(label5);
					ctx.LoadReaderWriter();
					ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("EndSubItem"));
					ctx.MarkLabel(label3);
				}
			}
		}

		private static void EmitReadAndAddItem(CompilerContext ctx, Local list, IProtoSerializer tail, MethodInfo add, bool castListForAdd)
		{
			ctx.LoadAddress(list, list.Type);
			if (castListForAdd)
			{
				ctx.Cast(add.DeclaringType);
			}
			Type expectedType = tail.ExpectedType;
			bool returnsValue = tail.ReturnsValue;
			if (tail.RequiresOldValue)
			{
				if (Helpers.IsValueType(expectedType) || !returnsValue)
				{
					using (Local local = new Local(ctx, expectedType))
					{
						if (Helpers.IsValueType(expectedType))
						{
							ctx.LoadAddress(local, expectedType);
							ctx.EmitCtor(expectedType);
						}
						else
						{
							ctx.LoadNullRef();
							ctx.StoreValue(local);
						}
						tail.EmitRead(ctx, local);
						if (!returnsValue)
						{
							ctx.LoadValue(local);
						}
					}
				}
				else
				{
					ctx.LoadNullRef();
					tail.EmitRead(ctx, null);
				}
			}
			else
			{
				if (!returnsValue)
				{
					throw new InvalidOperationException();
				}
				tail.EmitRead(ctx, null);
			}
			Type parameterType = add.GetParameters()[0].ParameterType;
			if (parameterType != expectedType)
			{
				if (parameterType == ctx.MapType(typeof(object)))
				{
					ctx.CastToObject(expectedType);
				}
				else
				{
					if (!(Helpers.GetUnderlyingType(parameterType) == expectedType))
					{
						throw new InvalidOperationException("Conflicting item/add type");
					}
					ConstructorInfo constructor = Helpers.GetConstructor(parameterType, new Type[1] { expectedType }, nonPublic: false);
					ctx.EmitCtor(constructor);
				}
			}
			ctx.EmitCall(add, list.Type);
			if (add.ReturnType != ctx.MapType(typeof(void)))
			{
				ctx.DiscardValue();
			}
		}

		protected MethodInfo GetEnumeratorInfo(TypeModel model, out MethodInfo moveNext, out MethodInfo current)
		{
			Type type = null;
			Type expectedType = ExpectedType;
			MethodInfo instanceMethod = Helpers.GetInstanceMethod(expectedType, "GetEnumerator", null);
			Type expectedType2 = Tail.ExpectedType;
			Type returnType;
			if (instanceMethod != null)
			{
				returnType = instanceMethod.ReturnType;
				moveNext = Helpers.GetInstanceMethod(returnType, "MoveNext", null);
				PropertyInfo property = Helpers.GetProperty(returnType, "Current", nonPublic: false);
				current = ((property == null) ? null : Helpers.GetGetMethod(property, nonPublic: false, allowInternal: false));
				if (moveNext == null && model.MapType(ienumeratorType).IsAssignableFrom(returnType))
				{
					moveNext = Helpers.GetInstanceMethod(model.MapType(ienumeratorType), "MoveNext", null);
				}
				if (moveNext != null && moveNext.ReturnType == model.MapType(typeof(bool)) && current != null && current.ReturnType == expectedType2)
				{
					return instanceMethod;
				}
				moveNext = (current = (instanceMethod = null));
			}
			Type type2 = model.MapType(typeof(IEnumerable<>), demand: false);
			if (type2 != null)
			{
				type2 = type2.MakeGenericType(expectedType2);
				type = type2;
			}
			if (type != null && type.IsAssignableFrom(expectedType))
			{
				instanceMethod = Helpers.GetInstanceMethod(type, "GetEnumerator");
				returnType = instanceMethod.ReturnType;
				moveNext = Helpers.GetInstanceMethod(model.MapType(ienumeratorType), "MoveNext");
				current = Helpers.GetGetMethod(Helpers.GetProperty(returnType, "Current", nonPublic: false), nonPublic: false, allowInternal: false);
				return instanceMethod;
			}
			type = model.MapType(ienumerableType);
			instanceMethod = Helpers.GetInstanceMethod(type, "GetEnumerator");
			returnType = instanceMethod.ReturnType;
			moveNext = Helpers.GetInstanceMethod(returnType, "MoveNext");
			current = Helpers.GetGetMethod(Helpers.GetProperty(returnType, "Current", nonPublic: false), nonPublic: false, allowInternal: false);
			return instanceMethod;
		}

		protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
		{
			using (Local local = ctx.GetLocalWithValue(ExpectedType, valueFrom))
			{
				MethodInfo moveNext;
				MethodInfo current;
				MethodInfo enumeratorInfo = GetEnumeratorInfo(ctx.Model, out moveNext, out current);
				Type returnType = enumeratorInfo.ReturnType;
				bool writePacked = WritePacked;
				using (Local local3 = new Local(ctx, returnType))
				{
					using (Local local2 = (writePacked ? new Local(ctx, ctx.MapType(typeof(SubItemToken))) : null))
					{
						if (writePacked)
						{
							ctx.LoadValue(fieldNumber);
							ctx.LoadValue(2);
							ctx.LoadReaderWriter();
							ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("WriteFieldHeader"));
							ctx.LoadValue(local);
							ctx.LoadReaderWriter();
							ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("StartSubItem"));
							ctx.StoreValue(local2);
							ctx.LoadValue(fieldNumber);
							ctx.LoadReaderWriter();
							ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("SetPackedField"));
						}
						ctx.LoadAddress(local, ExpectedType);
						ctx.EmitCall(enumeratorInfo, ExpectedType);
						ctx.StoreValue(local3);
						using (ctx.Using(local3))
						{
							CodeLabel label = ctx.DefineLabel();
							CodeLabel label2 = ctx.DefineLabel();
							ctx.Branch(label2, @short: false);
							ctx.MarkLabel(label);
							ctx.LoadAddress(local3, returnType);
							ctx.EmitCall(current, returnType);
							Type expectedType = Tail.ExpectedType;
							if (expectedType != ctx.MapType(typeof(object)) && current.ReturnType == ctx.MapType(typeof(object)))
							{
								ctx.CastFromObject(expectedType);
							}
							Tail.EmitWrite(ctx, null);
							ctx.MarkLabel(label2);
							ctx.LoadAddress(local3, returnType);
							ctx.EmitCall(moveNext, returnType);
							ctx.BranchIfTrue(label, @short: false);
						}
						if (writePacked)
						{
							ctx.LoadValue(local2);
							ctx.LoadReaderWriter();
							ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("EndSubItem"));
						}
					}
				}
			}
		}

		public override void Write(object value, ProtoWriter dest)
		{
			bool writePacked = WritePacked;
			bool flag = (writePacked & CanUsePackedPrefix(value)) && value is ICollection;
			SubItemToken token;
			if (writePacked)
			{
				ProtoWriter.WriteFieldHeader(fieldNumber, WireType.String, dest);
				if (flag)
				{
					ProtoWriter.WritePackedPrefix(((ICollection)value).Count, packedWireType, dest);
					token = default(SubItemToken);
				}
				else
				{
					token = ProtoWriter.StartSubItem(value, dest);
				}
				ProtoWriter.SetPackedField(fieldNumber, dest);
			}
			else
			{
				token = default(SubItemToken);
			}
			bool flag2 = !SupportNull;
			foreach (object item in (IEnumerable)value)
			{
				if (flag2 && item == null)
				{
					throw new NullReferenceException();
				}
				Tail.Write(item, dest);
			}
			if (writePacked)
			{
				if (flag)
				{
					ProtoWriter.ClearPackedField(fieldNumber, dest);
				}
				else
				{
					ProtoWriter.EndSubItem(token, dest);
				}
			}
		}

		private bool CanUsePackedPrefix(object obj)
		{
			return ArrayDecorator.CanUsePackedPrefix(packedWireType, Tail.ExpectedType);
		}

		public override object Read(object value, ProtoReader source)
		{
			try
			{
				int field = source.FieldNumber;
				object obj = value;
				if (value == null)
				{
					value = Activator.CreateInstance(concreteType);
				}
				bool flag = IsList && !SuppressIList;
				if (packedWireType != WireType.None && source.WireType == WireType.String)
				{
					SubItemToken token = ProtoReader.StartSubItem(source);
					if (flag)
					{
						IList list = (IList)value;
						while (ProtoReader.HasSubValue(packedWireType, source))
						{
							list.Add(Tail.Read(null, source));
						}
					}
					else
					{
						object[] array = new object[1];
						while (ProtoReader.HasSubValue(packedWireType, source))
						{
							array[0] = Tail.Read(null, source);
							add.Invoke(value, array);
						}
					}
					ProtoReader.EndSubItem(token, source);
				}
				else if (flag)
				{
					IList list2 = (IList)value;
					do
					{
						list2.Add(Tail.Read(null, source));
					}
					while (source.TryReadFieldHeader(field));
				}
				else
				{
					object[] array2 = new object[1];
					do
					{
						array2[0] = Tail.Read(null, source);
						add.Invoke(value, array2);
					}
					while (source.TryReadFieldHeader(field));
				}
				return (obj == value) ? null : value;
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
				{
					throw ex.InnerException;
				}
				throw;
			}
		}
	}
}
