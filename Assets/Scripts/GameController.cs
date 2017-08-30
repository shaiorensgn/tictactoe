using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public GameObject menuButton;
    public Sprite xSprite;
    public Sprite oSprite;
    public int size;

    private bool _vsComputer;
    private bool _playerTurn = true;

    private enum Pieces { BLANK = 0, X, O };

    private Button[] _buttons;
    private Pieces[,] _spaces;

    struct Space {
        public int x;
        public int y;

        public Space(int _x, int _y) {
            x = _x;
            y = _y;
        }
    }

	// Use this for initialization
	void Start() {
        menuButton.SetActive(false);
        _vsComputer = PlayerPrefsManager.GetVsComputer();

        _buttons = new Button[size * size];
        _spaces = new Pieces[size, size];
        
        int j = 0;
        for (int i = 0; i < transform.childCount; ++i) {
            Transform child = transform.GetChild(i);

            // make sure we don't get the menu button in the button array
            if (!child.gameObject.Equals(menuButton)) {
                Button btn = child.gameObject.GetComponent<Button>();
                if (btn) {
                    _buttons[j] = btn;
                    j++;
                }
            }
        }
    }

    public void onMenuClick() {
        SceneManager.LoadScene("menu");
    }

    public void OnButtonClick(int i) {
        Image img = _buttons[i].GetComponent<Image>();

        int x = i % size;
        int y = i / size;

        // check the space value just to be absolutely certain
        if (_spaces[x, y] == Pieces.BLANK && img) {
            img.sprite = _playerTurn ? xSprite : oSprite;
            img.color = Color.white;

            _buttons[i].interactable = false;
            _spaces[x, y] = _playerTurn ? Pieces.X : Pieces.O;

            int status = checkWinCondition(x, y);
            if (status == 1) {
                endGame(true);
            }
            else if (status == 2) {
                endGame(false);
            }
            else {
                _playerTurn = !_playerTurn;

                if (_vsComputer) {
                    doComputerMove();
                }
            }
        }
        
        // if the image doesn't exist for some crazy reason then give the player another chance at a move?
    }

    private void doComputerMove() {
        int x, y;
        Space choice;

        // This isn't a perfect playing algorithm, I'd rather have a fun game than an impossible game even if it plays really dumb sometimes.
        // If you want a perfectly playing algorithm you'd probably use the minmax algorithm.
        // WHY NO TUPLES UNITY?! :(
        List<Space> winningMoves = new List<Space>();
        List<Space> drawingMoves = new List<Space>();
        List<Space> otherMoves = new List<Space>();

        for (y = 0; y < size; y++) {
            for (x = 0; x < size; x++) {
                if (_spaces[x, y] == Pieces.BLANK) {
                    _spaces[x, y] = Pieces.O;

                    int result = checkWinCondition(x, y);

                    if (result == 1) {
                        winningMoves.Add(new Space(x, y));
                    }
                    else if (result == 2) {
                        drawingMoves.Add(new Space(x, y));
                    }
                    else {
                        otherMoves.Add(new Space(x, y));
                    }

                    _spaces[x, y] = Pieces.BLANK;
                }
            }
        }
        
        if (winningMoves.Count > 0) {
            // if there are winning moves then pick one of those
            choice = winningMoves[new System.Random().Next(0, winningMoves.Count)];
        }
        else if (drawingMoves.Count > 0) {
            choice = drawingMoves[new System.Random().Next(0, drawingMoves.Count)];
        }
        else {
            choice = otherMoves[new System.Random().Next(0, otherMoves.Count)];
        }

        Button button = _buttons[choice.x + choice.y * size];
        Image img = button.GetComponent<Image>();

        if (img) {
            img.sprite = oSprite;
            img.color = Color.white;

            button.interactable = false;
            _spaces[choice.x, choice.y] = Pieces.O;

            int status = checkWinCondition(choice.x, choice.y);
            if (status == 1) {
                endGame(true);
            }
            else if (status == 2) {
                endGame(false);
            }

            _playerTurn = !_playerTurn;
        }
    }

    private void endGame(bool won) {
        Transform instructions = transform.Find("Instructions");
        if (instructions) {
            Text text = instructions.GetComponent<Text>() as Text;

            if (text) {
                if (won) {
                    text.text = "Player " + (_playerTurn ? Pieces.X : Pieces.O) + " won!";
                }
                else
                    text.text = "Draw game!";
            }

        }

        menuButton.SetActive(true);
    }

    private int checkWinCondition(int x, int y) {
        // This should check the row, col, and maybe diag belonging to x, y to see if they complete a row/col/diag
        // if they do, then that player has won the game
        // Go back to menu
        
        // This code here is probably more efficient, and certainly easier to read for a non-standard Tic Tac Toe board
        // Constraints for this code is that the board must be square. Non-square boards would clearly not work with diagonals and that kind of defeats all of the trickiness in TTT.
        List<Pieces> temp = new List<Pieces>(size);

        // diagonal \
        if (x == y) {
            for (int i = 0; i < size; ++i) {
                temp.Add(_spaces[i, i]);
            }

            if (temp.TrueForAll(equalToCurrentPlayerPiece)) {
                return 1;
            }

            temp.Clear();
        }

        // diagonal /
        if (x == size - y - 1) {
            for (int i = 0; i < size; ++i) {
                temp.Add(_spaces[size - i - 1, i]);
            }

            if (temp.TrueForAll(equalToCurrentPlayerPiece)) {
                return 1;
            }

            temp.Clear();
        }

        // horizontal
        for (int i = 0; i < size; ++i) {
            temp.Add(_spaces[x, i]);
        }

        if (temp.TrueForAll(equalToCurrentPlayerPiece)) {
            return 1;
        }

        temp.Clear();

        // vertical
        for (int i = 0; i < size; ++i) {
            temp.Add(_spaces[i, y]);
        }

        if (temp.TrueForAll(equalToCurrentPlayerPiece)) {
            return 1;
        }

        // This code here is far more efficient for a standard Tic Tac Toe board, but it's a bit harder to parse for a human
        // We could combine the 2 methods, but that's a slippery slope... at what point do you just go with a generic solution?
        //pieces currentPlayerPiece = (_playerTurn ? pieces.X : pieces.O);

        //// diagonal \
        //if (x == y && _spaces[0, 0] == _spaces[1, 1] && _spaces[0, 0] == _spaces[2, 2] && _spaces[0, 0] == currentPlayerPiece)
        //    return 1;

        //// diagonal /
        //if (x == size - y - 1 && _spaces[2, 0] == _spaces[1, 1] && _spaces[2, 0] == _spaces[0, 2] && _spaces[2, 0] == currentPlayerPiece)
        //    return 1;

        //// horizontal
        //if (_spaces[0, y] == _spaces[1, y] && _spaces[0, y] == _spaces[2, y] && _spaces[0, y] == currentPlayerPiece)
        //    return 1;

        //// vertical
        //if (_spaces[x, 0] == _spaces[x, 1] && _spaces[x, 0] == _spaces[x, 2] && _spaces[x, 0] == currentPlayerPiece)
        //    return 1;

        // Check for draw game
        bool isDraw = true;
        foreach (Pieces space in _spaces) {
            if (space == Pieces.BLANK) {
                isDraw = false;
                break;
            }
        }
        
        return isDraw ? 2 : 0;
    }

    private bool equalToCurrentPlayerPiece(Pieces obj) {
        return obj == (_playerTurn ? Pieces.X : Pieces.O);
    }
}
