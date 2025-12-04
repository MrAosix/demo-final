using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Snake6 : MonoBehaviour
{
    private Vector2 direction = Vector2.zero;
    private List<Transform> segments;
    private Coroutine growCoroutine;
    private bool isMoving = false;

    public bool canMove = true;
    public GameManager gameManager;
    public Transform segmentPrefab;
    public float speed = 1f;

    private void Start()
    {
        segments = new List<Transform>();
        segments.Add(this.transform);

        growCoroutine = StartCoroutine(Growing());
    }

    public void SetDirection(string dir)
    {
        switch (dir)
        {
            case "up":
                direction = Vector2.up;
                isMoving = true;
                break;
            case "down":
                direction = Vector2.down;
                isMoving = true;
                break;
            case "left":
                direction = Vector2.left;
                isMoving = true;
                break;
            case "right":
                direction = Vector2.right;
                isMoving = true;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            this.transform.position = new Vector3(
                this.transform.position.x + direction.x * speed,
                this.transform.position.y + direction.y * speed,
                0.0f
            );
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            this.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

    }

    private IEnumerator Growing()
    {
        var interval = new WaitForSeconds(0.04f);
        while (true)
        {
            if (isMoving && canMove)
            {
                Grow();
            }
            yield return interval;
        }
    }

    private void Grow()
    {
        Transform segment = Instantiate(this.segmentPrefab);
        segment.position = this.transform.position;
        segments.Add(segment);
    }

    public void ResetState()
    {
        for (int i = 1; i < segments.Count; i++)
        {
            Destroy(segments[i].gameObject);
        }

        segments.Clear();
        segments.Add(this.transform);
        this.transform.position = new Vector3(-9.79f, -2.58f, 0);
        canMove = true;
        direction = Vector2.zero;
        isMoving = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            gameManager.OnSnakeHitObstacle();
        }
        else if (other.CompareTag("Victory"))
        {
            gameManager.OnSnakeHitVictory(this.gameObject);
        }
    }
}
