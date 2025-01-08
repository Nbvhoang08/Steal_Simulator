using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
public class GameManager : MonoBehaviour , IObserver
{
    [SerializeField] private UnityEngine.Playables.PlayableDirector introTimeline; // Timeline
    public UnityEngine.Playables.PlayableDirector outroTimeline;
    [SerializeField] private bool isCountingDown = false;
    public float countdownTime = 30f;
    public float time;
    public bool GameOver { get; private set; } = false;
    public static GameManager Instance { get; private set; }
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Subject.RegisterObserver(this); // Đăng ký observer
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void OnNotify(string eventName,object eventData)
    {
        if(eventName == "End")
        {
            CancelInvoke("SpawnObject");
        }
    }
    private void StartCountdown()
    {
        countdownTime = time; // Đặt lại thời gian đếm ngược
        isCountingDown = true; // Bắt đầu đếm ngược
    }

    void Start()
    {
        
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    public void StartGame()
    {
        if (introTimeline != null)
        {
            introTimeline.stopped += OnIntroFinished;
            introTimeline.Play();
        }
    }   
    private void OnIntroFinished(UnityEngine.Playables.PlayableDirector director)
    {
        UIManager.Instance.CloseUI<StartGame>(0.2f);    
        UIManager.Instance.OpenUI<GamePlay>();
        StartCountdown();
        Subject.NotifyObservers("StartGame");

    }
    private void OnOuttroFinished(UnityEngine.Playables.PlayableDirector director)
    {
        UIManager.Instance.OpenUI<EndGame>();
    }
    private void OnSceneUnloaded(Scene current)
    {
        DOTween.KillAll(); // Hủy tất cả các tween khi Scene đóng.
    }

    private void OnDestroy()
    {
        // Đảm bảo gỡ sự kiện để tránh lỗi
        if (Instance == this)
        {
            Subject.UnregisterObserver(this); // Hủy đăng ký observer
            Instance = null; // Làm rỗng instance
        }
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    void Update()
    {
       if (isCountingDown)
        {
            // Giảm thời gian đếm ngược
            countdownTime -= Time.deltaTime;
            // Kiểm tra nếu hết thDeời gian
            if (countdownTime <= 0)
            {
                EndGame();
                isCountingDown = false;
                return;
            }
        }
    }
    private void EndGame()
    {
  
       if (outroTimeline != null)
        {
            outroTimeline.stopped += OnOuttroFinished;
            outroTimeline.Play();
        }
        Subject.NotifyObservers("End");
    } 
}
