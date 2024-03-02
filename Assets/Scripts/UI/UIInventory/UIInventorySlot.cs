using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Camera mainCamera;
    private Canvas parentCanvas;
    private Transform parentItem;
    private GridCursor gridCursor;
    private Cursor cursor;
    private GameObject draggedItem;

    [SerializeField] private UIInventoryBar inventoryBar = null;
    [SerializeField] private GameObject itemPrefab = null;
    [SerializeField] private int slotNumber = 0;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;

    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [HideInInspector] public bool isSelected = false;


    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
        EventHandler.DropSelectedItemEvent += DropSelectedItemAtMousePosition;
        EventHandler.RemoveSelectedItemFromInventoryEvent += RemoveSelectedItemFromInventory;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
        EventHandler.DropSelectedItemEvent -= DropSelectedItemAtMousePosition;
        EventHandler.RemoveSelectedItemFromInventoryEvent -= RemoveSelectedItemFromInventory;
    }


    /// <summary>
    /// sets this inventory slot item to be selected
    /// </summary>
    private void SetSelectedItem()
    {
        // Clear currently highlighted items
        inventoryBar.ClearHighlightOnInventorySlots();

        //highlight item on inventory bar
        isSelected = true;

        //Set highlighted inventory  slots
        inventoryBar.SetHighlightedInventorySlots();

        //Set use radius for cursors
        gridCursor.ItemUseGridRadius = itemDetails.itemUseGridRadius;
        cursor.ItemUseRadius = itemDetails.itemUseRadius;

        //If item requires a grid cursor then enable cursor
        if (itemDetails.itemUseGridRadius > 0)
        {
            gridCursor.EnableCursor();
        }
        else
        {
            gridCursor.DisableCursor();
        }

        if (itemDetails.itemUseRadius > 0f)
        {
            cursor.EnableCursor();
        }
        else
        {
            cursor.DisabelCursor();
        }

        //Set itemType
        gridCursor.SelectedItemType = itemDetails.itemType;
        cursor.SelectedItemType = itemDetails.itemType;

        //Set item selected in inventory
        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, itemDetails.itemCode);

        if (itemDetails.canBeCarried == true)
        {
            //Show player carrying item
            Player.Instance.ShowCarriedItem(itemDetails.itemCode);
        }
        else // show player carrying nothing
        {
            Player.Instance.ClearCarriedItem();
        }
    }



    private void ClearSelectedItem()
    {
        //clear cursors
        ClearCursors();

        //Clear currently highlighted items
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = false;

        //set no item selected in inventory
        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);


        //Clear player carrying item
        Player.Instance.ClearCarriedItem();
    }


    private void DropSelectedItemAtMousePosition()
    {
        if (itemDetails != null && isSelected)
        {
            // if valid cursor position
            if (gridCursor.CursorPositionIsValid)
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

                //creates item prefab at mouse position
                GameObject itemGameObject = Instantiate(itemPrefab,new Vector3(worldPosition.x, worldPosition.y - Settings.gridCellSize/2f, worldPosition.z), Quaternion.identity, parentItem);
                Item item = itemGameObject.GetComponent<Item>();
                item.ItemCode = itemDetails.itemCode;

                //remove item from players inventory
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);

                //if no more of item is selected
                if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
                {
                    ClearSelectedItem();
                }
            }

            
        }
    }

    private void RemoveSelectedItemFromInventory()
    {
        if (itemDetails != null && isSelected)
        {
            int itemCode = itemDetails.itemCode;

            // Remove item from players inventory
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode);

            // If no more of the item then clear selected
            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, itemCode) == -1)
            {
                ClearSelectedItem();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemDetails != null)
        {
            //Disable keyboard input
            Player.Instance.DisablePlayerInputAndResetMovement();

            //instantiate gameobject as dragged item
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

            //Get image for dragged item
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventorySlotImage.sprite;

            SetSelectedItem();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //move gameobject as dragged item
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Destroy game object as dragged item
        if (draggedItem != null)
        {
            Destroy(draggedItem);

            //If drag ends over inventory bar, get item drag is over and swap them
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                //get the slot number where the drag ended
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;

                //Swap inverntory items in inventory list
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                //also destroys tooltip when click finished
                DestroyInventoryTextBox();

                //Clear selected item
                ClearSelectedItem();

            }
            //else attempt to drop the item if it can be dropped
            else 
            {
                if (itemDetails.canBeDropped)
                {
                    DropSelectedItemAtMousePosition();
                }
            }

            //Re enables player input
            Player.Instance.EnablePlayerInput();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Populate textbox with item details
        if (itemQuantity != 0)
        {
            //Instantiate inventory text box
            inventoryBar.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryBar.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            // set item description
            string itemTypeDescription = InventoryManager.Instance.GetItemDescription(itemDetails.itemType);

            //populate text box
            inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

            //Set text box position according to inv bar position
            if (inventoryBar.IsInventoryBarPositionBottom)
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyInventoryTextBox();
    }

    public void DestroyInventoryTextBox()
    {
        if (inventoryBar.inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameobject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // if left click
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //if inventory slot is currently selected then deselect
            if (isSelected == true)
            {
                ClearSelectedItem();
            }
            else 
            {
                if (itemQuantity > 0)
                {
                    SetSelectedItem();
                }
            }
        }
    }

    private void ClearCursors()
    {
        //disablecursor
        gridCursor.DisableCursor();
        cursor.DisabelCursor();

        gridCursor.SelectedItemType = ItemType.none;
        cursor.SelectedItemType = ItemType.none; 


    }

    public void SceneLoaded()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
        mainCamera = Camera.main;
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
    }


}
