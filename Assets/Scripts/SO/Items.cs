using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items")]
public class Items : ScriptableObject, IEnumerable<Item>
{
    public List<Item> ItemList = new();

    public IEnumerator<Item> GetEnumerator()
    {
        return ItemList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}