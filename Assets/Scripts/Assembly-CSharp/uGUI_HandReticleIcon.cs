using UnityEngine;
using UnityEngine.UI;

public class uGUI_HandReticleIcon : MonoBehaviour
{
	public HandReticle.IconType type;

	public Graphic[] graphic;

	private GameObject go;

	private Sequence sequence = new Sequence();

	private void Awake()
	{
		go = base.gameObject;
	}

	public void SetActive(bool active, float duration = 0f)
	{
		if (duration == 0f)
		{
			sequence.Set(duration, active, active);
		}
		else
		{
			sequence.Set(duration, active);
		}
	}

	public void UpdateIcon()
	{
		if (sequence.active)
		{
			sequence.Update(Time.deltaTime);
			float t = sequence.t;
			bool flag = t > 0f;
			if (go.activeSelf != flag)
			{
				go.SetActive(flag);
			}
			int i = 0;
			for (int num = graphic.Length; i < num; i++)
			{
				graphic[i].canvasRenderer.SetAlpha(t);
			}
		}
	}
}
