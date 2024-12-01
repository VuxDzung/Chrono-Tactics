using UnityEngine;
using Utils;

public class DeveloperController : M_Singleton<DeveloperController>
{
    [Header("References")]
    [SerializeField] private GameObject console;

    [Header("Input")]
    [SerializeField] private KeyCode consoleInput = KeyCode.Tilde;

    private void Update()
    {
        if (Input.GetKeyDown(consoleInput))
        {
            console.SetActive(!console.activeSelf);
        }
    }
}