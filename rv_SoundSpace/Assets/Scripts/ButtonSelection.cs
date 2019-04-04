using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSelection : MonoBehaviour
{
    public Button SisterButton;

    public void OnClick()
    {
        var ClickedButton = EventSystem.current.currentSelectedGameObject;
        Transform ClickedButtonisSelected;

        if(ClickedButton.transform.childCount > 0)
        {
            ClickedButtonisSelected = ClickedButton.transform.GetChild(0);

            var sisterIsSelected = SisterButton.transform.GetChild(0);

            //turn on or off isSelected panel of the button that you clicked on
            if (ClickedButtonisSelected.gameObject.activeInHierarchy)
            {
                ClickedButtonisSelected.gameObject.SetActive(false);
                sisterIsSelected.gameObject.SetActive(true);
            }
            else
            {
                ClickedButtonisSelected.gameObject.SetActive(true);
                sisterIsSelected.gameObject.SetActive(false);
            }
        }

       
    }
}
