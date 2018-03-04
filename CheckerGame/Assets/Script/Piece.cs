using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour
{
    [SerializeField]
    public bool _isWhite;
    [SerializeField]
    public bool _isKing;

    public bool validMove(Piece[,] board, int x1, int y1, int x2, int y2)
    {
        //if moving on top of another piece
        if (board[x2, y2] != null)
        {
            return false;
        }

        int deltaMove = Mathf.Abs(x1 - x2);
        int deltaMoveY = y2 - y1;

        if (_isWhite || _isKing)
        {
            if (deltaMove == 1)
            {
                if (deltaMoveY == 1)
                {
                    return true;
                }                    
            }
            else if (deltaMove == 2)
            {
                if (deltaMoveY == 2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];

                    if (p != null && p._isWhite != _isWhite)
                    {
                        return true;
                    }
                }
            }
        }

        if (!_isWhite || _isKing)
        {
            if (deltaMove == 1)
            {
                if (deltaMoveY == -1)
                {
                    return true;
                }
            }
            else if (deltaMove == 2)
            {
                if (deltaMoveY == -2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];

                    if (p != null && p._isWhite != _isWhite)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool isForcedToMove(Piece[,] board,int x,int y)
    {
        if (_isWhite || _isKing)
        {
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                //if theres a piece, and it is not the same color as ours check if we can place after a jump
                if (p != null && p._isWhite != _isWhite)
                {
                    if (board[x - 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
            if (x <= 5 && y <= 5)
            {
                Piece p = board[x + 1, y + 1];
                //if theres a piece, and it is not the same color as ours check if we can place after a jump
                if (p != null && p._isWhite != _isWhite)
                {
                    if (board[x + 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        if(!_isWhite || _isKing)
        {            
             if (x >= 2 && y >= 2)
            {
                Piece p = board[x - 1, y - 1];
                //if theres a piece, and it is not the same color as ours check if we can place after a jump
                if (p != null && p._isWhite != _isWhite)
                {
                     if (board[x - 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }

            if (x <= 5 && y >= 2)
            {
                Piece p = board[x + 1, y - 1];
                //if theres a piece, and it is not the same color as ours check if we can place after a jump
                if (p != null && p._isWhite != _isWhite)
                {
                    if (board[x + 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

}
