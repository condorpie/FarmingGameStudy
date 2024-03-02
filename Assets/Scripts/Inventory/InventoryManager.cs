using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int, ItemDetails> itemDetailsDictionary;

    private int[] selectedInventoryItem; // the index of the array is the inventory list, and the value is the item code

    public List<InventoryItem>[] inventoryLists;

    [HideInInspector] public int[] inventoryListCapacityIntArray; // the index of the array is the inventory list, and the value is the capacity of that inventory list

    [SerializeField] private SO_ItemList itemList = null;

    protected override void Awake()
    {
        base.Awake();

        //CreateInventory Lists
        CreateInventoryLists();

        //create item details dictionary
        CreateItemDetailsDictionary();

        //initialise selected inventory item arry
        selectedInventoryItem = new int[(int)InventoryLocation.count];

        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {
            selectedInventoryItem[i] = -1;
        }
    }


    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        //initialise inventory list capacity array
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        //initialise player inventory list capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }

    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();

        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    /// <summary>
    /// Add an item tot he inventory list for the inventory location and then destroy the game object
    /// <summary>
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {
        AddItem(inventoryLocation, item);

        Destroy(gameObjectToDelete);
    }

    /// <summary>
    /// Add an item tot he inventory list for the inventory location
    /// <summary>
    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode = item.ItemCode;
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        //check if inventory already holds item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        //send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    ///summary
    ///Add an item of type itemCode to the inventory list for the inventorylocation
    ///

    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        // check if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else 
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        //send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }




    /// <summary>
    /// Add item to the end of inventory
    /// <summary>
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1;
        inventoryList.Add(inventoryItem);

        //DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// Add item to position in the inventory
    /// <summary>
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity + 1;
        inventoryItem.itemQuantity = quantity;
        inventoryItem.itemCode = itemCode;
        inventoryList[position] = inventoryItem;

        //DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// Swap item at fromItem index with item at toItem Index in inventoryLocation inventory list
    /// <summary>

    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        //if fromItem index and toItemIndex are within the bounds of the list, not the same, and greater than or equal to 0
        if (fromItem < inventoryLists[(int)inventoryLocation].Count && toItem < inventoryLists[(int)inventoryLocation].Count
            && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];

            inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
            inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;

            //send the event that inventory has been updated
            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);

        }
    }

    /// <summary>
    /// Clear the selected inv item for inventoryLocation
    /// <summary>
    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }


    /// <summary>
    /// Find if an item is already in the inventory. Returns the item position
    /// in the inventory list, or -1 if the item is not the inventory
    /// <summary>
    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemCode == itemCode)
            {
                return i;
            }

        }

        return -1;
    }


    public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);

        if (itemCode == -1)
        {
            return null;
        }
        else
        {
            return GetItemDetails(itemCode);
        }
           
    }


    //get the selected item for the inventorylocation or =1 if nothing is selected
    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }


    /// <summary>
    /// Returns the itemsDetails (from the SO_ItemList) for the itemCode, or null if the item code doesnt exist
    /// <summary>
    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;

        if (itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the item descryption for an item type - returns the item type description as a string for a given ItemType
    /// <summary>

    public string GetItemDescription(ItemType itemType)
    {
        string itemTypeDescription;
        switch (itemType)
        {
            case ItemType.Breaking_tool:
                itemTypeDescription = Settings.BreakingTool;
                break;

            case ItemType.Chopping_tool:
                itemTypeDescription = Settings.ChoppingTool;
                break;

            case ItemType.Hoeing_tool:
                itemTypeDescription = Settings.HoeingTool;
                break;

            case ItemType.Reaping_tool:
                itemTypeDescription = Settings.ReapingTool;
                break;

            case ItemType.Watering_tool:
                itemTypeDescription = Settings.WateringTool;
                break;

            case ItemType.Collecting_tool:
                itemTypeDescription = Settings.CollectingTool;
                break;

            default:
                itemTypeDescription = itemType.ToString();
                break;
        }

        return itemTypeDescription;
    }

    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        //Check if Inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList, itemCode, itemPosition);
        }

        //Send the event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity - 1;

        if (quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[position] = inventoryItem;
        }
        else
        {
            inventoryList.RemoveAt(position);
        }

    }

    /// <summary>
    /// Set the selected inventory item for inventoryLocation to itemCode
    /// <summary>
    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }



    //****** INVENTORY DEBUG METHOD*********
    //private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    //{
    //   foreach (InventoryItem inventoryItem in inventoryList)
    //    {
    //        Debug.Log("Item Decription:" + InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription + "    Item Quantity: " + inventoryItem.itemQuantity);
    //    }
    //    Debug.Log("******************************");
    //}

}
