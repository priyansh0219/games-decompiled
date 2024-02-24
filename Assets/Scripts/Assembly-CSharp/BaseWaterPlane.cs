using System;
using System.Linq;
using UnityEngine;

public sealed class BaseWaterPlane : MonoBehaviour, ICompileTimeCheckable
{
	[SerializeField]
	private bool waterPlaneStayVisible;

	[SerializeField]
	[AssertNotNull]
	private Transform waterPlane;

	[SerializeField]
	[AssertNotNull]
	private Renderer waterRender;

	[SerializeField]
	[AssertNotNull(AssertNotNullAttribute.Options.AllowEmptyCollection)]
	private Renderer[] fogRenderers;

	[SerializeField]
	[AssertNotNull(AssertNotNullAttribute.Options.AllowEmptyCollection)]
	private Renderer[] waterOnWallRenderers;

	[SerializeField]
	[AssertNotNull(AssertNotNullAttribute.Options.AllowEmptyCollection)]
	private GameObject[] children;

	[NonSerialized]
	private float _leakAmount;

	[NonSerialized]
	public Transform hostTrans;

	[NonSerialized]
	public bool isGhost;

	[NonSerialized]
	public float waterlevel;

	[NonSerialized]
	private bool _waterVisible;

	[SerializeField]
	private Vector3 extentsCenter;

	[SerializeField]
	private Vector3 extentsSize;

	public float leakAmount
	{
		get
		{
			return _leakAmount;
		}
		set
		{
			_leakAmount = value;
			_waterVisible = !isGhost && (_leakAmount > 0f || waterPlaneStayVisible);
			UpdateWaterPlaneLevel();
			if (waterPlaneStayVisible && waterPlane.position.y > Ocean.GetOceanLevel())
			{
				_waterVisible = false;
			}
			UpdateChildrenActive();
			if (_waterVisible)
			{
				UpdateMaterial();
			}
		}
	}

	public void UpdateChildrenActive()
	{
		for (int i = 0; i < children.Length; i++)
		{
			children[i].SetActive(_waterVisible);
		}
	}

	public void OnPlayerEntered(Player p)
	{
		waterRender.enabled = true;
	}

	public void OnPlayerExited(Player p)
	{
		waterRender.enabled = false;
	}

	private void Awake()
	{
		leakAmount = 0f;
		UpdateChildrenActive();
	}

	private void OnAddedToBase(Base baseComp)
	{
		isGhost = !(baseComp != null) || baseComp.isGhost;
		isGhost = isGhost || GetComponentInParent<ConstructableBase>() != null;
		leakAmount = 0f;
		UpdateChildrenActive();
	}

	private float GetClipValue(Vector3 newPos)
	{
		float num = Mathf.Abs(extentsSize.y) * 0.5f;
		_ = extentsCenter;
		float num2 = num + extentsCenter.y;
		float num3 = newPos.y / num2;
		return Mathf.Clamp01(1f - Mathf.Sqrt(1f - num3 * num3));
	}

	private void UpdateWaterPlaneLevel()
	{
		if (hostTrans != null)
		{
			Vector3 localPosition = waterPlane.localPosition;
			localPosition.y = waterlevel - hostTrans.position.y;
			waterPlane.localPosition = localPosition;
		}
	}

	private void UpdateMaterial()
	{
		if (hostTrans != null)
		{
			Vector3 localPosition = waterPlane.localPosition;
			float clipValue = GetClipValue(localPosition);
			waterRender.material.SetFloat(ShaderPropertyID._ClipedValue, clipValue);
			for (int i = 0; i < fogRenderers.Length; i++)
			{
				fogRenderers[i].material.SetFloat(ShaderPropertyID._LocalFloodLevel, leakAmount);
			}
			for (int j = 0; j < waterOnWallRenderers.Length; j++)
			{
				waterOnWallRenderers[j].material.SetFloat(ShaderPropertyID._LocalFloodLevel, waterlevel);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(extentsCenter, extentsSize);
	}

	[ContextMenu("Upgrade Configuration")]
	public void UpgradeConfiguration()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		if (!component)
		{
			Debug.LogWarning("Could not find BoxCollider to copy extents from.");
			return;
		}
		extentsCenter = component.center;
		extentsSize = component.size;
		UnityEngine.Object.DestroyImmediate(component, allowDestroyingAssets: true);
		Collider component2 = GetComponent<Collider>();
		VFXSurface component3 = GetComponent<VFXSurface>();
		if ((bool)component3 && !component2)
		{
			UnityEngine.Object.DestroyImmediate(component3, allowDestroyingAssets: true);
		}
		children = (from p in base.gameObject.GetComponentsInChildren<Transform>()
			select p.gameObject).ToArray();
	}

	public string CompileTimeCheck()
	{
		if (extentsSize.sqrMagnitude < 0.01f)
		{
			return "BaseWaterPlane missing extents";
		}
		BoxCollider component = GetComponent<BoxCollider>();
		if ((bool)component && (!component.enabled || component.isTrigger))
		{
			return "BaseWaterPlane still has BoxCollider to copy extents from";
		}
		return null;
	}
}
