using System;
using Story;
using UWE;
using UnityEngine;

public class PrecursorActivatedPillar : MonoBehaviour, IStoryGoalListener, ICompileTimeCheckable
{
	public float extendedY = 2f;

	public float dampening = 1.5f;

	public float lightIntensity = 4.5f;

	private float idleY;

	private float defaultZ;

	private float defaultX;

	private bool active = true;

	[AssertNotNull]
	public Renderer[] illumRenderers;

	[AssertNotNull]
	public Light flare;

	[AssertNotNull]
	public GameObject animatedMesh;

	public FMODAsset openSound;

	public FMODAsset closeSound;

	public FMOD_CustomLoopingEmitter openedLoopingSound;

	public StoryGoal disableOnGoal;

	private bool extended;

	private bool isFullyExtended;

	private MaterialPropertyBlock materialProps;

	public void OnTriggerEnter(Collider col)
	{
		if (!active)
		{
			return;
		}
		GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
		if (!entityRoot)
		{
			entityRoot = col.gameObject;
		}
		if (entityRoot.GetComponentInChildren<Player>() != null)
		{
			if ((bool)openSound)
			{
				Utils.PlayFMODAsset(openSound, base.transform);
			}
			if ((bool)openedLoopingSound)
			{
				openedLoopingSound.Play();
			}
			extended = true;
			isFullyExtended = false;
		}
	}

	private void Retract()
	{
		if ((bool)closeSound)
		{
			Utils.PlayFMODAsset(closeSound, base.transform);
		}
		if ((bool)openedLoopingSound)
		{
			openedLoopingSound.Stop();
		}
		extended = false;
		isFullyExtended = true;
	}

	public void OnTriggerExit(Collider col)
	{
		GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
		if (!entityRoot)
		{
			entityRoot = col.gameObject;
		}
		if (entityRoot.GetComponentInChildren<Player>() != null)
		{
			Retract();
		}
	}

	private void Start()
	{
		idleY = animatedMesh.transform.localPosition.y;
		defaultX = animatedMesh.transform.localPosition.x;
		defaultZ = animatedMesh.transform.localPosition.z;
		flare.intensity = 0f;
		materialProps = new MaterialPropertyBlock();
		SetGlowColor(Color.black, onUpdate: false);
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main && !string.IsNullOrEmpty(disableOnGoal.key))
		{
			if (!main.IsGoalComplete(disableOnGoal.key))
			{
				main.AddListener(this);
			}
			else
			{
				SetInactive();
			}
		}
	}

	private bool SetGlowColor(Color color, bool onUpdate)
	{
		bool result = true;
		for (int i = 0; i < illumRenderers.Length; i++)
		{
			Renderer obj = illumRenderers[i];
			obj.GetPropertyBlock(materialProps);
			Color value;
			if (onUpdate)
			{
				value = Color.Lerp(materialProps.GetColor(ShaderPropertyID._GlowColor), color, Time.deltaTime / dampening);
				if (Mathf.Abs(value.r - color.r) < 0.0001f)
				{
					value.r = color.r;
				}
				if (Mathf.Abs(value.g - color.g) < 0.0001f)
				{
					value.g = color.g;
				}
				if (Mathf.Abs(value.b - color.b) < 0.0001f)
				{
					value.b = color.b;
				}
			}
			else
			{
				value = color;
			}
			materialProps.SetColor(ShaderPropertyID._GlowColor, value);
			obj.SetPropertyBlock(materialProps);
			if (!Mathf.Approximately(value.r, color.r) || !Mathf.Approximately(value.g, color.g) || !Mathf.Approximately(value.b, color.b))
			{
				result = false;
			}
		}
		return result;
	}

	private void Update()
	{
		if (extended != isFullyExtended)
		{
			bool num = SetGlowColor(extended ? Color.white : Color.black, onUpdate: true);
			Vector3 vector = (extended ? new Vector3(defaultX, extendedY, defaultZ) : new Vector3(defaultX, idleY, defaultZ));
			if (Mathf.Abs(vector.y - animatedMesh.transform.localPosition.y) < 0.0001f)
			{
				animatedMesh.transform.localPosition = vector;
			}
			else
			{
				animatedMesh.transform.localPosition = Vector3.Lerp(animatedMesh.transform.localPosition, vector, Time.deltaTime / dampening);
			}
			bool flag = Mathf.Approximately(animatedMesh.transform.localPosition.y, vector.y);
			float num2 = (extended ? lightIntensity : 0f);
			if (Mathf.Abs(flare.intensity - num2) < 0.0001f)
			{
				flare.intensity = num2;
			}
			else
			{
				flare.intensity = Mathf.Lerp(flare.intensity, num2, Time.deltaTime / dampening);
			}
			bool flag2 = Mathf.Approximately(flare.intensity, num2);
			if (num && flag && flag2)
			{
				isFullyExtended = extended;
			}
		}
	}

	private void OnDestroy()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			main.RemoveListener(this);
		}
	}

	private void SetInactive()
	{
		active = false;
		Retract();
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, disableOnGoal.key, StringComparison.OrdinalIgnoreCase))
		{
			SetInactive();
		}
	}

	public string CompileTimeCheck()
	{
		if (string.IsNullOrEmpty(disableOnGoal.key))
		{
			return null;
		}
		return StoryGoalUtils.CheckStoryGoal(disableOnGoal);
	}
}
