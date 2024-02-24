using System.Collections;
using RootMotion.FinalIK;
using UWE;
using UnityEngine;

[RequireComponent(typeof(ConditionRules))]
[RequireComponent(typeof(Animator))]
public class ArmsController : MonoBehaviour
{
	private class ArmAiming
	{
		public AimIK aimer;

		public bool shouldAim;

		private float weightVelocity;

		public void FindAimer(GameObject ikObj, Transform aimedXform)
		{
			AimIK[] componentsInChildren = ikObj.GetComponentsInChildren<AimIK>();
			foreach (AimIK aimIK in componentsInChildren)
			{
				if (aimIK.solver.transform == aimedXform)
				{
					if (aimer != null)
					{
						Debug.LogError("Found multiple AimIK components on " + ikObj.GetFullHierarchyPath() + " that manipulate the transform " + aimedXform.gameObject.GetFullHierarchyPath());
					}
					else
					{
						aimer = aimIK;
					}
				}
			}
			if (aimer == null)
			{
				Debug.LogError("Could not find AimIK comp on " + ikObj.GetFullHierarchyPath() + " for transform " + aimedXform.gameObject.GetFullHierarchyPath());
			}
		}

		public void Update(float smoothTime)
		{
			if (!(aimer == null))
			{
				aimer.solver.IKPositionWeight = Mathf.SmoothDamp(aimer.solver.IKPositionWeight, shouldAim ? 1f : 0f, ref weightVelocity, smoothTime);
			}
		}
	}

	public float smoothSpeedUnderWater = 4f;

	public float smoothSpeedAboveWater = 8f;

	public float turnAnimationDampTime;

	[AssertNotNull]
	public Transform leftAimIKTransform;

	[AssertNotNull]
	public Transform rightAimIKTransform;

	[AssertNotNull]
	public Transform lookTargetTransform;

	[AssertNotNull]
	public Transform attachedLeftHandTarget;

	[AssertNotNull]
	public Transform leftHandElbow;

	[AssertNotNull]
	public BleederAttachTarget bleederAttackTarget;

	public float ikToggleTime = 0.5f;

	[AssertNotNull]
	public Transform leftHandAttach;

	[AssertNotNull]
	public Transform rightHandAttach;

	[AssertNotNull]
	public Transform leftHand;

	[AssertNotNull]
	public Transform rightHand;

	private Animator animator;

	private Player player;

	private GUIHand guiHand;

	private Vector3 smoothedVelocity = Vector3.zero;

	private float previousYAngle;

	private PDA pda;

	private Vector3 lookTargetResetPos;

	private InspectOnFirstPickup inspectObject;

	private int restoreQuickSlot = -1;

	private string inspectObjectParam;

	private bool inspecting;

	private ArmAiming leftAim = new ArmAiming();

	private ArmAiming rightAim = new ArmAiming();

	private PlayerTool lastTool;

	private FullBodyBipedIK ik;

	private bool isPuttingAwayTool;

	private Vector3[] previousVelocities = new Vector3[2];

	private Vector3 prevPosition;

	private float diveObstaclesScanInterval = 0.5f;

	private bool obstaclesBelow = true;

	private float nextDiveObstaclesScanTime;

	private Transform leftWorldTarget;

	private Transform rightWorldTarget;

	private bool reconfigureWorldTarget;

	private bool wasBleederAttached;

	private bool wasPdaInUse;

	private void Start()
	{
		animator = base.gameObject.GetComponent<Animator>();
		player = Utils.GetLocalPlayerComp();
		guiHand = player.guiHand;
		InstallAnimationRules();
		leftAim.FindAimer(base.gameObject, leftAimIKTransform);
		rightAim.FindAimer(base.gameObject, rightAimIKTransform);
		ik = GetComponent<FullBodyBipedIK>();
		bleederAttackTarget = GetComponentInChildren<BleederAttachTarget>();
		lookTargetResetPos = lookTargetTransform.transform.localPosition;
		pda = player.GetPDA();
	}

	public void StartInspectObjectAsync(InspectOnFirstPickup obj)
	{
		StartCoroutine(InspectObjectAsync(obj));
	}

	public float StartHolsterTime(float time)
	{
		InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
		int activeSlot = Inventory.main.quickSlots.activeSlot;
		PlayerTool playerTool = ((heldItem != null && heldItem.item != null) ? heldItem.item.GetComponent<PlayerTool>() : null);
		float result = 0f;
		if ((bool)playerTool)
		{
			time += playerTool.holsterTime;
			Inventory.main.ReturnHeld(immediate: false);
			result = playerTool.holsterTime;
		}
		pda.Close();
		Inventory.main.quickSlots.SetIgnoreHotkeyInput(ignore: true);
		Player.main.GetPDA().SetIgnorePDAInput(ignore: true);
		StartCoroutine(HolsterTimeAsync(playerTool ? activeSlot : (-1), time));
		return result;
	}

	private IEnumerator HolsterTimeAsync(int quickslot, float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		if (quickslot != -1)
		{
			Inventory.main.quickSlots.Select(quickslot);
		}
		Inventory.main.quickSlots.SetIgnoreHotkeyInput(ignore: false);
		Player.main.GetPDA().SetIgnorePDAInput(ignore: false);
	}

	public IEnumerator InspectObjectAsync(InspectOnFirstPickup obj)
	{
		if (inspecting)
		{
			yield break;
		}
		restoreQuickSlot = (obj.restoreQuickSlot ? Inventory.main.quickSlots.activeSlot : (-1));
		InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
		float holsterTime = 0f;
		float inspectDuration = obj.inspectDuration;
		PlayerTool playerTool = ((heldItem != null && heldItem.item != null) ? heldItem.item.GetComponent<PlayerTool>() : null);
		if ((bool)playerTool)
		{
			holsterTime = playerTool.holsterTime;
		}
		inspectObject = obj;
		if (Inventory.main.ReturnHeld())
		{
			inspecting = true;
			Inventory.main.quickSlots.SetIgnoreHotkeyInput(ignore: true);
			Player.main.GetPDA().SetIgnorePDAInput(ignore: true);
			inspectObjectParam = (string.IsNullOrEmpty(inspectObject.animParam) ? ("holding_" + inspectObject.pickupAble.GetTechType().AsString(lowercase: true)) : inspectObject.animParam);
			yield return null;
			yield return new WaitForSeconds(holsterTime);
			inspectObject.OnInspectObjectBegin();
			animator.SetBool(inspectObjectParam, value: true);
			animator.SetBool("using_tool_first", value: true);
			yield return new WaitForSeconds(inspectDuration);
			Inventory.main.quickSlots.SetIgnoreHotkeyInput(ignore: false);
			Player.main.GetPDA().SetIgnorePDAInput(ignore: false);
			animator.SetBool(inspectObjectParam, value: false);
			animator.SetBool("using_tool_first", value: false);
			inspectObject.gameObject.SetActive(value: false);
			inspectObject.OnInspectObjectDone();
			if (restoreQuickSlot != -1)
			{
				Inventory.main.quickSlots.Select(restoreQuickSlot);
			}
			inspectObject = null;
			inspectObjectParam = null;
			restoreQuickSlot = -1;
			inspecting = false;
		}
		else
		{
			inspectObject = null;
			restoreQuickSlot = -1;
		}
	}

	private void OnInspectObjectDone()
	{
	}

	public void ResetLookTargetToInitialPos()
	{
		lookTargetTransform.localPosition = lookTargetResetPos;
	}

	private void InstallAnimationRules()
	{
		GetComponent<ConditionRules>().AddCondition(() => Inventory.main.GetHeldTool() as Welder != null).WhenChanges(delegate(bool newValue)
		{
			SafeAnimator.SetBool(animator, "holding_welder", newValue);
		});
		GetComponent<ConditionRules>().AddCondition(delegate
		{
			float y = player.gameObject.transform.position.y;
			return y > player.GetWaterLevel() - 1f && y < player.GetWaterLevel() + 1f && !Player.main.IsInside() && Player.main.IsUnderwaterForSwimming();
		}).WhenChanges(delegate(bool newValue)
		{
			SafeAnimator.SetBool(animator, "on_surface", newValue);
		});
		GetComponent<ConditionRules>().AddCondition(() => player.GetInMechMode()).WhenChanges(delegate(bool newValue)
		{
			SafeAnimator.SetBool(animator, "using_mechsuit", newValue);
		});
	}

	public Vector3 GetSmoothedVelocity()
	{
		return smoothedVelocity;
	}

	private Vector3 GetRelativeVelocity()
	{
		Vector3 velocity = player.gameObject.GetComponent<PlayerController>().velocity;
		Transform aimingTransform = player.camRoot.GetAimingTransform();
		Vector3 result = Vector3.zero;
		if (player.IsUnderwater() || !player.groundMotor.IsGrounded())
		{
			result = aimingTransform.InverseTransformDirection(velocity);
		}
		else
		{
			Vector3 forward = aimingTransform.forward;
			forward.y = 0f;
			forward.Normalize();
			result.z = Vector3.Dot(forward, velocity);
			Vector3 right = aimingTransform.right;
			right.y = 0f;
			right.Normalize();
			result.x = Vector3.Dot(right, velocity);
		}
		return result;
	}

	private void SetPlayerSpeedParameters()
	{
		Vector3 relativeVelocity = GetRelativeVelocity();
		float num = (Player.main.IsUnderwater() ? smoothSpeedUnderWater : smoothSpeedAboveWater);
		smoothedVelocity = Vector3.Slerp(smoothedVelocity, relativeVelocity, num * Time.deltaTime);
		animator.SetFloat(AnimatorHashID.move_speed, smoothedVelocity.magnitude);
		animator.SetFloat(AnimatorHashID.move_speed_x, smoothedVelocity.x);
		animator.SetFloat(AnimatorHashID.move_speed_y, smoothedVelocity.y);
		animator.SetFloat(AnimatorHashID.move_speed_z, smoothedVelocity.z);
		MainCameraControl main = MainCameraControl.main;
		SafeAnimator.SetFloat(animator, "view_pitch", main.GetCameraPitch());
		Transform viewModel = main.viewModel;
		float deltaTime = Time.deltaTime;
		if (viewModel != null && deltaTime > 0f)
		{
			float y = viewModel.eulerAngles.y;
			float value = Mathf.DeltaAngle(previousYAngle, y) / deltaTime;
			animator.SetFloat("view_turn", value, turnAnimationDampTime, deltaTime);
			previousYAngle = y;
		}
	}

	public void TriggerAnimParam(string paramName, float duration = 0f)
	{
		animator.SetBool(paramName, value: true);
		StartSetAnimParam(paramName, duration);
	}

	private void StartSetAnimParam(string paramName, float duration)
	{
		StartCoroutine(SetAnimParamAsync(paramName, value: false, duration));
	}

	private IEnumerator SetAnimParamAsync(string paramName, bool value, float duration)
	{
		yield return new WaitForSeconds(duration);
		animator.SetBool(paramName, value);
	}

	private void Reconfigure(PlayerTool tool)
	{
		ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = leftHandElbow;
		ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 1f;
		if (tool == null)
		{
			leftAim.shouldAim = false;
			rightAim.shouldAim = false;
			ik.solver.leftHandEffector.target = null;
			ik.solver.rightHandEffector.target = null;
			if (!pda.isActiveAndEnabled)
			{
				if ((bool)leftWorldTarget)
				{
					ik.solver.leftHandEffector.target = leftWorldTarget;
					ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = null;
					ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 0f;
				}
				if ((bool)rightWorldTarget)
				{
					ik.solver.rightHandEffector.target = rightWorldTarget;
				}
			}
			return;
		}
		if (!IsBleederAttached())
		{
			leftAim.shouldAim = tool.ikAimLeftArm;
			if (tool.useLeftAimTargetOnPlayer)
			{
				ik.solver.leftHandEffector.target = attachedLeftHandTarget;
			}
			else
			{
				ik.solver.leftHandEffector.target = tool.leftHandIKTarget;
			}
		}
		else
		{
			leftAim.shouldAim = tool.ikAimRightArm;
			ik.solver.leftHandEffector.target = null;
		}
		rightAim.shouldAim = tool.ikAimRightArm;
		ik.solver.rightHandEffector.target = tool.rightHandIKTarget;
	}

	private void UpdateHandIKWeights()
	{
		float positionWeight = ik.solver.rightHandEffector.positionWeight;
		float num = ((ik.solver.rightHandEffector.target != null) ? 1f : 0f);
		float positionWeight2 = ik.solver.leftHandEffector.positionWeight;
		float num2 = ((ik.solver.leftHandEffector.target != null) ? 1f : 0f);
		if (ikToggleTime == 0f)
		{
			positionWeight = num;
			positionWeight2 = num2;
		}
		else
		{
			positionWeight = Mathf.MoveTowards(positionWeight, num, Time.deltaTime / ikToggleTime);
			positionWeight2 = Mathf.MoveTowards(positionWeight2, num2, Time.deltaTime / ikToggleTime);
		}
		ik.solver.rightHandEffector.positionWeight = positionWeight;
		ik.solver.rightHandEffector.rotationWeight = positionWeight;
		ik.solver.leftHandEffector.positionWeight = positionWeight2;
		ik.solver.leftHandEffector.rotationWeight = positionWeight2;
		if (positionWeight2 > 0f || positionWeight > 0f)
		{
			ik.solver.IKPositionWeight = 1f;
		}
		else
		{
			ik.solver.IKPositionWeight = 0f;
		}
	}

	public bool IsBleederAttached()
	{
		return bleederAttackTarget.attached;
	}

	private void UpdateDiving()
	{
		bool flag = !Player.main.precursorOutOfWater && player.GetMode() == Player.Mode.Normal && !player.IsUnderwater() && !player.playerController.activeController.grounded && player.playerController.velocity.y < 0f;
		if (flag && Time.time >= nextDiveObstaclesScanTime)
		{
			nextDiveObstaclesScanTime = Time.time + diveObstaclesScanInterval;
			Vector3 direction = player.playerController.velocity.normalized + Vector3.down;
			int layerMask = ~(1 << LayerMask.NameToLayer("Player"));
			float maxDistance = 5f;
			if (player.transform.position.y > 0f)
			{
				maxDistance = (player.transform.position.y + 1.5f) / Vector3.Dot(direction.normalized, Vector3.down);
			}
			obstaclesBelow = Physics.Raycast(player.transform.position, direction, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
		}
		else
		{
			obstaclesBelow = obstaclesBelow || !flag;
		}
		SafeAnimator.SetBool(animator, "diving", flag && !obstaclesBelow);
		SafeAnimator.SetBool(animator, "diving_land", flag && obstaclesBelow);
	}

	public void SetWorldIKTarget(Transform leftTarget, Transform rightTarget)
	{
		leftWorldTarget = leftTarget;
		rightWorldTarget = rightTarget;
		reconfigureWorldTarget = true;
	}

	private void Update()
	{
		bool flag = IsBleederAttached();
		SetPlayerSpeedParameters();
		bool value = player.timeGrabbed != 0f && (double)player.timeGrabbed + 0.4 > (double)Time.time;
		SafeAnimator.SetBool(animator, "grab", value);
		bool value2 = player.timeBashed != 0f && (double)player.timeBashed + 0.4 > (double)Time.time;
		SafeAnimator.SetBool(animator, "bash", value2);
		SafeAnimator.SetBool(animator, "is_underwater", player.IsUnderwater() && Player.main.motorMode != Player.MotorMode.Vehicle);
		SafeAnimator.SetBool(animator, "cinematics_enabled", !GameOptions.GetVrAnimationMode());
		PlayerTool playerTool = guiHand.GetTool();
		if (player.GetPDA().isInUse)
		{
			playerTool = null;
		}
		if (reconfigureWorldTarget || playerTool != lastTool || flag != wasBleederAttached || (playerTool != null && playerTool.PollForceConfigureIK()) || pda.isActiveAndEnabled != wasPdaInUse)
		{
			Reconfigure(playerTool);
		}
		lastTool = playerTool;
		wasPdaInUse = pda.isActiveAndEnabled;
		leftAim.Update(ikToggleTime);
		rightAim.Update(ikToggleTime);
		UpdateHandIKWeights();
		Player.main.IsUnderwater();
		if (bleederAttackTarget.attached)
		{
			SafeAnimator.SetBool(animator, "using_tool", GameInput.GetButtonHeld(GameInput.Button.RightHand));
			if (GameInput.GetButtonHeld(GameInput.Button.RightHand))
			{
				GoalManager.main.OnCustomGoalEvent("AttackBleeder");
			}
		}
		else
		{
			SafeAnimator.SetBool(animator, "using_tool", guiHand.GetUsingTool());
			SafeAnimator.SetBool(animator, "using_tool_alt", guiHand.GetAltAttacking());
		}
		SafeAnimator.SetBool(animator, "holding_tool", playerTool != null);
		Inventory.main.GetHeldTool();
		SafeAnimator.SetBool(animator, "in_seamoth", Player.main.inSeamoth);
		SafeAnimator.SetBool(animator, "in_exosuit", Player.main.inExosuit);
		SafeAnimator.SetBool(animator, "cyclops_steering", Player.main.GetMode() == Player.Mode.Piloting);
		SafeAnimator.SetBool(animator, "bleeder", bleederAttackTarget.attached);
		SafeAnimator.SetBool(animator, "jump", player.GetPlayFallingAnimation());
		SafeAnimator.SetFloat(animator, "verticalOffset", MainCameraControl.main.GetImpactBob());
		wasBleederAttached = flag;
		UpdateDiving();
	}

	public bool IsInAnimationState(string layer, string state)
	{
		int num = Animator.StringToHash(layer + "." + state);
		return animator.GetCurrentAnimatorStateInfo(0).nameHash == num;
	}

	public void OnToolBleederHitAnim(AnimationEvent e)
	{
		if ((bool)bleederAttackTarget.bleeder)
		{
			GUIHand component = player.gameObject.GetComponent<GUIHand>();
			bleederAttackTarget.bleeder.OnHit(component.GetTool());
			component.OnToolBleederHitAnim();
		}
	}

	public void OnToolUseAnim(AnimationEvent e)
	{
		player.gameObject.GetComponent<GUIHand>().OnToolUseAnim();
	}

	public void OnToolReloadBeginAnim(AnimationEvent e)
	{
	}

	public void OnToolReloadEndAnim(AnimationEvent e)
	{
	}

	public void OnToolReloadAnim(AnimationEvent e)
	{
	}

	public void OnSwimFastStartAnim(AnimationEvent e)
	{
		Utils.GetLocalPlayerComp().OnSwimFastStartAnim(e);
	}

	public void OnSwimFastEndAnim(AnimationEvent e)
	{
		Utils.GetLocalPlayerComp().OnSwimFastEndAnim(e);
	}

	private void FireExSpray(AnimationEvent e)
	{
		player.gameObject.GetComponent<GUIHand>().FireExSpray();
	}

	public void BashHit(AnimationEvent e)
	{
		player.gameObject.GetComponent<GUIHand>().BashHit();
	}

	public void SetUsingBuilder(bool isUsingBuilder)
	{
		if ((bool)animator)
		{
			SafeAnimator.SetBool(animator, "using_builder", isUsingBuilder);
		}
	}

	private void spawn_pda()
	{
	}

	private void kill_pda()
	{
	}

	public void OnToolAnimDraw(AnimationEvent e)
	{
		player.gameObject.GetComponent<GUIHand>().OnToolAnimDraw();
	}

	public void OnToolAnimHolster(AnimationEvent e)
	{
		player.gameObject.GetComponent<GUIHand>().OnToolAnimHolster();
	}

	public void SetUsingPda(bool isUsing)
	{
		SafeAnimator.SetBool(animator, "using_pda", isUsing);
	}
}
