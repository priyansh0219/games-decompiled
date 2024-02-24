using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Discord
{
	public class StoreManager
	{
		internal struct FFIEvents
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void EntitlementCreateHandler(IntPtr ptr, ref Entitlement entitlement);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void EntitlementDeleteHandler(IntPtr ptr, ref Entitlement entitlement);

			internal EntitlementCreateHandler OnEntitlementCreate;

			internal EntitlementDeleteHandler OnEntitlementDelete;
		}

		internal struct FFIMethods
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void FetchSkusCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void FetchSkusMethod(IntPtr methodsPtr, IntPtr callbackData, FetchSkusCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void CountSkusMethod(IntPtr methodsPtr, ref int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetSkuMethod(IntPtr methodsPtr, long skuId, ref Sku sku);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetSkuAtMethod(IntPtr methodsPtr, int index, ref Sku sku);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void FetchEntitlementsCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void FetchEntitlementsMethod(IntPtr methodsPtr, IntPtr callbackData, FetchEntitlementsCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void CountEntitlementsMethod(IntPtr methodsPtr, ref int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetEntitlementMethod(IntPtr methodsPtr, long entitlementId, ref Entitlement entitlement);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetEntitlementAtMethod(IntPtr methodsPtr, int index, ref Entitlement entitlement);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result HasSkuEntitlementMethod(IntPtr methodsPtr, long skuId, ref bool hasEntitlement);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void StartPurchaseCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void StartPurchaseMethod(IntPtr methodsPtr, long skuId, IntPtr callbackData, StartPurchaseCallback callback);

			internal FetchSkusMethod FetchSkus;

			internal CountSkusMethod CountSkus;

			internal GetSkuMethod GetSku;

			internal GetSkuAtMethod GetSkuAt;

			internal FetchEntitlementsMethod FetchEntitlements;

			internal CountEntitlementsMethod CountEntitlements;

			internal GetEntitlementMethod GetEntitlement;

			internal GetEntitlementAtMethod GetEntitlementAt;

			internal HasSkuEntitlementMethod HasSkuEntitlement;

			internal StartPurchaseMethod StartPurchase;
		}

		public delegate void FetchSkusHandler(Result result);

		public delegate void FetchEntitlementsHandler(Result result);

		public delegate void StartPurchaseHandler(Result result);

		public delegate void EntitlementCreateHandler(ref Entitlement entitlement);

		public delegate void EntitlementDeleteHandler(ref Entitlement entitlement);

		private IntPtr MethodsPtr;

		private FFIMethods Methods => (FFIMethods)Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));

		public event EntitlementCreateHandler OnEntitlementCreate;

		public event EntitlementDeleteHandler OnEntitlementDelete;

		internal StoreManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
			events.OnEntitlementCreate = delegate(IntPtr ptr, ref Entitlement entitlement)
			{
				if (this.OnEntitlementCreate != null)
				{
					this.OnEntitlementCreate(ref entitlement);
				}
			};
			events.OnEntitlementDelete = delegate(IntPtr ptr, ref Entitlement entitlement)
			{
				if (this.OnEntitlementDelete != null)
				{
					this.OnEntitlementDelete(ref entitlement);
				}
			};
			Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
		}

		public void FetchSkus(FetchSkusHandler callback)
		{
			FFIMethods.FetchSkusCallback fetchSkusCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.FetchSkus(MethodsPtr, Utility.Retain(fetchSkusCallback), fetchSkusCallback);
		}

		public int CountSkus()
		{
			int count = 0;
			Methods.CountSkus(MethodsPtr, ref count);
			return count;
		}

		public Sku GetSku(long skuId)
		{
			Sku sku = default(Sku);
			Result result = Methods.GetSku(MethodsPtr, skuId, ref sku);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return sku;
		}

		public Sku GetSkuAt(int index)
		{
			Sku sku = default(Sku);
			Result result = Methods.GetSkuAt(MethodsPtr, index, ref sku);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return sku;
		}

		public void FetchEntitlements(FetchEntitlementsHandler callback)
		{
			FFIMethods.FetchEntitlementsCallback fetchEntitlementsCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.FetchEntitlements(MethodsPtr, Utility.Retain(fetchEntitlementsCallback), fetchEntitlementsCallback);
		}

		public int CountEntitlements()
		{
			int count = 0;
			Methods.CountEntitlements(MethodsPtr, ref count);
			return count;
		}

		public Entitlement GetEntitlement(long entitlementId)
		{
			Entitlement entitlement = default(Entitlement);
			Result result = Methods.GetEntitlement(MethodsPtr, entitlementId, ref entitlement);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return entitlement;
		}

		public Entitlement GetEntitlementAt(int index)
		{
			Entitlement entitlement = default(Entitlement);
			Result result = Methods.GetEntitlementAt(MethodsPtr, index, ref entitlement);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return entitlement;
		}

		public bool HasSkuEntitlement(long skuId)
		{
			bool hasEntitlement = false;
			Result result = Methods.HasSkuEntitlement(MethodsPtr, skuId, ref hasEntitlement);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return hasEntitlement;
		}

		public void StartPurchase(long skuId, StartPurchaseHandler callback)
		{
			FFIMethods.StartPurchaseCallback startPurchaseCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.StartPurchase(MethodsPtr, skuId, Utility.Retain(startPurchaseCallback), startPurchaseCallback);
		}

		public IEnumerable<Entitlement> GetEntitlements()
		{
			int num = CountEntitlements();
			List<Entitlement> list = new List<Entitlement>();
			for (int i = 0; i < num; i++)
			{
				list.Add(GetEntitlementAt(i));
			}
			return list;
		}

		public IEnumerable<Sku> GetSkus()
		{
			int num = CountSkus();
			List<Sku> list = new List<Sku>();
			for (int i = 0; i < num; i++)
			{
				list.Add(GetSkuAt(i));
			}
			return list;
		}
	}
}
