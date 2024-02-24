using TMPro;
using UnityEngine;

public class MainMenuChangeset : MonoBehaviour
{
	private void Start()
	{
		string plasticChangeSetOfBuild = SNUtils.GetPlasticChangeSetOfBuild();
		if (!string.IsNullOrEmpty(plasticChangeSetOfBuild))
		{
			base.gameObject.GetComponent<TextMeshProUGUI>().text = "Changeset #" + plasticChangeSetOfBuild;
		}
	}
}
