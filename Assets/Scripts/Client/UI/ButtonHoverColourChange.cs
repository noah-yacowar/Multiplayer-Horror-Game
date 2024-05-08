using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonHoverColourChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textMesh.color = Color.red; // Change the color to red when hovering
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textMesh.color = Color.white; // Restore the original color when not hovering
    }

    public void OnDisable()
    {
        textMesh.color = Color.white; // Restore the original color when not hovering
    }
}