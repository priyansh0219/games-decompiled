using System;
using UnityEngine;

namespace UWE
{
	public class BundleWrapper
	{
		public delegate void OnLoadedHandler(UnityEngine.Object asset);

		private AssetBundle bundle;

		private AssetBundleRequest req;

		private OnLoadedHandler onLoaded;

		public AssetBundle Get()
		{
			return bundle;
		}

		public bool IsReady()
		{
			return bundle != null;
		}

		public bool IsBusy()
		{
			return req != null;
		}

		public void Reset(string path)
		{
			if (!IsBusy())
			{
				if (bundle != null)
				{
					bundle.Unload(unloadAllLoadedObjects: false);
				}
				bundle = AssetBundle.LoadFromFile(path);
				if (bundle == null)
				{
					Debug.LogError("Could not load asset bundle file " + path);
				}
			}
		}

		public void LoadAsync(string name, Type type, OnLoadedHandler onLoaded)
		{
			if (IsBusy())
			{
				Debug.LogError("requested async load while another in progress. asset name = " + name);
				return;
			}
			if (bundle == null)
			{
				Debug.LogError("requested async load of " + name + " before bundle was loaded");
				return;
			}
			req = bundle.LoadAssetAsync(name, type);
			this.onLoaded = onLoaded;
		}

		public void Update()
		{
			if (req != null && req.isDone)
			{
				if (onLoaded != null)
				{
					onLoaded(req.asset);
				}
				req = null;
			}
		}
	}
}
