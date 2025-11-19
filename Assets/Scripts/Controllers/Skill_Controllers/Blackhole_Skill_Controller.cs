using System.Collections.Generic;
using UnityEngine;

public class Blackhole_Skill_Controller : MonoBehaviour
{
    [SerializeField] private GameObject hotKeyPrefab;
    [SerializeField] private List<KeyCode> keyCodeList;

    private float maxSize;
    private float growSpeed;
    private float shrinkSpeed;
    private float blackholeTimer;
    private float originalBlackholeDuration;

    public bool canGrow = true;
    public bool canShrink;
    private bool canCreateHotKeys = true;
    private bool cloneAttackReleased;
    private bool playerCanDisappear = true;

    private int amountOfAttacks = 4;
    private float cloneAttackCooldown = .3f;
    private float cloneAttackTimer;

    private List<Transform> targets = new List<Transform>();
    private List<GameObject> createdHotKey = new List<GameObject>();

    public bool playerCanExitState { get; private set; }

    public void SetupBlackhole(float _maxSize, float _growSpeed, float _shrinkSpeed, int _amountOfAttacks, float _cloneAttackCooldown, float _blackholeDuration)
    {
        maxSize = _maxSize;
        growSpeed = _growSpeed;
        shrinkSpeed = _shrinkSpeed;
        amountOfAttacks = _amountOfAttacks;
        cloneAttackCooldown = _cloneAttackCooldown;

        blackholeTimer = _blackholeDuration;
        originalBlackholeDuration = _blackholeDuration;

        if (SkillManager.instance.clone.crystalInsteadOfClone)
            playerCanDisappear = false;

    }

    private void Update()
    {
        cloneAttackTimer -= Time.deltaTime;

        if (!canShrink)
            blackholeTimer -= Time.deltaTime;

        if (!canShrink && blackholeTimer < originalBlackholeDuration - 1f)
        {
            CheckForEarlyFinish();
        }

        if (blackholeTimer <= 0 && !canShrink)
        {
            blackholeTimer = Mathf.Infinity;

            if (targets.Count > 0 && !cloneAttackReleased)
            {
                ReleaseCloneAttack();
            }
            else
            {
                FinishBlackHoleAbility();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !cloneAttackReleased && !canShrink)
        {
            ReleaseCloneAttack();
        }

        CloneAttackLogic();

        if (canGrow && !canShrink)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(maxSize, maxSize), growSpeed * Time.deltaTime);
        }

        if (canShrink)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);

            if (transform.localScale.x <= 0.05f)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    private void CheckForEarlyFinish()
    {
        targets.RemoveAll(target => target == null);

        if (targets.Count == 0 && createdHotKey.Count == 0 && !cloneAttackReleased)
        {
            FinishBlackHoleAbility();
        }
    }

    private void ReleaseCloneAttack()
    {
        if (cloneAttackReleased) return;
        
        DestroyHotKeys();
        cloneAttackReleased = true;
        canCreateHotKeys = false;

        AutoAddAllEnemies();

        if (targets.Count <= 0)
        {
            FinishBlackHoleAbility();
            return;
        }

        if (playerCanDisappear && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            playerCanDisappear = false;
            PlayerManager.instance.player.fx.MakeTransprent(true);
        }
    }

    private void AutoAddAllEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxSize / 2);
        int addedCount = 0;
        
        foreach (Collider2D collider in colliders)
        {
            if (collider != null && collider.GetComponent<Enemy>() != null)
            {
                if (!targets.Contains(collider.transform))
                {
                    targets.Add(collider.transform);
                    addedCount++;
                }
            }
        }
    }

    private void CloneAttackLogic()
    {
        if (cloneAttackTimer < 0 && cloneAttackReleased && amountOfAttacks > 0)
        {
            cloneAttackTimer = cloneAttackCooldown;

            int removedCount = targets.RemoveAll(target => target == null);

            if (targets == null || targets.Count == 0)
            {

                FinishBlackHoleAbility();
                return;
            }

            int randomIndex = Random.Range(0, targets.Count);

            if (targets[randomIndex] == null)
            {
                targets.RemoveAt(randomIndex);
                return;
            }

            float xOffset = Random.Range(0, 100) > 50 ? 1.5f : -1.5f;

            if (SkillManager.instance == null)
            {
                return;
            }

            if (SkillManager.instance.clone.crystalInsteadOfClone)
            {
                if (SkillManager.instance.crystal != null)
                {
                    SkillManager.instance.crystal.CreateCrystal();
                    SkillManager.instance.crystal.CurrentCrystalChooseRandomTarget();
                }
            }
            else
            {
                if (SkillManager.instance.clone != null && targets[randomIndex] != null)
                {
                    SkillManager.instance.clone.CreateClone(targets[randomIndex], new Vector3(xOffset, 0));
                }
            }
            
            amountOfAttacks--;

            if (amountOfAttacks <= 0)
            {
                Invoke("FinishBlackHoleAbility", 0.5f);
            }
        }
    }

    private void FinishBlackHoleAbility()
    {
        if (canShrink) 
        {
            return;
        }

        
        DestroyHotKeys();
        playerCanExitState = true;
        canShrink = true;
        canGrow = false;
        cloneAttackReleased = true;

        Invoke("ForceDestroy", 3f);
    }

    private void ForceDestroy()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void DestroyHotKeys()
    {
        if (createdHotKey.Count <= 0)
            return;

        for (int i = createdHotKey.Count - 1; i >= 0; i--)
        {
            if (createdHotKey[i] != null)
                Destroy(createdHotKey[i]);
        }
        createdHotKey.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.GetComponent<Enemy>() != null)
        {
            collision.GetComponent<Enemy>().FreezeTime(true);
            CreateHotKey(collision);
            
            if (!targets.Contains(collision.transform))
            {
                targets.Add(collision.transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null && collision.GetComponent<Enemy>() != null)
        {
            collision.GetComponent<Enemy>().FreezeTime(false);
            
            if (targets.Contains(collision.transform))
                targets.Remove(collision.transform);
        }
    }


    private void CreateHotKey(Collider2D collision)
    {
        if (keyCodeList.Count <= 0)
        {
            return;
        }

        if (!canCreateHotKeys)
        {
            return;
        }

        if (collision == null || collision.transform == null)
        {
            return;
        }

        GameObject newHotKey = Instantiate(hotKeyPrefab, collision.transform.position + new Vector3(0, 2), Quaternion.identity);
        
        Rigidbody2D rb = newHotKey.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
        
        createdHotKey.Add(newHotKey);

        KeyCode chosenKey = keyCodeList[Random.Range(0, keyCodeList.Count)];
        keyCodeList.Remove(chosenKey);

        Blackhole_HotKey_Controller newHotKeyScript = newHotKey.GetComponent<Blackhole_HotKey_Controller>();

        if (newHotKeyScript != null)
        {
            newHotKeyScript.SetupHotKey(chosenKey, collision.transform, this);
        }
    }

    public void AddEnemyToList(Transform _enemyTransform)
    {
        if (_enemyTransform != null && !targets.Contains(_enemyTransform))
        {
            targets.Add(_enemyTransform);
        }
    }

    private void OnDestroy()
    {  
        DestroyHotKeys();
        
        foreach (Transform target in targets)
        {
            if (target != null && target.GetComponent<Enemy>() != null)
            {
                target.GetComponent<Enemy>().FreezeTime(false);
            }
        }
        targets.Clear();
        
        CancelInvoke();
    }
}