using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ViewportSide
{
    All,
    XAxis,
    YAxis,
    Top,
    Bottom,
    Left,
    Right
}

public static class CameraUtility
{
    public static bool IsInsideViewport(Camera camera, Vector3 position, float offset = 0)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(position);

        return viewportPoint.x >= 0 - offset && viewportPoint.x <= 1 + offset &&
           viewportPoint.y >= 0 - offset && viewportPoint.y <= 1 + offset;
    }

    public static Vector3 GetRandomLocationOutsideViewport(Camera camera, float offset = 0, ViewportSide viewportSide = ViewportSide.All)
    {
        // Get screen bounds in world space
        Vector2 screenMin = camera.ViewportToWorldPoint(Vector2.zero);
        Vector2 screenMax = camera.ViewportToWorldPoint(Vector2.one);

        Vector2 randomPoint = Vector2.zero;

        switch (viewportSide)
        {
            case ViewportSide.All:
                var side = Random.Range(0, 4);
                switch (side)
                {
                    case 0: // Left
                        randomPoint = new Vector2(screenMin.x - offset, Random.Range(screenMin.y, screenMax.y));
                        break;
                    case 1: // Right
                        randomPoint = new Vector2(screenMax.x + offset, Random.Range(screenMin.y, screenMax.y));
                        break;
                    case 2: // Bottom
                        randomPoint = new Vector2(Random.Range(screenMin.x, screenMax.x), screenMin.y - offset);
                        break;
                    case 3: // Top
                        randomPoint = new Vector2(Random.Range(screenMin.x, screenMax.x), screenMax.y + offset);
                        break;
                }
                break;
            case ViewportSide.XAxis:
                int xSide = Random.Range(0, 2);
                switch (xSide)
                {
                    case 0: // Left
                        randomPoint = new Vector2(screenMin.x - offset, Random.Range(screenMin.y, screenMax.y));
                        break;
                    case 1: // Right
                        randomPoint = new Vector2(screenMax.x + offset, Random.Range(screenMin.y, screenMax.y));
                        break;
                }
                break;
            case ViewportSide.YAxis:
                int ySide = Random.Range(0, 2);
                switch (ySide)
                {
                    case 0: // Bottom
                        randomPoint = new Vector2(Random.Range(screenMin.x, screenMax.x), screenMin.y - offset);
                        break;
                    case 1: // Top
                        randomPoint = new Vector2(Random.Range(screenMin.x, screenMax.x), screenMax.y + offset);
                        break;
                }
                break;
            case ViewportSide.Top:
                randomPoint = new Vector2(Random.Range(screenMin.x, screenMax.x), screenMax.y + offset);
                break;
            case ViewportSide.Bottom:
                randomPoint = new Vector2(Random.Range(screenMin.x, screenMax.x), screenMin.y - offset);
                break;
            case ViewportSide.Left:
                randomPoint = new Vector2(screenMin.x - offset, Random.Range(screenMin.y, screenMax.y));
                break;
            case ViewportSide.Right:
                randomPoint = new Vector2(screenMax.x + offset, Random.Range(screenMin.y, screenMax.y));
                break;
        }

        
        return randomPoint;
    }

    public static Vector3 GetRandomLocationInsideViewport(Camera camera, float offset = 0)
    {
        // Get screen bounds in world space
        Vector2 screenMin = camera.ViewportToWorldPoint(Vector2.zero);
        Vector2 screenMax = camera.ViewportToWorldPoint(Vector2.one);

        return new Vector2(Random.Range(screenMin.x + offset, screenMax.x - offset), Random.Range(screenMin.y + offset, screenMax.y - offset));
    }
}
