using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Verse;

namespace RimWorld
{

public class Apparel : ThingWithComps
{
    //Working vars
    private bool wornByCorpseInt;

    //Properties
    public Pawn Wearer
    {
        get
        {
            var apparelTracker = ParentHolder as Pawn_ApparelTracker;
            return apparelTracker != null ? apparelTracker.pawn : null;
        }
    }
    public bool WornByCorpse { get { return wornByCorpseInt; } }
    public string WornGraphicPath
    {
        get
        {
            if (StyleDef != null && !StyleDef.wornGraphicPath.NullOrEmpty())
                return StyleDef.wornGraphicPath;

            if (!def.apparel.wornGraphicPaths.NullOrEmpty())
                return def.apparel.wornGraphicPaths[thingIDNumber % def.apparel.wornGraphicPaths.Count];

            return def.apparel.wornGraphicPath;
        }
    }
    public override string DescriptionDetailed
	{
		get
		{
			string descr = base.DescriptionDetailed;
			if( WornByCorpse )
				descr += "\n" + "WasWornByCorpse".Translate();
			
			return descr;
		}
	}
    public override Color DrawColor
    {
        get
        {
            if (StyleDef != null && StyleDef.color != default)
                return StyleDef.color;

            return base.DrawColor;
        }
    }
    public Color? DesiredColor
    {
        get
        {
            var colorable = GetComp<CompColorable>();
            return colorable == null ? (Color?)null : colorable.DesiredColor;
        }
        set
        {
            var colorable = GetComp<CompColorable>();
            if (colorable != null)
                colorable.DesiredColor = value;
            else
                Log.Error("Tried setting " + nameof(Apparel) + "." + nameof(DesiredColor) + " without having " + nameof(CompColorable) + " comp!");
        }
    }


    public override string GetInspectStringLowPriority()
    {
        string str = base.GetInspectStringLowPriority();

        if (StyleDef != null)
        {
            if (!str.NullOrEmpty())
                str += "\n";
            str += "VariantOf".Translate().CapitalizeFirst() + ": " + def.LabelCap;
        }

        if( ModsConfig.BiotechActive )
        {
            if (!str.NullOrEmpty())
                str += "\n";

            str += "WearableBy".Translate() + ": " + def.apparel.developmentalStageFilter.ToCommaList().CapitalizeFirst();
        }

        return str;
    }

    public bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
    {
        if (!def.IsApparel)
            return false;

        if (!def.apparel.PawnCanWear(pawn, ignoreGender))
            return false;

        return true;
    }

    public void Notify_PawnKilled()
	{
		if( def.apparel.careIfWornByCorpse )
			wornByCorpseInt = true;

        foreach (var c in AllComps)
        {
            c.Notify_WearerDied();
        }
	}

	public void Notify_PawnResurrected()
	{
		wornByCorpseInt = false;
	}

    public override void Notify_ColorChanged()
    {
        if (Wearer != null)
            Wearer.apparel.Notify_ApparelChanged();
        
        base.Notify_ColorChanged();
    }

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look( ref wornByCorpseInt, "wornByCorpse" );
	}

	public virtual void DrawWornExtras()
	{
        var comps = AllComps;
        for(var i = 0; i < comps.Count; i++)
        {
            comps[i].CompDrawWornExtras();
        }
	}

	public virtual bool CheckPreAbsorbDamage(DamageInfo dinfo)
	{
        var comps = AllComps;
        for(var i = 0; i < comps.Count; i++)
        {
            comps[i].PostPreApplyDamage(dinfo, out bool absorbed);
            if(absorbed)
                return true;
        }

		return false;
	}

	public virtual bool AllowVerbCast(Verb verb)
	{
        var comps = AllComps;
        for(var i = 0; i < comps.Count; i++)
        {
            if(!comps[i].CompAllowVerbCast(verb))
                return false;
        }

		return true;
	}

	public virtual IEnumerable<Gizmo> GetWornGizmos()
    {
        var comps = AllComps;
        for( int i = 0; i < comps.Count; i++ )
        {
            var comp = comps[i];
            foreach (var g in comp.CompGetWornGizmosExtra())
            {
                yield return g;
            }
        }
	}

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        foreach (var s in base.SpecialDisplayStats())
        {
            yield return s;
        }

        RoyalTitleDef maxSatisfiedTitle = DefDatabase<FactionDef>.AllDefsListForReading
            .SelectMany(f => f.RoyalTitlesAwardableInSeniorityOrderForReading)
            .Where(t => t.requiredApparel != null && t.requiredApparel.Any(req => req.ApparelMeetsRequirement(def, false)))
            .OrderByDescending(t => t.seniority)
            .FirstOrDefault();
        
        if (maxSatisfiedTitle != null)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Apparel, 
                "Stat_Thing_Apparel_MaxSatisfiedTitle".Translate(), 
                maxSatisfiedTitle.GetLabelCapForBothGenders(), 
                "Stat_Thing_Apparel_MaxSatisfiedTitle_Desc".Translate(), 
                StatDisplayOrder.Thing_Apparel_MaxSatisfiedTitle, 
                null, 
                new [] { new Dialog_InfoCard.Hyperlink(maxSatisfiedTitle) });
        }
    }

    public override string GetInspectString()
	{
		var s = base.GetInspectString();

		if( WornByCorpse )
		{
			if( s.Length > 0 )
				s += "\n";

			s += "WasWornByCorpse".Translate();
		}

		return s;
	}

	public virtual float GetSpecialApparelScoreOffset()
	{
        var score = 0f;

        var comps = AllComps;
        for(var i = 0; i < comps.Count; i++)
        {
            score += comps[i].CompGetSpecialApparelScoreOffset();
        }

		return score;
	}
    
    [DebugOutput]
    private static void ApparelValidLifeStages()
    {
        List<TableDataGetter<ThingDef>> getters = new List<TableDataGetter<ThingDef>>();
        getters.Add(new TableDataGetter<ThingDef>("name", t => t.LabelCap ));
        getters.Add(new TableDataGetter<ThingDef>("valid life stage", t => t.apparel.developmentalStageFilter.ToCommaList() ));


        DebugTables.MakeTablesDialog<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where(t => t.IsApparel), getters.ToArray() );
    }
}
}
