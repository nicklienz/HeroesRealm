using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
public class AutoGridWalk : MonoBehaviour
{
    public Tilemap tilemap; // Tilemap yang akan digunakan untuk pathfinding
    public TileBase walkableTile, protectedTile; // Tile yang dianggap sebagai jalur yang bisa dilewati
    private TilemapCollider2D tilemapCollider;
    private Grid grid;
    private float gridSize = 1f;
    private float moveSpeed = 2f;
    private float moveDelay = 0f;
    [SerializeField] List<Vector3Int> pathList;
    private Coroutine moveToPathCoroutine;
    [SerializeField] private bool stopAutoWalk = false;
    [SerializeField] private bool canControlWalk = true; 
    [SerializeField] private bool depan, belakang, kiri, kanan;
    private Vector3 lookDirection = Vector3.forward;
    [SerializeField] private Vector3 targetPos;

    private void Awake()
    {
        tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
        grid = tilemap.GetComponentInParent<Grid>();
        targetPos = transform.position;
    }

    private void Update() 
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");
        if(Input.GetMouseButtonDown(0) && canControlWalk)
        {
            stopAutoWalk = false;
            Vector3 clickPos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(clickPos);
            Vector3Int tilePos = tilemap.WorldToCell(worldPos);
            Debug.Log(clickPos +"    " + worldPos +"    "+ tilePos);
            if (tilemap.HasTile(tilePos) && (tilemap.GetTile(tilePos) == walkableTile || tilemap.GetTile(tilePos) == protectedTile)) 
            {
                // Jika tile yang ditekan adalah tile yang bisa dilewati,
                // jalankan algoritma A* untuk mencari path dari posisi karakter ke posisi klik
                Vector3Int startCell = grid.WorldToCell(transform.position);
                Vector3Int endCell = tilePos;
                StartPathfinding(startCell, endCell);
            }
        } else if(inputHorizontal != 0 || inputVertical != 0 && canControlWalk)
        {
            stopAutoWalk = true;
            if(Mathf.Abs(inputHorizontal)> Mathf.Abs(inputVertical))
                {
                inputVertical = 0;
            } else if(Mathf.Abs(inputHorizontal) < Mathf.Abs(inputVertical))
            {
                inputHorizontal = 0;
            }

            // ubah arah pandangan
            if (inputHorizontal > 0)
            {
                lookDirection = Vector3.right;
                inputVertical = 0; // set inputVertical menjadi 0 agar tidak bisa bergerak vertical
            }
            else if (inputHorizontal < 0)
            {
                lookDirection = Vector3.left;
                inputVertical = 0;
            }

            if (inputVertical > 0)
            {
                lookDirection = Vector3.forward;
                inputHorizontal = 0; // set inputHorizontal menjadi 0 agar tidak bisa bergerak horizontal
            }
            else if (inputVertical < 0)
            {
                lookDirection = Vector3.back;
                inputHorizontal = 0;
            }

            // ubah targetPosition sesuai arah pandangan
            if ((transform.position - targetPos).magnitude < 0.1f)
            {
                if (inputHorizontal > 0 && !kanan)
                {
                    targetPos += Vector3.right * gridSize;
                }
                if (inputHorizontal < 0 && !kiri)
                {
                    targetPos += Vector3.left * gridSize;
                }

                if (inputVertical > 0 && !depan)
                {
                    targetPos += Vector3.forward * gridSize;
                }
                if (inputVertical < 0 && !belakang)
                {
                    targetPos += Vector3.back * gridSize;
                }
            }
        }
        Vector3Int targetTilePos = tilemap.WorldToCell(targetPos);
        if(Vector3.Distance(transform.position, targetPos) > 0.01f && stopAutoWalk && transform.position != targetPos && (tilemap.HasTile(targetTilePos) && tilemap.GetTile(targetTilePos) == walkableTile || tilemap.GetTile(targetTilePos) == protectedTile))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            canControlWalk = false;
        } else if (stopAutoWalk && transform.position != targetPos && (!tilemap.HasTile(targetTilePos) || tilemap.GetTile(targetTilePos) != walkableTile || tilemap.GetTile(targetTilePos) != protectedTile))
        {
            RoundPosition();
            targetPos = transform.position;
        } else if (targetPos == transform.position)
        {
            canControlWalk = true;
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
                if (closedList.Contains(neighbor) || tile == null)
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

    public void StartPathfinding(Vector3Int start, Vector3Int end)
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
            moveToPathCoroutine = StartCoroutine(MoveToPath(path));
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
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f && !stopAutoWalk)
            {
                canControlWalk = false;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                targetPos = targetPosition;
                yield return null;
            }
            // Menunggu sejenak sebelum melanjutkan ke sel berikutnya
            yield return new WaitForSeconds(moveDelay);
            canControlWalk = true;
        }

        // Reset Coroutine menjadi null setelah selesai
        moveToPathCoroutine = null;
    }
}

           