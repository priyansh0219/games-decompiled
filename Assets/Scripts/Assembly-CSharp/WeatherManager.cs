using UWE;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
	public FMOD_StudioEventEmitter soundScape;

	public VFXWeatherManager vfxWeatherManager;

	public string windParam = "Wind";

	public string rainParam = "Rain";

	public float timeScalar;

	public float rainScalar;

	public float windScalar;

	public float minRainScalar;

	public float minWindScalar;

	public float minRainPlusWindLightingScalar;

	public float _outputRainScalar;

	public float _outputWindScalar;

	public float _outputLightningScalar;

	private void Start()
	{
		PrefabSpawnBase component = base.gameObject.GetComponent<PrefabSpawnBase>();
		vfxWeatherManager = component.spawnedObj.GetComponent<VFXWeatherManager>();
		soundScape.PlayUI();
	}

	private void Update()
	{
		float num = UWE.Utils.StableNoise(Time.timeSinceLevelLoad * timeScalar * rainScalar / 1000000f);
		float num2 = UWE.Utils.StableNoise(Time.timeSinceLevelLoad * timeScalar * windScalar / 1000000f);
		Transform transform = Player.main.gameObject.transform;
		float num3 = Mathf.PerlinNoise(transform.position.x * num, transform.position.z * num).Clamp(0f, 1f);
		float num4 = Mathf.PerlinNoise(transform.position.x * num2, transform.position.z * num2).Clamp(0f, 1f);
		_outputRainScalar = Mathf.Clamp01(num3 - minRainScalar) / (1f - minRainScalar);
		_outputWindScalar = Mathf.Clamp01(num4 - minWindScalar) / (1f - minWindScalar);
		float num5 = Mathf.Clamp01(_outputRainScalar + _outputWindScalar);
		_outputLightningScalar = Mathf.Clamp01(num5 - minRainPlusWindLightingScalar) / (1f - minRainPlusWindLightingScalar);
		soundScape.SetParameterValue(rainParam, _outputRainScalar);
		soundScape.SetParameterValue(windParam, _outputWindScalar);
		vfxWeatherManager.rainScalar = _outputRainScalar;
		vfxWeatherManager.windScalar = _outputWindScalar;
		vfxWeatherManager.lightningsScalar = _outputLightningScalar;
	}

	private void OnDisable()
	{
		Debug.Log("WeatherManager Disabled");
		vfxWeatherManager.rainScalar = (vfxWeatherManager.windScalar = (vfxWeatherManager.lightningsScalar = 0f));
	}
}
