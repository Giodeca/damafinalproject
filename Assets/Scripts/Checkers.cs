using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkers : MonoBehaviour
{
    public List<Node> nodesEat = new List<Node>();
    public bool isDama;
    public GameObject crown;
    public bool isEnemy;
    public bool colorISWhite;
    private void OnEnable()
    {
        EventManager.ClearList += OnClear;
        EventManager.OnPiecePromoted += OnPromotion;
    }


    private void OnDisable()
    {
        EventManager.ClearList -= OnClear;
        EventManager.OnPiecePromoted -= OnPromotion;
    }
    private void Start()
    {
        nodesEat = new List<Node>();
    }


    private void OnClear()
    {
        nodesEat.Clear();
    }
    private void OnPromotion(GameObject obj)
    {
        if (obj == this.gameObject)
        {
            isDama = true;
            crown.SetActive(true);
            nodesEat.Clear();
            Debug.Log(gameObject.name + "   " + isDama);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Piece") || collision.gameObject.CompareTag("PieceEnemy"))
        {
            GridManager.Instance.pieceEaten = true;
            if (this.gameObject != GridManager.Instance.selectedPiece)
            {
                if (isEnemy)
                {
                    GridManager.Instance.aiPiece.Remove(gameObject);
                    if (GridManager.Instance.aiPiece.Count <= 0)
                    {
                        SceneManager.LoadScene("WIN");
                    }
                }
                else
                {
                    GridManager.Instance.MYPiece.Remove(gameObject);
                    if (GridManager.Instance.MYPiece.Count <= 0)
                    {
                        SceneManager.LoadScene("lOSE");
                    }
                }

                Destroy(gameObject);
            }

        }
    }
}
