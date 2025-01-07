using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IObserver
{
    public int Money;
    public string Name;
    public CharacterType Type;
    void Awake()
    {
        Subject.RegisterObserver(this);
    }
    void OnDestroy()
    {
        Subject.UnregisterObserver(this);
    }
    public void OnNotify(string eventName, object eventData)
    {
        if (eventName == "AddMoney")
        {
            // Kiểm tra xem dữ liệu có chứa thông tin Character và Price
            Debug.Log("tiền về");
            var eventInfo = eventData as dynamic;
            if (eventInfo != null)
            {
                // Kiểm tra xem Character trong eventData có phải là đối tượng hiện tại hay không
                if (eventInfo.Character == this)
                {
                    int price = eventInfo.Price;
                    AddMoney(price);
                }
            }
        }
    }
    public void AddMoney(int money)
    {
        Money += money;
        Subject.NotifyObservers("updateRank");
    }

}
public enum CharacterType
{
    Blue,
    Red,
    Pink,
    Yellow
}

