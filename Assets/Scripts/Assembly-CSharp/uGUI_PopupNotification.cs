using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Gendarme;
using UnityEngine;

public class uGUI_PopupNotification : MonoBehaviour
{
	private enum Phase
	{
		None = 0,
		Zero = 1,
		In = 2,
		One = 3,
		Out = 4,
		Done = 5
	}

	[SuppressMessage("Gendarme.Rules.Performance", "AvoidLargeStructureRule")]
	public struct Entry
	{
		public string id;

		public string data;

		public float duration;

		public PopupNotificationSkin skin;

		public string title;

		public string text;

		public string controls;

		public Sprite sprite;

		public FMODAsset sound;
	}

	private const ManagedUpdate.Queue queueUpdate = ManagedUpdate.Queue.UpdateAfterInput;

	private const ManagedUpdate.Queue queueLayoutComplete = ManagedUpdate.Queue.UILayoutComplete;

	[AssertLocalization(1)]
	private const string encyPressPDAButtonFormat = "EncyNotificationPressPDAToView";

	[AssertLocalization]
	private const string encyTimeCapsuleTitleKey = "EncyNotificationTimeCapsule";

	[AssertLocalization]
	private const string encyEntryUnlockedTitleKey = "EncyNotificationEntryUnlocked";

	[AssertLocalization]
	private const string encyJournalUnlockedTitleKey = "EncyNotificationJournalUnlocked";

	[AssertLocalization]
	private const string resourceDiscoveredTitleKey = "MapRoomResourceDiscovered";

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public uGUI_PopupNotificationSkin[] skins;

	public float ringDuration = 30f;

	public float defaultDuration = 5f;

	[AssertNotNull]
	public FMODAsset soundEncyUnlock;

	[AssertNotNull]
	public FMODAsset soundTimeCapsuleUnlock;

	[AssertNotNull]
	public FMODAsset soundResourceDiscovered;

	[AssertNotNull]
	public Sprite timeCapsulePopup;

	[AssertNotNull]
	public Sprite resourceDiscoveredPopup;

	[AssertNotNull]
	public FMODAsset[] soundsWithVoice;

	private float timeIn;

	private float timeDuration;

	private float timeOut;

	private float start;

	private Phase phase;

	private float value;

	private List<Entry> queue = new List<Entry>();

	private Entry current;

	private uGUI_PopupNotificationSkin skin;

	private EventInstance eventInstance;

	private HashSet<Guid> soundsWithVoiceIDList = new HashSet<Guid>();

	public static uGUI_PopupNotification main { get; private set; }

	public bool isShowingMessage
	{
		get
		{
			if (phase >= Phase.In)
			{
				return phase <= Phase.Out;
			}
			return false;
		}
	}

	public string id => current.id;

	public string data => current.data;

	private void Awake()
	{
		if (main != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		main = this;
		canvasGroup.alpha = 0f;
		FMODAsset[] array = soundsWithVoice;
		foreach (FMODAsset fMODAsset in array)
		{
			soundsWithVoiceIDList.Add(new Guid(fMODAsset.id));
		}
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
		PDAEncyclopedia.onAdd = (PDAEncyclopedia.OnAdd)Delegate.Combine(PDAEncyclopedia.onAdd, new PDAEncyclopedia.OnAdd(OnEncyclopediaAdd));
		KnownTech.onAnalyze += OnAnalyze;
		uGUI_CanvasScaler.AddUIScaleListener(OnUIScaleChange);
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnDisable()
	{
		uGUI_CanvasScaler.RemoveUIScaleListener(OnUIScaleChange);
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
		PDAEncyclopedia.onAdd = (PDAEncyclopedia.OnAdd)Delegate.Remove(PDAEncyclopedia.onAdd, new PDAEncyclopedia.OnAdd(OnEncyclopediaAdd));
		KnownTech.onAnalyze -= OnAnalyze;
	}

	private void OnDestroy()
	{
		StopSound(immediately: true);
	}

	public void Enqueue(Entry entry)
	{
		if (entry.sound != null)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				if (queue[i].sound == entry.sound)
				{
					entry.sound = null;
					break;
				}
			}
		}
		queue.Add(entry);
	}

	public void Show(Entry entry)
	{
		current = entry;
		SetSkin(current.skin);
		skin.OnShow(current);
		timeIn = 0.25f;
		timeDuration = current.duration;
		timeOut = 0.25f;
		StopSound(immediately: true);
		FMODAsset sound = ((current.sound != null) ? current.sound : skin.defaultSound);
		StartSound(sound);
		if (phase == Phase.None)
		{
			phase = Phase.Zero;
		}
		start = PDA.time - timeIn * value;
	}

	public void ShowNext(Entry entry)
	{
		if (!IsPlayingVoicedSound())
		{
			Hide();
		}
		queue.Insert(0, entry);
	}

	public void Hide()
	{
		if (timeDuration < 0f)
		{
			timeDuration = 0f;
		}
		start = PDA.time - (timeIn + Mathf.Max(0f, timeDuration) + timeOut * (1f - value));
	}

	private void OnUpdate()
	{
		float num = PDA.time - start;
		Phase phase = ((this.phase != 0) ? ((num < 0f) ? Phase.Zero : ((num < timeIn) ? Phase.In : ((timeDuration < 0f || num < timeIn + timeDuration) ? Phase.One : ((!(num < timeIn + timeDuration + timeOut)) ? Phase.Done : Phase.Out)))) : Phase.None);
		if (this.phase != phase)
		{
			if ((this.phase <= Phase.Zero || this.phase == Phase.Done) && (phase == Phase.In || phase == Phase.One))
			{
				PlaySound(skin.soundSlideIn);
			}
			else if ((this.phase == Phase.In || this.phase == Phase.One) && phase == Phase.Out)
			{
				PlaySound(skin.soundSlideOut);
			}
			this.phase = phase;
			canvasGroup.alpha = (isShowingMessage ? 1f : 0f);
			if (phase == Phase.Out)
			{
				StopSound(immediately: false);
			}
			if (phase == Phase.Done)
			{
				_ = current;
				_ = current;
				current = default(Entry);
			}
		}
		float b = ((timeDuration < 0f) ? MathExtensions.Step(0f, timeIn, num, wrap: false) : MathExtensions.Trapezoid(0f, timeIn, timeDuration, timeOut, num, wrap: false));
		if (!Mathf.Approximately(value, b))
		{
			value = b;
			ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
		}
		if ((this.phase == Phase.None || this.phase == Phase.Done) && queue.Count > 0)
		{
			if (this.phase == Phase.None)
			{
				this.phase = Phase.Zero;
			}
			Entry entry = queue[0];
			queue.RemoveAt(0);
			Show(entry);
		}
		if (skin != null)
		{
			skin.OnUpdate();
		}
	}

	private void OnLayoutComplete()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
		if (skin != null)
		{
			skin.SetTransition(value);
		}
	}

	private void OnUIScaleChange(float scale)
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UILayoutComplete, OnLayoutComplete);
	}

	private void PlaySound(FMODAsset sound)
	{
		if (sound != null)
		{
			RuntimeManager.PlayOneShotAttached(sound.path, Player.main.gameObject);
		}
	}

	private void StartSound(FMODAsset sound)
	{
		if (sound != null)
		{
			eventInstance = RuntimeManager.CreateInstance(sound.path);
			eventInstance.start();
		}
	}

	private void StopSound(bool immediately)
	{
		if (eventInstance.isValid())
		{
			eventInstance.stop(immediately ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}

	public bool IsPlayingVoicedSound()
	{
		if (eventInstance.isValid())
		{
			eventInstance.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.PLAYING)
			{
				eventInstance.getDescription(out var description).CheckResult();
				description.getID(out var item).CheckResult();
				if (soundsWithVoiceIDList.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SetSkin(PopupNotificationSkin skin)
	{
		for (int i = 0; i < skins.Length; i++)
		{
			skins[i].SetVisible(i == (int)skin);
		}
		this.skin = skins[(int)skin];
	}

	public void OnEncyclopediaAdd(CraftNode node, bool verbose)
	{
		if (node != null && verbose)
		{
			Entry entry = default(Entry);
			entry.id = "PDAEncyclopediaTab";
			entry.data = node.id;
			entry.duration = defaultDuration;
			entry.skin = PopupNotificationSkin.Unlock;
			entry.controls = LanguageCache.GetButtonFormat("EncyNotificationPressPDAToView", GameInput.Button.PDA);
			entry.sound = soundEncyUnlock;
			Entry entry2 = entry;
			CraftNode craftNode = node.topmost as CraftNode;
			PDAEncyclopedia.EntryData.Kind kind = PDAEncyclopedia.EntryData.Kind.Encyclopedia;
			if (PDAEncyclopedia.GetEntryData(entry2.data, out var entryData))
			{
				kind = entryData.kind;
			}
			switch (kind)
			{
			case PDAEncyclopedia.EntryData.Kind.Encyclopedia:
				entry2.title = Language.main.Get("EncyNotificationEntryUnlocked");
				entry2.sprite = entryData.popup;
				entry2.sound = entryData.sound;
				break;
			case PDAEncyclopedia.EntryData.Kind.Journal:
				entry2.title = Language.main.Get("EncyNotificationJournalUnlocked");
				entry2.sprite = entryData.popup;
				entry2.sound = entryData.sound;
				entry2.skin = PopupNotificationSkin.Journal;
				break;
			case PDAEncyclopedia.EntryData.Kind.TimeCapsule:
				entry2.title = Language.main.Get("EncyNotificationTimeCapsule");
				entry2.sprite = timeCapsulePopup;
				entry2.sound = soundTimeCapsuleUnlock;
				break;
			}
			if (craftNode == null || craftNode == node)
			{
				entry2.text = Language.main.Get(entry2.data);
			}
			else
			{
				string empty = string.Empty;
				empty = ((entryData == null || entryData.kind != PDAEncyclopedia.EntryData.Kind.TimeCapsule) ? Language.main.Get($"Ency_{entry2.data}") : TimeCapsuleContentProvider.GetTitle(entryData.key));
				string arg = Language.main.Get($"EncyPath_{craftNode.id}");
				entry2.text = $"<color=#FFA621>{arg}</color>\n{empty}";
			}
			ShowNext(entry2);
		}
	}

	public void OnAnalyze(KnownTech.AnalysisTech analysis, bool verbose)
	{
		if (verbose)
		{
			Entry entry = default(Entry);
			entry.duration = defaultDuration;
			entry.skin = PopupNotificationSkin.Unlock;
			entry.title = Language.main.Get(analysis.unlockMessage);
			entry.text = Language.main.Get(analysis.techType.AsString());
			entry.sprite = analysis.unlockPopup;
			entry.sound = analysis.unlockSound;
			Entry entry2 = entry;
			Enqueue(entry2);
		}
	}

	public void OnResourceDiscovered(TechType techType)
	{
		Entry entry = default(Entry);
		entry.duration = defaultDuration;
		entry.skin = PopupNotificationSkin.Unlock;
		entry.title = Language.main.Get("MapRoomResourceDiscovered");
		entry.text = Language.main.Get(techType.AsString());
		entry.sprite = resourceDiscoveredPopup;
		entry.sound = soundResourceDiscovered;
		Entry entry2 = entry;
		Enqueue(entry2);
	}
}
