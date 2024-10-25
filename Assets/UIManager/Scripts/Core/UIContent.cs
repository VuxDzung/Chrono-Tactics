using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevOpsGuy.GUI
{
    public class UIContent : MonoBehaviour
    {
        protected string id;

        public string Id => id;

        public virtual void Set(string id)
        {
            this.id = id;
        }
    }
}