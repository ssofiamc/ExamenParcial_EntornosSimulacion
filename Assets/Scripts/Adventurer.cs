using UnityEngine;

public class Adventurer : MonoBehaviour
{
    [Header("Estadísticas del Aventurero")] 
    public float health = 20f; // Vida del aventurero
    public float attack = 5f; // Ataque del aventurero
    public float speed = 1.5f; // Velocidad del aventurero
    public float visionRange = 5f; // Rango de visión del aventurero

    [Header("Estado del Aventurero")]
    public bool isAlive = true; // Indica si el aventurero está vivo
    public AdventurerState currentState = AdventurerState.Exploring; // Estado actual del aventurero

    private Vector3 destination; // Destino del aventurero
    private float h;

}
