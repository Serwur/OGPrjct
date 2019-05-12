using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public string nameOfInventoryContainer = "Inventory";
    public string nameOfAttachedInventoryContainer = "Sword Inventory";

    Transform slotsOfInventory, slotsOfAttachedInvenotry; //slots in inventory
    List<Transform> allSlots = new List<Transform>(); //List of all slots

    public List<Transform> AllSlots { get => allSlots; set => allSlots = value; }

    public void Start()
    {
        slotsOfInventory = GameObject.Find(nameOfInventoryContainer).transform;
        slotsOfAttachedInvenotry = GameObject.Find(nameOfAttachedInventoryContainer).transform;

        // ------ Connection of two inventories ------

        foreach (Transform slot in slotsOfInventory)
        {
            AllSlots.Add(slot);
        }
        foreach (Transform slot in slotsOfAttachedInvenotry)
        {
            AllSlots.Add(slot);
            
        }
    }

    public List<Transform> getAllSlots()
    {
        return AllSlots;
        
    }

}
