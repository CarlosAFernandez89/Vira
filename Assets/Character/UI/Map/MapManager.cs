using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Character.UI.Map
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance;

        [SerializeField] private InputActionReference mapInputActionReference;
        [SerializeField] private GameObject miniMap;
        [SerializeField] private GameObject largeMap;
        

        public bool IsLargeMapOpen { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            CloseLargeMap();
        }

        private void OnEnable()
        {
            if (mapInputActionReference != null)
            {
                mapInputActionReference.ToInputAction().performed += OnMapToggled;
            }
        }

        private void OnDisable()
        {
            if (mapInputActionReference != null)
            {
                mapInputActionReference.ToInputAction().performed -= OnMapToggled;
            }
        }

        private void OnMapToggled(InputAction.CallbackContext ctx)
        {
            IsLargeMapOpen = !IsLargeMapOpen;

            switch (IsLargeMapOpen)
            {
                case true: OpenLargeMap();
                    break;
                case false: CloseLargeMap();
                    break;
            }
        }

        private void OpenLargeMap()
        {
            miniMap.SetActive(false);
            largeMap.SetActive(true);
        }

        private void CloseLargeMap()
        {
            largeMap.SetActive(false);
            miniMap.SetActive(true);
        }
    }
}
