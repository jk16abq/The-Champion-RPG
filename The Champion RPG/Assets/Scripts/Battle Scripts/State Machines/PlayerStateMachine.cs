﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Battle{
    public class PlayerStateMachine : MonoBehaviour
    {
        private BattleStateMachine BSM;
        public Stats.PlayerStats pStats;
        public BasePlayer player;
        public enum TurnState
        {
            PROCESSING,
            ADDTOLIST,
            WAITING,
            SELECTING,
            ACTION,
            DEAD
        }

        public TurnState currentState;
        //bool of player to see if they are alive
        public bool playerAlive = true;
        //variables for progress bar
        private float curCooldown;
        private float maxCooldown = 2f;
        //variables for health bar
        public Image ProgressBar;
        public Image HealthBar;
        //[Header("Defeated Player Transition")]
        //public Text defeat;
        //public Vector2 playerPosition;
        //public VectorValue playerStorage;
        //IeNumerator
        public GameObject EnemyToAttack;
        private bool actionStarted = false;
        private Vector2 startPosition;
        private float animSpeed = 10f;
        //defence power
        private float def_strength = 0;


    // Start is called before the first frame update
        void Start()
    {
            //gathers players stats into battle
            player.GetStats(pStats);
            startPosition = transform.position;
            //how fast does the progress bar charge
            curCooldown = Random.Range(0, 0.5f);
            BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
            currentState = TurnState.PROCESSING;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
        UpdateProgressBar();
        switch (currentState)
        {
            case (TurnState.PROCESSING):
                    for (int i = 0; i < pStats.PlayerSkills.Count; i++)
                    {
                        if (player.curStamina >= pStats.PlayerSkills[i].skillCost)
                        {
                            currentState = TurnState.ADDTOLIST;
                        }
                    }
                    break;

            case (TurnState.ADDTOLIST):
                    BSM.PlayersToManage.Add(this.gameObject);
                    currentState = TurnState.WAITING;
                break;

            case (TurnState.WAITING):
                    break;

            case (TurnState.ACTION):
                    StartCoroutine(TimeForAction());
                break;

            case (TurnState.DEAD):
                    if (!playerAlive)
                    {
                        return;
                    }
                    else
                    {
                        playerDefeated();
                        BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                        playerAlive = false;
                    }
                break;
        }
            //check if player is defending
            userDefending(def_strength);
    }
    
    void UpdateProgressBar()
    {
            //curCooldown = curCooldown + Time.deltaTime;
            //float calcCooldown = curCooldown / maxCooldown;
            if (player.curStamina < player.maxStamina)
            {
                player.curStamina = player.curStamina + (Time.deltaTime * ((player.agility * 4)+ (player.fighting * 5)));
            }
                float calcStamina = player.curStamina / player.maxStamina;
                ProgressBar.transform.localScale = new Vector2(Mathf.Clamp(calcStamina, 0, 1), ProgressBar.transform.localScale.y);
            //for(int i = 0; i < pStats.PlayerSkills.Count; i++)
            //{
            //    if (player.curStamina >= pStats.PlayerSkills[i].skillCost)
            //    {
            //        currentState = TurnState.ADDTOLIST;
            //    }
            //}
        //    if (player.curStamina >= player.maxStamina)
        //{
        //    currentState = TurnState.ADDTOLIST;
        //}
    }

        void UpdateHealthBar()
        {

            float healthBar = player.CurHP / player.MaxHP;
            HealthBar.transform.localScale = new Vector2(Mathf.Clamp(healthBar, 0, 1), HealthBar.transform.localScale.y);
            
        }

        //movement to the enemy and damage after some time
        private IEnumerator TimeForAction()
        {
            if (actionStarted)
            {
                yield break;
            }
            actionStarted = true;
            //perfrom defence skill if skill type is Att
            if (BSM.PerformList[0].choosenAttack.skillType == "Att")
            {
                //enemy movement to the player
                Vector2 enemyPosition = new Vector2(EnemyToAttack.transform.position.x - 1f, EnemyToAttack.transform.position.y);
            while (MoveToEnemy(enemyPosition))
            {
                yield return null;
            }
            //attack process
            yield return new WaitForSeconds(0.2f);
            //dealing damage
            
                DoDamage();
            }
            //perfrom defence skill if skill type is Def
            else if(BSM.PerformList[0].choosenAttack.skillType == "Def")
            {
                DoDefence();
            }
            //enemy movement back to the starting position
            Vector3 firstPosition = startPosition;
            while (MoveBack(firstPosition))
            {
                yield return null;
            }
            //action is removed from BSM
            BSM.PerformList.RemoveAt(0);
            //reset BSM
            if (BSM.battleStates != BattleStateMachine.PerformAction.WIN && BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
            {
                BSM.battleStates = BattleStateMachine.PerformAction.WAIT;
                //reset the enemy state
                curCooldown = 0f;
                currentState = TurnState.PROCESSING;
            }
            else
            {
                currentState = TurnState.PROCESSING;
            }
                //end the action stage
                actionStarted = false;

            }

        private void playerDefeated()
        {
            //take the object from the list and hide the attack panel
            BSM.PlayersInBattle.Remove(this.gameObject);
            BSM.PlayersToManage.Remove(this.gameObject);
            BSM.SelectPanel.SetActive(false);
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color32(225,0,0,255);
            //wait for a moment
            //yield return new WaitForSeconds(0.5f);
            //playerStorage.initialValue = playerPosition;
            //AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("House");
            //while (!asyncOperation.isDone)
            //{
            //    yield return null;
            //}


        }

        private bool MoveToEnemy(Vector3 distance)
        {
            return distance != (transform.position = Vector3.MoveTowards(transform.position, distance, animSpeed * Time.deltaTime));
        }

        private bool MoveBack(Vector3 distance)
        {
            return distance != (transform.position = Vector3.MoveTowards(transform.position, distance, animSpeed * Time.deltaTime));
        }

        //receive damage
        public void TakeDamage(float damageAmount)
        {
            float dmgAmount = damageAmount - def_strength;
            if(dmgAmount <= 0)
            {
                dmgAmount = 0;
            }
            player.CurHP -= dmgAmount;
            if(player.CurHP <= 0)
            {
                currentState = TurnState.DEAD;
            }
            def_strength = 0;
        }

        //do damage
        public void DoDamage()
        {
                BaseAttack attk = (BaseAttack)BSM.PerformList[0].choosenAttack;
                float calc_damage = player.strenght + attk.attackDamage;
                EnemyToAttack.GetComponent<EnemyStateMachine>().ReceiveDamage(calc_damage);
                def_strength = 0;
            player.curStamina = player.curStamina - attk.skillCost;
        }

        //do defence
        public void DoDefence()
        {
            BaseDef deff = (BaseDef)BSM.PerformList[0].choosenAttack;
            def_strength = deff.defenceStrength;
            player.curStamina = player.curStamina - deff.skillCost;
        }

        //change the object to defence mode after using defence skill, otherwise stay normal
        public void userDefending(float checkdef)
        {
            if (checkdef == 0)
            {
                this.gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            }
            else
            {
                this.gameObject.GetComponent<SpriteRenderer>().color = new Color32(100, 100, 255, 255);
            }
        }
    }
}
