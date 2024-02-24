using UnityEngine;

public class VehicleInterface_EnergyBar : MonoBehaviour
{
	public EnergyMixin energyMixin;

	public GameObject energyBar;

	public Material energyBarMat;

	private void Start()
	{
		energyBarMat = energyBar.GetComponent<Renderer>().material;
	}

	private void Update()
	{
		float energyScalar = energyMixin.GetEnergyScalar();
		energyBarMat.SetFloat(ShaderPropertyID._Amount, energyScalar);
		if (energyScalar < 0.2f)
		{
			float num = Mathf.Max(0.2f, Mathf.PingPong(Time.time, 1f));
			Color value = new Color(1f, num, num, 1f);
			energyBarMat.SetColor(ShaderPropertyID._Color, value);
		}
		else
		{
			energyBarMat.SetColor(ShaderPropertyID._Color, Color.white);
		}
	}
}
