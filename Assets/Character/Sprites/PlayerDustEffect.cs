using UnityEngine;

namespace Character.Sprites
{
    public class PlayerDustEffect : MonoBehaviour
    {
    public AnimationClip dustAnimationClip; // The animation clip for the dust
    public float dustSpawnRate = 0.2f; // How often to spawn the dust (in seconds)
    public float dustHeight = -0.35f; // Height of the dust spawn (above the ground)

    private float lastSpawnTime = 0f;
    private Transform feetTransform; // The position of the character's feet
    private bool isWalking = false; // Whether the character is walking

    private void Start()
    {
        feetTransform = transform; // Assuming the character's feet are at the character's position, otherwise adjust
    }

    private void Update()
    {
        // Check if the character is walking by speed or animation state
        float speed = GetComponent<Rigidbody2D>().linearVelocity.magnitude; // Assuming the character uses a Rigidbody for movement
        isWalking = speed > 0.1f; // If speed is greater than a threshold, consider walking

        // Spawn dust effect when walking
        if (isWalking && Time.time - lastSpawnTime > dustSpawnRate)
        {
            SpawnDust();
            lastSpawnTime = Time.time; // Reset the spawn timer
        }
    }

    private void SpawnDust()
    {
        // Instantiate a new GameObject with a SpriteRenderer and Animation component
        GameObject dustInstance = new GameObject("DustEffect");
        dustInstance.transform.position = new Vector3(feetTransform.position.x, feetTransform.position.y + dustHeight, feetTransform.position.z);

        // Add the necessary components to the dust GameObject
        SpriteRenderer spriteRenderer = dustInstance.AddComponent<SpriteRenderer>();
        Animation dustAnimation = dustInstance.AddComponent<Animation>();

        // Add the dust animation clip to the Animation component
        dustAnimation.AddClip(dustAnimationClip, "DustAnimation");

        // Play the animation
        dustAnimation.Play("DustAnimation");

        // Optionally, destroy the dust effect after it finishes
        Destroy(dustInstance, dustAnimationClip.length); // The dust effect will be destroyed after the animation finishes
    }
    }
}
