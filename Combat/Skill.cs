using UnityEngine;
using System.Collections;

public class Skill {

    public string name { get; protected set; }
    public SkillType type { get; protected set; }
    public AffinityType affinity { get; protected set; }
    public StatusEffect statusEffect { get; protected set; }
    public float potency { get; protected set; }

    public Skill()
    {
    } 

}

public class Fire : Skill
{
    public Fire()
    {
        this.name = "Fire";
        this.type = SkillType.Offensive;
        this.potency = 300f;
        this.statusEffect = new Burn();
        this.affinity = AffinityType.Fire;
    }
}

public class Heal : Skill
{
    public Heal()
    {
        this.name = "Heal";
        this.type = SkillType.Heal;
        this.potency = 100f;
        this.statusEffect = new Regen();
        this.affinity = AffinityType.Fire;
    }
}

public class Cut : Skill
{
    public Cut()
    {
        this.name = "Cut";
        this.type = SkillType.Offensive;
        this.potency = 200f;
        this.statusEffect = new Bleed();
        this.affinity = AffinityType.Physical;
    }
}