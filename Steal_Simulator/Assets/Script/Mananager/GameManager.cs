using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
public class GameManager : MonoBehaviour , IObserver
{
    [SerializeField] private UnityEngine.Playables.PlayableDirector introTimeline; // Timeline
    [SerializeField] private bool isCountingDown = false;
    public float countdownTime = 30f;
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
        countdownTime = 30f; // Đặt lại thời gian đếm ngược
        isCountingDown = true; // Bắt đầu đếm ngược
    }

    void Start()
    {
        // Đăng ký sự kiện khi Scene được chuyển đổi
          // Lắng nghe sự kiện kết thúc Timeline
        if (introTimeline != null)
        {
            introTimeline.stopped += OnIntroFinished;
            introTimeline.Play();
        }
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
     private void OnIntroFinished(UnityEngine.Playables.PlayableDirector director)
    {
        UIManager.Instance.OpenUI<StartGame>();
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
            CancelInvoke("SpawnObject");
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
        Subject.NotifyObservers("End");
        Time.timeScale = 0;
    } 
}
