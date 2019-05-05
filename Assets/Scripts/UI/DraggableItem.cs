using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{

    public float snapToSlotConstant = 120f; //distance 
    public string nameOfInventoryContainer = "Inventory";
    public string nameOfAttachedInventoryContainer = "Sword Inventory";

    Transform slotsOfInventory, slotsOfAttachedInvenotry; //slots in inventory
    List<Transform> allSlots = new List<Transform>(); //List of all slots
    float smallestDistance = 1000f;
    

    public void Start()
    {
        slotsOfInventory = GameObject.Find(nameOfInventoryContainer).transform;
        slotsOfAttachedInvenotry = GameObject.Find(nameOfAttachedInventoryContainer).transform;

        // ------ Connection of two inventories ------

        foreach(Transform slot in slotsOfInventory)
        {
            allSlots.Add(slot);
        }
        foreach(Transform slot in slotsOfAttachedInvenotry)
        {
            allSlots.Add(slot);
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


        foreach (Transform slot in allSlots)
        {
            
            smallestDistance = Vector2.Distance(this.transform.position, slot.position);
            if (smallestDistance <= snapToSlotConstant && slot.childCount ==0) //looks for a slot that is the nearest
            {
                this.transform.position = slot.position; //snaps to a slot
                this.transform.SetParent(slot);

            }

        }
        if(smallestDistance > snapToSlotConstant)
        {
            this.transform.position = this.transform.parent.position; //snaps to a slot
        }

    }
}
