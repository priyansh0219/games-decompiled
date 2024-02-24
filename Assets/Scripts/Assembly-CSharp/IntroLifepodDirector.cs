using Story;
using UnityEngine;

public class IntroLifepodDirector : MonoBehaviour, IProtoTreeEventListener
{
	public GameObject[] toggleActiveObjects = new GameObject[5];

	public BoxCollider fireExtinguisherPickupVolume;

	public LightingController lightingController;

	public FMOD_CustomLoopingEmitter music;

	[AssertNotNull]
	public EscapePod escapePod;

	[AssertNotNull]
	public LiveMixin escapePodLiveMixin;

	public GameObject repairRadioNode;

	public static IntroLifepodDirector main;

	private static bool debugIntro;

	private bool playingMusic;

	public static bool IsActive { get; private set; }

	private void Awake()
	{
		main = this;
		if (debugIntro)
		{
			EnableIntroSequence();
		}
		else
		{
			ToggleActiveObjects(on: false);
		}
	}

	private void Update()
	{
		if (playingMusic)
		{
			music.Play();
		}
		else
		{
			music.Stop();
		}
	}

	private void OnDestroy()
	{
		if (main == this)
		{
			IsActive = false;
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (!(escapePodLiveMixin.GetHealthFraction() > 0.99f))
		{
			escapePod.ShowDamagedEffects();
			lightingController.SnapToState(2);
			uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod3Header"), new Color32(243, 201, 63, byte.MaxValue), 2f);
			uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod3Content"), new Color32(233, 63, 27, byte.MaxValue));
			uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod3Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		}
	}

	public void PlayMusic()
	{
		playingMusic = true;
	}

	public void EnableIntroSequence()
	{
		playingMusic = true;
		IsActive = true;
		escapePod.DamagePlayer();
		ToggleActiveObjects(on: true);
	}

	public void ConcludeIntroSequence()
	{
		playingMusic = false;
		lightingController.LerpToState(2, 5f);
		if ((bool)Player.main.playerAnimator)
		{
			Player.main.playerAnimator.SetBool("holster_extinguisher_first", value: true);
			Player.main.playerAnimator.SetBool("holding_fireextinguisher", value: false);
		}
		Invoke("ResetExtinguisherFirst", 4f);
		Invoke("OpenPDA", 4.1f);
		Invoke("ResetFirstUse", 8f);
		if ((bool)fireExtinguisherPickupVolume)
		{
			fireExtinguisherPickupVolume.enabled = false;
		}
		ToggleActiveObjects(on: false);
		uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod3Header"), new Color32(243, 201, 63, byte.MaxValue), 2f);
		uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod3Content"), new Color32(233, 63, 27, byte.MaxValue));
		uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod3Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
	}

	private void OpenPDA()
	{
		Player.main.GetPDA().Open(PDATab.Intro, null, OnClosePDA);
		Player.main.playerAnimator.SetBool("using_tool_first", value: true);
		Player.main.armsController.SetUsingPda(isUsing: true);
		StoryGoalManager storyGoalManager = StoryGoalManager.main;
		if ((bool)storyGoalManager)
		{
			storyGoalManager.OnGoalComplete("Trigger_PDAIntroBegin");
		}
	}

	private void OnClosePDA(PDA pda)
	{
		IsActive = false;
		StoryGoalManager storyGoalManager = StoryGoalManager.main;
		if ((bool)storyGoalManager)
		{
			storyGoalManager.OnGoalComplete("Trigger_PDAIntroEnd");
		}
	}

	private void ResetFirstUse()
	{
		if ((bool)Player.main.playerAnimator)
		{
			Player.main.playerAnimator.SetBool("using_tool_first", value: false);
		}
	}

	private void ResetExtinguisherFirst()
	{
		if ((bool)Player.main.playerAnimator)
		{
			Player.main.playerAnimator.SetBool("holster_extinguisher_first", value: false);
		}
	}

	private void ToggleActiveObjects(bool on)
	{
		for (int i = 0; i < toggleActiveObjects.Length; i++)
		{
			GameObject gameObject = toggleActiveObjects[i];
			if ((bool)gameObject)
			{
				gameObject.SetActive(on);
			}
		}
	}

	private void RepairRadio()
	{
		if ((bool)repairRadioNode)
		{
			repairRadioNode.SendMessage("PlayClip");
		}
	}
}
