using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;


[ExecuteAlways]
public class TileMapGridProperties : MonoBehaviour
{
    private Tilemap tileMap;
    private Grid grid;
    [SerializeField] private SO_GridProperties gridProperties = null;
    [SerializeField] private GridBoolProperty gridBoolProperty = GridBoolProperty.diggable;


    private void OnEnable()
    {
        // Only populate in editor
        if (!Application.IsPlaying(gameObject))
        {
            tileMap = GetComponent<Tilemap>();

            if (gridProperties != null)
            {
                gridProperties.gridPropertyList.Clear();
            }

        }
    }


    //private void OnDisable()
    //{
        // Only populate in editor
      //  if (!Application.IsPlaying(gameObject))
        //{
          //  UpdateGridProperties();

            //if (gridProperties != null)
            //{
    //            EditorUtility.SetDirty(gridProperties);
           // }

//        }
  //  }


    private void UpdateGridProperties()
    {
        tileMap.CompressBounds();

        if (!Application.IsPlaying(gameObject))
        {
            if (gridProperties != null)
            {
                Vector3Int startCell = tileMap.cellBounds.min;
                Vector3Int endCell = tileMap.cellBounds.max;

                for (int x = startCell.x; x < endCell.x; x++)
                {
                    for (int y = startCell.y; y < endCell.y; y++)
                    {
                        TileBase tile = tileMap.GetTile(new Vector3Int(x, y, 0));

                        if (tile != null)
                        {
                            gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), gridBoolProperty, true));
                        }
                    }
                }
            }

        }
    }

    private void Update()
    {
        //Only populates in editore
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("DISABLE PROPERTY TILEMAPS"); 
        }
    }


}
