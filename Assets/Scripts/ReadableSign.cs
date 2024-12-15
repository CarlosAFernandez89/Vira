using System;
using Character;
using Character.Abilities.Charms;
using GameplayAbilitySystem;
using Interface;
using UnityEngine;
using UnityEngine.Serialization;

public class ReadableSign : MonoBehaviour, IInteract
{
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    [SerializeField] public Color successColor = Color.green;
    [SerializeField] public Color failedColor = Color.red;

    [SerializeField] public CharmAbilityBase charmAbility;
    
    private CharmManager _charmManager;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    public void Interact()
    {
        Debug.Log("ReadableSign Interact");

        if (_charmManager != null)
        {
            if (_charmManager.GrantCharmAbility(charmAbility))
            {
                _spriteRenderer.color = successColor;
                return;
            }
            _spriteRenderer.color = failedColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
           other.gameObject.GetComponent<PlayerActions>().SetInteractTarget(this);
           _charmManager = other.gameObject.GetComponent<CharmManager>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerActions>().ClearInteractTarget();
            _spriteRenderer.color = _originalColor;
            _charmManager = null;
        }
    }
}
