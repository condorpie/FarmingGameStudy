using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCursor : MonoBehaviour
{

    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;

    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite redCursorSprite = null;
    [SerializeField] private SO_CropDetailsList so_cropDetailsList = null;

    private bool _cursorPositionIsValid = false;

    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }

    private int _itemUseGridRadius = 0;

    public int ItemUseGridRadius { get => _itemUseGridRadius; set => _itemUseGridRadius = value; }

    private ItemType _selectedItemType;

    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }

    public bool _cursorIsEnabled = false;

    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled = value; }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (CursorIsEnabled)
        {
            DisplayCursor();
        }
    }

    private Vector3Int DisplayCursor()
    {
        if (grid != null)
        {
            // Get grid popsition for cursor
            Vector3Int gridPosition = GetGridPositionForCursor();

            // Get grid position for player
            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            //Set cursor Sprite
            SetCursorValidity(gridPosition, playerGridPosition);

            //Get rect transform position for cursor
            cursorRectTransform.position = GetRectTransformPositionForCursor(gridPosition);

            return gridPosition;
        }
        else 
        {
            return Vector3Int.zero;
        }
    }

    private void SceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }

    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        SetCursorToValid();

        //check item use radius is valid
        if (Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius
            || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius)
        {
            SetCursorToInvalid();
            return;
        }

        // Get selected item details
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)
        {
            SetCursorToInvalid();
            return;
        }

        // Get grid property details at cursor position
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        if (gridPropertyDetails != null)
        {
            // Determine cursor validity based on inventory item selected and grid property details
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (!IsCursorValidForSeed(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.Commodity:
                    if (!IsCursorValidForCommodity(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                
                case ItemType.Watering_tool:
                case ItemType.Breaking_tool:
                case ItemType.Chopping_tool:
                case ItemType.Hoeing_tool:
                case ItemType.Reaping_tool:
                case ItemType.Collecting_tool:
                    if (!IsCursorValidForTool(gridPropertyDetails, itemDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.none:
                    break;

                case ItemType.count:
                    break;

                default:
                    break;
            }
        }
        else 
        {
            SetCursorToInvalid();
            return;
        }

    }

    private void SetCursorToInvalid()
    {
        cursorImage.sprite = redCursorSprite;
        CursorPositionIsValid = false;
    }

    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
    }

    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }

    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }

    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        //Switch on Tool
        switch (itemDetails.itemType)
        {

            // REAPABLE SCENARY CURSOR

            case ItemType.Hoeing_tool:
                if (gridPropertyDetails.isDiggable == true && gridPropertyDetails.daysSinceDug == -1)
                {
                    #region Meed to get any items at location so we can check if they are reapable
                    //Get world poisiton for cursor
                    Vector3 cursorWorldPosition = new Vector3(GetWorldPositionForCursor().x + 0.5f, GetWorldPositionForCursor().y + 0.5f, 0f);

                    //Get list of items at cursor location
                    List<Item> itemList = new List<Item>();

                    HelperMethods.GetComponentsAtBoxLocation<Item>(out itemList, cursorWorldPosition, Settings.cursorSize, 0f);
                    #endregion

                    //Loop through items found to see if any are reapable type - we are not going to let player dig where there are reapable scenary items
                    bool foundReapable = false;

                    foreach (Item item in itemList)
                    {
                        if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reapable_scenary)
                        {
                            foundReapable = true;
                            break;
                        }
                    }

                    if (foundReapable)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            

            // WATERING TOOL CURSOR

            case ItemType.Watering_tool:
                if (gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.daysSinceWatered == -1)
                {
                    return true;
                }
                else 
                {
                    return false;
                }


            // HARVESTING TOOL CURSOR

            case ItemType.Chopping_tool:
            case ItemType.Collecting_tool:

                // Check if item can be harvested with item selected, check item is fully grown

                //Check if seed planted
                if (gridPropertyDetails.seedItemCode != -1)
                {
                    //Get crop details for seed
                    CropDetails cropDetails = so_cropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);
                    
                    // if crop details can be found
                    if (cropDetails != null)
                    {
                        // Check if crop is fully grown
                        if (gridPropertyDetails.growthDays >= cropDetails.growthDays[cropDetails.growthDays.Length - 1])
                        {
                            // Check if crop can be harvested with tool selected
                            if (cropDetails.CanUseToolToHarvestCrop(itemDetails.itemCode))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return false;

            default:
                return false;
        }
    }




    public void DisableCursor()
    {
        cursorImage.color = Color.clear;

        CursorIsEnabled = false;
    }

    public void EnableCursor()
    {
        cursorImage.color = new Color(1f,1f,1f,1f);
        CursorIsEnabled = true;
    }

    public Vector3Int GetGridPositionForCursor()
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

        return grid.WorldToCell(worldPosition);
    }

    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);
    }
    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition)
    {
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);
        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);
    }

    public Vector3 GetWorldPositionForCursor()
    {
        return grid.CellToWorld(GetGridPositionForCursor());
    }

    

}
