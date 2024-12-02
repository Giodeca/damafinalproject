using UnityEngine;

public class Node : MonoBehaviour
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public GameObject unit;
    public int index;
    public Node(bool walkable, Vector3 worldPos)
    {
        this.walkable = walkable;
        this.worldPosition = worldPos;

    }



    public bool HasUnit()
    {
        Collider[] colliders = Physics.OverlapSphere(worldPosition, 0.5f, LayerMask.GetMask("Piece"));
        if (colliders.Length > 0)
        {
            unit = colliders[0].gameObject;
            return true;
        }
        else
        {
            return false;
        }
    }
    public string GetUnitName()
    {
        if (unit != null)
        {
            return unit.tag;
        }
        return null;
    }
}
