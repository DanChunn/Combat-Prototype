using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Combatant {

    public enum stat
    {
        name, hp, tp, physicalPower, spiritPower, defensePower
    }

    //general info
    public string name { get; protected set; }
    public bool friendly { get; protected set; }
    public bool isKO { get; protected set; }

    //stats affected by base stats
    public float maxHP { get; protected set; }
    public float physicalPower { get; protected set; }
    public float spiritPower { get; protected set; }
    public float defensePower { get; protected set; }

    //changing stats
    public float hp { get; protected set; }

    public List<Skill> skillsList { get; protected set; }
    public Dictionary<StatusEffect, int> statusEffects { get; protected set; }
    protected Dictionary<StatusEffect, int> buffs = new Dictionary<StatusEffect, int>();
    protected Dictionary<StatusEffect, int> debuffs = new Dictionary<StatusEffect, int>();

    public Combatant() { }

    public Combatant(string _name, int _hp, float _physicalPower, float _spiritPower, int _defensePower, bool _friendly)
    {
        name = _name;
        maxHP = _hp;
        hp = maxHP;
        physicalPower = _physicalPower;
        spiritPower = _spiritPower;
        defensePower = _defensePower;
        friendly = _friendly;
        isKO = false;
        skillsList = new List<Skill>();
        statusEffects = new Dictionary<StatusEffect, int>();
        skillsList.Add(new Heal());
        skillsList.Add(new Fire());
        skillsList.Add(new Cut());
    }
    
    protected bool IsKO()
    {
        if (hp <= 0)
        {
            hp = 0;
            return true;
        }
        return false;
    }

    protected float ApplyStatusEffectsOfStat(float valueMod, Combatant.stat stat)
    {
        foreach (KeyValuePair<StatusEffect, int> effect in statusEffects)
        {
            if (effect.Key.modStat == stat)
            {
                valueMod *= effect.Key.potency;
            }
        }
        valueMod = (int)Mathf.Round(valueMod);
        return valueMod;
    }

    protected int CheckForNegativeAndRoundDamage(float dmgMod, float defMod)
    {
        int dmgDone = (int)Mathf.Round(dmgMod - defMod);
        if(dmgDone < 0)
        {
            dmgDone = 0;
        }
        return dmgDone;
    }

    protected float AssignSkillPowerValue(Combatant attacker, Skill skill)
    {
        AffinityType affinity = skill.affinity;
        float valueMod;

        if(affinity == AffinityType.Physical)
        {
            valueMod = attacker.physicalPower;
        }
        else
        {
            valueMod = attacker.spiritPower;
        }
        return valueMod;
    }

    protected Combatant.stat RetrieveStatMod(Skill skill)
    {
        Combatant.stat statMod;

        if (skill.affinity == AffinityType.Physical)
        {
            statMod = Combatant.stat.physicalPower;
        }
        else
        {
            statMod = Combatant.stat.spiritPower;
        }

        return statMod;
    }

    protected void ApplyHeal(int healDone)
    {
        hp += healDone;
        if(hp > maxHP)
        {
            hp = maxHP;
        }
    }
    
    protected void ApplyDamage(int dmgDone)
    {
        hp -= dmgDone;
        if(hp <= 0)
        {
            hp = 0;
            isKO = true;
        }
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (effect != null)
        {
            if (statusEffects.ContainsKey(effect))
            {
                statusEffects.Remove(effect);
                Debug.Log("Status Effect: " + effect.name + " overidden");
            }
            statusEffects.Add(effect, effect.duration);
            Debug.Log("Status Effect: " + effect.name + " for " + effect.duration+ " turns");
        }

    }

    public void ApplyStatusEffect(StatusEffect effect, int dmgDone)
    {
        if(effect != null)
        {
            if (statusEffects.ContainsKey(effect))
            {
                statusEffects.Remove(effect);
                Debug.Log("Status Effect: " + effect.name + " overidden");
            }
            effect.SetTickPerTurn(dmgDone);
            statusEffects.Add(effect, effect.duration);
            Debug.Log("Status Effect: " + effect.name + " tick per turn: " + effect.TickPerTurn + " for " + effect.duration + " turns");
        }
        
    }

    public void ApplyAttackDamage(Combatant combatant)
    {
        float defMod = defensePower;
        defMod = ApplyStatusEffectsOfStat(defMod, Combatant.stat.defensePower);
        float dmgMod = combatant.physicalPower;
        dmgMod = ApplyStatusEffectsOfStat(dmgMod, Combatant.stat.physicalPower);

        int dmgDone = CheckForNegativeAndRoundDamage(dmgMod, defMod);

        //TODO: if weak double the damage, if crit double the dmg

        ApplyDamage(dmgDone);

        Debug.Log(combatant.name + " has attacked " + name + " for " + dmgDone + " damage. " + combatant.physicalPower + " physical power vs " + defMod + " defense power");
        Debug.Log(name + " HP = " + hp);
    }

    public void ApplySkillAndEffect(Skill skill, Combatant attacker)
    {
        float defMod = defensePower;
        defMod = ApplyStatusEffectsOfStat(defMod, Combatant.stat.defensePower);

        float dmgMod = AssignSkillPowerValue(attacker, skill);
        Combatant.stat statMod = RetrieveStatMod(skill);
        dmgMod = ApplyStatusEffectsOfStat(dmgMod, statMod);

        float flatPotency = skill.potency / 100;
        dmgMod *= flatPotency;

        Debug.Log("Skill: " + skill.name + ", dmgMod: " + dmgMod + ", skillModPower : " + flatPotency);

        if (skill.type == SkillType.Offensive)
        { 
            int dmgDone = CheckForNegativeAndRoundDamage(dmgMod, defMod);
            ApplyDamage(dmgDone);
            ApplyStatusEffect(skill.statusEffect, dmgDone);
            Debug.Log(attacker.name + " has attacked " + name + " for " + dmgDone + " damage. " + AssignSkillPowerValue(attacker, skill) + " "+statMod +" vs " + defMod + " defense power");
            Debug.Log(name + " HP = " + hp);
        }
        else if (skill.type == SkillType.Heal)
        {
            int healDone = (int)Mathf.Round(dmgMod);
            ApplyHeal(healDone);
            ApplyStatusEffect(skill.statusEffect, healDone);
            Debug.Log(attacker.name + " has healed " + name + " for " + healDone + " damage. " + AssignSkillPowerValue(attacker, skill) + " " + statMod + " vs " + defMod + " defense power");
            Debug.Log(name + " HP = " + hp);
        }
    }



    public void DecrementStatusEffectDuration()
    {
        //Debug.Log(this.Name + " : Decrement Effects");
        List<StatusEffect> keys = new List<StatusEffect>(statusEffects.Keys);
        foreach (StatusEffect key in keys)
        {
            if (key.type != StatusEffectType.guard)
            {
                statusEffects[key]--;
                if (statusEffects[key] <= 0)
                {
                    Debug.Log("Status Effect: " + key.name + " removed.");
                    statusEffects.Remove(key);
                }
            }

        }
    }

    public void DecrementGuardDuration()
    {
        List<StatusEffect> keys = new List<StatusEffect>(statusEffects.Keys);

        foreach (StatusEffect key in keys)
        {
            if(key.type == StatusEffectType.guard)
            {
                statusEffects[key]--;
                if (statusEffects[key] <= 0)
                {
                    statusEffects.Remove(key);
                }
            }
        }
    }

    public void ApplyDotDamage()
    {
        List<StatusEffect> keys = new List<StatusEffect>(statusEffects.Keys);
        int dotDmg = 0;

        foreach (StatusEffect key in keys)
        {
            if(key.type == StatusEffectType.DOT)
            {
                //if key.affinity = weakness double
                dotDmg += key.TickPerTurn;
                int x = statusEffects[key] - 1;
                Debug.Log(key.name + " ticks for " + key.TickPerTurn + " " + x + ", turns left.");
            }
        }
        hp -= dotDmg;
        if(hp <= 0)
        {
            hp = 0;
        }
        Debug.Log(name + " HP: " + hp);
    }

    public void ApplyHotHeal()
    {
        List<StatusEffect> keys = new List<StatusEffect>(statusEffects.Keys);
        int hotHeal = 0;
        foreach (StatusEffect key in keys)
        {
            if (key.type == StatusEffectType.HOT)
            {
                hotHeal += key.TickPerTurn;
                int x = statusEffects[key] - 1;
                Debug.Log(key.name + " ticks for " + key.TickPerTurn + " " + x + ", turns left.");
            }
        }
        hp += hotHeal;
        if(hp >= maxHP)
        {
            hp = maxHP;
        }
        Debug.Log(name + " HP: " + hp);
    }

}
