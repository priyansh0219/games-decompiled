using UnityEngine;

namespace Subnautica
{
	public class Platform : MonoBehaviour
	{
		public Transform platformParent;

		private void OnCollisionStay(Collision collision)
		{
			if (collision.gameObject.CompareTag("Player"))
			{
				collision.transform.parent = platformParent;
			}
		}

		private void OnCollisionExit(Collision collision)
		{
			if (collision.gameObject.CompareTag("Player"))
			{
				collision.transform.parent = null;
			}
		}
	}
}
