using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using System.Collections;

public class GamePlay : CanvasUI, IObserver
{
    public Text textTimer;
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

    // List để thay thế cho Dictionary trong Inspector
    [Header("Character Info List")]
    public List<CharacterInfo> characterInfoList = new List<CharacterInfo>();

    

    void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        Subject.RegisterObserver(this);
    }

    void OnDestroy()
    {
        Subject.UnregisterObserver(this);
    }

    public void OnNotify(string eventName, object eventData)
    {
        if (eventName == "updateRank")
        {
            updateText();
            UpdateRanking();
        }
        if (eventName == "StartGame")
        {
            characters = new List<Character>();
            // Tìm tất cả các đối tượng có component Character trong scene
            Character[] foundCharacters = FindObjectsOfType<Character>();

            foreach (var character in foundCharacters)
            {
                if (!characters.Contains(character))
                {
                    characters.Add(character);
                }
            }
            foreach (Transform child in InforParent)
            {
                Destroy(child.gameObject);
            }
            StartCoroutine(loadData());
        }
    }

    IEnumerator loadData()
    {
        yield return new WaitForSeconds(1);
        UpdateCharacterInfo();
    }

    void UpdateRanking()
    {
        var sortedCharacters = characters.OrderByDescending(c => c.Money).ToList();
        UpdateRankingUIWithAnimation(sortedCharacters);
    }

    private void UpdateRankingUIWithAnimation(List<Character> sortedCharacters)
    {
        for (int i = 0; i < sortedCharacters.Count; i++)
        {
            var characterInfo = characterInfoList.FirstOrDefault(x => x.character == sortedCharacters[i]);
            if (characterInfo == null)
            {
                Debug.LogError($"Character {sortedCharacters[i]} không có trong characterInfoList!");
                continue;
            }
            GameObject infoObject = characterInfo.infoObject;
            Vector3 newPosition = infoObject.transform.localPosition;
            newPosition.y = -i * 75f;  // Chỉnh sửa khoảng cách giữa các item (ví dụ: 100 đơn vị)
            infoObject.transform.DOLocalMove(newPosition, 0.5f).SetEase(Ease.OutQuad);
            infoObject.transform.SetSiblingIndex(i);
        }
    }
    void updateText()
    {
        foreach(CharacterInfo info in characterInfoList )
        {
            Text nameText = info.infoObject.transform.Find("Name").GetComponent<Text>();
            Text moneyText = info.infoObject.transform.Find("Money").GetComponent<Text>();
            nameText.text = info.character.Name;
            moneyText.text = '$' + info.character.Money.ToString();
        }
    }

   private void UpdateCharacterInfo()
    {
     // Làm sạch tất cả các đối tượng con của parent
       
        characterInfoList.Clear();
        // Tạo lại thông tin cho mỗi character và thêm vào list
        foreach (var character in characters)
        {
            GameObject infoObject = Instantiate(InforPrefab, InforParent);
            CharacterInfo characterInfo = new CharacterInfo
            {
                    character = character,
                    infoObject = infoObject
                };
                characterInfoList.Add(characterInfo);
            // Cập nhật thông tin UI
            Text nameText = infoObject.transform.Find("Name").GetComponent<Text>();
            Text moneyText = infoObject.transform.Find("Money").GetComponent<Text>();
            Image characterImage = infoObject.GetComponent<Image>();

            nameText.text = character.Name;
            moneyText.text = '$' + character.Money.ToString();

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
    void Update()
    {
        if (gameManager != null)
        {
            int countdownTime = Mathf.Max(0, Mathf.FloorToInt(gameManager.countdownTime));
            textTimer.text = countdownTime.ToString();
        }
        else
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }
}
[System.Serializable]
    public class CharacterInfo
    {
        public Character character;
        public GameObject infoObject;
    }
