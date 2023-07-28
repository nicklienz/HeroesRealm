using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyMoves : MonoBehaviour
{
    private Pathfinding pathfinding;
    private Enemy enemy;
    [SerializeField] private Vector3Int spawnPos, randomDest;
    [SerializeField] private bool patrol, moving, kejar, attacking;
    [SerializeField] private float sphereRadius;
    private Vector3 offsetRay = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector3 lookDirection, movementDirection;
    private float gridSize = 1f;

    private void Awake()
    {
        pathfinding = GetComponent<Pathfinding>();
        spawnPos = Vector3Int.RoundToInt(transform.position);
    }
    private void Start() 
    {
        enemy = this.gameObject.GetComponent<Enemy>();
        patrol = false;
        moving = false;
        kejar = false;
        attacking = false;
    }

    void Update()
    {
        if(!pathfinding.isMoving)
        {
            RoundPosition();
        }
        DetectPlayer();
        CollidePlayer();
        if(!moving && !patrol && enemy.enemyState == EnemyState.patroli)
        {
            patrol = true;
            kejar = false;
            attacking = false;
            StartCoroutine(EnemyAIMove());
        } else if (enemy.enemyState == EnemyState.kejar && !kejar && enemy.playerDetected != null)
        {
            kejar = true;
            patrol = false;
            attacking = false;
            StartCoroutine(EnemyAIKejar(enemy.playerDetected.transform));
        } else if(enemy.enemyState == EnemyState.attack && enemy.playerCollided != null && !attacking)
        {
            attacking = true;
            StartCoroutine(enemy.AttackingPlayer(enemy.playerCollided.gameObject.GetComponent<Character>()));
        }
    }

    private void DetectPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + offsetRay, enemy.enemySO.enemyDetectRadius);
        bool playerDetected = false;
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.tag =="Player")
            {
                enemy.enemyState = EnemyState.kejar;
                enemy.playerDetected = collider.transform;
                playerDetected = true;
                break;
            }
        }
        if (!playerDetected)
        {
            enemy.playerDetected = null;
            kejar = false;
            enemy.enemyState = EnemyState.patroli;
        }
        
    }
    private void CollidePlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + offsetRay, sphereRadius);
        bool playerCollided = false;
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                enemy.enemyState = EnemyState.attack;
                enemy.playerCollided = collider.transform;
                playerCollided = true;
                break;
            }
        }
        if(!playerCollided)
        {
            enemy.playerCollided = null;
            kejar = false;
        }
        
    }    
    private IEnumerator EnemyAIMove()
    {
        while (!moving && patrol)
        {
            StartCoroutine(SeekRandomPath());
            yield return null;
            Vector3Int tilePosition = ManajerTiles.Instance.tilemap.WorldToCell(randomDest);
            if (ManajerTiles.Instance.tilemap.HasTile(tilePosition) && ManajerTiles.Instance.tilemap.GetTile(tilePosition) == ManajerTiles.Instance.walkableTile && ManajerTiles.Instance.tilemap.GetTile(tilePosition) == ManajerTiles.Instance.temporaryTile )
            {
                // Jika tile yang ditekan adalah tile yang bisa dilewati,
                // jalankan algoritma A* untuk mencari path dari posisi karakter ke posisi klik
                Vector3Int startCell = ManajerTiles.Instance.grid.WorldToCell(transform.position);
                Vector3Int endCell = tilePosition;
                pathfinding.StartPathfinding(startCell, endCell, enemy.enemySO.enemyWalkSpeed);
                //Debug.Log("executed");
                yield return new WaitForSeconds(enemy.enemySO.enemyIntervalMove);
            }
        }
        yield return null;
    }
    private IEnumerator EnemyAIKejar(Transform playerTransform)
    {
        Vector3Int tilePosition = ManajerTiles.Instance.tilemap.WorldToCell(playerTransform.position);
        if (ManajerTiles.Instance.tilemap.HasTile(tilePosition) && ManajerTiles.Instance.tilemap.GetTile(tilePosition) == ManajerTiles.Instance.walkableTile)
        {
            // Jika tile yang ditekan adalah tile yang bisa dilewati,
            // jalankan algoritma A* untuk mencari path dari posisi karakter ke posisi klik
            Vector3Int startCell = ManajerTiles.Instance.grid.WorldToCell(transform.position);
            Vector3Int endCell = tilePosition;
            pathfinding.StartPathfinding(startCell, endCell, enemy.enemySO.enemyWalkSpeed);
        }
        yield return null;
    }    
    private IEnumerator SeekRandomPath()
    {
        yield return new WaitForSeconds(enemy.enemySO.enemyIntervalMove);
        randomDest = new Vector3Int(
        Random.Range(spawnPos.x - enemy.enemySO.enemyMoveRadius, spawnPos.x + enemy.enemySO.enemyMoveRadius),0,
        Random.Range(spawnPos.z - enemy.enemySO.enemyMoveRadius, spawnPos.z + enemy.enemySO.enemyMoveRadius));
        yield return null;
    }

    private void LookRotation()
    {
        if(Mathf.Abs(movementDirection.x)> Mathf.Abs(movementDirection.z))
        {
            movementDirection.x = Mathf.Sign(movementDirection.x);
            movementDirection.z = 0;
        } else if(Mathf.Abs(movementDirection.x) < Mathf.Abs(movementDirection.z))
        {
            movementDirection.x = 0;
            movementDirection.z = Mathf.Sign(movementDirection.z);
        }
        if (movementDirection != Vector3.zero)
        {
            lookDirection = movementDirection.normalized; // Normalisasi vektor gerakan menjadi vektor satuan
        }

        if (lookDirection != Vector3.zero)
        {
            // Membuat rotasi berdasarkan lookDirection
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.GetChild(0).transform.rotation = targetRotation;
        }
    }
    private void RoundPosition()
    {
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Round(newPosition.x / gridSize) * gridSize;
        newPosition.y = Mathf.Round(newPosition.y / gridSize) * gridSize;
        newPosition.z = Mathf.Round(newPosition.z / gridSize) * gridSize;
        transform.position = newPosition;
    }
    
}