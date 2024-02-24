using UnityEngine;

public class Accumulator : HandTarget, IHandTarget
{
	public const float kTransferRate = 0.5f;

	private PowerRelay takePowerRelay;

	private IPowerInterface storePowerInterface;

	[AssertLocalization]
	private const string accumulatorStatusFormatKey = "AccumulatorStatus";

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		takePowerRelay = GetComponent<PowerRelay>();
		PowerSource component = GetComponent<PowerSource>();
		storePowerInterface = component.GetComponent<IPowerInterface>();
	}

	public float UpdatePower(float timePassed)
	{
		float a = storePowerInterface.GetMaxPower() - storePowerInterface.GetPower();
		float powerFromInbound = takePowerRelay.GetPowerFromInbound();
		float amountStored = 0f;
		float num = Mathf.Min(timePassed * 0.5f, Mathf.Min(a, powerFromInbound));
		if (num > 0f)
		{
			float amountConsumed = 0f;
			takePowerRelay.ConsumeEnergy(num, out amountConsumed);
			storePowerInterface.AddEnergy(amountConsumed, out amountStored);
		}
		return amountStored;
	}

	private void Update()
	{
		if (base.gameObject.activeInHierarchy)
		{
			UpdatePower(Time.deltaTime);
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (base.gameObject.GetComponent<Constructable>().constructed)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat("AccumulatorStatus", Mathf.RoundToInt(storePowerInterface.GetPower()), Mathf.RoundToInt(storePowerInterface.GetMaxPower())), translate: false);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
	}
}
