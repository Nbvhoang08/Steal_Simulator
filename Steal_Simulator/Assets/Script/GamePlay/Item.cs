using System;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour , IObserver
{ 
    [Header("Loot Settings")]
    private int price;
    public Character character;
    [SerializeField] private float lootRange = 2f; // Khoảng cách để bắt đầu loot
    [SerializeField] private float lootTime = 2f; // Thời gian đứng yên để loot
    [SerializeField] private string playerTag = "Player"; // Tag để xác định Player
    [SerializeField] private string enemyTag = "Enemy";   // Tag để xác định Enemy
    [SerializeField] private Image progressCircle; // Progress Circle riêng của item

    [Header("Item Visual Settings")]
    [SerializeField] private Renderer itemRenderer;       // Renderer của item để highlight
    [SerializeField] private Material highlightMaterial;  // Material highlight
    private Material originalMaterial; // Lưu Material gốc để hoàn nguyên

    [Header("Vehicle Settings")]
    [SerializeField] private Transform vehicleDropPoint; // Vị trí trên xe để item bay đến
    [SerializeField] private float flySpeed = 5f;        // Tốc độ bay về xe
    [SerializeField] private float destroyDelay = 0.3f;  // Thời gian delay trước khi hủy item

    [SerializeField] private bool isLooted = false;         // Trạng thái đã loot
    [SerializeField] private bool isLooting = false;        // Đang bị loot hay không
    [SerializeField] private bool isFlyingToVehicle = false; // Trạng thái đang bay đến xe
    private float lootTimer = 0f;          // Bộ đếm thời gian loot
    private Transform currentLooter = null; // Đối tượng đang cố loot item
    public static Item Instance { get; private set; }
     private void Awake()
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
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Subject.UnregisterObserver(this); // Hủy đăng ký observer
            Instance = null; // Làm rỗng instance
        }

    }
    public void OnNotify(string eventName, object eventData)
    {

    }
    private void Start()
    {
        // Lấy Material gốc từ Renderer
        if (itemRenderer != null)
        {
            originalMaterial = itemRenderer.material;
        }

        // Đảm bảo progressCircle được tắt ban đầu
        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0f; // Reset progress bar
            progressCircle.gameObject.SetActive(false); // Ẩn UI ban đầu
        }
    }

    private void Update()
    {
        if (isFlyingToVehicle)
        {
            FlyToVehicle();
            return;
        }

        if (!isLooted)
        {
            CheckLooting();
        }
    }

    private void CheckLooting()
    {
        // Kiểm tra xem có đối tượng nào trong phạm vi loot
        Collider[] colliders = Physics.OverlapSphere(transform.position, lootRange);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag(playerTag) || collider.CompareTag(enemyTag))
            {
                currentLooter = collider.transform;

                // Kiểm tra nếu looter đứng yên
                Rigidbody rb = currentLooter.GetComponent<Rigidbody>();
                if (rb != null && rb.velocity.magnitude < 0.1f)
                {
                    lootTimer += Time.deltaTime;

                    // Highlight và bật UI khi bắt đầu loot
                    if (!isLooting && lootTimer > 0f)
                    {
                        HighlightItem(true);
                        ShowProgressUI(true);
                        UpdateProgressCircle(lootTimer / lootTime);
                    }

                    if (lootTimer >= lootTime)
                    {
                        if (collider.CompareTag(playerTag))
                        {
                            LootByPlayer(currentLooter);
                        }
                        else if (collider.CompareTag(enemyTag))
                        {
                            LootByEnemy(currentLooter);
                        }
                    }
                }
                else
                {
                    // Reset nếu looter di chuyển
                    ResetLooting();
                }

                return; // Thoát vòng lặp sau khi tìm thấy một looter hợp lệ
            }
        }

        // Reset nếu không có ai trong phạm vi loot
        ResetLooting();
    }

    private void LootByPlayer(Transform looter)
    {
        isLooted = true;
        isLooting = true;
        HighlightItem(false);
        ShowProgressUI(false);

        // Item trở thành "child" của Player
        transform.SetParent(looter);
        transform.localPosition = new Vector3(0, 20, 20); // Vị trí relative (tùy chỉnh nếu cần)
        Debug.Log($"{looter.name} (Player) đã loot item!");
    }

    private void LootByEnemy(Transform looter)
    {
        isLooted = true;
        isLooting = true;
        HighlightItem(false);
        ShowProgressUI(false);
        Debug.Log($"{looter.name} (Enemy) đã loot item!");

        Destroy(gameObject); // Xóa item sau khi Enemy loot
    }

    private void FlyToVehicle()
    {
        // Di chuyển item về phía xe
        transform.position = Vector3.MoveTowards(transform.position, vehicleDropPoint.position, flySpeed * Time.deltaTime);
        Subject.NotifyObservers("AddMoney", new{Character = character, Price = price});
        // Kiểm tra nếu item đã đến xe
        if (Vector3.Distance(transform.position, vehicleDropPoint.position) < 0.1f)
        {

            DestroyItem();
        }
    }

    private void DestroyItem()
    {
        Debug.Log("Item đã đến xe và bị hủy!");
        Destroy(gameObject, destroyDelay);
    }

    void OnTriggerEnter(Collider other)
    {
        // Khi Player đến gần xe
        if(other!=null){Debug.Log(other.gameObject.name);}
        if (other.CompareTag("Car") && isLooted)
        {
            // Tách item khỏi Player
            transform.SetParent(null);
            currentLooter = null;
            // Bắt đầu bay về phía xe
            isFlyingToVehicle = true;
            Debug.Log("Item đang bay về xe...");
        }
    }

    private void HighlightItem(bool highlight)
    {
        if (itemRenderer != null)
        {
            // Thay đổi Material theo trạng thái highlight
            itemRenderer.material = highlight ? highlightMaterial : originalMaterial;
        }
    }

    private void ShowProgressUI(bool show)
    {
        if (progressCircle != null)
        {
            progressCircle.gameObject.SetActive(show);
        }
    }

    private void UpdateProgressCircle(float progress)
    {
        if (progressCircle != null)
        {
            progressCircle.fillAmount = progress; // Giá trị từ 0 (0%) đến 1 (100%)
        }
    }

    private void ResetLooting()
    {
        lootTimer = 0f;
        if (!isLooting)
        {
            HighlightItem(false);
            ShowProgressUI(false);
            UpdateProgressCircle(0f); // Reset progress bar
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn để biểu thị phạm vi loot trong Scene
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lootRange);
    }
}
