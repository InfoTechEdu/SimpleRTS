using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float startingHealth = 100f;               
    public Slider slider;                             
    public Image fillImage;                           
    public Color fullHealthColor = Color.green;       
    public Color zeroHealthColor = Color.red;               

    [SerializeField] private float currentHealth;                 
    private bool isDead;

    public Action<Transform> onDeathCallback;

    public float CurrentHealthDebug { get => currentHealth; set => currentHealth = value; }
    public bool IsDead { get => isDead; set => isDead = value; }

    private void OnEnable()
    {
        currentHealth = startingHealth;

        slider.maxValue = startingHealth;

        isDead = false;

        SetHealthUI();
    }
    private void Update()
    {
        
    }

    public void SetOnDeadCallback(Action<Transform> onDeath)
    {
        onDeathCallback = onDeath;
    }
    public void Treat(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, startingHealth);

        SetHealthUI();
    }
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        SetHealthUI();

        if (currentHealth <= 0f && !isDead)
        {
            Debug.Log("Dead unit - " + gameObject.name);
            OnDeath();
        }
    }
    private void SetHealthUI()
    {
        slider.value = currentHealth;

        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, currentHealth / startingHealth);
    }
    private void OnDeath()
    {
        isDead = true;

        onDeathCallback?.Invoke(transform);

        Destroy(gameObject);
    }
}
