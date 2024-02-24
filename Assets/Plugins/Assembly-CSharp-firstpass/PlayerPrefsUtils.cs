using UnityEngine;

public static class PlayerPrefsUtils
{
	public class PlayerPrefsString
	{
		public string value;

		public string key { get; private set; }

		public PlayerPrefsString(string key, string initValue)
		{
			value = initValue;
			this.key = key;
		}

		public void Load()
		{
			value = PlayerPrefs.GetString(key, value);
		}

		public void Save()
		{
			PlayerPrefs.SetString(key, value);
			PlayerPrefs.Save();
		}
	}

	public class PlayerPrefsFloat
	{
		private string key;

		private string guiLabel;

		private float defaultVal;

		private string guiInput;

		public float val => PlayerPrefs.GetFloat(key, defaultVal);

		public PlayerPrefsFloat(string key, string guiLabel, float defaultVal)
		{
			this.key = key;
			this.guiLabel = guiLabel;
			this.defaultVal = defaultVal;
			guiInput = string.Concat(val);
		}

		public void SetAndSave(float val)
		{
			PlayerPrefs.SetFloat(key, val);
			PlayerPrefs.Save();
		}

		public void LayoutGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(guiLabel + " (currently " + val + "): ");
			guiInput = GUILayout.TextField(guiInput);
			if (GUILayout.Button("Apply") && float.TryParse(guiInput, out var result))
			{
				SetAndSave(result);
			}
			GUILayout.EndHorizontal();
		}
	}

	public static bool PrefsToggle(bool defaultVal, string key, string label)
	{
		bool num = PlayerPrefs.GetInt(key, defaultVal ? 1 : 0) > 0;
		bool flag = GUILayout.Toggle(num, " " + label);
		PlayerPrefs.SetInt(key, flag ? 1 : 0);
		if (num != flag)
		{
			PlayerPrefs.Save();
		}
		return flag;
	}
}
