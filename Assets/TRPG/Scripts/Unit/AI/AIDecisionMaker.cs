using FishNet;
using FishNet.Managing;
using FishNet.Object;
using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TRPG.Unit;
using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AIDecisionMaker : CoreNetworkBehaviour
    {
        public static int defaultActionPoint = 2;
        private const string AIUnitNameFormat = "AI Unit [{0}]";
        public static AIDecisionMaker Instance;

        [SerializeField] private List<Transform> spawnPointList;
        [SerializeField] private List<AIUnitController> selectedUnitList = new List<AIUnitController>();

        private Dictionary<AIUnitController, int> activeAIDictionary = new Dictionary<AIUnitController, int>();

        private int spawnPointIndex = 0;
        [SerializeField]
        private int currentUnitIndex;
        [SerializeField]
        private AIUnitController currentAIUnit;
        private NetworkManager networkManager;

        public bool IsAITurn { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // - Init all the ai enemies.
            selectedUnitList.ForEach(aiUnit => {
                Transform spawnPoint = spawnPointList[spawnPointIndex];
                AIUnitController activeUnit = Instantiate(aiUnit, spawnPoint.position, Quaternion.Euler(0, Random.Range(-180, 180), 0));
                ServerManager.Spawn(activeUnit.gameObject, Owner);
                activeUnit.gameObject.name = string.Format(AIUnitNameFormat, spawnPointIndex);
                RegisterAI(activeUnit);
                spawnPointIndex++;
            });
        }

        [Server]
        public virtual void StartAITurn()
        {
            ResetActiveUnits();
            currentUnitIndex = 0;

            IsAITurn = true;

            EvaluateCurrentUnit();
        }

        private void EvaluateCurrentUnit()
        {
            if (currentUnitIndex >= activeAIDictionary.Count)
            {
                EndAITurn();
                return;
            }

            currentAIUnit = activeAIDictionary.ElementAt(currentUnitIndex).Key;

            if (!HasEnoughPoint(currentAIUnit))
            {
                // Move to the next unit
                currentUnitIndex++;
                EvaluateCurrentUnit();
                return;
            }
            Debug.LogWarning("EvaluateCurrentUnit");
            // Start decision-making process for the current unit
            StartCoroutine(DelayBeforeDecide());
        }

        private IEnumerator DelayBeforeDecide()
        {
            yield return new WaitForSeconds(3f);

            if (currentAIUnit == null) yield break;

            if (currentAIUnit.AbilityController.HasActiveAbility || currentAIUnit.Motor.IsMoving)
            {
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(DelayBeforeDecide());
                yield break;
            }

            Decide();
        }

        [Server]
        public void Decide()
        {
            AIAbilityBehaviour bestAbility = null;
            int highestScore = int.MinValue;

            Debug.Log($"Evaluating abilities for: {currentAIUnit.gameObject.name}");

            foreach (AIAbilityBehaviour ability in currentAIUnit.AIAbilities.AbilityList)
            {
                if (HasEnoughPoint(currentAIUnit))
                {
                    int score = ability.Evaluate();
                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestAbility = ability;
                    }
                }
            }

            if (bestAbility != null)
            {
                currentAIUnit.AbilityController.TryStartAbility(bestAbility.Data);
                SpendActionPoint(currentAIUnit, bestAbility.Data.Cost);
            }

            StartCoroutine(HandleUnitAfterAction());
        }

        private IEnumerator HandleUnitAfterAction()
        {
            yield return new WaitForSeconds(1f);

            if (!HasEnoughPoint(currentAIUnit))
            {
                currentUnitIndex++;
            }

            EvaluateCurrentUnit();
        }

        [Server]
        public virtual void EndAITurn()
        {
            IsAITurn = false;
            TRPGGameManager.Instance.StartFirstPlayerTurn();
        }

        private void ResetActiveUnits()
        {
            activeAIDictionary.Keys.ToList().ForEach(key => activeAIDictionary[key] = defaultActionPoint);
        }

        [Server]
        public void SpendActionPoint(AIUnitController unit, ActionPointCost cost)
        {
            if (activeAIDictionary.ContainsKey(unit))
            {
                int finalPoint = activeAIDictionary[unit];

                switch (cost)
                {
                    case ActionPointCost.Half:
                        finalPoint -= 1;
                        break;
                    case ActionPointCost.Full:
                        finalPoint -= 2;
                        break;
                }

                finalPoint = Mathf.Max(finalPoint, 0);
                activeAIDictionary[unit] = finalPoint;
            }
        }

        public bool HasEnoughPoint(AIUnitController unit)
        {
            return activeAIDictionary.TryGetValue(unit, out int points) && points > 0;
        }

        public virtual void RegisterAI(AIUnitController unit)
        {
            if (!activeAIDictionary.ContainsKey(unit))
            {
                activeAIDictionary.Add(unit, defaultActionPoint);
            }
        }

        public virtual bool UnregisterAI(AIUnitController unit)
        {
            return activeAIDictionary.Remove(unit);
        }
    }
}