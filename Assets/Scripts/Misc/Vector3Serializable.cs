using UnityEngine;

[System.Serializable]

public class Vector3Serializable
{
    public Vector3Serializable(Vector3 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }
    
    public float x, y, z;

    public Vector3Serializable(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Serializable()
    {
        
    }
}
