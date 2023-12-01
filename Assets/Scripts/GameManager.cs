using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Events;
using Dan.Main;

public class GameManager : MonoBehaviour
{
    [Header("Wallpaper")]
    [SerializeField] private GameObject scrolling_wallpaper;
    [SerializeField] private Color wallpaper_colour;

    [Header("Grid")]
    [SerializeField] private Color board_cell_colour;
    [SerializeField] private int grid_x;
    [SerializeField] private int grid_y;
    [SerializeField] private GameObject board_prefab;
    [SerializeField] private GameObject gameBoardParent;

    [Header("Cells")]
    [SerializeField] private Cell grid_cell_prefab;
    private Cell[,] cellArray;

    [Header("Tiles")]
    [SerializeField] private Color starting_tile_colour;
    [SerializeField] private float hueOffset;
    [SerializeField] private Tile tile_prefab;

    [Header("Animation")]
    [SerializeField] private float animationDuration;
    [SerializeField] AnimationCurve tileSpawnCurve;
    [SerializeField] AnimationCurve tileMergeCurve;

    [Header("GameUI")]
    [SerializeField] private UIManager UIManager;
    [SerializeField] private GameObject GameUI_prefab;
    [SerializeField] private ParticleSystem fallingStars;
    private bool fallingStarsInUse = false;
    private GameObject gameUI;

    private int numTilesOnBoard;
    private int numTilesLimit;
    private bool isAnimating;

    private bool mergeFlag;
    private bool moveFlag;

    private bool isWinner;

    private float Score = 0;

    // Start is called before the first frame update
    void Start()
    {
        initializeGame();
    }

    public void newGame()
    {
        initializeGame();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if an animation is currently in progress
        if (isAnimating)
        {
            return; // Skip input handling
        }

        // Handle input if no animation is in progress
        if (Input.GetKeyDown("up"))
        {
            StartCoroutine(MoveTilesAndAnimate(Vector2Int.up, 0, grid_y - 1, 1, -1));
        }
        else if (Input.GetKeyDown("down"))
        {
            StartCoroutine(MoveTilesAndAnimate(Vector2Int.down, 0, 0, 1, 1));
        }
        else if (Input.GetKeyDown("left"))
        {
            StartCoroutine(MoveTilesAndAnimate(Vector2Int.left, 0, grid_y - 1, 1, -1));
        }
        else if (Input.GetKeyDown("right"))
        {
            StartCoroutine(MoveTilesAndAnimate(Vector2Int.right, grid_x - 1, grid_y - 1, -1, -1));
        }
    }

    private void spawnTesting()
    {
        // Spawn tile on cell
        Tile tile = Instantiate(tile_prefab);
        tile.name = "tile";
        tile.transform.position = cellArray[0, 0].transform.position;
        tile.spriteRenderer.color = starting_tile_colour;
        tile.transform.SetParent(gameBoardParent.transform);
        tile.cell = cellArray[0, 0];
        cellArray[0, 0].tile = tile;
        upgradeTile(tile);
        StartCoroutine(upgradeTileAnimation(tile));
        StartCoroutine(SpawnTileAnimation(tile.transform));

        Tile tile1 = Instantiate(tile_prefab);
        tile1.name = "tile";
        tile1.transform.position = cellArray[1, 0].transform.position;
        tile1.spriteRenderer.color = starting_tile_colour;
        tile1.transform.SetParent(gameBoardParent.transform);
        tile1.cell = cellArray[1, 0];
        cellArray[1, 0].tile = tile1;

        Tile tile2 = Instantiate(tile_prefab);
        tile2.name = "tile";
        tile2.transform.position = cellArray[2, 0].transform.position;
        tile2.spriteRenderer.color = starting_tile_colour;
        tile2.transform.SetParent(gameBoardParent.transform);
        tile2.cell = cellArray[2, 0];
        cellArray[2, 0].tile = tile2;
    }

    private IEnumerator MoveTilesAndAnimate(Vector2Int dir, int starting_X, int starting_Y, int increment_X, int increment_Y)
    {
        isAnimating = true; // Set animation flag

        moveAllTiles(dir, starting_X, starting_Y, increment_X, increment_Y);

        // Flags for moving and merging
        float waitDuration = 0;
        bool madeLegalMove = false;
        if (moveFlag)
        {
            madeLegalMove = true;
            waitDuration += animationDuration; // tile move

            if (mergeFlag)
            {
                waitDuration += animationDuration; //tile merge 
            }
        }

        // Made a legal move
        if (madeLegalMove)
        {
            spawnTile();
        }

        // Debug.Log("waitDuration  = " + waitDuration);
        yield return new WaitForSeconds(waitDuration);

        isAnimating = false; // Reset animation flag
        moveFlag = false; // Reset move flag
        mergeFlag = false; // Reset merge flag
        ResetTileMergeStatus();  //Reset merged tiles merge status
    }


    private int evaluateBoard()
    {
        // 0 -> game lost; 
        // 1 -> game won;
        // 2 -> game in progress;

        if (isWinner)
        {
            Debug.Log("isWinner");
            return 1;
        }

        // Full board and no moves available
        if (numTilesOnBoard == numTilesLimit && !isAvailableMove())
        {
            Debug.Log("Full board and no moves available");
            return 0;
        }

        //Debug.Log("default");
        return 2;
    }

    private bool isAvailableMove()
    {
        foreach (Cell cell in cellArray)
        {
            if (inBoundsAndMergingAdjacent(cell, Vector2Int.up) ||
            inBoundsAndMergingAdjacent(cell, Vector2Int.down) ||
            inBoundsAndMergingAdjacent(cell, Vector2Int.left) ||
            inBoundsAndMergingAdjacent(cell, Vector2Int.right))
            {
                return true;
            }
        }

        return false;
    }

    private bool inBoundsAndMergingAdjacent(Cell cell, Vector2Int dir)
    {
        Vector2Int tempCoords = cell.coordinates + dir;
        if (tempCoords.x < grid_x && tempCoords.x >= 0 && tempCoords.y < grid_y && tempCoords.y >= 0)
        {
            if (cell.tile.number == cellArray[tempCoords.x, tempCoords.y].tile.number)
            {
                // Merge still possible
                return true;
            }
        }

        return false;
    }


    // Debug tiles
    void logTiles()
    {
        StringBuilder sb = new StringBuilder();
        for (int y = cellArray.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < cellArray.GetLength(0); x++)
            {
                if (cellArray[x, y].tile)
                {
                    sb.Append(1);
                    sb.Append(' ');
                }
                else
                {
                    sb.Append(0);
                    sb.Append(' ');
                }
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    private void moveAllTiles(Vector2Int dir, int starting_X, int starting_Y, int increment_X, int increment_Y)
    {
        for (int y = starting_Y; y < grid_y && y >= 0; y = y + increment_Y)
        {
            for (int x = starting_X; x < grid_x && x >= 0; x = x + increment_X)
            {
                if (cellArray[x, y].tile)
                {
                    moveTile(cellArray[x, y].tile, dir);
                }
            }
        }
    }

    private void moveTile(Tile tile, Vector2Int dir)
    {
        Vector2Int originalCoordinates = tile.cell.coordinates;
        Vector2Int tempCoords = originalCoordinates;

        //Debug.Log($"Originalcoords = {originalCoordinates.ToString()}\ntempcoords = {tempCoords.ToString()}");

        bool mergeOpportunity = false;
        while (!isOutOfBounds(tempCoords, dir))
        {
            if (hasAdjacent(tempCoords, dir))
            {
                mergeOpportunity = true;
                break;
            }

            tempCoords += dir;
        }

        if (mergeOpportunity)
        {
            if (!cellArray[tempCoords.x + dir.x, tempCoords.y + dir.y].tile.isMerged && tile.number == cellArray[tempCoords.x + dir.x, tempCoords.y + dir.y].tile.number)
            {
                // Merge into next cell
                tempCoords += dir;
                moveFlag = true;
                mergeFlag = true;
                MergeAndLinkTile(tile, tempCoords);
            }
            else
            {
                // Transform to cell just before adjacent
                if (originalCoordinates != tempCoords)
                {
                    moveFlag = true;
                    TransformAndLinkTile(tile, tempCoords);
                }
            }
        }
        else
        {
            if (originalCoordinates != tempCoords)
            {
                //Transform to end
                moveFlag = true;
                TransformAndLinkTile(tile, tempCoords);
            }
        }

    }

    private void TransformAndLinkTile(Tile tile, Vector2Int tempCoords)
    {
        // Remove tile form original cell's tile
        tile.cell.tile = null;

        // Add tile to new cell'stile AND add cell to tile's cell
        tile.cell = cellArray[tempCoords.x, tempCoords.y];
        cellArray[tempCoords.x, tempCoords.y].tile = tile;

        // Start tile move animation
        Vector3 targetPosition = cellArray[tempCoords.x, tempCoords.y].transform.position;
        StartCoroutine(AnimateTileMovement(tile, targetPosition));
    }

    private void MergeAndLinkTile(Tile tile, Vector2Int adjCoords)
    {
        // Make duplicate for animation
        GameObject decoyTile = Instantiate(tile.gameObject);
        decoyTile.transform.position = tile.transform.position;

        // Derefence cell's tile and destroy tile
        tile.cell.tile = null;
        Destroy(tile.gameObject);

        upgradeTile(cellArray[adjCoords.x, adjCoords.y].tile);
        cellArray[adjCoords.x, adjCoords.y].tile.isMerged = true;
        numTilesOnBoard--;

        // Start tile merge animation
        Vector3 targetPosition = cellArray[adjCoords.x, adjCoords.y].transform.position;
        StartCoroutine(AnimateTileMerging(decoyTile.GetComponent<Tile>(), cellArray[adjCoords.x, adjCoords.y].tile, targetPosition));
    }

    private void ResetTileMergeStatus()
    {
        foreach (Cell cell in cellArray)
        {
            if (cell.tile)
            {
                cell.tile.isMerged = false;
            }
        }
    }

    private IEnumerator AnimateTileMerging(Tile decoy, Tile destinationTile, Vector3 targetPosition)
    {
        // Start animation
        Vector3 startPosition = decoy.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            decoy.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        // Ensure the tile is perfectly positioned at the target position
        decoy.transform.position = targetPosition;
        Destroy(decoy.gameObject);

        // Update Scores
        Score += destinationTile.number;
        gameUI.GetComponent<GameUI>().Score.text = "Score: " + Score;

        if (Score > LeaderBoard.HIGHSCORE)
        {
            LeaderBoard.HIGHSCORE = Score;
            gameUI.GetComponent<GameUI>().Best.text = "Best: " + Score;
        }

        // Start upgrade animation
        StartCoroutine(upgradeTileAnimation(destinationTile));
    }

    private IEnumerator upgradeTileAnimation(Tile destinationTile)
    {
        // Set visual number
        destinationTile.textMeshPro.text = destinationTile.number.ToString();

        // Change HUE
        Color.RGBToHSV(destinationTile.spriteRenderer.color, out float h, out float s, out float v);
        float newHue = (h + hueOffset) % 1f;
        Color newColour = Color.HSVToRGB(newHue, s, v);
        destinationTile.spriteRenderer.color = newColour;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / animationDuration;
            float normalizedTime = Mathf.Clamp01(t);

            // Evaluate the animation curve to get the interpolated scale
            float interpolatedScale = tileMergeCurve.Evaluate(normalizedTime);

            // Apply the scale to the tile transform
            destinationTile.transform.localScale = new Vector3(interpolatedScale, interpolatedScale, 1f);

            yield return null;
        }

        // Check if the tile is null before settling its position
        if (destinationTile != null)
        {
            destinationTile.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private IEnumerator SpawnTileAnimation(Transform tileTransform)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animationDuration;
            float normalizedTime = Mathf.Clamp01(t);

            // Evaluate the animation curve to get the interpolated scale
            float interpolatedScale = tileSpawnCurve.Evaluate(normalizedTime);

            // Apply the scale to the tile transform
            tileTransform.localScale = new Vector3(interpolatedScale, interpolatedScale, 1f); // FIXME
            yield return null;
        }

        // Check if the tileTransform is null before settling its scale
        if (tileTransform != null)
        {
            // Settle the scale to the target scale
            tileTransform.localScale = new Vector3(1f, 1f, 1f);
        }

        // Check for win or full board
        int evalValue = evaluateBoard();
        switch (evalValue)
        {
            case 0:
                // Game lost
                displayLoseScreen();
                break;

            case 1:
                // Game won
                displayWinOrContinue();
                isWinner = false;
                break;

            default:
                break;
        }
    }



    private IEnumerator AnimateTileMovement(Tile tile, Vector3 targetPosition)
    {
        Vector3 startPosition = tile.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            tile.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        // Check if the tile is null before settling its position
        if (tile != null)
        {
            // Settle the scale to the target scale
            tile.transform.position = targetPosition;
        }
    }

    private bool hasAdjacent(Vector2Int coordinates, Vector2Int dir)
    {
        Vector2Int adjacentCoordinates = coordinates + dir;
        if (cellArray[adjacentCoordinates.x, adjacentCoordinates.y].tile)
        {
            return true;
        }

        return false;
    }

    private bool isOutOfBounds(Vector2Int coordinates, Vector2Int dir)
    {
        Vector2Int outOfBoundCoordinates = coordinates + dir;
        if (outOfBoundCoordinates.x < grid_x && outOfBoundCoordinates.x >= 0 && outOfBoundCoordinates.y < grid_y && outOfBoundCoordinates.y >= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void spawn2InitialTiles()
    {
        spawnTile();
        spawnTile();
    }

    private void spawnTile()
    {
        if (numTilesOnBoard + 1 <= numTilesLimit)
        {
            // Legal spawn
            numTilesOnBoard++;

            // Get available cell
            List<Cell> availableCells = getAvailableCells();
            int randomSpawn = UnityEngine.Random.Range(0, availableCells.Count); // 0 to [availableMoves.Count-1] (inclusive)
            Cell randomCell = availableCells[randomSpawn];

            // Spawn tile on cell
            Tile tile = Instantiate(tile_prefab);
            tile.name = "tile";
            tile.transform.position = randomCell.transform.position;
            tile.spriteRenderer.color = starting_tile_colour;
            tile.transform.SetParent(gameBoardParent.transform);

            // Link cell and tile
            tile.cell = randomCell;
            randomCell.tile = tile;

            // Start on 4
            int randomUpgrade = UnityEngine.Random.Range(1, 11); // 1 to 10 (inclusive)
            if (randomUpgrade <= 2)
            {
                // 20%
                upgradeTile(tile);
                StartCoroutine(upgradeTileAnimation(tile));
            }

            // Animate tile spawn
            StartCoroutine(SpawnTileAnimation(tile.transform));

        }
        else
        {
            Debug.Log("Cannot spawn tile. Limit reached!");
        }
    }

    private List<Cell> getAvailableCells()
    {
        List<Cell> cells = new List<Cell>();
        foreach (Cell cell in cellArray)
        {
            if (!cell.tile)
            {
                cells.Add(cell);
            }
        }
        return cells;
    }

    private void upgradeTile(Tile tile)
    {
        // Double number
        tile.number = tile.number * 2;

        if (tile.number == 2048)
        {
            isWinner = true;
            Debug.Log("WE HAVE A WINNER!");
        }
    }

    private void initializeGame()
    {
        // Reset variables
        numTilesOnBoard = 0;
        isAnimating = false;
        mergeFlag = false;
        moveFlag = false;
        isWinner = false;
        Score = 0;

        // Destroy already created cells, tiles and panel
        foreach (Transform child in gameBoardParent.transform)
        {
            Debug.Log(child.name);
            Destroy(child.gameObject);
        }

        // Initialize cells, tiles and panel
        if (grid_x >= 1 && grid_y >= 1)
        {
            // Set number of tiles to 0
            numTilesLimit = grid_x * grid_y;
            numTilesOnBoard = 0;

            // Spawn boardPanel
            GameObject boardPanel = Instantiate(board_prefab);
            boardPanel.name = "Panel";

            // Sprite size and padding
            float spriteSize = grid_cell_prefab.GetComponent<SpriteRenderer>().bounds.size.x;
            float betweenCellPadding = spriteSize * 0.2f; // Padding between cells

            // Initialize cellArray and tile States
            cellArray = new Cell[grid_x, grid_y];

            // Create empty gameObject for cells
            GameObject cellsGrid = new GameObject("Cells");

            for (int y = 0; y < grid_y; y++)
            {
                for (int x = 0; x < grid_x; x++)
                {
                    // Create cell
                    Cell cell = Instantiate(grid_cell_prefab);
                    cell.GetComponent<SpriteRenderer>().color = board_cell_colour;
                    cell.name = $"Cell [{x}, {y}]";
                    cell.transform.position = new Vector3((x * spriteSize) + (x * betweenCellPadding), (y * spriteSize) + (y * betweenCellPadding));
                    cell.transform.SetParent(cellsGrid.transform);
                    cell.tile = null;
                    cell.coordinates = new Vector2Int(x, y);

                    // Add cell to cell array
                    cellArray[x, y] = cell;
                }
            }

            // Padding for panel
            float panelPadding = spriteSize * 0.6f; // Padding on sides of Panel
            float newWidth = (grid_x * spriteSize) + (betweenCellPadding * grid_x) + panelPadding;
            float newHeight = (grid_y * spriteSize) + (betweenCellPadding * grid_y) + panelPadding * 1.25f;
            float panelSize = newHeight;

            // Center Panel to generated grid of cells
            Vector3 middlePointOfGrid = Vector3.Lerp(cellArray[0, 0].transform.position, cellArray[grid_x - 1, grid_y - 1].transform.position, 0.5f);
            const float constantShadowSize = 0.13f;
            boardPanel.transform.position = new Vector3(middlePointOfGrid.x, middlePointOfGrid.y - constantShadowSize, 1);
            boardPanel.GetComponent<SpriteRenderer>().size = new Vector2(newWidth, newHeight);
            boardPanel.GetComponent<SpriteRenderer>().color = board_cell_colour;

            // Make GameBoard the parent of cells and boardpanel
            boardPanel.transform.SetParent(gameBoardParent.transform);
            cellsGrid.transform.SetParent(gameBoardParent.transform);

            // Place gameUI at top
            gameUI = Instantiate(GameUI_prefab);
            gameUI.transform.SetParent(gameBoardParent.transform);
            gameUI.name = "GameUI";
            gameUI.transform.position += new Vector3(middlePointOfGrid.x, middlePointOfGrid.y + panelSize / 2f, middlePointOfGrid.z);

            // Display Scores
            gameUI.GetComponent<GameUI>().Score.text = "Score: " + Score;
            gameUI.GetComponent<GameUI>().Best.text = "Best: " + LeaderBoard.HIGHSCORE;

            // Correctly scale the gameUI
            float special = (grid_x + grid_y) / 2f;
            float scaleFactor = (spriteSize * (special) + betweenCellPadding * ((special) - 1f) + panelPadding) / (spriteSize * 4 + betweenCellPadding * 3 + panelPadding);
            gameUI.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

            // Get heighest y value
            Transform gameUIchild = gameUI.transform.Find("Title");
            SpriteRenderer childSpriteRenderer = gameUIchild.GetComponent<SpriteRenderer>();
            float top_Y_coordinate = gameUIchild.position.y + childSpriteRenderer.bounds.size.y * 0.5f;

            // Get lowest y value
            float bottom_Y_coordinate = middlePointOfGrid.y - (panelSize / 2f);

            // Get middle center of 2 Y's
            Vector3 top = new Vector3(middlePointOfGrid.x, top_Y_coordinate, middlePointOfGrid.z);
            Vector3 bottom = new Vector3(middlePointOfGrid.x, bottom_Y_coordinate, middlePointOfGrid.z);
            Vector3 centerPointForCamera = Vector3.Lerp(top, bottom, 0.5f);

            // Fix camera to generated grid
            fixCameraToGeneratedGrid(centerPointForCamera, betweenCellPadding, spriteSize, Mathf.Abs(top_Y_coordinate - bottom_Y_coordinate));

            // Spawn 2 starting tiles
            spawn2InitialTiles();
        }
        else
        {
            Debug.Log("grid_x and grid_y must be >= 1");
        }
    }

    private void fixCameraToGeneratedGrid(Vector3 centerPointForCamera, float betweenCellPadding, float spriteSize, float gameSize)
    {
        // Center camera to grid
        Camera.main.transform.position = new Vector3(centerPointForCamera.x, centerPointForCamera.y, -10f);

        // Set camera orthographic size
        float gridSizeX = (grid_x * spriteSize) + (grid_x * betweenCellPadding);
        float gridSizeY = (grid_y * spriteSize) + (grid_y * betweenCellPadding);
        float orthographicSize = Mathf.Max(gridSizeX / (2f * Camera.main.aspect), gameSize / 2f);

        //  float orthographicSize = gameSize / 2f;
        float cameraPadding = gameSize * 0.1f; // Camera padding on top and bottom
        orthographicSize += cameraPadding;
        Camera.main.orthographicSize = orthographicSize;

        // Setup Particle effects

        float cameraWidth = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, Camera.main.nearClipPlane)).x - Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane)).x;
        float particleShapeRadius = cameraWidth * 0.5f;
        var shape = fallingStars.shape;
        shape.radius = particleShapeRadius;

        fallingStars.transform.position = new Vector3(centerPointForCamera.x, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, Camera.main.nearClipPlane)).y, 1);
    }

    ////////////////////////////////////////////////////////////////////////////

    private void displayWinOrContinue()
    {
        StartCoroutine(EnableAndDestroyFallingStars());
    }

    private IEnumerator EnableAndDestroyFallingStars()
    {
        // Enable the targetObject
        if (fallingStarsInUse)
        {
            yield break;
        }

        fallingStarsInUse = true;
        fallingStars.gameObject.SetActive(true);

        float totalDuration = fallingStars.main.duration + fallingStars.main.startLifetime.constantMax;

        // Wait for the specified delay
        yield return new WaitForSeconds(totalDuration);

        // Destroy the targetObject
        fallingStars.gameObject.SetActive(false);
        fallingStarsInUse = false;
    }

    private void displayLoseScreen()
    {
        //TODO:
        StartCoroutine(displayLoseMenuAfterDelay(1f));
    }

    private IEnumerator displayLoseMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UIManager.displayLoseScreen();
    }
}
