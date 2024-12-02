using UnityEngine;

public class Rip : MonoBehaviour
{
    //public List<Node> validMoveNodes = new List<Node>();
    //public List<Node> eatNodes = new List<Node>();
    //public List<Node> destroyNode = new List<Node>();
    private void OnEnable()
    {
        EventManager.ClearBoard += OnClear;
    }

    private void OnDisable()
    {
        EventManager.ClearBoard -= OnClear;
    }

    private void OnClear()
    {
        Destroy(gameObject);
    }

}
