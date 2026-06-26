using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(CharacterController))] 
public class MovimientoCamara : MonoBehaviour
{
    public enum ModoCamara { Persona, NoClip }

    [Header("Configuración General")]
    public ModoCamara modoActual = ModoCamara.Persona; 
    public float velocidadCaminar = 3.0f;
    public float velocidadNoClip = 5.0f;
    [Range(0.01f, 2.0f)]
    public float sensibilidadRaton = 1.0f; 

    [Header("Físicas Modo Persona")]
    public float gravedad = 9.81f;
    public float fuerzaSalto = 4.0f; 

    [Header("Configuración de Agacharse")]
    public float velocidadAgachado = 1.5f; // Velocidad reducida al caminar agachado

    [Header("Sincronización con el Barco")]
    public Transform objetoBarco; // 🌟 ARRASTRA AQUÍ TU OBJETO "boot" DESDE EL INSPECTOR

    private CharacterController controller;
    private float rotacionX = 0;
    private float rotacionY = 0;
    private bool juegoCongelado = false;
    private float velocidadVertical = 0f; 

    // Guardar las alturas originales para poder volver a ellas al soltar la tecla
    private float alturaOriginalCuerpo = 5.0f;
    private float alturaOriginalCamaraY = 0f;
    private bool estaAgachado = false;

    // Variables internas para rastrear el movimiento del barco
    private Vector3 ultimaPosicionBarco;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Mantenemos tus valores de altura estables que ya te funcionan bien
        controller.height = 5.0f;
        controller.center = Vector3.zero;
        alturaOriginalCuerpo = controller.height;

        Cursor.lockState = CursorLockMode.Locked;
        
        rotacionY = transform.localRotation.eulerAngles.y;
        rotacionX = transform.localRotation.eulerAngles.x;

        // Recordar la posición Y inicial que tiene tu cámara hija en el Inspector
        if (transform.childCount > 0)
        {
            alturaOriginalCamaraY = transform.GetChild(0).localPosition.y;
        }

        // Inicializamos la posición del barco si ya está asignado
        if (objetoBarco != null)
        {
            ultimaPosicionBarco = objetoBarco.position;
        }
    }

    // 🌟 Se ejecuta en LateUpdate para que vaya DESPUÉS de que el barco se mueva con el agua
    void LateUpdate()
    {
        // 1. ALTERNAR MODO (Tecla º)
        if (Keyboard.current != null && Keyboard.current.backslashKey.wasPressedThisFrame)
        {
            modoActual = (modoActual == ModoCamara.Persona) ? ModoCamara.NoClip : ModoCamara.Persona;
            if (modoActual == ModoCamara.NoClip)
            {
                velocidadVertical = 0f;
                // Si pasamos a NoClip, nos aseguramos de que el cuerpo recupere su altura normal
                RestaurarAlturaNormal();
            }
            Debug.Log($"[Cámara] Modo cambiado a: {modoActual}");
        }

        // Alternar el estado de congelación al pulsar Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            juegoCongelado = !juegoCongelado;
            Cursor.lockState = juegoCongelado ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (juegoCongelado) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            juegoCongelado = false;
        }

        // 2. ROTACIÓN DEL RATÓN
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 deltaRaton = Mouse.current.delta.ReadValue();
            float ratonX = deltaRaton.x * sensibilidadRaton * 0.01f;
            float ratonY = deltaRaton.y * sensibilidadRaton * 0.01f;

            rotacionY += ratonX;
            rotacionX -= ratonY;
            rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);

            // El objeto principal (Jugador_Fisico) solo gira hacia los lados (Y)
            transform.localRotation = Quaternion.Euler(0, rotacionY, 0); 
            
            // La cámara real (que es el primer hijo) mira hacia arriba y abajo (X)
            if (transform.childCount > 0)
            {
                transform.GetChild(0).localRotation = Quaternion.Euler(rotacionX, 0, 0); 
            }
        }

        // 3. MOVIMIENTO SEGÚN EL MODO ACTIVO
        if (modoActual == ModoCamara.Persona)
        {
            ManejarAgachado(); // Gestiona el tamaño físico antes de desplazar al personaje
            ManejarModoPersona();
        }
        else
        {
            ManejarModoNoClip();
        }

        // 🌟 Guardamos la posición que ha tomado el barco al final de este fotograma
        if (objetoBarco != null)
        {
            ultimaPosicionBarco = objetoBarco.position;
        }
    }

    void ManejarAgachado()
    {
        if (Keyboard.current == null) return;

        // Si se mantiene pulsado el Control Izquierdo, nos agachamos
        if (Keyboard.current.leftCtrlKey.isPressed)
        {
            if (!estaAgachado)
            {
                estaAgachado = true;
                // Reducimos el CharacterController a la mitad de su altura original
                controller.height = alturaOriginalCuerpo * 0.5f;
                
                // Bajamos la vista de la cámara a la mitad de su altura local relativa
                if (transform.childCount > 0)
                {
                    Transform camara = transform.GetChild(0);
                    camara.localPosition = new Vector3(camara.localPosition.x, alturaOriginalCamaraY * 0.5f, camara.localPosition.z);
                }
            }
        }
        else
        {
            // Al soltar la tecla, volvemos al estado normal
            if (estaAgachado)
            {
                RestaurarAlturaNormal();
            }
        }
    }

    void RestaurarAlturaNormal()
    {
        estaAgachado = false;
        controller.height = alturaOriginalCuerpo;
        
        if (transform.childCount > 0)
        {
            Transform camara = transform.GetChild(0);
            camara.localPosition = new Vector3(camara.localPosition.x, alturaOriginalCamaraY, camara.localPosition.z);
        }
    }

    void ManejarModoPersona()
    {
        Vector3 movimientoFinal = Vector3.zero;

        // 🌟 A) Calcular cuánto se ha desplazado el barco en este fotograma debido a las olas
        if (objetoBarco != null)
        {
            Vector3 desplazamientoBarco = objetoBarco.position - ultimaPosicionBarco;
            movimientoFinal += desplazamientoBarco; // Añadimos el empuje del barco al movimiento de la cámara
        }

        // B) Input del jugador (Caminar)
        Vector3 direccionInput = Vector3.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) direccionInput += transform.forward;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) direccionInput -= transform.forward;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direccionInput -= transform.right;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direccionInput += transform.right;
        }

        direccionInput.y = 0; 

        // Modificador de velocidad: si estás agachado vas lento, si estás de pie y pulsas Shift vas con turbo
        float velocidadActual = velocidadCaminar;
        if (estaAgachado)
        {
            velocidadActual = velocidadAgachado;
        }
        else if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
        {
            velocidadActual *= 3.0f; 
        }

        direccionInput = direccionInput.normalized * velocidadActual * Time.deltaTime;
        movimientoFinal += direccionInput; // Sumamos tu velocidad al caminar

        // C) Físicas, Gravedad y Salto
        if (controller.isGrounded)
        {
            velocidadVertical = -0.5f;

            // Bloqueamos el salto si estás agachado por realismo técnico
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed && !estaAgachado)
            {
                velocidadVertical = fuerzaSalto;
            }
        }
        else
        {
            velocidadVertical -= gravedad * Time.deltaTime;
        }

        movimientoFinal.y += velocidadVertical * Time.deltaTime;

        // Aplicamos el movimiento total acumulado (Empuje de olas + Tu movimiento + Gravedad)
        controller.Move(movimientoFinal);
    }

    void ManejarModoNoClip()
    {
        Vector3 direccion = Vector3.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) direccion += transform.forward;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) direccion -= transform.forward;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direccion -= transform.right;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direccion += transform.right;

            direccion.y = 0;

            if (Keyboard.current.spaceKey.isPressed) direccion += Vector3.up;
            if (Keyboard.current.leftCtrlKey.isPressed) direccion += Vector3.down; // Sigue bajando el NoClip con normalidad
        }

        // Multiplicador Shift (Turbo)
        float velocidadActual = velocidadNoClip;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
        {
            velocidadActual *= 3.0f; 
        }

        // En NoClip se ignora por completo el desplazamiento del barco
        transform.position += direccion.normalized * velocidadActual * Time.deltaTime;
    }
}