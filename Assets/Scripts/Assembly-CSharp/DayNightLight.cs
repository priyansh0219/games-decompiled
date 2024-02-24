using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[SkipProtoContractCheck]
[RequireComponent(typeof(Light))]
public class DayNightLight : FadeLightBase, IManagedUpdateBehaviour, IManagedBehaviour
{
	[ProtoMember(1)]
	public AnimationCurve colorR;

	[ProtoMember(2)]
	public AnimationCurve colorG;

	[ProtoMember(3)]
	public AnimationCurve colorB;

	[ProtoMember(4)]
	public AnimationCurve intensity;

	[ProtoMember(5)]
	public AnimationCurve sunFraction;

	public Color replaceColor = Color.white;

	public float replaceFraction;

	public float fade = 1f;

	private Light light;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "DayNightLights";
	}

	private void Awake()
	{
		light = GetComponent<Light>();
	}

	private void OnEnable()
	{
		BehaviourUpdateUtils.Register(this);
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	public void ManagedUpdate()
	{
		if (DayNightCycle.main != null)
		{
			float dayScalar = DayNightCycle.main.GetDayScalar();
			Evaluate(dayScalar);
		}
	}

	private void Evaluate(float dayScalar)
	{
		float r = colorR.Evaluate(dayScalar);
		float g = colorG.Evaluate(dayScalar);
		float b = colorB.Evaluate(dayScalar);
		float num = sunFraction.Evaluate(dayScalar);
		float num2 = intensity.Evaluate(dayScalar);
		Color a = new Color(r, g, b);
		light.color = Color.Lerp(a, replaceColor, num * replaceFraction);
		light.intensity = UWE.Utils.IntensityToGamma(num2 * fade);
	}

	public override void Fade(float fadeValue)
	{
		fade = fadeValue;
		light.enabled = (double)fadeValue > 0.0001;
	}

	public void Replace(Color color, float fraction)
	{
		replaceColor = color;
		replaceFraction = fraction;
	}

	public override Color EditorPreview(float fadeValue, float dayScalar, Color color, float fraction)
	{
		light = GetComponent<Light>();
		Fade(fadeValue);
		Replace(color, fraction);
		Evaluate(dayScalar);
		return light.color;
	}

	public override void ResetPreview()
	{
		Fade(1f);
		Replace(Color.white, 0f);
		Evaluate(0.5f);
	}
}
