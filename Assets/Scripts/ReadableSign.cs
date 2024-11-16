using System;
using Character;
using UnityEngine;

public class ReadableSign : MonoBehaviour, IInteract
{
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    [SerializeField] public Color desiredColor;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    public void Interact()
    {
        Debug.Log("ReadableSign Interact");
        _spriteRenderer.color = desiredColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
           other.gameObject.GetComponent<PlayerActions>().SetInteractTarget(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerActions>().ClearInteractTarget();
            _spriteRenderer.color = _originalColor;
        }
    }
}
