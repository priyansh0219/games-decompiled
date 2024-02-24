using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(EnergyMixin))]
public class Terraformer : PlayerTool
{
	public static bool debugInfiniteAmmo;

	public float range = 10f;

	public float editRadius = 1f;

	public int type = 2;

	public Animator animator;

	public FMODAsset cutReadySound;

	public FMOD_CustomLoopingEmitter placeLoopSound;

	public FMOD_CustomLoopingEmitter cutLoopSound;

	private FMOD_CustomLoopingEmitter loopSound;

	private GameObject probe;

	private bool penDown;

	private bool isDig;

	private bool outOfAmmoMsged;

	private Stack<GameObject> strokePool = new Stack<GameObject>();

	private Stack<GameObject> activeStrokes = new Stack<GameObject>();

	[NonSerialized]
	[ProtoMember(1)]
	public int ammo;

	public int GetAmmoCount()
	{
		return ammo;
	}

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
		ErrorMessage.AddMessage(Language.main.Get("TerraformerInstructions"));
	}

	public override void OnHolster()
	{
		base.OnHolster();
		foreach (GameObject item in strokePool)
		{
			UnityEngine.Object.Destroy(item);
		}
		strokePool.Clear();
		foreach (GameObject activeStroke in activeStrokes)
		{
			UnityEngine.Object.Destroy(activeStroke);
		}
		activeStrokes.Clear();
		UnityEngine.Object.Destroy(probe);
	}

	private GameObject GetStroke()
	{
		if (strokePool.Count == 0)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			obj.transform.localScale = 2f * new Vector3(editRadius, editRadius, editRadius);
			UnityEngine.Object.Destroy(obj.GetComponent<Collider>());
			return obj;
		}
		return strokePool.Pop();
	}

	public override void Awake()
	{
		base.Awake();
	}

	private void PrepareProbe()
	{
		if (probe == null)
		{
			probe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			probe.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			UnityEngine.Object.Destroy(probe.GetComponent<Collider>());
		}
	}

	public void Update()
	{
		if (LargeWorld.main == null || usingPlayer == null)
		{
			return;
		}
		PrepareProbe();
		Transform aimingTransform = usingPlayer.camRoot.GetAimingTransform();
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(aimingTransform.position, aimingTransform.forward, out hitInfo, range, 1073741824);
		if (flag)
		{
			probe.transform.position = hitInfo.point;
			if (hitInfo.distance < editRadius)
			{
				probe.SetActive(value: false);
			}
			else
			{
				probe.SetActive(value: true);
			}
			if (penDown && (activeStrokes.Count == 0 || (activeStrokes.Peek().transform.position - hitInfo.point).magnitude > editRadius))
			{
				if (!isDig && ammo <= 0 && !debugInfiniteAmmo)
				{
					if (!outOfAmmoMsged)
					{
						ErrorMessage.AddMessage(Language.main.Get("TerraformerOutOfMaterial"));
					}
					outOfAmmoMsged = true;
				}
				else if (!isDig && hitInfo.distance < editRadius)
				{
					ErrorMessage.AddMessage(Language.main.Get("TerraformerTooClose"));
				}
				else
				{
					if (!isDig)
					{
						ammo--;
					}
					GameObject stroke = GetStroke();
					stroke.transform.position = hitInfo.point;
					stroke.SetActive(value: true);
					if (!isDig)
					{
						stroke.GetComponent<Renderer>().material.color = Color.green;
					}
					else
					{
						stroke.GetComponent<Renderer>().material.color = Color.red;
					}
					activeStrokes.Push(stroke);
				}
			}
		}
		else
		{
			probe.SetActive(value: false);
		}
		if (penDown && !GameInput.GetButtonHeld(GameInput.Button.RightHand) && !GameInput.GetButtonHeld(GameInput.Button.LeftHand))
		{
			_ = Vector3.zero;
			foreach (GameObject activeStroke in activeStrokes)
			{
				if (Input.GetKey(KeyCode.LeftAlt))
				{
					LargeWorld.main.streamer.PerformBoxEdit(new Bounds(activeStroke.transform.position, new Vector3(5f, 5f, 5f)), MainCamera.camera.transform.rotation, !isDig, Convert.ToByte(type));
				}
				else
				{
					LargeWorld.main.streamer.PerformSphereEdit(activeStroke.transform.position, editRadius, !isDig, Convert.ToByte(type));
				}
				if (isDig)
				{
					ammo++;
				}
				_ = activeStroke.transform.position;
				strokePool.Push(activeStroke);
				activeStroke.SetActive(value: false);
			}
			activeStrokes.Clear();
			penDown = false;
			SafeAnimator.SetBool(animator, "use_loop", value: false);
			loopSound.Stop();
		}
		if (penDown)
		{
			return;
		}
		bool flag2 = Input.GetKey(KeyCode.LeftShift) || GameInput.GetButtonDown(GameInput.Button.LeftHand);
		if (flag2 != isDig)
		{
			isDig = flag2;
			SafeAnimator.SetBool(animator, "terraformer_mode_on", isDig);
			if (isDig)
			{
				Utils.PlayFMODAsset(cutReadySound, base.transform);
			}
		}
		if (GameInput.GetButtonDown(GameInput.Button.RightHand) || GameInput.GetButtonDown(GameInput.Button.LeftHand))
		{
			if (!flag)
			{
				ErrorMessage.AddMessage(Language.main.Get("TerraformerTooFar"));
				return;
			}
			penDown = true;
			outOfAmmoMsged = false;
			SafeAnimator.SetBool(animator, "use_loop", value: true);
			loopSound = (isDig ? cutLoopSound : placeLoopSound);
			loopSound.Play();
		}
	}

	public override void OnToolUseAnim(GUIHand hand)
	{
	}

	public override bool GetUsedToolThisFrame()
	{
		return penDown;
	}
}
