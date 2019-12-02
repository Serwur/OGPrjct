using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Item : MonoBehaviour
{
    public itemInInventory scritptableObjectItem;
    public Vector3 offsetOfUITextClick = new Vector3(1, 1, 0);

    private bool canPickUpTheItem = false; //when we are in correct distance from the object
    private bool isItemAlreadyPickedUp = false;

    private TextMeshProUGUI letterToDisplay;

    private GameObject letterWithBrackets;

    private Color colorOfLetter;
    float speedOfColorFading = 0.7f;

    private Transform draggableInventory;

    private int indexofInventoryToAddThisItemTo = 0;
    // Start is called before the first frame update
    void Start()
    {
        letterWithBrackets = GameObject.Find("Main Canvas").GetComponent<UIHolder>().ClickEOverCollectableItem;

        letterToDisplay = letterWithBrackets.transform.GetComponent<TextMeshProUGUI>();
        Color startingColor = letterToDisplay.color;
        startingColor.a = 0;
        letterToDisplay.color = startingColor;

        draggableInventory = GameObject.Find("Main Canvas").transform.Find("Canvas - Draggable Menu");

    }

    private void Update()
    {
        if (canPickUpTheItem == true)
        {
            colorOfLetter = letterToDisplay.color;
            if (colorOfLetter.a < 1)
            {
                colorOfLetter.a += Time.deltaTime * speedOfColorFading;
                letterToDisplay.color = colorOfLetter;

            }
        }
        else
        {
            if (colorOfLetter.a > 0 )
            {
                colorOfLetter = letterToDisplay.color;
                colorOfLetter.a -= Time.deltaTime * speedOfColorFading;
                letterToDisplay.color = colorOfLetter;

                if(colorOfLetter.a <= 0.05f)
                {
                    colorOfLetter.a = 0;
                    letterToDisplay.color = colorOfLetter;
                    letterWithBrackets.SetActive(false);

                }

            }
        }
    }


    public void GetItem()
    {
        //put item in a list and when we open the inventory, we spawn the items
        Debug.Log("GetItem()");
        
        if(scritptableObjectItem.steteOfItem != ItemState.NORMAL_INVENTORY)
        {
            Debug.Log("Get Not Normal object");
            if (CheckIfUsingInventoryIsEmpty() == true)
            {
                //Dodaj do inventory
                if (scritptableObjectItem.steteOfItem == ItemState.ACTIVATIONAL)
                {
                    draggableInventory.GetComponent<ItemsCurrentlyHave>().ActivationalItemCurrentlyHave = scritptableObjectItem;
                }
                else
                {
                    draggableInventory.GetComponent<ItemsCurrentlyHave>().PassivalItemCurrentlyHave = scritptableObjectItem;
                }
            }
            else
            {
                Debug.Log("Get Normal object");

                if ( CheckIfNormalInventoryIsFull() == false)
                {
                    //Dodaj do inventory
                    draggableInventory.GetComponent<ItemsCurrentlyHave>().ItemsWeCurrenctlyHave[indexofInventoryToAddThisItemTo] = scritptableObjectItem;

                }
                else
                {
                    Debug.Log("No space");
                    //powiadomienie ze nie ma wolnego miejsca
                }
            }
        }
        else
        {
            if (CheckIfNormalInventoryIsFull() == false)
            {
                draggableInventory.GetComponent<ItemsCurrentlyHave>().ItemsWeCurrenctlyHave[indexofInventoryToAddThisItemTo] = scritptableObjectItem;
                //Dodaj do inventory
            }
            else
            {
                Debug.Log("No space");
                //powiadomienie ze nie ma wolnego miejsca
            }
        }


    }

    /// <summary>
    /// Checking if slot for passive or active is empty or taken
    /// </summary>
    /// <returns></returns>
    bool CheckIfUsingInventoryIsEmpty()
    {
        if(scritptableObjectItem.steteOfItem == ItemState.ACTIVATIONAL)
        {
            if (draggableInventory.GetComponent<ItemsCurrentlyHave>().ActivationalItemCurrentlyHave != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if (scritptableObjectItem.steteOfItem == ItemState.PASSIVE)
        {
            if (draggableInventory.GetComponent<ItemsCurrentlyHave>().PassivalItemCurrentlyHave != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            Debug.LogError("You put a wrong item state!!!");
            return false;
        }
    }


    bool CheckIfNormalInventoryIsFull()
    {
        int numberOfSlot = 0;
        foreach (itemInInventory item in draggableInventory.GetComponent<ItemsCurrentlyHave>().ItemsWeCurrenctlyHave)
        {
            if(item == null)
            {
                indexofInventoryToAddThisItemTo = numberOfSlot;
                return false;
            }
            numberOfSlot++;
        }
        if (numberOfSlot == 8)
            return true;
        else return false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (isItemAlreadyPickedUp == false)
        {
            Debug.Log(other.name);
            if (other.tag == "Player")
            {
                canPickUpTheItem = true;

                letterWithBrackets.GetComponent<WorldUIFollow>().Offset = offsetOfUITextClick;
                letterWithBrackets.GetComponent<WorldUIFollow>().LookAt = transform.Find("Sprite");

                Debug.Log("Player IS in the Item zone");
                letterWithBrackets.SetActive(true);
                letterToDisplay.text = "(E)";

                if (Input.GetKeyDown(KeyCode.O))
                {
                    GetItem();
                    DestroyObjectOutOf3DSpace();
                }

            }
        }

    }

    void DestroyObjectOutOf3DSpace()
    {
        //Vanishes the E letter here
        isItemAlreadyPickedUp = true;
        canPickUpTheItem = false;
        transform.Find("Sprite").gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {

        if (isItemAlreadyPickedUp == false)
        {
            if (other.gameObject.name == "Player")
            {
                Debug.Log("Player is NOT in the Item zone");
                canPickUpTheItem = false;

            }
        }
    }

    

}
