using UnityEngine;

public class TridentHome : MonoBehaviour
{
	public Transform[] movePath;

	public bool inHole = true;

	public GameObject tridentPrefab;

	public Trident trident;

	public TridentHole[] holes;

	private void OnDrawGizmos()
	{
		iTween.DrawPath(movePath, Color.magenta);
	}

	private void Awake()
	{
		GameObject gameObject = Object.Instantiate(tridentPrefab, base.gameObject.transform.position, Quaternion.identity);
		trident = gameObject.GetComponent<Trident>();
		trident.gameObject.SetActive(value: false);
		trident.movePath = movePath;
	}

	public void AwakeNearbyTrident(TridentHole tridentHole)
	{
		if (!trident.gameObject.activeInHierarchy && ((trident.forwardDirection && tridentHole == holes[0]) || (!trident.forwardDirection && tridentHole == holes[1])))
		{
			trident.Spawn();
		}
	}
}
