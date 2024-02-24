using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Gendarme;
using UnityEngine;
using UnityEngine.UI;

public class ManagedUpdate : MonoBehaviour, ICanvasElement
{
	public delegate void OnUpdate();

	[SuppressMessage("Gendarme.Rules.Naming", "UseCorrectSuffixRule")]
	public enum Queue
	{
		First = 0,
		UpdateFirst = 1,
		UpdateBeforeInput = 2,
		UpdateCameraTransform = 3,
		UpdateInput = 4,
		UpdateGUIHand = 5,
		UpdateAfterInput = 6,
		Update = 7,
		UpdatePDA = 8,
		UpdateLast = 9,
		LateUpdateFirst = 10,
		LateUpdateBeforeInput = 11,
		LateUpdateCameraTransform = 12,
		LateUpdateInput = 13,
		LateUpdateCamera = 14,
		LateUpdateAfterInput = 15,
		LateUpdate = 16,
		LateUpdateLast = 17,
		PreCanvasFirst = 18,
		PreCanvasRectTransform = 19,
		PreCanvasCanvasScaler = 20,
		PreCanvasDrag = 21,
		PreCanvasPing = 22,
		PreCanvasTooltip = 23,
		PreCanvasLast = 24,
		UIPreLayout = 25,
		UILayout = 26,
		UIPostLayout = 27,
		UILayoutComplete = 28,
		UILayoutCompleteSelection = 29,
		UIPreRender = 30,
		UILatePreRender = 31,
		UIGraphicUpdateComplete = 32,
		OnGUIFirst = 33,
		OnGUI = 34,
		OnGUILast = 35,
		EndOfFrameFirst = 36,
		EndOfFrame = 37,
		EndOfFrameLast = 38,
		Last = 39
	}

	public class QueueComparer : IEqualityComparer<Queue>
	{
		public bool Equals(Queue x, Queue y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(Queue obj)
		{
			return (int)obj;
		}
	}

	private static ManagedUpdate _main;

	public static readonly QueueComparer sQueueComparer = new QueueComparer();

	private readonly Dictionary<Queue, List<OnUpdate>> subscribers = new Dictionary<Queue, List<OnUpdate>>(sQueueComparer);

	private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	private Coroutine endOfFrameRoutine;

	public static ManagedUpdate main
	{
		get
		{
			if (_main == null)
			{
				GameObject gameObject = new GameObject("ManagedUpdate");
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				_main = gameObject.AddComponent<ManagedUpdate>();
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
			}
			return _main;
		}
	}

	public Queue lastQueue { get; private set; }

	Transform ICanvasElement.transform => null;

	private void OnEnable()
	{
		endOfFrameRoutine = StartCoroutine(EndOfFrameRoutine());
		Canvas.preWillRenderCanvases += OnPreWillRenderCanvases;
	}

	private void OnDisable()
	{
		Canvas.preWillRenderCanvases -= OnPreWillRenderCanvases;
		if (endOfFrameRoutine != null)
		{
			StopCoroutine(endOfFrameRoutine);
			endOfFrameRoutine = null;
		}
	}

	private void Update()
	{
		CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
		CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
		ExecuteRange(Queue.UpdateFirst, Queue.UpdateLast);
	}

	private void LateUpdate()
	{
		ExecuteRange(Queue.LateUpdateFirst, Queue.LateUpdateLast);
		base.useGUILayout = HasAnyListeners(Queue.OnGUIFirst, Queue.OnGUILast);
	}

	private void OnGUI()
	{
		ExecuteRange(Queue.OnGUIFirst, Queue.OnGUILast);
	}

	public static void Subscribe(Queue queue, OnUpdate action)
	{
		if (action != null)
		{
			if (!main.subscribers.TryGetValue(queue, out var value))
			{
				value = new List<OnUpdate>();
				main.subscribers.Add(queue, value);
			}
			value.Add(action);
		}
	}

	public static void Unsubscribe(Queue queue, OnUpdate action)
	{
		if (!main.subscribers.TryGetValue(queue, out var value))
		{
			return;
		}
		int num = value.LastIndexOf(action);
		if (num >= 0)
		{
			value.RemoveAt(num);
			if (value.Count == 0)
			{
				main.subscribers.Remove(queue);
			}
		}
	}

	public static void Unsubscribe(OnUpdate action)
	{
		for (Queue queue = Queue.First; queue <= Queue.Last; queue++)
		{
			Unsubscribe(queue, action);
		}
	}

	private void OnPreWillRenderCanvases()
	{
		ExecuteRange(Queue.PreCanvasFirst, Queue.PreCanvasLast);
	}

	private IEnumerator EndOfFrameRoutine()
	{
		while (true)
		{
			yield return waitForEndOfFrame;
			ExecuteRange(Queue.EndOfFrameFirst, Queue.EndOfFrameLast);
			lastQueue = Queue.First;
		}
	}

	private void ExecuteRange(Queue first, Queue last)
	{
		for (Queue queue = first; queue <= last; queue++)
		{
			Execute(queue);
		}
	}

	private bool HasAnyListeners(Queue first, Queue last)
	{
		for (Queue queue = first; queue <= last; queue++)
		{
			if (subscribers.ContainsKey(queue))
			{
				return true;
			}
		}
		return false;
	}

	private void Execute(Queue queue)
	{
		lastQueue = queue;
		if (!subscribers.TryGetValue(queue, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			OnUpdate onUpdate = value[num];
			if (MathExtensions.IsDestroyed(onUpdate))
			{
				value.RemoveAt(num);
			}
			else
			{
				try
				{
					onUpdate();
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.ToString());
				}
			}
		}
		if (value.Count == 0)
		{
			subscribers.Remove(queue);
		}
	}

	void ICanvasElement.Rebuild(CanvasUpdate executing)
	{
		switch (executing)
		{
		case CanvasUpdate.Prelayout:
			Execute(Queue.UIPreLayout);
			break;
		case CanvasUpdate.Layout:
			Execute(Queue.UILayout);
			break;
		case CanvasUpdate.PostLayout:
			Execute(Queue.UIPostLayout);
			break;
		case CanvasUpdate.PreRender:
			Execute(Queue.UIPreRender);
			break;
		case CanvasUpdate.LatePreRender:
			Execute(Queue.UILatePreRender);
			break;
		}
	}

	void ICanvasElement.LayoutComplete()
	{
		Execute(Queue.UILayoutComplete);
		Execute(Queue.UILayoutCompleteSelection);
	}

	void ICanvasElement.GraphicUpdateComplete()
	{
		Execute(Queue.UIGraphicUpdateComplete);
	}

	bool ICanvasElement.IsDestroyed()
	{
		return this == null;
	}

	private string GetDebug()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (Queue queue = Queue.First; queue <= Queue.Last; queue++)
		{
			if (subscribers.TryGetValue(queue, out var value))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('\n');
				}
				stringBuilder.Append(queue);
				for (int i = 0; i < value.Count; i++)
				{
					OnUpdate onUpdate = value[i];
					stringBuilder.Append("\n    ").Append(i).Append(' ')
						.Append((onUpdate != null) ? string.Concat(onUpdate.Method.DeclaringType, " ", onUpdate.Method) : "null");
				}
			}
		}
		return stringBuilder.ToString();
	}

	private void DrawDebug()
	{
		GUIStyle label = GUI.skin.label;
		GUIContent content = new GUIContent(GetDebug());
		Vector2 vector = label.CalcSize(content);
		GUI.Label(new Rect(10f, 10f, vector.x, vector.y), content);
	}
}
