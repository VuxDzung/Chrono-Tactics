using TRPG.Unit;
using UnityEngine;

namespace TRPG
{
    public class WinCondition : ScriptableObject
    {
        protected TRPGGameManager gameManager;

        public virtual void Setup(TRPGGameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public virtual bool IsPassed()
        {
            return false;
        }
    }
}