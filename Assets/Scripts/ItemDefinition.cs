using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Item/Definition")]
public class ItemDefinition : ScriptableObject
{
    public string itemID;
    public string displayName;
    public Sprite sprite;
    public bool collectable;
}