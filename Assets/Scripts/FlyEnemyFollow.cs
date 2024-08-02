using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyEnemyFollow : MonoBehaviour
{
    [SerializeField] private float bulletTimer = 5;
    private float bulletTime;

    public GameObject enemyBullet;
    private GameManager gameManager;
    public Transform spawnPoint;
    public float bulletSpeed;
    public int damageTakenPerShot = 20;

    public Transform target; // Assign the player as the target in the Inspector
    private PlayerMovementManager playerMovementManager;

    public HealthBar healthBar;
    public int maxHealth = 60;
    public int currentHealth;

    private UIManager manager;
    public float followSpeed = 5f; // Speed at which the enemy follows the player
    public float stoppingDistance = 2f; // Distance at which the enemy stops moving towards the player
    public float movementRange = 10f;

    private bool isMoving = false;
    private bool playerDetected = false;
    public int coinsEarnedOnEnemyKill = 10;

    // Particle system for death animation
    public ParticleSystem deathParticleSystem;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        manager = FindObjectOfType<UIManager>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        // If you haven't already set it in the Inspector, you can find the player target in code
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform; // Assuming the player has the tag "Player"
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (distanceToPlayer <= movementRange)
        {
            playerDetected = true; // Detect the player
        }

        // Check if the player is within the movement range
        if (playerDetected)
        {
            MoveTowardsPlayer(); // Move the enemy towards the player

            if (!isMoving)
            {
                ShootAtPlayer(); // Shoot only if the enemy is not moving
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        if (target != null)
        {
            // Calculate the direction to the player, keeping the enemy's y position unchanged
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0; // Keep y direction at 0 to maintain the same height

            // Calculate the distance to the player
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);

            // Move towards the player if the enemy is not within the stopping distance
            if (distanceToPlayer > stoppingDistance)
            {
                isMoving = true;

                // Move the enemy towards the player, keeping its height constant
                transform.position += direction * followSpeed * Time.deltaTime;

                // Rotate the enemy to face the player on the x-z plane
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
            }
            else
            {
                isMoving = false;

                // Ensure the enemy is facing the player directly when within stopping distance
                Vector3 lookDirection = (target.position - transform.position).normalized;
                lookDirection.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * followSpeed);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet")) // Assuming "Bullet" is the tag for player bullets
        {
            TakeDamage(damageTakenPerShot);
            Destroy(collision.gameObject);

            manager.IncreaseCoins(coinsEarnedOnEnemyKill);

            int currentLevelCoinCount = PlayerPrefs.GetInt("levelCoinCount");
            PlayerPrefs.SetInt("levelCoinCount", currentLevelCoinCount + coinsEarnedOnEnemyKill);
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            int currentLevelEnemyCount = PlayerPrefs.GetInt("levelEnemyCount");
            PlayerPrefs.SetInt("levelEnemyCount", currentLevelEnemyCount + 1);

            if (gameManager.enemyCount > 0)
            {
                gameManager.DecreaseEnemyCount();
            }

            TriggerDeathParticleSystem(); // Trigger death particle system
        }
    }

    private void TriggerDeathParticleSystem()
    {
        if (deathParticleSystem != null)
        {
            deathParticleSystem.transform.parent = null; // Detach the particle system from the enemy
            deathParticleSystem.Play(); // Play the particle system
            Destroy(deathParticleSystem.gameObject, 3f); // Destroy the particle system after 3 seconds
        }
        Destroy(gameObject); // Destroy the enemy immediately
    }

    void ShootAtPlayer()
    {
        bulletTime -= Time.deltaTime;

        if (bulletTime > 0) return;

        bulletTime = bulletTimer;

        // Create the bullet at the spawn point
        GameObject bulletObj = Instantiate(enemyBullet, spawnPoint.transform.position, spawnPoint.transform.rotation);
        Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();

        // Calculate the direction to shoot the bullet towards the player
        Vector3 shootDirection = (target.position - spawnPoint.position).normalized;

        // Apply force to the bullet to shoot it in the calculated direction
        bulletRig.AddForce(shootDirection * bulletSpeed, ForceMode.VelocityChange);

        // Destroy the bullet after 5 seconds to clean up
        Destroy(bulletObj, 3f);
    }
}
