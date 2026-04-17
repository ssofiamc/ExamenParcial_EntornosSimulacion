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
    private float h; // Tiempo de la simulación

    private void Start()
    {
        destination = transform.position; // Inicializa el destino del aventurero
    }

    public void Simulate(float h)
    {
        if (!isAlive) return; // Si el aventurero está muerto, no hace nada

        this.h = h; // Actualiza el valor de h para la simulación

        EvaluateState(); // Evalúa el estado del aventurero

        switch (currentState) //Según el estado
        {
            case AdventurerState.Exploring: //Si está explorando
                Explore();
                break;
            case AdventurerState.SearchingChest: //Si está buscando un cofre
                EvaluateState();
                break;
            case AdventurerState.Fighting: //Si está peleando
                EvaluateState();
                break;
            case AdventurerState.Fleeing: //Si está huyendo
                EvaluateState();
                break;
        }
        Move(); // Se mueve el aventurero
    }

    void EvaluateState()
    {
        Monster nearestMonster = FindNearestMonster(); // Detecta el monstruo más cercano
        if (nearestMonster != null) // Si encuentra un monstruo
        {
            if (health < 5f) // Si la salud es menor a 5
            {
                currentState = AdventurerState.Fleeing; // El aventurero huye
                destination = transform.position + (transform.position - nearestMonster.transform.position).normalized * visionRange; // Calcula una ruta para huir del monstruo
            }
            else //Si no
            {
                currentState = AdventurerState.Fighting; // El aventurero pelea con el monstruo
                destination = nearestMonster.transform.position; // Entonces se acerca al monstruo
                if (Vector3.Distance(transform.position, destination) < 0.5f) // Si está cerca del monstruo
                {
                    nearestMonster.TakeDamage(attack * h); // Lo ataca y el daño depende del tiempo de ataque
                    destination = transform.position; // Se queda en su posición para seguir atacando
                }
            }
            return; // Y ya
        }

        Chest nearestChest = FindNearestChest(); // Busca el cofre más cercano
        if (nearestChest != null) // Si encuentra un cofre
        {
            currentState = AdventurerState.SearchingChest; // El aventurero busca el cofre
            destination = nearestChest.transform.position; // Se va al cofre
            if (Vector3.Distance(transform.position, destination) < 0.3f) // Si está cerca al cofre 
            {
                OpenChest(nearestChest); // Lo abre y este le aumenta las estadísticas
            }
            return;
        }
        currentState = AdventurerState.Exploring; // Si no hay nada pues sigue explorando
    }

    void Move()
    {
        Vector3 direction = (destination - transform.position).normalized; // Calcula la dirección hacia el destino
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, speed * Time.deltaTime, LayerMask.GetMask("Obstacles")); // Verifica si hay obstáculos en el camino

        if (hit.collider == null) // Si no hay obstáculos
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * h); //Entonces se mueve
        }
        else // Si hay un obstáculo
        {
            if (currentState == AdventurerState.Exploring)
            {
                SelectNewDestination(); // Va a otra parte
            }
        }
    }

    void Explore()
    {
        if (Vector3.Distance(transform.position, destination) < 0.5f) // Si termina su ruta
        {
            SelectNewDestination(); // Pues escoge otra más
        }
    }

    void OpenChest(Chest cofre)
    {
        if (cofre.isMimic) // Si el cofre es mimico
        {
            TakeDamage(10f); // Le hace daño
            if (cofre.monsterPrefab != null) Instantiate(cofre.monsterPrefab, cofre.transform.position, Quaternion.identity); // Saca el prefab del monstruo
        }
        else // Sino
        {
            health += cofre.healthBonus; // Le da salud
            attack += cofre.attackBonus; // Le da ataque
        }
        Destroy(cofre.gameObject); // Destruye el objeto
        FindObjectOfType<SimulationManager>().adventurers.RemoveAll(a => a == null); // y lo elimina de la lista de los cofres
    }

    public void TakeDamage(float damage) //Cuando es mímico y le hace daño
    {
        health -= damage; // Si la salud que tiene es poca
        if (health <= 0) { isAlive = false; gameObject.SetActive(false); } // Entonces lo mata
    }

    void SelectNewDestination() // Acá es que selecciona otra ruta al azar
    {
        Vector3 direction = new Vector3(
            Random.Range(-visionRange, visionRange),
            Random.Range(-visionRange, visionRange),
            0
        );

        Vector3 targetPoint = transform.position + direction;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction.normalized,
            visionRange,
            LayerMask.GetMask("Obstacles")
        );

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)direction.normalized * offset;
        }
        else
        {
            destination = targetPoint;
        }
    }

    Monster FindNearestMonster() //Encuentra al monstruo más cercano dentro de su rango de visión y sin obstáculos en el camino
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, visionRange, LayerMask.GetMask("Monsters")
            );

        Monster nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Monster m = hit.GetComponent<Monster>();
            if (m == null || !m.isAlive) continue;

            Vector3 dir = m.transform.position - transform.position;
            float dist = dir.magnitude;

            RaycastHit2D blockHit = Physics2D.Raycast(
                transform.position,
                dir.normalized,
                dist,
                LayerMask.GetMask("Obstacles")
            );
            if (blockHit.collider != null) continue;

            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = m;
            }
        }
        return nearest;
    }

    Chest FindNearestChest() // Encuentra el cofre más cercano dentro de su rango de visión
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, visionRange, LayerMask.GetMask("Chests")
            );

        Chest nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Chest c = hit.GetComponent<Chest>();
            if (c == null) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = c;
            }
        }
        return nearest;
    }

    private void OnDrawGizmosSelected() // Muestra los círculitos de lo que hace el aventurero
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination);
    }
}