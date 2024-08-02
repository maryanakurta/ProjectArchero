using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    [SerializeField] private float timer = 5;
    private float bulletTime;

    public GameObject enemyBullet;
    private GameManager gameManager;
    public Transform spawnPoint;
    public float bulletSpeed;
    public int damageTakenPerShot = 20;
    public int coinsEarnedOnEnemyKill = 10;

    public NavMeshAgent enemy;
    public Transform target;
    private PlayerMovementManager playerMovementManager;

    public HealthBar healthBar;
    public int maxHealth = 60;
    public int currentHealth;

    private UIManager manager;
    public float movementRange = 10f;
    private bool playerDetected = false;

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
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        // Detect player within the specified range
        if (distanceToPlayer <= movementRange)
        {
            playerDetected = true;
        }

        // If the player is detected, follow and shoot at the player
        if (playerDetected)
        {
            enemy.SetDestination(target.position);

            if (distanceToPlayer <= enemy.stoppingDistance)
            {
                ShootAtPlayer(); // Shoot only if within stopping distance
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
            Destroy(deathParticleSystem.gameObject, 2f); // Destroy the particle system after 2 seconds
        }
        Destroy(gameObject); // Destroy the enemy immediately
    }

    void ShootAtPlayer()
    {
        bulletTime -= Time.deltaTime;

        if (bulletTime > 0 ) return;

        bulletTime = timer;

        GameObject bulletObj = Instantiate(enemyBullet, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;
        Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();
        bulletRig.AddForce(bulletRig.transform.forward * bulletSpeed);
        Destroy(bulletObj, 3f);
    }
}
