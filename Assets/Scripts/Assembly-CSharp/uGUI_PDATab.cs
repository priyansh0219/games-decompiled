using Gendarme;
using UnityEngine;

public class uGUI_PDATab : MonoBehaviour
{
	protected uGUI_PDA pda;

	public virtual int notificationsCount => 0;

	protected virtual void Awake()
	{
	}

	public void Register(uGUI_PDA pda)
	{
		this.pda = pda;
	}

	public virtual void OnOpenPDA(PDATab tab, bool explicitly)
	{
	}

	public virtual void OnClosePDA()
	{
	}

	public virtual void Open()
	{
	}

	public virtual void Close()
	{
	}

	public virtual void OnWarmUp()
	{
		OnUpdate(isOpen: true);
		OnLateUpdate(isOpen: true);
	}

	public virtual void OnUpdate(bool isOpen)
	{
	}

	public virtual void OnLateUpdate(bool isOpen)
	{
	}

	public virtual void OnLanguageChanged()
	{
	}

	public virtual void OnBindingsChanged()
	{
	}

	public virtual uGUI_INavigableIconGrid GetInitialGrid()
	{
		return null;
	}

	[SuppressMessage("Gendarme.Rules.Naming", "AvoidRedundancyInMethodNameRule")]
	public virtual bool OnButtonDown(GameInput.Button button)
	{
		switch (button)
		{
		case GameInput.Button.UINextTab:
			pda.OpenTab(pda.GetNextTab());
			return true;
		case GameInput.Button.UIPrevTab:
			pda.OpenTab(pda.GetPreviousTab());
			return true;
		default:
			if (button == GameInput.button1)
			{
				ClosePDA();
				return true;
			}
			return false;
		}
	}

	protected void ClosePDA()
	{
		Player.main.GetPDA().Close();
	}
}
