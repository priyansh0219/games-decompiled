using TMPro;
using UnityEngine;

public class BaseBioReactorGeometry : MonoBehaviour, IBaseModuleGeometry, IObstacle
{
	public Transform storagePivot;

	public Animator animator;

	public TextMeshProUGUI text;

	[AssertNotNull]
	public VFXAnimator liquidAnim;

	[AssertNotNull]
	public VFXController fxControl;

	[AssertNotNull]
	public LightingController lightsControl;

	[AssertLocalization]
	private const string bioReactorActiveMessage = "BaseBioReactorActive";

	[AssertLocalization]
	private const string bioReactorInactiveMessage = "BaseBioReactorInactive";

	private Base.Face _geometryFace;

	public Base.Face geometryFace
	{
		get
		{
			return _geometryFace;
		}
		set
		{
			_geometryFace = value;
			InitState();
		}
	}

	private void Start()
	{
		InitState();
		lightsControl.LerpToState(1, 1f);
		liquidAnim.duration = 1f;
		liquidAnim.Play();
	}

	public void PlayHatchAnimation()
	{
		animator.SetTrigger(AnimatorHashID.hatch_open);
	}

	public void SetState(bool state)
	{
		animator.SetBool(AnimatorHashID.reactor_on, state);
		if (state)
		{
			text.text = string.Format("<color=#00ff00>{0}</color>", Language.main.Get("BaseBioReactorActive"));
			if (!liquidAnim.reverse)
			{
				liquidAnim.duration = 15f;
				liquidAnim.reverse = state;
				liquidAnim.Play();
				fxControl.Play(0);
				lightsControl.LerpToState(0, 15f);
			}
		}
		else
		{
			text.text = string.Format("<color=#ff0000>{0}</color>", Language.main.Get("BaseBioReactorInactive"));
			if (liquidAnim.reverse)
			{
				liquidAnim.duration = 10f;
				liquidAnim.reverse = state;
				liquidAnim.Play();
				fxControl.StopAndDestroy(0, 2f);
				lightsControl.LerpToState(1, 8f);
			}
		}
	}

	public void OnHover(HandTargetEventData eventData)
	{
		BaseBioReactor module = GetModule();
		if (module != null)
		{
			module.OnHover();
		}
	}

	public void OnUse(HandTargetEventData eventData)
	{
		BaseBioReactor module = GetModule();
		if (module != null)
		{
			module.OnUse(this);
		}
	}

	private BaseBioReactor GetModule()
	{
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			IBaseModule module = componentInParent.GetModule(geometryFace);
			if (module != null)
			{
				return module as BaseBioReactor;
			}
		}
		return null;
	}

	private void InitState()
	{
		BaseBioReactor module = GetModule();
		SetState(module != null && module.producingPower);
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		reason = null;
		return true;
	}
}
