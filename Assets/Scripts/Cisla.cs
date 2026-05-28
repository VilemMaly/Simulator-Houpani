using System.Collections.Generic;
using UnityEngine;

public class Cisla : MonoBehaviour
{
    [Header("Prefabs číslic 0–9 (index = číslo)")]
    public GameObject[] digitPrefabs = new GameObject[10];

    [Header("Prefabs písmen A-Z (index 0 = A, 25 = Z)")]
    public GameObject[] letterPrefabs = new GameObject[26];

    [Header("Nastavení layoutu")]
    public float spacing = 0.2f;

    [Header("Scale")]
    public float digitScale = 1f;
    public float letterScale = 1f;

    [Header("Volitelné kopírování do jiného Cisla scriptu")]
    public Cisla mirrorTarget;

    public enum Direction
    {
        Right,
        Left,
        Up,
        Down
    }

    public Direction direction = Direction.Right;

    private readonly List<GameObject> spawnedCharacters = new List<GameObject>();

    // uložené poslední hodnoty
    private int currentNumber = 0;
    private string currentText = "";

    private void Start()
    {
        Write(0);
    }

    // WRITE INT
    public void Write(int number)
    {
        currentNumber = number;
        currentText = Mathf.Abs(number).ToString();

        SpawnText(currentText);

        // mirror
        if (mirrorTarget != null && mirrorTarget != this)
        {
            mirrorTarget.WriteFromMirror(number);
        }
    }

    // WRITE STRING
    public void Write(string text)
    {
        if (string.IsNullOrEmpty(text))
            text = "";

        text = text.ToUpper();

        currentText = text;

        SpawnText(text);

        // mirror
        if (mirrorTarget != null && mirrorTarget != this)
        {
            mirrorTarget.WriteFromMirror(text);
        }
    }

    // interní mirror pro int
    private void WriteFromMirror(int number)
    {
        currentNumber = number;
        currentText = Mathf.Abs(number).ToString();

        SpawnText(currentText);
    }

    // interní mirror pro string
    private void WriteFromMirror(string text)
    {
        if (string.IsNullOrEmpty(text))
            text = "";

        text = text.ToUpper();

        currentText = text;

        SpawnText(text);
    }

    // společné spawnování
    private void SpawnText(string text)
    {
        Clear();

        Vector3 dirVector = GetDirectionVector();

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            GameObject prefabToSpawn = null;
            float usedScale = digitScale;

            // čísla
            if (char.IsDigit(c))
            {
                int digit = c - '0';

                if (digit >= 0 && digit < digitPrefabs.Length)
                {
                    prefabToSpawn = digitPrefabs[digit];
                    usedScale = digitScale;
                }
            }
            // písmena
            else if (char.IsLetter(c))
            {
                char upper = char.ToUpper(c);

                int letterIndex = upper - 'A';

                if (letterIndex >= 0 && letterIndex < letterPrefabs.Length)
                {
                    prefabToSpawn = letterPrefabs[letterIndex];
                    usedScale = letterScale;
                }
            }

            // pokud není prefab
            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"Chybí prefab pro znak: {c}");
                continue;
            }

            Vector3 positionOffset = dirVector * spacing * i;

            GameObject obj = Instantiate(
                prefabToSpawn,
                transform.position + positionOffset,
                transform.rotation,
                transform
            );

            obj.transform.localScale = Vector3.one * usedScale;

            spawnedCharacters.Add(obj);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < spawnedCharacters.Count; i++)
        {
            if (spawnedCharacters[i] != null)
                Destroy(spawnedCharacters[i]);
        }

        spawnedCharacters.Clear();
    }

    private Vector3 GetDirectionVector()
    {
        switch (direction)
        {
            case Direction.Right:
                return transform.right;

            case Direction.Left:
                return -transform.right;

            case Direction.Up:
                return transform.up;

            case Direction.Down:
                return -transform.up;

            default:
                return transform.right;
        }
    }
}