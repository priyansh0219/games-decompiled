using TMPro;
using UnityEngine;

public class uGUI_RecipeItem : MonoBehaviour, ITooltip
{
	[AssertNotNull]
	public uGUI_ItemIcon icon;

	[AssertNotNull]
	public TextMeshProUGUI text;

	private TechType techType;

	private int has = int.MinValue;

	private int needs = int.MinValue;

	public RectTransform rectTransform { get; private set; }

	public bool showTooltipOnDrag => false;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void Initialize()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Deinitialize()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Set(TechType techType, int has, int needs, bool ping)
	{
		this.techType = techType;
		icon.SetForegroundSprite(SpriteManager.Get(techType));
		if (ping && has > this.has)
		{
			icon.PunchScale();
		}
		if (this.has != has || this.needs != needs)
		{
			this.has = has;
			this.needs = needs;
			text.text = $"{IntStringCache.GetStringForInt(this.has)}/{IntStringCache.GetStringForInt(this.needs)}";
		}
	}

	public void GetTooltip(TooltipData data)
	{
		TooltipFactory.Ingredient(techType, data);
	}
}
