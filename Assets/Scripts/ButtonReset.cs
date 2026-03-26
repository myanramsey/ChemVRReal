using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonReset : MonoBehaviour
{
    public void DeselectButton()
    {
        // Check if the current selected object is this button and then deselect it
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}