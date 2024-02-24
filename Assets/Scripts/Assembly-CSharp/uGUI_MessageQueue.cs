using System;
using System.Collections.Generic;
using System.Text;
using Gendarme;
using TMPro;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Naming", "UseCorrectSuffixRule")]
public class uGUI_MessageQueue : MonoBehaviour, ICompileTimeCheckable
{
	private sealed class Message : IComparable<Message>
	{
		public int id;

		public int group;

		public float delay;

		public float duration;

		public float time;

		public TextMeshProUGUI component;

		public void Initialize(int id, int group, StringBuilder text, float delay, float duration)
		{
			this.id = id;
			this.group = group;
			this.delay = delay;
			this.duration = duration;
			component.gameObject.SetActive(value: true);
			component.SetAlpha(0f);
			component.SetText(text);
		}

		public void Deinitialize()
		{
			id = 0;
			group = 0;
			delay = 0f;
			duration = 0f;
			time = 0f;
			component.SetText("\u200b");
			component.gameObject.SetActive(value: false);
		}

		public int CompareTo(Message other)
		{
			int num = group.CompareTo(other.group);
			if (num != 0)
			{
				return num;
			}
			return (delay - time).CompareTo(other.delay - other.time);
		}
	}

	private enum Easing
	{
		None = 0,
		SineIn = 1,
		SineOut = 2,
		SineInOut = 3
	}

	[AssertNotNull]
	public GameObject prefab;

	[AssertNotNull]
	public RectTransform canvas;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[Range(0.01f, 1f)]
	public float timeFadeIn = 0.2f;

	[Range(0.01f, 1f)]
	public float timeFadeOut = 0.2f;

	[Range(0.01f, 1f)]
	public float timeSizeIn = 0.2f;

	[Range(0.01f, 1f)]
	public float timeAlphaIn = 0.1f;

	[Range(0.01f, 1f)]
	public float timeAlphaOut = 0.1f;

	[Range(0.01f, 1f)]
	public float timeSizeOut = 0.2f;

	public float lineSpacing = 5f;

	[NonSerialized]
	[HideInInspector]
	public bool visible;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.PreCanvasRectTransform;

	private List<Message> messages = new List<Message>();

	private int group;

	private bool sort;

	private bool lastActive;

	private float time;

	private ObjPool<Message> poolMessages;

	private void Awake()
	{
		poolMessages = new ObjPool<Message>(4, 4, delegate(Message message)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, canvas);
			message.component = gameObject.GetComponent<TextMeshProUGUI>();
			gameObject.SetActive(value: false);
		}, delegate(Message message)
		{
			message.Deinitialize();
		});
	}

	private void OnEnable()
	{
		time = timeFadeOut;
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasRectTransform, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasRectTransform, OnUpdate);
	}

	private void OnUpdate()
	{
		float deltaTime = PDA.deltaTime;
		if (sort)
		{
			sort = false;
			messages.Sort();
		}
		bool flag = true;
		bool flag2 = false;
		for (int i = 0; i < messages.Count; i++)
		{
			Message message = messages[i];
			float num = message.delay + timeSizeIn + timeAlphaIn + message.duration;
			if (message.time > message.delay && message.time < num)
			{
				flag2 = true;
			}
			message.time += deltaTime;
			if (flag)
			{
				if (message.time > message.delay && message.time < num)
				{
					flag = false;
				}
			}
			else
			{
				message.duration += Mathf.Clamp(message.time - num, 0f, deltaTime);
			}
		}
		for (int num2 = messages.Count - 1; num2 >= 0; num2--)
		{
			Message message2 = messages[num2];
			if (message2.time >= message2.delay + timeSizeIn + timeAlphaIn + message2.duration + timeAlphaOut + timeSizeOut)
			{
				messages.RemoveAt(num2);
				poolMessages.Release(message2);
			}
		}
		float num3 = 0f;
		float width = canvas.rect.width;
		for (int j = 0; j < messages.Count; j++)
		{
			Message message3 = messages[j];
			if (!(message3.time < message3.delay))
			{
				TextMeshProUGUI component = message3.component;
				RectTransform rectTransform = component.rectTransform;
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
				float num4 = (component.preferredHeight + lineSpacing) * Trapezoid(message3.delay, timeSizeIn, timeAlphaIn + message3.duration + timeAlphaOut, timeSizeOut, Easing.SineOut, Easing.SineIn, message3.time);
				float alpha = Trapezoid(message3.delay + timeSizeIn, timeAlphaIn, message3.duration, timeAlphaOut, Easing.SineOut, Easing.SineIn, message3.time);
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num4);
				component.SetAlpha(alpha);
				rectTransform.anchoredPosition = new Vector2(0f, 0f - num3);
				num3 += num4;
			}
		}
		num3 = Mathf.Max(0f, num3 - lineSpacing);
		time += deltaTime;
		if (lastActive != flag2)
		{
			float num5 = (lastActive ? timeFadeIn : timeFadeOut);
			float num6 = (lastActive ? timeFadeOut : timeFadeIn);
			time = (1f - Mathf.Clamp01(time / num5)) * num6;
			lastActive = flag2;
		}
		float num7;
		if (visible)
		{
			num7 = Mathf.Clamp01(time / (lastActive ? timeFadeIn : timeFadeOut));
			if (!lastActive)
			{
				num7 = 1f - num7;
			}
			num7 = Ease(Easing.SineInOut, num7);
		}
		else
		{
			num7 = 0f;
		}
		canvasGroup.alpha = num7;
		canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num3);
	}

	public void NewGroup()
	{
		group++;
	}

	public void Add(int id, StringBuilder text, float delay, float duration)
	{
		sort = true;
		Message message = poolMessages.Get();
		message.Initialize(id, group, text, delay, duration);
		messages.Add(message);
	}

	public void Remove(int id)
	{
		if (id == 0)
		{
			return;
		}
		for (int num = messages.Count - 1; num >= 0; num--)
		{
			Message message = messages[num];
			if (message.id == id)
			{
				messages.RemoveAt(num);
				poolMessages.Release(message);
			}
		}
	}

	public bool HasQueued()
	{
		return messages.Count > 0;
	}

	private static float Ease(Easing mode, float value)
	{
		value = Mathf.Clamp01(value);
		switch (mode)
		{
		case Easing.SineIn:
			return 1f - Mathf.Cos(value * (float)Math.PI * 0.5f);
		case Easing.SineOut:
			return Mathf.Sin(value * (float)Math.PI * 0.5f);
		case Easing.SineInOut:
			return 0.5f - 0.5f * Mathf.Cos(value * (float)Math.PI);
		default:
			return value;
		}
	}

	private static float Trapezoid(float a, float b, float c, float d, Easing easeIn, Easing easeOut, float x)
	{
		float num = (x - a) / b;
		float num2 = (a + b + c + d - x) / d;
		if (!(num < num2))
		{
			return Ease(easeOut, num2);
		}
		return Ease(easeIn, num);
	}

	public string CompileTimeCheck()
	{
		if (prefab.GetComponent<TextMeshProUGUI>() == null)
		{
			return "prefab must have Text component assigned";
		}
		return null;
	}
}
