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
    public Transform objetoBarco; 

    private CharacterController controller;
    private float rotacionX = 0;
    private float rotacionY = 0;
    private bool juegoCongelado = false;
    private float velocidadVertical = 0f; 

    private float alturaOriginalCuerpo = 5.0f;
    private float alturaOriginalCamaraY = 0f;
    private bool estaAgachado = false;

    
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
        
        if (Keyboard.current != null && Keyboard.current.backslashKey.wasPressedThisFrame) 
        {
            modoActual = (modoActual == ModoCamara.Persona) ? ModoCamara.NoClip : ModoCamara.Persona; 
            if (modoActual == ModoCamara.NoClip) 
            {
                velocidadVertical = 0f; 
                RestaurarAlturaNormal();
                transform.SetParent(null); 
            }
        }

        
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

        
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked) 
        {
            Vector2 deltaRaton = Mouse.current.delta.ReadValue(); 
            float ratonX = deltaRaton.x * sensibilidadRaton * 0.01f; 
            float ratonY = deltaRaton.y * sensibilidadRaton * 0.01f; 

            rotacionY += ratonX; 
            rotacionX -= ratonY; 
            rotacionX = Mathf.Clamp(rotacionX, -90f, 90f); 

            
            
            transform.rotation = Quaternion.Euler(0, rotacionY, 0);  
            
            
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

        
        if (controller.isGrounded) 
        {
            velocidadVertical = -1.5f; 

            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed && !estaAgachado) 
            {
                
                transform.SetParent(null);
                velocidadVertical = fuerzaSalto; 
            }
        }
        else 
        {
            velocidadVertical -= gravedad * Time.deltaTime; 
            
            
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
        }

        movimientoFinal.y += velocidadVertical * Time.deltaTime; 

        
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

    
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
        if (modoActual == ModoCamara.Persona && hit.normal.y > 0.6f)
        {
            
            if (hit.transform == objetoBarco || hit.transform.IsChildOf(objetoBarco))
            {
                
                if (transform.parent != hit.transform)
                {
                    
                    transform.SetParent(objetoBarco);
                }
            }
            else
            {
                
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }
            }
        }
    }
}