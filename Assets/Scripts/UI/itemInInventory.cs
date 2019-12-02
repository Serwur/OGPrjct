
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item Of Isnventory", menuName = "Item")]
public class itemInInventory : ScriptableObject
{
    public new string name;
    public string description;
    public Sprite artwork;

    //is passive/is active/or just a member of inventory
    public ItemState steteOfItem;

}
