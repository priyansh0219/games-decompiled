using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class CreatureBehaviour : MonoBehaviour
{
	[ProtoMember(1)]
	public Vector3 leashPosition = Vector3.zero;

	private void Start()
	{
		Creature component = base.gameObject.GetComponent<Creature>();
		if (leashPosition != Vector3.zero && component != null)
		{
			component.leashPosition = leashPosition;
		}
		Object.Destroy(this);
	}
}
