using System;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
public class uGUI_Pings : MonoBehaviour
{
	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.PreCanvasPing;

	private const float scaleOffScreen = 0.35f;

	private const float scaleOnScreen = 0.5f;

	private const float scaleHover = 1f;

	private const float interpInRate = 12f;

	private const float interpOutRate = 8f;

	[AssertNotNull]
	public uGUI_Ping prefabPing;

	[AssertNotNull]
	public RectTransform pingCanvas;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	public float padding = 40f;

	public float alphaMin;

	public float alphaMax = 0.5f;

	private Dictionary<string, uGUI_Ping> pings = new Dictionary<string, uGUI_Ping>(20);

	private PrefabPool<uGUI_Ping> poolPings;

	private bool visible = true;

	private void Awake()
	{
		poolPings = new PrefabPool<uGUI_Ping>(prefabPing.gameObject, pingCanvas, 10, 4, delegate(uGUI_Ping entry)
		{
			entry.rectTransform.anchorMin = Vector2.zero;
			entry.rectTransform.anchorMax = Vector2.zero;
			entry.Uninitialize();
		}, delegate(uGUI_Ping entry)
		{
			entry.Uninitialize();
		});
	}

	private void OnEnable()
	{
		foreach (KeyValuePair<string, PingInstance> item in PingManager)
		{
			OnAdd(item.Value);
		}
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasPing, UpdatePings);
		PingManager.onAdd = (PingManager.OnAdd)Delegate.Combine(PingManager.onAdd, new PingManager.OnAdd(OnAdd));
		PingManager.onRemove = (PingManager.OnRemove)Delegate.Combine(PingManager.onRemove, new PingManager.OnRemove(OnRemove));
		PingManager.onRename = (PingManager.OnRename)Delegate.Combine(PingManager.onRename, new PingManager.OnRename(OnRename));
		PingManager.onIconChange = (PingManager.OnIconChange)Delegate.Combine(PingManager.onIconChange, new PingManager.OnIconChange(OnIconChange));
		PingManager.onColor = (PingManager.OnColor)Delegate.Combine(PingManager.onColor, new PingManager.OnColor(OnColor));
		PingManager.onVisible = (PingManager.OnVisible)Delegate.Combine(PingManager.onVisible, new PingManager.OnVisible(OnVisible));
	}

	private void OnDisable()
	{
		using (Dictionary<string, uGUI_Ping>.Enumerator enumerator = pings.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				poolPings.Release(enumerator.Current.Value);
			}
		}
		pings.Clear();
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasPing, UpdatePings);
		PingManager.onAdd = (PingManager.OnAdd)Delegate.Remove(PingManager.onAdd, new PingManager.OnAdd(OnAdd));
		PingManager.onRemove = (PingManager.OnRemove)Delegate.Remove(PingManager.onRemove, new PingManager.OnRemove(OnRemove));
		PingManager.onRename = (PingManager.OnRename)Delegate.Remove(PingManager.onRename, new PingManager.OnRename(OnRename));
		PingManager.onIconChange = (PingManager.OnIconChange)Delegate.Remove(PingManager.onIconChange, new PingManager.OnIconChange(OnIconChange));
		PingManager.onColor = (PingManager.OnColor)Delegate.Remove(PingManager.onColor, new PingManager.OnColor(OnColor));
		PingManager.onVisible = (PingManager.OnVisible)Delegate.Remove(PingManager.onVisible, new PingManager.OnVisible(OnVisible));
	}

	private bool IsVisibleNow()
	{
		Player main = Player.main;
		if (main == null)
		{
			return false;
		}
		if (main.cinematicModeActive && main.GetPilotingChair() == null)
		{
			return false;
		}
		if (main.GetMode() == Player.Mode.Sitting)
		{
			return false;
		}
		PDA pDA = main.GetPDA();
		if (pDA == null)
		{
			return false;
		}
		uGUI_PDA main2 = uGUI_PDA.main;
		if (pDA.isInUse && main2 != null && main2.currentTabType != PDATab.Ping)
		{
			return false;
		}
		if (uGUI.main.craftingMenu.selected)
		{
			return false;
		}
		return true;
	}

	private void UpdatePings()
	{
		try
		{
			bool flag = IsVisibleNow();
			if (visible != flag)
			{
				visible = flag;
				canvasGroup.alpha = (visible ? 1f : 0f);
			}
			if (!visible)
			{
				return;
			}
			Camera camera = MainCamera.camera;
			Transform transform = camera.transform;
			Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
			float aspect = camera.aspect;
			float num = Mathf.Tan(camera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
			Rect rect = pingCanvas.rect;
			float num2 = rect.width * 0.5f;
			float num3 = rect.height * 0.5f;
			float num4 = num2 - padding;
			float num5 = num3 - padding;
			Vector3 forward = transform.forward;
			Vector3 position = transform.position;
			foreach (KeyValuePair<string, uGUI_Ping> ping in pings)
			{
				PingInstance pingInstance = PingManager.Get(ping.Key);
				if (!pingInstance.visible)
				{
					continue;
				}
				uGUI_Ping value = ping.Value;
				float minDist = pingInstance.minDist;
				float range = pingInstance.range;
				Vector3 position2 = pingInstance.GetPosition();
				Vector3 vector = new Vector3(worldToLocalMatrix.m00 * position2.x + worldToLocalMatrix.m01 * position2.y + worldToLocalMatrix.m02 * position2.z + worldToLocalMatrix.m03, worldToLocalMatrix.m10 * position2.x + worldToLocalMatrix.m11 * position2.y + worldToLocalMatrix.m12 * position2.z + worldToLocalMatrix.m13, worldToLocalMatrix.m20 * position2.x + worldToLocalMatrix.m21 * position2.y + worldToLocalMatrix.m22 * position2.z + worldToLocalMatrix.m23);
				float magnitude = vector.magnitude;
				if (magnitude > minDist)
				{
					float num6 = Mathf.Abs(vector.z) * num;
					float num7 = num6 * aspect;
					Vector2 vector2 = new Vector2(vector.x / num7, vector.y / num6);
					Vector2 vector3 = new Vector2(vector2.x * num2, vector2.y * num3);
					float num8 = -1f;
					if (vector.z < 0f || Mathf.Abs(vector3.x) > num4 || Mathf.Abs(vector3.y) > num5)
					{
						num8 = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
						num8 = (num8 + 360f) % 360f;
						float num9 = vector3.y / vector3.x;
						if (vector3.y >= 0f)
						{
							vector3.x = num5 / num9;
							vector3.y = num5;
						}
						else
						{
							vector3.x = (0f - num5) / num9;
							vector3.y = 0f - num5;
						}
						if (vector3.x > num4)
						{
							vector3.x = num4;
							vector3.y = num4 * num9;
						}
						else if (vector3.x < 0f - num4)
						{
							vector3.x = 0f - num4;
							vector3.y = (0f - num4) * num9;
						}
					}
					value.SetAngle(num8);
					value.rectTransform.anchoredPosition = new Vector2(vector3.x + num2, vector3.y + num3);
					float num10 = ((range > 0f) ? Mathf.Lerp(alphaMin, alphaMax, (magnitude - minDist) / range) : 0f);
					value.SetIconAlpha(num10);
					if (num10 > 0f)
					{
						value.SetDistance(magnitude);
					}
					value.gameObject.SetActive(value: true);
					Vector3 normalized = (position2 - position).normalized;
					bool num11 = Vector3.Dot(forward, normalized) > 0.9848f;
					float target = (num11 ? 1f : 0f);
					value.SetTextAlpha(MathExtensions.StableLerp(value.GetTextAlpha(), target, 12f, 8f, Time.unscaledDeltaTime));
					float target2 = (num11 ? 1f : ((num8 < 0f) ? 0.5f : 0.35f));
					value.SetScale(MathExtensions.StableLerp(value.GetScale(), target2, 12f, 8f, Time.unscaledDeltaTime));
				}
				else
				{
					value.SetIconAlpha(0f);
					value.SetTextAlpha(0f);
					value.gameObject.SetActive(value: false);
				}
			}
		}
		finally
		{
		}
	}

	private void OnAdd(PingInstance instance)
	{
		uGUI_Ping uGUI_Ping2 = poolPings.Get();
		uGUI_Ping2.Initialize();
		uGUI_Ping2.SetVisible(instance.visible);
		uGUI_Ping2.SetColor(PingManager.colorOptions[instance.colorIndex]);
		uGUI_Ping2.SetIcon(SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(instance.pingType)));
		uGUI_Ping2.SetLabel(instance.GetLabel());
		uGUI_Ping2.SetIconAlpha(0f);
		uGUI_Ping2.SetTextAlpha(0f);
		pings.Add(instance.Id, uGUI_Ping2);
	}

	private void OnRemove(string id)
	{
		if (pings.TryGetValue(id, out var value))
		{
			pings.Remove(id);
			poolPings.Release(value);
		}
	}

	private void OnRename(PingInstance instance)
	{
		if (!(instance == null) && pings.TryGetValue(instance.Id, out var value))
		{
			value.SetLabel(instance.GetLabel());
		}
	}

	private void OnIconChange(PingInstance instance)
	{
		if (!(instance == null) && pings.TryGetValue(instance.Id, out var value))
		{
			value.SetIcon(SpriteManager.Get(SpriteManager.Group.Pings, PingManager.sCachedPingTypeStrings.Get(instance.pingType)));
		}
	}

	private void OnColor(string id, Color color)
	{
		if (pings.TryGetValue(id, out var value))
		{
			value.SetColor(color);
		}
	}

	private void OnVisible(string id, bool visible)
	{
		if (pings.TryGetValue(id, out var value))
		{
			value.SetVisible(visible);
		}
	}
}
