using UnityEngine;

public class SelectableGobbos : MonoBehaviour
{
    public SpriteRenderer gobboRenderer;
    private Material material;
    private float defaulted = 0f;
    private float selected = 1f;

    private void Awake()
    {
        if (gobboRenderer == null)
        {
            gobboRenderer = GetComponent<SpriteRenderer>();
            if (gobboRenderer == null)
            {
                Debug.LogError($"SelectableGobbos: SpriteRenderer missing on {gameObject.name}");
                return;
            }
        }
    }

    private void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
        material.SetFloat("_Toggle", defaulted);
    }

    public void OnSelected()
    {
        material.SetFloat("_Toggle", selected);
    }

    public void OnDeselected()
    {
        material.SetFloat("_Toggle", defaulted);
    }
}
