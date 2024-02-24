using System;
using System.Runtime.InteropServices;

namespace Discord
{
	public class RelationshipManager
	{
		internal struct FFIEvents
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void RefreshHandler(IntPtr ptr);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void RelationshipUpdateHandler(IntPtr ptr, ref Relationship relationship);

			internal RefreshHandler OnRefresh;

			internal RelationshipUpdateHandler OnRelationshipUpdate;
		}

		internal struct FFIMethods
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate bool FilterCallback(IntPtr ptr, ref Relationship relationship);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void FilterMethod(IntPtr methodsPtr, IntPtr callbackData, FilterCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result CountMethod(IntPtr methodsPtr, ref int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetMethod(IntPtr methodsPtr, long userId, ref Relationship relationship);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetAtMethod(IntPtr methodsPtr, uint index, ref Relationship relationship);

			internal FilterMethod Filter;

			internal CountMethod Count;

			internal GetMethod Get;

			internal GetAtMethod GetAt;
		}

		public delegate bool FilterHandler(ref Relationship relationship);

		public delegate void RefreshHandler();

		public delegate void RelationshipUpdateHandler(ref Relationship relationship);

		private IntPtr MethodsPtr;

		private FFIMethods Methods => (FFIMethods)Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));

		public event RefreshHandler OnRefresh;

		public event RelationshipUpdateHandler OnRelationshipUpdate;

		internal RelationshipManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
		{
			if (eventsPtr == IntPtr.Zero)
			{
				throw new ResultException(Result.InternalError);
			}
			InitEvents(eventsPtr, ref events);
			MethodsPtr = ptr;
			if (MethodsPtr == IntPtr.Zero)
			{
				throw new ResultException(Result.InternalError);
			}
		}

		private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
		{
			events.OnRefresh = delegate
			{
				if (this.OnRefresh != null)
				{
					this.OnRefresh();
				}
			};
			events.OnRelationshipUpdate = delegate(IntPtr ptr, ref Relationship relationship)
			{
				if (this.OnRelationshipUpdate != null)
				{
					this.OnRelationshipUpdate(ref relationship);
				}
			};
			Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
		}

		public void Filter(FilterHandler callback)
		{
			FFIMethods.FilterCallback callback2 = delegate(IntPtr ptr, ref Relationship relationship)
			{
				return callback(ref relationship);
			};
			Methods.Filter(MethodsPtr, IntPtr.Zero, callback2);
		}

		public int Count()
		{
			int count = 0;
			Result result = Methods.Count(MethodsPtr, ref count);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return count;
		}

		public Relationship Get(long userId)
		{
			Relationship relationship = default(Relationship);
			Result result = Methods.Get(MethodsPtr, userId, ref relationship);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return relationship;
		}

		public Relationship GetAt(uint index)
		{
			Relationship relationship = default(Relationship);
			Result result = Methods.GetAt(MethodsPtr, index, ref relationship);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return relationship;
		}
	}
}
