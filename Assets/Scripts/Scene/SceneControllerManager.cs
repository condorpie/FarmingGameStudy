using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneControllerManager : SingletonMonobehaviour<SceneControllerManager>
{
    private bool isFading;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup faderCanvasGroup = null;
    [SerializeField] private Image faderImage = null;
    public SceneName startingSceneName;


    private IEnumerator Fade(float finalAlpha)
    {
        isFading = true;

        //sets so player cant click screen while fading
        faderCanvasGroup.blocksRaycasts = true;

        //calculates speed of fade based on alhpa
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;

        //while the canvasgroup hasnt reached final alhpa
        while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);

            //waits for frame then continues
            yield return null;
        }

        //Set the flag to flase when fade finishes
        isFading = false;

        //Stop the Canvas group from blocking raycasts
        faderCanvasGroup.blocksRaycasts = false;

    }


    private IEnumerator FadeAndSwitchScenes(string sceneName, Vector3 spawnPosition)
    {
        //Call before unload a fade out event
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();

        //Start fading to blakc and finish before counting
        yield return StartCoroutine(Fade(1f));

        //Store scene data
        SaveLoadManager.Instance.StoreCurrentSceneData();

        //Set Player Position
        Player.Instance.gameObject.transform.position = spawnPosition;

        //Call before scene unload event
        EventHandler.CallBeforeSceneUnloadEvent();

        //Unload the current active scene
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        //Start Loading the given scene and wait for it to finish
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        //Call after scene load event
        EventHandler.CallAfterSceneLoadEvent();

        //Restore new scene data
        SaveLoadManager.Instance.RestoreCurrentSceneData();

        //Start fading back in and wait for it to finish before exiting the function
        yield return StartCoroutine(Fade(0f));

        //Call after scene load and fade in event
        EventHandler.CallAfterSceneLoadFadeInEvent();
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        // Allow the given scene to load over several frams and add it to already laoding scenes (mainly the persistant scene)
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        //Find the scene that was most recently  loaded (one at last index)
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        //Set the newly loaded scene as the active scene (marks to be unloaded next)
        SceneManager.SetActiveScene(newlyLoadedScene);

    }

    /// <summary>
    /// START OF GAME SCENE LOADING
    /// </summary>
    /// <returns></returns>
    private IEnumerator Start()
    {
        //Set the initial black screen fade when game starts
        faderImage.color = new Color(0f, 0f, 0f, 1f);
        faderCanvasGroup.alpha = 1f;

        //start the first scene loading and wait for it to finish.
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName.ToString()));

        // subscribe to event
        EventHandler.CallAfterSceneLoadEvent();

        SaveLoadManager.Instance.RestoreCurrentSceneData();

        //once scene is loaded fade in
        StartCoroutine(Fade(0f));
    }

    //This is the main point of contact and influnece from the rest of the project
    //this will be called when the player wants to change scenes

    public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
    {
        //if a fade isnt happening then start fading and switch scenes
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneName, spawnPosition));
        }
    }


    
}
