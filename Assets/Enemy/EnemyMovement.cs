using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyMovement : MonoBehaviour
{
    public Tilemap tilemap; // Tilemap yang akan digunakan untuk pathfinding
    Enemy enemy;
    public TileBase walkableTile, protectedTile; // Tile yang dianggap sebagai jalur yang bisa dilewati
    private TilemapCollider2D tilemapCollider;
    private Grid grid;
    [SerializeField] List<Vector3Int> pathList;
    private Coroutine moveToPathCoroutine;
    [SerializeField] float moveDelay;
    [SerializeField] Vector3Int spawnPos, randomDest, prevDest ,minRange, maxRange;
    [SerializeField] bool moving;

    //private Rigidbody rb;

    private void Awake()
    {
        tilemap = GameObject.Find("Grid").GetComponentInChildren<Tilemap>();
        tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
        grid = tilemap.GetComponentInParent<Grid>();
        spawnPos = Vector3Int.RoundToInt(transform.position);
        //rb = GetComponent<Rigidbody>();
    }
    private void Start() 
    {
        enemy = this.gameObject.GetComponent<Enemy>();    
        moving = false;
        StartCoroutine(EnemyAIMove());
    }
    private IEnumerator EnemyAIMove()
    {
        while (moving == false)
        {
            StartCoroutine(SeekRandomPath());
            yield return null;
            Vector3Int tilePosition = tilemap.WorldToCell(randomDest);
            if (tilemap.HasTile(tilePosition) && tilemap.GetTile(tilePosition) == walkableTile)
            {
                // Jika tile yang ditekan adalah tile yang bisa dilewati,
                // jalankan algoritma A* untuk mencari path dari posisi karakter ke posisi klik
                Vector3Int startCell = grid.WorldToCell(transform.position);
                Vector3Int endCell = tilePosition;
                StartPathfinding(startCell, endCell);
                prevDest = startCell;
                //Debug.Log("executed");
                yield return new WaitForSeconds(enemy.enemySO.enemyIntervalMove);
            }
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
                TileBase tile = tilemap.GetTile(neighbor);
                // Jika neighbor ada dalam closed list atau tidak dapat dilalui, lewati
                if (closedList.Contains(neighbor) || tile == protectedTile || tile == null)
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
        }
    }
    private IEnumerator MoveToPath(List<Vector3Int> path)
    {
        // Menentukan kecepatan pergerakan karakter
        // Loop untuk menggerakkan karakter ke setiap sel dalam path
        for (int i = 0; i < path.Count; i++)
        {
            // Menghitung posisi dunia dari sel pada path
            Vector3 targetPosition = tilemap.CellToWorld(path[i]);

            // Menggerakkan karakter secara smooth menuju target position
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, enemy.enemySO.enemyWalkSpeed * Time.deltaTime);
                yield return null;
            }

            // Menunggu sejenak sebelum melanjutkan ke sel berikutnya
            yield return new WaitForSeconds(moveDelay);
        }
        //Debug.Log("selesai jalan");
        // Reset Coroutine menjadi null setelah selesai
        moveToPathCoroutine = null;
        pathList.Clear();
        moving = false;
    }
}