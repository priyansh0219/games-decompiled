using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

public static class SpriteManager
{
	public enum Group
	{
		None = 0,
		Item = 1,
		Background = 2,
		Category = 3,
		Log = 4,
		Tab = 5,
		Pings = 6,
		ItemActions = 7
	}

	public class SpriteCache
	{
		public readonly Vector2[] vertices;

		public readonly Vector2[] uv;

		public readonly ushort[] triangles;

		public SpriteCache(Sprite sprite)
		{
			vertices = sprite.vertices;
			uv = sprite.uv;
			triangles = sprite.triangles;
		}
	}

	private static readonly Dictionary<Group, string> mapping;

	private static Sprite _defaultSprite;

	private static Dictionary<string, Dictionary<string, Sprite>> atlases;

	public static bool hasInitialized;

	private static Dictionary<string, Sprite> resources;

	private static readonly Dictionary<Sprite, SpriteCache> spriteCache;

	private static readonly HashSet<Sprite> slice9GridSprites;

	public static Sprite defaultSprite
	{
		get
		{
			if (_defaultSprite == null)
			{
				_defaultSprite = Get(Group.None, "Unknown");
			}
			return _defaultSprite;
		}
	}

	static SpriteManager()
	{
		mapping = new Dictionary<Group, string>
		{
			{
				Group.Item,
				"Items"
			},
			{
				Group.Category,
				"Categories"
			},
			{
				Group.Tab,
				"Tabs"
			},
			{
				Group.None,
				"Default"
			},
			{
				Group.Log,
				"Log"
			},
			{
				Group.Pings,
				"Pings"
			},
			{
				Group.ItemActions,
				"ItemActions"
			},
			{
				Group.Background,
				"Backgrounds"
			}
		};
		resources = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
		spriteCache = new Dictionary<Sprite, SpriteCache>();
		slice9GridSprites = new HashSet<Sprite>();
		AsyncOperationHandle<IList<SpriteAtlas>> asyncOperationHandle = AddressablesUtility.LoadAllAsync<SpriteAtlas>("SpriteAtlases");
		asyncOperationHandle.Completed += OnLoadedSpriteAtlases;
	}

	private static void OnLoadedSpriteAtlases(AsyncOperationHandle<IList<SpriteAtlas>> handle)
	{
		hasInitialized = true;
		if (handle.Status == AsyncOperationStatus.Failed)
		{
			Debug.LogError("SpriteManager failed to find atlases");
			atlases = new Dictionary<string, Dictionary<string, Sprite>>(0);
			return;
		}
		IList<SpriteAtlas> result = handle.Result;
		int count = result.Count;
		atlases = new Dictionary<string, Dictionary<string, Sprite>>(count);
		for (int i = 0; i < count; i++)
		{
			SpriteAtlas spriteAtlas = result[i];
			int spriteCount = spriteAtlas.spriteCount;
			Dictionary<string, Sprite> dictionary = new Dictionary<string, Sprite>(spriteCount, StringComparer.InvariantCultureIgnoreCase);
			Sprite[] array = new Sprite[spriteCount];
			spriteAtlas.GetSprites(array);
			foreach (Sprite sprite in array)
			{
				string name = sprite.name;
				name = name.Substring(0, name.Length - "(Clone)".Length);
				dictionary.Add(name, sprite);
			}
			atlases.Add(spriteAtlas.name, dictionary);
		}
	}

	private static Sprite Get(string atlasName, string name, Sprite defaultSprite)
	{
		Sprite value = null;
		if (atlases.TryGetValue(atlasName, out var value2))
		{
			value2.TryGetValue(name, out value);
		}
		if (value != null)
		{
			return value;
		}
		return defaultSprite;
	}

	public static Sprite Get(Group group, string name, Sprite defaultSprite = null)
	{
		if (!hasInitialized)
		{
			Debug.LogError("SpriteManager: Race condition. Asking for sprite before the atlases have finished loading.");
			return defaultSprite;
		}
		if (mapping.TryGetValue(group, out var value))
		{
			return Get(value, name, defaultSprite);
		}
		return defaultSprite;
	}

	public static Sprite GetUnitySprite(Group group, string name, Sprite defaultSprite = null)
	{
		Sprite sprite = Get(group, name);
		if (sprite != null)
		{
			return sprite;
		}
		return defaultSprite;
	}

	public static Sprite Get(TechType techType)
	{
		return Get(Group.Item, techType.AsString(), defaultSprite);
	}

	public static Sprite Get(TechType techType, Sprite defaultSprite)
	{
		return Get(Group.Item, techType.AsString(), defaultSprite);
	}

	private static Sprite GetResource(string path)
	{
		if (resources.TryGetValue(path, out var value))
		{
			return value;
		}
		value = Resources.Load<Sprite>(path);
		if (value == null)
		{
			Debug.LogErrorFormat("Sprite not found in Resources at path {0}", path);
		}
		else
		{
			SetProceduralSlice9Grid(value);
		}
		resources.Add(path, value);
		return value;
	}

	public static Sprite GetBackground(CraftData.BackgroundType backgroundType)
	{
		switch (backgroundType)
		{
		case CraftData.BackgroundType.Blueprint:
			return GetResource("Sprites/Backgrounds/Blueprint");
		case CraftData.BackgroundType.PlantWater:
			return GetResource("Sprites/Backgrounds/PlantWater");
		case CraftData.BackgroundType.PlantWaterSeed:
			return GetResource("Sprites/Backgrounds/PlantWater");
		case CraftData.BackgroundType.PlantAir:
			return GetResource("Sprites/Backgrounds/PlantAir");
		case CraftData.BackgroundType.PlantAirSeed:
			return GetResource("Sprites/Backgrounds/PlantAir");
		case CraftData.BackgroundType.ExosuitArm:
			return GetResource("Sprites/Backgrounds/ExosuitArm");
		default:
			return GetResource("Sprites/Backgrounds/Normal");
		}
	}

	public static Sprite GetBackground(TechType techType)
	{
		return GetBackground(TechData.GetBackgroundType(techType));
	}

	public static SpriteCache GetSpriteCache(Sprite sprite)
	{
		if (sprite == null)
		{
			return null;
		}
		if (!spriteCache.TryGetValue(sprite, out var value))
		{
			value = new SpriteCache(sprite);
			spriteCache.Add(sprite, value);
		}
		return value;
	}

	public static bool GetProceduralSlice9Grid(Sprite sprite)
	{
		return slice9GridSprites.Contains(sprite);
	}

	public static void SetProceduralSlice9Grid(Sprite sprite)
	{
		slice9GridSprites.Add(sprite);
	}
}
