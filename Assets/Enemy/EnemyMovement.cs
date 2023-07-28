using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyMovement : MonoBehaviour
{
    Enemy enemy;
    private TilemapCollider2D tilemapCollider;
    [SerializeField] List<Vector3Int> pathList;
    private Coroutine moveToPathCoroutine;
    [SerializeField] float moveDelay;
    [SerializeField] Vector3Int spawnPos, randomDest ,minRange, maxRange;
    [SerializeField] bool patrol, moving, kejar, attacking;
    [SerializeField] private float rayLength, sphereRadius;
    private Vector3 offsetRay = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector3 lookDirection, movementDirection, previousPosition;
    private float gridSize = 1f;

    private void Awake()
    {
        tilemapCollider = ManajerTiles.Instance.tilemap.GetComponent<TilemapCollider2D>();
        spawnPos = Vector3Int.RoundToInt(transform.position);
    }
    private void Start() 
    {
        enemy = this.gameObject.GetComponent<Enemy>();
        previousPosition = spawnPos;    
        OccupyTilemap(previousPosition);
        patrol = false;
        moving = false;
        kejar = false;
        attacking = false;
    }

    void FixedUpdate()
    {
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
            if (ManajerTiles.Instance.tilemap.HasTile(tilePosition) && ManajerTiles.Instance.tilemap.GetTile(tilePosition) == ManajerTiles.Instance.walkableTile)
            {
                // Jika tile yang ditekan adalah tile yang bisa dilewati,
                // jalankan algoritma A* untuk mencari path dari posisi karakter ke posisi klik
                Vector3Int startCell = ManajerTiles.Instance.grid.WorldToCell(transform.position);
                Vector3Int endCell = tilePosition;
                StartPathfinding(startCell, endCell);
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
            StartPathfinding(startCell, endCell);
        }
        yield return null;
    }    

    private List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        // Inisialisasi open list dan closed list
        List<Vector3Int> openList = new List<Vector3Int>();
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        // Tambahkan titik awal ke open list
        openList.Add(start);

        // Buat dictionary untuk menyimpan parent dari setiap sel
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

        // Buat dictionary untuk menyimpan g cost dari setiap sel
        Dictionary<Vector3Int, float> gCost = new Dictionary<Vector3Int, float>();
        gCost[start] = 0;

        // Buat dictionary untuk menyimpan f cost dari setiap sel
        Dictionary<Vector3Int, float> fCost = new Dictionary<Vector3Int, float>();
        fCost[start] = GetDistance(start, end);

        // Loop hingga open list kosong
        while (openList.Count > 0)
        {
            Vector3Int current = GetLowestFCost(openList, fCost);
            openList.Remove(current);
            closedList.Add(current);

            // Jika current adalah titik akhir, berarti path ditemukan
            if (current == end)
            {
                // Rekonstruksi path dari parent dictionary
                while (parent.ContainsKey(current))
                {
                    path.Add(current);
                    current = parent[current];
                }

                // Reverse path agar urutan titik awal ke titik akhir
                path.Reverse();

                return path;
            }

            // Loop melalui tetangga-tetangga current
            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                TileBase tile = ManajerTiles.Instance.tilemap.GetTile(neighbor);
                // Jika neighbor ada dalam closed list atau tidak dapat dilalui, lewati
                if (closedList.Contains(neighbor) || tile == ManajerTiles.Instance.protectedTile || tile == null)
                {
                    continue;
                }

                // Hitung g cost dari current ke neighbor melalui current path
                float tentativeGCost = gCost[current] + GetDistance(current, neighbor);

                // Jika neighbor belum ada dalam open list atau memiliki g cost yang lebih rendah,
                // atau tidak ada g cost sebelumnya (belum dieksplorasi),
                // update parent, g cost, dan f cost neighbor
                if (!openList.Contains(neighbor) || tentativeGCost < gCost[neighbor] || !gCost.ContainsKey(neighbor))
                {
                    parent[neighbor] = current;
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = tentativeGCost + GetDistance(neighbor, end);

                    // Jika neighbor belum ada dalam open list, tambahkan ke open list
                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        // Jika sampai di sini, berarti path tidak ditemukan
        return null;
    }

    private Vector3Int GetLowestFCost(List<Vector3Int> list, Dictionary<Vector3Int, float> fCost)
    {
        Vector3Int lowestFCostCell = list[0];
        float lowestFCost = fCost[lowestFCostCell];

        for (int i = 1; i < list.Count; i++)
        {
            if (fCost[list[i]] < lowestFCost)
            {
                lowestFCost = fCost[list[i]];
                lowestFCostCell = list[i];
            }
        }

        return lowestFCostCell;
    }

    private List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();

        // Menambahkan tetangga atas, bawah, kiri, dan kanan
        neighbors.Add(new Vector3Int(cell.x, cell.y + 1, cell.z));
        neighbors.Add(new Vector3Int(cell.x, cell.y - 1, cell.z));
        neighbors.Add(new Vector3Int(cell.x - 1, cell.y, cell.z));
        neighbors.Add(new Vector3Int(cell.x + 1, cell.y, cell.z));

        return neighbors;
    }

    private float GetDistance(Vector3Int a, Vector3Int b)
    {
    // Menggunakan Manhattan distance sebagai heuristic
    return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerator SeekRandomPath()
    {
        yield return new WaitForSeconds(enemy.enemySO.enemyIntervalMove);
        randomDest = new Vector3Int(
        Random.Range(spawnPos.x - enemy.enemySO.enemyMoveRadius, spawnPos.x + enemy.enemySO.enemyMoveRadius),0,
        Random.Range(spawnPos.z - enemy.enemySO.enemyMoveRadius, spawnPos.z + enemy.enemySO.enemyMoveRadius));
        yield return null;
    }
    
    private void StartPathfinding(Vector3Int start, Vector3Int end)
    {
        // Reset path sebelumnya jika ada
        if (moveToPathCoroutine != null)
        {
            StopCoroutine(moveToPathCoroutine);
            moveToPathCoroutine = null;
        }

        // Menjalankan algoritma A* untuk mencari path
        List<Vector3Int> path = FindPath(start, end);

        // Jika ditemukan path, jalankan Coroutine MoveToPath
        if (path != null)
        {
            moving = true;
            moveToPathCoroutine = StartCoroutine(MoveToPath(path));
        } else
        {
            moving = false;
            patrol = false;
            kejar = false;
        }
    }
    private IEnumerator MoveToPath(List<Vector3Int> path)
    {
        // Menentukan kecepatan pergerakan karakter
        // Loop untuk menggerakkan karakter ke setiap sel dalam path
        for (int i = 0; i < path.Count; i++)
        {
            // Menghitung posisi dunia dari sel pada path
            Vector3 targetPosition = ManajerTiles.Instance.tilemap.CellToWorld(path[i]);

            // Menggerakkan karakter secara smooth menuju target position
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + offsetRay, targetPosition - transform.position, out hit, Vector3.Distance(transform.position, targetPosition)))
                {
                    // Jika pemain bersentuhan dengan sesuatu, hentikan pergerakan
                    moving = false;
                    yield break;
                }
                movementDirection = targetPosition - transform.position;
                LookRotation();
                Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, enemy.enemySO.enemyWalkSpeed * Time.deltaTime);
                OccupyTilemap(targetPosition);
                newPos.y = transform.position.y;
                transform.position = newPos;
                yield return null;
            }

            // Menunggu sejenak sebelum melanjutkan ke sel berikutnya
            yield return new WaitForSeconds(moveDelay);
        }
        // Reset Coroutine menjadi null setelah selesai
        moveToPathCoroutine = null;
        pathList.Clear();
        moving = false;
        patrol = false;
        kejar = false;
    }
    public void OccupyTilemap(Vector3 newPosition)
    {
        // Hapus tile di posisi sebelumnya
        Vector3Int previousPos = ManajerTiles.Instance.tempTilemap.WorldToCell(previousPosition);
        Vector3Int newPos =  ManajerTiles.Instance.tempTilemap.WorldToCell(newPosition);
        ManajerTiles.Instance.RemoveTile(previousPos);

        // Tambahkan tile di posisi baru
        ManajerTiles.Instance.AddTile(newPos);

        // Lakukan update posisi sebelumnya dengan posisi yang baru
        previousPosition = newPosition;
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