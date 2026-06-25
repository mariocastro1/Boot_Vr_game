using UnityEngine;
using UnityEngine.InputSystem; 

public class MovimientoCamara : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 3.0f; 
    
    [Range(0.01f, 2.0f)]
    public float sensibilidadRaton = 0.5f; 

    private float rotacionX = 0;
    private float rotacionY = 0;
    private bool juegoCongelado = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        rotacionY = transform.localRotation.eulerAngles.y;
        rotacionX = transform.localRotation.eulerAngles.x;
    }

    void Update()
    {
        // Alternar el estado de congelación al pulsar Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            juegoCongelado = !juegoCongelado;
            Cursor.lockState = juegoCongelado ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // Si el juego está congelado (has pulsado Escape), paramos por completo este Update
        if (juegoCongelado) return;

        // Si haces clic en la pantalla de juego, volvemos a bloquear el ratón automáticamente
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            juegoCongelado = false;
        }

        // 1. MOVIMIENTO (Solo funciona si no está congelado)
        Vector3 direccion = Vector3.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) direccion += transform.forward;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) direccion -= transform.forward;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direccion -= transform.right;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direccion += transform.right;

            direccion.y = 0; 

            if (Keyboard.current.spaceKey.isPressed) direccion += Vector3.up;
            if (Keyboard.current.leftCtrlKey.isPressed) direccion += Vector3.down;
        }

        transform.position += direccion.normalized * velocidad * Time.deltaTime;

        // 2. ROTACIÓN (Solo funciona si no está congelado y el ratón está bloqueado)
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 deltaRaton = Mouse.current.delta.ReadValue();
            
            float ratonX = deltaRaton.x * sensibilidadRaton * 0.01f;
            float ratonY = deltaRaton.y * sensibilidadRaton * 0.01f;

            rotacionY += ratonX;
            rotacionX -= ratonY;
            rotacionX = Mathf.Clamp(rotacionX, -90f, 90f); 

            transform.localRotation = Quaternion.Euler(rotacionX, rotacionY, 0);
        }
    }
}