using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatusEffect
{
    public string name { get; protected set; }
    public AffinityType affinity { get; protected set; }
    public StatusEffect statusEffect { get; protected set; }
    public float potency { get; protected set; }


    public StatusEffectType type { get; protected set; }
    public int duration { get; protected set; }
    public Combatant.stat modStat { get; protected set; }
    public int TickPerTurn { get; protected set; }

    

    public StatusEffect()
    {
    }

    public void SetTickPerTurn(int dmgOrHealDone)
    {
        float flatPotency = potency / 100;
        TickPerTurn = (int)Mathf.Round(dmgOrHealDone * flatPotency);
    }
}
 
public class DefenseGuard : StatusEffect
{ 
    public DefenseGuard()
    {
        this.name = "DefenseGuard";
        this.type = StatusEffectType.guard;
        this.modStat = Combatant.stat.defensePower;
        this.duration = 1;
        this.potency = 1.5f;
    }
}

public class Burn : StatusEffect
{
    public Burn()
    {
        this.name = "Burn";
        this.type = StatusEffectType.DOT;
        this.modStat = Combatant.stat.hp;
        this.duration = 1;
        this.potency = 30f;
        this.affinity = AffinityType.Fire;
    }
}

public class Bleed : StatusEffect
{
    public Bleed()
    {
        this.name = "Bleed";
        this.type = StatusEffectType.DOT;
        this.modStat = Combatant.stat.hp;
        this.duration = 2;
        this.potency = 20f;
        this.affinity = AffinityType.Physical;
    }
}

public class Regen : StatusEffect
{
    public Regen()
    {
        this.name = "Regen";
        this.type = StatusEffectType.HOT;
        this.modStat = Combatant.stat.hp;
        this.duration = 1;
        this.potency = 50f;
        this.affinity = AffinityType.Holy;
    }
}