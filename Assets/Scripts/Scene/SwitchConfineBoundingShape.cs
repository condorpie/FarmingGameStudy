using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchConfineBoundingShape : MonoBehaviour
{
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
    }


    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
    }



    /// <summary>
    /// Switch the collider that cinemachine uses to define the bounds of the screen
    /// </summary>
    private void SwitchBoundingShape() 
    {
        // get the polygon collider in the boundsconfiner gameobject which is used by cinemachine to prevent the camera going beyond screen edge
        PolygonCollider2D polygonCollider2D = GameObject.FindGameObjectWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();

        CinemachineConfiner cinemachineConfiner = GetComponent<CinemachineConfiner>();

        cinemachineConfiner.m_BoundingShape2D = polygonCollider2D;

        //since the confiner bounds have changed need to call this to clear cache

        cinemachineConfiner.InvalidatePathCache();
    }
}
