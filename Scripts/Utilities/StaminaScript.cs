using UnityEngine;

namespace FabioSenoDev
{
    public class StaminaScript : MonoBehaviour
    {
        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float regenDelay = 1f;
        [SerializeField] private float currentStamina;

        private float regenTimer;

        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public float NormilisedStamina => maxStamina <= 0f ? 0f : currentStamina / maxStamina;


        private void Awake()
        {
            maxStamina = Mathf.Max(0f, maxStamina);
            staminaRegenRate = Mathf.Max(0f, staminaRegenRate);
            regenDelay = Mathf.Max(0f, regenDelay);
            currentStamina = Mathf.Clamp(currentStamina <= 0f ? maxStamina : currentStamina, 0f, maxStamina);
        }

        private void Update()
        {
            if (regenTimer > 0f)
            {
                regenTimer -= Time.deltaTime;
                return;
            }

            if (currentStamina >= maxStamina)
            {
                return;
            }

            currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
        }

        public bool UseStamina(float amount)
        {
            if(amount <= 0f)
            {
                return true;
            }
            if ( currentStamina < amount)
            {
                return false;
            }

            currentStamina -= amount;
            regenTimer = regenDelay;
            return true;

        }
        public void RestoreStamina(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        }
        public void ResetToFull()
        {
            currentStamina = maxStamina;
            regenTimer = 0f;
        }
    }
}
