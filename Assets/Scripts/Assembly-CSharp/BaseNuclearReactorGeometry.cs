using TMPro;
using UnityEngine;

public class BaseNuclearReactorGeometry : MonoBehaviour, IBaseModuleGeometry, IObstacle
{
	public Transform storagePivot;

	public TextMeshProUGUI text;

	[AssertNotNull]
	public FMOD_CustomEmitter workSound;

	[AssertLocalization]
	private const string reactorActiveMessage = "BaseNuclearReactorActive";

	[AssertLocalization]
	private const string reactorInactiveMessage = "BaseNuclearReactorInactive";

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

	private void Awake()
	{
	}

	private void Start()
	{
		InitState();
	}

	public void SetState(bool state)
	{
		if (state)
		{
			text.text = string.Format("<color=#00ff00>{0}</color>", Language.main.Get("BaseNuclearReactorActive"));
			workSound.Play();
		}
		else
		{
			text.text = string.Format("<color=#ff0000>{0}</color>", Language.main.Get("BaseNuclearReactorInactive"));
			workSound.Stop();
		}
	}

	public void OnHover(HandTargetEventData eventData)
	{
		BaseNuclearReactor module = GetModule();
		if (module != null)
		{
			module.OnHover();
		}
	}

	public void OnUse(HandTargetEventData eventData)
	{
		BaseNuclearReactor module = GetModule();
		if (module != null)
		{
			module.OnUse(this);
		}
	}

	private BaseNuclearReactor GetModule()
	{
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			IBaseModule module = componentInParent.GetModule(geometryFace);
			if (module != null)
			{
				return module as BaseNuclearReactor;
			}
		}
		return null;
	}

	private void InitState()
	{
		BaseNuclearReactor module = GetModule();
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
