using UnityEngine;

public class DistanceCullManager : MonoBehaviour
{
	private DistanceCull[] distCull;

	public int checksPerFrame = 15;

	private int currentIndex;

	private SNCameraRoot cameraRoot;

	private Vector3 cameraPos = Vector3.zero;

	private void Update()
	{
		if (distCull == null || cameraRoot == null)
		{
			distCull = base.gameObject.GetComponentsInChildren<DistanceCull>(includeInactive: true);
			cameraRoot = SNCameraRoot.main;
			return;
		}
		cameraPos = cameraRoot.transform.position;
		int num = currentIndex + checksPerFrame;
		if (num > distCull.Length)
		{
			num = distCull.Length;
		}
		for (int i = currentIndex; i < num; i++)
		{
			float sqrMagnitude = (cameraPos - distCull[i].transform.position).sqrMagnitude;
			bool flag = (distCull[i].proximityCulling ? (sqrMagnitude < distCull[i].distanceSqr) : (sqrMagnitude > distCull[i].distanceSqr));
			if (distCull[i].isEnabled && flag)
			{
				distCull[i].DisableObject();
			}
			else if (!flag)
			{
				if (distCull[i].isEnabled)
				{
					distCull[i].EnableObject();
				}
				else
				{
					distCull[i].gameObject.SetActive(value: true);
				}
			}
		}
		currentIndex += checksPerFrame;
		if (currentIndex >= distCull.Length - 1)
		{
			currentIndex = 0;
		}
	}
}
