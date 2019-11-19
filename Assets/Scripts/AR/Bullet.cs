using Gameboard;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject m_HitEffect;
	public float m_Speed;

    float m_distance;
	Vector3 m_startPosition;
    bool m_exploded;

	void Start()
    {
        m_startPosition = transform.position;
	}

    public Entity spawningEntity
    {
        get;
        set;
    }

    // Update is called once per frame
    void Update()
    {
		transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, m_startPosition) >= m_distance)
		{
            Explode();
        }
    }

    void Explode()
    {
        if (!m_exploded)
        {
            // explicitly set the parent in case the world root is a game object
            Instantiate(m_HitEffect, transform.position, Quaternion.identity, transform.parent);
            Destroy(gameObject);
            m_exploded = true;
        }
    }

	public void SetFlyDis(float dis)
	{
		m_distance = dis;
	}

    void OnTriggerEnter(Collider other)
    {
        Explode();

        if (spawningEntity)
        {
            var handlers = spawningEntity.GetComponents<IProjectileHitHandler>();
            foreach (var handler in handlers)
            {
                handler.OnHit(other.gameObject);
            }
        }
    }
}
