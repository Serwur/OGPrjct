using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


//We have to also switch the itemsWeCurrenctlyHave items every time we move object
//You can make the currentarray public and put some random items there
//you also have to do a function of taking an item from 3d space and putting it to the inventory array
public class ItemsCurrentlyHave : MonoBehaviour
{
    public static ItemsCurrentlyHave Instance;

    public Transform backgroundInventory;//UI inventory member
    public Transform descriptions; //UI inventory member

    private bool isInventoryOpen = false;


    /*
    public itemInInventory ActivationalItemCurrentlyHave { get; set; } = null;
    public itemInInventory PassivalItemCurrentlyHave { get; set; } = null;
    public itemInInventory[] ItemsWeCurrenctlyHave { get; set; } = new itemInInventory[8];
    */
    /*
    [SerializeField] public GameObject ActivationalItemObject { get; set; }
    [SerializeField] public GameObject[] AllObjectItemsInInventory { get; set; } = new GameObject[8];
    [SerializeField] public GameObject PassivallItemObject { get; set; }
    */

    public itemInInventory ActivationalItemCurrentlyHave = null;
    public itemInInventory PassivalItemCurrentlyHave = null;
    public itemInInventory[] ItemsWeCurrenctlyHave = new itemInInventory[8];

    [SerializeField] public GameObject ActivationalItemObject;
    [SerializeField] public GameObject[] AllObjectItemsInInventory = new GameObject[8];
    [SerializeField] public GameObject PassivallItemObject;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Transform backGroundInventory = transform.Find("Background Inventory");
        Transform inventoryObject = backGroundInventory.Find("Inventory");
        Transform swordInventoryObject = backGroundInventory.Find("Sword Inventory");
        Transform descriptions = transform.Find("Descriptions");

        for (int i = 0; i < 8; i++)
        {
            AllObjectItemsInInventory[i] = inventoryObject.GetChild(i).GetChild(0).gameObject;
        }

        PassivallItemObject = swordInventoryObject.GetChild(1).GetChild(0).gameObject;
        ActivationalItemObject = swordInventoryObject.GetChild(0).GetChild(0).gameObject;

        ReloadScriptableObjectsInList();


        backGroundInventory.gameObject.SetActive(false);
        descriptions.gameObject.SetActive(false);

    }

    private void Update()
    {
        //For debug

        if (Input.GetKeyDown(KeyCode.I))
        {
            
            ShowDraggableInventory(!isInventoryOpen);
        }

        
    }

    /// <summary>
    /// Opens the inventory.
    /// </summary>
    /// <param name="isTrue"></param>
    private void ShowDraggableInventory(bool isTrue)
    {
        backgroundInventory.gameObject.SetActive(isTrue);
        descriptions.gameObject.SetActive(isTrue);
        isInventoryOpen = isTrue;

        SpawnItemsInInventory();

        AnimatedWeaponExtensions.Instance.ReloadExtensionSprites();
    }


    /// <summary>
    /// Function that should be called every time we open an inventory.
    /// Switches all the neccessary item values to existing game objects.
    /// Simply takes itemsWeCurrenctlyHave and put it in allObjectItemsInInventory.
    /// </summary>
    void SpawnItemsInInventory()
    {
        for(int i = 0; i<8; i++)
        {
            if (ItemsWeCurrenctlyHave[i] == null)
            {
                Color colorOfItem = AllObjectItemsInInventory[i].GetComponent<Image>().color;
                colorOfItem.a = 0;
                AllObjectItemsInInventory[i].GetComponent<Image>().color = colorOfItem;
            }
            else
            {
                //If the item was previously transparent we have to:
                Color colorOfItem = AllObjectItemsInInventory[i].GetComponent<Image>().color;
                colorOfItem.a = 1;
                AllObjectItemsInInventory[i].GetComponent<Image>().color = colorOfItem;

                //AllObjectItemsInInventory[i].GetComponent<Image>().sprite = ItemsWeCurrenctlyHave[i].artwork; TO DO: DELETE THE COMMENT
                AllObjectItemsInInventory[i].GetComponent<DraggableItem>().item = ItemsWeCurrenctlyHave[i];
                AllObjectItemsInInventory[i].GetComponent<DraggableItem>().isThisItemTaken = true;
            }

        }

        if(PassivalItemCurrentlyHave == null)
        {
            Color colorOfItem = PassivallItemObject.GetComponent<Image>().color;
            colorOfItem.a = 0;
            PassivallItemObject.GetComponent<Image>().color = colorOfItem;
        }
        else
        {
            Debug.Log("color 1111111111");
            Color colorOfItem = PassivallItemObject.GetComponent<Image>().color;
            colorOfItem.a = 1;
            PassivallItemObject.GetComponent<Image>().color = colorOfItem;

            //PassivallItemObject.GetComponent<Image>().sprite = PassivalItemCurrentlyHave.artwork;
            PassivallItemObject.GetComponent<DraggableItem>().item = PassivalItemCurrentlyHave;

            PassivallItemObject.GetComponent<DraggableItem>().isThisItemTaken = true;
        }

        if (ActivationalItemCurrentlyHave == null)
        {
            Color colorOfItem = ActivationalItemObject.GetComponent<Image>().color;
            colorOfItem.a = 0;
            ActivationalItemObject.GetComponent<Image>().color = colorOfItem;
        }
        else
        {
            Color colorOfItem = ActivationalItemObject.GetComponent<Image>().color;
            colorOfItem.a = 1;
            ActivationalItemObject.GetComponent<Image>().color = colorOfItem;

            //ActivationalItemObject.GetComponent<Image>().sprite = ActivationalItemCurrentlyHave.artwork;
            ActivationalItemObject.GetComponent<DraggableItem>().item = ActivationalItemCurrentlyHave;

            ActivationalItemObject.GetComponent<DraggableItem>().isThisItemTaken = true;
        }

  
    }

    public void ReloadScriptableObjectsInList()
    {
        
        for(int i = 0; i< AllObjectItemsInInventory.Length; i++) {
            if(AllObjectItemsInInventory[i].GetComponent<DraggableItem>().isThisItemTaken == true)
            {
                ItemsWeCurrenctlyHave[i] = AllObjectItemsInInventory[i].GetComponent<DraggableItem>().item;
            }
            else{
                ItemsWeCurrenctlyHave[i] = null;
            }
        }   

        if(ActivationalItemObject.GetComponent<DraggableItem>().isThisItemTaken == true)
        {
            ActivationalItemCurrentlyHave = ActivationalItemObject.GetComponent<DraggableItem>().item;
        }
        else
        {
            ActivationalItemCurrentlyHave = null;
        }

        if (PassivallItemObject.GetComponent<DraggableItem>().isThisItemTaken == true)
        {
            PassivalItemCurrentlyHave = PassivallItemObject.GetComponent<DraggableItem>().item;
        }
        else
        {
            PassivalItemCurrentlyHave = null;
        }
    }

}
