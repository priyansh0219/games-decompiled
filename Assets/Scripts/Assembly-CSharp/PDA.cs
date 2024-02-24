using FMOD.Studio;
using UWE;
using UnityEngine;

public class PDA : MonoBehaviour
{
	public enum State
	{
		Opened = 0,
		Closed = 1,
		Opening = 2,
		Closing = 3
	}

	public delegate void OnClose(PDA pda);

	private const float timeDraw = 0.5f;

	private const float timeHolster = 0.3f;

	public const string pauseId = "PDA";

	[AssertNotNull]
	public GameObject prefabScreen;

	[AssertNotNull]
	public Transform screenAnchor;

	private Sequence sequence = new Sequence(initialState: false);

	private int prevQuickSlot = -1;

	private bool targetWasSet;

	private Transform target;

	private OnClose onCloseCallback;

	private float activeSqrDistance;

	private bool ignorePDAInput;

	private uGUI_PDA _ui;

	private EventInstance audioSnapshotInstance;

	public bool isInUse { get; private set; }

	public bool isFocused
	{
		get
		{
			if (ui != null)
			{
				return ui.focused;
			}
			return false;
		}
	}

	public bool isOpen => state == State.Opened;

	public State state
	{
		get
		{
			if (sequence.target)
			{
				if (!sequence.active)
				{
					return State.Opened;
				}
				return State.Opening;
			}
			if (!base.gameObject.activeInHierarchy || !sequence.active)
			{
				return State.Closed;
			}
			return State.Closing;
		}
	}

	public uGUI_PDA ui
	{
		get
		{
			if (_ui == null)
			{
				GameObject gameObject = Object.Instantiate(prefabScreen);
				_ui = gameObject.GetComponent<uGUI_PDA>();
				gameObject.GetComponent<uGUI_CanvasScaler>().SetAnchor(screenAnchor);
				_ui.Initialize();
			}
			return _ui;
		}
	}

	public static float time { get; private set; }

	public static float deltaTime { get; private set; }

	public static float GetDeltaTime()
	{
		return deltaTime;
	}

	public void SetIgnorePDAInput(bool ignore)
	{
		ignorePDAInput = ignore;
	}

	public static void PerformUpdate()
	{
		Player main = Player.main;
		PDA pDA = ((main != null) ? main.GetPDA() : null);
		if (pDA != null && pDA.isActiveAndEnabled)
		{
			pDA.ManagedUpdate();
		}
		else
		{
			UpdateTime(pausedByPDA: false);
		}
	}

	private static void UpdateTime(bool pausedByPDA)
	{
		deltaTime = (pausedByPDA ? Time.unscaledDeltaTime : Time.deltaTime);
		time += deltaTime;
		Shader.SetGlobalFloat(ShaderPropertyID._PDATime, time);
	}

	public static void Deinitialize()
	{
		time = 0f;
	}

	private void ManagedUpdate()
	{
		bool flag = MiscSettings.pdaPause && (state == State.Opened || state == State.Opening || state == State.Closing);
		FreezeTime.Set(FreezeTime.Id.PDA, flag ? sequence.t : 0f);
		bool flag2 = FreezeTime.GetTopmostId() == FreezeTime.Id.PDA;
		bool flag3 = flag && flag2;
		UpdateTime(flag3);
		Player.main.playerAnimator.updateMode = (flag3 ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal);
		sequence.Update(deltaTime);
		Player main = Player.main;
		if (isInUse && isFocused && GameInput.GetButtonDown(GameInput.Button.PDA) && !ui.introActive)
		{
			Close();
		}
		else if (targetWasSet && (target == null || (target.transform.position - main.transform.position).sqrMagnitude >= activeSqrDistance))
		{
			Close();
		}
	}

	public bool Open(PDATab tab = PDATab.None, Transform target = null, OnClose onCloseCallback = null)
	{
		if (isInUse || ignorePDAInput)
		{
			return false;
		}
		uGUI.main.quickSlots.SetTarget(null);
		prevQuickSlot = Inventory.main.quickSlots.activeSlot;
		bool num = Inventory.main.ReturnHeld();
		Player main = Player.main;
		if (!num || main.cinematicModeActive)
		{
			return false;
		}
		MainCameraControl.main.SaveLockedVRViewModelAngle();
		Inventory.main.quickSlots.SetSuspendSlotActivation(value: true);
		isInUse = true;
		main.armsController.SetUsingPda(isUsing: true);
		base.gameObject.SetActive(value: true);
		ui.OnOpenPDA(tab);
		sequence.Set(0.5f, target: true, Activated);
		GoalManager.main.OnCustomGoalEvent("Open_PDA");
		if (HandReticle.main != null)
		{
			HandReticle.main.RequestCrosshairHide();
		}
		Inventory.main.SetViewModelVis(state: false);
		targetWasSet = target != null;
		this.target = target;
		this.onCloseCallback = onCloseCallback;
		if (targetWasSet)
		{
			activeSqrDistance = (target.transform.position - main.transform.position).sqrMagnitude + 1f;
		}
		if (audioSnapshotInstance.isValid())
		{
			audioSnapshotInstance.start();
		}
		UwePostProcessingManager.OpenPDA();
		return true;
	}

	public void Close()
	{
		if (isInUse && !ignorePDAInput)
		{
			Player main = Player.main;
			QuickSlots quickSlots = Inventory.main.quickSlots;
			quickSlots.EndAssign();
			MainCameraControl.main.ResetLockedVRViewModelAngle();
			Vehicle vehicle = main.GetVehicle();
			if (vehicle != null)
			{
				uGUI.main.quickSlots.SetTarget(vehicle);
			}
			targetWasSet = false;
			target = null;
			main.armsController.SetUsingPda(isUsing: false);
			quickSlots.SetSuspendSlotActivation(value: false);
			ui.OnClosePDA();
			if (HandReticle.main != null)
			{
				HandReticle.main.UnrequestCrosshairHide();
			}
			Inventory.main.SetViewModelVis(state: true);
			sequence.Set(0.3f, target: false, Deactivated);
			if (audioSnapshotInstance.isValid())
			{
				audioSnapshotInstance.stop(STOP_MODE.ALLOWFADEOUT);
				audioSnapshotInstance.release();
			}
			UwePostProcessingManager.ClosePDA();
			if (onCloseCallback != null)
			{
				OnClose onClose = onCloseCallback;
				onCloseCallback = null;
				onClose(this);
			}
		}
	}

	public void Activated()
	{
		UWE.Utils.lockCursor = false;
		ui.Select();
		ui.OnPDAOpened();
	}

	public void Deactivated()
	{
		if (!ignorePDAInput)
		{
			Inventory.main.quickSlots.Select(prevQuickSlot);
		}
		ui.OnPDAClosed();
		base.gameObject.SetActive(value: false);
		isInUse = false;
	}
}
