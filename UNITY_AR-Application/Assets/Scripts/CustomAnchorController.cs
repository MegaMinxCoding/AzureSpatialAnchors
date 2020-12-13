using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Diagnostics;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEngine.EventSystems;
using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(SharingService))]
public class CustomAnchorController : MonoBehaviour
{
    #region Variables Inspector
    [Header("Gameobjects")]
    [SerializeField]
    [Tooltip("The interaction gameobject to give the user a backend response.")]
    public GameObject userFeedback;
    [SerializeField]
    [Tooltip("The prefab which is getting displayed on the position of an Anchor.")]
    public GameObject anchorVisualizerPrefab;
    [SerializeField] //located in the the using Microsoft.Azure.SpatialAnchors.Unity;
    [Tooltip("Just drag the Gameobject with the anchor manager on it")]
    public SpatialAnchorManager cloudManager;
    [SerializeField] //located in the the using Microsoft.Azure.SpatialAnchors.Unity;
    [Tooltip("Determines when the anchor is expiring from the Azure account. After this time its lost.")]
    public int dayOffsetToExpiration;



    [Header("UI Elements")]
    [SerializeField]
    [Tooltip("button bottom right")]
    public GameObject buttonPlaceNewAnchor;
    [SerializeField]
    [Tooltip("button bottom left")]
    public GameObject buttonLoadASA;
    [Tooltip("button bottom middle, not visible at the beginning")]
    public GameObject buttonSaveAnchor;
    [Tooltip("button bottom middle, not visible at the beginning")]
    public GameObject buttonResetSession;

    #endregion

    #region Global Declarations
    // Is the instance of the given prefab (look at inspector)
    private GameObject prefabInstance;

    // Is needed to perform the virtual Raycast from the touch to the mesh
    private ARRaycastManager aRRaycastManager;

    // Is able to show all the feature points getting processed by ASA SDK
    private ARPointCloudManager pointCloudManager;

    // Is responsible for logging all the debug informations
    private LoggerScript logger;

    // if this is false, the touch input is not getting recognized
    bool isReadyForInput = false;

    // if this is enabled the sessionUpdated event keeps logging the spatial data capture progress
    bool waitingForMoreSpatialData = false;

    
    #endregion


    /// <summary>
    /// Update is called every frame of the running application.
    /// </summary>
    private void Update()
    {
        inputToAnchor();
    }

 /// <summary>
    /// Start is called before the first frame update; 
    /// </summary>
    void Start()
    {
        initLogger();
        initARManagerScripts();
        initAzureSpatialAnchorManager();
        defaultUI();
    }

    #region Initialize on Start()

   

    /// <summary>
    /// Initializes the logger. The App wont start without it. 
    /// !! call it as the first init, because other methods rely on it.
    /// </summary>
    private void initLogger()
    {
        logger = userFeedback.GetComponent<LoggerScript>();
        if (logger == null)
        {
            throw new NotImplementedException();
        }
    }

   /// <summary>
   /// Initializes the aRRaycastManager and the pointCloudManager
   /// </summary>
    private void initARManagerScripts()
    {
        aRRaycastManager = FindObjectOfType<ARRaycastManager>();
        pointCloudManager = FindObjectOfType<ARPointCloudManager>();
        if (aRRaycastManager == null || pointCloudManager == null)
        {
            logger.Log("Error in the CustomAnchorManager.initGlobals(), pointCloudManager and RayCastmanager should not be null!!", TextState.ERROR);
        }
        logger.Log("Globals initialized.");
    }

    /// <summary>
    /// inits all variables and eventhandler provided by the SpatialAnchorManager --> you need to call it in the start()
    /// </summary>
    private void initAzureSpatialAnchorManager()
    {
        if (cloudManager == null)
        {
            logger.Log("No instance of the anchor manager availiable. Check the reference.", TextState.ERROR);
            Destroy(this);
        }
        if (!areCredentialsSet())
        {
            logger.Log("Credentials are empty. Fill them before building the app!", TextState.ERROR);
        }
        if (anchorVisualizerPrefab == null)
        {
            logger.Log("No prefab linked to show the local anchor.", TextState.ERROR);
        }

        subscribeCloudManagerToEvents();

        logger.Log("ASA-Session succsessfull initialized.");
    }
    #endregion

    #region UI Button Actions

    /// <summary>
    /// Method gets called when the UI button is clicked. It activates the input recognition
    /// </summary>
    public void buttonClick_createNewAnchor()
    {
        saveAnchorUI();
        // show the user the feature points that are getting found in the scene
        activatePointCloud();
        activateInputRecognizing();
        logger.Log("Tap somewhere to create a new Anchor...", TextState.WARNING);
    }

    /// <summary>
    /// Method gets called when the UI button is clicked. It executes the Task <see cref="saveNewSpatialAnchorAtPrefabPose"/>
    /// </summary>
    public async void buttonClick_saveAnchor()
    {

        deactivateInputRecognizing();
        logger.Log("Bis dahin auch");
        await saveNewSpatialAnchorAtPrefabPose();
        deactivatePointCloud();
    }

   
    /// <summary>
    /// Method gets called when the UI button is clicked. It executes the Task <see cref="loadAnchorAsync"/>
    /// </summary>
    public async void buttonClick_LoadAnchor()
    {
        await loadAnchorAsync();
    }

    /// <summary>
    /// Method gets called when the UI button is clicked. It resets the session.
    /// </summary>
    public void buttonClick_resetSession()
    {
        ResetManager();
    }


    #endregion

    #region UserInterface Control
    /// <summary>
    /// Default UI means the left and right button is active, middle not.
    /// </summary>
    private void defaultUI()
    {
        buttonPlaceNewAnchor.SetActive(true);
        buttonLoadASA.SetActive(true);
        buttonSaveAnchor.SetActive(false);
        buttonResetSession.SetActive(false);

    }
    /// <summary>
    /// Default UI means the left and right button is disabled, middle not.
    /// </summary>
    private void saveAnchorUI()
    {
        buttonPlaceNewAnchor.SetActive(false);
        buttonLoadASA.SetActive(false);
        buttonSaveAnchor.SetActive(true);
        buttonResetSession.SetActive(false);
    }

    /// <summary>
    /// All UI elements (except the <see cref="logger"/>) are disabled.
    /// </summary>
    private void disableUI()
    {
        buttonPlaceNewAnchor.SetActive(false);
        buttonLoadASA.SetActive(false);
        buttonSaveAnchor.SetActive(false);
        buttonResetSession.SetActive(false);
    }

    private void resetUI()
    {
        buttonPlaceNewAnchor.SetActive(false);
        buttonLoadASA.SetActive(false);
        buttonSaveAnchor.SetActive(false);
        buttonResetSession.SetActive(true);
    }

 

    #endregion

    #region Eventhandler of the CloudManager

    /// <summary>
    /// This method is called when the session (property in the cloud manager is started) 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CloudManager_SessionStarted(object sender, EventArgs e)
    {
        logger.Log("Anchor session in the cloudmanager started.");
        defaultUI();
    }

    /// <summary>
    /// This method is called when the cloud manager detects an error.
    /// Usually it doesnt works as expected. Rely on the LogDebug instead and setup the loglevel to Warning or Error. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args">contains the error msg</param>
    private void CloudManager_Error(object sender, SessionErrorEventArgs args)
    {
        logger.Log("An Error occured: " + args.ErrorMessage, TextState.ERROR);
    }

    /// <summary>
    /// This method is called when the <see cref="SpatialAnchorManager"/> makes a debug. 
    /// You can change the loglevel of the SpatialAnchorManager in the unity inspector to determine the amount of messages. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args">contains the debug msg</param>
    private void CloudManager_LogDebug(object sender, OnLogDebugEventArgs args)
    {
        logger.Log($"ASA: {args.Message} ");
    }
    /// <summary>
    /// This method is called when all anchors (<see cref="AnchorLocateCriteria"/>)are located that you gave the watcher. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args">cointains the watcher reference or the bool if the location process was cancelled.</param>
    private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
    {
        
        //implement this when you watch for multiple anchors. 
        if (args.Cancelled) logger.Log("The watcher process was cancelled and finished not succsessful.", TextState.ERROR);
    }

    /// <summary>
    /// This method is called after any anchor given to the <see cref="CloudSpatialAnchorWatcher"/> of the session is located. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        logger.Log(String.Format("Anchor recognized as a possible anchor {0} {1}", args.Identifier, args.Status));
        if (args.Status == LocateAnchorStatus.Located)
        {
            OnCloudAnchorLocated(args.Anchor);
        }
    }

    /// <summary>
    /// This method is called when the AnchorLocatedEvent is fired. <see cref="CloudManager_AnchorLocated(object, AnchorLocatedEventArgs)"/>
    /// </summary>
    /// <param name="newAnchorFromAzure"></param>
    private void OnCloudAnchorLocated(CloudSpatialAnchor newAnchorFromAzure)
    {
        Pose anchorPose = newAnchorFromAzure.GetPose();
        logger.Log($"Anchor from Cloud Pose: {anchorPose}");
        createOrUpdatePrefabInstance(anchorPose);
        resetUI();
    }

    /// <summary>
    /// This is called when the session parameter (isReadyForCreate, RecommendedForCreateProgress, UserFeedback) changes 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void CloudManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
    {
        if (waitingForMoreSpatialData)
        {
            float createProgress = args.Status.RecommendedForCreateProgress;
            logger.Log($"Progress {createProgress:0%}");
        }
        
    }
    #endregion

    #region Private helperMethods

    /// <summary>
    /// Subscribes the <see cref="CustomAnchorController"/> to all needed events.
    /// Thats espeacially necesarry when you destroy the session in the <see cref="ResetManager"/>.
    /// </summary>
    private void subscribeCloudManagerToEvents()
    {
        cloudManager.SessionUpdated += CloudManager_SessionUpdated;
        cloudManager.AnchorLocated += CloudManager_AnchorLocated;
        cloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
        cloudManager.LogDebug += CloudManager_LogDebug;
        cloudManager.Error += CloudManager_Error;
        cloudManager.SessionStarted += CloudManager_SessionStarted;
    }


    /// <summary>
    /// Checks if the <see cref="cloudManager"/> has all the informations he needs to connect to the Azure service.
    /// !!!NOTE that it doesn't checks whether the credentials are actually valid!!!
    /// </summary>
    /// <returns>a boolean </returns>
    private bool areCredentialsSet()
    {
        if (string.IsNullOrWhiteSpace(cloudManager.SpatialAnchorsAccountId)
                || string.IsNullOrWhiteSpace(cloudManager.SpatialAnchorsAccountKey)
                || string.IsNullOrWhiteSpace(cloudManager.SpatialAnchorsAccountDomain))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// If the session is <see cref="isReadyForInput"/> this method checks every frame (<see cref="Update"/>) for a new touch input.
    /// As soon as there is a touch recognized the <see cref="prefabInstance"/> gets posed at the <see cref="RaycastHit"/>. 
    /// </summary>
    private void inputToAnchor()
    {
        if (isReadyForInput)
        {
            const TrackableType trackableTypes = TrackableType.FeaturePoint | TrackableType.PlaneWithinPolygon;

            if (Input.touchCount == 0) return;


            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;
            if (isTouchOverUIElement(touch)) return;


            var hits = new List<ARRaycastHit>();
            aRRaycastManager.Raycast(touch.position, hits, trackableTypes);
            bool inputFound = hits.Count > 0;

            if (inputFound)
            {
                createOrUpdatePrefabInstance(hits[0].pose);
                return;
            }
        }
    }

    /// <summary>
    /// Checks if the users touch is on the Button. If so, then the the result is true.
    /// </summary>
    /// <param name="touch">A touch input done by the User</param>
    /// <returns></returns>
    private bool isTouchOverUIElement(Touch touch)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return false;

        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(touch.position.x, touch.position.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);
        return results.Count > 0;
    }

    /// <summary>
    /// Updates the pose of the <see cref="prefabInstance"/>.
    /// </summary>
    /// <param name="newPose"></param>
    private void createOrUpdatePrefabInstance(in Pose newPose)
    {
       
        if (prefabInstance == null) {
            prefabInstance = Instantiate(anchorVisualizerPrefab, newPose.position, newPose.rotation);
        }
        else
        {
            prefabInstance.transform.position = newPose.position;
            prefabInstance.transform.rotation = newPose.rotation;
        }
    }
    /// <summary>
    /// Activates the input "listener". For more informations check out the: <see cref="inputToAnchor"/>
    /// </summary>
    private void activateInputRecognizing()
    {
        isReadyForInput = true;
    }
    /// <summary>
    /// Deactivates the input "listener". For more informations check out the: <see cref="inputToAnchor"/>
    /// </summary>
    private void deactivateInputRecognizing()
    {
        isReadyForInput = false;
    }

    private void activatePointCloud()
    {
        pointCloudManager.SetTrackablesActive(true);
    }
    private void deactivatePointCloud()
    {
        pointCloudManager.SetTrackablesActive(false);
    }

    /// <summary>
    /// Resets the whole session and every global variable related to this session.
    /// </summary>
    private void ResetManager()
    {
        cloudManager.DestroySession();
        Destroy(prefabInstance);
        prefabInstance = null;
        deactivateInputRecognizing();
        deactivatePointCloud();
        logger.Log("Session reset done.", TextState.SUCCSESS);
        defaultUI();
    }


    #endregion

    #region AZURE SESSION RELATED METHODS
    /// <summary>
    /// This method creates an anchor 
    /// </summary>
    /// <returns>an awaitable Task</returns>
    private async Task saveNewSpatialAnchorAtPrefabPose()
    {
        //at first start the session
        await startSessionAsync();
        disableUI();
        // get the CloudNative Anchor as a component of a gameobject 
        CloudNativeAnchor cna = prefabInstance.GetComponent<CloudNativeAnchor>();
        if (!cna)
        {
            logger.Log("No CNA existing. Creating a new one.");
            cna = prefabInstance.AddComponent<CloudNativeAnchor>();
        }
        // If the cloud portion of the anchor hasn't been created yet, create it
        if (cna.CloudAnchor == null)
        {
            cna.NativeToCloud();
        }
        // Get the cloud portion of the anchor
        var cloudAnchor = cna.CloudAnchor;
        // make anchor expire after the given time
        cloudAnchor.Expiration = DateTimeOffset.Now.AddDays(dayOffsetToExpiration);
        //wait while device is capturing enough visual data to create anchor
        logger.Log("Turn your phone to capture spatial data:");
        
        while (!cloudManager.IsReadyForCreate)
        {
            waitingForMoreSpatialData = true;
            await Task.Delay(100);
        }
        waitingForMoreSpatialData = false;
        try
        {
            // save anchor to cloud
            await cloudManager.CreateAnchorAsync(cloudAnchor);
            // successfull?
            if (cloudAnchor != null)
            {
                logger.Log($"Anchor saved with id: {cloudAnchor.Identifier}");
                logger.Log($"Trying to save it to the SharingService");
                await httpPost(cloudAnchor.Identifier);
                logger.Log($"Saved succsessful with ID: \n{cloudAnchor.Identifier}");
            }
            else
            {
                logger.Log("Failed to save to ASA Cloud, but no exception was thrown.");

            }
        }
        catch (Exception e)
        {
            logger.Log($"Error while saving: {e.Message} (ErrorType: {e.GetType()})");
        }
        //clear all data to load the data from a new session
        deactivatePointCloud();
        resetUI();
    }


    /// <summary>
    /// Loads the Anchor from <see cref="httpGet"/> and creates an <see cref="AnchorLocateCriteria"/>
    /// </summary>
    /// <returns></returns>
    private async Task loadAnchorAsync()
    {
        await startSessionAsync();
        AnchorLocateCriteria criteria = new AnchorLocateCriteria();
        string[] identifier = { await httpGet() };
        criteria.Identifiers = identifier;
        
        if ((cloudManager != null) && (cloudManager.Session != null))
        {
            cloudManager.Session.CreateWatcher(criteria);
        }
        else
        {
            logger.Log("Not able to create a Watcher, because manager or session is null!", TextState.ERROR);
            await Task.Delay(2000);
        }
    }
    /// <summary>
    /// Starts the session in the <see cref="cloudManager"/>. 
    /// </summary>
    /// <returns></returns>
    private async Task startSessionAsync()
    {
        try
        {
            logger.Log("Trying to start the session.");
            await cloudManager.StartSessionAsync();
            
                 
        }
        catch (Exception e)
        {
            logger.Log(e.GetType().ToString());
            logger.Log(e.Message);
            logger.Log("Could not start ASA session.", TextState.ERROR);
            // in case this happens, the user has some time to read it. 
            await Task.Delay(1000);
        }
    }


    #endregion

    #region SharingService

    /// <summary>
    /// Performs the post request to your personal sharing service.
    /// 
    /// --> Change the implementation to make it fit to your own service.
    /// </summary>
    /// <param name="id">The anchor ID you want to save.</param>
    /// <returns>Indeed nothing. Its just an awaitable task.</returns>
    private async Task httpPost(string id)
    {
        SharingService sharingService = FindObjectOfType<SharingService>();
        try
        {
            await sharingService.postToAPIasync(id, logger);
        }
        catch (Exception e)
        {
            logger.Log($"Something went wrong while posting: \nMsg: {e.Message} - Type: {e.GetType()} ");
        }
    }

    /// <summary>
    /// Performs the get request to your personal sharing service
    /// 
    /// --> Change the implementation to make it fit to your own service.
    /// </summary>
    /// <returns>The identifier string of the last anchor saved in the service.</returns>
    private async Task<string> httpGet()
    {
        SharingService sharingService = FindObjectOfType<SharingService>();
        try
        {
            return await sharingService.getFromAPIasync(logger);
        }
        catch (Exception e)
        {
            logger.Log($"Something went wrong while httpGet: \nMsg: {e.Message} - Type: {e.GetType()} ");
            return String.Empty;
        }
    }

    
    #endregion
}
