using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CreditsAssembler
	{
		public static IEnumerable<CreditsEntry> AllCredits()
		{
			List<CreditsEntry> testers = new List<CreditsEntry>();
			yield return new CreditRecord_Space(200f);
			yield return new CreditRecord_Title("Credits_Developers".Translate());
			yield return new CreditRecord_Role("", "Tynan Sylvester");
			yield return new CreditRecord_Role("", "Piotr Walczak");
			yield return new CreditRecord_Role("", "Igor Lebedev");
			yield return new CreditRecord_Role("", "Matt Ritchie");
			yield return new CreditRecord_Role("", "Kenneth Ellersdorfer");
			yield return new CreditRecord_Role("", "Alex Mulford");
			yield return new CreditRecord_Role("", "Ben Rog-Wilhelm");
			yield return new CreditRecord_Role("", "Matt Quail");
			yield return new CreditRecord_Role("", "Nick Barrash");
			yield return new CreditRecord_Role("", "Jay Lemmon");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credit_MusicAndSound".Translate());
			yield return new CreditRecord_Role("", "Alistair Lindsay");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credit_GameArt".Translate());
			yield return new CreditRecord_Role("", "Oskar Potocki");
			yield return new CreditRecord_Role("", "Tynan Sylvester");
			yield return new CreditRecord_Role("", "Rhopunzel");
			yield return new CreditRecord_Role("", "Ricardo Tomé");
			yield return new CreditRecord_Role("", "Tamara Osborn");
			yield return new CreditRecord_Role("", "Mehran Iranloo");
			yield return new CreditRecord_Role("", "Kay Fedewa");
			yield return new CreditRecord_Role("", "Jon Larson");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credits_AdditionalDevelopment".Translate());
			yield return new CreditRecord_Role("", "Joe Gasparich");
			yield return new CreditRecord_Role("", "Gavan Woolery");
			yield return new CreditRecord_Role("", "David 'Rez' Graham");
			yield return new CreditRecord_Role("", "Ben Grob");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credit_TestLead".Translate());
			yield return new CreditRecord_Role("", "Pheanox");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credits_TitleCommunity".Translate());
			yield return new CreditRecord_Role("Credit_ModDonation".Translate(), "Zhentar").Compress();
			yield return new CreditRecord_Role("Credit_ModDonation".Translate(), "Haplo").Compress();
			yield return new CreditRecord_Role("Credit_ModDonation".Translate(), "iame6162013").Compress();
			yield return new CreditRecord_Role("Credit_ModDonation".Translate(), "Shinzy").Compress();
			yield return new CreditRecord_Role("Credit_WritingDonation".Translate(), "John Woolley").Compress();
			yield return new CreditRecord_Role("Credit_Moderator".Translate(), "ItchyFlea").Compress();
			yield return new CreditRecord_Role("Credit_Moderator".Translate(), "Ramsis").Compress();
			yield return new CreditRecord_Role("Credit_WikiMaster".Translate(), "ZestyLemons").Compress();
			yield return new CreditRecord_Title("Credits_TitleTester".Translate());
			testers.Add(new CreditRecord_Role("", "ItchyFlea").Compress());
			testers.Add(new CreditRecord_Role("", "Ramsis").Compress());
			testers.Add(new CreditRecord_Role("", "Haplo").Compress());
			testers.Add(new CreditRecord_Role("", "DubskiDude").Compress());
			testers.Add(new CreditRecord_Role("", "Harry Bryant").Compress());
			testers.Add(new CreditRecord_Role("", "ChJees").Compress());
			testers.Add(new CreditRecord_Role("", "Sneaks").Compress());
			testers.Add(new CreditRecord_Role("", "AWiseCorn").Compress());
			testers.Add(new CreditRecord_Role("", "Zero747").Compress());
			testers.Add(new CreditRecord_Role("", "Mehni").Compress());
			testers.Add(new CreditRecord_Role("", "XeoNovaDan").Compress());
			testers.Add(new CreditRecord_Role("", "_alphaBeta_").Compress());
			testers.Add(new CreditRecord_Role("", "TheDee05").Compress());
			testers.Add(new CreditRecord_Role("", "Vas").Compress());
			testers.Add(new CreditRecord_Role("", "JimmyAgnt007").Compress());
			testers.Add(new CreditRecord_Role("", "Gouda Quiche").Compress());
			testers.Add(new CreditRecord_Role("", "Drb89").Compress());
			testers.Add(new CreditRecord_Role("", "Jimyoda").Compress());
			testers.Add(new CreditRecord_Role("", "Semmy").Compress());
			testers.Add(new CreditRecord_Role("", "DianaWinters").Compress());
			testers.Add(new CreditRecord_Role("", "Goldenpotatoes").Compress());
			testers.Add(new CreditRecord_Role("", "Skissor").Compress());
			testers.Add(new CreditRecord_Role("", "Laos").Compress());
			testers.Add(new CreditRecord_Role("", "Evul").Compress());
			testers.Add(new CreditRecord_Role("", "Coenmjc").Compress());
			testers.Add(new CreditRecord_Role("", "MarvinKosh").Compress());
			testers.Add(new CreditRecord_Role("", "Gaesatae").Compress());
			testers.Add(new CreditRecord_Role("", "Letharion").Compress());
			testers.Add(new CreditRecord_Role("", "Skullywag").Compress());
			testers.Add(new CreditRecord_Role("", "Jaxxa").Compress());
			testers.Add(new CreditRecord_Role("", "ReZpawner").Compress());
			testers.Add(new CreditRecord_Role("", "tedvs").Compress());
			testers.Add(new CreditRecord_Role("", "RawCode").Compress());
			testers.Add(new CreditRecord_Role("", "Enystrom8734").Compress());
			testers.Add(new CreditRecord_Role("", "TeiXeR").Compress());
			foreach (CreditsEntry item in Reformat2Cols(testers))
			{
				yield return item;
			}
			yield return new CreditRecord_Role("", "Many other gracious volunteers!");
			yield return new CreditRecord_Space(200f);
			foreach (LoadedLanguage lang in LanguageDatabase.AllLoadedLanguages)
			{
				lang.LoadMetadata();
				if (lang.info.credits.Count > 0)
				{
					yield return new CreditRecord_Title("Credits_TitleLanguage".Translate(lang.FriendlyNameEnglish));
				}
				foreach (CreditsEntry item2 in Reformat2Cols(lang.info.credits))
				{
					yield return item2;
				}
			}
			bool firstModCredit = false;
			HashSet<string> allModders = new HashSet<string>();
			List<string> tmpModders = new List<string>();
			foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder.InRandomOrder())
			{
				if (mod.Official)
				{
					continue;
				}
				tmpModders.Clear();
				tmpModders.AddRange(mod.Authors);
				for (int num = tmpModders.Count - 1; num >= 0; num--)
				{
					tmpModders[num] = tmpModders[num].Trim();
					if (!allModders.Add(tmpModders[num].ToLowerInvariant()))
					{
						tmpModders.RemoveAt(num);
					}
				}
				if (tmpModders.Count <= 0)
				{
					continue;
				}
				foreach (string modder in tmpModders)
				{
					if (!firstModCredit)
					{
						yield return new CreditRecord_Title("Credits_TitleMods".Translate());
						firstModCredit = true;
					}
					yield return new CreditRecord_Role(mod.Name, modder).Compress();
				}
			}
			IEnumerable<CreditsEntry> Reformat2Cols(List<CreditsEntry> entries)
			{
				string crediteePrev = null;
				for (int i = 0; i < entries.Count; i++)
				{
					CreditsEntry langCred = entries[i];
					if (langCred is CreditRecord_Role creditRecord_Role)
					{
						if (crediteePrev != null)
						{
							yield return new CreditRecord_RoleTwoCols(crediteePrev, creditRecord_Role.creditee).Compress();
							crediteePrev = null;
						}
						else
						{
							crediteePrev = creditRecord_Role.creditee;
						}
					}
					else
					{
						if (crediteePrev != null)
						{
							yield return new CreditRecord_RoleTwoCols(crediteePrev, "").Compress();
							crediteePrev = null;
						}
						yield return langCred;
					}
				}
				if (crediteePrev != null)
				{
					yield return new CreditRecord_RoleTwoCols(crediteePrev, "").Compress();
				}
			}
		}
	}
}
