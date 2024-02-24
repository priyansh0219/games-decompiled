using System.Collections;
using UnityEngine;

public class uGUI_ScannerIcon : MonoBehaviour
{
	public static uGUI_ScannerIcon main;

	public Vector2 iconSize = new Vector2(72f, 72f);

	public float timeIn = 1f;

	public float timeOut = 0.5f;

	public float oscReduction = 100f;

	public float oscFrequency = 5f;

	public float oscScale = 2f;

	public float oscDuration = 2f;

	private uGUI_ItemIcon icon;

	private Sequence sequence = new Sequence();

	private bool show;

	private float oscSeed;

	private float oscTime;

	private void Awake()
	{
		if (main == null)
		{
			main = this;
			GameObject gameObject = new GameObject("ScannerIcon");
			icon = gameObject.AddComponent<uGUI_ItemIcon>();
			icon.Init(null, base.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
			StartCoroutine(SetupSpriteAsync());
			icon.SetBackgroundSprite(SpriteManager.GetBackground(CraftData.BackgroundType.Normal));
			icon.SetSize(iconSize);
			icon.SetBackgroundRadius(Mathf.Min(iconSize.x, iconSize.y) * 0.5f);
			SetAlpha(0f);
			sequence.ForceState(state: false);
		}
		else
		{
			Debug.LogError("uGUI_ScannerIcon : Awake() : Duplicate uGUI_ScannerIcon component found!");
			Object.Destroy(this);
		}
	}

	private IEnumerator SetupSpriteAsync()
	{
		while (!SpriteManager.hasInitialized)
		{
			yield return null;
		}
		icon.SetForegroundSprite(SpriteManager.Get(SpriteManager.Group.Item, TechType.Scanner.AsString()));
	}

	private void LateUpdate()
	{
		if (sequence.target != show)
		{
			if (show && sequence.t == 0f)
			{
				oscTime = Time.time;
				oscSeed = Random.value;
			}
			sequence.Set(show ? timeIn : timeOut, show);
		}
		if (sequence.active)
		{
			if (sequence.t > 0f)
			{
				float o = 0f;
				float o2 = 0f;
				float t = Mathf.Clamp01((Time.time - oscTime) / oscDuration);
				MathExtensions.Oscillation(oscReduction, oscFrequency, oscSeed, t, out o, out o2);
				icon.rectTransform.localScale = new Vector3(1f + o * oscScale, 1f + o2 * oscScale, 1f);
			}
			sequence.Update();
		}
		SetAlpha(sequence.target ? 1f : sequence.t);
		show = false;
	}

	private void SetAlpha(float alpha)
	{
		icon.SetAlpha(alpha, alpha, alpha);
	}

	public void Show()
	{
		show = true;
	}
}
