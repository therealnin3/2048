using System;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public int number;
    public TextMeshPro textMeshPro;
    public Cell cell;
    public bool isMerged = false;
}