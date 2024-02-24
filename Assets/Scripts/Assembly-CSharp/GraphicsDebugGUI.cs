using System.Collections.Generic;
using UWE;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class GraphicsDebugGUI : MonoBehaviour
{
	public delegate void LayoutGUIFunction();

	private interface GraphicsOption
	{
		void OnGUIUpdate();

		void OnGUI(char hotkey);
	}

	private class ComponentToggle<C> : GraphicsOption where C : MonoBehaviour
	{
		private C comp;

		private string label;

		private string prefsKey;

		private bool toggleObject;

		public ComponentToggle(string label, string prefsKey, bool toggleObject)
		{
			this.label = label;
			this.prefsKey = prefsKey;
			this.toggleObject = toggleObject;
			C[] array = Object.FindObjectsOfType<C>();
			int num = 0;
			if (num < array.Length)
			{
				C val = array[num];
				comp = val;
			}
		}

		public void OnGUIUpdate()
		{
		}

		private bool HotkeyDown(char hotkey)
		{
			if (Event.current.isKey)
			{
				return Event.current.character == hotkey;
			}
			return false;
		}

		public void OnGUI(char hotkey)
		{
			if (!(comp != null))
			{
				return;
			}
			if (toggleObject)
			{
				if (UWE.Utils.ToggleChanged(comp.gameObject.activeInHierarchy, hotkey + " :: " + label) || HotkeyDown(hotkey))
				{
					comp.gameObject.SetActive(!comp.gameObject.activeInHierarchy);
					PlayerPrefs.SetInt(prefsKey, comp.gameObject.activeInHierarchy ? 1 : 0);
				}
			}
			else if (UWE.Utils.ToggleChanged(comp.enabled, hotkey + " :: " + label) || HotkeyDown(hotkey))
			{
				comp.enabled = !comp.enabled;
				PlayerPrefs.SetInt(prefsKey, comp.enabled ? 1 : 0);
			}
		}
	}

	public static GraphicsDebugGUI main = null;

	public Int2[] resPresets = new Int2[3]
	{
		new Int2(1920, 1080),
		new Int2(1280, 720),
		new Int2(800, 450)
	};

	private Bloom bloom;

	private WaterscapeVolume fog;

	private WaterscapeVolumeOnCamera waterVolumeOnCamera;

	private WaterSurfaceOnCamera waterSurfaceOnCamera;

	private FrameTimeOverlay measure;

	public static string[] ToggledShaderKeywords = new string[2] { "ALBEDO_ONLY", "PDA_YELLOW" };

	private bool showToggledObjects;

	private List<GraphicsOption> options = new List<GraphicsOption>();

	private Vector2 scrollPos = Vector2.zero;

	private float marmoMinSkyAtNight;

	public event LayoutGUIFunction onLayoutGUI;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		options.Add(new ComponentToggle<WaterSunShaftsOnCamera>("Light shafts", "uwe.sn.lightShafts", toggleObject: false));
		options.Add(new ComponentToggle<Bloom>("Bloom", "uwe.sn.bloom", toggleObject: false));
		options.Add(new ComponentToggle<Antialiasing>("Anti-aliasing", "uwe.sn.aa", toggleObject: false));
		options.Add(new ComponentToggle<FrameTimeOverlay>("Frame Time Graph", "", toggleObject: false));
		options.Add(new ComponentToggle<AmbientParticles>("Ambiant Particles", "", toggleObject: false));
		options.Add(new ComponentToggle<VisualizeDepth>("Visualize Depth", "uwe.sn.depth", toggleObject: false));
		if (SNUtils.IsEngineDeveloper())
		{
			options.Add(new ComponentToggle<WeatherManager>("Weather Manager", "", toggleObject: false));
			options.Add(new ComponentToggle<Grayscale>("GrayScale", "uwe.sn.grayscale", toggleObject: false));
			options.Add(new ComponentToggle<WaterscapeVolumeOnCamera>("Underwater Fog", "uwe.sn.fog", toggleObject: false));
			options.Add(new ComponentToggle<LensWater>("Water Drip Effect", "uwe.sn.lensWater", toggleObject: false));
		}
		WaterscapeVolume[] array = Object.FindObjectsOfType<WaterscapeVolume>();
		int num = 0;
		if (num < array.Length)
		{
			WaterscapeVolume waterscapeVolume = array[num];
			fog = waterscapeVolume;
		}
		WaterscapeVolumeOnCamera[] array2 = Object.FindObjectsOfType<WaterscapeVolumeOnCamera>();
		num = 0;
		if (num < array2.Length)
		{
			WaterscapeVolumeOnCamera waterscapeVolumeOnCamera = array2[num];
			waterVolumeOnCamera = waterscapeVolumeOnCamera;
		}
		WaterSurfaceOnCamera[] array3 = Object.FindObjectsOfType<WaterSurfaceOnCamera>();
		num = 0;
		if (num < array3.Length)
		{
			WaterSurfaceOnCamera waterSurfaceOnCamera = array3[num];
			this.waterSurfaceOnCamera = waterSurfaceOnCamera;
		}
		FrameTimeOverlay[] array4 = Object.FindObjectsOfType<FrameTimeOverlay>();
		num = 0;
		if (num < array4.Length)
		{
			FrameTimeOverlay frameTimeOverlay = array4[num];
			measure = frameTimeOverlay;
		}
	}

	private void ForceAllLOD(int lev)
	{
		LODGroup[] array = Object.FindObjectsOfType<LODGroup>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ForceLOD(lev);
		}
	}

	private void OnGUI()
	{
		foreach (GraphicsOption option in options)
		{
			option.OnGUIUpdate();
		}
		GUILayout.BeginArea(new Rect(20f, 20f, (float)Screen.width * 0.3f, Screen.height));
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.BeginVertical("box");
		GUILayout.Label("Device:" + SystemInfo.graphicsDeviceName + "\nVersion:" + SystemInfo.graphicsDeviceVersion + "\n\nTarget frame rate = " + Application.targetFrameRate + "\nvsync count = " + QualitySettings.vSyncCount + "\nQuality level: " + QualitySettings.GetQualityLevel() + "\nRes: " + Screen.width + "x" + Screen.height + "\n" + ((measure != null && measure.enabled) ? ("ms/frame: " + measure.GetFrameMS().ToString("0.##") + " or " + 1000f / measure.GetFrameMS() + " FPS\n") : ""));
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("Texture Quality (current: " + (4 - QualitySettings.masterTextureLimit) + ")");
		for (int num = 3; num >= 0; num--)
		{
			if (GUILayout.Button(string.Concat(4 - num)))
			{
				QualitySettings.masterTextureLimit = num;
			}
		}
		GUILayout.EndHorizontal();
		SNUtils.IsEngineDeveloper();
		char c = '1';
		foreach (GraphicsOption option2 in options)
		{
			option2.OnGUI(c++);
		}
		if (SNUtils.IsEngineDeveloper())
		{
			GUILayout.BeginVertical("box");
			GUILayout.Label("Shader keyword toggles:");
			GUILayout.BeginHorizontal("textarea");
			GUILayout.Label("Force Particles Lighting per vertex");
			if (GUILayout.Button("TRUE (Cheaper)"))
			{
				Shader.EnableKeyword("FX_FORCELIGHT_VERTEX");
			}
			if (GUILayout.Button("FALSE (default)"))
			{
				Shader.DisableKeyword("FX_FORCELIGHT_VERTEX");
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.BeginVertical("box");
			GUILayout.Label("Shader keyword toggles:");
			string[] toggledShaderKeywords = ToggledShaderKeywords;
			foreach (string text in toggledShaderKeywords)
			{
				GUILayout.BeginHorizontal("textarea");
				GUILayout.Label(text);
				if (GUILayout.Button("Enable"))
				{
					Shader.EnableKeyword(text);
				}
				if (GUILayout.Button("Disable"))
				{
					Shader.DisableKeyword(text);
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.BeginVertical("box");
			GUILayout.Label("LOD:");
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Disable Model LOD"))
			{
				LODGroup[] array = Object.FindObjectsOfType<LODGroup>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].ForceLOD(0);
				}
			}
			if (GUILayout.Button("Enable Model LOD"))
			{
				LODGroup[] array = Object.FindObjectsOfType<LODGroup>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].ForceLOD(-1);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Force LOD: ");
			if (GUILayout.Button("OFF"))
			{
				ForceAllLOD(-1);
			}
			for (int j = 0; j < 5; j++)
			{
				if (GUILayout.Button(string.Concat(j)))
				{
					ForceAllLOD(j);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
		if (waterVolumeOnCamera != null || waterSurfaceOnCamera != null)
		{
			GUILayout.BeginVertical("box");
			if (waterVolumeOnCamera != null)
			{
				waterVolumeOnCamera.SetVisible(!GUILayout.Toggle(!waterVolumeOnCamera.GetVisible(), "Water Volume"));
			}
			if (waterSurfaceOnCamera != null)
			{
				waterSurfaceOnCamera.SetVisible(!GUILayout.Toggle(!waterSurfaceOnCamera.GetVisible(), "Water Surface"));
			}
			GUILayout.EndVertical();
		}
		GUILayout.Label("ShaderLOD: " + Shader.globalMaximumLOD);
		GUILayout.BeginHorizontal();
		LayoutShaderLODButton(100);
		LayoutShaderLODButton(200);
		LayoutShaderLODButton(300);
		LayoutShaderLODButton(400);
		GUILayout.EndHorizontal();
		GUILayout.Label("LODGroup bias: " + QualitySettings.lodBias);
		QualitySettings.lodBias = GUILayout.HorizontalSlider(QualitySettings.lodBias, 0f, 10f);
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("Shadow cascades: " + QualitySettings.shadowCascades);
		if (GUILayout.Button("1"))
		{
			QualitySettings.shadowCascades = 1;
		}
		if (GUILayout.Button("2"))
		{
			QualitySettings.shadowCascades = 2;
		}
		if (GUILayout.Button("4"))
		{
			QualitySettings.shadowCascades = 4;
		}
		GUILayout.EndHorizontal();
		if (this.onLayoutGUI != null)
		{
			this.onLayoutGUI();
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void OnDisable()
	{
		PlayerPrefs.Save();
	}

	private void LayoutShaderLODButton(int lod)
	{
		if (GUILayout.Button(string.Concat(lod)))
		{
			Shader.globalMaximumLOD = lod;
		}
	}
}
