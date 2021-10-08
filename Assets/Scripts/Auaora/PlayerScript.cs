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
        [SerializeField] private GameObject attackIndicator;
        [SerializeField] private GameObject attackIndicatorTarget;
        [SerializeField] private GameObject visualRef;
        private HUDScript hudRef;

        [Header("Movement Variables")]
        private Vector2 speed;
        [SerializeField] private float maxSpeed = 4f;
        private bool mouseAim = false;
        [SerializeField] private LayerMask groundMask;
        private Vector2 knockbackVector = Vector2.zero;

        [Header("Dash Variables")]
        private bool attackBlockingDash = false;
        [SerializeField] private float dashSpeed = 8;
        private float internalDashCooldown;
        [SerializeField] private float dashCooldown = 0.3f;
        private Vector2 dashVector = Vector2.zero;

        [Header("Misc Variables")]
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float attackForce = 5f;
        [SerializeField] private LayerMask attackMask;
        private PlayerState currentState = PlayerState.Regular;
        public PlayerState CurrentState { get { return currentState; } }
        [SerializeField] private int maxHealth;
        private int currentHealth;

        public BlockInteraction BlockInteraction;

        private float intangible = 0f;
        private bool dead;

        // Sets some things automatically
        void Start()
        {
            currentHealth = maxHealth + AbilityManager.SoleManager.GetHealthBonus();
            rigRef = GetComponent<Rigidbody>();
            hudRef = FindObjectOfType<HUDScript>();

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
                if (IfPlayerNotState(false, true, true, true, true) || (currentState == PlayerState.Attacking && !attackBlockingDash))
                {
                    currentState = PlayerState.Dashing;
                    BeginDash();
                }
            }

            if (Input.GetButtonDown("Attack"))
            {
                if (IfPlayerNotState(false, true, true, true, true))
                {
                    StartAttack();
                }
            }

            // Control speed
            if (IfPlayerNotState(false, true, true, true, true) && Time.timeScale != 0)
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
                    if (speed != Vector2.zero)
                    {
                        RotateVisuals(new Vector3(speed.x, 0f, speed.y));
                    }
                }
                else if (currentState == PlayerState.Hitstun)
                {
                    if (knockbackVector.magnitude <= 0.5f || rigRef.velocity.magnitude <= 0.5f)
                    {
                        EndHitstun();
                    }
                    else
                    {
                        rigRef.velocity = Vector2.SmoothDamp(knockbackVector, Vector2.zero, ref knockbackVector, 1.5f);
                    }
                }
                
                if (currentState != PlayerState.Dashing)
                {
                    //print("Checking Fall");
                    if (!Physics.Raycast(transform.position, Vector3.down, 1.5f, groundMask))
                    {
                        Fall();
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
                attackIndicator.transform.LookAt(new Vector3(transform.position.x + AimInputVector().x, transform.position.y, transform.position.z + AimInputVector().y));

                /*
                Vector3 target = (Vector3)AimInputVector();
                float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
                Quaternion quat = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
                dashIndicator.transform.rotation = Quaternion.Slerp(dashIndicator.transform.rotation, quat, 0.5f);
                */

                HandleAnimation();
            }
        }

        private void Fall()
        {
            playerAnim.SetTrigger("Fall");
            Die();
        }

        private void BeKilled()
        {
            playerAnim.SetTrigger("Death");
            Die();
        }

        private void Die()
        {
            dead = true;
            rigRef.velocity = Vector3.zero;
            speed = Vector2.zero;
        }

        private void EndHitstun()
        {
            currentState = PlayerState.Regular;
            playerAnim.SetTrigger("StunEnd");
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

            playerAnim.SetTrigger("Dash");
            RotateVisuals(rigRef.velocity);
        }

        public void TakeDamage(float damage, Vector3 damagePos, bool knockback = true)
        {
            if (!dead)
            {
                if (damage <= 0)
                {
                    return;
                }
                currentState = PlayerState.Hitstun;
                gameObject.layer = 16;
                intangible = 2f;
                currentHealth -= Mathf.FloorToInt(damage);
                hudRef.SetHealthAmount(currentHealth, maxHealth);
                if (currentHealth <= 0)
                {
                    BeKilled();
                }
                else
                {
                    if (knockback)
                    {
                        knockbackVector = (transform.position - damagePos).normalized * 20f;
                        rigRef.velocity = knockbackVector;
                        playerAnim.SetTrigger("Stun");
                    }
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

        private void RotateVisuals(Vector3 inDir)
        {
            visualRef.transform.rotation = Quaternion.LookRotation(inDir);
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
            }
            else
            {
                direction = new Vector2(Input.GetAxisRaw("AimHor"), Input.GetAxisRaw("AimVer"));
            }
            direction.Normalize();
            return direction;
        }

        private void StartAttack()
        {
            playerAnim.SetFloat("AttackSpeed", AbilityManager.SoleManager.GetAttackSpeed());
            playerAnim.SetTrigger("Attack");
            currentState = PlayerState.Attacking;
            rigRef.velocity = Vector3.zero;
            RotateVisuals(attackIndicatorTarget.transform.position - transform.position);
        }

        public void EnactAttack()
        {
            print("ATTACKING " + attackIndicatorTarget.transform.position);
            Collider[] hitObjects = Physics.OverlapSphere(attackIndicatorTarget.transform.position, 0.5f, attackMask);
            bool enemy = false;
            print("THINGS HIT: " + hitObjects.Length);
            foreach (Collider hitCol in hitObjects)
            {
                if (hitCol.GetComponent<EnemyBehaviour>())
                {
                    print("KNOCKBACK");
                    Vector2 aim = AimInputVector();
                    Vector3 direction = new Vector3(aim.x, 0.25f, aim.y);
                    hitCol.GetComponent<EnemyBehaviour>().Knockback(direction * (attackForce + AbilityManager.SoleManager.GetAttackForceBonus()));
                }
            }
            if (!enemy)
            {
                // break block beneath
            }
        }

        public void ActivateAttackDashBlock()
        {
            attackBlockingDash = true;
        }

        public void DeactivateAttackDashBlock()
        {
            attackBlockingDash = false;
        }

        private void CancelMeleeAttack()
        {
            playerAnim.Play("TestIdle");
            HandleAnimation();
        }

        public void EndPlayerAsAttacking()
        {
            currentState = PlayerState.Regular;
        }

        private bool IfPlayerNotState(bool regularState, bool dashingState, bool attackingState, bool hitstunState, bool deadState)
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
            if (deadState && dead)
            {
                return false;
            }
            return true;
        }

        public void PrintState()
        {
            print("State: " + currentState.ToString());
        }
    }
}

public enum PlayerState
{
    Regular,
    Dashing,
    Attacking,
    Hitstun
}