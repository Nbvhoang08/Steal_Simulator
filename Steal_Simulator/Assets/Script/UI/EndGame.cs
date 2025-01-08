using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    void OnEnable()
    {
        // Clear all child objects of InforParent
        foreach (Transform child in InforParent)
        {
            Destroy(child.gameObject); // Destroys each child object completely
        }

        // Spawn new info objects and update character info
        characterInfoObjects.Clear();  // Clear any existing mappings in the dictionary

        foreach (var character in characters)
        {
            GameObject infoObject = Instantiate(InforPrefab, InforParent);
            characterInfoObjects.Add(character, infoObject);
        }

        UpdateCharacterInfo();  // Update info after spawning
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
                Image characterImage = infoObject.GetComponent<Image>();

                // Cập nhật tên và số tiền
                nameText.text = character.Name;
                moneyText.text = '$'+character.Money.ToString();

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
