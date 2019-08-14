//

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class BoardBuilderWindow : EditorWindow
{
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    private const string kSizeXName = "Editor.SizeX";
    private const string kSizeYName = "Editor.SizeY";
    private const string kYNoiseName = "Editor.YNoise";

    private const string kCellName = "Editor.CellPrefab";
    private const string kWallName = "Editor.WallPrefab";
    private const string kHomebaseName = "Editor.HomebasePrefab";
    private const string kMouseSpawnerName = "Editor.MouseSpawnerPrefab";
    private const string kCatSpawnerName = "Editor.CatSpawnerPrefab";

    private int m_SizeX = 100;
    private int m_SizeY = 100;
    private float m_YNoise = 0.05f;

    private GameObject m_CellPrefab = null;
    private GameObject m_WallPrefab = null;
    private GameObject m_HomebasePrefab = null;
    private GameObject m_MouseSpawnerPrefab = null;
    private GameObject m_CatSpawnerPrefab = null;

    private bool m_IsGenerating = false;
    private int m_GeneratingProgress = 0;

    // Add menu named "My Window" to the Window menu
    [MenuItem("LabRats/Board Builder")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        BoardBuilderWindow window = (BoardBuilderWindow)EditorWindow.GetWindow(typeof(BoardBuilderWindow));
        window.Show();
    }

    /// <summary>
    /// Called when activated
    /// </summary>
    protected void OnEnable()
    {
        LoadAll();
    }

    /// <summary>
    /// Draw the window
    /// </summary>
    protected void OnGUI()
    {
        GUILayout.Label("Board Builder");

        GUILayout.Space(5.0f);
        GUILayout.Label("BOARD");

        IntField(kSizeXName, "Size X:", ref m_SizeX);
        IntField(kSizeYName, "Size Y:", ref m_SizeY);
        FloatField(kYNoiseName, "Y Noise:", ref m_YNoise);

        GUILayout.Space(5.0f);
        GUILayout.Label("PREFABS");

        ObjectField(kCellName, "Cell Prefab", ref m_CellPrefab);
        ObjectField(kWallName, "Wall Prefab", ref m_WallPrefab);
        ObjectField(kHomebaseName, "Homebase Prefab", ref m_HomebasePrefab);
        ObjectField(kMouseSpawnerName, "Mouse Spawner Prefab", ref m_MouseSpawnerPrefab);
        ObjectField(kCatSpawnerName, "Cat Spawner Prefab", ref m_CatSpawnerPrefab);

        GUILayout.Space(25.0f);

        if (m_IsGenerating)
        {
            var maxProgress = m_SizeX * m_SizeY;
            GUILayout.Label("Progress: " + m_GeneratingProgress + " / " + maxProgress);

            GUILayout.Space(5.0f);
            if (GUILayout.Button("Stop generation!"))
            {
                StopGenerateBoard();
            }
        }
        else
        {
            if (GUILayout.Button("Generate Board"))
            {
                GenerateBoard();
            }
        }
    }

    /// <summary>
    /// Load all fields
    /// </summary>
    public void LoadAll()
    {
        LoadInt(kSizeXName, ref m_SizeX);
        LoadInt(kSizeYName, ref m_SizeY);
        LoadFloat(kYNoiseName, ref m_YNoise);

        LoadObject(kCellName, ref m_CellPrefab);
        LoadObject(kWallName, ref m_WallPrefab);
        LoadObject(kHomebaseName, ref m_HomebasePrefab);
        LoadObject(kMouseSpawnerName, ref m_MouseSpawnerPrefab);
        LoadObject(kCatSpawnerName, ref m_CatSpawnerPrefab);
    }

    /// <summary>
    /// Generates the board
    /// </summary>
    private void GenerateBoard()
    {
        if (IsObjectInvalid("Cell", m_CellPrefab)) return;
        if (IsObjectInvalid("Wall", m_WallPrefab)) return;

        m_IsGenerating = true;
        m_GeneratingProgress = 0;

        StartCoroutine("GenerateBoardInternal");
    }

    /// <summary>
    /// Stops to generate the board
    /// </summary>
    private void StopGenerateBoard()
    {
        StopCoroutine("GenerateBoardInternal");
        m_IsGenerating = false;
    }

    /// <summary>
    /// Coroutine for generate the board
    /// </summary>
    private IEnumerator GenerateBoardInternal()
    {
        // Get the board object and clean up all children
        var boardTransform = FindOrCreateBoardObject();
        EditorUtil.DestroyChildren(boardTransform);

        // Generate the board floor and external walls
        for (int z = 0; z < m_SizeY; ++z)
        {
            for (int x = 0; x < m_SizeX; ++x)
            {
                var coord = new Vector2Int(x, z);

                var index = (coord.x + coord.y) % 2 == 0 ? 1 : 0;

                var obj = Instantiate<GameObject>(m_CellPrefab);
                obj.name = "board_" + coord;
                obj.transform.SetParent(boardTransform);

                // Position the block
                obj.transform.localPosition = new Vector3(
                    coord.x,
                    UnityEngine.Random.value * m_YNoise,
                    coord.y);

                PlaceWall(Direction.North, coord, boardTransform, coord.y == m_SizeY - 1);
                PlaceWall(Direction.East, coord + Vector2Int.right, boardTransform, coord.x == m_SizeX - 1);
                PlaceWall(Direction.South, coord, boardTransform, coord.y == 0);
                PlaceWall(Direction.West, coord, boardTransform, coord.x == 0);

                m_GeneratingProgress++;
            }

            yield return null;
        }

        Debug.Log("Board Generated!");
        m_IsGenerating = false;
    }

    /// <summary>
    /// Find or create the board object
    /// </summary>
    /// <returns></returns>
    private Transform FindOrCreateBoardObject()
    {
        var obj = GameObject.Find("Board");
        if (obj == null)
        {
            obj = new GameObject();
            obj.name = "Board";
        }

        return obj.transform;
    }

    /// <summary>
    /// Place a wall in the given coord with the given direction
    /// </summary>
    /// <param name="place">True to spawn the wall false to skip spawning</param>
    private void PlaceWall(Direction direction, Vector2Int coord, Transform parent, bool place = true)
    {
        if (!place)
            return;

        var obj = Instantiate(m_WallPrefab, Vector3.zero, Quaternion.identity, parent);
        obj.name = "wall_" + coord;

        var halfBoardWidth = 0.5f;
        var halfWallWidth = 0.025f;
        

        var center = new Vector3(
            coord.x,
            0.75f,                         // Change when we have a height variable
            coord.y);

        Vector3 offset = Vector3.zero;
        switch(direction)
        {
            case Direction.North:
                offset = new Vector3(0.0f, 0.0f, halfBoardWidth - halfWallWidth);
                break;

            case Direction.East:
                offset = new Vector3(-1.0f * (halfWallWidth + halfBoardWidth), 0.0f, 0.0f);
                break;

            case Direction.West:
                offset = new Vector3(halfWallWidth- halfBoardWidth, 0.0f, 0.0f);
                break;

            case Direction.South:
                offset = new Vector3(0.0f, 0.0f, halfWallWidth - halfBoardWidth);
                break;
        }

        if (direction == Direction.North || direction == Direction.South)
            obj.transform.Rotate(0, 90f, 0);
        obj.transform.localPosition = center + offset;
        obj.transform.SetParent(parent);
    }

    #region GUI_HELPERS
    /// <summary>
    /// Start a coroutine
    /// </summary>
    /// <param name="name"></param>
    private void StartCoroutine(string name)
    {
        marijnz.EditorCoroutines.EditorCoroutines.StartCoroutine(name, this);
    }

    /// <summary>
    /// Stop a coroutine
    /// </summary>
    /// <param name="name"></param>
    private void StopCoroutine(string name)
    {
        marijnz.EditorCoroutines.EditorCoroutines.StopCoroutine(name, this);
    }

    /// <summary>
    /// Check if the object is null and print an error message
    /// </summary>
    private bool IsObjectInvalid(string name, GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("Cannot generate Board: " + name + " prefab is null!");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Help to change a float field and serialize it to disk
    /// </summary>
    private void FloatField(string key, string label, ref float number)
    {
        var n = EditorGUILayout.DelayedFloatField(label, number);
        if (n != number)
        {
            PlayerPrefs.SetFloat(key, n);
            number = n;
        }
    }

    /// <summary>
    /// Helper to load a float field
    /// </summary>
    private void LoadFloat(string key, ref float number)
    {
        if (PlayerPrefs.HasKey(key))
        {
            number = PlayerPrefs.GetFloat(key);
        }
    }


    /// <summary>
    /// Help to change a float field and serialize it to disk
    /// </summary>
    private void IntField(string key, string label, ref int number)
    {
        var n = EditorGUILayout.DelayedIntField(label, number);
        if (n != number)
        {
            PlayerPrefs.SetInt(key, n);
            number = n;
        }
    }

    /// <summary>
    /// Helper to load a float field
    /// </summary>
    private void LoadInt(string key, ref int number)
    {
        if (PlayerPrefs.HasKey(key))
        {
            number = PlayerPrefs.GetInt(key);
        }
    }
    
    /// <summary>
    /// Helper to change an object field and serialize it to disk
    /// </summary>
    private void ObjectField(string key, string label, ref GameObject go)
    {
        var prefab = EditorGUILayout.ObjectField(label, go, typeof(GameObject), false) as GameObject;
        if (prefab != go)
        {
            if (prefab == null)
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();

                go = null;
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(prefab);

                PlayerPrefs.SetString(key, path);
                PlayerPrefs.Save();

                go = prefab;
            }
        }
    }

    /// <summary>
    /// Helper to load an object field
    /// </summary>
    private void LoadObject(string key, ref GameObject go)
    {
        if (go == null && PlayerPrefs.HasKey(key))
        {
            var path = PlayerPrefs.GetString(key);
            go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        }
    }
#endregion
}
