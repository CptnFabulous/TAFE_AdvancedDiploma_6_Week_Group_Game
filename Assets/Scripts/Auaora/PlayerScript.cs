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
        [SerializeField] private Animator playerVisualsAnim;
        [SerializeField] private PlayerCameraScript cameraRef;
        [SerializeField] private GameObject attackIndicator;
        [SerializeField] private GameObject attackIndicatorTarget;
        [SerializeField] private GameObject attackIndicatorGround;
        [SerializeField] private GameObject visualRef;
        private HUDScript hudRef;

        [Header("Movement Variables")]
        private Vector2 speed;
        [SerializeField] private float runSpeed = 4f;
        [SerializeField] private float attackRunSpeed = 2f;
        private bool mouseAim = false;
        [SerializeField] private LayerMask groundMask;
        private Vector2 knockbackVector = Vector2.zero;
        [SerializeField] private float fallTimerMax = 0.5f;
        private float fallTimer = 1f;
        private bool attackSlowdown = false;
        private bool attackStop = false;

        [Header("Dash Variables")]
        private bool attackBlockingDash = false;
        [SerializeField] private float dashSpeed = 8;
        private float internalDashTimer;
        [SerializeField] private float dashTime = 0.3f;
        [SerializeField] private float dashCooldown = 1.1f;
        private float internalDashCooldown = -1f;
        private Vector2 dashVector = Vector2.zero;
        private bool dashOffCooldown = true;

        [Header("Misc Variables")]
        [SerializeField] private float maxRange = 2f;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float attackForce = 5f;
        [SerializeField] private LayerMask attackMask;
        [SerializeField] private LayerMask placeableMask;

        private PlayerState currentState = PlayerState.Regular;
        public PlayerState CurrentState { get { return currentState; } }
        [SerializeField] private int maxHealth;
        private int currentHealth;

        public BlockInteraction blockInteraction;

        private float intangible = 0f;
        private bool dead;

        // Sets some things automatically
        void Start()
        {
            currentHealth = maxHealth + AbilityManager.SoleManager.GetHealthBonus();
            rigRef = GetComponent<Rigidbody>();
            hudRef = FindObjectOfType<HUDScript>();
            hudRef.SetHealthAmount(currentHealth, maxHealth + AbilityManager.SoleManager.GetHealthBonus());

            fallTimer = fallTimerMax;
            AbilityManager.SoleManager.ZeroPlatforms(); //temp?

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
            if (internalDashTimer > 0 && currentState == PlayerState.Dashing)
            {
                rigRef.velocity = new Vector3(dashVector.x * dashSpeed, 0f, dashVector.y * dashSpeed);
                intangible = 0.2f;
                internalDashTimer -= 1 * Time.deltaTime;
                if (internalDashTimer <= 0)
                {
                    EndDash();
                }
            }

            //Checks for dash input
            if (Input.GetButtonDown("Dash"))
            {
                if ((IfPlayerNotState(false, true, true, true, true) || (currentState == PlayerState.Attacking && !attackBlockingDash)) && dashOffCooldown)
                {
                    if (!dead)
                    {
                        currentState = PlayerState.Dashing;
                        BeginDash();
                    }
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
            if (IfPlayerNotState(false, true, false, true, true) && Time.timeScale != 0)
            {
                speed.x = Input.GetAxisRaw("Horizontal") * 10f;
                speed.y = Input.GetAxisRaw("Vertical") * 10f;
                speed.Normalize();
                if (attackStop)
                {
                    speed *= 0;
                }
                else if (attackSlowdown)
                {
                    speed *= attackRunSpeed;
                }
                else
                {
                    speed *= runSpeed;
                }
            }

            // Apply movement
            if (!dead && Time.timeScale != 0)
            {
                if (currentState == PlayerState.Regular || currentState == PlayerState.Attacking)
                {
                    rigRef.velocity = GetMovementDirection();
                    if (currentState == PlayerState.Regular)
                    {
                        if (speed != Vector2.zero)
                        {
                            RotateVisuals(GetMovementDirection());
                        }
                    }
                    else
                    {
                        RotateVisuals(attackIndicatorTarget.transform.position - transform.position);
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
                        if (fallTimer <= 0f)
                        {
                            Fall();
                        }
                        else
                        {
                            fallTimer -= Time.deltaTime;
                        }
                    }
                    else
                    {
                        fallTimer = fallTimerMax;
                    }
                }

                Vector3 newPos = transform.position + new Vector3(AimInputVector().x, 0f, AimInputVector().y);
                Vector3 newDir = (newPos - transform.position).normalized;
                newDir = Quaternion.Euler(0, 45, 0) * newDir;
                attackIndicator.transform.LookAt(transform.position + newDir);
                attackIndicatorTarget.transform.localPosition = new Vector3(0f, 0f, Mathf.Clamp(AimInputMagnitude(), 0.5f, maxRange * AbilityManager.SoleManager.GetAttackRangeBonus()));
                attackIndicatorGround.transform.position = new Vector3(Mathf.Round(attackIndicatorTarget.transform.position.x), attackIndicatorGround.transform.position.y, Mathf.Round(attackIndicatorTarget.transform.position.z));

                HandleAnimation();

                if (internalDashCooldown != -1f)
                {
                    if (internalDashCooldown >= (dashCooldown - AbilityManager.SoleManager.GetDashCooldownBonus()))
                    {
                        dashOffCooldown = true;
                        internalDashCooldown = -1f;
                    }
                    else
                    {
                        internalDashCooldown += Time.deltaTime;
                    }
                    hudRef.UpdateDashCooldown(internalDashCooldown, (dashCooldown - AbilityManager.SoleManager.GetDashCooldownBonus()));
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
        }

        private void Fall()
        {
            AnimateTrigger("Fall");
            Die();
        }

        private void BeKilled()
        {
            AnimateTrigger("Death");
            Die();
        }

        private void Die()
        {
            dead = true;
            rigRef.velocity = Vector3.zero;
            speed = Vector2.zero;

            StartCoroutine(nameof(StartAgain));
        }

        /// <summary>
        /// temp
        /// </summary>
        private IEnumerator StartAgain()
        {
            yield return new WaitForSeconds(3);
            GameManager.Instance.StartAgain();
        }

        private void EndHitstun()
        {
            currentState = PlayerState.Regular;
            AnimateTrigger("StunEnd");
        }

        //Ends dashing
        private void EndDash()
        {
            internalDashTimer = 0;
            currentState = PlayerState.Regular;
            gameObject.layer = 13;
        }

        private void BeginDash()
        {
            Vector2 direction = new Vector2(GetMovementDirection().x, GetMovementDirection().z).normalized;
            if (direction == Vector2.zero)
            {
                EndDash();
                return;
            }
            rigRef.velocity = new Vector3(direction.x * dashSpeed, 0f, direction.y * dashSpeed);
            dashVector = direction;
            internalDashTimer = dashTime;
            dashOffCooldown = false;
            internalDashCooldown = 0f;

            currentState = PlayerState.Dashing;
            intangible = 0.8f;
            gameObject.layer = 16;

            AnimateTrigger("Dash");
            RotateVisuals(rigRef.velocity);

            Instantiate(Resources.Load("Effects/Dash"), transform.position, visualRef.transform.rotation);
        }

        public void TakeDamage(float damage, Vector3 damagePos, bool knockback = true)
        {
            if (!dead)
            {
                if (damage <= 0)
                {
                    return;
                }
                DeactivateAttackSlowdown();
                DeactivateAttackStop();
                currentState = PlayerState.Hitstun;
                attackSlowdown = false;
                gameObject.layer = 16;
                intangible = 2f;
                currentHealth -= Mathf.FloorToInt(damage);
                hudRef.SetHealthAmount(currentHealth, maxHealth + AbilityManager.SoleManager.GetHealthBonus());
                if (currentHealth <= 0)
                {
                    BeKilled();
                }
                else
                {
                    if (knockback)
                    {
                        knockbackVector = (transform.position - damagePos).normalized * 10f;
                        rigRef.velocity = knockbackVector;
                        AnimateTrigger("Stun");
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
            if (currentState == PlayerState.Regular && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
            {
                AnimateBool("Walk", true);
            }
            else
            {
                AnimateBool("Walk", false);
            }
        }

        private void AnimateTrigger(string triggerName)
        {
            playerAnim.SetTrigger(triggerName);
            playerVisualsAnim.SetTrigger(triggerName);
        }

        private void AnimateBool(string boolName, bool boolValue)
        {
            playerAnim.SetBool(boolName, boolValue);
            playerVisualsAnim.SetBool(boolName, boolValue);
        }

        private void AnimateFloat(string floatName, float floatValue)
        {
            playerAnim.SetFloat(floatName, floatValue);
            playerVisualsAnim.SetFloat(floatName, floatValue);
        }

        public bool IsIntangible()
        {
            return intangible > 0 ? true : false;
        }

        public void HealPlayer(float health)
        {
            health = Mathf.Floor(Mathf.Clamp(health, 1f, 999f));
            if (currentHealth < (maxHealth + AbilityManager.SoleManager.GetHealthBonus()) && currentHealth > 0 && !dead)
            {
                currentHealth += Mathf.FloorToInt(health);
            }
            hudRef.SetHealthAmount(currentHealth, maxHealth + AbilityManager.SoleManager.GetHealthBonus());
        }

        public Vector3 GetMovementDirection()
        {
            Vector3 velDir = new Vector3(speed.x, 0f, speed.y);
            velDir = Quaternion.Euler(0, 45, 0) * velDir;
            return velDir;
        }

        public Vector2 AimInputVector()
        {
            Vector2 point1 = cameraRef.GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
            Vector2 point2 = cameraRef.GetComponent<Camera>().WorldToViewportPoint(visualRef.transform.position);
            Vector2 direction = (point1 - point2).normalized;

            return direction;
        }

        private float AimInputMagnitude()
        {
            Vector2 point1 = cameraRef.GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
            Vector2 point2 = cameraRef.GetComponent<Camera>().WorldToViewportPoint(visualRef.transform.position);
            print("MAGNITUDE:" + (point1 - point2).magnitude);
            return (point1 - point2).magnitude * 15f;
        }

        private void StartAttack()
        {
            AnimateFloat("AttackSpeed", AbilityManager.SoleManager.GetAttackSpeed());
            AnimateTrigger("Attack");
            currentState = PlayerState.Attacking;
            rigRef.velocity = Vector3.zero;
            RotateVisuals(attackIndicatorTarget.transform.position - transform.position);
        }

        public void EnactAttack()
        {
            //particle effect
            Instantiate(Resources.Load("Effects/Shockwave"), attackIndicatorTarget.transform.position, Quaternion.identity);

            print("ATTACKING " + attackIndicatorTarget.transform.position);
            Collider[] hitObjects = Physics.OverlapBox(attackIndicatorTarget.transform.position, new Vector3(1f * AbilityManager.SoleManager.GetAttackRangeBonus(), 0.3f, 1f * AbilityManager.SoleManager.GetAttackRangeBonus()), attackIndicator.transform.rotation, attackMask);
            bool enemy = hitObjects.Length > 0;
            print("THINGS HIT: " + hitObjects.Length);
            foreach (Collider hitCol in hitObjects)
            {
                if (hitCol.GetComponent<EnemyBehaviour>())
                {
                    print("KNOCKBACK");
                    Vector3 aim = attackIndicator.transform.forward;
                    aim = Quaternion.Euler(-45, 0, 0) * aim;
                    Vector3 direction = new Vector3(aim.x, 0.25f, aim.y);
                    hitCol.GetComponent<EnemyBehaviour>().Knockback(direction * (attackForce + AbilityManager.SoleManager.GetAttackForceBonus()));
                    if (AbilityManager.SoleManager.DoesPlayerHaveSpecialAbility(0))
                    {
                        AbilityManager.SoleManager.SpawnSpecialAbility(0, hitCol.transform.position + (hitCol.transform.position - transform.position).normalized).GetComponent<DelayedExplosiveScript>().SetFollow(hitCol.gameObject, (hitCol.transform.position - transform.position).normalized);
                    }
                }
            }
            if (!enemy)
            {
                // break block beneath
                if (blockInteraction.TryCheckBlock(attackIndicatorTarget.transform.position, Vector3.down))
                {
                    blockInteraction.TargetedChunk.DamageBlock(blockInteraction.TargetedBlockCoords, 1);
                }
                RaycastHit hit;
                Physics.Raycast(attackIndicatorTarget.transform.position, Vector3.down, out hit, 3f, placeableMask);
                if (hit.collider)
                {
                    if (hit.collider.GetComponent<PlaceablePlatformScript>())
                    {
                        hit.collider.GetComponent<PlaceablePlatformScript>().BreakPlatform();
                    }
                }
                else if (AbilityManager.SoleManager.DoesPlayerHaveSpecialAbility(1))
                {
                    AbilityManager.SoleManager.SpawnSpecialAbility(1, new Vector3(Mathf.Round(attackIndicatorTarget.transform.position.x / 2) * 2, 0.5f, Mathf.Round(attackIndicatorTarget.transform.position.z / 2) * 2));
                }
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

        public void ActivateAttackSlowdown()
        {
            attackSlowdown = true;
        }

        public void DeactivateAttackSlowdown()
        {
            attackSlowdown = false;
        }

        public void ActivateAttackStop()
        {
            attackStop = true;
        }

        public void DeactivateAttackStop()
        {
            attackStop = false;
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