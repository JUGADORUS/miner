using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    public PickaxeData data;

    public int Durability;
    public int MaxDurability;
    public int Damage;
    public int RangeOfDigging;
    public int RewardMultiplyer;
    public SpriteRenderer Icon;

    [SerializeField] public float MinExplosionForce;
    [SerializeField] public float MaxExplosionForce;

    private Rigidbody2D rb;
    [SerializeField] private float maxRotationSpeed = 500f;
    [SerializeField] private Vector2 customCenterOfMass;

    private void FixedUpdate()
    {
        ControlAngularVelocity();
    }

    private void Awake()
    {
        MaxDurability = data.maxDurability;
        Durability = MaxDurability;
        Damage = data.damage;
        RangeOfDigging = data.rangeOfDigging;
        RewardMultiplyer = data.rewardMultiplyer;
        Icon.sprite = data.icon;

        rb = GetComponent<Rigidbody2D>();
        rb.centerOfMass = customCenterOfMass;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AddExplosionForce2D(collision);

        if (collision.gameObject.tag == "block")
        {
            Vector2 normal = collision.contacts[0].normal;
            Block block = collision.gameObject.GetComponent<Block>();
            Vector2 contactPoint = collision.contacts[0].point;

            GameEvents.OnHitBlock?.Invoke(block, Durability);
            GameEvents.OnHitBlockWithDirectionPosition?.Invoke(block, contactPoint, normal);
        }
    }

    public void AddExplosionForce2D(Collision2D collision)
    {
        Vector2 contactPoint = collision.contacts[0].point;
        rb.linearVelocity = Vector2.zero;
        float explosionForce = Random.Range(MinExplosionForce,MaxExplosionForce);
        Vector2 explosionDir = rb.position - contactPoint;
        explosionDir.Normalize();
        rb.AddForce(explosionDir * explosionForce, ForceMode2D.Impulse);
    }

    private void ControlAngularVelocity()
    {
        if (Mathf.Abs(rb.angularVelocity) > maxRotationSpeed)
        {
            rb.angularVelocity = Mathf.Sign(rb.angularVelocity) * maxRotationSpeed;
        }
    }
}
