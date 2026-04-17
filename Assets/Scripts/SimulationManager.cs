using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    public float secondsPerIteration = 0.5f;
    private float timer = 0f;

    [Header("Entidades de la Mazmorra")]
    public List<Adventurer> adventurers = new List<Adventurer>();
    public List<Monster> monsters = new List<Monster>();

    void Start()
    {
        adventurers = new List<Adventurer>(FindObjectsByType<Adventurer>(FindObjectsSortMode.None)); // Encuentra todos los aventureros en la escena y los agrega a la lista
        monsters = new List<Monster>(FindObjectsByType<Monster>(FindObjectsSortMode.None)); // Encuentra todos los monstruos en la escena y los agrega a la lista
    }

    void Update()
    {
        timer += Time.deltaTime; // Cuenta el tiempo de la simulación

        if (timer >= secondsPerIteration)
        {
            timer = 0f;
            Simulate(); //Simula los pasos
        }
    }

    void Simulate()
    {
        adventurers.RemoveAll(a => a == null || !a.isAlive); // Limpia las listas por si algún explorador estpa muerto
        monsters.RemoveAll(m => m == null || !m.isAlive); // Limpia las listas por si algún monstruo esta muerto

        foreach (Adventurer adv in adventurers) //Simula los aventureros
        {
            if (adv != null && adv.isAlive)
            {
                adv.Simulate(secondsPerIteration);
            }
        }

        foreach (Monster mon in monsters) //Simula los monstruos
        {
            if (mon != null && mon.isAlive)
            {
                mon.Simulate(secondsPerIteration);
            }
        }

    }
}