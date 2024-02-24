using System;
using System.Collections.Generic;
using UnityEngine;

public class uGUI_IconNotifier : MonoBehaviour
{
	public delegate void AnimationDone();

	public enum AnimationType
	{
		From = 0,
		To = 1
	}

	[Serializable]
	public class IconAnimator
	{
		public float duration = 1f;

		public AnimationCurve x;

		public AnimationCurve y;

		public AnimationCurve scale;

		public AnimationCurve alpha;

		public bool oscillate = true;

		[Range(0f, 0.999f)]
		public float oscOffset;

		public float oscReduction = 100f;

		public float oscFrequency = 5f;

		public float oscScale = 1f;
	}

	private class Request
	{
		public TechType techType;

		public AnimationType animation;

		public uGUI_ItemIcon icon;

		public float random;

		public float t;

		public AnimationDone callback;

		public Request(TechType techType, AnimationType animation, AnimationDone callback)
		{
			this.techType = techType;
			this.animation = animation;
			this.callback = callback;
		}
	}

	public RectTransform canvas;

	public float interval = 0.3f;

	public float iconSize = 64f;

	public IconAnimator[] animators;

	private const int poolChunkSize = 4;

	private List<uGUI_ItemIcon> pool = new List<uGUI_ItemIcon>();

	private Queue<Request> queue = new Queue<Request>();

	private List<Request> active = new List<Request>();

	private float time;

	public static uGUI_IconNotifier main { get; private set; }

	private void Awake()
	{
		if (main == null)
		{
			main = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
		time = 0f - interval;
	}

	private void Update()
	{
		if (queue.Count > 0 && PDA.time - time >= interval)
		{
			Request request = queue.Dequeue();
			uGUI_ItemIcon instance = GetInstance();
			instance.SetForegroundSprite(SpriteManager.Get(request.techType));
			instance.SetSize(iconSize, iconSize);
			instance.SetAsLastSibling();
			instance.SetActive(active: true);
			request.icon = instance;
			request.random = UnityEngine.Random.value;
			active.Add(request);
			time = PDA.time;
		}
		for (int num = active.Count - 1; num >= 0; num--)
		{
			Request request2 = active[num];
			if (Animate(request2))
			{
				if (request2.callback != null)
				{
					request2.callback();
				}
				active.RemoveAt(num);
			}
		}
	}

	private void OnValidate()
	{
		if (interval < 0f)
		{
			interval = 0f;
		}
		if (iconSize < 0f)
		{
			iconSize = 64f;
		}
	}

	public void Play(TechType techType, AnimationType animation, AnimationDone callback = null)
	{
		queue.Enqueue(new Request(techType, animation, callback));
	}

	private uGUI_ItemIcon GetInstance()
	{
		uGUI_ItemIcon uGUI_ItemIcon2;
		if (pool.Count == 0)
		{
			for (int i = 0; i < 4; i++)
			{
				GameObject obj = new GameObject("NotifierIcon");
				uGUI_ItemIcon2 = obj.AddComponent<uGUI_ItemIcon>();
				obj.layer = canvas.gameObject.layer;
				uGUI_ItemIcon2.Init(null, canvas, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f));
				uGUI_ItemIcon2.raycastTarget = false;
				uGUI_ItemIcon2.SetActive(active: false);
				pool.Add(uGUI_ItemIcon2);
			}
		}
		int index = pool.Count - 1;
		uGUI_ItemIcon2 = pool[index];
		pool.RemoveAt(index);
		return uGUI_ItemIcon2;
	}

	private void ReleaseInstance(uGUI_ItemIcon iconData)
	{
		if (!(iconData == null))
		{
			iconData.SetActive(active: false);
			pool.Add(iconData);
		}
	}

	private bool Animate(Request request)
	{
		AnimationType animation = request.animation;
		IconAnimator iconAnimator = animators[(int)animation];
		float t = request.t;
		Rect rect = canvas.rect;
		uGUI_ItemIcon icon = request.icon;
		icon.SetPosition(iconAnimator.x.Evaluate(t) * rect.width, iconAnimator.y.Evaluate(t) * rect.height);
		float num = iconAnimator.scale.Evaluate(t);
		float o = 0f;
		float o2 = 0f;
		if (iconAnimator.oscillate && t >= iconAnimator.oscOffset)
		{
			MathExtensions.Oscillation(t: Mathf.Clamp01(t - iconAnimator.oscOffset) / (1f - iconAnimator.oscOffset) * iconAnimator.duration, reduction: iconAnimator.oscReduction, frequency: iconAnimator.oscFrequency, seed: request.random, o: out o, o1: out o2);
		}
		icon.SetScale(num + o * iconAnimator.oscScale, num + o2 * iconAnimator.oscScale);
		float num2 = iconAnimator.alpha.Evaluate(t);
		icon.SetAlpha(num2, num2, num2);
		if (request.t == 1f)
		{
			ReleaseInstance(request.icon);
			request.icon = null;
			return true;
		}
		request.t = Mathf.Clamp01(request.t + PDA.deltaTime / iconAnimator.duration);
		return false;
	}
}
