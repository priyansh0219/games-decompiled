using TMPro;
using UnityEngine;

public class uGUI_HealthBar : MonoBehaviour
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
	public Animation animation;

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

	private AnimationState statePulse;

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
		animation.wrapMode = WrapMode.Loop;
		animation.Stop();
		statePulse = animation.GetState(0);
		statePulse.blendMode = AnimationBlendMode.Blend;
		statePulse.weight = 1f;
		statePulse.layer = 0;
		statePulse.speed = 0f;
		text.enableCulling = true;
	}

	private void OnEnable()
	{
		lastFixedUpdateTime = PDA.time;
		statePulse.enabled = true;
		pulseTween.Start();
	}

	private void LateUpdate()
	{
		bool num = showNumbers;
		showNumbers = false;
		Player main = Player.main;
		if (main != null)
		{
			LiveMixin component = main.GetComponent<LiveMixin>();
			if (component != null)
			{
				if (!subscribed)
				{
					subscribed = true;
					component.onHealDamage.AddHandler(base.gameObject, OnHealDamage);
				}
				float num2 = component.health - component.tempDamage;
				float maxHealth = component.maxHealth;
				SetValue(num2, maxHealth);
				float num3 = Mathf.Clamp01(num2 / pulseReferenceCapacity);
				float time = 1f - num3;
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
				float num4 = pulseDelay + pulseTime;
				if (pulseTween.duration > 0f && num4 <= 0f)
				{
					statePulse.normalizedTime = 0f;
				}
				pulseTween.duration = num4;
			}
			PDA pDA = main.GetPDA();
			if (pDA != null && pDA.isInUse)
			{
				showNumbers = true;
			}
		}
		if (statePulse.enabled)
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
			LiveMixin component = main.GetComponent<LiveMixin>();
			if (component != null)
			{
				component.onHealDamage.RemoveHandler(base.gameObject, OnHealDamage);
			}
		}
	}

	private void SetValue(float has, float capacity)
	{
		float target = Mathf.Clamp01(has / capacity);
		curr = Mathf.SmoothDamp(curr, target, ref vel, dampSpeed, float.PositiveInfinity, PDA.deltaTime);
		bar.value = curr;
		text.text = IntStringCache.GetStringForInt(Mathf.CeilToInt(curr * capacity));
	}

	private void OnHealDamage(float damage)
	{
		float maxScale = 1f + Mathf.Clamp01(damage / 100f);
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
		statePulse.normalizedTime = Mathf.Clamp01((pulseTween.duration * scalar - pulseDelay) / pulseTime);
	}
}
