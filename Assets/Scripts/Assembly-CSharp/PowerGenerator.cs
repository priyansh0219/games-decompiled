using System;
using ProtoBuf;
using UnityEngine;

[Obsolete]
[ProtoContract]
public class PowerGenerator : MonoBehaviour
{
	private void Start()
	{
		Debug.LogWarningFormat(this, "destroying deprecated power generator on '{0}'", base.gameObject);
		UnityEngine.Object.Destroy(this);
	}
}
