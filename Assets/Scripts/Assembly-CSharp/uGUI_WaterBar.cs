using TMPro;
using UnityEngine;

public class uGUI_WaterBar : MonoBehaviour
{
	private const float punchDamp = 100f;

	private const float puchFrequency = 5f;

	[AssertNotNull]
	public uGUI_CircularBar bar;

	[AssertNotNull]
	public RectTransform icon;

	[AssertNotNull]
	public TextMeshProUGUI text;

	public float dampSpeed = 0.1f;

	[Space]
	public float pulseReferenceCapacity = 100f;

	public AnimationCurve pulseDelayCurve = new AnimationCurve();

	public AnimationCurve pulseTimeCurve = new AnimationCurve();

	[AssertNotNull]
	public Animation pulseAnimation;

	public float rotationSpringCoef = 100f;

	public float rotationVelocityDamp = 0.9f;

	public float rotationVelocityMax = -1f;

	public float rotationRandomVelocity = 1000f;

	private float curr;

	private float vel;

	private float punchSeed;

	private float punchMaxScale = 2f;

	private Vector3 punchInitialScale;

	private Vector3 punchScale = new Vector3(0f, 0f, 0f);

	private CoroutineTween punchTween;

	private bool subscribed;

	private CoroutineTween pulseTween;

	private float pulseDelay = -1f;

	private float pulseTime = -1f;

	private AnimationState pulseAnimationState;

	private int cachedValue = int.MinValue;

	private float rotationCurrent;

	private float rotationVelocity;

	private bool showNumbers;

	private float lastFixedUpdateTime;

	private void Awake()
	{
		punchTween = new CoroutineTween(this)
		{
			deltaTimeProvider = PDA.GetDeltaTime,
			mode = CoroutineTween.Mode.Once,
			onStart = OnPunchStart,
			onUpdate = OnPunchUpdate,
			onStop = OnPunchStop
		};
		pulseTween = new CoroutineTween(this)
		{
			mode = CoroutineTween.Mode.Loop,
			duration = 0f,
			onUpdate = OnPulse
		};
		pulseAnimation.wrapMode = WrapMode.Loop;
		pulseAnimation.Stop();
		pulseAnimationState = pulseAnimation.GetState(0);
		if (pulseAnimationState != null)
		{
			pulseAnimationState.blendMode = AnimationBlendMode.Blend;
			pulseAnimationState.weight = 1f;
			pulseAnimationState.layer = 0;
			pulseAnimationState.speed = 0f;
		}
		text.enableCulling = true;
	}

	private void OnEnable()
	{
		lastFixedUpdateTime = PDA.time;
		if (pulseAnimationState != null)
		{
			pulseAnimationState.enabled = true;
		}
		pulseTween.Start();
	}

	private void LateUpdate()
	{
		bool num = showNumbers;
		showNumbers = false;
		Player main = Player.main;
		if (main != null)
		{
			Survival component = main.GetComponent<Survival>();
			if (component != null)
			{
				if (!subscribed)
				{
					subscribed = true;
					component.onDrink.AddHandler(base.gameObject, OnDrink);
				}
				float water = component.water;
				float capacity = 100f;
				SetValue(water, capacity);
				float num2 = Mathf.Clamp01(water / pulseReferenceCapacity);
				float time = 1f - num2;
				pulseDelay = pulseDelayCurve.Evaluate(time);
				if (pulseDelay < 0f)
				{
					pulseDelay = 0f;
				}
				pulseTime = pulseTimeCurve.Evaluate(time);
				if (pulseTime < 0f)
				{
					pulseTime = 0f;
				}
				float num3 = pulseDelay + pulseTime;
				if (pulseTween.duration > 0f && num3 <= 0f)
				{
					pulseAnimationState.normalizedTime = 0f;
				}
				pulseTween.duration = num3;
			}
			PDA pDA = main.GetPDA();
			if (pDA != null && pDA.isInUse)
			{
				showNumbers = true;
			}
		}
		if (pulseAnimationState != null && pulseAnimationState.enabled)
		{
			icon.localScale += punchScale;
		}
		else
		{
			icon.localScale = punchScale;
		}
		if (num != showNumbers)
		{
			rotationVelocity += Random.Range(0f - rotationRandomVelocity, rotationRandomVelocity);
		}
		if (MathExtensions.CoinRotation(ref rotationCurrent, showNumbers ? 180f : 0f, ref lastFixedUpdateTime, PDA.time, ref rotationVelocity, rotationSpringCoef, rotationVelocityDamp, rotationVelocityMax))
		{
			icon.localRotation = Quaternion.Euler(0f, rotationCurrent, 0f);
		}
	}

	private void OnDisable()
	{
		punchTween.Stop();
		pulseTween.Stop();
		if (!subscribed)
		{
			return;
		}
		subscribed = false;
		Player main = Player.main;
		if (main != null)
		{
			Survival component = main.GetComponent<Survival>();
			if (component != null)
			{
				component.onDrink.RemoveHandler(base.gameObject, OnDrink);
			}
		}
	}

	private void SetValue(float has, float capacity)
	{
		float target = has / capacity;
		curr = Mathf.SmoothDamp(curr, target, ref vel, dampSpeed, float.PositiveInfinity, PDA.deltaTime);
		bar.value = curr;
		int num = Mathf.CeilToInt(curr * capacity);
		if (cachedValue != num)
		{
			cachedValue = num;
			text.text = IntStringCache.GetStringForInt(cachedValue);
		}
	}

	private void OnDrink(float drinkAmount)
	{
		float maxScale = 1f + Mathf.Clamp01(drinkAmount / 100f);
		Punch(2.5f, maxScale);
	}

	private void Punch(float duration, float maxScale)
	{
		punchTween.duration = duration;
		punchMaxScale = maxScale;
		punchTween.Start();
	}

	private void OnPunchStart()
	{
		punchInitialScale = icon.localScale;
		punchSeed = Random.value;
	}

	private void OnPunchUpdate(float t)
	{
		float o = 0f;
		MathExtensions.Oscillation(100f, 5f, punchSeed, t, out var o2, out o);
		punchScale = new Vector3(o2 * punchMaxScale, o * punchMaxScale, 0f);
	}

	private void OnPunchStop()
	{
		punchScale = new Vector3(0f, 0f, 0f);
		if (!(icon == null))
		{
			icon.localScale = punchInitialScale;
		}
	}

	private void OnPulse(float scalar)
	{
		if (!(pulseAnimationState == null))
		{
			pulseAnimationState.normalizedTime = Mathf.Clamp01((pulseTween.duration * scalar - pulseDelay) / pulseTime);
		}
	}
}
