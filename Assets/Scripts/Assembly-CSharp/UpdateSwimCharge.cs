using UnityEngine;

public class UpdateSwimCharge : MonoBehaviour
{
	public FMOD_CustomEmitter swimChargeLoop;

	public float chargePerSecond = 0.25f;

	private void FixedUpdate()
	{
		bool num = Inventory.Get().equipment.GetCount(TechType.SwimChargeFins) > 0;
		bool flag = true;
		bool flag2 = false;
		if (num && flag && Player.main.IsUnderwater() && Player.main.gameObject.GetComponent<Rigidbody>().velocity.magnitude > 2f)
		{
			PlayerTool heldTool = Inventory.main.GetHeldTool();
			if (heldTool != null)
			{
				EnergyMixin component = heldTool.gameObject.GetComponent<EnergyMixin>();
				if (component != null && component.AddEnergy(chargePerSecond * Time.deltaTime))
				{
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			swimChargeLoop.Play();
		}
		else
		{
			swimChargeLoop.Stop();
		}
	}
}
