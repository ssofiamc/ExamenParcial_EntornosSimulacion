using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Tipo de Cofre")] // Si es normal o mímico
    public bool isMimic = false;

    [Header("Recompensas (Cofre Normal)")] // Las estadísticas que le aumenta al explorador
    public float healthBonus = 5f;
    public float attackBonus = 2f;

    [Header("Configuración del Mímico")] // Acá se configura el prefab del monstruo que va a salir
    public GameObject monsterPrefab;
}
