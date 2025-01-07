using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Enemy : Character
{
     public Transform carPosition; // Vị trí của xe
    public float lootRange = 2f; // Phạm vi loot
    public float lootTime = 3f; // Thời gian loot
    private bool isLooting = false;
    private bool hasLooted = false;
    private NavMeshAgent agent;
    private Transform currentItem;
    private float lootTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Lấy NavMeshAgent để di chuyển
    }

    void Update()
    {
        if (hasLooted) return;

        if (isLooting)
        {
            LootItem();
        }
        else
        {
            // Nếu chưa loot, di chuyển đến item
            if (currentItem != null)
            {
                MoveToItem(currentItem);
            }
            else
            {
                // Tìm item gần nhất để loot
                FindNearestItem();
            }
        }
    }

    // Tìm item gần nhất
    private void FindNearestItem()
    {
       // Lấy tất cả các đối tượng có kiểu Item
        Item[] items = GameObject.FindObjectsOfType<Item>();
    
        if (items.Length > 0)
        {
            // Chọn ngẫu nhiên một đối tượng Item
            Item randomItem = items[Random.Range(0, items.Length)];

            // Di chuyển đến đối tượng Item ngẫu nhiên
            currentItem = randomItem.transform;
            agent.SetDestination(currentItem.position);
        }
        else
        {
            Debug.LogWarning("No items found in the scene.");
        }
    }

    // Di chuyển đến item
    private void MoveToItem(Transform item)
    {
        if (Vector3.Distance(transform.position, item.position) <= lootRange)
        {
            // Đến gần item, bắt đầu loot
            StartLooting();
        }
        else
        {
            agent.SetDestination(item.position);
        }
    }

    // Bắt đầu loot item
    private void StartLooting()
    {
        isLooting = true;
        lootTimer = 0f;
        // Bật UI loot và các hiệu ứng khác nếu cần
    }

    // Thực hiện loot item
    private void LootItem()
    {
        lootTimer += Time.deltaTime;

        if (lootTimer >= lootTime)
        {
            // Loot xong
            hasLooted = true;
            isLooting = false;

            // Di chuyển đến xe
            agent.SetDestination(carPosition.position);

            // Bật UI loot thành công nếu cần
        }
    }

    // Hiển thị UI loot
  
}
