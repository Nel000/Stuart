using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelNavigation : MonoBehaviour
{
    private const float duration = 1.5f;

    [SerializeField] private int currentPanel;
    [SerializeField] private int lastPanelPlayed;
    [SerializeField] private int nextPanel;
    [SerializeField] private int maxPanelReached;

    private bool isLocked;

    [SerializeField] private GameObject[] panels;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();

        maxPanelReached = PlayerPrefs.GetInt("MaxPanelReached");
        lastPanelPlayed = PlayerPrefs.GetInt("LastPanelPlayed");

        if (!PlayerPrefs.HasKey("CurrentPanel"))
        {
            currentPanel = 0;
            nextPanel = 1;
            PlayerPrefs.SetInt("CurrentPanel", 0);
            PlayerPrefs.SetInt("LastPanelPlayed", 0);
            PlayerPrefs.SetInt("IsLastPanelPlayed", 0);
            PlayerPrefs.SetInt("MaxPanelReached", 0);
        }
        else
        {
            currentPanel = PlayerPrefs.GetInt("CurrentPanel");
            
            nextPanel = maxPanelReached + 1;
            
            /*if (PlayerPrefs.GetInt("IsLastPanelPlayed") == 1)
            {
                
            }
            else if (PlayerPrefs.GetInt("IsLastPanelPlayed") == 0)
            {
                if (maxPanelReached >= lastPanelPlayed)
                {
                    nextPanel = maxPanelReached + 1;
                }
                else if (maxPanelReached < lastPanelPlayed)
                {
                    maxPanelReached--;
                    PlayerPrefs.SetInt("MaxPanelReached", maxPanelReached);
                    nextPanel = maxPanelReached + 1;
                }
            }*/

            // Modify image for all played panels
            if (currentPanel > 0)
            {
                for (int i = 0; i < maxPanelReached; i++)
                {
                    panels[i].GetComponent<SpriteRenderer>().color = Color.grey;
                }

                if (PlayerPrefs.GetInt("IsLastPanelPlayed") == 0
                    && maxPanelReached < currentPanel)
                {
                    panels[currentPanel - 1].GetComponent<SpriteRenderer>().
                        color = Color.white;
                }
            }
            
            if (nextPanel > panels.Length)
                nextPanel = panels.Length;

            if (currentPanel > 0)
            {
                cam.orthographicSize = 2.1f;
                transform.position = new Vector3(
                    panels[currentPanel - 1].transform.position.x,
                    panels[currentPanel - 1].transform.position.y, -10);
            }
        }
    }

    private void Update()
    {
        if (Input.GetButtonUp("Select") && currentPanel == 0 && !isLocked)
        {
            Debug.Log("Move to first panel");
            isLocked = true;
            StartCoroutine(Move(0));
            currentPanel = 1;
            PlayerPrefs.SetInt("CurrentPanel", currentPanel);
        }

        if (Input.GetAxis("Horizontal") > 0 && currentPanel > 0
            && currentPanel < nextPanel && !isLocked)
        {
            Debug.Log("Move to next panel");
            isLocked = true;
            StartCoroutine(Move(PlayerPrefs.GetInt("CurrentPanel")));
            ++currentPanel;
            PlayerPrefs.SetInt("CurrentPanel", currentPanel);
        }

        if (Input.GetAxis("Horizontal") < 0 && currentPanel > 1
            && currentPanel <= nextPanel && !isLocked)
        {
            Debug.Log("Move to previous panel");
            isLocked = true;
            --currentPanel;
            PlayerPrefs.SetInt("CurrentPanel", currentPanel);
            StartCoroutine(Move(currentPanel - 1));
        }

        if (Input.GetButtonDown("Select") && currentPanel > 0 && !isLocked)
        {
            PlayerPrefs.SetInt("LastPanelPlayed", currentPanel);
            PlayerPrefs.SetInt("IsLastPanelPlayed", 0);
            SceneManager.LoadScene(currentPanel);
        }
    }

    private IEnumerator Move(int index)
    {
        float timeElapsed = 0;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = panels[index].transform.position;

        do
        {
            if (cam.orthographicSize > 2.1f)
                cam.orthographicSize = 
                    cam.orthographicSize - 2f * Time.deltaTime;
            else
                cam.orthographicSize = 2.1f;
            transform.position = Vector3.Lerp(
                startPosition, new Vector3(targetPosition.x, targetPosition.y, -10), 
                timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        while (timeElapsed < duration);

        transform.position = new Vector3(targetPosition.x, targetPosition.y, -10);
        isLocked = false;
    }
}
