using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour {

    public enum Command
    {
        Attack, Skills, Items, Defend, Initial
    }

    //UI stuff
    [SerializeField]
    private Button attackButton;
    [SerializeField]
    private Button defenseButton;
    [SerializeField]
    private Button skillsButton;
    public GameObject enemyPanel;
    public GameObject partyPanel;
    public GameObject commandPanel;
    public GameObject skillsCommandPanel;

    public List<Combatant> battleQueue { get; private set; }
    public List<Combatant> partyList { get; private set; }
    public List<Combatant> enemyList { get; private set; }
    public List<Combatant> defendList { get; private set; }
    public Combatant currentTurn { get; private set; }
    public Skill selectedSkill { get; set; }
    private List<Combatant> selectedList = null;

    private ScrollableList enemyScrollList;
    private ScrollableList partyScrollList;
    private int partyKOCount = 0;
    private int enemyKOCount = 0;
    private int selected = 0;
    private int turn = 0;
    private int round = 0;
    private bool changingStates;
    private BattleTurnState turnState = BattleTurnState.TurnStart;
    private Command command = Command.Initial;
    private Combatant attacker = null;
    private Combatant target = null;

    // Use this for initialization
    void Start () {
        battleQueue = new List<Combatant>();
        partyList = new List<Combatant>();
        enemyList = new List<Combatant>();
        defendList = new List<Combatant>();
        commandPanel.SetActive(false);
        Combatant p1 = new Combatant("Friendly1",100 ,10,55,10,10, true);
        Combatant p2 = new Combatant("Friendly2", 100, 10,10, 25, 10, true);
        Combatant e1 = new Combatant("Enemy1",150, 10, 20,10,10, false);
        Combatant e2 = new Combatant("Enemy2", 70, 10, 25, 10, 10, false);
        Combatant e3 = new Combatant("Enemy3", 70, 10, 32, 10, 10, false);

        battleQueue.Add(e1);
        battleQueue.Add(p1);
        battleQueue.Add(p2);
        battleQueue.Add(e2);
        battleQueue.Add(e3);

        enemyList.Add(e1);
        enemyList.Add(e2);
        enemyList.Add(e3);
        partyList.Add(p1);
        partyList.Add(p2);
        enemyPanel.SetActive(true);
        partyPanel.SetActive(true);
        enemyScrollList = enemyPanel.GetComponent<ScrollableList>();
        partyScrollList = partyPanel.GetComponent<ScrollableList>();

        attackButton.onClick.AddListener(() => {   
            ChangeState(BattleTurnState.Targeting);
            command = Command.Attack;
            Debug.Log("Attack Button Pressed"); });

        defenseButton.onClick.AddListener(() => 
        {   
            ChangeState(BattleTurnState.Targeting);
            command = Command.Defend;
            Debug.Log("Defend Button Pressed");});
        skillsButton.onClick.AddListener(() =>
        {
            ChangeState(BattleTurnState.SkillDecision);
            command = Command.Skills;
            Debug.Log("Skills Button Pressed");
        });
    }

    void Update () {
        //Debug.Log(turnState);
        
        /*Start of each turn for an individual character*/
        if(turnState == BattleTurnState.TurnStart)
        {
            /* This if statement is used for coroutines, so it doesnt start a coroutine loop */
            if (changingStates != true)
            {
                //Determines who is attacking and places them out of guard state if they are guarding
                currentTurn = battleQueue[turn];
                currentTurn.DecrementGuardDuration();
                enemyScrollList.UpdateInformation();//can optimize this part more
                partyScrollList.UpdateInformation();

                ChangeState(BattleTurnState.Decision);
            }

        }
        
        /*Choose between Attack, Defense, Items, etc as your command*/
        if (turnState == BattleTurnState.Decision)
        {
            if (changingStates != true)
            {
                //If friendly, command is set by panel
                if (currentTurn.friendly == true)
                {
                    commandPanel.SetActive(true);
                }
                //TODO
                //If enemy, command is set to attack or skill based on AI
                else
                { 
                    command = Command.Attack;
                    ChangeState(BattleTurnState.Targeting);
                }
            }
        }

        if(turnState == BattleTurnState.SkillDecision)
        {
            if(changingStates != true)
            {
                commandPanel.SetActive(false);
                skillsCommandPanel.SetActive(true);

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    skillsCommandPanel.SetActive(false);
                    commandPanel.SetActive(true);
                    ChangeState(BattleTurnState.Decision);
                }
            }
        }

        /*Enabled after selecting Attack, Skills, Items*/
        if(turnState == BattleTurnState.Targeting)
        {
            if (changingStates != true)
            {
                if (currentTurn.friendly == true)
                {
                    commandPanel.SetActive(false);
                    skillsCommandPanel.SetActive(false);
                    if (command == Command.Attack)
                    {
                        selectedList = enemyList;
                        target = selectedList[selected];
                        enemyScrollList.TargetHighlightItem(enemyList.IndexOf(target), UnityEngine.Color.cyan);

                    }
                    if (command == Command.Defend)
                    {
                        defendList = new List<Combatant>();
                        defendList.Add(battleQueue[turn]);
                        selectedList = defendList;
                        target = battleQueue[turn];
                        partyScrollList.TargetHighlightItem(partyList.IndexOf(target), UnityEngine.Color.yellow);
                    }
                    if(command == Command.Skills)
                    {
                        if(selectedSkill == null)
                        {

                        }
                        else if(selectedSkill.type == SkillType.Offensive)
                        {
                            selectedList = enemyList;
                            target = selectedList[selected];
                            enemyScrollList.TargetHighlightItem(enemyList.IndexOf(target), UnityEngine.Color.cyan);
                        }
                        else if(selectedSkill.type == SkillType.Heal)
                        {
                            selectedList = partyList;
                            target = selectedList[selected];
                            partyScrollList.TargetHighlightItem(partyList.IndexOf(target), UnityEngine.Color.yellow);
                        }
                    }
                }


                else
                {
                    //FindNonKOCombatant(EnemyList);
                    if (command == Command.Attack)
                    {

                        target = partyList[0];
                        if(target.hp < 1)
                        {
                            target = partyList[1];
                        }
                        ChangeState(BattleTurnState.Animate);
                    }

                }

                

                //Confirm target
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    ChangeState(BattleTurnState.Animate);
                    Debug.Log("Selected Target Confirmed: " + target.name);

                    if (command == Command.Attack)
                    {
                        enemyScrollList.UnhighlightItem(enemyList.IndexOf(target));
                    }
                    if(command == Command.Defend)
                    {
                        partyScrollList.UnhighlightItem(partyList.IndexOf(target));
                    }
                    if (command == Command.Skills)
                    {
                        if (selectedList == partyList)
                        {
                            partyScrollList.UnhighlightItem(partyList.IndexOf(target));
                        }
                        else if (selectedList == enemyList)
                        {
                            enemyScrollList.UnhighlightItem(enemyList.IndexOf(target));
                        }
                    }
                }

                //Select next target
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {

                    if (currentTurn.friendly == true && (command == Command.Attack || (command == Command.Skills && selectedSkill.type == SkillType.Offensive)))
                    {
                        FindNextNonKOCombatant(enemyList);
                        enemyScrollList.UnhighlightItem(enemyList.IndexOf(target));
                        target = selectedList[selected];
                    }
                    else if(command == Command.Skills && selectedSkill.type == SkillType.Heal)
                    {
                        FindNextNonKOCombatant(partyList);
                        partyScrollList.UnhighlightItem(partyList.IndexOf(target));
                        target = selectedList[selected];
                    }

                    Debug.Log("Next Target Selected: " + target.name);
                }

                //Select previous target
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {

                    if (currentTurn.friendly == true && (command == Command.Attack || (command == Command.Skills && selectedSkill.type == SkillType.Offensive)))
                    {
                        FindPreviousNonKOCombatant(enemyList);
                        enemyScrollList.UnhighlightItem(enemyList.IndexOf(target));
                        target = selectedList[selected];
                    }
                    else if (command == Command.Skills && selectedSkill.type == SkillType.Heal)
                    {
                        FindPreviousNonKOCombatant(partyList);
                        partyScrollList.UnhighlightItem(partyList.IndexOf(target));
                        target = selectedList[selected];
                    }

                        Debug.Log("Previous Target Selected: " + target.name);
                }

                if (Input.GetKeyDown(KeyCode.Escape)){
                    selected = 0;
                    if (currentTurn.friendly == true && command == Command.Attack)
                    {
                        enemyScrollList.UnhighlightItem(enemyList.IndexOf(target));
                    }
                    else if(command == Command.Defend){
                        partyScrollList.UnhighlightItem(partyList.IndexOf(target));
                    }
                    else if(command == Command.Skills)
                    {
                        if(selectedList == partyList)
                        {
                            partyScrollList.UnhighlightItem(partyList.IndexOf(target));
                        }
                        else if(selectedList == enemyList)
                        {
                            enemyScrollList.UnhighlightItem(enemyList.IndexOf(target));
                        }
                    }
                    if(command != Command.Skills)
                    {
                        ChangeState(BattleTurnState.Decision);
                    }
                    else
                    {
                        ChangeState(BattleTurnState.SkillDecision);
                    }
                   
                }
            }
        }

        if(turnState == BattleTurnState.Animate)
        {
            if(changingStates != true)
            {
                commandPanel.SetActive(false);
                attacker = battleQueue[turn];
                if (command == Command.Attack)
                {
                    Debug.Log("Attack Animate");
                    if (attacker.friendly == true)
                    {
                        //target = target;
                    }
                    else
                    {
                        //target = friendlyList[0];
                    }

                    target.ApplyAttackDamage(attacker);
                    CheckTargetIsKO(target);
                }

                else if(command == Command.Defend)
                {
                    Debug.Log("Defend Animate");
                    attacker.ApplyStatusEffect(new DefenseGuard());
                }

                else if(command == Command.Skills)
                {
                    if(selectedSkill.type == SkillType.Offensive)
                    {
                        Debug.Log("Skill Animate: " + selectedSkill.type + " by " + attacker.name);
                        target.ApplySkillAndEffect(selectedSkill, attacker);
                        CheckTargetIsKO(target);
                    }
                    else if (selectedSkill.type == SkillType.Heal)
                    {
                        Debug.Log("Skill Animate: " + selectedSkill.type + " by  " + attacker.name);
                        target.ApplySkillAndEffect(selectedSkill, attacker);
                    }
                }

                if (FindNonKOCombatant(enemyList) != false)
                {
                    ChangeState(BattleTurnState.TurnComplete);
                }
            }
        }
        
        if(turnState == BattleTurnState.TurnComplete)
        {
            if(changingStates != true)
            {
                turn++;
                currentTurn.ApplyHotHeal();
                currentTurn.ApplyDotDamage();
                currentTurn.DecrementStatusEffectDuration();
                command = Command.Initial;
                if (turn >= battleQueue.Count)
                {
                    turn = 0;
                    round++;
                    /*
                    foreach (Combatant c in battleQueue)
                    {
                        c.DecrementStatusEffectDuration();
                    }*/
                    
                }
                enemyScrollList.UpdateInformation();
                partyScrollList.UpdateInformation();
                Debug.Log("ROUND: " + round + " TURN: " + turn);
                ChangeState(BattleTurnState.TurnStart);
            }
        }

        if(turnState == BattleTurnState.Victory)
        {
            if(changingStates != true){
                Debug.Log("VICTORY");
            }
        }

        if(turnState == BattleTurnState.Defeat)
        {
            if(changingStates != true)
            {
                Debug.Log("DEFEAT");
            }
        }
    }

    public void ChangeState(BattleTurnState s)
    {
        if (changingStates != true)
        {
            StartCoroutine(changeTurnStateTo(s));
        }
    }

    IEnumerator changeTurnStateTo(BattleTurnState s)
    {
        changingStates = true;
        yield return new WaitForSeconds(.25f);
        turnState = s;
        Debug.Log("..." + turnState);

        if (s == BattleTurnState.Targeting)
        {
            if(command == Command.Attack)
            {
                FindNonKOCombatant(enemyList);
            }
            else if (command == Command.Skills && selectedSkill != null)
            {
                if (selectedSkill.type == SkillType.Heal)
                {
                    FindNonKOCombatant(partyList);
                }
                else if (selectedSkill.type == SkillType.Offensive)
                {
                    FindNonKOCombatant(enemyList);
                }
            }
        }

        changingStates = false;
    }

    /* Finds a NON KO combatant in the list used for targeting
   If no combatant is available, then either the battle ends in victory or defeat*/
    bool FindNonKOCombatant(List<Combatant> cl)
    {
        if (cl.Count <= 0)
        {
            return false;
        }
        selected = 0;
        while (cl[selected].isKO)
        {
            selected++;
            if (selected >= cl.Count)
            {
                return false;
            }
        }
        return true;
    }

    /* Finds next target */
    void FindNextNonKOCombatant(List<Combatant> cl)
    {
        if (cl.Count <= 0)
        {
            return;
        }
        selected++;
        if (selected >= selectedList.Count)
        {
            selected = 0;
        }

        while (cl[selected].isKO)
        {
            selected++;
            if (selected >= selectedList.Count)
            {
                selected = 0;
            }
        }
    }

    /* Finds previous target */
    void FindPreviousNonKOCombatant(List<Combatant> cl)
    {
        if (cl.Count <= 0)
        {
            return;
        }
        selected--;
        if (selected < 0)
        {
            selected = selectedList.Count - 1;
        }

        while (cl[selected].isKO)
        {
            selected--;
            if (selected < 0)
            {
                selected = selectedList.Count - 1;
            }
        }
    }



    public void CheckTargetIsKO(Combatant target)
    {
        if (target.isKO)
        {
            if (battleQueue.IndexOf(target) < battleQueue.IndexOf(attacker))
            {
                turn--;
            }
            Debug.Log(target.name + " KILLED");
            battleQueue.Remove(target);
            if (target.friendly == true)
            {
                partyScrollList.UpdateInformation();
                partyScrollList.KOItem(partyList.IndexOf(target));
                partyKOCount++;
                if (partyKOCount >= partyList.Count)
                {
                    Debug.Log(partyKOCount + "party KO");
                    ChangeState(BattleTurnState.Defeat);
                }
            }
            else
            {
                enemyScrollList.KOItem(enemyList.IndexOf(target));
                enemyScrollList.UpdateInformation();
                enemyKOCount++;
                if (enemyKOCount >= enemyList.Count)
                {
                    Debug.Log(enemyKOCount + "enemy KO");
                    ChangeState(BattleTurnState.Victory);
                }
            }
        }
    }
}
