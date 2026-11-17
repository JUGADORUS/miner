using UnityEngine;

public class UIAnimation : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float amplitude = 15f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    internal void SetOrigin(Vector3 position)
    {
        startPosition = position;
        transform.localPosition = position;
    }

    private void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
