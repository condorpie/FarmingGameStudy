using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class CropDetails 
{
    [ItemCodeDescription]
    public int seedItemCode; //this is code for seed
    public int[] growthDays; //days growth for each stage
    [ItemCodeDescription]
     // prefab to use when implementing growth stages
    public GameObject[] growthPrefab;
    public Sprite[] growthSprite;
    public Season[] seasons;
    public Sprite harvestedSprite; // sprite used once harvested
    [ItemCodeDescription]
    public int harvestedTransformItemCode; // if the item transforms into another item when harvested this item code will be populated
    public bool hideCropBeforeHarvestedAnimation; // if the crop needs to be disabled before harvest
    public bool disableCropCollidersBeforeHarvestAnimation; //disables colliders on crop if needed
    public bool isHarvestedAnimation;
    public bool isHarvestActionEffect = false; // flag to determine if there is an animation when harvested
    public bool spawnCropProducedAtPlayerPosition;
    public HarvestActionEffect harvestActionEffect; // the vfx for harvest animation

    [ItemCodeDescription]
    public int[] harvestToolItemCode; // item codes for the tools that can harvest
    public int[] requiredHarvestAction; // number of harvest actions requiored for tool in harvest tool code array
    [ItemCodeDescription]
    public int[] cropProducedItemCode; // array of item codes produced for harvested crop
    public int[] cropProducedMinQuantity;
    public int[] cropProducedMaxQuantity;
    public int daysToRegrow;

    ///summary
    ///returns true if the tool item code can be used to harvest crop
    ///summary

    public bool CanUseToolToHarvestCrop(int toolItemCode)
    {
        if (RequiredHarvestActionsForTool(toolItemCode) == -1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    ///summary
    ///returns -1 if the tool cant be used to harvest this crop, else returns the number of harvest actions required by this tool
    ///summary
    public int RequiredHarvestActionsForTool(int toolItemCode)
    {
        for (int i = 0; i < harvestToolItemCode.Length; i++)
        {
            if (harvestToolItemCode[i] == toolItemCode)
            {
                return requiredHarvestAction[i];
            }
        }
        return -1;
    }
}
