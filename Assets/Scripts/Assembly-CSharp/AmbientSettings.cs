using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class AmbientSettings : MonoBehaviour
{
	[ProtoMember(1)]
	public Color ambientLight = new Color(0.20392157f, 0.20392157f, 0.20392157f);

	public void Capture()
	{
		ambientLight = RenderSettings.ambientLight;
	}

	public override string ToString()
	{
		return ambientLight.ToString();
	}
}
