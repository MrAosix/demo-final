using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Snake snake1;
    public Snake2 snake2;
    public Snake3 snake3;
    public Snake4 snake4;
    public Snake5 snake5;
    public Snake6 snake6;
    public TextMeshProUGUI victoryText;

    private Coroutine winCoroutine;
    private bool snake1Won = false;
    private bool snake2Won = false;
    private bool snake3Won = false;
    private bool snake4Won = false;
    private bool snake5Won = false;
    private bool snake6Won = false;

    public void OnSnakeHitObstacle() {
        snake1Won = false;
        snake2Won = false;
        snake3Won = false;
        snake4Won = false;
        snake5Won = false;
        snake6Won = false;
        snake1.ResetState();
        snake2.ResetState();
        snake3.ResetState();
        snake4.ResetState();
        snake5.ResetState();
        snake6.ResetState();
    }

    public void OnSnakeHitVictory(GameObject snake) {
        if (snake == snake1.gameObject && !snake1Won) {
            snake1Won = true;
            snake1.canMove = false;
        } else if (snake == snake2.gameObject && !snake2Won) {
            snake2Won = true;
            snake2.canMove = false;
        }
        else if (snake == snake3.gameObject && !snake3Won)
        {
            snake3Won = true;
            snake3.canMove = false;
        }
        else if (snake == snake4.gameObject && !snake4Won)
        {
            snake4Won = true;
            snake4.canMove = false;
        }
        else if (snake == snake5.gameObject && !snake5Won)
        {
            snake5Won = true;
            snake5.canMove = false;
        }
        else if (snake == snake6.gameObject && !snake6Won)
        {
            snake6Won = true;
            snake6.canMove = false;
        }

        if (snake1Won && snake2Won && snake3Won && snake4Won && snake5Won && snake6Won)
        {
            Victory();
        }
    }

    private void Victory() {
        victoryText.gameObject.SetActive(true);
        snake1.ResetState();
        snake2.ResetState();
        snake3.ResetState();
        snake4.ResetState();
        snake5.ResetState();
        snake6.ResetState();
        snake1Won = false;
        snake2Won = false;
        snake3Won = false;
        snake4Won = false;
        snake5Won = false;
        snake6Won = false;
        winCoroutine = StartCoroutine(WinDisplayRoutine());
    }

    private IEnumerator WinDisplayRoutine() {
        yield return new WaitForSeconds(3f);
        victoryText.gameObject.SetActive(false);
    }
}
