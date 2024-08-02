using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerMovementManager : MonoBehaviour
{
    public VariableJoystick joystick;
    public CharacterController controller;
    public HealthBar healthBar;
    public int maxHealth = 60;
    public int damageTakenPerShot = 20;
    public int currentHealth;
    
    public float moveSpeed = 5f;
    public float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    public Animator playerAnimator;

    public Canvas inputCanvas;
    
    public bool isJoystick;

    public GameObject playerBullet;
    public Transform bulletSpawnPoint;
    [SerializeField] private float shootTimer = 5;
    private bool isMoving = false;

    public float shootInterval = 0.5f;  // Adjust the shooting interval as needed

    public Canvas resultCanvas;  // Reference to the result canvas
    public TextMeshProUGUI resultText;  // Reference to the result text
    public TextMeshProUGUI coinCountText;  // Reference to the coin count text
    public TextMeshProUGUI enemiesKilledText;  // Reference to the enemies killed text

    public GameObject muzzleFlashPrefab;

    private void Start()
    {
        resultCanvas.gameObject.SetActive(false);
        EnableJoystickInput();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    public void EnableJoystickInput()
    {
        isJoystick = true;
        inputCanvas.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isMoving)
        {
            Transform nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null)
            {
                FaceTarget(nearestEnemy);
                ShootAtEnemy(nearestEnemy);
            }
        }

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(100);
            GameOver();
        }
        
        if (collision.gameObject.CompareTag("Enemy Bullet"))
        {
            TakeDamage(damageTakenPerShot);
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Lava"))
        {
            TakeDamage(maxHealth); // Instantly reduce health to 0
            GameOver(); // Trigger game over
        }
    }

    public void GameOver()
    {
        // Trigger the die animation
        playerAnimator.SetBool("isGameOver", true);

        // Start the coroutine to handle waiting for the animation to finish
        StartCoroutine(HandleGameOver());
    }

    private IEnumerator HandleGameOver()
    {
        // Get the length of the die animation clip
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        // Wait for the duration of the die animation
        yield return new WaitForSeconds(2);

        // After the animation has played, show the game over UI
        resultCanvas.gameObject.SetActive(true);
        resultText.text = "Game Over!";
        coinCountText.text = "Coins: " + PlayerPrefs.GetInt("levelCoinCount").ToString();
        enemiesKilledText.text = "Enemies Killed: " + (PlayerPrefs.GetInt("levelEnemyCount")).ToString();

        // Destroy the player game object
        Destroy(gameObject);
    }

    public void GameWin()
    {
        Destroy(gameObject);
        resultCanvas.gameObject.SetActive(true);

        // Display the "You Win" message
        resultText.text = "Congrats! You Win";

        // Set the coinText and enemyText to the current PlayerPrefs values
        coinCountText.text = "Coins: " + PlayerPrefs.GetInt("levelCoinCount").ToString();
        enemiesKilledText.text = "Enemies Killed: " + (PlayerPrefs.GetInt("levelEnemyCount") - 1).ToString();
    }

    public void DestroyPlayer()
    {
        gameObject.SetActive(false);
        Debug.Log("Hit enemy");
    }

    private void FixedUpdate()
    {
        if (isJoystick)
        {
            var movementDirection = new Vector3(joystick.Direction.x, 0.0f, joystick.Direction.y).normalized;

            if (movementDirection.magnitude >= 0.1f)
            {
                isMoving = true;
                playerAnimator.SetBool("isMoving", true);
                float targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

                controller.Move(movementDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                isMoving = false;
                playerAnimator.SetBool("isMoving", false);
            }
        }
    }

    void TakeDamage(int damage)
    {
        if (currentHealth >= 0)
        {
            currentHealth -= damage;
            healthBar.SetHealth(currentHealth);
        }
        else return;
    }

    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearestEnemy = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(currentPosition, enemy.transform.position);
            if (distanceToEnemy < minDistance)
            {
                minDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }
        return nearestEnemy;
    }

    void FaceTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void ShootAtEnemy(Transform target)
    {
        shootTimer -= Time.deltaTime;

        playerAnimator.SetTrigger("Shoot");

        if (shootTimer > 0) return;

        shootTimer = shootInterval;

        GameObject bullet = Instantiate(playerBullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        Vector3 direction = (target.position - bulletSpawnPoint.position).normalized;
        bulletRb.AddForce(direction * 1000);
        Destroy(bullet, 2f);

        GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Destroy the muzzle flash after 0.1 seconds
        Destroy(muzzleFlash, 0.1f);
    }
}
