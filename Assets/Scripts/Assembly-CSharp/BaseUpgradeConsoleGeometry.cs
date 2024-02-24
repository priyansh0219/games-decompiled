using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class BaseUpgradeConsoleGeometry : MonoBehaviour, IBaseModuleGeometry, IObstacle
{
	private const float infoUpdateInterval = 1f;

	[AssertNotNull]
	public Fabricator fabricator;

	[AssertNotNull]
	public SubNameInput subNameInput;

	[AssertNotNull]
	public TextMeshProUGUI infoPanel;

	[AssertNotNull]
	public GameObject modulePrefab;

	private VehicleDockingBay dockingBay;

	private Base.Face _geometryFace;

	public Base.Face geometryFace
	{
		get
		{
			return _geometryFace;
		}
		set
		{
			_geometryFace = value;
			OnGeometryFaceChanged();
		}
	}

	private IEnumerator Start()
	{
		while (true)
		{
			if (dockingBay != null)
			{
				Vehicle dockedVehicle = dockingBay.GetDockedVehicle();
				infoPanel.text = GetVehicleInfo(dockedVehicle);
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void OnDestroy()
	{
		if (dockingBay != null)
		{
			VehicleDockingBay vehicleDockingBay = dockingBay;
			vehicleDockingBay.onDockedChanged = (VehicleDockingBay.OnDockedChanged)Delegate.Remove(vehicleDockingBay.onDockedChanged, new VehicleDockingBay.OnDockedChanged(OnDockedChanged));
		}
	}

	private BaseUpgradeConsole GetModule()
	{
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			IBaseModule module = componentInParent.GetModule(geometryFace);
			if (module != null)
			{
				return module as BaseUpgradeConsole;
			}
		}
		return null;
	}

	private void OnGeometryFaceChanged()
	{
		BaseUpgradeConsole module = GetModule();
		if (module == null)
		{
			Base componentInParent = GetComponentInParent<Base>();
			if (componentInParent != null)
			{
				componentInParent.SpawnModule(modulePrefab, _geometryFace);
			}
		}
		if (module != null)
		{
			fabricator.logic = module.crafterLogic;
		}
		if (dockingBay != null)
		{
			VehicleDockingBay vehicleDockingBay = dockingBay;
			vehicleDockingBay.onDockedChanged = (VehicleDockingBay.OnDockedChanged)Delegate.Remove(vehicleDockingBay.onDockedChanged, new VehicleDockingBay.OnDockedChanged(OnDockedChanged));
			dockingBay = null;
		}
		Transform parent = GetComponent<Transform>().parent;
		if (parent != null)
		{
			dockingBay = parent.GetComponentInChildren<VehicleDockingBay>();
			if (dockingBay != null)
			{
				VehicleDockingBay vehicleDockingBay2 = dockingBay;
				vehicleDockingBay2.onDockedChanged = (VehicleDockingBay.OnDockedChanged)Delegate.Combine(vehicleDockingBay2.onDockedChanged, new VehicleDockingBay.OnDockedChanged(OnDockedChanged));
			}
		}
		OnDockedChanged();
	}

	private void OnDockedChanged()
	{
		Vehicle vehicle = ((dockingBay == null) ? null : dockingBay.GetDockedVehicle());
		subNameInput.SetTarget((vehicle == null) ? null : vehicle.subName);
	}

	private string GetVehicleInfo(Vehicle vehicle)
	{
		if (vehicle == null)
		{
			return Language.main.Get("SubmersibleNotDocked");
		}
		StringBuilder stringBuilder = new StringBuilder();
		string value = vehicle.GetName();
		stringBuilder.Append(value);
		stringBuilder.Append(' ');
		stringBuilder.Append(Language.main.Get("SubmersibleDocked"));
		EnergyMixin component = vehicle.GetComponent<EnergyMixin>();
		if (component != null)
		{
			float energyScalar = component.GetEnergyScalar();
			stringBuilder.Append('\n');
			stringBuilder.Append("<size=30>");
			if (energyScalar == 1f)
			{
				stringBuilder.Append(Language.main.Get("SubmersibleFullyCharged"));
			}
			else
			{
				stringBuilder.Append(Language.main.Get("SubmersibleCharging"));
				stringBuilder.Append(' ');
				stringBuilder.Append(Mathf.RoundToInt(energyScalar * 100f).ToString());
				stringBuilder.Append('%');
			}
			stringBuilder.Append("</size>");
		}
		return stringBuilder.ToString();
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		reason = null;
		BaseUpgradeConsole module = GetModule();
		if (module != null && module.crafterLogic != null && module.crafterLogic.craftingTechType != 0)
		{
			reason = Language.main.Get("DeconstructUpgradeConsoleFabricatorError");
			return false;
		}
		return true;
	}
}
