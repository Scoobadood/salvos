using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Per https://www.youtube.com/watch?v=64uOVmQ5R1k
/// This is a Scene loading state machine
/// States:
/// 	Reset (garbage collect)
/// 	Preload (starts loading level as asynch)
/// 	Load (load the game scene)
/// 	Unloading (clean up unused resrources)
/// 	Post load state (run processes, update current scene name)
/// 	Ready State (last chance to run gc.collect, Get user input to start)
/// 	Run State
/// </summary>
public class MainController : MonoBehaviour {

	/// <summary>
	/// Static instance
	/// </summary>
	private static MainController mainController;

	/// <summary>
	/// The name of the current scene.
	/// </summary>
	private string currentSceneName;

	/// <summary>
	/// The name of the next scene.
	/// </summary>
	private string nextSceneName;

	/// <summary>
	/// The resource unload task.
	/// </summary>
	private AsyncOperation resourceUnloadTask;

	/// <summary>
	/// The scene load task.
	/// </summary>
	private AsyncOperation sceneLoadTask;

	/// <summary>
	/// The state of the scene.
	/// </summary>
	private SceneState sceneState;

	/// <summary>
	/// Definition of UpdateDelegate delegate
	/// </summary>
	private delegate void UpdateDelegate( );

	/// <summary>
	/// An array of delegates
	/// </summary>
	private UpdateDelegate[] updateDelegates;


	/// <summary>
	/// Tee up a scene change if the desired scene is different from the current scene
	/// Called from the MenuController and the GameController
	/// </summary>
	/// <param name="nextSceneName">Next scene name.</param>
	public static void SwitchScenes( string nextSceneName ) {
		if (mainController != null) {
			if (mainController.currentSceneName != nextSceneName) {
				mainController.nextSceneName = nextSceneName;
			}
		}
	}
				


	// Use this for initialization
	protected void Awake () {
		// Keep alive between Scene changes
		Object.DontDestroyOnLoad( gameObject );

		// Make singleton
		mainController = this;

		// Setup the UpdateDelegates
		updateDelegates = new UpdateDelegate[ (int) SceneState.Count ];
		updateDelegates[ (int) SceneState.Reset] = UpdateSceneReset;
		updateDelegates[ (int) SceneState.Preload] = UpdateScenePreload;
		updateDelegates[ (int) SceneState.Load] = UpdateSceneLoad;
		updateDelegates[ (int) SceneState.Unload] = UpdateSceneUnload;
		updateDelegates[ (int) SceneState.Postload] = UpdateScenePostload;
		updateDelegates[ (int) SceneState.Ready] = UpdateSceneReady;
		updateDelegates[ (int) SceneState.Run] = UpdateSceneRun;

		// Main menu first
		nextSceneName = "Menu Scene";

		// And reset
		sceneState = SceneState.Reset;
	}

	/// <summary>
	/// Handle the reset pass by collecting garbage
	/// </summary>
	void UpdateSceneReset( ) {
		System.GC.Collect ();

		// And transition
		sceneState = SceneState.Preload;
	}


	/// <summary>
	/// Handle the preload state by starting an async task to load the state
	/// </summary>
	void UpdateScenePreload( ) {
		sceneLoadTask = SceneManager.LoadSceneAsync (nextSceneName);
		sceneState = SceneState.Load;
	}

	/// <summary>
	/// Check for loaded and when done, transition to unload
	/// </summary>
	void UpdateSceneLoad( ) {
		if (sceneLoadTask.isDone) {
			sceneState = SceneState.Unload;
		} else {
			// update scene loading progress
		}
	}


	/// <summary>
	/// Unload unused resources
	/// </summary>
	void UpdateSceneUnload( ) {
		if (resourceUnloadTask == null) {
			resourceUnloadTask = Resources.UnloadUnusedAssets ();
		} else {
			if (resourceUnloadTask.isDone) {
				resourceUnloadTask = null;
				sceneState = SceneState.Postload;
			}
		}	
	}


	/// <summary>
	/// Make this the current scene
	/// </summary>
	void UpdateScenePostload( ) {
		currentSceneName = nextSceneName;
		sceneState = SceneState.Ready;
	}


	/// <summary>
	/// Prepare for running. Handle anything that needs to happen here.
	/// Also display 'click to contniue' or similar
	/// </summary>
	void UpdateSceneReady( ) {
		// GC but only if there are not assets currently 
		// unused but needed later
		System.GC.Collect ();
		sceneState = SceneState.Run;
	}

	void UpdateSceneRun( ) {
		// Do run behaviour for the scene
		if (nextSceneName != currentSceneName) {
			sceneState = SceneState.Reset;
		}
	}


	/// <summary>
	/// Call the appropriate delegate for the state machine
	/// </summary>
	void Update () {
		if (updateDelegates [(int)sceneState] != null) {
			updateDelegates [(int)sceneState] ();
		}
	}


	protected void OnDestroy( ) {
		// Tidy up delegates
		if (updateDelegates != null) {
			for (int i = 0; i < (int)SceneState.Count; i++) {
				updateDelegates [i] = null;
			}
			updateDelegates = null;
		}

		// Tidy up the singleton too
		if (mainController != null) {
			mainController = null;
		}

	}

	/// <summary>
	/// An enumeration of states to manage the state machine
	/// </summary>
	private enum SceneState { Reset, Preload, Load, Unload, Postload, Ready, Run, Count };
}
