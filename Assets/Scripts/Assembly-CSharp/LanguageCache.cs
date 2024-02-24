using System.Collections.Generic;

public static class LanguageCache
{
	public class ButtonText
	{
		public GameInput.Button action;

		public string binding;

		public string cachedText;
	}

	private static readonly Dictionary<string, ButtonText> buttonTextCache = new Dictionary<string, ButtonText>();

	private static readonly Dictionary<int, string> oxygenCache = new Dictionary<int, string>();

	private static readonly Dictionary<TechType, string> pickupCache = new Dictionary<TechType, string>();

	private static readonly Dictionary<TechType, string> packupCache = new Dictionary<TechType, string>();

	private static bool initialized;

	public static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			GameInput.OnBindingsChanged += OnBindingsChanged;
		}
	}

	public static void Deinitialize()
	{
		if (initialized)
		{
			initialized = false;
			GameInput.OnBindingsChanged -= OnBindingsChanged;
		}
	}

	public static void OnLanguageChanged()
	{
		buttonTextCache.Clear();
		oxygenCache.Clear();
		pickupCache.Clear();
		packupCache.Clear();
	}

	public static void OnBindingsChanged()
	{
		buttonTextCache.Clear();
	}

	public static string GetButtonFormat(string key, GameInput.Button action)
	{
		Initialize();
		string binding = GameInput.GetBinding(GameInput.PrimaryDevice, action, GameInput.BindingSet.Primary);
		ButtonText orAddNew = buttonTextCache.GetOrAddNew(key);
		if (orAddNew.action != action || orAddNew.binding != binding || string.IsNullOrEmpty(orAddNew.cachedText))
		{
			orAddNew.action = action;
			orAddNew.binding = binding;
			orAddNew.cachedText = Language.main.GetFormat(key, GameInput.FormatButton(action));
		}
		return orAddNew.cachedText;
	}

	public static string GetOxygenText(int secondsLeft)
	{
		if (!oxygenCache.TryGetValue(secondsLeft, out var value))
		{
			value = ((secondsLeft > 0) ? Language.main.GetFormat("OxygenFormat", secondsLeft) : Language.main.Get("Empty"));
			oxygenCache.Add(secondsLeft, value);
		}
		return value;
	}

	public static string GetPickupText(TechType techType)
	{
		if (!pickupCache.TryGetValue(techType, out var value))
		{
			value = Language.main.GetFormat("PickUpFormat", Language.main.Get(techType));
			pickupCache.Add(techType, value);
		}
		return value;
	}

	public static string GetPackUpText(TechType techType)
	{
		if (!packupCache.TryGetValue(techType, out var value))
		{
			value = Language.main.GetFormat("PackUpFormat", Language.main.Get(techType));
			packupCache.Add(techType, value);
		}
		return value;
	}
}
