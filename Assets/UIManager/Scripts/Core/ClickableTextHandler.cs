using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickableTextHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent onClick;

    private TMP_Text textMeshPro;
    

    private void Awake()
    {
        textMeshPro = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Get the index of the character clicked
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, eventData.position, Camera.main);

        if (linkIndex != -1) // Clicked on a link
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID(); // Retrieve the ID assigned in the <link> tag
            HandleLinkClick();
        }
    }

    private void HandleLinkClick()
    {
        onClick?.Invoke();
    }
}
