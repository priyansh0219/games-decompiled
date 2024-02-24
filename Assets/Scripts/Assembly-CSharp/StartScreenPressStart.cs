using TMPro;
using UnityEngine;

public class StartScreenPressStart : MonoBehaviour
{
	[AssertNotNull]
	public TextMeshProUGUI text;

	[AssertLocalization]
	private const string keyStartScreenLoading = "StartScreenLoading";

	[AssertLocalization(1)]
	private const string keyPressStart = "PressStart";

	private bool loading;

	private Vector2 initPos;

	private RectTransform textRT;

	private float animT;

	public bool IsLoading => loading;

	private void OnEnable()
	{
		OnLanguageChanged();
		Language.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDisable()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
	}

	private void Start()
	{
		textRT = text.GetComponent<RectTransform>();
		initPos = textRT.anchoredPosition;
		SetLoading(_loading: false);
	}

	private void Update()
	{
		animT += Time.deltaTime;
		if (loading)
		{
			textRT.anchoredPosition = initPos;
			textRT.localScale = Vector3.one;
			float a = (1f + Mathf.Sin(animT * 10f)) * 0.5f;
			text.color = new Color(text.color.r, text.color.g, text.color.b, a);
		}
		else
		{
			float num = 0.95f + Mathf.Max(0f, (Mathf.Sin(animT * 5f) + 0.1f) * 0.1f);
			textRT.localScale = new Vector3(num, num, 1f);
		}
	}

	private void OnLanguageChanged()
	{
		string text = ((!loading) ? Language.main.GetFormat("PressStart", GameInput.FormatButton(GameInput.Button.UIMenu)) : Language.main.Get("StartScreenLoading"));
		this.text.text = text;
	}

	public void SetLoading(bool _loading)
	{
		loading = _loading;
		animT = 0f;
		OnLanguageChanged();
	}
}
