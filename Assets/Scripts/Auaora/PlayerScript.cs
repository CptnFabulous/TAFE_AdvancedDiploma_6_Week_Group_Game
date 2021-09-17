using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Auaora
{
    public class PlayerScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rigRef;
        [SerializeField] private Animator playerAnim;
        [SerializeField] private PlayerCameraScript cameraRef;
        [SerializeField] private GameObject dashIndicator;

        [Header("Movement Variables")]
        private Vector2 speed;
        [SerializeField] private float maxSpeed = 4f;
        private bool mouseAim = false;

        private Vector2 knockbackVector = Vector2.zero;

        [Header("Dash Variables")]
        [SerializeField] private float dashSpeed = 8;
        private float internalDashCooldown;
        [SerializeField] private float dashCooldown = 0.3f;
        private Vector2 dashVector = Vector2.zero;

        [Header("Misc Variables")]
        private PlayerState currentState = PlayerState.Regular;
        public PlayerState CurrentState { get { return currentState; } }
        [SerializeField] private int maxHealth;
        private int currentHealth;

        private float intangible = 0f;
        private bool dead;

        // Sets some things automatically
        void Start()
        {
            currentHealth = maxHealth;
            rigRef = GetComponent<Rigidbody>();

            if (!cameraRef && FindObjectOfType<PlayerCameraScript>())
            {
                cameraRef = FindObjectOfType<PlayerCameraScript>();
            }

            if (PlayerPrefs.GetInt("mouseAim", 1) == 0)
            {
                mouseAim = false;
            }
            else
            {
                mouseAim = true;
            }
        }

        void Update()
        {
            //Checks if dash should end, else continue dash
            if (internalDashCooldown > 0 && currentState == PlayerState.Dashing)
            {
                rigRef.velocity = new Vector3(dashVector.x * dashSpeed, 0f, dashVector.y * dashSpeed);
                intangible = 0.2f;
                internalDashCooldown -= 1 * Time.deltaTime;
                if (internalDashCooldown <= 0)
                {
                    EndDash();
                }
            }

            //Checks for dash input
            if (Input.GetButtonDown("Dash"))
            {
                if (IfPlayerNotState(false, true, true, true))
                {
                    currentState = PlayerState.Dashing;

                    BeginDash();
                }
            }

            // Control speed
            if (IfPlayerNotState(false, true, true, true) && Time.timeScale != 0)
            {
                speed.x = Input.GetAxisRaw("Horizontal") * 10f;
                speed.y = Input.GetAxisRaw("Vertical") * 10f;
                speed.Normalize();
                speed *= maxSpeed;
            }

            // Apply movement
            if (!dead && Time.timeScale != 0)
            {
                if (currentState == PlayerState.Regular)
                {
                    rigRef.velocity = new Vector3(speed.x, 0f, speed.y);
                }
                else if (currentState == PlayerState.Hitstun)
                {
                    if (knockbackVector == Vector2.zero)
                    {
                        EndHitstun();
                    }
                    else
                    {
                        //apply hitstun and reduce
                    }
                }
            }

            if (intangible > 0)
            {
                intangible -= Time.deltaTime;
                if (intangible <= 0)
                {
                    gameObject.layer = 13;
                }
            }

            if (Time.timeScale != 0)
            {
                dashIndicator.transform.LookAt(new Vector3 (transform.position.x + AimInputVector().x, transform.position.y, transform.position.z + AimInputVector().y));

                /*
                Vector3 target = (Vector3)AimInputVector();
                float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
                Quaternion quat = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
                dashIndicator.transform.rotation = Quaternion.Slerp(dashIndicator.transform.rotation, quat, 0.5f);
                */

                HandleAnimation();
            }
        }

        private void EndHitstun()
        {
            currentState = PlayerState.Regular;
        }

        //Ends dashing
        private void EndDash()
        {
            internalDashCooldown = 0;
            currentState = PlayerState.Regular;
            gameObject.layer = 13;
        }

        private void BeginDash()
        {
            Vector2 direction = new Vector2(Mathf.Clamp(Input.GetAxisRaw("Horizontal") * 1000, -1, 1), Mathf.Clamp(Input.GetAxisRaw("Vertical") * 1000, -1, 1)).normalized;
            rigRef.velocity = new Vector3(direction.x * dashSpeed, 0f, direction.y * dashSpeed);
            dashVector = direction;
            internalDashCooldown = dashCooldown;

            currentState = PlayerState.Dashing;
            intangible = 0.8f;
            gameObject.layer = 16;
        }

        public void TakeDamage(float damage, Vector3 damagePos, bool knockback = true)
        {
            if (!dead)
            {
                if (damage <= 0)
                {
                    return;
                }
                currentHealth -= Mathf.FloorToInt(damage);
                //FindObjectOfType<HUDScript>().ShowDamageNumber(damage, transform.position, true);
                //sceneMuleRef.UpdateUi();
                if (knockback)
                {
                    knockbackVector = (transform.position - damagePos) * 4f;
                }
            }
        }

        private bool IsInputAllowed()
        {
            if (Time.timeScale == 0)
            {
                return false;
            }
            if (dead)
            {
                return false;
            }
            else if (currentState != PlayerState.Hitstun)
            {
                return false;
            }
            else if (currentState == PlayerState.Dashing)
            {
                return false;
            }
            else if (currentState == PlayerState.Attacking)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //Sends animation controller relevant information
        private void HandleAnimation()
        {
            if (currentState == PlayerState.Regular && Input.GetAxisRaw("Horizontal") != 0)
            {
                playerAnim.SetBool("Walk", true);
            }
            else
            {
                playerAnim.SetBool("Walk", false);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {

        }

        public bool IsIntangible()
        {
            return intangible > 0 ? true : false;
        }

        public void HealPlayer(float health)
        {
            health = Mathf.Floor(Mathf.Clamp(health, 1f, 999f));
            if (currentHealth < maxHealth && currentHealth > 0 && !dead)
            {
                currentHealth += Mathf.FloorToInt(health);
            }
            //sceneMuleRef.UpdateUi();
        }

        public Vector2 AimInputVector()
        {
            Vector2 direction = Vector2.zero;
            if (mouseAim)
            {
                Vector3 smolDir = (cameraRef.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f)) - cameraRef.transform.position).normalized;
                smolDir *= 10f;
                direction = cameraRef.transform.position + smolDir - gameObject.transform.position;
                print(cameraRef.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f)) + ": pos");
            }
            else
            {
                direction = new Vector2(Input.GetAxisRaw("AimHor"), Input.GetAxisRaw("AimVer"));
            }
            direction.Normalize();
            return direction;
        }

        public void CancelMeleeAttack()
        {
            playerAnim.Play("TestIdle");
            HandleAnimation();
        }

        public void PlayAttackAnim(string paramName, float speed = 1f)
        {
            playerAnim.SetFloat("AttackSpeed", speed);
            playerAnim.SetTrigger(paramName);
        }

        public void SetPlayerAsAttacking()
        {
            currentState = PlayerState.Attacking;
        }

        public void EndPlayerAsAttacking()
        {
            currentState = PlayerState.Regular;
        }

        private bool IfPlayerNotState(bool regularState, bool dashingState, bool attackingState, bool hitstunState)
        {
            if (regularState && currentState == PlayerState.Regular)
            {
                return false;
            }
            if (dashingState && currentState == PlayerState.Dashing)
            {
                return false;
            }
            if (attackingState && currentState == PlayerState.Attacking)
            {
                return false;
            }
            if (hitstunState && currentState == PlayerState.Hitstun)
            {
                return false;
            }
            return true;
        }

        public void PrintState()
        {
            print("State: " + currentState.ToString());
        }

        /*
        private void AttemptInteract()
        {
            Collider2D[] collArray = Physics2D.OverlapCircleAll(transform.position, 0.75f, 1);

            for (int i = 0; i < collArray.Length; i++)
            {
                if (collArray[i].CompareTag("Interact"))
                {
                    collArray[i].GetComponent<InteractScript>().Interact();
                }
            }
        }
        */
    }
}

public enum PlayerState
{
    Regular,
    Dashing,
    Attacking,
    Hitstun
}