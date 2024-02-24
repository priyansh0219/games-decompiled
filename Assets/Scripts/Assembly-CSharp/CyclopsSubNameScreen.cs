using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CyclopsSubNameScreen : MonoBehaviour
{
	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public GameObject content;

	private LiveMixin liveMixing;

	private Vector3 cachedScale;

	private void Start()
	{
		liveMixing = GetComponentInParent<LiveMixin>();
	}

	private void ContentOn()
	{
		content.SetActive(value: true);
	}

	private void ContentOff()
	{
		content.SetActive(value: false);
	}

	private void LateUpdate()
	{
		if (!content.activeSelf)
		{
			return;
		}
		Transform transform = content.transform;
		Vector3 localScale = transform.localScale;
		if (Mathf.Approximately(localScale.x, cachedScale.x) && Mathf.Approximately(localScale.y, cachedScale.y) && Mathf.Approximately(localScale.z, cachedScale.z))
		{
			return;
		}
		cachedScale = localScale;
		using (ListPool<TextMeshProUGUI> listPool = Pool<ListPool<TextMeshProUGUI>>.Get())
		{
			List<TextMeshProUGUI> list = listPool.list;
			transform.GetComponentsInChildren(list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].SetScaleDirty();
			}
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject) && liveMixing.IsAlive())
		{
			animator.SetBool("PanelActive", value: true);
			ContentOn();
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject))
		{
			animator.SetBool("PanelActive", value: false);
			Invoke("ContentOff", 0.5f);
		}
	}
}
