using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
public class PlayerMovement : MonoBehaviour
{
    private Character character;
    private Joystick joystick;
    private float gridSize = 1f;
    private float moveSpeed = 1.5f;
    private Vector3 lookDirection = Vector3.forward;
    [SerializeField] private Vector3 targetPos;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private Vector3Int  prevTilePos, currTilePos;
    private void Start()
    {
        character = GetComponent<Character>();
        joystick = GameObject.Find("CanvasUtama").GetComponentInChildren<Joystick>();
        targetPos = character.characterSO.ReturnPosition();
    }
    public void TeleportPos(Vector3 newPos)
    {
        targetPos = newPos;
    }

    private void Update() 
    {
        float inputHorizontal = joystick.inputVector.x;
        float inputVertical = joystick.inputVector.z;
        if(isMoving)
        {
            character.ChangeAnimationState("move");
            inputHorizontal = 0;
            inputVertical = 0;
        } else
        {
            character.ChangeAnimationState("idle");
        }
        if (inputHorizontal != 0 || inputVertical != 0)
        {
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
        
            if (lookDirection != Vector3.zero)
            {
                // Membuat rotasi berdasarkan lookDirection
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                transform.GetChild(0).transform.rotation = targetRotation;
            }
            
            // ubah targetPosition sesuai arah pandangan
            if ((transform.position - targetPos).magnitude < 0.01f)
            {
                if (inputHorizontal > 0)
                {
                    targetPos += Vector3.right * gridSize;
                }
                if (inputHorizontal < 0)
                {
                    targetPos += Vector3.left * gridSize;
                }

                if (inputVertical > 0)
                {
                    targetPos += Vector3.forward * gridSize;
                }
                if (inputVertical < 0)
                {
                    targetPos += Vector3.back * gridSize;
                }
            }
        }

        Vector3Int tilePos = ManajerTiles.Instance.tilemap.WorldToCell(targetPos);
        if (ManajerTiles.Instance.tilemap.HasTile(tilePos) && (ManajerTiles.Instance.tilemap.GetTile(tilePos) == ManajerTiles.Instance.walkableTile || ManajerTiles.Instance.tilemap.GetTile(tilePos) == ManajerTiles.Instance.protectedTile)) 
        {
            isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            if((transform.position - targetPos).magnitude < 0.05f)
            {
                isMoving = false;
                RoundPosition();
            }
        } else
        {
            targetPos = transform.position;
            isMoving = false;
            RoundPosition();
        }
    }
    private void RoundPosition()
    {
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Round(newPosition.x / gridSize) * gridSize;
        newPosition.y = Mathf.Round(newPosition.y / gridSize) * gridSize;
        newPosition.z = Mathf.Round(newPosition.z / gridSize) * gridSize;
        transform.position = newPosition;
        character.characterSO.x = newPosition.x;
        character.characterSO.y = newPosition.y;
        character.characterSO.z = newPosition.z;
        OccupyTilemap();
    }

    private void OccupyTilemap()
    {
        currTilePos = ManajerTiles.Instance.tempTilemap.WorldToCell(transform.position);
        if (currTilePos != prevTilePos)
        {
            // Hapus tile pada posisi sebelumnya
            ManajerTiles.Instance.tempTilemap.SetTile(prevTilePos, null);

            // Atur tile baru pada posisi saat ini
            ManajerTiles.Instance.tempTilemap.SetTile(currTilePos,ManajerTiles.Instance.charTile);

            // Perbarui posisi sel tile sebelumnya menjadi posisi saat ini
            prevTilePos = currTilePos;
        }
    }
}

           
