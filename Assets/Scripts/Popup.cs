using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [Header("Prefaby znaků")]
    public GameObject plusPrefab;
    public GameObject[] digitPrefabs = new GameObject[10];

    [Header("Materiál")]
    public Material overrideMaterial;

    [Header("Layout")]
    public float spacing = 0.2f;
    public float scale = 1f;

    public enum Direction
    {
        Right,
        Left,
        Up,
        Down
    }

    public Direction direction = Direction.Right;

    [Header("Animace")]
    public float moveSpeed = 1f;
    public float lifetime = 1.5f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private Vector3 startPosition;
    private float timer;

    public void Show(int score)
    {
        Clear();

        startPosition = transform.position;
        timer = 0f;

        string text = "+" + Mathf.Abs(score).ToString();

        Vector3 dirVector = GetDirectionVector();

        int visualIndex = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            GameObject prefabToSpawn = null;

            if (c == '+')
            {
                prefabToSpawn = plusPrefab;
            }
            else if (char.IsDigit(c))
            {
                int digit = c - '0';

                if (digit >= 0 && digit <= 9)
                {
                    prefabToSpawn = digitPrefabs[digit];
                }
            }

            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"Chybí prefab pro znak {c}");
                continue;
            }

            Vector3 positionOffset = dirVector * spacing * visualIndex;

            GameObject obj = Instantiate(
                prefabToSpawn,
                transform.position + positionOffset,
                transform.rotation,
                transform
            );

            obj.transform.localScale = Vector3.one * scale;

            // Nastavení materiálu
            if (overrideMaterial != null)
            {
                Renderer[] renderers =
                    obj.GetComponentsInChildren<Renderer>();

                foreach (Renderer r in renderers)
                {
                    r.material = overrideMaterial;
                }
            }

            spawnedObjects.Add(obj);

            visualIndex++;
        }
    }

    private void Update()
    {
        if (spawnedObjects.Count <= 0)
            return;

        timer += Time.deltaTime;

        float moveAmount = moveSpeed * Time.deltaTime;

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
            {
                spawnedObjects[i].transform.position +=
                    transform.up * moveAmount;
            }
        }

        if (timer >= lifetime)
        {
            Clear();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
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