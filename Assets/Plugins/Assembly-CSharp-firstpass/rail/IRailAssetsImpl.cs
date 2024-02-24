using System;
using System.Collections.Generic;

namespace rail
{
	public class IRailAssetsImpl : RailObject, IRailAssets, IRailComponent
	{
		internal IRailAssetsImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailAssetsImpl()
		{
		}

		public virtual RailResult AsyncRequestAllAssets(string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncRequestAllAssets(swigCPtr_, user_data);
		}

		public virtual RailResult QueryAssetInfo(ulong asset_id, RailAssetInfo asset_info)
		{
			IntPtr intPtr = ((asset_info == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailAssetInfo__SWIG_0());
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailAssets_QueryAssetInfo(swigCPtr_, asset_id, intPtr);
			}
			finally
			{
				if (asset_info != null)
				{
					RailConverter.Cpp2Csharp(intPtr, asset_info);
				}
				RAIL_API_PINVOKE.delete_RailAssetInfo(intPtr);
			}
		}

		public virtual RailResult AsyncUpdateAssetsProperty(List<RailAssetProperty> asset_property_list, string user_data)
		{
			IntPtr intPtr = ((asset_property_list == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailAssetProperty__SWIG_0());
			if (asset_property_list != null)
			{
				RailConverter.Csharp2Cpp(asset_property_list, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncUpdateAssetsProperty(swigCPtr_, intPtr, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRailAssetProperty(intPtr);
			}
		}

		public virtual RailResult AsyncDirectConsumeAssets(List<RailAssetItem> assets, string user_data)
		{
			IntPtr intPtr = ((assets == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailAssetItem__SWIG_0());
			if (assets != null)
			{
				RailConverter.Csharp2Cpp(assets, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncDirectConsumeAssets(swigCPtr_, intPtr, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRailAssetItem(intPtr);
			}
		}

		public virtual RailResult AsyncStartConsumeAsset(ulong asset_id, string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncStartConsumeAsset(swigCPtr_, asset_id, user_data);
		}

		public virtual RailResult AsyncUpdateConsumeProgress(ulong asset_id, string progress, string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncUpdateConsumeProgress(swigCPtr_, asset_id, progress, user_data);
		}

		public virtual RailResult AsyncCompleteConsumeAsset(ulong asset_id, uint quantity, string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncCompleteConsumeAsset(swigCPtr_, asset_id, quantity, user_data);
		}

		public virtual RailResult AsyncExchangeAssets(List<RailAssetItem> old_assets, RailProductItem to_product_info, string user_data)
		{
			IntPtr intPtr = ((old_assets == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailAssetItem__SWIG_0());
			if (old_assets != null)
			{
				RailConverter.Csharp2Cpp(old_assets, intPtr);
			}
			IntPtr intPtr2 = ((to_product_info == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailProductItem__SWIG_0());
			if (to_product_info != null)
			{
				RailConverter.Csharp2Cpp(to_product_info, intPtr2);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncExchangeAssets(swigCPtr_, intPtr, intPtr2, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRailAssetItem(intPtr);
				RAIL_API_PINVOKE.delete_RailProductItem(intPtr2);
			}
		}

		public virtual RailResult AsyncExchangeAssetsTo(List<RailAssetItem> old_assets, RailProductItem to_product_info, ulong add_to_exist_assets, string user_data)
		{
			IntPtr intPtr = ((old_assets == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailAssetItem__SWIG_0());
			if (old_assets != null)
			{
				RailConverter.Csharp2Cpp(old_assets, intPtr);
			}
			IntPtr intPtr2 = ((to_product_info == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailProductItem__SWIG_0());
			if (to_product_info != null)
			{
				RailConverter.Csharp2Cpp(to_product_info, intPtr2);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncExchangeAssetsTo(swigCPtr_, intPtr, intPtr2, add_to_exist_assets, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRailAssetItem(intPtr);
				RAIL_API_PINVOKE.delete_RailProductItem(intPtr2);
			}
		}

		public virtual RailResult AsyncSplitAsset(ulong source_asset, uint to_quantity, string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncSplitAsset(swigCPtr_, source_asset, to_quantity, user_data);
		}

		public virtual RailResult AsyncSplitAssetTo(ulong source_asset, uint to_quantity, ulong add_to_asset, string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncSplitAssetTo(swigCPtr_, source_asset, to_quantity, add_to_asset, user_data);
		}

		public virtual RailResult AsyncMergeAsset(List<RailAssetItem> source_assets, string user_data)
		{
			IntPtr intPtr = ((source_assets == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailAssetItem__SWIG_0());
			if (source_assets != null)
			{
				RailConverter.Csharp2Cpp(source_assets, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncMergeAsset(swigCPtr_, intPtr, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRailAssetItem(intPtr);
			}
		}

		public virtual RailResult AsyncMergeAssetTo(List<RailAssetItem> source_assets, ulong add_to_asset, string user_data)
		{
			IntPtr intPtr = ((source_assets == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailAssetItem__SWIG_0());
			if (source_assets != null)
			{
				RailConverter.Csharp2Cpp(source_assets, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailAssets_AsyncMergeAssetTo(swigCPtr_, intPtr, add_to_asset, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRailAssetItem(intPtr);
			}
		}

		public virtual ulong GetComponentVersion()
		{
			return RAIL_API_PINVOKE.IRailComponent_GetComponentVersion(swigCPtr_);
		}

		public virtual void Release()
		{
			RAIL_API_PINVOKE.IRailComponent_Release(swigCPtr_);
		}
	}
}
