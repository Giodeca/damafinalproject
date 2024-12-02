using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public enum Turn
{
    PlayerTurn, EnemyTurn
}
public class GridManager : Singleton<GridManager>
{
    private Stack<GameState> gameStateStack = new Stack<GameState>();
    Node[,] grid;
    public Vector2 gridWorldSize;
    public float nodeRadious;
    public LayerMask unwalkableMask;
    float nodeDiameter;
    int gridSizeX, gridSizeY;
    [SerializeField] private GameObject whiteCube;
    [SerializeField] private GameObject blueCube;
    [SerializeField] private GameObject whitePiece;
    [SerializeField] private GameObject bluePiece;
    [SerializeField] private GameObject test;
    public GameObject selectedPiece;
    private Camera mainCamera;
    bool randomChoise;
    private List<Node> validMoveNodes = new List<Node>();
    private List<Node> eatNodes = new List<Node>();
    public List<Node> destroyNode = new List<Node>();

    public List<GameObject> aiPiece = new List<GameObject>();
    public List<GameObject> MYPiece = new List<GameObject>();
    private GameObject backUpPiece;
    public bool pieceEaten;
    private bool hasToEat;
    public Material movedMat;
    public bool enemyMoved;
    public Node MIDNode;
    Dictionary<Node, Node> captureDictionary = new Dictionary<Node, Node>();
    public bool routineIsRunnig;
    public Turn turn;
    public TMP_Text turnText;
    public TMP_Text piece;
    public TMP_Text pieceEnemy;
    private bool isWhite;
    private bool pieceSelected;

    private void Start()
    {
        nodeDiameter = nodeRadious * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
        mainCamera = Camera.main;
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        randomChoise = Random.value < 0.5f;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadious) + Vector3.forward * (y * nodeDiameter + nodeRadious);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadious, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint)
                {
                    gridX = x,
                    gridY = y
                };

                GameObject cubeToSpawn = (x + y) % 2 == 0 ? whiteCube : blueCube;
                GameObject spawnedCube = Instantiate(cubeToSpawn, worldPoint, Quaternion.identity);
                spawnedCube.transform.localScale = new Vector3(nodeDiameter, nodeDiameter, nodeDiameter);

                if ((x + y) % 2 != 0)
                {
                    Vector3 pieceSpawnPosition = worldPoint + Vector3.up * (nodeDiameter / 2);

                    if (randomChoise)
                    {
                        if (y < 3)
                        {
                            GameObject myPiece = Instantiate(whitePiece, pieceSpawnPosition, Quaternion.identity);
                            myPiece.tag = "Piece";
                            myPiece.GetComponent<Checkers>().colorISWhite = true;
                            turn = Turn.PlayerTurn;
                            MYPiece.Add(myPiece);
                            isWhite = true;
                        }
                        else if (y > gridSizeY - 4)
                        {
                            GameObject enemyPiece = Instantiate(bluePiece, pieceSpawnPosition, Quaternion.identity);
                            enemyPiece.tag = "PieceEnemy";
                            enemyPiece.GetComponent<Checkers>().isEnemy = true;
                            aiPiece.Add(enemyPiece);
                        }
                    }
                    else
                    {
                        if (y < 3)
                        {
                            GameObject myPiece = Instantiate(bluePiece, pieceSpawnPosition, Quaternion.identity);
                            myPiece.tag = "Piece";
                            turn = Turn.EnemyTurn;
                            MYPiece.Add(myPiece);
                            isWhite = false;
                        }
                        else if (y > gridSizeY - 4)
                        {
                            GameObject enemyPiece = Instantiate(whitePiece, pieceSpawnPosition, Quaternion.identity);
                            enemyPiece.tag = "PieceEnemy";
                            enemyPiece.GetComponent<Checkers>().colorISWhite = true;
                            enemyPiece.GetComponent<Checkers>().isEnemy = true;
                            aiPiece.Add(enemyPiece);
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        PieceSelection();
        PieceMovement();
        if (Input.GetKeyDown(KeyCode.Space) && turn == Turn.EnemyTurn)
        {
            AiMovement();
        }
        if (Input.GetKeyDown(KeyCode.K))
            TurnChange();

        turnText.text = " " + turn;


        if (isWhite)
        {
            piece.text = " " + MYPiece.Count;
            pieceEnemy.text = " " + aiPiece.Count;
        }
        else
        {
            piece.text = " " + aiPiece.Count;
            pieceEnemy.text = " " + MYPiece.Count;
        }

        if (Input.GetKeyDown(KeyCode.U) && !routineIsRunnig && !pieceSelected)
        {
            UndoMove();
        }
    }

    public void AiCallButton()
    {
        if (turn == Turn.EnemyTurn)
        {
            AiMovement();
        }
    }

    private void PieceSelection()
    {
        if (!hasToEat)
        {
            if (turn == Turn.PlayerTurn)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    EventManager.ClearBoard?.Invoke();
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (hit.collider.CompareTag("Piece") /*|| hit.collider.CompareTag("PieceEnemy")*/)
                        {
                            pieceSelected = true;
                            selectedPiece = hit.collider.gameObject;
                            validMoveNodes = GetValidMoveNodes(selectedPiece.transform.position);

                            foreach (Node moveNode in validMoveNodes)
                            {
                                Vector3 pieceSpawnPosition = moveNode.worldPosition + Vector3.up * (nodeDiameter / 2);
                                Instantiate(test, pieceSpawnPosition, Quaternion.identity);
                            }
                        }

                    }

                }
            }

        }

    }
    private void PieceMovement()
    {
        if (selectedPiece != null && Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.tag == "MoveHere")
                {
                    if (!routineIsRunnig)
                    {
                        pieceSelected = false;
                        routineIsRunnig = true;
                        StartCoroutine(MovePieceLerp(selectedPiece, hit.collider.gameObject, 0.3f));
                    }

                }
            }
        }
    }

    public void TurnChange()
    {
        if (turn == Turn.PlayerTurn)
        {
            turn = Turn.EnemyTurn;

        }
        else if (turn == Turn.EnemyTurn)
        {
            turn = Turn.PlayerTurn;
        }
    }
    private IEnumerator MovePieceLerp(GameObject piece, GameObject pos, float duration)
    {
        SaveGameState();

        Vector3 startPosition = piece.transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            piece.transform.position = Vector3.Lerp(startPosition, pos.transform.position, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Node currentNode = GetNodeFromWorldPoint(pos.transform.position);

        if ((piece.tag == "Piece" && currentNode.gridY == gridSizeY - 1) || (piece.tag == "PieceEnemy" && currentNode.gridY == 0))
        {
            PromotePiece(piece);
        }

        backUpPiece = piece;
        validMoveNodes.Clear();
        eatNodes.Clear();
        selectedPiece = null;
        EventManager.ClearBoard?.Invoke();
        Eat(currentNode);
        piece.transform.position = pos.transform.position;
        routineIsRunnig = false;
    }


    private void PromotePiece(GameObject piece)
    {
        EventManager.OnPiecePromoted?.Invoke(piece);
        //TurnChange();
        Debug.Log("ChangeTurn");
    }

    private void Eat(Node currentNode)
    {

        if (pieceEaten)
        {
            selectedPiece = backUpPiece;
            validMoveNodes = GetValidMoveNodes(selectedPiece.transform.position);

            if (captureDictionary.Count > 0)
            {
                hasToEat = true;
                foreach (Node moveNode in validMoveNodes)
                {
                    Vector3 pieceSpawnPosition = moveNode.worldPosition + Vector3.up * (nodeDiameter / 2);
                    Instantiate(test, pieceSpawnPosition, Quaternion.identity);
                }
            }
            else
            {
                pieceEaten = false;
                hasToEat = false;
                backUpPiece = null;
                TurnChange();

            }
        }
        else
        {
            hasToEat = false;
            TurnChange();
        }

        eatNodes.Clear();

    }
    public void AiMovement()
    {
        List<GameObject> movablePieces = new List<GameObject>();
        List<GameObject> capturingPieces = new List<GameObject>();

        foreach (GameObject piece in aiPiece)
        {
            selectedPiece = piece;
            validMoveNodes = GetValidMoveNodes(selectedPiece.transform.position);
            if (validMoveNodes.Count > 0)
            {
                movablePieces.Add(selectedPiece);
                if (selectedPiece.GetComponent<Checkers>().nodesEat.Count > 0)
                {
                    //Material mat = selectedPiece.GetComponent<MeshRenderer>().material;
                    //selectedPiece.GetComponent<MeshRenderer>().material = movedMat;
                    capturingPieces.Add(selectedPiece);
                }
            }
        }

        if (capturingPieces.Count > 0)
        {
            GameObject pieceToMove = capturingPieces[Random.Range(0, capturingPieces.Count)];
            selectedPiece = pieceToMove;
            //Material mat = selectedPiece.GetComponent<MeshRenderer>().material;
            //selectedPiece.GetComponent<MeshRenderer>().material = movedMat;
            validMoveNodes = GetValidMoveNodes(selectedPiece.transform.position);

            Node moveNode = selectedPiece.GetComponent<Checkers>().nodesEat[Random.Range(0, selectedPiece.GetComponent<Checkers>().nodesEat.Count)];
            Vector3 pieceSpawnPosition = moveNode.worldPosition + Vector3.up * (nodeDiameter / 2);
            GameObject testtt = Instantiate(test, pieceSpawnPosition, Quaternion.identity);
            if (!routineIsRunnig)
            {
                routineIsRunnig = true;
                StartCoroutine(MovePieceLerp(selectedPiece, testtt, 0.3f));
            }

            //if (selectedPiece != null)
            //    selectedPiece.GetComponent<MeshRenderer>().material = mat;
            EventManager.ClearList?.Invoke();
            eatNodes.Clear();
        }
        else if (movablePieces.Count > 0)
        {
            GameObject pieceToMove = movablePieces[Random.Range(0, movablePieces.Count)];
            selectedPiece = pieceToMove;
            //Material mat = selectedPiece.GetComponent<MeshRenderer>().material;
            //selectedPiece.GetComponent<MeshRenderer>().material = movedMat;
            validMoveNodes = GetValidMoveNodes(selectedPiece.transform.position);

            Node moveNode = validMoveNodes[Random.Range(0, validMoveNodes.Count)];
            Vector3 pieceSpawnPosition = moveNode.worldPosition + Vector3.up * (nodeDiameter / 2);
            GameObject testtt = Instantiate(test, pieceSpawnPosition, Quaternion.identity);
            if (!routineIsRunnig)
            {
                routineIsRunnig = true;
                StartCoroutine(MovePieceLerp(selectedPiece, testtt, 0.3f));
            }
        }
        else
        {
            hasToEat = false;
        }
        enemyMoved = false;
    }




    private Node GetNodeFromWorldPoint(Vector3 worldPos)
    {
        float percentX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPos.z + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    private List<Node> GetValidMoveNodes(Vector3 piecePosition)
    {
        List<Node> validNodes = new List<Node>();
        List<Node> captureNodes = new List<Node>();
        captureDictionary.Clear();

        Node currentNode = GetNodeFromWorldPoint(piecePosition);

        if (selectedPiece != null && selectedPiece.tag == "Piece" && !selectedPiece.GetComponent<Checkers>().isDama)
        {

            AddCaptureNode(currentNode.gridX + 2, currentNode.gridY + 2, currentNode.gridX + 1, currentNode.gridY + 1, captureNodes);
            AddCaptureNode(currentNode.gridX - 2, currentNode.gridY + 2, currentNode.gridX - 1, currentNode.gridY + 1, captureNodes);


            if (captureNodes.Count > 0)
            {
                eatNodes.AddRange(captureNodes);
                return captureNodes;
            }


            AddValidNode(currentNode.gridX + 1, currentNode.gridY + 1, validNodes, captureNodes);
            AddValidNode(currentNode.gridX - 1, currentNode.gridY + 1, validNodes, captureNodes);
        }
        else if (selectedPiece != null && selectedPiece.tag == "PieceEnemy" && !selectedPiece.GetComponent<Checkers>().isDama)
        {

            AddCaptureNode(currentNode.gridX + 2, currentNode.gridY - 2, currentNode.gridX + 1, currentNode.gridY - 1, captureNodes);
            AddCaptureNode(currentNode.gridX - 2, currentNode.gridY - 2, currentNode.gridX - 1, currentNode.gridY - 1, captureNodes);


            if (captureNodes.Count > 0)
            {
                selectedPiece.GetComponent<Checkers>().nodesEat.AddRange(captureNodes);
                return captureNodes;
            }


            AddValidNode(currentNode.gridX + 1, currentNode.gridY - 1, validNodes, captureNodes);
            AddValidNode(currentNode.gridX - 1, currentNode.gridY - 1, validNodes, captureNodes);
        }
        else if (selectedPiece != null && selectedPiece.tag == "Piece" && selectedPiece.GetComponent<Checkers>().isDama)
        {

            AddCaptureNode(currentNode.gridX + 2, currentNode.gridY + 2, currentNode.gridX + 1, currentNode.gridY + 1, captureNodes);
            AddCaptureNode(currentNode.gridX - 2, currentNode.gridY + 2, currentNode.gridX - 1, currentNode.gridY + 1, captureNodes);
            AddCaptureNode(currentNode.gridX + 2, currentNode.gridY - 2, currentNode.gridX + 1, currentNode.gridY - 1, captureNodes);
            AddCaptureNode(currentNode.gridX - 2, currentNode.gridY - 2, currentNode.gridX - 1, currentNode.gridY - 1, captureNodes);


            if (captureNodes.Count > 0)
            {
                eatNodes.AddRange(captureNodes);
                return captureNodes;
            }


            AddValidNode(currentNode.gridX + 1, currentNode.gridY + 1, validNodes, captureNodes);
            AddValidNode(currentNode.gridX - 1, currentNode.gridY + 1, validNodes, captureNodes);
            AddValidNode(currentNode.gridX + 1, currentNode.gridY - 1, validNodes, captureNodes);
            AddValidNode(currentNode.gridX - 1, currentNode.gridY - 1, validNodes, captureNodes);
        }
        else if (selectedPiece != null && selectedPiece.tag == "PieceEnemy" && selectedPiece.GetComponent<Checkers>().isDama)
        {

            AddCaptureNode(currentNode.gridX + 2, currentNode.gridY + 2, currentNode.gridX + 1, currentNode.gridY + 1, captureNodes);
            AddCaptureNode(currentNode.gridX - 2, currentNode.gridY + 2, currentNode.gridX - 1, currentNode.gridY + 1, captureNodes);
            AddCaptureNode(currentNode.gridX + 2, currentNode.gridY - 2, currentNode.gridX + 1, currentNode.gridY - 1, captureNodes);
            AddCaptureNode(currentNode.gridX - 2, currentNode.gridY - 2, currentNode.gridX - 1, currentNode.gridY - 1, captureNodes);


            if (captureNodes.Count > 0)
            {
                selectedPiece.GetComponent<Checkers>().nodesEat.AddRange(captureNodes);
                return captureNodes;
            }


            AddValidNode(currentNode.gridX + 1, currentNode.gridY + 1, validNodes, captureNodes);
            AddValidNode(currentNode.gridX - 1, currentNode.gridY + 1, validNodes, captureNodes);
            AddValidNode(currentNode.gridX + 1, currentNode.gridY - 1, validNodes, captureNodes);
            AddValidNode(currentNode.gridX - 1, currentNode.gridY - 1, validNodes, captureNodes);
        }


        return validNodes;
    }

    private void AddValidNode(int x, int y, List<Node> validNodes, List<Node> eatChance)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            Node node = grid[x, y];
            if (node.walkable && !node.HasUnit())
            {
                validNodes.Add(node);
            }
        }
    }

    private void AddCaptureNode(int x, int y, int midX, int midY, List<Node> captureNodes)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            Node node = grid[x, y];
            Node midNode = grid[midX, midY];
            if (node.walkable && !node.HasUnit() && midNode.HasUnit() && midNode.GetUnitName() != selectedPiece.tag)
            {
                captureNodes.Add(node);
                captureDictionary[node] = midNode;
            }
        }
    }


    private void SaveGameState()
    {
        GameState state = new GameState
        {
            playerPieces = SavePieceStates(MYPiece),
            enemyPieces = SavePieceStates(aiPiece),
            turn = turn,
            //selectedPiece = selectedPiece != null ? new PieceState(selectedPiece) : null,
            pieceEaten = pieceEaten,
            hasToEat = hasToEat,
        };

        gameStateStack.Push(state);
    }

    private List<PieceState> SavePieceStates(List<GameObject> pieces)
    {
        List<PieceState> pieceStates = new List<PieceState>();
        foreach (GameObject piece in pieces)
        {
            pieceStates.Add(new PieceState(piece));
        }
        return pieceStates;
    }

    public void UndoMove()
    {
        if (gameStateStack.Count > 0)
        {
            EventManager.ClearBoard?.Invoke();
            GameState previousState = gameStateStack.Pop();

            // Rimuovi i pezzi attuali
            foreach (GameObject piece in MYPiece)
            {
                Destroy(piece);
            }
            foreach (GameObject piece in aiPiece)
            {
                Destroy(piece);
            }


            MYPiece = RestorePieceStates(previousState.playerPieces);
            aiPiece = RestorePieceStates(previousState.enemyPieces);


            piece.text = isWhite ? " " + MYPiece.Count : " " + aiPiece.Count;
            pieceEnemy.text = isWhite ? " " + aiPiece.Count : " " + MYPiece.Count;


            turn = previousState.turn;
            selectedPiece = null;
            pieceEaten = previousState.pieceEaten;
            hasToEat = previousState.hasToEat;


            validMoveNodes.Clear();
            eatNodes.Clear();
            captureDictionary.Clear();
            destroyNode.Clear();


            turnText.text = " " + turn;
        }
    }

    private List<GameObject> RestorePieceStates(List<PieceState> pieceStates)
    {
        List<GameObject> pieces = new List<GameObject>();
        foreach (PieceState state in pieceStates)
        {
            GameObject piece = InstantiatePiece(state);
            pieces.Add(piece);


            var checkersComponent = piece.GetComponent<Checkers>();
            checkersComponent.isDama = state.isDama;
            checkersComponent.isEnemy = state.isEnemy;
            checkersComponent.colorISWhite = state.colorISWhite;
            if (state.isDama)
            {
                checkersComponent.crown.SetActive(true);
            }
        }
        return pieces;
    }

    private GameObject InstantiatePiece(PieceState state)
    {
        Debug.Log($"Restoring piece: colorISWhite={state.colorISWhite}, position={state.position}");
        GameObject prefab = state.colorISWhite ? whitePiece : bluePiece;

        GameObject piece = Instantiate(prefab, state.position, Quaternion.identity);
        piece.tag = state.tag;
        piece.GetComponent<Checkers>().isDama = state.isDama;
        piece.GetComponent<Checkers>().isEnemy = state.isEnemy;
        if (state.isDama)
        {
            piece.GetComponent<Checkers>().crown.SetActive(true);
        }
        return piece;
    }

    private class GameState
    {
        public List<PieceState> playerPieces;
        public List<PieceState> enemyPieces;
        public Turn turn;
        public PieceState selectedPiece;
        public bool pieceEaten;
        public bool hasToEat;
        //public bool routineIsRunnig;
    }

    private class PieceState
    {
        public Vector3 position;
        public bool isDama;
        public bool isEnemy;
        public bool isWhite;
        public string tag;
        public bool colorISWhite;

        public PieceState(GameObject piece)
        {
            position = piece.transform.position;
            isDama = piece.GetComponent<Checkers>().isDama;
            isEnemy = piece.GetComponent<Checkers>().isEnemy;
            colorISWhite = piece.GetComponent<Checkers>().colorISWhite;
            tag = piece.tag;
        }
    }
}



