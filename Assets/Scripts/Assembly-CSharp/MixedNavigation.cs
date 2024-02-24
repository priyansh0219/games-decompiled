using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MixedNavigation : MonoBehaviour, IUpdateSelectedHandler, IEventSystemHandler
{
	public enum Mode
	{
		None = 0,
		Automatic = 3,
		Explicit = 4
	}

	[Serializable]
	public struct Entry
	{
		public Mode mode;

		public Selectable selectable;
	}

	private const ManagedUpdate.Queue queueLayoutComplete = ManagedUpdate.Queue.UILayoutComplete;

	[AssertNotNull]
	public Selectable target;

	public Entry up;

	public Entry down;

	public Entry left;

	public Entry right;

	public void Awake()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutComplete, OnUILayoutComplete);
	}

	private void OnUILayoutComplete()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UILayoutComplete, OnUILayoutComplete);
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		target.navigation = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = GetSelectable(up, MoveDirection.Up),
			selectOnDown = GetSelectable(down, MoveDirection.Down),
			selectOnRight = GetSelectable(right, MoveDirection.Right),
			selectOnLeft = GetSelectable(left, MoveDirection.Left)
		};
	}

	private Selectable GetSelectable(Entry entry, MoveDirection direction)
	{
		switch (entry.mode)
		{
		default:
			throw new NotImplementedException($"{entry.mode} navigation mode support is not implemented!");
		case Mode.None:
			return null;
		case Mode.Automatic:
			return target.FindSelectable(target.transform.rotation * GetVector(direction));
		case Mode.Explicit:
			return entry.selectable;
		}
	}

	private static Vector3 GetVector(MoveDirection direction)
	{
		switch (direction)
		{
		default:
			return Vector3.zero;
		case MoveDirection.Left:
			return Vector3.left;
		case MoveDirection.Up:
			return Vector3.up;
		case MoveDirection.Right:
			return Vector3.right;
		case MoveDirection.Down:
			return Vector3.down;
		}
	}

	public void OnUpdateSelected(BaseEventData eventData)
	{
		UpdateNavigation();
	}
}
