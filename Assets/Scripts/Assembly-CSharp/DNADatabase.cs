using System.Collections.Generic;
using UWE;
using UnityEngine;

public class DNADatabase : MonoBehaviour
{
	public static DNADatabase main;

	public TextAsset DNA_database;

	public GameObject serumPrefab;

	public List<DNADatabaseRow> dnaRows = new List<DNADatabaseRow>();

	public bool debug;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		dnaRows = CSVUtils.Read<DNADatabaseRow>(DNA_database.text);
		if (!debug)
		{
			return;
		}
		Debug.Log("Read " + dnaRows.Count + " DNA rows from " + DNA_database.text + ".");
		foreach (DNADatabaseRow dnaRow in dnaRows)
		{
			Debug.Log(" => " + dnaRow.ToString());
		}
	}

	public DNADatabaseRow GetRowForBehavior(string behaviorName)
	{
		foreach (DNADatabaseRow dnaRow in dnaRows)
		{
			if (dnaRow.behavior.ToLower() == behaviorName.ToLower())
			{
				return dnaRow;
			}
		}
		return null;
	}

	public Serum CreateSerum(string sampleName, Vector3 position)
	{
		Serum serum = null;
		DNADatabaseRow dNADatabaseRow = null;
		foreach (DNADatabaseRow dnaRow in dnaRows)
		{
			if (dnaRow.sampleName.ToLower() == sampleName.ToLower())
			{
				dNADatabaseRow = dnaRow;
				break;
			}
		}
		if (dNADatabaseRow != null)
		{
			serum = Object.Instantiate(serumPrefab.gameObject, position, Quaternion.identity).GetComponent<Serum>();
			serum.dnaEntry.CopyFrom(dNADatabaseRow);
		}
		else
		{
			Debug.Log("DNADatabase.CreateSerum(" + sampleName + ") failed: couldn't find DNA entry with this sample name.");
		}
		return serum;
	}
}
