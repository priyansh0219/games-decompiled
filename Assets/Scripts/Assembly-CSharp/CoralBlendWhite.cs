using UnityEngine;

[RequireComponent(typeof(LiveMixin))]
public class CoralBlendWhite : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	private Renderer[] renderers;

	private MaterialPropertyBlock propertyBlock;

	public float blendTime = 8f;

	private bool killed;

	private float timeOfDeath;

	private bool done;

	private int timesDied;

	private bool updatingDeathFade;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "CoralBlendWhite";
	}

	private void Awake()
	{
		renderers = GetComponentsInChildren<Renderer>();
		propertyBlock = new MaterialPropertyBlock();
	}

	private void OnKill()
	{
		timeOfDeath = Time.time;
		killed = true;
		timesDied++;
		if (!done)
		{
			RegisterForDeathUpdate();
		}
		if (timesDied >= 2)
		{
			Living component = base.gameObject.GetComponent<Living>();
			if ((bool)component)
			{
				Object.Destroy(component);
			}
		}
		else
		{
			LiveMixin component2 = GetComponent<LiveMixin>();
			component2.health = component2.maxHealth * 0.5f;
		}
	}

	public void ManagedUpdate()
	{
		if (done)
		{
			return;
		}
		float num = Time.time - timeOfDeath;
		float num2 = Mathf.Min(1f, num / blendTime);
		float value = num2 * 0.3f;
		float value2 = num2;
		if (propertyBlock != null)
		{
			propertyBlock.SetFloat(ShaderPropertyID._Brightness, value);
			propertyBlock.SetFloat(ShaderPropertyID._Gray, value2);
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].SetPropertyBlock(propertyBlock);
			}
		}
		if (num2 == 1f)
		{
			UnregisterFromDeathUpdate();
			updatingDeathFade = false;
			done = true;
		}
	}

	private void RegisterForDeathUpdate()
	{
		BehaviourUpdateUtils.Register(this);
		updatingDeathFade = true;
	}

	private void UnregisterFromDeathUpdate()
	{
		if (updatingDeathFade)
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	private void OnEnable()
	{
		if (updatingDeathFade)
		{
			RegisterForDeathUpdate();
		}
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}
}
