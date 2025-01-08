using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class Enemy : Character , IObserver
{
    public Transform carPosition; // Vị trí của xe
    public float lootRange = 50f; // Phạm vi loot
    public float lootTime = 3f; // Thời gian loot
    private NavMeshAgent agent;
    public Transform currentItem;
    public bool letMoving;
    public Animator anim; // Animator của nhân vật
    [SerializeField] private bool isReturningToCar = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Lấy NavMeshAgent để di chuyển
        letMoving = false;
    }
    public override void OnNotify(string eventName, object eventData)
    {
        base.OnNotify(eventName, eventData);
        if(eventName == "StartGame")
        {
            letMoving = true ;
        }
        if(eventName == "End")
        {
            letMoving = false;
        }
    }
    void Update()
    {
        if(!letMoving) return;
        // Điều khiển animation dựa trên trạng thái di chuyển
        bool isMoving = agent.velocity.magnitude > 0.1f;
        anim.SetBool("run", isMoving);

        // Nếu đã loot xong item, di chuyển về xe
        if (lootedItem != null)
        {
            if (!isReturningToCar)
            {
                MoveToCar();
            }
            return;
        }else
        {
           if(isReturningToCar)
           {
                isReturningToCar = false;
           }
            // Nếu có mục tiêu hiện tại, di chuyển đến đó
            if (currentItem != null)
            {
                MoveToItem(currentItem);
            }
            else
            {
                // Tìm item ngẫu nhiên để loot
                FindRandomItem();
            }
        }
    }
    // Tìm item ngẫu nhiên để loot
    private void FindRandomItem()
    {
        Item[] items = GameObject.FindObjectsOfType<Item>();

        // Lọc ra các item chưa bị loot
        List<Item> availableItems = new List<Item>();
        foreach (var item in items)
        {
            if (!item.isLooted)
            {
                availableItems.Add(item);
            }
        }

        if (availableItems.Count > 0)
        {
            // Chọn item ngẫu nhiên
            currentItem = availableItems[Random.Range(0, availableItems.Count)].transform;

            // Di chuyển đến mục tiêu
            agent.SetDestination(currentItem.position);
            agent.isStopped = false;
        }
        else
        {
            currentItem = null;
            agent.isStopped = true; // Đứng yên nếu không có item nào để loot
        }
    }

    // Di chuyển đến item
    private void MoveToItem(Transform item)
    {
        if (item == null || item.GetComponent<Item>().isLooted)
        {
            currentItem = null;
            FindRandomItem(); // Tìm item khác
            return;
        }

        // Di chuyển đến item
        if (!agent.pathPending && agent.remainingDistance > lootRange)
        {
            agent.SetDestination(item.position);
        }
    }

    // Khi chạm vào item, bắt đầu loot
    private void OnTriggerEnter(Collider other)
    {
       if (currentItem != null && other.transform == currentItem)
        {   
          
            agent.isStopped = true; // Dừng di chuyển
            agent.velocity = Vector3.zero;    // Loại bỏ quán tính hoàn toàn
        }
    }
    // Di chuyển về vị trí xe
    private void MoveToCar()
    {
        isReturningToCar = true;
        agent.isStopped = false;
        agent.SetDestination(carPosition.position);
    }
  
}
