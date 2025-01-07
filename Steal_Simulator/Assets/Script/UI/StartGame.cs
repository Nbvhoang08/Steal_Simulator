using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class StartGame : CanvasUI
{
   // Start is called before the first frame update
    public RectTransform[] childElements; // Các phần tử con
    public float animationDuration = 0.5f; // Thời gian hiệu ứng
    public  GameManager gameManager;
    public Vector2[] initialPositions;
    [Header("Sound Setting")]
    public Sprite OnVolume;
    public Sprite OffVolume;
    [SerializeField] private Button buttonImage;
    private void OnEnable()
    {
        ShowUI();
    }
    private void Awake()
    {
        // Lưu lại vị trí ban đầu của các phần tử con
        initialPositions = new Vector2[childElements.Length];
        for (int i = 0; i < childElements.Length; i++)
        {
            initialPositions[i] = childElements[i].anchoredPosition;
            // Đặt các phần tử con ra ngoài màn hình trước
            childElements[i].anchoredPosition += new Vector2(-Screen.width, 0);
              // Đăng ký sự kiện OnClic
        }
        if(gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }
    void Update()
    {
        if(gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }else
        UpdateButtonImage();
    }

    public void ShowUI()
    {
        if (childElements == null || childElements.Length == 0)
        {
            Debug.LogError("childElements chưa được gán hoặc rỗng!");
            return;
        }

        // Đảm bảo initialPositions đã được khởi tạo
        if (initialPositions == null || initialPositions.Length != childElements.Length)
        {
            Debug.LogError("initialPositions chưa được khởi tạo hoặc không khớp với số lượng childElements!");
            return;
        }

        // Tạo hiệu ứng trượt vào
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < childElements.Length; i++)
        {
            RectTransform child = childElements[i];

            // Tọa độ bắt đầu ngoài màn hình
            Vector2 startPosition;
            if (i == 0) // Phần tử đầu tiên trượt từ trên xuống
            {
                startPosition = initialPositions[i] + new Vector2(0, Screen.height);
            }
            else // Các phần tử còn lại trượt từ dưới lên
            {
                startPosition = initialPositions[i] - new Vector2(0, Screen.height);
            }

            // Đặt vị trí ban đầu của phần tử
            child.anchoredPosition = startPosition;

            // Thêm hiệu ứng di chuyển đến vị trí đích
            sequence.Join(child.DOAnchorPos(initialPositions[i], animationDuration).SetEase(Ease.OutBack));
        }
    }

    public void HideUI()
    {
        // Tạo hiệu ứng trượt ra
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < childElements.Length; i++)
        {
            Vector2 targetPosition;

            // Xác định hướng trượt: phần tử đầu tiên lên trên, các phần tử còn lại xuống dưới
            if (i == 0)
            {
                targetPosition = initialPositions[i] + new Vector2(0, Screen.height); // Trượt lên trên
            }
            else
            {
                targetPosition = initialPositions[i] - new Vector2(0, Screen.height); // Trượt xuống dưới
            }

            // Thêm hiệu ứng cho từng phần tử vào sequence (cùng lúc)
            sequence.Join(childElements[i].DOAnchorPos(targetPosition, animationDuration).SetEase(Ease.InBack));
        }

        // Sau khi hoàn tất hiệu ứng, mở giao diện mới
        // sequence.OnComplete(() =>
        // {
            
        //     Subject.NotifyObservers("startGame");
            
        // });
    }
     public void SoundBtn()
    {
        OnButtonClick(buttonImage);
        SoundManager.Instance.TurnOn = !SoundManager.Instance.TurnOn;
        UpdateButtonImage();
        SoundManager.Instance.PlayClickSound();

    }

    private void UpdateButtonImage()
    {
        if (SoundManager.Instance.TurnOn)
        {
            buttonImage.image.sprite = OnVolume;
        }
        else
        {
            buttonImage.image.sprite = OffVolume;
        }
    }
    public void StartBtn()
    {
        HideUI();
        gameManager.StartGame();
        SoundManager.Instance.PlayClickSound();
    }
    
    private void OnButtonClick(Button button)
    {
        // Hiệu ứng rung nhẹ khi click
        button.transform.DOShakePosition(0.5f, strength: new Vector3(5, 5, 0), vibrato: 10, randomness: 90)
                .SetEase(Ease.OutBack);
    }
}
