using System.Collections;
using System.Collections.Generic;
using System.Text;
using UWE;
using UnityEngine;

public class BuilderTool : PlayerTool
{
	private const float hitRange = 30f;

	[AssertLocalization(1)]
	private const string constructFormat = "ConstructFormat";

	[AssertLocalization(1)]
	private const string deconstructFormat = "DeconstructFormat";

	[AssertLocalization(2)]
	private const string constructDeconstructFormat = "ConstructDeconstructFormat";

	[AssertLocalization]
	private const string noPowerKey = "NoPower";

	[AssertLocalization(2)]
	private const string requireMultipleFormat = "RequireMultipleFormat";

	[AssertLocalization(1)]
	private const string builderUse = "BuilderUseFormat";

	[AssertLocalization(2)]
	private const string builderConstructCancel = "BuilderWithGhostFormat";

	private const float deconstructRange = 11f;

	public float powerConsumptionConstruct = 0.5f;

	public float powerConsumptionDeconstruct = 0.5f;

	public Renderer bar;

	public int barMaterialID = 1;

	public Transform nozzleLeft;

	public Transform nozzleRight;

	public Transform beamLeft;

	public Transform beamRight;

	public float nozzleRotationSpeed = 10f;

	[Range(0.01f, 5f)]
	public float pointSwitchTimeMin = 0.1f;

	[Range(0.01f, 5f)]
	public float pointSwitchTimeMax = 1f;

	public Animator animator;

	public FMOD_CustomLoopingEmitter buildSound;

	[AssertNotNull]
	public FMODAsset completeSound;

	private bool isConstructing;

	private Constructable constructable;

	private int handleInputFrame = -1;

	private Vector3 leftPoint = Vector3.zero;

	private Vector3 rightPoint = Vector3.zero;

	private float leftConstructionTime;

	private float rightConstructionTime;

	private float leftConstructionInterval;

	private float rightConstructionInterval;

	private Vector3 leftConstructionPoint;

	private Vector3 rightConstructionPoint;

	private string customUseText;

	private bool wasPlacing;

	private bool wasPlacingRotatable;

	private Material barMaterial;

	private void Start()
	{
		if (barMaterial == null)
		{
			barMaterial = bar.materials[barMaterialID];
		}
		SetBeamActive(state: false);
	}

	private void OnDisable()
	{
		buildSound.Stop();
	}

	private void Update()
	{
		HandleInput();
	}

	private void LateUpdate()
	{
		Quaternion b = Quaternion.identity;
		Quaternion b2 = Quaternion.identity;
		bool flag = constructable != null;
		if (isConstructing != flag)
		{
			isConstructing = flag;
			if (isConstructing)
			{
				leftConstructionInterval = Random.Range(pointSwitchTimeMin, pointSwitchTimeMax);
				rightConstructionInterval = Random.Range(pointSwitchTimeMin, pointSwitchTimeMax);
				leftConstructionPoint = constructable.GetRandomConstructionPoint();
				rightConstructionPoint = constructable.GetRandomConstructionPoint();
			}
			else
			{
				leftConstructionTime = 0f;
				rightConstructionTime = 0f;
			}
		}
		else if (isConstructing)
		{
			leftConstructionTime += Time.deltaTime;
			rightConstructionTime += Time.deltaTime;
			if (leftConstructionTime >= leftConstructionInterval)
			{
				leftConstructionTime %= leftConstructionInterval;
				leftConstructionInterval = Random.Range(pointSwitchTimeMin, pointSwitchTimeMax);
				leftConstructionPoint = constructable.GetRandomConstructionPoint();
			}
			if (rightConstructionTime >= rightConstructionInterval)
			{
				rightConstructionTime %= rightConstructionInterval;
				rightConstructionInterval = Random.Range(pointSwitchTimeMin, pointSwitchTimeMax);
				rightConstructionPoint = constructable.GetRandomConstructionPoint();
			}
			leftPoint = nozzleLeft.parent.InverseTransformPoint(leftConstructionPoint);
			rightPoint = nozzleRight.parent.InverseTransformPoint(rightConstructionPoint);
			Debug.DrawLine(nozzleLeft.position, leftConstructionPoint, Color.white);
			Debug.DrawLine(nozzleRight.position, rightConstructionPoint, Color.white);
		}
		if (isConstructing)
		{
			b = Quaternion.LookRotation(leftPoint, Vector3.up);
			b2 = Quaternion.LookRotation(rightPoint, Vector3.up);
			Vector3 localScale = beamLeft.localScale;
			localScale.z = leftPoint.magnitude;
			beamLeft.localScale = localScale;
			localScale = beamRight.localScale;
			localScale.z = rightPoint.magnitude;
			beamRight.localScale = localScale;
			Debug.DrawLine(nozzleLeft.position, leftConstructionPoint, Color.white);
			Debug.DrawLine(nozzleRight.position, rightConstructionPoint, Color.white);
		}
		float t = nozzleRotationSpeed * Time.deltaTime;
		nozzleLeft.localRotation = Quaternion.Slerp(nozzleLeft.localRotation, b, t);
		nozzleRight.localRotation = Quaternion.Slerp(nozzleRight.localRotation, b2, t);
		SetBeamActive(isConstructing);
		SetUsingAnimation(isConstructing);
		if (isConstructing)
		{
			buildSound.Play();
		}
		else
		{
			buildSound.Stop();
		}
		UpdateBar();
		constructable = null;
	}

	private void HandleInput()
	{
		if (handleInputFrame == Time.frameCount)
		{
			return;
		}
		handleInputFrame = Time.frameCount;
		if (!base.isDrawn || Builder.isPlacing || !AvatarInputHandler.main.IsEnabled() || TryDisplayNoPowerTooltip())
		{
			return;
		}
		Targeting.AddToIgnoreList(Player.main.gameObject);
		Targeting.GetTarget(30f, out var result, out var distance);
		if (result == null)
		{
			return;
		}
		bool buttonHeld = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
		bool buttonDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
		bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
		Constructable constructable = result.GetComponentInParent<Constructable>();
		if (constructable != null && distance > constructable.placeMaxDistance)
		{
			constructable = null;
		}
		string reason;
		if (constructable != null)
		{
			OnHover(constructable);
			if (buttonHeld)
			{
				Construct(constructable, state: true);
			}
			else if (constructable.DeconstructionAllowed(out reason))
			{
				if (buttonHeld2)
				{
					if (constructable.constructed)
					{
						Builder.ResetLast();
						constructable.SetState(value: false, setAmount: false);
					}
					else
					{
						Construct(constructable, state: false, buttonDown);
					}
				}
			}
			else if (buttonDown && !string.IsNullOrEmpty(reason))
			{
				ErrorMessage.AddMessage(reason);
			}
			return;
		}
		BaseDeconstructable baseDeconstructable = result.GetComponentInParent<BaseDeconstructable>();
		if (baseDeconstructable == null)
		{
			BaseExplicitFace componentInParent = result.GetComponentInParent<BaseExplicitFace>();
			if (componentInParent != null)
			{
				baseDeconstructable = componentInParent.parent;
			}
		}
		if (!(baseDeconstructable != null) || !(distance <= 11f))
		{
			return;
		}
		if (baseDeconstructable.DeconstructionAllowed(out reason))
		{
			OnHover(baseDeconstructable);
			if (buttonDown)
			{
				Builder.ResetLast();
				baseDeconstructable.Deconstruct();
			}
		}
		else if (buttonDown && !string.IsNullOrEmpty(reason))
		{
			ErrorMessage.AddMessage(reason);
		}
	}

	private bool TryDisplayNoPowerTooltip()
	{
		if (!HasEnergyOrInBase())
		{
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, "NoPower", translate: true);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			main.SetIcon(HandReticle.IconType.Default);
			return true;
		}
		return false;
	}

	public override bool OnRightHandDown()
	{
		if (Player.main.IsBleederAttached())
		{
			return true;
		}
		if (!HasEnergyOrInBase())
		{
			return false;
		}
		uGUI_BuilderMenu.Show();
		return true;
	}

	private bool HasEnergyOrInBase()
	{
		if (Player.main.IsInSub())
		{
			return true;
		}
		return energyMixin.charge > 0f;
	}

	public override bool OnLeftHandDown()
	{
		HandleInput();
		return isConstructing;
	}

	public override bool OnLeftHandHeld()
	{
		HandleInput();
		return isConstructing;
	}

	public override bool OnLeftHandUp()
	{
		HandleInput();
		return isConstructing;
	}

	private void Construct(Constructable c, bool state, bool start = false)
	{
		if (c != null && !c.constructed && HasEnergyOrInBase())
		{
			CoroutineHost.StartCoroutine(ConstructAsync(c, state, start));
		}
	}

	private IEnumerator ConstructAsync(Constructable c, bool state, bool start)
	{
		float amount = (state ? powerConsumptionConstruct : powerConsumptionDeconstruct) * Time.deltaTime;
		energyMixin.ConsumeEnergy(amount);
		bool wasConstructed = c.constructed;
		bool flag;
		if (state)
		{
			flag = c.Construct();
			if (!flag && !wasConstructed)
			{
				Utils.PlayFMODAsset(completeSound, c.transform);
			}
		}
		else
		{
			TaskResult<bool> result = new TaskResult<bool>();
			TaskResult<string> resultReason = new TaskResult<string>();
			_ = c.transform.position;
			yield return c.DeconstructAsync(result, resultReason);
			flag = result.Get();
			_ = c.constructedAmount;
			_ = 0f;
			if (!flag && (start || isConstructing))
			{
				string text = resultReason.Get();
				if (!string.IsNullOrEmpty(text))
				{
					ErrorMessage.AddError(text);
				}
			}
		}
		bool flag2 = usingPlayer != null;
		if (flag && flag2)
		{
			constructable = c;
		}
		if (!wasConstructed && c.constructed)
		{
			TechType techType = c.techType;
			Vector3 constructedPosition = c.transform.position;
			if (flag2 && Builder.lastTechType != 0 && c.techType == Builder.lastTechType)
			{
				yield return Builder.BeginAsync(Builder.lastTechType);
			}
			CraftingAnalytics.main.OnConstruct(techType, constructedPosition);
		}
	}

	private void OnHover(Constructable constructable)
	{
		if (constructable.constructed && !constructable.deconstructionAllowed)
		{
			return;
		}
		HandReticle main = HandReticle.main;
		string buttonFormat = LanguageCache.GetButtonFormat("DeconstructFormat", GameInput.Button.Deconstruct);
		if (constructable.constructed)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.Get(constructable.techType), translate: false);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, buttonFormat, translate: false);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(Language.main.GetFormat("ConstructDeconstructFormat", LanguageCache.GetButtonFormat("ConstructFormat", GameInput.Button.LeftHand), buttonFormat));
		foreach (KeyValuePair<TechType, int> remainingResource in constructable.GetRemainingResources())
		{
			TechType key = remainingResource.Key;
			string text = Language.main.Get(key);
			int value = remainingResource.Value;
			if (value > 1)
			{
				stringBuilder.AppendLine(Language.main.GetFormat("RequireMultipleFormat", text, value));
			}
			else
			{
				stringBuilder.AppendLine(text);
			}
		}
		main.SetText(HandReticle.TextType.Hand, Language.main.Get(constructable.techType), translate: false);
		main.SetText(HandReticle.TextType.HandSubscript, stringBuilder.ToString(), translate: false);
		main.SetProgress(constructable.amount);
		main.SetIcon(HandReticle.IconType.Progress, 1.5f);
	}

	private void OnHover(BaseDeconstructable deconstructable)
	{
		HandReticle main = HandReticle.main;
		main.SetText(HandReticle.TextType.Hand, deconstructable.Name, translate: true);
		main.SetText(HandReticle.TextType.HandSubscript, LanguageCache.GetButtonFormat("DeconstructFormat", GameInput.Button.Deconstruct), translate: false);
	}

	public override bool GetUsedToolThisFrame()
	{
		return isConstructing;
	}

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
		wasPlacing = false;
		wasPlacingRotatable = false;
		UpdateCustomUseText();
		GameInput.OnBindingsChanged += UpdateCustomUseText;
	}

	public override void OnHolster()
	{
		base.OnHolster();
		if (uGUI_BuilderMenu.IsOpen())
		{
			uGUI_BuilderMenu.Hide();
		}
		Builder.End();
		SetBeamActive(state: false);
		GameInput.OnBindingsChanged -= UpdateCustomUseText;
	}

	public override string GetCustomUseText()
	{
		bool isPlacing = Builder.isPlacing;
		bool flag = isPlacing && Builder.canRotate;
		if (isPlacing != wasPlacing || flag != wasPlacingRotatable)
		{
			UpdateCustomUseText();
			wasPlacing = isPlacing;
			wasPlacingRotatable = flag;
		}
		return customUseText;
	}

	private void UpdateCustomUseText()
	{
		if (Builder.isPlacing)
		{
			customUseText = Language.main.GetFormat("BuilderWithGhostFormat", GameInput.FormatButton(GameInput.Button.LeftHand), GameInput.FormatButton(GameInput.Button.RightHand));
			if (Builder.canRotate)
			{
				string format = Language.main.GetFormat("GhostRotateInputHint", GameInput.FormatButton(GameInput.Button.CyclePrev, allBindingSets: true), GameInput.FormatButton(GameInput.Button.CycleNext, allBindingSets: true));
				customUseText = $"{customUseText}\n{format}";
			}
		}
		else
		{
			customUseText = LanguageCache.GetButtonFormat("BuilderUseFormat", GameInput.Button.RightHand);
		}
	}

	private void UpdateBar()
	{
		if (!(bar == null))
		{
			float value = ((energyMixin.capacity > 0f) ? (energyMixin.charge / energyMixin.capacity) : 0f);
			barMaterial.SetFloat(ShaderPropertyID._Amount, value);
		}
	}

	private void SetBeamActive(bool state)
	{
		if (beamLeft != null)
		{
			beamLeft.gameObject.SetActive(state);
		}
		if (beamRight != null)
		{
			beamRight.gameObject.SetActive(state);
		}
	}

	private void SetUsingAnimation(bool state)
	{
		if (!(animator == null) && animator.isActiveAndEnabled)
		{
			SafeAnimator.SetBool(animator, "using_tool", state);
		}
	}
}
