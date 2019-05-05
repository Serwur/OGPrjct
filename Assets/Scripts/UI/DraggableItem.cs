using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{

    public float snapToSlotConstant = 120f; //distance 
    
    float smallestDistance = 1000f;

    Inventory inventory;

    void Start()
    {
        inventory = GameObject.Find("Background Inventory").GetComponent<Inventory>();
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
        Transform slotYouWantToOccupy = null;

        foreach (Transform slot in inventory.getAllSlots())
        {
            
            smallestDistance = Vector2.Distance(this.transform.position, slot.position);
            if (smallestDistance <= snapToSlotConstant && slot.childCount ==0) //looks for a slot that is the nearest
            {
                Debug.Log("Attach to slot");

                this.transform.position = slot.position; //snaps to a slot
                this.transform.SetParent(slot);


                
            }
            else if (smallestDistance <= snapToSlotConstant)
            {
                slotYouWantToOccupy = slot;
                Debug.Log("Wtf");
            }

        }
        if(smallestDistance > snapToSlotConstant || slotYouWantToOccupy.childCount != 0) //lub zajęte
        {
            Debug.Log("Go back to previous slot");
            this.transform.position = this.transform.parent.position; //snaps to a slot
        }
        else
        {
            Debug.Log("Wtf2");
        }

    }
}
