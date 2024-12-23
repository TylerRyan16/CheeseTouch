using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroCutscene : MonoBehaviour
{
    private bool cutsceneActive = true;

    public GameObject greg;
    public GameObject darren;
    public Animator gregAnimator;
    public GameObject zombiePrefab;

    // GREG MOVING RIGHT & ZOOM OUT TO BASKETBALL COURT
    private Vector2 gregStartPos;
    private Vector2 gregEndPos;
    private Vector2 cameraStartPos;
    private Vector2 cameraEndPos;
    private float gregDuration = 5f;
    private float cameraDuration = 2f;
    private float elapsedTime = 0f;
    private float cameraSizeStart;
    private float cameraSizeEnd;
    private bool cameraActionStarted = false;

    // DARREN ENTERS
    private bool moveDarrenStarted = false;
    private float darrenElapsedTime = 0f;
    private float darrenMoveDuration = 3f;
    private Vector2 darrenStartPos;
    private Vector2 darrenEndPos;

    void Start()
    {
        if (greg != null && darren != null)
        {
            InitializePositions();
            gregAnimator = greg.GetComponent<Animator>();
            if (gregAnimator == null)
            {
                Debug.LogError("Animator not found");
                cutsceneActive = false;
            }
        }
        else
        {
            Debug.LogError("Greg or Darren game object not found");
            cutsceneActive = false;
        }
    }

    void Update()
    {
        if (cutsceneActive)
        {
            MoveGregRight();
        }
        else if (moveDarrenStarted)
        {
            MoveDarrenOntoScreen();
        }
    }

    private void InitializePositions()
    {
        // Initialize Greg's positions
        gregStartPos = greg.transform.position;
        gregEndPos = gregStartPos + new Vector2(45f, 0f);

        // Initialize camera positions and size
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraStartPos = mainCamera.transform.position;
            cameraEndPos = cameraStartPos + new Vector2(55f, 0f);
            cameraSizeStart = mainCamera.orthographicSize;
            cameraSizeEnd = cameraSizeStart + 3f;
        }
        else
        {
            Debug.LogError("Main Camera not found.");
            cutsceneActive = false;
        }

        // Initialize Darren's start and end positions
        darrenStartPos = darren.transform.position;
        darrenEndPos = darrenStartPos + new Vector2(-35f, 0f);
    }

    private void MoveGregRight()
    {
        elapsedTime += Time.deltaTime;

        // Start Greg's walk animation when moving
        if (gregAnimator != null && elapsedTime <= gregDuration)
        {
            gregAnimator.Play("GregWalk");
        }

        // Move Greg
        if (elapsedTime <= gregDuration)
        {
            float t = elapsedTime / gregDuration;
            greg.transform.position = Vector2.Lerp(gregStartPos, gregEndPos, t);
        }

        // Start moving the camera after 3 seconds have passed
        if (elapsedTime >= 3f && !cameraActionStarted)
        {
            cameraActionStarted = true;
        }

        // Move and resize the camera if the action has started
        if (cameraActionStarted && elapsedTime <= gregDuration)
        {
            float cameraElapsed = elapsedTime - 3f;
            float tCamera = cameraElapsed / cameraDuration;

            // Move and resize camera simultaneously
            Vector3 cameraPosition = Vector2.Lerp(cameraStartPos, cameraEndPos, tCamera);
            Camera.main.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, -10);
            Camera.main.orthographicSize = Mathf.Lerp(cameraSizeStart, cameraSizeEnd, tCamera);
        }

        // End Greg's movement and start Darren's movement
        if (elapsedTime >= gregDuration)
        {
            cutsceneActive = false;
            moveDarrenStarted = true;
            gregAnimator.Play("GregIdle"); // Switch to idle or stop walking
        }
    }

    private void MoveDarrenOntoScreen()
    {
        darrenElapsedTime += Time.deltaTime;

        // Move Darren from right to left over time
        float t = Mathf.Clamp01(darrenElapsedTime / darrenMoveDuration);
        darren.transform.position = Vector2.Lerp(darrenStartPos, darrenEndPos, t);

        // End Darren's movement after the duration
        if (t >= 1f)
        {
            moveDarrenStarted = false; // End this step

            // Get the sprite from the zombiePrefab's SpriteRenderer
            SpriteRenderer zombieSpriteRenderer = zombiePrefab.GetComponent<SpriteRenderer>();
            SpriteRenderer darrenSpriteRenderer = darren.GetComponent<SpriteRenderer>();

            if (zombieSpriteRenderer != null && darrenSpriteRenderer != null)
            {
                darrenSpriteRenderer.sprite = zombieSpriteRenderer.sprite; // Set Darren's sprite to the zombie sprite
                darren.transform.localScale *= 0.5f;
            }
            else
            {
                Debug.LogError("SpriteRenderer not found on zombiePrefab or Darren.");
            }

            // Start coroutine to move Darren back after 3 seconds
            StartCoroutine(ReturnDarrenToStart());
        }
    }

    private IEnumerator ReturnDarrenToStart()
    {
        
        // Wait 3 seconds before moving Darren back
        yield return new WaitForSeconds(3f);

        SpriteRenderer darrenSpriteRenderer = darren.GetComponent<SpriteRenderer>();
        darrenSpriteRenderer.flipX = true;

        float returnDuration = 2f; // Duration for Darren to move back
        float returnElapsedTime = 0f;

        while (returnElapsedTime < returnDuration)
        {
            returnElapsedTime += Time.deltaTime;
            float t = returnElapsedTime / returnDuration;
            darren.transform.position = Vector2.Lerp(darrenEndPos, darrenStartPos, t);
            yield return null;
        }

        // Ensure Darren is exactly at the start position after the movement
        darren.transform.position = darrenStartPos;

        SceneManager.LoadScene("CharacterSelect");
    }

}
