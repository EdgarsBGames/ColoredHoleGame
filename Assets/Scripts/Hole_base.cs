using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;

public class Hole_base : MonoBehaviour
{
    public HoleType theType;

    public GameObject holeParentObj,holeObjHitzone,changeBoardObj;


    // type change timer min max
    public float minChangeTime = 5f;
    public float maxChangeTime = 15f;
    [SerializeField]private int variantAmount = 2;

    public float changeWarningTime = 1f;

    public Material[] holeColors;//0 reward , 1 lose ,2 time

    public int Variant = 0;

    public float targetTime;

    public HoleType currentHType;

    public MMF_Player blueScoreFeedback, redScoreFeedback,holeChangeFeedback;

    [SerializeField] private int blueHoleScore, redHoleScore;

    public enum HoleType {Reward, Lose}; //option to add more types 

    void Start()
    {
        targetTime = Random.Range(minChangeTime, maxChangeTime);
        Variant = Random.Range(0, 3);
        SetHoleType();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Gamemanager.Instance.ballManager.holdingBall)
        {
            Timer();
        }
        
    }


    //logic for balls that land in the hole
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            int randoNumb = Random.Range(0, 2); //on 1 change position of object else just add score
            switch (currentHType)
            {
                case HoleType.Reward:
                    Gamemanager.Instance.AddOrRemoveTime(5f);
                    blueScoreFeedback?.PlayFeedbacks();
                    //swaps object position
                    if(randoNumb == 1)
                    {
                        Gamemanager.Instance.MoveObject(holeParentObj);
                    }
                    
                    //adds score
                    Gamemanager.Instance.AddScore(blueHoleScore);
                    Gamemanager.Instance.ballManager.ResetBallPosition();
                    return;
                case HoleType.Lose:
                    // reduce timer by 10 sec 
                    Gamemanager.Instance.AddOrRemoveTime(-10f);
                    
                    redScoreFeedback?.PlayFeedbacks();
                    // Debug.Log("-10sec timer");
                    if (randoNumb == 1)
                    {
                        Gamemanager.Instance.MoveObject(holeParentObj);
                    }
                    
                    Gamemanager.Instance.AddScore(redHoleScore);
                    Gamemanager.Instance.ballManager.ResetBallPosition();
                    return;
                    //  case HoleType.Time:
                    //add time to player 5 sec
                    //    Gamemanager.Instance.AddOrRemoveTime(5f);
                    //     Debug.Log("adding time");
                    //    Gamemanager.Instance.MoveObject(holeParentObj);
                    //   return;
            }
        }
        
    }

    void SetHoleType()
    {
        switch (Variant)
        {
            case 0:
                holeObjHitzone.GetComponent<MeshRenderer>().material = holeColors[0];
                targetTime = Random.Range(minChangeTime, maxChangeTime);
                currentHType = HoleType.Reward;
                return;
            case 1:
                holeObjHitzone.GetComponent<MeshRenderer>().material = holeColors[1];
                targetTime = Random.Range(minChangeTime, maxChangeTime);
                currentHType = HoleType.Lose;
                return;
            case 2:
                // add more hole types when neccesary
               // holeObjHitzone.GetComponent<MeshRenderer>().material = holeColors[2];
              //  targetTime = Random.Range(minChangeTime, maxChangeTime);
              //  currentHType = HoleType.Time;
                
                return;
        }
    }

    IEnumerator HoleChangeWarning()
    {
      //  changeBoardObj.SetActive(true);
        holeChangeFeedback?.PlayFeedbacks();
        yield return new WaitForSeconds(changeWarningTime);
        SetHoleType();
        yield break;
    }


    //put this somewhere else 
    void Timer()
    {
        targetTime -= Time.deltaTime;
        //Will only work if ball is not held down
        if(targetTime <= 0.0f)
        {
            if (!Gamemanager.Instance.ballManager.holdingBall)
            {
                Variant = Random.Range(0, variantAmount);
                StartCoroutine(HoleChangeWarning());
            }               
            
        }
    }

}
