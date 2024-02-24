using System.Diagnostics;
using System.IO;
using UnityEngine;

public sealed class CellDebugger : MonoBehaviour
{
	public string csvFilename = "cell_debug.csv";

	private StreamWriter file;

	private EntityCell currentCell;

	private EntityCell lastCell;

	public static CellDebugger Instance { get; private set; }

	private void Start()
	{
		Instance = this;
		CreateFile();
	}

	private void OnDestroy()
	{
		CloseFile();
		Instance = null;
	}

	[ContextMenu("Flush")]
	private void Flush()
	{
		file.Flush();
	}

	[ContextMenu("Restart")]
	private void Restart()
	{
		CloseFile();
		CreateFile();
	}

	private void CreateFile()
	{
		file = FileUtils.CreateTextFile(csvFilename);
	}

	private void CloseFile()
	{
		file.Close();
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	internal void OnBeginProcess(EntityCell cell)
	{
		currentCell = cell;
		lastCell = cell;
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	internal void OnEndProcess(EntityCell cell)
	{
		currentCell = null;
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	public void OnSpawnVirtualEntity(VirtualPrefabIdentifier vpid, LargeWorldEntity ent)
	{
		Write("OnSpawnVirtualEntity", lastCell, ent, vpid, ent.transform.position);
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	public void OnSpawnEntity(VirtualPrefabIdentifier vpid, GameObject go)
	{
		LargeWorldEntity component = go.GetComponent<LargeWorldEntity>();
		PrefabIdentifier component2 = go.GetComponent<PrefabIdentifier>();
		Write("OnSpawnEntity", lastCell, component, vpid, component.transform.position);
		Write("OnSpawnEntity", lastCell, component, component2, component.transform.position);
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	internal void OnSerializeWaiter(EntityCell entityCell, LargeWorldEntity ent)
	{
		Write("OnSerializeWaiter", entityCell, ent);
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	internal void OnAddEntity(EntityCell entityCell, LargeWorldEntity ent)
	{
		Write("OnAddEntity", entityCell, ent);
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	internal void OnAddWaiter(EntityCell entityCell, LargeWorldEntity ent)
	{
		Write("OnAddWaiter", entityCell, ent);
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	internal void OnAwakeWaiter(EntityCell entityCell, LargeWorldEntity ent)
	{
		Write("OnAwakeWaiter", entityCell, ent);
	}

	[Conditional("ENABLE_CELL_DEBUGGER")]
	internal void OnDestroyWaiter(EntityCell entityCell, LargeWorldEntity ent)
	{
		Write("OnDestroyWaiter", entityCell, ent);
	}

	private void Write(string category, EntityCell cell, LargeWorldEntity ent)
	{
		Write(category, cell, ent, ent.GetComponent<UniqueIdentifier>(), ent.transform.position);
	}

	private void Write(string category, EntityCell cell, LargeWorldEntity ent, UniqueIdentifier uid, Vector3 pos)
	{
		file.Write(category);
		file.Write(", ");
		file.Write(cell.BatchId);
		file.Write(", ");
		file.Write(cell.CellId);
		file.Write(", ");
		file.Write(cell.Level);
		file.Write(", ");
		file.Write(cell.CurrentState);
		file.Write(", ");
		file.Write(uid.ClassId);
		file.Write(", ");
		file.Write(uid.Id);
		file.Write(", ");
		file.Write(ent.name);
		file.Write(", ");
		file.Write(ent.cellLevel);
		file.Write(", ");
		file.Write(ent.GetInstanceID());
		file.Write(", ");
		file.Write(pos.x);
		file.Write(", ");
		file.Write(pos.y);
		file.Write(", ");
		file.Write(pos.z);
		file.Write(", ");
		file.WriteLine();
	}
}
