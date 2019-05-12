using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public string nameOfInventoryContainer = "Inventory";
    public string nameOfAttachedInventoryContainer = "Sword Inventory";

    private int numberOfInventoryContainer = 0;

    Transform slotsOfInventory, slotsOfAttachedInvenotry; //slots in inventory
    List<Transform> allSlots = new List<Transform>(); //List of all slots
    List<Transform> inventorySlots = new List<Transform>();

    public List<Transform> AllSlots { get => allSlots; set => allSlots = value; }
    public int NumberOfInventoryContainer { get => numberOfInventoryContainer; set => numberOfInventoryContainer = value; }
    public Transform SlotsOfInventory { get => slotsOfInventory; set => slotsOfInventory = value; }
    public Transform SlotsOfAttachedInvenotry { get => slotsOfAttachedInvenotry; set => slotsOfAttachedInvenotry = value; }
    public List<Transform> InventorySlots { get => inventorySlots; set => inventorySlots = value; }

    public void Awake()
    {
        SlotsOfInventory = GameObject.Find(nameOfInventoryContainer).transform;
        SlotsOfAttachedInvenotry = GameObject.Find(nameOfAttachedInventoryContainer).transform;

        // ------ Connection of two inventories ------

        foreach (Transform slot in SlotsOfInventory)
        {
            AllSlots.Add(slot);
            inventorySlots.Add(slot);
        }
        foreach (Transform slot in SlotsOfAttachedInvenotry)
        {
            AllSlots.Add(slot);
            NumberOfInventoryContainer++;   
        }
    }

    public List<Transform> getAllSlots()
    {
        return AllSlots;
        
    }

}
