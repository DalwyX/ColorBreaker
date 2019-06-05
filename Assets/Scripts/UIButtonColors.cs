using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class UIButtonColors : MonoBehaviour,
    //IPointerClickHandler,
    //IDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{

    [SerializeField] Color basicButtonColor;
    [SerializeField] Color highlightedButtonColor;
    [SerializeField] Color basicTextColor;
    [SerializeField] Color highlightedTextColor;
    [SerializeField] bool boldOnHihglight;

    Image buttonImage;
    Text buttonText;

    private void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<Text>();
        buttonImage.color = basicButtonColor;
        buttonText.color = basicTextColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.color = highlightedButtonColor;
        buttonText.color = highlightedTextColor;
        buttonText.fontStyle = (boldOnHihglight) ? FontStyle.Bold : FontStyle.Normal;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.color = basicButtonColor;
        buttonText.color = basicTextColor;
        buttonText.fontStyle = FontStyle.Normal;
    }

}
