using ProtoBuf;

[ProtoContract]
public class BasePowerDistributor : Constructable
{
	public override void OnConstructedChanged(bool constructed)
	{
		if (constructed)
		{
			PowerPlug[] componentsInChildren = GetComponentsInChildren<PowerPlug>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetActive(value: true);
			}
		}
	}
}
