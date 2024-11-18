using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace GameplayAbilitySystem
{
    public class CoroutineRunner : MonoBehaviour
    {
        // Singleton pattern (non-static instance)
        private static CoroutineRunner _instance;

        private Dictionary<IEnumerator, Coroutine> _activeCoroutines = new Dictionary<IEnumerator, Coroutine>();

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CoroutineRunner");
                    _instance = obj.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(obj); // Keeps it persistent across scenes
                }
                return _instance;
            }
        }

        // Start a routine and store it in the dictionary
        public Coroutine StartRoutine(IEnumerator routine)
        {
            Coroutine coroutine = StartCoroutine(routine);
            _activeCoroutines[routine] = coroutine; // Store reference to the routine
            //Debug.Log("Starting coroutine: " + routine);
            return coroutine;
        }

        // Start a routine with a delay and store it in the dictionary
        public Coroutine StartRoutineWithDelay(IEnumerator routine, float delay)
        {
            // Create a new delayed routine and store it
            IEnumerator delayedRoutine = DelayedStartRoutine(routine, delay);
            // Store the original routine (not the delayed one) in the dictionary
            Coroutine coroutine = StartRoutine(delayedRoutine);
            _activeCoroutines[routine] = coroutine; // Store the original routine's reference in the dictionary
            return coroutine;
        }

        // Delayed start of a routine
        private IEnumerator DelayedStartRoutine(IEnumerator routine, float delay)
        {
            yield return new WaitForSeconds(delay);
            yield return routine;
        }

        // Stop a specific coroutine using its associated IEnumerator
        public void StopRoutine(IEnumerator routine)
        {
            if (routine == null)
            {
                Debug.LogError("Cannot stop routine because the provided IEnumerator is null.");
                return;
            }

            if (_activeCoroutines.ContainsKey(routine))
            {
                Coroutine coroutine = _activeCoroutines[routine];
                StopCoroutine(routine);
                _activeCoroutines.Remove(routine); // Remove the reference from the dictionary
                //Debug.Log("Stopped coroutine: " + routine);
            }
            else
            {
                Debug.LogWarning("Coroutine not found for the provided IEnumerator: " + routine);
            }
        }

        // Optional: Cleanup method to remove all coroutines
        public void Cleanup()
        {
            foreach (var routine in _activeCoroutines.Keys)
            {
                StopCoroutine(_activeCoroutines[routine]);
            }

            _activeCoroutines.Clear();
            Destroy(gameObject); // Clean up the CoroutineRunner instance
            _instance = null;
        }
    }
}
