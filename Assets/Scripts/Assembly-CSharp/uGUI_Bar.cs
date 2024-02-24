using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class uGUI_Bar : MonoBehaviour
{
	public Image icon;

	public Text text;

	public bool percent;

	public float dampSpeed = 0.1f;

	public float topBorder = 0.04f;

	public float bottomBorder = 0.04f;

	public Gradient colorIcon = new Gradient();

	public Gradient colorBar = new Gradient();

	protected GameObject go;

	protected Image image;

	protected Material mat;

	protected float curr;

	protected float vel;

	private int cachedValue = int.MinValue;

	private int cachedCapacity = int.MinValue;

	[AssertLocalization(1)]
	private const string barPercentFormatKey = "BarPercentFormat";

	[AssertLocalization(2)]
	private const string barFractionFormatKey = "BarFractionFormat";

	protected virtual void Awake()
	{
		go = base.gameObject;
		image = GetComponent<Image>();
		mat = image.material;
		if (mat != null)
		{
			mat = new Material(mat);
			image.material = mat;
		}
		mat.SetFloat(ShaderPropertyID._TopBorder, topBorder);
		mat.SetFloat(ShaderPropertyID._BottomBorder, bottomBorder);
		InvokeRepeating("UpdateVisibility", 0f, 0.25f);
	}

	protected virtual void LateUpdate()
	{
	}

	public void SetValue(float has, float capacity, string stringValue = null)
	{
		float num = has / capacity;
		int num2 = Mathf.RoundToInt(has);
		int num3 = Mathf.RoundToInt(capacity);
		if (text != null)
		{
			if (stringValue != null)
			{
				text.text = stringValue;
			}
			else if (percent)
			{
				int num4 = Mathf.RoundToInt(num * 100f);
				if (cachedValue != num4)
				{
					cachedValue = num4;
					text.text = Language.main.GetFormat("BarPercentFormat", num);
				}
			}
			else if (cachedValue != num2 || cachedCapacity != num3)
			{
				cachedValue = num2;
				cachedCapacity = num3;
				text.text = Language.main.GetFormat("BarFractionFormat", num2, num3);
			}
		}
		curr = Mathf.SmoothDamp(curr, num, ref vel, dampSpeed);
		mat.SetFloat(ShaderPropertyID._Amount, curr);
		mat.SetColor(ShaderPropertyID._Color, colorBar.Evaluate(num));
		if (icon != null)
		{
			icon.color = colorIcon.Evaluate(num);
		}
	}

	protected virtual bool IsVisible()
	{
		return true;
	}

	private void UpdateVisibility()
	{
		base.gameObject.SetActive(IsVisible());
	}
}
