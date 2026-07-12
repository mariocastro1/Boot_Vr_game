using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SistemaAgarre : MonoBehaviour
{
    [Header("Configuración de Agarre")]
    public Transform holdPoint; 
    public float distanciaMaxima = 3.5f; 
    public LayerMask capaInteractuable; 
    public string capaSacar = "Sacar"; // Capa de la pila dispensadora

    [Header("Interfaz de Usuario (UI)")]
    public GameObject cartelGrab; 

    private Rigidbody objetoAgarrado;
    private ConfigurableJoint articulacionFisica;

    void Update()
    {
        // 1. CONTROL DEL CARTELITO
        if (objetoAgarrado == null) //
        {
            Ray rayo = new Ray(transform.position, transform.forward); //
            RaycastHit golpe; //

            if (Physics.Raycast(rayo, out golpe, distanciaMaxima, capaInteractuable)) //
            {
                if (golpe.collider.GetComponent<Rigidbody>() != null) //
                {
                    cartelGrab.SetActive(true); //
                }
                else
                {
                    cartelGrab.SetActive(false); //
                }
            }
            else
            {
                cartelGrab.SetActive(false); //
            }
        }
        else
        {
            cartelGrab.SetActive(false); //
        }

        // 2. DETECCIÓN DE LA TECLA 'E'
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) //
        {
            if (objetoAgarrado == null) //
            {
                Ray rayo = new Ray(transform.position, transform.forward); //
                RaycastHit golpe; //

                if (Physics.Raycast(rayo, out golpe, distanciaMaxima, capaInteractuable)) //
                {
                    IntentarAgarrar(golpe); //
                }
            }
            else
            {
                SoltarObjeto(); //
            }
        }
    }

    void IntentarAgarrar(RaycastHit golpe) //
    {
        Rigidbody rbOriginal = golpe.collider.GetComponent<Rigidbody>(); //
        if (rbOriginal != null) //
        {
            StopAllCoroutines(); //

            // ¿Estamos mirando la pila dispensadora (Capa Sacar)?
            if (golpe.collider.gameObject.layer == LayerMask.NameToLayer(capaSacar)) //
            {
                ContenedorIngrediente contenedor = golpe.collider.GetComponent<ContenedorIngrediente>(); //
                
                if (contenedor != null && contenedor.prefabAClonar != null) //
                {
                    // Instanciamos el Prefab limpio directamente en la posición del holdPoint en el mundo
                    GameObject clon = Instantiate(contenedor.prefabAClonar, holdPoint.position, holdPoint.rotation); //
                    
                    // Forzamos a que use la capa Default
                    clon.layer = LayerMask.NameToLayer("Default"); //
                    clon.tag = "Untagged"; //

                    objetoAgarrado = clon.GetComponent<Rigidbody>(); //
                }
                else
                {
                    Debug.LogWarning("La pila no tiene asignado el Prefab en el componente ContenedorIngrediente."); //
                    return; //
                }
            }
            else
            {
                // Si miramos un tomate normal en la mesa (Capa Default), lo agarramos directamente
                objetoAgarrado = rbOriginal; //
            }

            // Independizamos el objeto para moverlo con la mano
            objetoAgarrado.transform.SetParent(null); //

            objetoAgarrado.isKinematic = false; //
            objetoAgarrado.useGravity = false; //

            Collider col = objetoAgarrado.GetComponent<Collider>(); //
            if (col != null) //
            {
                col.enabled = false; // Apagamos colisiones temporalmente en la mano
            }

            articulacionFisica = holdPoint.gameObject.AddComponent<ConfigurableJoint>(); //
            articulacionFisica.autoConfigureConnectedAnchor = false; //

            Rigidbody rbMano = holdPoint.GetComponent<Rigidbody>(); //
            if (rbMano != null) //
            {
                articulacionFisica.connectedBody = objetoAgarrado; //
            }

            articulacionFisica.anchor = Vector3.zero; //
            articulacionFisica.connectedAnchor = Vector3.zero; //

            ConfigurarArticulacion(); //
        }
    }

    void ConfigurarArticulacion() //
    {
        if (articulacionFisica == null) return; //

        articulacionFisica.xMotion = ConfigurableJointMotion.Locked; //
        articulacionFisica.yMotion = ConfigurableJointMotion.Locked; //
        articulacionFisica.zMotion = ConfigurableJointMotion.Locked; //

        articulacionFisica.angularXMotion = ConfigurableJointMotion.Locked; //
        articulacionFisica.angularYMotion = ConfigurableJointMotion.Locked; //
        articulacionFisica.angularZMotion = ConfigurableJointMotion.Locked; //

        JointDrive drive = new JointDrive(); //
        drive.positionSpring = 0f; //
        drive.positionDamper = 0f; //
        drive.maximumForce = 0f; //
        
        articulacionFisica.xDrive = drive; //
        articulacionFisica.yDrive = drive; //
        articulacionFisica.zDrive = drive; //
    }

    void SoltarObjeto() //
    {
        if (articulacionFisica != null) //
        {
            Destroy(articulacionFisica); //
        }

        if (objetoAgarrado != null) //
        {
            Collider col = objetoAgarrado.GetComponent<Collider>(); //
            if (col != null) //
            {
                col.enabled = true; // RE-ACTIVACIÓN OBLIGATORIA DEL COLLIDER AL SOLTAR
            }

            objetoAgarrado.linearVelocity = Vector3.zero; //
            objetoAgarrado.angularVelocity = Vector3.zero; //

            objetoAgarrado.isKinematic = false; //
            objetoAgarrado.useGravity = true; //

            StartCoroutine(EsperarYFijarObjeto(objetoAgarrado)); //

            objetoAgarrado = null; //
        }
    }

    IEnumerator EsperarYFijarObjeto(Rigidbody rb) //
    {
        yield return new WaitForSeconds(0.15f); //

        while (rb != null && rb.linearVelocity.magnitude > 0.05f) //
        {
            yield return null; //
        }

        if (rb != null) //
        {
            rb.linearVelocity = Vector3.zero; //
            rb.angularVelocity = Vector3.zero; //

            RaycastHit hitMesa; //
            // Si el rayo detecta la mesa o el barco abajo, se ancla a él para moverse con las olas
            if (Physics.Raycast(rb.transform.position, Vector3.down, out hitMesa, 3.0f)) //
            {
                rb.transform.SetParent(hitMesa.transform); //
                rb.isKinematic = true; // Se congela SOLIDARIO al barco
            }
        }
    }
}