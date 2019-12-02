using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedWeaponExtensions : MonoBehaviour
{
    private Transform activeExtension; //animated
    private Transform passiveExtension;

    private Transform activeExtensionParent; //Parent of extensions (because the children change)
    private Transform passiveExtensionParent;

    private Transform draggableMenu;
    private ItemsCurrentlyHave itemsCurrentlyHave;

    public static AnimatedWeaponExtensions Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        activeExtension = transform.GetChild(0).GetChild(0);
        passiveExtension = transform.GetChild(1).GetChild(0);

        draggableMenu = transform.parent.Find("Canvas - Draggable Menu");
        itemsCurrentlyHave = draggableMenu.GetComponent<ItemsCurrentlyHave>();

        Transform swordInventory = draggableMenu.Find("Background Inventory").Find("Sword Inventory");
        activeExtensionParent = swordInventory.GetChild(0);
        passiveExtensionParent = swordInventory.GetChild(1);
    }

    private void OnEnable()
    {
        ReloadExtensionSprites();
    }

    /// <summary>
    /// Checks If we have some extensions on sword and reloads whetever the state opens
    /// </summary>
    public void ReloadExtensionSprites()
    {
        
            //To do: put some images
            //activeExtension.GetComponent<Image>().sprite = itemsCurrentlyHave.ActivationalItemCurrentlyHave.artwork;
            if(itemsCurrentlyHave.ActivationalItemCurrentlyHave != null)
            {
            activeExtension.gameObject.SetActive(true);
            activeExtension.GetComponent<Image>().color = itemsCurrentlyHave.ActivationalItemObject.GetComponent<Image>().color;
            }
            else
            {
            activeExtension.gameObject.SetActive(false);

            }



        //To do: put some images
        //passiveExtension.GetComponent<Image>().sprite = itemsCurrentlyHave.PassivalItemCurrentlyHave.artwork;
            if (itemsCurrentlyHave.PassivalItemCurrentlyHave != null)
            {
            passiveExtension.gameObject.SetActive(true);

            passiveExtension.GetComponent<Image>().color = itemsCurrentlyHave.PassivallItemObject.GetComponent<Image>().color;
            }
            else
            {
            passiveExtension.gameObject.SetActive(false);

            }
        
    }


}
