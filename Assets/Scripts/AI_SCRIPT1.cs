using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AI_SCRIPT1 : MonoBehaviour
{
	class MOVESandPOSITIONS
	{
		public NODE pos;
		public float score;
		public MOVESandPOSITIONS(NODE _pos, float _score)
		{
			pos = _pos;
			score = _score;

		}
	}

	enum GameMode
	{
		SINGLE_PLAYER,
		LOCAL_MULTUPLAYER
	}

	struct NODE
	{
		public int x;
		public int y;
		public bool visited;
		public List<Vector2> visitedFromOrTo;
		// public List<NODESCORES> NS;

		public bool isCornerNode;
		public bool isWalkable;


		public NODE(int _x, int _y, bool _visited, bool _isCornerNode, bool _isWalkable)
		{
			visitedFromOrTo = new List<Vector2>();
			x = _x;
			y = _y;
			visited = _visited;
			isCornerNode = _isCornerNode;
			isWalkable = _isWalkable;
		}

	}
	[SerializeField]
	Material LINENMAT;
	bool aiWon
	{
		get
		{
			if (currentx == aiWinPos.x && currenty == aiWinPos.y)

				return true;
			else
				return false;
		}


	}
	bool playerWon
	{
		get
		{
			if (currentx == playerWinPos.x && currenty == playerWinPos.y)

				return true;
			else
				return false;
		}


	}
	[SerializeField] Transform AITURN;
	[SerializeField] Transform PLAYERTURN;
	[SerializeField] GameObject Ball;
	[SerializeField] int xsize;
	[SerializeField] int ysize;
	[SerializeField] GameObject Dots;
	[SerializeField] Transform GuideQuad;
	[SerializeField] Text VERDICT;
	[SerializeField] Canvas gameOverCanvas;
	[SerializeField] GameMode mGameMode;

	bool playerOneTurn;
	bool gameEnded;
	bool tie;


	int currentx;
	int currenty;
	List<NODE> availableMoves;

	Vector2 playerWinPos;
	Vector2 aiWinPos;
	NODE[,] G;
	List<MOVESandPOSITIONS> movesCheckList;
	List<Vector2> checkedNodes;
	Vector2 MouseStartpos, MouseEndPos;
	LineRenderer L1;

	// Use this for initialization
	void Start()
	{
		playerOneTurn = true;
		tie = false;
		EnableGameOverMenu(false);
		availableMoves = new List<NODE>();
		GuideQuad.gameObject.SetActive(false);
		GuideQuad.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
		L1 = GetComponent<LineRenderer>();
		L1.positionCount = 2;
		L1.startColor = Color.white;
		L1.endColor = Color.white;
		L1.enabled = false;
		gameEnded = false;
		if (xsize == 0 || ysize == 0)
			return;
		transform.position += new Vector3(xsize / 2, ysize / 2, 0);
		transform.localScale = new Vector3(((((float)xsize + 1) / 2) / 10), (((float)ysize - 1) / 2) / 10, 1);

		Camera.main.orthographicSize = ysize / 1.3f;
		Camera.main.transform.position = transform.position;
		currentx = Mathf.RoundToInt(xsize / 2);
		currenty = Mathf.RoundToInt(ysize / 2);
		playerWinPos.x = Mathf.RoundToInt(xsize / 2);
		playerWinPos.y = 0;
		aiWinPos.x = Mathf.RoundToInt(xsize / 2);
		aiWinPos.y = ysize - 1;
		fillGrid();
		G[currentx, currenty].visited = true;
		resetBallPosition();

		for (int x = 0; x < xsize; x++)
		{
			for (int y = 0; y < ysize; y++)
			{
				GameObject temp;
				temp = Instantiate(Dots, new Vector3(x, y, 0), Quaternion.identity);
				if (G[x, y].isCornerNode && G[x, y].isWalkable)
				{
					temp.GetComponent<SpriteRenderer>().color = Color.white;
					//temp.transform.localScale = new Vector3(temp.transform.localScale.x + 0.3f, temp.transform.localScale.y + 0.3f, 1);
					checkAndAddLineRenderer(x, y);
				}
				else if (x == playerWinPos.x && y == playerWinPos.y || x == aiWinPos.x && y == aiWinPos.y)
				{
					temp.GetComponent<SpriteRenderer>().color = Color.white;
					List<NODE> N = getMoves(x, y);
					foreach (NODE node in N)
					{
						if (node.isCornerNode == true)
						{
							addLineRenderer(new Vector3(node.x, node.y, -1), new Vector3(node.x, y, -1), false);
						}
					}
				}
				else if (G[x, y].isWalkable && !G[x, y].isCornerNode)
					temp.GetComponent<SpriteRenderer>().color = Color.white;

			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		PLAYERTURN.gameObject.SetActive(playerOneTurn);
		AITURN.gameObject.SetActive(!playerOneTurn);
		if ((aiWon || playerWon || tie) && !gameEnded)
		{
			gameEnded = true;
			StartCoroutine("SHOWGAMEOVERCANVAS");
			if (aiWon == true)
				VERDICT.text = mGameMode == GameMode.SINGLE_PLAYER ? "YOU LOST!" : "PLAYER 2 WON!";

			else if (playerWon)
				VERDICT.text = mGameMode == GameMode.SINGLE_PLAYER ? "VICTORY IS YOURS!" : "PLAYER 1 WON!";

			else
				VERDICT.text = "ITS A TIE!";
		}


		if (!gameEnded)
		{
			if (mGameMode == GameMode.LOCAL_MULTUPLAYER || playerOneTurn)
			{
				G[currentx, currenty].visited = true;

				if (Input.GetMouseButton(0))
				{
					MouseEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

					if (!IsClickValid(MouseEndPos))
					{
						L1.enabled = false;
						MouseStartpos = new Vector2(-1, -1);
						MouseEndPos = new Vector2(-1, -1);
						GuideQuad.gameObject.SetActive(false);
						availableMoves.Clear();
						return;
					}

					if (availableMoves.Count == 0)
					{
						availableMoves = getMoves(currentx, currenty);
					}
					MouseStartpos = new Vector2(currentx, currenty);
					L1.enabled = true;
					L1.SetPosition(0, new Vector3(MouseStartpos.x, MouseStartpos.y, -0.5f));
					L1.SetPosition(1, new Vector3(MouseEndPos.x, MouseEndPos.y, -0.5f));
					GuideQuad.gameObject.SetActive(true);

					NODE shorestNODE = getShortestDistance(availableMoves, MouseStartpos, MouseEndPos);
					GuideQuad.transform.position = new Vector3(shorestNODE.x, shorestNODE.y, 0);
				}

				if (Input.GetMouseButtonUp(0))
				{
					if (!GuideQuad.gameObject.activeInHierarchy) return;

					L1.enabled = false;
					MouseStartpos = new Vector2(-1, -1);
					MouseEndPos = new Vector2(-1, -1);
					moveUP(GuideQuad.transform.position);
					GuideQuad.gameObject.SetActive(false);
					availableMoves.Clear();
				}

			}
		}
	}

	private bool IsClickValid(Vector2 mouseEndPos)
	{
		return mouseEndPos.y >= 0 && mouseEndPos.y < ysize;
	}

	private double shortestDistance(float x1, float y1, float x2, float y2, float x3, float y3)
	{
		float px = x2 - x1;
		float py = y2 - y1;
		float temp = (px * px) + (py * py);
		float u = ((x3 - x1) * px + (y3 - y1) * py) / (temp);
		if (u > 1)
		{
			u = 1;
		}
		else if (u < 0)
		{
			u = 0;
		}
		float x = x1 + u * px;
		float y = y1 + u * py;

		float dx = x - x3;
		float dy = y - y3;
		double dist = Mathf.Sqrt(dx * dx + dy * dy);
		return dist;

	}
	void checkAndAddLineRenderer(int x, int y)
	{
		if (x > 1 && x < xsize - 1 && G[x - 1, y].isCornerNode)
		{
			addLineRenderer(new Vector3(x, y, -0.5f), new Vector3(x - 1, y, -0.5f), false);
		}
		if (y > 0 && G[x, y - 1].isCornerNode)
		{
			addLineRenderer(new Vector3(x, y, -0.5f), new Vector3(x, y - 1, -0.5f), false);
		}
	}
	IEnumerator callAIMove()
	{

		yield return new WaitForSeconds(0.1f);
		if (getMoves(currentx, currenty).Count <= 0)
		{
			tie = true;
		}
		else
			aiMove();

	}

	void moveUP(Vector2 newpos)
	{
		if ((mGameMode == GameMode.LOCAL_MULTUPLAYER || playerOneTurn) && isValid(currentx, currenty, Mathf.RoundToInt(newpos.x), Mathf.RoundToInt(newpos.y)))
		{
			addLineRenderer(new Vector3(currentx, currenty, 0), new Vector3(newpos.x, newpos.y, 0), false);

			G[currentx, currenty].visitedFromOrTo.Add(newpos);
			G[Mathf.RoundToInt(newpos.x), Mathf.RoundToInt(newpos.y)].visitedFromOrTo.Add(new Vector2(currentx, currenty));

			currentx = Mathf.RoundToInt(newpos.x);
			currenty = Mathf.RoundToInt(newpos.y);
			resetBallPosition();

			if (!G[currentx, currenty].visited)
			{
				G[currentx, currenty].visited = true;
				playerOneTurn = !playerOneTurn;
				if (mGameMode == GameMode.SINGLE_PLAYER)
					StartCoroutine("callAIMove");

			}
			else
			{
				if (getMoves(currentx, currenty).Count <= 0)
					tie = true;
			}
		}
	}


	NODE getShortestDistance(List<NODE> N, Vector2 lineStart, Vector2 lineEnd)
	{
		double shortestDist = 1000;
		NODE returnNODE = N[0];
		foreach (NODE node in N)
		{
			double newCompareableDist;
			newCompareableDist = shortestDistance(lineStart.x, lineStart.y, lineEnd.x, lineEnd.y, node.x, node.y);
			if (newCompareableDist < shortestDist)
			{
				shortestDist = newCompareableDist;
				returnNODE = node;
			}
		}
		return returnNODE;
	}

	IEnumerator SHOWGAMEOVERCANVAS()
	{
		yield return new WaitForSeconds(1);
		EnableGameOverMenu(true);
	}

	public void EnableGameOverMenu(bool val)
	{
		gameOverCanvas.enabled = val;
	}

	void fillGrid()
	{
		G = new NODE[xsize, ysize];
		for (int x = 0; x < xsize; x++)
		{
			for (int y = 0; y < ysize; y++)
			{
				if (x == 1 || y == 0 || x == xsize - 2 || y == ysize - 1 || y == 1 || y == ysize - 2)
				{
					if (y == 0 || y == ysize - 1)
					{
						if ((x == playerWinPos.x))
							G[x, y] = new NODE(x, y, true, false, true);
						else
							G[x, y] = new NODE(x, y, false, false, false);
					}
					else if (y == 1 || y == ysize - 2)
					{
						if (x == playerWinPos.x)
							G[x, y] = new NODE(x, y, false, false, true);
						else
							G[x, y] = new NODE(x, y, true, true, true);
					}
					else
						G[x, y] = new NODE(x, y, true, true, true);

				}
				else if (x < 1 || x > xsize - 2)
					G[x, y] = new NODE(x, y, false, false, false);

				else
					G[x, y] = new NODE(x, y, false, false, true);
			}
		}

	}


	bool checkContains(int x1, int y1, int x2, int y2)
	{

		for (int i = 0; i < G[x1, y1].visitedFromOrTo.Count; i++)
		{
			if (G[x1, y1].visitedFromOrTo[i].x == x2 && G[x1, y1].visitedFromOrTo[i].y == y2)
			{
				return true;
			}
		}
		return false;
	}

	List<NODE> getMoves(int x, int y)
	{
		List<NODE> validNODES = new List<NODE>();

		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (!(i == 0 && j == 0))
					if (isValid(x, y, x + j, y + i))
					{
						validNODES.Add(G[x + j, y + i]);
					}
			}

		}
		return validNODES;
	}

	bool isValid(int initx, int inity, int finx, int finy)
	{
		if (finx >= 0 && finx <= xsize - 1 && finy >= 0 && finy <= ysize - 1)
		{
			if (G[finx, finy].isWalkable)
			{
				if (G[finx, finy].visited == false)
				{
					return true;
				}
				else
				{
					if (G[finx, finy].isCornerNode)
					{
						if (G[initx, inity].isCornerNode == true)
						{
							if (Vector2.Distance(new Vector2(initx, inity), new Vector2(finx, finy)) <= 1)
								return false;
						}
					}
					if (checkContains(initx, inity, finx, finy))
					{
						return false;
					}

				}
				return true;
			}
		}
		return false;
	}


	void addLineRenderer(Vector3 Pos1, Vector3 Pos2, bool green)
	{
		Pos1 = new Vector3(Pos1.x, Pos1.y, -0.5f);
		Pos2 = new Vector3(Pos2.x, Pos2.y, -0.5f);
		LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
		lineRenderer.material = LINENMAT;
		lineRenderer.widthMultiplier = 0.05f;
		lineRenderer.positionCount = 2;
		lineRenderer.startWidth = 0.1f;
		lineRenderer.endWidth = 0.1f;
		if (green)
		{
			lineRenderer.startColor = Color.green;
			lineRenderer.endColor = Color.green;
		}
		else
		{
			lineRenderer.startColor = Color.white;
			lineRenderer.endColor = Color.white;
		}

		lineRenderer.SetPosition(0, Pos1);
		lineRenderer.SetPosition(1, Pos2);
	}

	void moveBall()
	{
		resetBallPosition();
	}

	void resetBallPosition()
	{
		Ball.transform.position = new Vector3((currentx), currenty, -1);
	}

	void aiMove()
	{
		if (getMoves(currentx, currenty).Count > 0)
		{
			callpopulateAndEvalTree(G, 8, currentx, currenty);
			NODE AIMOVE = returnBestMove();
			addLineRenderer(new Vector2(currentx, currenty), new Vector2(AIMOVE.x, AIMOVE.y), false);

			G[currentx, currenty].visitedFromOrTo.Add(new Vector2(AIMOVE.x, AIMOVE.y));
			G[AIMOVE.x, AIMOVE.y].visitedFromOrTo.Add(new Vector2(currentx, currenty));
			currentx = AIMOVE.x;
			currenty = AIMOVE.y;
			if (getMoves(currentx, currenty).Count <= 0)
			{
				tie = true;
			}
			if (!aiWon)
			{
				if (G[currentx, currenty].visited)
					aiMove();
				else
				{
					G[AIMOVE.x, AIMOVE.y].visited = true;
					moveBall();
					playerOneTurn = true;
				}
			}
			else
				moveBall();
		}
		else
			tie = true;
	}

	void callpopulateAndEvalTree(NODE[,] G, int _depth, int _currentX, int _currentY)
	{
		movesCheckList = new List<MOVESandPOSITIONS>();
		checkedNodes = new List<Vector2>();
		alphaBetaMax(-100000000, 100000000, _depth, _depth, _currentX, _currentY);
	}

	NODE returnBestMove()
	{
		float MAX = -100000;
		int best = -1;


		float lowestScore = 0;
		for (int i = 0; i < movesCheckList.Count; i++)
		{
			if (G[movesCheckList[i].pos.x, movesCheckList[i].pos.y].isCornerNode)
			{
				movesCheckList[i].score += 2;
			}

			if (MAX < movesCheckList[i].score)
			{
				MAX = movesCheckList[i].score;
				best = i;
				lowestScore = movesCheckList[i].score;
			}
		}

		float leastDist = 1000;
		for (int i = 0; i < movesCheckList.Count; i++)
		{
			if (movesCheckList[i].score == lowestScore)
			{
				if (leastDist > Vector2.Distance(aiWinPos, new Vector2(movesCheckList[i].pos.x, movesCheckList[i].pos.y)))
				{
					best = i;
					leastDist = Vector2.Distance(aiWinPos, new Vector2(movesCheckList[i].pos.x, movesCheckList[i].pos.y));

				}

			}
		}

		if (best > -1)
			return movesCheckList[best].pos;

		NODE blank = new NODE();
		blank.x = 0;
		blank.y = 0;
		return blank;
	}

	//------------------- MAXIMIZING

	int alphaBetaMax(int alpha, int beta, int depthleft, int startDepth, int _currentX, int _currentY)
	{
		List<NODE> pointsAvailable = getMoves(_currentX, _currentY);

		if ((playerWon) || (aiWon) || (pointsAvailable.Count == 0 || depthleft == 0))
		{
			return (ysize - (evaluate(_currentX, _currentY)));
		}
		for (int i = 0; i < pointsAvailable.Count; i++)
		{

			NODE checkPos = pointsAvailable[i];

			bool tempBool, TEMPBOOL1;
			tempBool = G[_currentX, _currentY].visited;
			TEMPBOOL1 = G[checkPos.x, checkPos.y].visited;
			List<Vector2> curList = new List<Vector2>(); List<Vector2> checkList = new List<Vector2>();
			if (G[_currentX, _currentY].visitedFromOrTo.Count > 0)
				for (int j = 0; j < G[_currentX, _currentY].visitedFromOrTo.Count; j++)
				{
					curList.Add(new Vector2(G[_currentX, _currentY].visitedFromOrTo[j].x, G[_currentX, _currentY].visitedFromOrTo[j].y));
				}
			if (G[checkPos.x, checkPos.y].visitedFromOrTo.Count > 0)
				for (int j = 0; j < G[checkPos.x, checkPos.y].visitedFromOrTo.Count; j++)
				{
					checkList.Add(new Vector2(G[checkPos.x, checkPos.y].visitedFromOrTo[j].x, G[checkPos.x, checkPos.y].visitedFromOrTo[j].y));
				}

			G[_currentX, _currentY].visited = true;
			// G[checkPos.x, checkPos.y].visited = true;


			if (!checkContains(checkPos.x, checkPos.y, _currentX, _currentY))
			{
				G[checkPos.x, checkPos.y].visitedFromOrTo.Add(new Vector2(_currentX, _currentY));
			}
			if (!checkContains(_currentX, _currentY, checkPos.x, checkPos.y))
			{
				G[_currentX, _currentY].visitedFromOrTo.Add(new Vector2(checkPos.x, checkPos.y));
			}

			int returnScore = 0;

			if (!tempBool && !G[_currentX, _currentY].isCornerNode)
				returnScore += alphaBetaMin(alpha, beta, depthleft - 1, startDepth, checkPos.x, checkPos.y);
			else
				returnScore += alphaBetaMax(alpha, beta, depthleft - 1, startDepth, checkPos.x, checkPos.y);

			G[checkPos.x, checkPos.y].visitedFromOrTo = new List<Vector2>();
			G[_currentX, _currentY].visitedFromOrTo = new List<Vector2>();
			G[_currentX, _currentY].visited = tempBool;
			G[checkPos.x, checkPos.y].visited = TEMPBOOL1;

			if (checkList.Count > 0)
				for (int j = 0; j < checkList.Count; j++)
					G[checkPos.x, checkPos.y].visitedFromOrTo.Add(new Vector2(checkList[j].x, checkList[j].y));
			if (curList.Count > 0)
				for (int j = 0; j < curList.Count; j++)
					G[_currentX, _currentY].visitedFromOrTo.Add(new Vector2(curList[j].x, curList[j].y));

			G[_currentX, _currentY].visited = tempBool;

			if (depthleft == startDepth)
			{
				MOVESandPOSITIONS m = new MOVESandPOSITIONS(G[checkPos.x, checkPos.y], returnScore);
				movesCheckList.Add(m);
			}

			if (returnScore >= beta)
				return beta; // fail hard beta-cutoff
			if (returnScore > alpha)
				alpha = returnScore; // alpha acts like max in MiniMax
		}
		return alpha;
	}

	//------------MINIMIZING

	int alphaBetaMin(int alpha, int beta, int depthleft, int startDepth, int _currentX, int _currentY)
	{
		List<NODE> pointsAvailable = getMoves(_currentX, _currentY);
		if ((playerWon) || (aiWon) || (pointsAvailable.Count == 0 || depthleft == 0))
			return -(evaluate(_currentX, _currentY));

		for (int i = 0; i < pointsAvailable.Count; i++)
		{
			NODE checkPos = pointsAvailable[i];

			bool tempBool, TEMPBOOL1;
			tempBool = G[_currentX, _currentY].visited;
			TEMPBOOL1 = G[checkPos.x, checkPos.y].visited;
			List<Vector2> curList = new List<Vector2>(); List<Vector2> checkList = new List<Vector2>();
			if (G[_currentX, _currentY].visitedFromOrTo.Count > 0)
				for (int j = 0; j < G[_currentX, _currentY].visitedFromOrTo.Count; j++)
					curList.Add(new Vector2(G[_currentX, _currentY].visitedFromOrTo[j].x, G[_currentX, _currentY].visitedFromOrTo[j].y));

			if (G[checkPos.x, checkPos.y].visitedFromOrTo.Count > 0)
				for (int j = 0; j < G[checkPos.x, checkPos.y].visitedFromOrTo.Count; j++)
					checkList.Add(new Vector2(G[checkPos.x, checkPos.y].visitedFromOrTo[j].x, G[checkPos.x, checkPos.y].visitedFromOrTo[j].y));

			G[_currentX, _currentY].visited = true;
			// G[checkPos.x, checkPos.y].visited = true;


			if (!checkContains(checkPos.x, checkPos.y, _currentX, _currentY))
				G[checkPos.x, checkPos.y].visitedFromOrTo.Add(new Vector2(_currentX, _currentY));

			if (!checkContains(_currentX, _currentY, checkPos.x, checkPos.y))
				G[_currentX, _currentY].visitedFromOrTo.Add(new Vector2(checkPos.x, checkPos.y));

			int returnScore = 0;
			if (!tempBool && !G[_currentX, _currentY].isCornerNode)
				returnScore = alphaBetaMax(alpha, beta, depthleft - 1, startDepth, checkPos.x, checkPos.y);
			else
				returnScore = alphaBetaMin(alpha, beta, depthleft - 1, startDepth, checkPos.x, checkPos.y);

			G[checkPos.x, checkPos.y].visitedFromOrTo = new List<Vector2>();
			G[_currentX, _currentY].visitedFromOrTo = new List<Vector2>();
			G[_currentX, _currentY].visited = tempBool;
			G[checkPos.x, checkPos.y].visited = TEMPBOOL1;

			if (checkList.Count > 0)
				for (int j = 0; j < checkList.Count; j++)
					G[checkPos.x, checkPos.y].visitedFromOrTo.Add(new Vector2(checkList[j].x, checkList[j].y));

			if (curList.Count > 0)
				for (int j = 0; j < curList.Count; j++)
					G[_currentX, _currentY].visitedFromOrTo.Add(new Vector2(curList[j].x, curList[j].y));


			G[_currentX, _currentY].visited = tempBool;

			if (returnScore <= alpha)
				return alpha; // fail hard alpha-cutoff
			if (returnScore < beta)
				beta = returnScore; // beta acts like min in MiniMax
		}
		return beta;
	}



	int evaluate(int _currentx, int _currenty)
	{
		int evaluatedScore = Mathf.RoundToInt(Vector2.Distance(aiWinPos, new Vector2(_currentx, _currenty)));

		if (_currentx == aiWinPos.x && _currenty == aiWinPos.y)
			evaluatedScore -= 10;

		else if (_currentx == playerWinPos.x && _currenty == playerWinPos.y)
			evaluatedScore += 10;

		return evaluatedScore;
	}




}
