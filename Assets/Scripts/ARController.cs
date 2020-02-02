using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Events;
using TMPro;

public class ARController : MonoBehaviour
{
    public class OpenCardEvent : UnityEvent<OpenCard> { }

    [System.Serializable]
    public class SpawnedMushroom
    {
        public GameObject gameObject;
        public int score;

        public SpawnedMushroom(GameObject gameObject, int score)
        {
            this.gameObject = gameObject;
            this.score = score;
        }
    }

    [System.Serializable]
    public class OpenCard
    {
        public GameObject gameObject;
        public int score;

        public OpenCard(GameObject gameObject, int score)
        {
            this.gameObject = gameObject;
            this.score = score;
        }
    }

    public class OpenCardsByPoints : IComparer<OpenCard>
    {
        public int Compare(OpenCard x, OpenCard y)
        {
            if(x.score > y.score)
            {
                return -1;
            }
            else if(x.score == y.score)
            {
                return 0;
            }

            return 1;
        }
    }

    public OpenCardEvent onCardOpen;

    public Camera worldSpaceCanvasCamera;

    public Texture defaultTexture;
    public MushroomSpawnAnimation mushroomSpawnAnimation;

    public MushroomData[] mushrooms;

    public TextMeshProUGUI positionTextPrefab;
    public Canvas canvas;

    [SerializeField] private ARSession m_session;
    [SerializeField] private ARTrackedImageManager m_trackedImage;

    private Dictionary<string, GameObject> m_spawnedObjects;

    public Vector3? StartPosition { get; private set; }   // Center
    public List<SpawnedMushroom> SpawnedMushrooms { get; private set; }
    public List<TextMeshProUGUI> SpawnedPositionTexts { get; private set; }
    public List<TextMeshProUGUI> SpawnedOpenTexts { get; private set; }
    public List<OpenCard> OpenCards { get; private set; }
    public Dictionary<OpenCard, SpawnedMushroom> OpenToSpawned { get; private set; }

    private void Awake()
    {
        m_spawnedObjects = new Dictionary<string, GameObject>();
        SpawnedMushrooms = new List<SpawnedMushroom>();
        SpawnedPositionTexts = new List<TextMeshProUGUI>();
        SpawnedOpenTexts = new List<TextMeshProUGUI>();
        OpenCards = new List<OpenCard>();
        OpenToSpawned = new Dictionary<OpenCard, SpawnedMushroom>();

        onCardOpen = new OpenCardEvent();
    }

    private IEnumerator Start()
    {
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            throw new System.Exception("Not supported");
        }
        else
        {
            m_session.enabled = true;
        }

        if(m_trackedImage.descriptor.supportsMovingImages)
        {
            m_trackedImage.maxNumberOfMovingImages = 4;
        }
    }

    private void OnEnable()
    {
        m_trackedImage.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        m_trackedImage.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // Give the initial image a reasonable default scale
            //trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

            Debug.Log("Added: " + trackedImage.referenceImage.name);

            if (trackedImage.referenceImage.name.StartsWith("A_"))
            {
                var points = int.Parse(trackedImage.referenceImage.name.Substring(2));
                var spawnedMushroom = SpawnedMushrooms.Find(x => x.gameObject == m_spawnedObjects["SENE_" + (points < 10 ? "0" : "") + points]);
                var openCard = new OpenCard(trackedImage.gameObject, points);
                OpenCards.Add(openCard);
                OpenToSpawned.Add(openCard, spawnedMushroom);

                if (Debug.isDebugBuild)
                {
                    SpawnedOpenTexts.Add(Instantiate(positionTextPrefab, canvas.transform));
                }

                onCardOpen.Invoke(openCard);
            }
            else
            {
                MushroomData curData = null;

                for (int i = 0; i < mushrooms.Length; i++)
                {
                    if (mushrooms[i].ToString().Equals(trackedImage.referenceImage.name))
                    {
                        curData = mushrooms[i];
                        break;
                    }
                }

                if (curData == null) throw new System.Exception("curData: " + trackedImage.referenceImage.name + " not found!");

                var instance = Instantiate(curData.prefab, trackedImage.transform);

                mushroomSpawnAnimation.Play(instance.transform);
                m_spawnedObjects.Add(trackedImage.referenceImage.name, instance);
                SpawnedMushrooms.Add(new SpawnedMushroom(instance, curData.id));

                if (StartPosition == null && SpawnedMushrooms.Count == 1)
                {
                    StartPosition = SpawnedMushrooms[0].gameObject.transform.position;
                }

                if (Debug.isDebugBuild)
                {
                    SpawnedPositionTexts.Add(Instantiate(positionTextPrefab, canvas.transform));
                }
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if(trackedImage.trackingState != TrackingState.Tracking)
            {
                if (m_spawnedObjects.ContainsKey(trackedImage.referenceImage.name))
                {
                    m_spawnedObjects[trackedImage.referenceImage.name].SetActive(false);
                }
                //var spawnedMushroom = SpawnedMushrooms.Find(x => x.gameObject == m_spawnedObjects[trackedImage.referenceImage.name]);
                //GameObject trackable = m_spawnedObjects[trackedImage.referenceImage.name];
                //m_spawnedObjects.Remove(trackedImage.referenceImage.name);
                //SpawnedMushrooms.Remove(spawnedMushroom);
                //Destroy(trackable);
            }
            else
            {
                if (m_spawnedObjects.ContainsKey(trackedImage.referenceImage.name))
                {
                    m_spawnedObjects[trackedImage.referenceImage.name].SetActive(true);
                }

                //Debug.Log(trackedImage.transform.eulerAngles.y.ToString());
            }
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            if (m_spawnedObjects.ContainsKey(trackedImage.referenceImage.name))
            {
                var spawnedMushroom = SpawnedMushrooms.Find(x => x.gameObject == m_spawnedObjects[trackedImage.referenceImage.name]);
                m_spawnedObjects.Remove(trackedImage.referenceImage.name);
                SpawnedMushrooms.Remove(spawnedMushroom);
                Destroy(m_spawnedObjects[trackedImage.referenceImage.name]);
            }
        }
    }

}
