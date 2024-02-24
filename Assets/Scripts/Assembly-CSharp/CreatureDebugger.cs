using System.Collections.Generic;
using TMPro;
using UWE;
using UnityEngine;

public class CreatureDebugger : MonoBehaviour
{
	private class CreatureLabel
	{
		public Creature creature;

		public TextMeshProUGUI guiText;

		public RectTransform rectTransform;
	}

	[SerializeField]
	[AssertNotNull]
	private RectTransform rectTransform;

	[SerializeField]
	[AssertNotNull]
	private GameObject labelPrefab;

	public static CreatureDebugger main;

	public bool debug;

	public float debugRadius = 20f;

	private float timeSinceFindCreatures;

	private CreatureTracker tracker;

	private int numDebugStrings;

	private List<CreatureLabel> labels = new List<CreatureLabel>();

	private void Start()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "debugcreatures");
		DevConsole.RegisterConsoleCommand(this, "dbc");
		TextMeshProUGUI[] componentsInChildren = base.gameObject.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
		{
			RectTransform component = textMeshProUGUI.GetComponent<RectTransform>();
			if (component != null)
			{
				CreatureLabel creatureLabel = new CreatureLabel();
				creatureLabel.guiText = textMeshProUGUI;
				creatureLabel.rectTransform = component;
				labels.Add(creatureLabel);
			}
		}
	}

	public void OnConsoleCommand_dbc(NotificationCenter.Notification n)
	{
		OnConsoleCommand_debugcreatures(n);
	}

	public void OnConsoleCommand_debugcreatures(NotificationCenter.Notification n)
	{
		debug = !debug;
		TechType result = TechType.None;
		float result2 = 20f;
		if (n != null && n.data != null && n.data.Count > 0)
		{
			debug = true;
			string text = (string)n.data[0];
			if (!float.TryParse(text, out result2))
			{
				if (UWE.Utils.TryParseEnum<TechType>(text, out result))
				{
					result2 = 500f;
				}
				if (n.data.Count > 1)
				{
					float.TryParse((string)n.data[1], out result2);
				}
			}
			result2 = Mathf.Clamp(result2, 0f, 500f);
			if (result2 == 0f)
			{
				result2 = 20f;
			}
		}
		string text2 = $"Creature debugger now {debug}";
		if (debug)
		{
			string arg = ((result == TechType.None) ? string.Empty : $"{result.ToString()}, ");
			string arg2 = $"debug radius: {result2}";
			text2 = $"{text2} ({arg}{arg2})";
		}
		ErrorMessage.AddDebug(text2);
		if (debug)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = "CreatureDebugger";
			gameObject.transform.parent = Player.main.transform;
			gameObject.transform.localPosition = Vector3.zero;
			tracker = gameObject.AddComponent<CreatureTracker>();
			tracker.radius = result2;
			tracker.techTypeFilter = result;
		}
		else
		{
			Object.Destroy(tracker.gameObject);
			for (int i = 0; i < labels.Count; i++)
			{
				labels[i].guiText.text = string.Empty;
			}
		}
		foreach (CreatureLabel label in labels)
		{
			label.guiText.gameObject.SetActive(debug);
		}
	}

	private void AssignCreaturesToLabels()
	{
		for (int i = 0; i < labels.Count; i++)
		{
			CreatureLabel creatureLabel = labels[i];
			creatureLabel.creature = null;
			creatureLabel.guiText.text = string.Empty;
		}
		int num = 0;
		foreach (GameObject item in tracker.Get())
		{
			if (!(item == null))
			{
				if (num < labels.Count)
				{
					CreatureLabel creatureLabel2 = labels[num];
				}
				else
				{
					GameObject gameObject = Object.Instantiate(labelPrefab);
					gameObject.SetActive(value: true);
					RectTransform component = gameObject.GetComponent<RectTransform>();
					component.SetParent(rectTransform, worldPositionStays: false);
					component.localScale = Vector3.one;
					CreatureLabel creatureLabel2 = new CreatureLabel
					{
						guiText = gameObject.GetComponent<TextMeshProUGUI>(),
						rectTransform = component
					};
					labels.Add(creatureLabel2);
				}
				Creature component2 = item.GetComponent<Creature>();
				if (component2 != null)
				{
					labels[num].creature = component2;
					num++;
				}
			}
		}
		timeSinceFindCreatures = Time.time;
	}

	private void Update()
	{
		if (debug)
		{
			numDebugStrings = 0;
			if (Time.time > timeSinceFindCreatures + 0.5f)
			{
				AssignCreaturesToLabels();
			}
			for (int i = 0; i < labels.Count; i++)
			{
				UpdateCreatureLabel(labels[i], rectTransform.rect, ref numDebugStrings);
			}
		}
	}

	private static void UpdateCreatureLabel(CreatureLabel label, Rect rect, ref int numDebugStrings)
	{
		Creature creature = label.creature;
		if ((bool)creature)
		{
			string debugString = creature.GetDebugString();
			Vector3 vector = MainCamera.camera.WorldToScreenPoint(creature.transform.position);
			float num = vector.x / (float)Screen.width;
			float num2 = vector.y / (float)Screen.height;
			label.rectTransform.anchoredPosition = new Vector2((num - 0.5f) * rect.width, (num2 - 0.5f) * rect.height);
			float a = 1f;
			if (vector.z < 0f)
			{
				a = 0f;
			}
			label.guiText.color = new Color(1f, 1f, 1f, a);
			label.guiText.text = debugString;
		}
	}
}
