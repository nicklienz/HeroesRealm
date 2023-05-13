using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ManajerLevel : MonoBehaviour
{
    public static ManajerLevel instance;
    public static ManajerLevel Instance
    {
        get
        {
            // Jika instance belum ada, cari di scene
            if (instance == null)
            {
                instance = FindObjectOfType<ManajerLevel>();
            }
            
            return instance;
        }
    }
    // konstanta A dan B
    private const float A = 5f;
    private const float B = 1.4f;
    
    int numLevel = 300;

    // variabel untuk menyimpan pengalaman yang dibutuhkan untuk mencapai tiap level
   [SerializeField] int[] expTable;

    // konstruktor
    public ManajerLevel()
    {
        expTable = new int[numLevel];

        // mengisi nilai pengalaman yang dibutuhkan untuk tiap level
        for (int i = 0; i < numLevel; i++)
        {
            expTable[i] = Mathf.RoundToInt(A * Mathf.Pow(i + 1, B));
        }
    }

    // fungsi untuk mengambil pengalaman yang dibutuhkan untuk mencapai level tertentu
    public float GetExpForLevel(int level)
    {
        if (level < 1 || level > numLevel)
        {
            throw new ArgumentOutOfRangeException("Level harus berada di antara 1 dan 100");
        }

        return expTable[level - 1];
    }
    public int GetLevelForExp(int exp)
    {
        int level = 1;

        // Memeriksa setiap level untuk melihat apakah experience point yang diberikan mencukupi untuk naik level
        while (level <= numLevel && exp >= expTable[level-1])
        {
            level++;
        }

        return level-1;
    }
}
