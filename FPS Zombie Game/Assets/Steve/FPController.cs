using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FPController : MonoBehaviour
{
    public GameObject cam;
    public GameObject stevePrefab;
    public Slider healthbar;
    public Text ammoReserves;
    public Text ammoClipAmount;
    public Transform shotDirection;
    public Animator anim;
    public AudioSource[] footsteps;
    public AudioSource jump;
    public AudioSource land;
    public AudioSource ammoPickup;
    public AudioSource healthPickup;
    public AudioSource triggerSound;
    public AudioSource deathSound;
    public AudioSource reloadSound;
    public GameObject bloodPrefab;

    public GameObject uiBloodPrefab;
    public GameObject canvas;
    public GameObject gameOverPrefab;

    public CompassController compassC;
    public GameObject[] checkPoints;
    int currentCheckPoint = 0;
    public LayerMask checkPointLayer;
    
    public int lives = 3;
    private int timesDied = 0;
    private Vector3 startPosition;
    
    float cWidth;
    float cHeight;

    float speed = 0.1f;
    float Xsensitivity = 2;
    float Ysensitivity = 2;
    float MinimumX = -90;
    float MaximumX = 90;
    Rigidbody rb;
    CapsuleCollider capsule;
    Quaternion cameraRot;
    Quaternion characterRot;

    bool cursorIsLocked = true;
    bool lockCursor = true;

    float x;
    float z;

    //Inventory
    int ammo = 50;
    int maxAmmo = 50;
    int health = 100;
    int maxHealth = 100;
    int ammoClip = 10;
    int ammoClipMax = 10;

    bool playingWalking = false;
    bool previouslyGrounded = true;

    private GameObject steve;

    public void TakeHit(float amount)
    {
        if (GameStats.gameOver) return;
        
        health = (int) Mathf.Clamp(health - amount, 0, maxHealth);
        healthbar.value = health;

        GameObject bloodSplatter = Instantiate(uiBloodPrefab);
        bloodSplatter.transform.SetParent(canvas.transform);
        bloodSplatter.transform.position = new Vector3(Random.Range(0, cWidth), Random.Range(0, cHeight), 0);

        float bloodScale = Random.Range(0.3f, 1.5f);
        bloodSplatter.transform.localScale = new Vector3(bloodScale, bloodScale, 1);

        Destroy(bloodSplatter, 2.2f);

        if (health <= 0)
        {
            Vector3 pos = new Vector3(transform.position.x,
                                        Terrain.activeTerrain.SampleHeight(transform.position),
                                        transform.position.z);
            steve = Instantiate(stevePrefab, pos, transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Death");
            GameStats.gameOver = true;
            steve.GetComponent<AudioSource>().enabled = false;
            timesDied++;

            if (timesDied == lives)
            {
                Destroy(gameObject);
            }
            else
            {
                steve.GetComponent<GoToMainMenu>().enabled = false;
                cam.SetActive(false);
                Invoke("Respawn", 5);
            }

        }
    }

    void Respawn()
    {
        Destroy(steve);
        cam.SetActive(true);
        GameStats.gameOver = false;
        health = maxHealth;
        healthbar.value = health;
        transform.position = startPosition;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Home")
        {
            Vector3 pos = new Vector3(this.transform.position.x,
                                        Terrain.activeTerrain.SampleHeight(this.transform.position),
                                        this.transform.position.z);
            GameObject steve = Instantiate(stevePrefab, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Dance");
            GameStats.gameOver = true;
            Destroy(this.gameObject);
            GameObject gameOverText = Instantiate(gameOverPrefab);
            gameOverText.transform.SetParent(canvas.transform);
            gameOverText.transform.localPosition = Vector3.zero;
        }

        if (col.gameObject.CompareTag("SpawnPoint"))
        {
            startPosition = transform.position;
            if (col.gameObject == checkPoints[currentCheckPoint])
            {
                currentCheckPoint++;
                compassC.target = checkPoints[currentCheckPoint];
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        capsule = this.GetComponent<CapsuleCollider>();
        cameraRot = cam.transform.localRotation;
        characterRot = this.transform.localRotation;

        health = maxHealth;
        healthbar.value = health;

        ammoReserves.text = ammo + "";
        ammoClipAmount.text = ammoClip + "";

        cWidth = canvas.GetComponent<RectTransform>().rect.width;
        cHeight = canvas.GetComponent<RectTransform>().rect.height;

        startPosition = transform.position;
        compassC.target = checkPoints[0];
    }

    void ProcessZombieHit()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(shotDirection.position, shotDirection.forward, out hitInfo, 200, ~checkPointLayer))
        {
            GameObject hitZombie = hitInfo.collider.gameObject;
            if (hitZombie.tag == "Zombie")
            {
                GameObject blood = Instantiate(bloodPrefab, hitInfo.point, Quaternion.identity);
                blood.transform.LookAt(this.transform.position);
                Destroy(blood, 0.5f);

                hitZombie.GetComponent<ZombieController>().shotsTaken++;
                if (hitZombie.GetComponent<ZombieController>().shotsTaken ==
                    hitZombie.GetComponent<ZombieController>().shotsRequired)
                {
                    if (Random.Range(0, 10) < 5)
                    {
                        GameObject rdPrefab = hitZombie.GetComponent<ZombieController>().ragdoll;
                        GameObject newRD = Instantiate(rdPrefab, hitZombie.transform.position, hitZombie.transform.rotation);
                        newRD.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(shotDirection.forward * 10000);
                        Destroy(hitZombie);
                    }
                    else
                    {
                        hitZombie.GetComponent<ZombieController>().KillZombie();
                    }   
                }
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        UpdateCursorLock();
        
        Debug.DrawRay(shotDirection.transform.position, shotDirection.forward * 200, Color.red);
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool("arm", !anim.GetBool("arm"));

        if (Input.GetMouseButtonDown(0) && !anim.GetBool("fire") && anim.GetBool("arm") && GameStats.canShoot)
        {
            if (ammoClip > 0)
            {
                anim.SetTrigger("fire");
                ProcessZombieHit();
                ammoClip--;
                ammoClipAmount.text = ammoClip + "";
                GameStats.canShoot = false;
            }
            else 
                triggerSound.Play();


            //Debug.Log("Ammo Left in Clip: " + ammoClip);
        }

        if (Input.GetKeyDown(KeyCode.R) && anim.GetBool("arm"))
        {
            anim.SetTrigger("reload");
            reloadSound.Play();
            int amountNeeded = ammoClipMax - ammoClip;
            int ammoAvailable = amountNeeded < ammo ? amountNeeded : ammo;
            ammo -= ammoAvailable;
            ammoClip += ammoAvailable;
            ammoReserves.text = ammo + "";
            ammoClipAmount.text = ammoClip + "";
            //Debug.Log("Ammo Left: " + ammo);
            //Debug.Log("Ammo in Clip: " + ammoClip);
        }

        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!anim.GetBool("walking"))
            {
                anim.SetBool("walking", true);
                InvokeRepeating("PlayFootStepAudio", 0, 0.4f);
            }
        }
        else if (anim.GetBool("walking"))
        {
            anim.SetBool("walking", false);
            CancelInvoke("PlayFootStepAudio");
            playingWalking = false;
        }

        bool grounded = IsGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(0, 300, 0);
            jump.Play();
            if (anim.GetBool("walking"))
            {
                CancelInvoke("PlayFootStepAudio");
                playingWalking = false;
            }
        }
        else if (!previouslyGrounded && grounded)
        {
            land.Play();
        }

        previouslyGrounded = grounded;

    }

    void PlayFootStepAudio()
    {
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, footsteps.Length);

        audioSource = footsteps[n];
        audioSource.Play();
        footsteps[n] = footsteps[0];
        footsteps[0] = audioSource;
        playingWalking = true;
    }


    void FixedUpdate()
    {
        float yRot = Input.GetAxis("Mouse X") * Ysensitivity;
        float xRot = Input.GetAxis("Mouse Y") * Xsensitivity;

        cameraRot *= Quaternion.Euler(-xRot, 0, 0);
        characterRot *= Quaternion.Euler(0, yRot, 0);

        cameraRot = ClampRotationAroundXAxis(cameraRot);

        this.transform.localRotation = characterRot;
        cam.transform.localRotation = cameraRot;

        x = Input.GetAxis("Horizontal") * speed;
        z = Input.GetAxis("Vertical") * speed;

        transform.position += cam.transform.forward * z + cam.transform.right * x; //new Vector3(x * speed, 0, z * speed);

        UpdateCursorLock();
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    bool IsGrounded()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo,
                (capsule.height / 2f) - capsule.radius + 0.1f))
        {
            return true;
        }
        return false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Ammo" && ammo < maxAmmo)
        {
            ammo = Mathf.Clamp(ammo + 10, 0, maxAmmo);
            ammoReserves.text = ammo + "";
            //Debug.Log("Ammo: " + ammo);
            Destroy(col.gameObject);
            ammoPickup.Play();

        }
        else if (col.gameObject.tag == "MedKit" && health < maxHealth)
        {
            health = Mathf.Clamp(health + 25, 0, maxHealth);
            healthbar.value = health;
            //Debug.Log("MedKit: " + health);
            Destroy(col.gameObject);
            healthPickup.Play();
        }
        else if (col.gameObject.tag == "Lava")
        {
            health = Mathf.Clamp(health - 50, 0, maxHealth);
            healthbar.value = health;
            // Debug.Log("Health Level: " + health);
            if (health <= 0)
                deathSound.Play();
        }

        else if (IsGrounded())
        {
            if (anim.GetBool("walking") && !playingWalking)
            {
                InvokeRepeating("PlayFootStepAudio", 0, 0.4f);
            }
        }
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)
            InternalLockUpdate();
    }

    public void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            cursorIsLocked = false;
        else if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            cursorIsLocked = true;

        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

}
