using MatchFactoryCore.Scripts.Item;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Input
{
    public class MatchFactoryInput : MonoBehaviour
    {
        public void HandleClick(Ray ray, out GameObject go)
        {
            go = null;
            var raycast = Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("ItemFactory"));
            if (!raycast) return;
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.transform.parent.GetComponent<ItemFactory>().Id);
                go = hit.collider.transform.parent.gameObject;
            }
        }
    }
}