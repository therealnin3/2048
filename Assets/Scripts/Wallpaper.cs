using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wallpaper : MonoBehaviour
{
    [Header("Wallpaper")]
    [SerializeField] private float _x = 0.2f, _y = 0.2f;
    [SerializeField] private float numberOfSquares = 15f;
    [SerializeField] private Color wallpaper_colour;

    [SerializeField] private RawImage rawImage;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform parentRectTransform;

    private void Awake()
    {
        // Get max of canvas size
        float wallpaperSize = Mathf.Max(parentRectTransform.rect.width, parentRectTransform.rect.height);
        rectTransform.sizeDelta = new Vector2(wallpaperSize, wallpaperSize);

        // Set number of squares
        Rect uvRect = GetComponent<RawImage>().uvRect;
        uvRect.width = numberOfSquares;
        uvRect.height = numberOfSquares;

        // Change wallpaper colour
        rawImage.color = wallpaper_colour;

        // Set uvRect
        GetComponent<RawImage>().uvRect = uvRect;
    }

    private void Update()
    {
        rawImage.uvRect = new Rect(rawImage.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, rawImage.uvRect.size);

        if (parentRectTransform.rect.width != rectTransform.rect.width || parentRectTransform.rect.height != rectTransform.rect.height)
        {
            float wallpaperSize = Mathf.Max(parentRectTransform.rect.width, parentRectTransform.rect.height);
            rectTransform.sizeDelta = new Vector2(wallpaperSize, wallpaperSize);
        }
    }
}
