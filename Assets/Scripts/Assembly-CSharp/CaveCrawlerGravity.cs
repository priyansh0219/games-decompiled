using UnityEngine;

public class CaveCrawlerGravity : MonoBehaviour
{
	[AssertNotNull]
	public CaveCrawler caveCrawler;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public Rigidbody crawlerRigidbody;

	private void FixedUpdate()
	{
		crawlerRigidbody.useGravity = false;
		bool flag = base.transform.position.y >= 0f;
		int num;
		if (caveCrawler.IsOnSurface())
		{
			num = (liveMixin.IsAlive() ? 1 : 0);
			if (num != 0)
			{
				float num2 = 10f;
				Vector3 surfaceNormal = caveCrawler.GetSurfaceNormal();
				crawlerRigidbody.AddForce(-surfaceNormal * num2);
				goto IL_00a8;
			}
		}
		else
		{
			num = 0;
		}
		float num3 = (flag ? 9.81f : 2.7f);
		crawlerRigidbody.AddForce(-Vector3.up * Time.fixedDeltaTime * num3, ForceMode.VelocityChange);
		goto IL_00a8;
		IL_00a8:
		float num4 = ((num != 0) ? 1.6f : 0.03f);
		if (!flag)
		{
			num4 += 0.3f;
		}
		crawlerRigidbody.drag = num4;
	}
}
