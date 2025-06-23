using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    [Header("Notification Settings")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationParent;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private int maxNotifications = 5;

    public static NotificationManager Instance { get; private set; }

    private Queue<NotificationUI> activeNotifications = new Queue<NotificationUI>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        // Remove oldest if at max
        if (activeNotifications.Count >= maxNotifications)
        {
            NotificationUI oldest = activeNotifications.Dequeue();
            if (oldest != null)
            {
                oldest.ForceClose();
            }
        }

        // Create new notification
        GameObject notificationGO = Instantiate(notificationPrefab, notificationParent);
        NotificationUI notification = notificationGO.GetComponent<NotificationUI>();

        if (notification != null)
        {
            notification.Initialize(message, type, notificationDuration);
            activeNotifications.Enqueue(notification);

            StartCoroutine(AutoCloseNotification(notification));
        }
    }

    private IEnumerator AutoCloseNotification(NotificationUI notification)
    {
        yield return new WaitForSeconds(notificationDuration);

        if (notification != null && activeNotifications.Contains(notification))
        {
            CloseNotification(notification);
        }
    }

    public void CloseNotification(NotificationUI notification)
    {
        if (activeNotifications.Contains(notification))
        {
            List<NotificationUI> tempList = new List<NotificationUI>(activeNotifications);
            activeNotifications.Clear();

            foreach (NotificationUI notif in tempList)
            {
                if (notif != notification)
                {
                    activeNotifications.Enqueue(notif);
                }
            }
        }

        if (notification != null)
        {
            notification.Close();
        }
    }
}