using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{

    public float snapToSlotConstant = 120f; //distance 
    public itemInInventory item; //scritable object

    

    private float smallestDistance = 1000f;
    private Inventory inventory;
    private float timeStartedLerping;
    private Vector3 positionOfItem;
    private bool isMoving = false;
    


    public bool IsMoving { get => isMoving; set => isMoving = value; }
    public float TimeStartedLerping { get => timeStartedLerping; set => timeStartedLerping = value; }

    void Start()
    {
        
        inventory = GameObject.Find("Background Inventory").GetComponent<Inventory>();
        inventory.Description.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
        inventory.NameOfItem.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);




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
        this.transform.parent.SetAsLastSibling();           //Makes the item visible sets on bottom of the ui
        this.transform.parent.parent.SetAsLastSibling();

    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {


        //THIS HAPPENS WHEN YOU DRAG AN ITEM ABOVE SLOT

        
            if (item.isEquipable == true)
            {
            AttachOrDetachItem(inventory.AllSlots);
            }
            else if(item.isEquipable == false)
            {
            AttachOrDetachItem(inventory.InventorySlots);
            }




    }

    void AttachOrDetachItem(List<Transform> slotsOfItems)
    {
        Transform slotYouWantToOccupy = null;

        foreach (Transform slot in slotsOfItems)
        {



            smallestDistance = Vector2.Distance(this.transform.position, slot.position);
            if (smallestDistance <= snapToSlotConstant) //looks for a slot that is the nearest
            {
                this.transform.position = slot.position; //snaps to a slot

                if (slot.childCount != 0)
                {

                    slot.GetChild(0).GetComponent<DraggableItem>().TimeStartedLerping = Time.time;
                    slot.GetChild(0).GetComponent<DraggableItem>().IsMoving = true; //for child sets movement

                    slot.GetChild(0).transform.SetParent(this.transform.parent);
                    this.transform.SetParent(slot.transform);

                }

                this.transform.SetParent(slot.transform); //this goes to new parent
            }
            else if (smallestDistance <= snapToSlotConstant)
            {
                slotYouWantToOccupy = slot;
            }

        }


        //IT HAPPENS WHEN YOU DRAG AN ITEM ON AIR

        if (smallestDistance > snapToSlotConstant) //lub zajęte
        {
            TimeStartedLerping = Time.time;
            Debug.Log("Go back to previous slot");
            IsMoving = true;

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
