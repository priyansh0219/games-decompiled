using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_SceneProgressBar : uGUI_InputGroup
{
	public delegate float GetProgressDelegate();

	[AssertNotNull]
	public Image loadingBar;

	public TextMeshProUGUI description;

	private GetProgressDelegate GetProgress;

	private uGUI_InputGroup restoreGroup;

	private Material materialBar;

	private float smoothProgress;

	private float currentPogressSpeed;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdate;

	protected override void Awake()
	{
		base.Awake();
		materialBar = new Material(loadingBar.material);
		loadingBar.material = materialBar;
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdate, OnLateUpdate);
	}

	private void OnDestroy()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdate, OnLateUpdate);
		UWE.Utils.DestroyWrap(materialBar);
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		base.gameObject.SetActive(value: false);
	}

	public void Show(string descriptionText, GetProgressDelegate getProgress, uGUI_InputGroup restore = null)
	{
		GetProgress = getProgress;
		description.text = descriptionText;
		base.gameObject.SetActive(value: true);
		restoreGroup = null;
		uGUI_InputGroup uGUI_InputGroup2 = Select(lockMovement: true);
		restoreGroup = ((restore != null) ? restore : uGUI_InputGroup2);
	}

	public void Close()
	{
		uGUI_InputGroup uGUI_InputGroup2 = restoreGroup;
		restoreGroup = null;
		Deselect(uGUI_InputGroup2);
		GetProgress = null;
		materialBar.SetFloat(ShaderPropertyID._Amount, 0f);
	}

	private void OnLateUpdate()
	{
		if (GetProgress != null)
		{
			float target = GetProgress();
			smoothProgress = Mathf.SmoothDamp(smoothProgress, target, ref currentPogressSpeed, 0.5f);
			materialBar.SetFloat(ShaderPropertyID._Amount, smoothProgress);
		}
	}
}
