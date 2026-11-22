using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Item : MonoBehaviour
{
    [SerializeField] private ItemDefinition definition;

    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (definition != null)
        {
            spriteRenderer.sprite = definition.sprite;
            polygonCollider = transform.AddComponent<PolygonCollider2D>();
            if (definition.collectable) tag = "Collectable";
        }
        else Debug.Log("Error: No item definition selected");
    }
}