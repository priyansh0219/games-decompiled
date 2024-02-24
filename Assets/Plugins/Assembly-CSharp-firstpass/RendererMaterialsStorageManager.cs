using System.Collections.Generic;
using UnityEngine;

public static class RendererMaterialsStorageManager
{
	private class Container
	{
		public RendererMaterialsStorage rendererMaterialsStorage;

		public HashSet<object> owners = new HashSet<object>();
	}

	private static Dictionary<Renderer, Container> renderers = new Dictionary<Renderer, Container>();

	public static RendererMaterialsStorage GetRendererMaterialsStorage(Renderer renderer, object owner)
	{
		if (!renderers.TryGetValue(renderer, out var value))
		{
			value = new Container
			{
				rendererMaterialsStorage = new RendererMaterialsStorage(renderer)
			};
			renderers[renderer] = value;
		}
		value.owners.Add(owner);
		return value.rendererMaterialsStorage;
	}

	public static void TryDestroyCopiedAndRestoreInitialMaterials(RendererMaterialsStorage rendererMaterialsStorage, object owner)
	{
		foreach (KeyValuePair<Renderer, Container> renderer in renderers)
		{
			Renderer key = renderer.Key;
			Container value = renderer.Value;
			if (value.rendererMaterialsStorage == rendererMaterialsStorage)
			{
				value.owners.Remove(owner);
				if (value.owners.Count == 0)
				{
					value.rendererMaterialsStorage.DestroyCopiedAndRestoreInitialMaterials();
					renderers.Remove(key);
				}
				break;
			}
		}
	}
}
