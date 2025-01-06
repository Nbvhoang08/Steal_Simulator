using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
public class GamePlay : CanvasUI
{
    // Start is called before the first frame update
    public Text textTimer;
    public Player player;
    public GameManager gameManager; 

     [Header("Character Settings")]
    public List<Character> characters;

    [Header("UI Settings")]
    public GameObject InforPrefab; // Prefab UI để hiển thị thông tin
    public Transform InforParent; // Nơi chứa các prefab thông tin

    [Header("CharacterType Sprites")]
    public Sprite blueSprite;
    public Sprite redSprite;
    public Sprite pinkSprite;
    public Sprite yellowSprite;

    private Dictionary<Character, GameObject> characterInfoObjects = new Dictionary<Character, GameObject>();
    void Awake()
    {
        // Tìm tất cả các đối tượng có component Character trong scene
        Character[] foundCharacters = FindObjectsOfType<Character>();

        // Kiểm tra mỗi đối tượng xem có sẵn trong danh sách characters chưa
        foreach (var character in foundCharacters)
        {
            if (!characters.Contains(character))  // Nếu không có trong danh sách
            {
                characters.Add(character);  // Thêm vào danh sách
            }
        }
    }

    void Start()
    {
        // Khởi tạo thông tin ban đầu cho từng nhân vật
        foreach (var character in characters)
        {
            GameObject infoObject = Instantiate(InforPrefab, InforParent);
            characterInfoObjects.Add(character, infoObject);
        }
    }

    void Update()
    {
        // Cập nhật thông tin liên tục
        
        UpdateCharacterInfo();
        

        // Sắp xếp thứ tự hiển thị dựa trên số tiền
        UpdateRanking();
        if (gameManager != null)
        {
            int countdownTime = Mathf.Max(0, Mathf.FloorToInt(gameManager.countdownTime));
            textTimer.text = countdownTime.ToString();
           
        }
    }

   private void UpdateCharacterInfo()
    {
        // Cập nhật thông tin UI của từng character
        foreach (var character in characters)
        {
            if (characterInfoObjects.ContainsKey(character))
            {
                GameObject infoObject = characterInfoObjects[character];

                // Cập nhật tên và số tiền
                Text nameText = infoObject.transform.Find("Name").GetComponent<Text>();
                Text moneyText = infoObject.transform.Find("Money").GetComponent<Text>();
                Image characterImage = infoObject.transform.Find("CharacterImage").GetComponent<Image>();

                // Cập nhật tên và số tiền
                nameText.text = character.Name;
                moneyText.text = character.Money.ToString();

                // Thay đổi hình ảnh của character dựa trên CharacterType
                switch (character.Type)
                {
                    case CharacterType.Blue:
                        characterImage.sprite = blueSprite;
                        break;
                    case CharacterType.Red:
                        characterImage.sprite = redSprite;
                        break;
                    case CharacterType.Pink:
                        characterImage.sprite = pinkSprite;
                        break;
                    case CharacterType.Yellow:
                        characterImage.sprite = yellowSprite;
                        break;
                }
            }
        }
    }

    private void UpdateRanking()
    {
        // Sắp xếp danh sách characters theo số tiền giảm dần
        var sortedCharacters = characters.OrderByDescending(c => c.Money).ToList();

        // Cập nhật thứ tự xếp hạng trong bảng xếp hạng
        UpdateRankingUIWithAnimation(sortedCharacters);
    }

    private void UpdateRankingUIWithAnimation(List<Character> sortedCharacters)
    {
        // Thay đổi thứ tự xếp hạng trong UI với hiệu ứng di chuyển
        for (int i = 0; i < sortedCharacters.Count; i++)
        {
            GameObject infoObject = characterInfoObjects[sortedCharacters[i]];

            // Tính toán vị trí mới (relative position)
            Vector3 newPosition = infoObject.transform.localPosition;
            newPosition.y = -i * 100f;  // Chỉnh sửa khoảng cách giữa các item (ví dụ: 100 đơn vị)

            // Di chuyển object đến vị trí mới với animation (0.5 giây)
            infoObject.transform.DOLocalMove(newPosition, 0.5f).SetEase(Ease.OutQuad);

            // Cập nhật lại thứ tự xếp hạng trong hierarchy (giữ đúng thứ tự)
            infoObject.transform.SetSiblingIndex(i); 
        }
    }
    // Update is called once per frame
    
}
