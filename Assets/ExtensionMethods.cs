using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ExtensionMethods
{
    public static Vector2Int CoordinatesOf<T>(this T[,] matrix, T value)
    {
        int w = matrix.GetLength(0); // width
        int h = matrix.GetLength(1); // height

        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                if (matrix[x, y].Equals(value))
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1);
    }

    public static void DestroyChildren(this GameObject go)
    {
        foreach (Transform item in go.transform)
        {
            GameObject.Destroy(item.gameObject);
        }
    }

    public static (int minutes, int seconds) GetMinSec(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int remain_seconds = Mathf.FloorToInt(seconds % 60);
        return (minutes, remain_seconds);
    }

    public static IEnumerable<Vector2Int> GetNextCellPos(this Vector2Int pos, Vector2Int size)
    {
        int x = pos.x,
            y = pos.y;

        var xmin = 0;
        var xmax = size.x - 1;
        var ymin = 0;
        var ymax = size.y - 1;

        if (x + 1 <= xmax)
        {
            yield return new Vector2Int(x + 1, y);
        }
        if (x + 1 <= xmax && y + 1 <= ymax)
        {
            yield return new Vector2Int(x + 1, y + 1);
        }
        if (y + 1 <= ymax)
        {
            yield return new Vector2Int(x, y + 1);
        }
        if (x - 1 >= xmin && y + 1 <= ymax)
        {
            yield return new Vector2Int(x - 1, y + 1);
        }
        if (x - 1 >= xmin)
        {
            yield return new Vector2Int(x - 1, y);
        }
        if (x - 1 >= xmin && y - 1 >= ymin)
        {
            yield return new Vector2Int(x - 1, y - 1);
        }
        if (y - 1 >= ymin)
        {
            yield return new Vector2Int(x, y - 1);
        }
        if (x + 1 <= xmax && y - 1 >= ymin)
        {
            yield return new Vector2Int(x + 1, y - 1);
        }
    }

    public static Transform FindDeep(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }

    public static void SetActiveChildren(this GameObject gameObject, bool value)
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetActive(value);
        }
    }

    public static IEnumerable<GameObject> GetChildren(this GameObject gameObject, bool includeInactive = false)
    {
        var allChildren = gameObject.transform.Cast<Transform>().Select(n => n.gameObject);
        return includeInactive ? allChildren : allChildren.Where(n => n.activeSelf == true);
    }

    public static Vector2Int Mod(this Vector2Int a, Vector2Int b)
    {
        int aX = a.x < 0 ? b.x + a.x : a.x % b.x;
        int aY = a.y < 0 ? b.y + a.y : a.y % b.y;
        return new Vector2Int(aX, aY);
    }
}
