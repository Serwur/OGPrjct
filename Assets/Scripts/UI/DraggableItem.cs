using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{

    public float snapToSlotConstant = 120f; //distance 
    
    float smallestDistance = 1000f;

    Inventory inventory;

    float timeStartedLerping;
    Vector3 positionOfItem;
    bool isMoving = false;

    public bool IsMoving { get => isMoving; set => isMoving = value; }
    public float TimeStartedLerping { get => timeStartedLerping; set => timeStartedLerping = value; }

    void Start()
    {
        inventory = GameObject.Find("Background Inventory").GetComponent<Inventory>();
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
        Transform slotYouWantToOccupy = null;

        foreach (Transform slot in inventory.getAllSlots())
        {
            
            smallestDistance = Vector2.Distance(this.transform.position, slot.position);
            if (smallestDistance <= snapToSlotConstant) //looks for a slot that is the nearest
            {
                bool switchParents = false;
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
                Debug.Log("Wtf");
            }

        }
        if(smallestDistance > snapToSlotConstant || slotYouWantToOccupy.childCount != 0) //lub zajęte
        {
            TimeStartedLerping = Time.time;
            Debug.Log("Go back to previous slot");
            IsMoving = true;
            

            //this.transform.position = this.transform.parent.position; //snaps to a slot
        }
        else
        {
            Debug.Log("Wtf2");
        }

    }
}
