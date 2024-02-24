using System;
using ProtoBuf;
using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.UI;

[ProtoContract]
public class ThermalPlant : HandTarget, IHandTarget
{
	[AssertNotNull]
	public PowerSource powerSource;

	[AssertNotNull]
	public Constructable constructable;

	[AssertNotNull]
	public TextMeshProUGUI temperatureText;

	[AssertNotNull]
	public Image temperatureBar;

	[NonSerialized]
	[ProtoMember(1)]
	public int version;

	[NonSerialized]
	[ProtoMember(2)]
	public float temperature;

	private const float maxEnergyPerSec = 1.6500001f;

	private const float queryTempInterval = 10f;

	private const float addPowerInterval = 2f;

	private const float temperatureBarWidth = 300f;

	private const float temperatureBarHeight = 60f;

	[AssertLocalization(1)]
	private const string thermalPlantCelsiusFormatKey = "ThermalPlantCelsius";

	[AssertLocalization(2)]
	private const string thermalPlantStatusFormatKey = "ThermalPlantStatus";

	private void Start()
	{
		InvokeRepeating("QueryTemperature", UnityEngine.Random.value, 10f);
		InvokeRepeating("AddPower", UnityEngine.Random.value, 2f);
		temperatureBar.material = new Material(temperatureBar.material);
	}

	private void QueryTemperature()
	{
		WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
		if ((bool)main)
		{
			temperature = Mathf.Max(temperature, main.GetTemperature(base.transform.position));
			UpdateUI();
		}
	}

	public void AddPower()
	{
		if (constructable.constructed && temperature > 25f)
		{
			float num = 2f * DayNightCycle.main.dayNightSpeed;
			float amount = 1.6500001f * num * Mathf.Clamp01(Mathf.InverseLerp(25f, 100f, temperature));
			float amountStored = 0f;
			powerSource.AddEnergy(amount, out amountStored);
		}
	}

	public void UpdateUI()
	{
		temperatureText.text = Language.main.GetFormat("ThermalPlantCelsius", temperature);
		float value = temperature / 100f;
		temperatureBar.material.SetFloat(ShaderPropertyID._Amount, value);
		if (temperature < 25f)
		{
			temperatureBar.color = Color.red;
			temperatureText.color = Color.red;
			return;
		}
		float num = (temperature - 25f) / 75f;
		Color color = ((!(num < 0.3f)) ? UWE.Utils.LerpColor(Color.yellow, Color.green, (num - 0.4f) / 0.6f) : UWE.Utils.LerpColor(Color.red, Color.yellow, num / 0.4f));
		temperatureBar.color = color;
		temperatureText.color = color;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (constructable.constructed)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat("ThermalPlantStatus", Mathf.RoundToInt(powerSource.GetPower()), Mathf.RoundToInt(powerSource.GetMaxPower())), translate: false);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
	}
}
