using System.Collections.Generic;
using UnityEngine;

public class Cisla : MonoBehaviour
{
    [Header("Prefabs číslic 0–9 (index = číslo)")]
    public GameObject[] digitPrefabs = new GameObject[10];

    [Header("Nastavení layoutu")]
    public float spacing = 0.2f;
    public float scale = 1f;

    private void Start()
    {
        // Testovací zápis čísla
        Write(12345);
    }
    public enum Direction
    {
        Right,
        Left,
        Up,
        Down
    }

    public Direction direction = Direction.Right;

    private readonly List<GameObject> spawnedDigits = new List<GameObject>();

    public void Write(int number)
    {
        Clear();

        string text = Mathf.Abs(number).ToString();

        Vector3 dirVector = GetDirectionVector();

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (!char.IsDigit(c))
                continue;

            int digit = c - '0';

            if (digitPrefabs[digit] == null)
            {
                Debug.LogWarning($"Chybí prefab pro číslo {digit}");
                continue;
            }

            Vector3 positionOffset = dirVector * spacing * i;

            GameObject obj = Instantiate(
                digitPrefabs[digit],
                transform.position + positionOffset,
                transform.rotation,
                transform
            );

            obj.transform.localScale = Vector3.one * scale;

            spawnedDigits.Add(obj);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < spawnedDigits.Count; i++)
        {
            if (spawnedDigits[i] != null)
                Destroy(spawnedDigits[i]);
        }

        spawnedDigits.Clear();
    }

    private Vector3 GetDirectionVector()
    {
        switch (direction)
        {
            case Direction.Right: return transform.right;
            case Direction.Left: return -transform.right;
            case Direction.Up: return transform.up;
            case Direction.Down: return -transform.up;
            default: return transform.right;
        }
    }
}