using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum ItemState { ACTIVATIONAL, PASSIVE, NORMAL_INVENTORY, EMPTY};
public class DraggableItem : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{

    public float snapToSlotConstant = 120f; //distance 
    public itemInInventory item; //scritable object
    public bool isThisItemTaken = false; //is this really an item, not just an object pretanding to be an item
    public int numberOfObject = -1;

    public ItemState currentLocalizationOfItem = ItemState.NORMAL_INVENTORY;

    private float smallestDistance = 1000f;
    private Inventory inventory;
    private float timeStartedLerping;
    private Vector3 positionOfItem;
    private bool isMoving = false;

    private ItemsCurrentlyHave itemsCurrentlyHave;


    public bool IsMoving { get => isMoving; set => isMoving = value; }
    public float TimeStartedLerping { get => timeStartedLerping; set => timeStartedLerping = value; }

    void Awake()
    {
        SetUp();

    }

    void OnEnable()
    {
        MakeTheItemVisibleOrNot();
    }

    void SetUp()
    {
        itemsCurrentlyHave = GameObject.Find("Main Canvas").transform.Find("Canvas - Draggable Menu").GetComponent<ItemsCurrentlyHave>();

        inventory = GameObject.Find("Background Inventory").GetComponent<Inventory>();

        itemsCurrentlyHave.transform.Find("Descriptions").Find("DescriptionOfItem").Find("Text Area").Find("Text").gameObject.SetActive(false);
        itemsCurrentlyHave.transform.Find("Descriptions").Find("NameOfItem").Find("Text Area").Find("Text").gameObject.SetActive(false);

        currentLocalizationOfItem = transform.parent.GetComponent<TypeOfSlot>().typeOfField;
    }

    void MakeTheItemVisibleOrNot()
    {
        //When it has no item assigned - just disable ability to move it and make it invisible
        if (isThisItemTaken == false)
        {
            Color color = GetComponent<Image>().color;
            color.a = 0;
            GetComponent<Image>().color = color;
        }
        else
        {
            Color color = GetComponent<Image>().color;
            color.a = 1;
            GetComponent<Image>().color = color;
        }
    }

    void Update()
    {

        if(IsMoving == true)
        {
            
            transform.position = LinearInterpolation.LerpV3(this.transform.position, this.transform.parent.position, TimeStartedLerping, 0.7f);
            
            if(this.transform.position == this.transform.parent.position)
            {
                IsMoving = false;
            }
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Start Dragging");
        if (isThisItemTaken == true)
        {
        Debug.Log("isThisItemTaken = true");

            this.transform.parent.SetAsLastSibling();           //Makes the item visible sets on bottom of the ui
            this.transform.parent.parent.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        DropTheItem();

    }




    /// <summary>
    /// Happens whenever we drop an item that we were dragging on screen
    /// </summary>
    void DropTheItem()
    {

        Transform slotToSnap = FindTheDistanceToTheClosestSlot();
        DraggableItem itemOfSlotToSnap = slotToSnap.GetChild(0).GetComponent<DraggableItem>();

        ItemState typeOfSlotField = slotToSnap.GetComponent<TypeOfSlot>().typeOfField;

        if (Vector2.Distance(slotToSnap.position, transform.position) < snapToSlotConstant) //Try to Attach item to slot
        {
            //ChangeList(slotToSnap);

            //Situation: active item is in active slot. When we put this item to some inventory slot and we want to switch it with some passive item - it can't happen
            if (transform.parent.GetComponent<TypeOfSlot>().typeOfField == ItemState.ACTIVATIONAL && (itemOfSlotToSnap.item.steteOfItem != ItemState.ACTIVATIONAL && itemOfSlotToSnap.item.steteOfItem != ItemState.EMPTY) 
                || transform.parent.GetComponent<TypeOfSlot>().typeOfField == ItemState.PASSIVE && (itemOfSlotToSnap.item.steteOfItem != ItemState.PASSIVE && itemOfSlotToSnap.item.steteOfItem != ItemState.EMPTY))
            {
                LerpItemToSlot(transform.parent);
            }
            else if(typeOfSlotField == item.steteOfItem || //Check if it's the same state
                (typeOfSlotField == ItemState.NORMAL_INVENTORY && item.steteOfItem == ItemState.PASSIVE) || (typeOfSlotField == ItemState.NORMAL_INVENTORY && item.steteOfItem == ItemState.ACTIVATIONAL) ||
                (typeOfSlotField == ItemState.EMPTY && item.steteOfItem == ItemState.NORMAL_INVENTORY) || (typeOfSlotField == ItemState.EMPTY && item.steteOfItem == ItemState.PASSIVE) || (typeOfSlotField == ItemState.EMPTY && item.steteOfItem == ItemState.ACTIVATIONAL))
            {
                ChangeList(slotToSnap);

                Transform temporarySlot = transform.parent;
                LerpItemToSlot(slotToSnap); //snaps this to picked slot

                ChangeNumber(itemOfSlotToSnap);

                itemsCurrentlyHave.ReloadScriptableObjectsInList();

                itemOfSlotToSnap.LerpItemToSlot(temporarySlot); //snaps item from picked slot to this parent

                AnimatedWeaponExtensions.Instance.ReloadExtensionSprites(); //Reloads sprites of active and passive items on the bottom right corner of the screen
            }
            else //Returns item if it cant go to the picked slot
            {
                LerpItemToSlot(transform.parent);
            }
        }
        else //Get item to previous slot and don't attach it anywhere
        {
            LerpItemToSlot(transform.parent);
        }

        currentLocalizationOfItem = transform.parent.GetComponent<TypeOfSlot>().typeOfField;

    }

    void ChangeNumber(DraggableItem itemOfSlotToSnapDI)
    {
        int tempNumberOfSlot = numberOfObject;
        numberOfObject = itemOfSlotToSnapDI.numberOfObject;
        itemOfSlotToSnapDI.numberOfObject = tempNumberOfSlot;
    }

    /// <summary>
    /// Starts lerping this item to it's new parent
    /// </summary>
    /// <param name="slotToSnap"></param>
    /// <param name="itemToLerp"></param>
    public void LerpItemToSlot(Transform slotToSnap)
    {
        if(transform.parent != slotToSnap)
        transform.SetParent(slotToSnap);

        timeStartedLerping = Time.time;
        isMoving = true; //for child sets movement
    }



    /// <summary>
    /// Findst the closest slot after item drop
    /// </summary>
    Transform FindTheDistanceToTheClosestSlot() //it's just potential slot because the we check if it's distance is smaller than snapToSlotConstant and if it has rights to go to the slot(Active/passive/normal)
    {
        smallestDistance = 1000;
        Transform slotToSnapAfterDrop = null;

        foreach (GameObject oneOfItems in itemsCurrentlyHave.AllObjectItemsInInventory) //Checks if the closest is some from inventory
        {
            if (Vector2.Distance(this.transform.position, oneOfItems.transform.parent.position) < smallestDistance)
            {
                smallestDistance = Vector2.Distance(this.transform.position, oneOfItems.transform.parent.position);
                slotToSnapAfterDrop = oneOfItems.transform.parent;
            }
        }
        if (Vector2.Distance(this.transform.position, itemsCurrentlyHave.PassivallItemObject.transform.parent.position) < smallestDistance) //Checks if the closest is passive slot
        {
            smallestDistance = Vector2.Distance(this.transform.position, itemsCurrentlyHave.PassivallItemObject.transform.parent.position);
            slotToSnapAfterDrop = itemsCurrentlyHave.PassivallItemObject.transform.parent;
        }
        if (Vector2.Distance(this.transform.position, itemsCurrentlyHave.ActivationalItemObject.transform.parent.position) < smallestDistance)//Checks if the closest is active slot
        {
            smallestDistance = Vector2.Distance(this.transform.position, itemsCurrentlyHave.ActivationalItemObject.transform.parent.position);
            slotToSnapAfterDrop = itemsCurrentlyHave.ActivationalItemObject.transform.parent;
        }

       // Debug.Log("slotToSnapAfterDrop" + slotToSnapAfterDrop.name);
        return slotToSnapAfterDrop;
    }


    void ChangeList(Transform slot)
    {
        Debug.Log(currentLocalizationOfItem.ToString() + " to " + slot.GetComponent<TypeOfSlot>().typeOfField);
        if (currentLocalizationOfItem == ItemState.NORMAL_INVENTORY)
        {
            Debug.Log("PICKED ITEM = ItemState.NORMAL_INVENTORY");
            if (slot.GetComponent<TypeOfSlot>().typeOfField == ItemState.NORMAL_INVENTORY)
            {
                Debug.Log("1");

                GameObject temp = itemsCurrentlyHave.AllObjectItemsInInventory[numberOfObject];
                itemsCurrentlyHave.AllObjectItemsInInventory[numberOfObject] = itemsCurrentlyHave.AllObjectItemsInInventory[slot.GetChild(0).GetComponent<DraggableItem>().numberOfObject];
                itemsCurrentlyHave.AllObjectItemsInInventory[slot.GetChild(0).GetComponent<DraggableItem>().numberOfObject] = temp;
            }
            else if (slot.GetComponent<TypeOfSlot>().typeOfField == ItemState.PASSIVE)
            {
                Debug.Log("2");

                GameObject temp = itemsCurrentlyHave.AllObjectItemsInInventory[numberOfObject];
                itemsCurrentlyHave.AllObjectItemsInInventory[numberOfObject] = itemsCurrentlyHave.PassivallItemObject;
                itemsCurrentlyHave.PassivallItemObject = temp;
            }
            else if (slot.GetComponent<TypeOfSlot>().typeOfField == ItemState.ACTIVATIONAL)
            {
                Debug.Log("3");

                GameObject temp = itemsCurrentlyHave.AllObjectItemsInInventory[numberOfObject];
                itemsCurrentlyHave.AllObjectItemsInInventory[numberOfObject] = itemsCurrentlyHave.ActivationalItemObject;
                itemsCurrentlyHave.ActivationalItemObject = temp;
            }
        }
        else if (currentLocalizationOfItem == ItemState.ACTIVATIONAL)
        {
            if (slot.GetComponent<TypeOfSlot>().typeOfField == ItemState.NORMAL_INVENTORY)
            {
                Debug.Log("4");

                Debug.Log("ACTIVE TO NORMAL");
                GameObject temp = itemsCurrentlyHave.ActivationalItemObject;
                itemsCurrentlyHave.ActivationalItemObject = itemsCurrentlyHave.AllObjectItemsInInventory[slot.GetChild(0).GetComponent<DraggableItem>().numberOfObject];
                itemsCurrentlyHave.AllObjectItemsInInventory[slot.GetChild(0).GetComponent<DraggableItem>().numberOfObject] = temp;
            }
            else if (slot.GetComponent<TypeOfSlot>().typeOfField == ItemState.PASSIVE)
            {
                Debug.Log("5");

                GameObject temp = itemsCurrentlyHave.ActivationalItemObject;
                itemsCurrentlyHave.ActivationalItemObject = itemsCurrentlyHave.PassivallItemObject;
                itemsCurrentlyHave.PassivallItemObject = temp;
            }
        }
        else if (currentLocalizationOfItem == ItemState.PASSIVE)
        {
            if (slot.GetComponent<TypeOfSlot>().typeOfField == ItemState.NORMAL_INVENTORY)
            {
                Debug.Log("6");

                GameObject temp = itemsCurrentlyHave.PassivallItemObject;
                itemsCurrentlyHave.PassivallItemObject = itemsCurrentlyHave.AllObjectItemsInInventory[slot.GetChild(0).GetComponent<DraggableItem>().numberOfObject];
                itemsCurrentlyHave.AllObjectItemsInInventory[slot.GetChild(0).GetComponent<DraggableItem>().numberOfObject] = temp;
            }
            else if (slot.GetComponent<TypeOfSlot>().typeOfField == ItemState.ACTIVATIONAL)
            {
                Debug.Log("7");

                GameObject temp = itemsCurrentlyHave.PassivallItemObject;
                itemsCurrentlyHave.PassivallItemObject = itemsCurrentlyHave.ActivationalItemObject;
                itemsCurrentlyHave.ActivationalItemObject = temp;
            }

        }
    }


    public void ShowDescriptionAndName(bool isOn) // Its called when you hover over an item
    {

        inventory.NameOfItem.transform.GetChild(0).GetChild(2).gameObject.SetActive(isOn);
        inventory.Description.transform.GetChild(0).GetChild(2).gameObject.SetActive(isOn);
        inventory.Description.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = item.description;
        inventory.NameOfItem.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = item.name;

    }

}
