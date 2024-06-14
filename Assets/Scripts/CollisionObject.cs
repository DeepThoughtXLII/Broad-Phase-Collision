using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CollisionObject : MonoBehaviour
{

    private SpriteRenderer myRend;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color collisionColor;


    float fadeTime = 0;
    float fadeTotal = 1f;
    bool fading = false;

    Vector3 newPosition = Vector3.zero;

    [SerializeField] private Vector3 direction;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float speed = 1;

    private void Awake()
    {
        myRend = GetComponent<SpriteRenderer>();
        myRend.color = defaultColor;

        direction = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0).normalized;
        velocity = direction * speed;

        newPosition = transform.position;
    }

    public void Step(float time)
    {
        transform.position = newPosition;

        velocity = direction * speed;

        newPosition += velocity * time;
    }

    private void Update()
    {
        if (fading)
        {
            ColourChanging();
        }
    }


    #region getters / setters

    public float GetRadius()
    {
        return transform.localScale.x / 2;
    }

    public Vector2 GetPosition()
    {
        return new Vector2(newPosition.x, newPosition.y);
    }

    public void SetNewPosition(Vector2 newPosition)
    {
        this.newPosition = newPosition;
    }

    public Vector2 GetVelocity()
    {
        return velocity;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    #endregion

    public void ReflectDirection(Vector2 normal)
    {
        CollidingEffect();
        direction = Vector2.Reflect(direction, normal).normalized;
    }

    #region colour fade

    public void CollidingEffect()
    {
        fadeTime = 0;
        fading = true;
        myRend.color = collisionColor;
    }


    void ColourChanging()
    {
        if (fadeTime < fadeTotal)
        {
            fadeTime += Time.deltaTime;
            myRend.color = Color.Lerp(myRend.color, defaultColor, fadeTime / fadeTotal);
        }
        else
        {
            fading = false;
        }
    }

    #endregion



    private void OnDrawGizmos()
    {
        //Gizmos.DrawLine(transform.position, transform.position + velocity);
    }
}