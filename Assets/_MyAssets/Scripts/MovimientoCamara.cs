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
    public float velocidadAgachado = 1.5f; 

    [Header("Sincronización con el Barco")]
    public Transform objetoBarco; // Tu objeto base del barco ("Base_barco" o "boot")

    private CharacterController controller;
    private float rotacionX = 0;
    private float rotacionY = 0;
    private bool juegoCongelado = false;
    private float velocidadVertical = 0f; 

    private float alturaOriginalCuerpo = 5.0f;
    private float alturaOriginalCamaraY = 0f;
    private bool estaAgachado = false;

    // Variables internas para controlar si estamos sobre el barco
    private bool sobreElBarco = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        controller.height = 5.0f; 
        controller.center = Vector3.zero; 
        alturaOriginalCuerpo = controller.height;

        Physics.queriesHitTriggers = false; 

        Cursor.lockState = CursorLockMode.Locked; 
        
        rotacionY = transform.localRotation.eulerAngles.y; 
        rotacionX = transform.localRotation.eulerAngles.x; 

        if (transform.childCount > 0) 
        {
            alturaOriginalCamaraY = transform.GetChild(0).localPosition.y; 
        }
    }

    void Update()
    {
        // Alternar modo de cámara (Tecla º)
        if (Keyboard.current != null && Keyboard.current.backslashKey.wasPressedThisFrame) 
        {
            modoActual = (modoActual == ModoCamara.Persona) ? ModoCamara.NoClip : ModoCamara.Persona; 
            if (modoActual == ModoCamara.NoClip) 
            {
                velocidadVertical = 0f; 
                RestaurarAlturaNormal();
                transform.SetParent(null); // En NoClip nos desvinculamos de las coordenadas del barco
            }
        }

        // Alternar pausa (Escape)
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

        // PROCESAR MOVIMIENTO DEL JUGADOR
        if (modoActual == ModoCamara.Persona) 
        {
            ManejarAgachado(); 
            ManejarModoPersona(); 
        }
        else
        {
            ManejarModoNoClip(); 
        }
    }

void LateUpdate()
    {
        if (juegoCongelado) return;

        // ROTACIÓN DEL RATÓN CORREGIDA (Evita heredar los 90º del eje del barco)
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked) 
        {
            Vector2 deltaRaton = Mouse.current.delta.ReadValue(); 
            float ratonX = deltaRaton.x * sensibilidadRaton * 0.01f; 
            float ratonY = deltaRaton.y * sensibilidadRaton * 0.01f; 

            rotacionY += ratonX; 
            rotacionX -= ratonY; 
            rotacionX = Mathf.Clamp(rotacionX, -90f, 90f); 

            // 🌟 LA CORRECCIÓN MAESTRA: Aplica la rotación absoluta en el mundo real.
            // Esto ignora por completo si el objeto 'boot' o 'Base_barco' está girado en el editor.
            transform.rotation = Quaternion.Euler(0, rotacionY, 0);  
            
            // Rotación vertical de la mirada (Cámara hija)
            if (transform.childCount > 0) 
            {
                transform.GetChild(0).localRotation = Quaternion.Euler(rotacionX, 0, 0);  
            }
        }
    }

    void ManejarAgachado()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.leftCtrlKey.isPressed)
        {
            if (!estaAgachado)
            {
                estaAgachado = true;
                controller.height = alturaOriginalCuerpo * 0.5f;
                
                if (transform.childCount > 0)
                {
                    Transform camara = transform.GetChild(0);
                    camara.localPosition = new Vector3(camara.localPosition.x, alturaOriginalCamaraY * 0.5f, camara.localPosition.z);
                }
            }
        }
        else
        {
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

        // Input de caminar (Movimiento relativo a la orientación actual)
        Vector3 direccionInput = Vector3.zero; 

        if (Keyboard.current != null) 
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) direccionInput += transform.forward; 
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) direccionInput -= transform.forward; 
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direccionInput -= transform.right; 
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direccionInput += transform.right; 
        }

        direccionInput.y = 0;  

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
        movimientoFinal += direccionInput; 

        // GESTIÓN DE FÍSICAS VERTICALES, GRAVEDAD Y SALTO
        if (controller.isGrounded) 
        {
            velocidadVertical = -1.5f; // Fuerza constante hacia abajo para mantener el contacto firme con el suelo

            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed && !estaAgachado) 
            {
                // Al saltar, nos desvinculamos inmediatamente del padre para volar libremente por el aire
                transform.SetParent(null);
                velocidadVertical = fuerzaSalto; 
            }
        }
        else 
        {
            velocidadVertical -= gravedad * Time.deltaTime; 
            
            // Si caemos o estamos en el aire por cualquier motivo, aseguramos que no tenemos padre
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
        }

        movimientoFinal.y += velocidadVertical * Time.deltaTime; 

        // Aplicamos nuestro movimiento e inputs nativos
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
            if (Keyboard.current.leftCtrlKey.isPressed) direccion += Vector3.down; 
        }

        float velocidadActual = velocidadNoClip; 
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) 
        {
            velocidadActual *= 3.0f;  
        }

        transform.position += direccion.normalized * velocidadActual * Time.deltaTime; 
    }

    // 🌟 REGLA DE ORO: DETECCIÓN DE CONTACTO DIRECTO CON EL SUELO DEL BARCO
    // Esta función interna de Unity se dispara automáticamente cuando el CharacterController colisiona con algo
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Comprobamos si estamos en Modo Persona y si el objeto que pisamos está por debajo de nosotros (el suelo)
        if (modoActual == ModoCamara.Persona && hit.normal.y > 0.6f)
        {
            // Verificamos si lo que estamos pisando es el barco o cualquier parte dentro de su estructura física
            if (hit.transform == objetoBarco || hit.transform.IsChildOf(objetoBarco))
            {
                // Si aún no somos hijos, nos emparentamos directamente con el barco
                if (transform.parent != hit.transform)
                {
                    // Al poner el transform del golpe como padre, heredamos sus coordenadas globales de posición y rotación exactas
                    transform.SetParent(objetoBarco);
                }
            }
            else
            {
                // Si pisamos tierra firme u otro objeto fuera del barco, nos desvinculamos
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }
            }
        }
    }
}