using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GameController : MonoBehaviour
{
    private delegate void StateHandler();

    public ARSessionOrigin arSessionOrigin;
    public ARController arController;
    public Camera arCamera;
    public UIController uiController;

    public AudioSource scoreAudio;
    public AudioClip[] successClips;
    public AudioClip[] failClips;

    public ParticleSystem openCardParticles;

    private StateHandler m_currentState;

    private int m_playerCount = 2;
    private int m_curTotalPoints;

    private List<ARController.OpenCard> m_sortedOpenCards;

    private bool m_isMyTurn;
    private ARController.OpenCard m_openCard;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        m_currentState = EnterState_None;
    }

    private void Update()
    {
        m_currentState();
    }

    private void EnterState_None()
    {
        m_currentState = State_None;
    }

    private void State_None()
    {
        ExitState_None(EnterState_Play);
    }

    private void ExitState_None(StateHandler targetState)
    {
        m_currentState = targetState;
    }

    private void EnterState_Play()
    {
        arController.onCardOpen.AddListener(HandleOpenCard);
        m_currentState = State_Play;
    }

    private void State_Play()
    {
        if (Debug.isDebugBuild)
        {
            if (arController.SpawnedMushrooms != null && arController.SpawnedMushrooms.Count > 0)
            {
                for (int i = 0; i < arController.SpawnedPositionTexts.Count; i++)
                {
                    if (arController.SpawnedMushrooms[i].gameObject.activeSelf)
                    {
                        arController.SpawnedPositionTexts[i].transform.position = arCamera.WorldToScreenPoint(arController.SpawnedMushrooms[i].gameObject.transform.position);
                        arController.SpawnedPositionTexts[i].text = Vector3.Distance(arController.SpawnedMushrooms[i].gameObject.transform.position, arController.StartPosition.Value).ToString();
                    }
                    else
                    {
                        arController.SpawnedPositionTexts[i].gameObject.SetActive(false);
                    }
                }

                for (int i = 0; i < arController.SpawnedMushrooms.Count; i++)
                {
                    if (arController.mushroomSpawnAnimation.MushroomTransform == arController.SpawnedMushrooms[i].gameObject.transform && arController.mushroomSpawnAnimation.InAnimation) continue;

                    float distToCam = Vector3.Distance(arController.SpawnedMushrooms[i].gameObject.transform.position, arCamera.transform.position);

                    if (distToCam < 0.18f)
                    {
                        arController.SpawnedMushrooms[i].gameObject.transform.localScale = Vector3.Lerp(Vector3.one * 0.01f, Vector3.one, Mathf.InverseLerp(0.11f, 0.18f, distToCam));
                    }
                    else
                    {
                        arController.SpawnedMushrooms[i].gameObject.transform.localScale = Vector3.one;
                    }
                }
            }

            //if(arController.OpenCards.Count > 0)
            //{
            //    Vector3 p2 = arController.OpenCards[0].gameObject.transform.position;
            //    Vector3 p1 = arController.StartPosition.Value;
            //    var angleToFirst = Mathf.Atan2(p2.z - p1.z, p2.x - p1.x) * Mathf.Rad2Deg;
            //    Debug.Log("AngleToFirst: " + angleToFirst);
            //}

            Debug.Log("opencnt: " + arController.OpenCards.Count);

            if (arController.OpenCards.Count > 0)
            {
                //openCardParticles.transform.position = arController.OpenCards[0].gameObject.transform.position;
                //openCardParticles.transform.rotation = arController.OpenCards[0].gameObject.transform.rotation;

                Vector3 toFirst = arController.OpenCards[0].gameObject.transform.position - arController.StartPosition.Value;
                Vector3 rightOfFirst = Vector3.Cross(Vector3.up, toFirst);
                arController.SpawnedOpenTexts[0].text = "First";
                arController.SpawnedOpenTexts[0].transform.position = arCamera.WorldToScreenPoint(arController.OpenCards[0].gameObject.transform.position);

                if (arController.OpenCards.Count > 1)
                {
                    for (int i = 0; i < arController.SpawnedOpenTexts.Count; i++)
                    {
                        if (i >= arController.OpenCards.Count)
                        {
                            arController.SpawnedOpenTexts[i].gameObject.SetActive(false);
                        }
                    }

                    for (int i = 1; i < arController.OpenCards.Count; i++)
                    {
                        var dot = Vector3.Dot(rightOfFirst, arController.OpenCards[i].gameObject.transform.position - arController.StartPosition.Value);

                        if (dot < 0)
                        {
                            arController.SpawnedOpenTexts[i].text = "To right";
                            arController.SpawnedOpenTexts[i].transform.position = arCamera.WorldToScreenPoint(arController.OpenCards[i].gameObject.transform.position);

                            if (arController.OpenCards[i].score > arController.OpenCards[0].score)
                            {
                                arController.SpawnedOpenTexts[i].text = "To righ: GOT IT";
                            }
                            else
                            {
                                arController.SpawnedOpenTexts[i].text = "To righ: WRONG";
                            }
                        }
                        else
                        {
                            arController.SpawnedOpenTexts[i].text = "To left";
                            arController.SpawnedOpenTexts[i].transform.position = arCamera.WorldToScreenPoint(arController.OpenCards[i].gameObject.transform.position);

                            if (arController.OpenCards[i].score > arController.OpenCards[0].score)
                            {
                                arController.SpawnedOpenTexts[i].text = "To left: WRONG";
                            }
                            else
                            {
                                arController.SpawnedOpenTexts[i].text = "To left: GOT IT";
                            }
                        }
                    }

                    var leftmost = GetLeftmostOpenCard();
                    var rightmost = GetRightmostOpenCard();

                    for (int i = 0; i < arController.OpenCards.Count; i++)
                    {
                        if (m_sortedOpenCards[0] == arController.OpenCards[i]) arController.SpawnedOpenTexts[i].text += "\n(MAX)";
                        if (m_sortedOpenCards[m_sortedOpenCards.Count - 1] == arController.OpenCards[i]) arController.SpawnedOpenTexts[i].text += "\n(MIN)";
                        if (leftmost == arController.OpenCards[i]) arController.SpawnedOpenTexts[i].text += "\n(leftmost)";
                        if (rightmost == arController.OpenCards[i]) arController.SpawnedOpenTexts[i].text += "\n(rightmost)";

                        if (arController.OpenCards.Count > 1)
                        {
                            if (arController.OpenCards[i] != leftmost)
                            {
                                var toLeft = GetToLeft(arController.OpenCards[i]);

                                if (toLeft != null)
                                {
                                    var toLeftName = arController.OpenToSpawned[toLeft].gameObject.name;
                                    arController.SpawnedOpenTexts[i].text += "\n toLeft: " + toLeftName;
                                }
                            }

                            if (arController.OpenCards[i] != rightmost)
                            {
                                var toRight = GetToRight(arController.OpenCards[i]);

                                if (toRight != null)
                                {
                                    var toRightName = arController.OpenToSpawned[toRight].gameObject.name;
                                    arController.SpawnedOpenTexts[i].text += "\n toRight: " + toRightName;
                                }
                            }
                        }
                    }
                }
            }
        }

        if(m_openCard != null)
        {
            UpdateOpenCard(m_openCard);
            m_openCard = null;
        }
    }

    public void StartMyTurn()
    {
        m_isMyTurn = true;
    }

    private void ExitState_Play(StateHandler targetState)
    {
        m_currentState = targetState;
    }

    private ARController.OpenCard GetLeftmostOpenCard()
    {
        int curLeftIndex = 0;

        for (int i = 0; i < arController.OpenCards.Count; i++)
        {
            Vector3 toCurLeft = arController.OpenCards[curLeftIndex].gameObject.transform.position - arController.StartPosition.Value;
            Vector3 rightOfFirst = Vector3.Cross(Vector3.up, toCurLeft);

            var dot = Vector3.Dot(rightOfFirst, arController.OpenCards[i].gameObject.transform.position - arController.StartPosition.Value);

            if (dot > 0)
            {
                curLeftIndex = i;
            }
        }

        return arController.OpenCards[curLeftIndex];
    }

    private ARController.OpenCard GetRightmostOpenCard()
    {
        int curRightIndex = 0;

        for (int i = 0; i < arController.OpenCards.Count; i++)
        {
            Vector3 toCurLeft = arController.OpenCards[curRightIndex].gameObject.transform.position - arController.StartPosition.Value;
            Vector3 rightOfFirst = Vector3.Cross(Vector3.up, toCurLeft);

            var dot = Vector3.Dot(rightOfFirst, arController.OpenCards[i].gameObject.transform.position - arController.StartPosition.Value);

            if (dot < 0)
            {
                curRightIndex = i;
            }
        }

        return arController.OpenCards[curRightIndex];
    }

    private ARController.OpenCard GetToLeft(ARController.OpenCard openCard)
    {
        ARController.OpenCard curLeft = null;
        float closestDist = float.MaxValue;

        for (int i = 0; i < arController.OpenCards.Count; i++)
        {
            if (arController.OpenCards[i] == openCard) continue;

            Vector3 toOpenCard = openCard.gameObject.transform.position - arController.StartPosition.Value;
            Vector3 rightOfOpen = Vector3.Cross(Vector3.up, toOpenCard);

            var dot = Vector3.Dot(rightOfOpen, arController.OpenCards[i].gameObject.transform.position - arController.StartPosition.Value);

            if (dot > 0)
            {
                if(curLeft == null)
                {
                    curLeft = arController.OpenCards[i];
                    closestDist = Vector3.Distance(openCard.gameObject.transform.position, curLeft.gameObject.transform.position);
                }
                else
                {
                    float dist = Vector3.Distance(arController.OpenCards[i].gameObject.transform.position, openCard.gameObject.transform.position);
                    if (dist < closestDist)
                    {
                        curLeft = arController.OpenCards[i];
                        closestDist = dist;
                    }
                }
            }
        }

        return curLeft;
    }

    private ARController.OpenCard GetToRight(ARController.OpenCard openCard)
    {
        ARController.OpenCard curRight = null;
        float closestDist = float.MaxValue;

        for (int i = 0; i < arController.OpenCards.Count; i++)
        {
            if (arController.OpenCards[i] == openCard) continue;

            Vector3 toOpenCard = openCard.gameObject.transform.position - arController.StartPosition.Value;
            Vector3 rightOfOpen = Vector3.Cross(Vector3.up, toOpenCard);

            var dot = Vector3.Dot(rightOfOpen, arController.OpenCards[i].gameObject.transform.position - arController.StartPosition.Value);

            if (dot < 0)
            {
                if (curRight == null)
                {
                    curRight = arController.OpenCards[i];
                    closestDist = Vector3.Distance(openCard.gameObject.transform.position, curRight.gameObject.transform.position);
                }
                else
                {
                    float dist = Vector3.Distance(arController.OpenCards[i].gameObject.transform.position, openCard.gameObject.transform.position);
                    if (dist < closestDist)
                    {
                        curRight = arController.OpenCards[i];
                        closestDist = dist;
                    }
                }
            }
        }

        return curRight;
    }

    public void HandleOpenCard(ARController.OpenCard openCard)
    {
        m_openCard = openCard;
    }

    private void UpdateOpenCard(ARController.OpenCard openCard)
    {
        m_sortedOpenCards = new List<ARController.OpenCard>(arController.OpenCards);

        m_sortedOpenCards.Sort(new ARController.OpenCardsByPoints());

        bool hasFailed = false;

        if (arController.OpenCards.Count > 1)
        {
            var leftmost = GetLeftmostOpenCard();
            var rightmost = GetRightmostOpenCard();

            if (leftmost == openCard)
            {
                var toRight = GetToRight(openCard);
                var scoreToRight = arController.OpenToSpawned[toRight].score;

                if (openCard.score < scoreToRight)
                {
                    uiController.OnOpenCard(true);
                }
                else
                {
                    hasFailed = true;
                }
            }

            if (rightmost == openCard)
            {
                var toLeft = GetToLeft(openCard);
                var scoreToLeft = arController.OpenToSpawned[toLeft].score;

                if (openCard.score > scoreToLeft)
                {
                    uiController.OnOpenCard(true);
                }
                else
                {
                    hasFailed = true;
                }
            }

            if(arController.OpenCards.Count == 2 && (leftmost == null || rightmost == null))
            {
                var toLeft = GetToLeft(openCard);
                var toRight = GetToRight(openCard);

                if ((toLeft != null && toLeft.score > openCard.score) || (toRight != null && toRight.score < openCard.score))
                {
                    hasFailed = true;
                }
                else
                {
                    uiController.OnOpenCard(true);
                }
            }

            if (arController.OpenCards.Count > 2)
            {
                if (openCard != leftmost)
                {
                    var toLeft = GetToLeft(openCard);

                    if (toLeft == null)
                    {
                        Debug.LogError("THIS IS TO LEFT!");
                        leftmost = openCard;
                    }

                    var scoreToLeft = arController.OpenToSpawned[toLeft].score;

                    if (openCard.score > scoreToLeft)
                    {
                        uiController.OnOpenCard(true);
                    }
                    else
                    {
                        hasFailed = true;
                    }
                }

                if (openCard != rightmost)
                {
                    var toRight = GetToRight(openCard);

                    if (toRight == null)
                    {
                        Debug.LogError("THIS IS TO RIGHT!");
                        rightmost = openCard;
                    }

                    var scoreToRight = arController.OpenToSpawned[toRight].score;

                    if (openCard.score < scoreToRight)
                    {
                        uiController.OnOpenCard(true);
                    }
                    else
                    {
                        hasFailed = true;
                    }
                }
            }

            scoreAudio.clip = hasFailed ? scoreAudio.clip = failClips[Random.Range(0, failClips.Length)] : successClips[Random.Range(0, successClips.Length)];

            if(hasFailed)
            {
                FailedOpenOrder(openCard);
            }

            scoreAudio.Play();
        }
    }

    private void FailedOpenOrder(ARController.OpenCard openCard)
    {
        arController.OpenCards.Remove(openCard);
        arController.OpenToSpawned.Remove(openCard);
        uiController.OnOpenCard(false);
    }
}
