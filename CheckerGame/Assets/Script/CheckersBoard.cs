using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CheckersBoard : MonoBehaviour
{
    public static CheckersBoard _Instance { set; get; }

    public Piece[,] _piecesArr = new Piece[8, 8];
    [SerializeField]
    public GameObject _whitePrefab;
    [SerializeField]
    public GameObject _blackPrefab;
    private Vector3 _playingBoardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 _pieceOffset = new Vector3(0.5f, 0.125f, 0.5f);
    [SerializeField]
    private Vector2 _mouseOverBoard;
    [SerializeField]
    private Vector2 _startDrag;
    [SerializeField]
    private Vector2 _finishDrag;
    public CanvasGroup _alertCanvas;
    [SerializeField]
    private float _lastAlert;
    [SerializeField]
    private bool _alertActive;
    [SerializeField]
    private bool _isGameOver;
    [SerializeField]
    private float _winningTime;
    [SerializeField]
    public Transform _chatMsgContainerTrans;
    [SerializeField]
    public GameObject _msgPrefab;
    [SerializeField]
    public GameObject _highlightContainer;
    [SerializeField]
    private bool _isItWhitesTurn;
    [SerializeField]
    private bool _hasBeenKilled;
    [SerializeField]
    public bool _isWhite;
    private Piece _selectedPlayingPiece;
    private List<Piece> _forcedPLayingPieces;
    private Client _client;

    private void Start()
    {
        _Instance = this;
        _client = FindObjectOfType<Client>();

        foreach (Transform t in _highlightContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (_client)
        {
            _isWhite = _client._isHost;
            alertPopUp(_client._playersList[0].name + " versus " + _client._playersList[1].name);
        }
        else
        {
            alertPopUp("White player's turn");
            Transform c = GameObject.Find("Canvas").transform;
            foreach (Transform t in c)
            {
                t.gameObject.SetActive(false);
            }

            c.GetChild(0).gameObject.SetActive(true);
        }

        _isItWhitesTurn = true;
        _forcedPLayingPieces = new List<Piece>();
        createPlayingBoard();
    }

    private void Update()
    {
        if (_isGameOver)
        {
            if (Time.time - _winningTime > 3.0f)
            {
                Server server = FindObjectOfType<Server>();
                Client client = FindObjectOfType<Client>();

                if (server)
                {
                    Destroy(server.gameObject);
                }
                if (client)
                {
                    Destroy(client.gameObject);
                }
                SceneManager.LoadScene("Menu");
            }
            return;
        }

        foreach (Transform t in _highlightContainer.transform)
        {
            t.Rotate(Vector3.up * 90 * Time.deltaTime);
        }

        UpdateAlertPopUp();

        updateMouseOverBoard();

        if((_isWhite) ? _isItWhitesTurn : !_isItWhitesTurn)
        {
            int x = (int)_mouseOverBoard.x;
            int y = (int)_mouseOverBoard.y;

            if (_selectedPlayingPiece != null)
            {
                updateDraggingOfPlayingPiece(_selectedPlayingPiece);
            }

            if (Input.GetMouseButtonDown(0))
            {
                chooseAPlayingPiece(x, y);
            }

            if (Input.GetMouseButtonUp(0))
            {
                tryToMoveAPlayingPiece((int)_startDrag.x, (int)_startDrag.y, x, y);
            }
        }
    }

    private void updateMouseOverBoard()
    {
        if (!Camera.main)
        {
            Debug.Log("Main camera can not be found!");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            _mouseOverBoard.x = (int)(hit.point.x - _playingBoardOffset.x);
            _mouseOverBoard.y = (int)(hit.point.z - _playingBoardOffset.z);
        }
        else
        {
            _mouseOverBoard.x = -1;
            _mouseOverBoard.y = -1;
        }
    }

    private void updateDraggingOfPlayingPiece(Piece p)
    {
        if (!Camera.main)
        {
            Debug.Log("Main camera can not be found!");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }

    private void chooseAPlayingPiece(int x, int y)
    {
        //check if out of bounds
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
        {
            return;
        }

        Piece p = _piecesArr[x, y];
        if (p != null && p._isWhite == _isWhite)
        {
            if (_forcedPLayingPieces.Count == 0)
            {
                _selectedPlayingPiece = p;
                _startDrag = _mouseOverBoard;
            }
            else
            {
                //else we will look fora a piece in the  forced playing pieces list
                if (_forcedPLayingPieces.Find(fp => fp == p) == null)
                {
                    return;
                }

                _selectedPlayingPiece = p;
                _startDrag = _mouseOverBoard;
            }
        }
    }
    public void tryToMoveAPlayingPiece(int startx1, int starty1, int endx2, int endy2)
    {
        _forcedPLayingPieces = lookForAnyPossibleMove();
        _startDrag = new Vector2(startx1, starty1);
        _finishDrag = new Vector2(endx2, endy2);
        _selectedPlayingPiece = _piecesArr[startx1, starty1];

        //check if out of bounds
        if (endx2 < 0 || endx2 >= 8 || endy2 < 0 || endy2 >= 8)
        {
            if (_selectedPlayingPiece != null)
            {
                moveAPlayingPiece(_selectedPlayingPiece, startx1, starty1);
            }
            _startDrag = Vector2.zero;
            _selectedPlayingPiece = null;
            highlightPlayingPiece();
            return;
        }

        if (_selectedPlayingPiece != null)
        {
            //if the piece has not been moved
            if (_finishDrag == _startDrag)
            {
                moveAPlayingPiece(_selectedPlayingPiece, startx1, starty1);
                _startDrag = Vector2.zero;
                _selectedPlayingPiece = null;
                highlightPlayingPiece();
                return;
            }

            //if the move is valid
            if (_selectedPlayingPiece.validMove(_piecesArr, startx1, starty1, endx2, endy2))
            {
                if (Mathf.Abs(endx2 - startx1) == 2)
                {
                    Piece p = _piecesArr[(startx1 + endx2) / 2, (starty1 + endy2) / 2];
                    if (p != null)
                    {
                        _piecesArr[(startx1 + endx2) / 2, (starty1 + endy2) / 2] = null;
                        DestroyImmediate(p.gameObject);
                        _hasBeenKilled = true;
                    }
                }

                if (_forcedPLayingPieces.Count != 0 && !_hasBeenKilled)
                {
                    moveAPlayingPiece(_selectedPlayingPiece, startx1, starty1);
                    _startDrag = Vector2.zero;
                    _selectedPlayingPiece = null;
                    highlightPlayingPiece();
                    return;
                }

                _piecesArr[endx2, endy2] = _selectedPlayingPiece;
                _piecesArr[startx1, starty1] = null;
                moveAPlayingPiece(_selectedPlayingPiece, endx2, endy2);

                finishedTurn();
            }
            else
            {
                moveAPlayingPiece(_selectedPlayingPiece, startx1, starty1);
                _startDrag = Vector2.zero;
                _selectedPlayingPiece = null;
                highlightPlayingPiece();
                return;
            }
        }
    }

    private List<Piece> lookForAnyPossibleMove(Piece p, int x, int y)
    {
        _forcedPLayingPieces = new List<Piece>();

        if (_piecesArr[x, y].isForcedToMove(_piecesArr, x, y))
        {
            _forcedPLayingPieces.Add(_piecesArr[x, y]);
        }

        highlightPlayingPiece();
        return _forcedPLayingPieces;
    }

    private List<Piece> lookForAnyPossibleMove()
    {
        _forcedPLayingPieces = new List<Piece>();

        //check all the pieces on the board
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (_piecesArr[i, j] != null && _piecesArr[i, j]._isWhite == _isItWhitesTurn)
                {
                    if (_piecesArr[i, j].isForcedToMove(_piecesArr, i, j))
                    {
                        _forcedPLayingPieces.Add(_piecesArr[i, j]);
                    }
                }
            }
        }

        highlightPlayingPiece();
        return _forcedPLayingPieces;
    }

    private void finishedTurn()
    {
        int x = (int)_finishDrag.x;
        int y = (int)_finishDrag.y;

        if (_selectedPlayingPiece != null)
        {
            if (_selectedPlayingPiece._isWhite && !_selectedPlayingPiece._isKing && y == 7)
            {
                _selectedPlayingPiece._isKing = true;
                _selectedPlayingPiece.GetComponentInChildren<Animator>().SetTrigger("FlipTrigger");
            }
            else if (!_selectedPlayingPiece._isWhite && !_selectedPlayingPiece._isKing && y == 0)
            {
                _selectedPlayingPiece._isKing = true;
                _selectedPlayingPiece.GetComponentInChildren<Animator>().SetTrigger("FlipTrigger");
            }
        }

        if(_client)
        {
            string msg = "CLIENTMOVE|";
            msg += _startDrag.x.ToString() + "|";
            msg += _startDrag.y.ToString() + "|";
            msg += _finishDrag.x.ToString() + "|";
            msg += _finishDrag.y.ToString();

            _client.send(msg);
        }

        _selectedPlayingPiece = null;
        _startDrag = Vector2.zero;

        if (lookForAnyPossibleMove(_selectedPlayingPiece, x, y).Count != 0 && _hasBeenKilled)
        {
            return;
        }

        _isItWhitesTurn = !_isItWhitesTurn;
        _hasBeenKilled = false;
        checkIfPlayerWon();
        lookForAnyPossibleMove();
    }


    private void playerHasWonTheGame(bool isWhite)
    {
        _winningTime = Time.time;

        if (isWhite)
        {
            alertPopUp("White player is the winner!");
        }
        else
        {
            alertPopUp("Black player is the winner!");
        }
        _isGameOver = true;
    }

    private void checkIfPlayerWon()
    {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false;
        bool hasBlack = false;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i]._isWhite)
            {
                hasWhite = true;
            }
            else
            {
                hasBlack = true;
            }
        }

        if (!hasWhite)
        {
            playerHasWonTheGame(false);
        }
        if (!hasBlack)
        {
            playerHasWonTheGame(true);
        }
    }



    private void highlightPlayingPiece()
    {
        foreach (Transform t in _highlightContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (_forcedPLayingPieces.Count > 0)
        {
            _highlightContainer.transform.GetChild(0).transform.position = _forcedPLayingPieces[0].transform.position + Vector3.down * 0.1f;
        }

        if (_forcedPLayingPieces.Count > 1)
        {
            _highlightContainer.transform.GetChild(1).transform.position = _forcedPLayingPieces[1].transform.position + Vector3.down * 0.1f;
        }
    }

    private void createPlayingBoard()
    {
        //generate white team and playing pieces
        for (int y = 0; y < 3; y++)
        {
            bool odd = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                createPieces((odd) ? x : x + 1, y);
            }
        }

        //generate black team and playing pieces
        for (int y = 7; y > 4; y--)
        {
            bool odd = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                createPieces((odd) ? x : x + 1, y);
            }
        }
    }

    private void createPieces(int x, int y)
    {
        bool isPieceWhite = (y > 3) ? false : true;
        GameObject go = Instantiate((isPieceWhite)? _whitePrefab : _blackPrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        _piecesArr[x, y] = p;
        moveAPlayingPiece(p, x, y);
    }

    private void moveAPlayingPiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + _playingBoardOffset + _pieceOffset;
    }

    public void chatMessage(string msg)
    {
        GameObject go = Instantiate(_msgPrefab) as GameObject;
        go.transform.SetParent(_chatMsgContainerTrans);
        go.GetComponentInChildren<Text>().text = msg;
    }

    public void sendChatMessage()
    {
        InputField i = GameObject.Find("MessageInput").GetComponent<InputField>();
        if (i.text == "")
        {
            return;
        }
        _client.send("CLIENTMESSAGE|" + i.text);
        i.text = "";
    }

    public void alertPopUp(string text)
    {
        _alertCanvas.GetComponentInChildren<Text>().text = text;
        _alertCanvas.alpha = 1;
        _lastAlert = Time.time;
        _alertActive = true;
    }

    public void UpdateAlertPopUp()
    {
        if (_alertActive)
        {
            if (Time.time - _lastAlert > 1.5f)
            {
                _alertCanvas.alpha = 1 - ((Time.time - _lastAlert) - 1.5f);

                if (Time.time - _lastAlert > 2.5f)
                {
                    _alertActive = false;
                }
            }
        }
    }


}
