using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_PopupMessage : MonoBehaviour, ICompileTimeCheckable
{
	protected enum Phase
	{
		None = 0,
		Zero = 1,
		In = 2,
		One = 3,
		Out = 4,
		Done = 5
	}

	private const ManagedUpdate.Queue queueUpdate = ManagedUpdate.Queue.UpdateAfterInput;

	private const ManagedUpdate.Queue queueLayoutComplete = ManagedUpdate.Queue.UILayoutComplete;

	[AssertNotNull]
	public GameObject root;

	[AssertNotNull]
	public Graphic background;

	[AssertNotNull]
	public TextMeshProUGUI text;

	public FMODAsset soundShow;

	public TextAnchor anchor = TextAnchor.MiddleLeft;

	public float ox = 20f;

	public float oy = 20f;

	public bool useUnscaledDeltaTime = true;

	protected RectTransform rootRT;

	protected PopupMessageCallback doneCallback;

	protected float timeDelay;

	protected float timeIn;

	protected float timeDuration;

	protected float timeOut;

	protected float start;

	protected Phase phase;

	protected float value;

	protected bool forceUpdate = true;

	public bool isShowingMessage
	{
		get
		{
			if (phase >= Phase.In)
			{
				return phase <= Phase.Out;
			}
			return false;
		}
	}

	public string showingMessage => text.text;

	private float timeNow
	{
		get
		{
			if (!useUnscaledDeltaTime)
			{
				return Time.time;
			}
			return Time.unscaledTime;
		}
	}

	private void Awake()
	{
		root.SetActive(value: false);
		rootRT = root.GetComponent<RectTransform>();
		RectTransform component = rootRT.parent.GetComponent<RectTransform>();
		component.pivot = Vector2.zero;
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.anchoredPosition = Vector2.zero;
		SetPosition(0f);
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
		uGUI_CanvasScaler.AddUIScaleListener(OnUIScaleChange);
	}

	private void OnDisable()
	{
		uGUI_CanvasScaler.RemoveUIScaleListener(OnUIScaleChange);
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
	}

	private void OnUpdate()
	{
		float num = timeNow - start;
		Phase phase = ((this.phase != 0) ? ((num < timeDelay) ? Phase.Zero : ((num < timeDelay + timeIn) ? Phase.In : ((timeDuration < 0f || num < timeDelay + timeIn + timeDuration) ? Phase.One : ((!(num < timeDelay + timeIn + timeDuration + timeOut)) ? Phase.Done : Phase.Out)))) : Phase.None);
		if (this.phase != phase)
		{
			this.phase = phase;
			root.SetActive(isShowingMessage);
			switch (phase)
			{
			case Phase.In:
				if (soundShow != null)
				{
					RuntimeManager.PlayOneShot(soundShow.path);
				}
				break;
			case Phase.Done:
				SetText(string.Empty);
				if (doneCallback != null)
				{
					doneCallback();
				}
				break;
			}
		}
		float position = ((timeDuration < 0f) ? MathExtensions.Step(timeDelay, timeIn, num, wrap: false) : MathExtensions.Trapezoid(timeDelay, timeIn, timeDuration, timeOut, num, wrap: false));
		SetPosition(position);
	}

	private void OnLayoutComplete()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
		float inverseScale = uGUI_CanvasScaler.GetInverseScale(base.transform);
		Rect rect = rootRT.parent.GetComponent<RectTransform>().rect;
		Rect rect2 = rootRT.rect;
		RectTransformExtensions.GetRectPositions(rect, rect2, anchor, ox * inverseScale, oy * inverseScale, out var p, out var p2);
		float t = MathExtensions.EaseOutSine(value);
		rootRT.anchoredPosition = Vector2.Lerp(p, p2, t);
	}

	private void OnUIScaleChange(float scale)
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
	}

	protected void SetPosition(float v)
	{
		v = Mathf.Clamp01(v);
		if (!Mathf.Approximately(value, v) || forceUpdate)
		{
			forceUpdate = false;
			value = v;
			ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
		}
	}

	public void SetBackgroundColor(Color color)
	{
		background.color = color;
	}

	public void SetText(string message, TextAnchor anchor = TextAnchor.UpperLeft)
	{
		if (text.text != message)
		{
			text.text = message;
			forceUpdate = true;
		}
		TextAlignmentOptions textAlignmentOptions = anchor.ToAlignment();
		if (text.alignment != textAlignmentOptions)
		{
			text.alignment = textAlignmentOptions;
			forceUpdate = true;
		}
	}

	public void Show(float timeDuration = 5f, float timeDelay = 0f, float timeIn = 0.25f, float timeOut = 0.25f, PopupMessageCallback doneCallback = null)
	{
		this.timeDelay = (timeDelay = Mathf.Max(0f, timeDelay));
		this.timeIn = Mathf.Max(0f, timeIn);
		this.timeDuration = timeDuration;
		this.timeOut = Mathf.Max(0f, timeOut);
		this.doneCallback = doneCallback;
		float num = timeNow;
		switch (phase)
		{
		case Phase.None:
			phase = Phase.Zero;
			start = num;
			break;
		case Phase.Zero:
			start = num;
			break;
		case Phase.In:
		case Phase.One:
		case Phase.Out:
			start = num - (timeDelay + timeIn * value);
			break;
		case Phase.Done:
			start = num;
			break;
		}
	}

	public void Hide()
	{
		if (timeDuration < 0f)
		{
			timeDuration = 0f;
		}
		switch (phase)
		{
		case Phase.Zero:
		case Phase.In:
		case Phase.One:
			start = timeNow - (timeDelay + timeIn + Mathf.Max(0f, timeDuration) + timeOut * (1f - value));
			break;
		case Phase.None:
		case Phase.Out:
		case Phase.Done:
			break;
		}
	}

	public string CompileTimeCheck()
	{
		if (root == base.gameObject)
		{
			return $"uGUI_PopupMessage : uGUI_PopupMessage.root == uGUI_PopupMessage.gameObject in\n{Dbg.LogHierarchy(base.gameObject)}";
		}
		RectTransform component = root.GetComponent<RectTransform>();
		if (component == null)
		{
			return $"uGUI_PopupMessage : root has no RectTransform component in\n{Dbg.LogHierarchy(base.gameObject)}";
		}
		Transform parent = component.parent;
		if (parent == null)
		{
			return $"uGUI_PopupMessage : root.GetComponent<RectTransform>().parent == null in\n{Dbg.LogHierarchy(base.gameObject)}";
		}
		if (parent.GetComponent<RectTransform>() == null)
		{
			return $"uGUI_PopupMessage : root.GetComponent<RectTransform>().parent has no RectTransform component in\n{Dbg.LogHierarchy(base.gameObject)}";
		}
		return null;
	}
}
