using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Discord
{
	public class ImageManager
	{
		internal struct FFIEvents
		{
		}

		internal struct FFIMethods
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void FetchCallback(IntPtr ptr, Result result, ImageHandle handleResult);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void FetchMethod(IntPtr methodsPtr, ImageHandle handle, bool refresh, IntPtr callbackData, FetchCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetDimensionsMethod(IntPtr methodsPtr, ImageHandle handle, ref ImageDimensions dimensions);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetDataMethod(IntPtr methodsPtr, ImageHandle handle, byte[] data, int dataLen);

			internal FetchMethod Fetch;

			internal GetDimensionsMethod GetDimensions;

			internal GetDataMethod GetData;
		}

		public delegate void FetchHandler(Result result, ImageHandle handleResult);

		private IntPtr MethodsPtr;

		private FFIMethods Methods => (FFIMethods)Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));

		internal ImageManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
			Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
		}

		public void Fetch(ImageHandle handle, bool refresh, FetchHandler callback)
		{
			FFIMethods.FetchCallback fetchCallback = delegate(IntPtr ptr, Result result, ImageHandle handleResult)
			{
				Utility.Release(ptr);
				callback(result, handleResult);
			};
			Methods.Fetch(MethodsPtr, handle, refresh, Utility.Retain(fetchCallback), fetchCallback);
		}

		public ImageDimensions GetDimensions(ImageHandle handle)
		{
			ImageDimensions dimensions = default(ImageDimensions);
			Result result = Methods.GetDimensions(MethodsPtr, handle, ref dimensions);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return dimensions;
		}

		public void GetData(ImageHandle handle, byte[] data)
		{
			Result result = Methods.GetData(MethodsPtr, handle, data, data.Length);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void Fetch(ImageHandle handle, FetchHandler callback)
		{
			Fetch(handle, refresh: false, callback);
		}

		public byte[] GetData(ImageHandle handle)
		{
			ImageDimensions dimensions = GetDimensions(handle);
			byte[] array = new byte[dimensions.Width * dimensions.Height * 4];
			GetData(handle, array);
			return array;
		}

		public Texture2D GetTexture(ImageHandle handle)
		{
			ImageDimensions dimensions = GetDimensions(handle);
			Texture2D texture2D = new Texture2D((int)dimensions.Width, (int)dimensions.Height, TextureFormat.RGBA32, mipChain: false, linear: true);
			texture2D.LoadRawTextureData(GetData(handle));
			texture2D.Apply();
			return texture2D;
		}
	}
}
