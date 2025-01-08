using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;
public class EndGame : CanvasUI
{
     public List<Character> characters;
    [Header("UI Settings")]
    public GameObject InforPrefab; // Prefab UI để hiển thị thông tin
    public Transform InforParent; // Nơi chứa các prefab thông tin
     [Header("CharacterType Sprites")]
    public Sprite blueSprite;
    public Sprite redSprite;
    public Sprite pinkSprite;
    public Sprite yellowSprite;
    [Header("Character Info List")]
    public List<CharacterInfo> characterInfoList = new List<CharacterInfo>();
     void Awake()
    {
        // Tìm tất cả các đối tượng có component Character trong scene
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
    }
    void OnEnable()
    {
        foreach (Transform child in InforParent)
        {
            Destroy(child.gameObject);
        }
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
        StartCoroutine(loadData());
    }
    
    IEnumerator loadData()
    {
        yield return new WaitForSeconds(0.3f);
        UpdateCharacterInfo();
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
        UpdateRanking();
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
            // infoObject.transform.DOLocalMove(newPosition, 0.5f).SetEase(Ease.OutQuad);
            infoObject.transform.SetSiblingIndex(i);
        }
        Time.timeScale =0;
        
    }
    IEnumerator stopGame(){
        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 0;
    }
     public void RetryBtn()
    {
        Time.timeScale = 1;
        StartCoroutine(ReLoad());
        SoundManager.Instance.PlayClickSound();
    }
    IEnumerator ReLoad()
    {
        yield return new WaitForSeconds(1);
        ReloadCurrentScene();
    }
    public void ReloadCurrentScene()
    {
        // Lấy tên của scene hiện tại 
        string currentSceneName = SceneManager.GetActiveScene().name;
        //Tải lại scene hiện tại
        SceneManager.LoadScene(currentSceneName);
        UIManager.Instance.OpenUI<StartGame>();
        UIManager.Instance.CloseUIDirectly<EndGame>();
        UIManager.Instance.CloseUIDirectly<GamePlay>();
    }

}