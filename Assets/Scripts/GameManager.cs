using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //플레이어와 Ai 게임 오브젝트
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject Ai;

    //플레이어와 Ai의 선택을 int로 표시 : 0은 협력, 1은 배신
    private int Player_Select;
    private int Ai_Select;

    //소지한 알의 수
    public int Player_EggCount;
    public int Ai_EggCount;

    //한 판에 거는 알의 수
    public int Bet = 5;

    //소지한 알의 수 표시
    public Text Player_Egg;
    public Text Ai_Egg;

    //시간 제한
    public Text TimeCount;
    private float Timer;

    //Animator 선언
    private Animator player_anim;
    private Animator Ai_anim;

    //라운드 수
    private int Round = 1;
    public int maxRound = 16;
    public Text RoundText;
    public GameObject Round_Panel;

    //플레이어가 배신과 협력한 개수
    private int GreedCount = 0;
    private int CoCount = 0;

    //결과를 넣는 리스트
    private List<int> ResultList = new List<int>();

    //결과 리스트 뒤집기
    private List<int> ReverseList = new List<int>();

    //플레이어가 버튼을 눌렀는지 아닌지
    [SerializeField] private bool isClick = false;

    //배신, 협력 버튼과 버튼 안의 텍스트
    public Button Greed_Btn;
    public Button Co_Btn;
    public Text Greed_Text;
    public Text Co_Text;

    //베팅한 알 텍스트 및 패널
    public GameObject BetPanel;
    public Text BetText;

    //둘의 선택 텍스트
    public GameObject ResultPanel;
    public Text PlayerResultText;
    public Text AiResultText;

    //+ - 알 뜨게 해주는 텍스트
    public Text PlayerCountText;
    public Text AiCountText;
    public GameObject PlayerCountPanel;
    public GameObject AiCountPanel;

    //오디오
    public List<AudioSource> audioSources = new List<AudioSource>();
    public AudioSource Bgm;

    //게임 오버 패널
    public GameObject GameOverPanel;
    public Text ResultText;

    // 메인 패널
    public GameObject MainPanel;

    public Text GameStartText;

    void Start()
    {
        GameOverPanel.SetActive(false);
        Bgm.volume = 0.4f;
        //시간 제한은 10초
        Timer = 21f;

        //처음 시작하는 알의 개수는 100개로 지정
        Player_EggCount = 100;
        Ai_EggCount = 100;

        //애니메이션 속도 조절
        player_anim = Player.GetComponent<Animator>();
        Ai_anim = Ai.GetComponent<Animator>();
        player_anim.speed = 0.5f;
        Ai_anim.speed = 0.5f;

        BetText.text = $"You Bet {Bet} Eggs";
    }

    void Update()
    {
        //게임 오버
        if (Round == maxRound || Player_EggCount <= 0 || Ai_EggCount <= 0)
        {
            StartCoroutine(GameOver());
        }
        else
        {
            //타이머 기능
            Timer = Timer - Time.deltaTime;
            TimeCount.text = $"{(int)Timer}";
            //destroy Game Start text 
            if(Timer <= 19)
            {
                Destroy(GameStartText);
            }
            //한 라운드 시간이 끝나면
            if (Timer <= 0f)
            {
                Judgment();
                Greed_Text.text = "Alone";
                Co_Text.text = "With";
                Greed_Btn.interactable = true;
                Co_Btn.interactable = true;
                Round++;
            }

            //알 갯수 텍스트
            Player_Egg.text = "" + Player_EggCount;
            Ai_Egg.text = "" + Ai_EggCount;

            //라운드 텍스트
            RoundText.text = $"Round {Round}";
        }
    }

    //프레임 조절 : 30프레임으로 고정 
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    //배신 버튼
    public void GreedBtn()
    {
        Btn();

        ResultList.Add(1);
        GreedCount++;

        PlayerResultText.text = "Alone";

        Player_Select = 1;
        AiSelect();
    }

    //협력 버튼
    public void CooperationBtn()
    {
        Btn();

        ResultList.Add(0);
        CoCount++;

        PlayerResultText.text = "With";

        Player_Select = 0;
        AiSelect();
    }

    //Ai의 배신 혹은 협력 선택 함수
    void AiSelect()
    {
        //플레이어의 선택을 집어넣은 리스트를 뒤집어놓은 리스트를 생성
        ResultList.Reverse();
        for(int i = 0; i < ResultList.Count; i++)
        {
            ReverseList.Add(ResultList[i]);
        }
        ResultList.Reverse(); //원래대로 되돌리기

        //라운드 1~5
        if (Round >= 1 && Round <= 5)
        {
            Ai_Select = Random.Range(0, 2);
        }
        //라운드 6~n
        else
        {
            if (GreedCount == CoCount || Mathf.Abs(GreedCount - CoCount) < 3) { 
                Ai_Select = Random.Range(0, 2); //배신 협력 횟수가 같거나 서로의 차가 3 이하일 때 랜덤으로 부여한다.
            }
            else if(Mathf.Abs(GreedCount - CoCount) >= 3)
            {
                if (GreedCount > CoCount)
                {
                    Ai_Select = 1; //플레이어가 배신한 횟수가 3번 더 많을 시에 배신을 선택
                }
                else if (GreedCount < CoCount)
                {
                    Ai_Select = 0; //플레이어가 협력한 횟수가 3번 더 많을 시에 협력을 선택
                }
            }
        }
        if (Ai_Select == 0) AiResultText.text = "With";
        else AiResultText.text = "Alone";
    }

    // 승 패 결정 함수
    public void Judgment()
    {
        if (!isClick) // 아무 버튼도 클릭하지 않았을 때, 플레이어의 알만 깎음.
        {
            Player_EggCount -= Bet * 2;
            AiResultText.text = "";
            PlayerResultText.text = "No Select, -10 Eggs";
            PlayerCountText.text = $"-{2 * Bet}";
            AiCountText.text = "";
            audioSources[1].Play();
        }
        else
        {
            if (Player_Select == Ai_Select) //플레이어와 Ai의 선택이 같을 시
            {
                if (Player_Select == 0 && Ai_Select == 0)
                {
                    //둘 다 협력
                    Player_EggCount += (Bet * 2);
                    Ai_EggCount += (Bet * 2);
                    PlayerCountText.text = $"+{2 * Bet}";
                    AiCountText.text = $"+{2 * Bet}";
                    audioSources[0].Play();
                }
                else
                {
                    //둘 다 배신
                    Player_EggCount -= (Bet * 3);
                    Ai_EggCount -= (Bet * 3);
                    PlayerCountText.text = $"-{3 * Bet}";
                    AiCountText.text = $"-{3 * Bet}";
                    audioSources[1].Play();
                }
            }
            else //플레이어와 Ai의 선택이 다를 시
            {
                if (Player_Select > Ai_Select)
                {
                    //플레이어만 배신
                    Player_EggCount += (Bet * 4);
                    Ai_EggCount -= Bet;
                    PlayerCountText.text = $"+{4 * Bet}";
                    AiCountText.text = $"-{Bet}";
                    audioSources[0].Play();
                }
                else
                {
                    //Ai만 배신
                    Ai_EggCount += (Bet * 4);
                    Player_EggCount -= Bet;
                    PlayerCountText.text = $"-{Bet}";
                    AiCountText.text = $"+{4 * Bet}";
                    audioSources[1].Play();
                }
            }
        }
        ReverseList.Clear(); //뒤집어 놓은 리스트 다시 비우기
        Timer = 16f; //타이머 리셋
        isClick = false; //클릭 여부를 다시 false로 바꿈
        StartCoroutine(ResultTextPanelOn()); //결과창 띄우기
        StartCoroutine(CountPanelText()); // + - 띄우기
    }

    //걸은 알 갯수 
    IEnumerator BetEgg()
    {
        BetPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        BetPanel.SetActive(false);
    }

    //둘의 결과 표시 
    IEnumerator ResultTextPanelOn()
    {
        ResultPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        ResultPanel.SetActive(false);
    }

    //+ - 텍스트
    IEnumerator CountPanelText()
    {
        PlayerCountPanel.SetActive(true);
        AiCountPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        PlayerCountPanel.SetActive(false);
        AiCountPanel.SetActive(false);
    }

    //배신 협력 버튼 공통사항
    void Btn()
    {
        audioSources[2].Play();
        isClick = true; //클릭 유무를 true로 바꿈
        //알 베팅
        Player_EggCount -= Bet;
        Ai_EggCount -= Bet;
        //버튼 비활성화 
        Greed_Btn.interactable = false;
        Co_Btn.interactable = false;
        Greed_Text.text = "";
        Co_Text.text = "";
        StartCoroutine(BetEgg()); //베팅 텍스트 띄우기
        //+ - 텍스트 띄우기
        PlayerCountText.text = $"-{Bet}";
        AiCountText.text = $"-{Bet}";
        StartCoroutine(CountPanelText());
        //시간이 3초 이상 남았을 시 3초로 줄이기
        if (Timer >= 3f)
        {
            Timer = 4f;
        }
    }

    // 라운드가 모두 끝나면
    IEnumerator GameOver()
    {
        Round_Panel.SetActive(false);
        BetPanel.SetActive(false);
        PlayerCountPanel.SetActive(false);
        AiCountPanel.SetActive(false);
        MainPanel.SetActive(false);
        ResultPanel.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        ResultText.text = $"Result : {Player_EggCount}";
        GameOverPanel.SetActive(true);
    }

    
}