using System.Collections.Generic;
using System.Text;
using FMOD.Studio;
using FMODUnity;
using Gendarme;
using UnityEngine;

namespace UWE
{
	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public class FreezeTime
	{
		public enum Id
		{
			None = 0,
			TextInput = 1,
			PDA = 2,
			HardcoreGameOver = 3,
			DevBeaconPanel = 4,
			IngameMenu = 5,
			FeedbackPanel = 6,
			WaitScreen = 7,
			Save = 8,
			Quit = 9,
			ApplicationFocus = 10
		}

		public class Info
		{
			public string bus;

			public bool pauseMusic;

			public string eventPath;

			public string paramName;
		}

		public delegate void OnFreeze();

		public delegate void OnUnfreeze();

		private struct Freezer
		{
			public Id id;

			public float value;

			public readonly Info info;

			public bool hasEvent;

			public EventInstance eventInstance;

			public PARAMETER_ID eventParamId;

			public Freezer(Id id, float value)
			{
				this.id = id;
				this.value = value;
				info = freezerInfo[id];
				hasEvent = InitEvent(info.eventPath, info.paramName, out eventInstance, out eventParamId);
				if (hasEvent)
				{
					eventInstance.start().CheckResult();
				}
			}

			public void UpdateEvent()
			{
				if (hasEvent)
				{
					eventInstance.setParameterByID(eventParamId, value).CheckResult();
				}
			}

			public void ReleaseEvent()
			{
				if (hasEvent)
				{
					UpdateEvent();
					hasEvent = false;
					eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE).CheckResult();
					eventInstance.release().CheckResult();
					eventInstance.clearHandle();
				}
			}
		}

		private static Dictionary<Id, Info> freezerInfo = new Dictionary<Id, Info>
		{
			{
				Id.None,
				new Info()
			},
			{
				Id.TextInput,
				new Info
				{
					bus = null,
					pauseMusic = false
				}
			},
			{
				Id.PDA,
				new Info
				{
					bus = "bus:/master/SFX_for_pause/PDA_pause",
					pauseMusic = false,
					eventPath = "event:/bz/ui/pda/pda_pause_snapshot",
					paramName = "pda_pause"
				}
			},
			{
				Id.HardcoreGameOver,
				new Info
				{
					bus = "bus:/master/SFX_for_pause",
					pauseMusic = false
				}
			},
			{
				Id.DevBeaconPanel,
				new Info
				{
					bus = "bus:/",
					pauseMusic = true
				}
			},
			{
				Id.IngameMenu,
				new Info
				{
					bus = "bus:/master/SFX_for_pause",
					pauseMusic = false
				}
			},
			{
				Id.FeedbackPanel,
				new Info
				{
					bus = "bus:/",
					pauseMusic = true
				}
			},
			{
				Id.WaitScreen,
				new Info
				{
					bus = null,
					pauseMusic = false
				}
			},
			{
				Id.Save,
				new Info
				{
					bus = null,
					pauseMusic = false
				}
			},
			{
				Id.Quit,
				new Info
				{
					bus = null,
					pauseMusic = false
				}
			},
			{
				Id.ApplicationFocus,
				new Info
				{
					bus = null,
					pauseMusic = true
				}
			}
		};

		public static OnFreeze onFreeze;

		public static OnUnfreeze onUnfreeze;

		private static float cachedTimeScale = 1f;

		private static List<Freezer> freezers = new List<Freezer>();

		public static bool PleaseWait
		{
			get
			{
				if (!Contains(Id.Save))
				{
					return Contains(Id.Quit);
				}
				return true;
			}
		}

		public static void Set(Id id, float value)
		{
			value = Mathf.Clamp01(value);
			int index = GetIndex(id);
			if (value > 0f)
			{
				if (index < 0)
				{
					bool num = freezers.Count == 0;
					if (num)
					{
						cachedTimeScale = Time.timeScale;
					}
					Freezer item = new Freezer(id, value);
					freezers.Add(item);
					freezers.Sort(PriorityComparer);
					UpdateTimeScale();
					UpdateBusPause(item.info.bus);
					item.UpdateEvent();
					if (num && onFreeze != null)
					{
						onFreeze();
					}
				}
				else
				{
					Freezer value2 = freezers[index];
					if (!Mathf.Approximately(value2.value, value))
					{
						value2.value = value;
						freezers[index] = value2;
						UpdateTimeScale();
						UpdateBusPause(value2.info.bus);
						value2.UpdateEvent();
					}
				}
			}
			else if (index >= 0)
			{
				Freezer freezer = freezers[index];
				freezer.value = value;
				freezers.RemoveAt(index);
				UpdateTimeScale();
				UpdateBusPause(freezer.info.bus);
				freezer.ReleaseEvent();
				if (freezers.Count == 0 && onUnfreeze != null)
				{
					onUnfreeze();
				}
			}
		}

		public static void Begin(Id id)
		{
			Set(id, 1f);
		}

		public static void End(Id id)
		{
			Set(id, 0f);
		}

		public static Id GetTopmostId()
		{
			if (freezers.Count <= 0)
			{
				return Id.None;
			}
			return freezers[freezers.Count - 1].id;
		}

		public static bool HasFreezers()
		{
			return freezers.Count > 0;
		}

		public static bool Contains(Id id)
		{
			for (int i = 0; i < freezers.Count; i++)
			{
				if (freezers[i].id == id)
				{
					return true;
				}
			}
			return false;
		}

		public static bool ShouldPauseMusic()
		{
			for (int i = 0; i < freezers.Count; i++)
			{
				if (freezers[i].info.pauseMusic)
				{
					return true;
				}
			}
			return false;
		}

		public static void Deinitialize()
		{
			for (int num = freezers.Count - 1; num >= 0; num--)
			{
				End(freezers[num].id);
			}
			freezers.Clear();
		}

		private static void UpdateTimeScale()
		{
			float num = 0f;
			for (int i = 0; i < freezers.Count; i++)
			{
				float value = freezers[i].value;
				if (num < value)
				{
					num = value;
				}
			}
			Time.timeScale = Mathf.Lerp(cachedTimeScale, 0f, num);
		}

		private static void UpdateBusPause(string bus)
		{
			if (string.IsNullOrEmpty(bus))
			{
				return;
			}
			float num = 0f;
			for (int i = 0; i < freezers.Count; i++)
			{
				Freezer freezer = freezers[i];
				if (string.Equals(freezer.info.bus, bus) && num < freezer.value)
				{
					num = freezer.value;
				}
			}
			FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
			if (studioSystem.hasHandle() && studioSystem.getBus(bus, out var bus2).CheckResult() && bus2.hasHandle())
			{
				bool flag = Mathf.Approximately(num, 1f);
				if (flag)
				{
					bus2.setVolume(1f - num).CheckResult();
					bus2.setPaused(flag).CheckResult();
				}
				else
				{
					bus2.setPaused(flag).CheckResult();
					bus2.setVolume(1f - num).CheckResult();
				}
			}
		}

		private static bool InitEvent(string eventPath, string parameterName, out EventInstance eventInstance, out PARAMETER_ID parameterId)
		{
			if (!string.IsNullOrEmpty(eventPath) && !string.IsNullOrEmpty(parameterName))
			{
				try
				{
					EventDescription eventDescription = RuntimeManager.GetEventDescription(eventPath);
					if (eventDescription.createInstance(out eventInstance).CheckResult())
					{
						if (eventDescription.getParameterDescriptionByName(parameterName, out var parameter).CheckResult())
						{
							parameterId = parameter.id;
							return true;
						}
						eventInstance.release().CheckResult();
						eventInstance.clearHandle();
						UnityEngine.Debug.LogErrorFormat("Failed to get parameter description by name '{0}' for event '{1}'!", parameterName, eventPath);
					}
					else
					{
						UnityEngine.Debug.LogErrorFormat("Failed to create event '{0}' from description!", eventPath);
					}
				}
				catch (EventNotFoundException)
				{
					UnityEngine.Debug.LogErrorFormat("Event '{0}' not found!", eventPath);
				}
			}
			eventInstance = default(EventInstance);
			parameterId = default(PARAMETER_ID);
			return false;
		}

		private static int PriorityComparer(Freezer a, Freezer b)
		{
			return a.id.CompareTo(b.id);
		}

		private static int GetIndex(Id id)
		{
			for (int i = 0; i < freezers.Count; i++)
			{
				if (freezers[i].id == id)
				{
					return i;
				}
			}
			return -1;
		}

		public static string Debug()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("cachedTimeScale: {0}", cachedTimeScale);
			for (int i = 0; i < freezers.Count; i++)
			{
				Freezer freezer = freezers[i];
				stringBuilder.AppendFormat("\n{0} {1} {2} {3}", i, freezer.id, freezer.value, (freezer.info.bus != null) ? freezer.info.bus : "null");
			}
			stringBuilder.AppendFormat("\nTime.timeScale: {0}", Time.timeScale);
			return stringBuilder.ToString();
		}
	}
}
