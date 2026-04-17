using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("Estadísticas del Monstruo")]
    public float health = 15f; // Vida del monstruo
    public float attack = 4f; // Ataque del monstruo
    public float speed = 1.2f; // Velocidad del monstruo
    public float visionRange = 4f; // Rango de visión del monstruo

    [Header("Estado")]
    public bool isAlive = true; // Indica si el monstruo está vivo
    public MonsterState currentState = MonsterState.Patrolling; // Estado actual del monstruo

    private Vector3 patrolCenter; // Lo que manda a patrullar
    private Vector3 destination; // Destino del monstruo
    private float h; // Tiempo de la simulación 
    private Adventurer currentTarget; // Para guardar a quién está persiguiendo

    void Start()
    {
        patrolCenter = transform.position; // Por donde va a estar patrullando
        destination = patrolCenter; // Se basa en su posición actual
    }

    public void Simulate(float h)
    {
        if (!isAlive) return; // Si el monstruo está muerto, no hace nada

        this.h = h; // Actualiza el valor de h para la simulación

        EvaluateState(); // Evalúa el estado del monstruo

        switch (currentState) // Según el estado
        {
            case MonsterState.Patrolling: // Si está patrullando
                Patrol();
                break;
            case MonsterState.Chasing: // Si está persiguiendo
                EvaluateState(); 
                break;
            case MonsterState.Attacking: // Si está atacando
                Attack();
                break;
        }

        Move(); // Se mueve el monstruo
    }

    void EvaluateState()
    {
        currentTarget = FindNearestAdventurer(); // Busca al aventurero más cercano

        if (currentTarget != null) // Si encuentra a un aventurero
        {
            destination = currentTarget.transform.position; // Se va hacia dónde está el aventurero
            float distance = Vector3.Distance(transform.position, destination); // Calcula la distancia al aventurero

            if (distance < 0.7f) // Si está cerca, ataca
            {
                currentState = MonsterState.Attacking; // Cambia el estado a atacando
            }
            else // Sino
            {
                currentState = MonsterState.Chasing; // Cambia el estado a persiguiendo
            }
        }
        else // Si no hay nada pues patrulla
        {
            currentState = MonsterState.Patrolling; // Cambia el estado a patrullando
        }
    }

    void Patrol() // Cuando patrulla
    {
        if (Vector3.Distance(transform.position, destination) < 0.5f) // Si llega al lugar que quería y pues no hay nada
        {
            destination = patrolCenter + (Vector3)Random.insideUnitCircle * 3f; // Escoge otro lugar al cual ir
        }
    }

    void Attack() 
    {
        if (currentTarget != null) // Si está el aventurero cerca
        {
            currentTarget.TakeDamage(attack * h); // Le hace daño al aventurero y pues este depende del tiempo que lo ataque
            destination = transform.position; // Se queda ahí mismo
        }
    }

    void Move()
    {
        Vector3 direction = (destination - transform.position).normalized; // Calcula la dirección hacia el destino
        float step = speed * h; // Mientras patrulla es como si estuviera caminando normal

        if (currentState == MonsterState.Patrolling) step *= 0.5f; 

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.7f, LayerMask.GetMask("Obstacles")); // Verifica si hay obstáculos en el camino

        if (hit.collider == null) // Si no hay nada
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, step); // Le da palante
        }
        else // Si no
        {
            if (currentState == MonsterState.Patrolling) destination = transform.position; // Cambia su destino
        }
    }

    public void TakeDamage(float damage) // El daño que recibe el aventurero
    {
        health -= damage; // Le afecta la salud 
        if (health <= 0) // Si tiene bajita la salud
        {
            isAlive = false; // Muere el monstruo
            gameObject.SetActive(false); // Desaparece de la escena
        }
    }

    Adventurer FindNearestAdventurer() //Encuentra al aventurero más cercano dentro de su rango de visión
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Adventurers"));
        Adventurer nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Adventurer adv = hit.GetComponent<Adventurer>();
            if (adv == null || !adv.isAlive) continue;

            // Verificamos línea de visión (que no haya paredes de por medio)
            Vector3 dir = adv.transform.position - transform.position;
            RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, LayerMask.GetMask("Obstacles"));

            if (wallCheck.collider == null) // Si el camino está limpio
            {
                float dist = dir.magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = adv;
                }
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
