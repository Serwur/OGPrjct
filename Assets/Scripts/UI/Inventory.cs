using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public string nameOfInventoryContainer = "Inventory";
    public string nameOfAttachedInventoryContainer = "Sword Inventory";
    


    private Transform slotsOfInventory, slotsOfAttachedInvenotry; //slots in inventory
    private GameObject nameOfItem;
    private List<Transform> allSlots = new List<Transform>(); //List of all slots
    private List<Transform> inventorySlots = new List<Transform>(); //Only inventory


    public List<Transform> AllSlots { get => allSlots; set => allSlots = value; }
    public Transform SlotsOfInventory { get => slotsOfInventory; set => slotsOfInventory = value; }
    public Transform SlotsOfAttachedInvenotry { get => slotsOfAttachedInvenotry; set => slotsOfAttachedInvenotry = value; }
    public List<Transform> InventorySlots { get => inventorySlots; set => inventorySlots = value; }
    public GameObject Description { get; set; }
    public GameObject NameOfItem { get => nameOfItem; set => nameOfItem = value; }

    public void Awake()
    {
        SlotsOfInventory = GameObject.Find(nameOfInventoryContainer).transform;
        SlotsOfAttachedInvenotry = GameObject.Find(nameOfAttachedInventoryContainer).transform;


        Description = GameObject.Find("DescriptionOfItem");
        nameOfItem = GameObject.Find("NameOfItem");

        // ------ Connection of two inventories ------

        foreach (Transform slot in SlotsOfInventory)
        {
            AllSlots.Add(slot);
            inventorySlots.Add(slot);
        }
        foreach (Transform slot in SlotsOfAttachedInvenotry)
        {
            AllSlots.Add(slot);
        }


    }

    public List<Transform> getAllSlots()
    {
        return AllSlots;
        
    }


    public void PutItemToANewSlot(itemInInventory scritptableObjectItem)
    {
        GameObject newItem = new GameObject(scritptableObjectItem.name);
        //parent it and position

        

    }

}
