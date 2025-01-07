using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour , IObserver
{ 
    [Header("Loot Settings")]
    [SerializeField] private int price;
    public Character character;
    [SerializeField] private float lootRange = 2f; // Khoảng cách để bắt đầu loot
    [SerializeField] private float lootTime = 2f; // Thời gian đứng yên để loot
    [SerializeField] private string playerTag = "Player"; // Tag để xác định Player
    [SerializeField] private string enemyTag = "Enemy";   // Tag để xác định Enemy


    [Header("Item Visual Settings")]
    [SerializeField] private Renderer itemRenderer;       // Renderer của item để highlight
    [SerializeField] private Rigidbody  rb;
    private Material originalMaterial; // Lưu Material gốc để hoàn nguyên

    [Header("Vehicle Settings")]
    [SerializeField] private Transform vehicleDropPoint; // Vị trí trên xe để item bay đến
    [SerializeField] private float flySpeed = 5f;        // Tốc độ bay về xe
    [SerializeField] private float destroyDelay = 0.1f;  // Thời gian delay trước khi hủy item

    [SerializeField] private bool isLooted = false;         // Trạng thái đã loot
    [SerializeField] private bool isLooting = false;        // Đang bị loot hay không
    [SerializeField] private bool isFlyingToVehicle = false; // Trạng thái đang bay đến xe
    private float lootTimer = 0f;          // Bộ đếm thời gian loot
    private Transform currentLooter = null; // Đối tượng đang cố loot item
    private Transform mainCameraTransform;
     private void Awake()
    {
        Subject.RegisterObserver(this); // Đăng ký observer   
         // Cache lại Camera.main để tăng hiệu năng
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }
    private void OnDestroy()
    {
   
        Subject.UnregisterObserver(this); // Hủy đăng ký observer

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
        vehicleDropPoint = GameObject.Find("Car").transform;
        rb = GetComponent<Rigidbody>();
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
    [SerializeField] private Image progressCircle; // Progress Circle riêng của item
    private void LateUpdate()
    {
       // Kiểm tra nếu progressCircle hoặc camera bị null
        if (progressCircle == null || mainCameraTransform == null)
        {
            return;
        }

        // Luôn hướng về phía camera
        progressCircle.transform.LookAt(mainCameraTransform);
        progressCircle.transform.Rotate(0, 180, 0);
    }
    private bool isBeingLooted = false; // Kiểm tra nếu item đang được loot
    private void CheckLooting()
    {
        // Kiểm tra xem có đối tượng nào trong phạm vi loot
        if (isBeingLooted)
        {
            return;
        }
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
                        HighlightItem(true, collider.gameObject.GetComponent<Character>());
                        ShowProgressUI(true);
                        UpdateProgressCircle(lootTimer / lootTime);
                    }

                    if (lootTimer >= lootTime)
                    {
                        Loot(currentLooter);
                        isBeingLooted = true; // Đánh dấu item đang bị loot
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

    [SerializeField] private Material[] highlightMaterial;  // Material highlight
    private void Loot(Transform looter)
    {
        transform.SetParent(looter);
        character  = GetComponentInParent<Character>();
        isLooted = true;
        isLooting = true;
        rb.useGravity = false;
        rb.mass = 0.01f;
        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        HighlightItem(false);
        ShowProgressUI(false);
        // Item trở thành "child" của Player
        transform.localPosition = new Vector3(0, 20, 20); // Vị trí relative (tùy chỉnh nếu cần)
    }

   
    private bool hasNotified = false;
    private void FlyToVehicle()
    {
        // Di chuyển item về phía xe
        transform.position = Vector3.MoveTowards(transform.position, vehicleDropPoint.position, flySpeed * Time.deltaTime);
        if (!hasNotified)
        {
            Subject.NotifyObservers("AddMoney", new { Character = character, Price = price });
            hasNotified = true;
        }

        // Kiểm tra nếu item đã đến xe
        if (Vector3.Distance(transform.position, vehicleDropPoint.position) < 50f)
        {
            DestroyItem();
        }
    }

    private void DestroyItem()
    {
        Destroy(gameObject, destroyDelay);
    }

    void OnTriggerEnter(Collider other)
    {
        // Khi Player đến gần xe
        if (other.CompareTag("Car") && isLooted)
        {
            // Tách item khỏi Player
            transform.SetParent(null);
            currentLooter = null;

            // Cho phép item được loot lại sau khi bay về xe
            isBeingLooted = false;

            // Bắt đầu bay về phía xe
            isFlyingToVehicle = true;
        }
    }

    private void HighlightItem(bool highlight, Character character = null)
    {
        if (itemRenderer != null)
        {
            // Nếu highlight là true, chọn Material dựa trên CharacterType
            if (highlight && character != null)
            {
                switch (character.Type)
                {
                    case CharacterType.Blue:
                        itemRenderer.material = highlightMaterial[0]; // Blue Material
                        break;
                    case CharacterType.Red:
                        itemRenderer.material = highlightMaterial[1]; // Red Material
                        break;
                    case CharacterType.Pink:
                        itemRenderer.material = highlightMaterial[2]; // Pink Material
                        break;
                    case CharacterType.Yellow:
                        itemRenderer.material = highlightMaterial[3]; // Yellow Material
                        break;
                    default:
                        itemRenderer.material = originalMaterial; // Default nếu không khớp
                        break;
                }
            }
            else
            {
                // Nếu không highlight, quay lại Material gốc
                itemRenderer.material = originalMaterial;
            }
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
        isLooting = false;

        if (currentLooter != null)
        {
            HighlightItem(false, currentLooter.GetComponent<Character>());
        }

        currentLooter = null;
        ShowProgressUI(false);

        // Reset trạng thái bị loot nếu không ai đang loot
        isBeingLooted = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn để biểu thị phạm vi loot trong Scene
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lootRange);
    }
}
