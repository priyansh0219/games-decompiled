using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class uGUI_RectTransformCallback : UIBehaviour
{
	public delegate void RectTransformCallback();

	private RectTransform _rt;

	public RectTransform rt
	{
		get
		{
			if (_rt == null)
			{
				_rt = GetComponent<RectTransform>();
			}
			return _rt;
		}
	}

	public event RectTransformCallback onDimensionsChange;

	public event RectTransformCallback onTransformChange;

	private void LateUpdate()
	{
		if (rt.hasChanged)
		{
			NotifyTransformChange();
			rt.hasChanged = false;
		}
	}

	private void NotifyTransformChange()
	{
		if (this.onTransformChange != null)
		{
			this.onTransformChange();
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		if (this.onDimensionsChange != null)
		{
			this.onDimensionsChange();
		}
	}
}
