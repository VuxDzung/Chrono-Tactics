using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VisualGrid : MonoBehaviour
{ 
    public void Select()
    {
        gameObject.SetActive(true);
    }

    public void Deselect()
    {
        gameObject.SetActive(false);
    }

    public bool IsActive => gameObject.activeSelf;
}
