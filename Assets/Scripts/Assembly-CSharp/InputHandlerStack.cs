using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class InputHandlerStack : MonoBehaviour
{
	private class Wrapper
	{
		private GameObject legacyHandler;

		private IInputHandler handler;

		public string name
		{
			get
			{
				if (legacyHandler != null)
				{
					return legacyHandler.name;
				}
				if (handler != null && !handler.Equals(null))
				{
					using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
					{
						StringBuilder sb = stringBuilderPool.sb;
						sb.Append("[IInputHandler] type:").Append(handler.GetType()).Append(" hashCode:")
							.Append(handler.GetHashCode());
						uGUI_InputGroup uGUI_InputGroup2 = handler as uGUI_InputGroup;
						if (uGUI_InputGroup2 != null)
						{
							sb.Append(" selected:").Append(uGUI_InputGroup2.selected).Append(" focused:")
								.Append(uGUI_InputGroup2.focused);
						}
						return sb.ToString();
					}
				}
				return "None";
			}
		}

		public Wrapper(GameObject handler)
		{
			legacyHandler = handler;
		}

		public Wrapper(IInputHandler handler)
		{
			this.handler = handler;
		}

		public void SetActive(InputFocusMode mode)
		{
			if (legacyHandler != null)
			{
				legacyHandler.SetActive(mode == InputFocusMode.Add || mode == InputFocusMode.Restore);
			}
			else
			{
				handler.OnFocusChanged(mode);
			}
		}

		public bool Equals(GameObject handler)
		{
			return object.Equals(legacyHandler, handler);
		}

		public bool Equals(IInputHandler handler)
		{
			return object.Equals(this.handler, handler);
		}

		public bool HandleInput()
		{
			if (legacyHandler != null)
			{
				return true;
			}
			if (handler != null && !handler.Equals(null))
			{
				return handler.HandleInput();
			}
			return false;
		}

		public bool HandleLateInput()
		{
			if (legacyHandler != null)
			{
				return true;
			}
			if (handler != null && !handler.Equals(null))
			{
				return handler.HandleLateInput();
			}
			return false;
		}
	}

	public static InputHandlerStack main;

	private static bool debug;

	public GameObject defaultHandler;

	private int lastPopFrame = -1;

	private Stack<Wrapper> stack = new Stack<Wrapper>();

	private void Awake()
	{
		if (main != null)
		{
			Debug.LogError("More than one InputHandlerStack instance!");
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			main = this;
			DevConsole.RegisterConsoleCommand(this, "debuginputhandlerstack");
		}
	}

	private void Start()
	{
		Push(defaultHandler);
	}

	private void Update()
	{
		if (debug)
		{
			DrawDebug();
		}
		if (Time.frameCount == lastPopFrame + 1 && stack.Count > 0)
		{
			stack.Peek().SetActive(InputFocusMode.Restore);
		}
		if (lastPopFrame != Time.frameCount && stack.Count > 0 && !stack.Peek().HandleInput())
		{
			Pop();
		}
	}

	private void LateUpdate()
	{
		if (lastPopFrame != Time.frameCount && stack.Count > 0 && !stack.Peek().HandleLateInput())
		{
			Pop();
		}
	}

	public void Push(IInputHandler handler)
	{
		Push(new Wrapper(handler));
	}

	public bool IsFocused(IInputHandler handler)
	{
		return stack.Peek()?.Equals(handler) ?? false;
	}

	public bool IsDefaultHandlerFocused()
	{
		return stack.Peek()?.Equals(defaultHandler) ?? false;
	}

	[Obsolete("Use PushHandler(IInputHandler handler) instead!")]
	public void Push(GameObject handler)
	{
		Push(new Wrapper(handler));
	}

	[Obsolete("Use IInputHandler interface instead! Return false from IInputHandler.HandleInput() to perform Pop()")]
	public void Pop(GameObject handler)
	{
		if (stack.Count > 0)
		{
			if (stack.Peek().Equals(handler))
			{
				Pop();
			}
			else
			{
				Debug.LogError($"InputHandlerStack push/pop mismatch! GameObject named {handler.name} tried to pop when it wasn't on top.");
			}
		}
		else
		{
			Debug.LogError("Nothing to Pop(). Input handler stack is empty.");
		}
	}

	private void Push(Wrapper wrapper)
	{
		if (stack.Count > 0)
		{
			stack.Peek().SetActive(InputFocusMode.Suspend);
		}
		stack.Push(wrapper);
		wrapper.SetActive(InputFocusMode.Add);
	}

	private void Pop()
	{
		stack.Pop().SetActive(InputFocusMode.Remove);
		lastPopFrame = Time.frameCount;
		if (stack.Count == 0)
		{
			Debug.LogError("Warning: Just popped the very last input handler!");
		}
	}

	private void DrawDebug()
	{
		Stack<Wrapper>.Enumerator enumerator = stack.GetEnumerator();
		int num = 0;
		while (enumerator.MoveNext())
		{
			Wrapper current = enumerator.Current;
			Dbg.Write("{0}: {1}\n", num, current.name);
			num++;
		}
	}

	private void OnConsoleCommand_debuginputhandlerstack(NotificationCenter.Notification n)
	{
		debug = !debug;
	}
}
