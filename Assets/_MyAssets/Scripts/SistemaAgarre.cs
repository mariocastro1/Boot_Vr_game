using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Necesario para usar Coroutines (temporizadores)

public class SistemaAgarre : MonoBehaviour
{
    [Header("Configuración de Agarre")]
    public Transform holdPoint; 
    public float distanciaMaxima = 3.5f; 
    public LayerMask capaInteractuable; 

    [Header("Interfaz de Usuario (UI)")]
    public GameObject cartelGrab; 

    private Rigidbody objetoAgarrado;
    private ConfigurableJoint articulacionFisica;

    void Update()
    {
        // 1. CONTROL DEL CARTELITO
        if (objetoAgarrado == null)
        {
            Ray rayo = new Ray(transform.position, transform.forward);
            RaycastHit golpe;

            if (Physics.Raycast(rayo, out golpe, distanciaMaxima, capaInteractuable))
            {
                if (golpe.collider.GetComponent<Rigidbody>() != null)
                {
                    cartelGrab.SetActive(true); 
                }
                else
                {
                    cartelGrab.SetActive(false);
                }
            }
            else
            {
                cartelGrab.SetActive(false); 
            }
        }
        else
        {
            cartelGrab.SetActive(false);
        }

        // 2. DETECCIÓN DE LA TECLA 'E'
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (objetoAgarrado == null)
            {
                Ray rayo = new Ray(transform.position, transform.forward);
                RaycastHit golpe;

                if (Physics.Raycast(rayo, out golpe, distanciaMaxima, capaInteractuable))
                {
                    IntentarAgarrar(golpe);
                }
            }
            else
            {
                SoltarObjeto();
            }
        }
    }

void IntentarAgarrar(RaycastHit golpe)
    {
        Rigidbody rb = golpe.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Si el objeto se estaba congelando por el script anterior, detenemos esa congelación
            StopAllCoroutines();

            objetoAgarrado = rb;
            objetoAgarrado.isKinematic = false; 
            objetoAgarrado.useGravity = false; // 🌟 IMPORTANTE: Quitamos la gravedad MIENTRAS lo tienes en la mano

            // 🌟 CLAVE: Apagamos el colisionador del objeto temporalmente para que NO choque con tus ojos/cuerpo
            Collider col = objetoAgarrado.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Creamos la articulación en el HoldPoint
            articulacionFisica = holdPoint.gameObject.AddComponent<ConfigurableJoint>();
            articulacionFisica.autoConfigureConnectedAnchor = false;

            Rigidbody rbMano = holdPoint.GetComponent<Rigidbody>();
            if (rbMano != null)
            {
                articulacionFisica.connectedBody = objetoAgarrado; 
            }

            articulacionFisica.anchor = Vector3.zero;
            articulacionFisica.connectedAnchor = Vector3.zero;

            ConfigurarArticulacion();
        }
    }

    void ConfigurarArticulacion()
    {
        if (articulacionFisica == null) return;

        // Bloqueo lineal y angular absoluto (se vuelve un bloque de hormigón pegado a tu pantalla)
        articulacionFisica.xMotion = ConfigurableJointMotion.Locked;
        articulacionFisica.yMotion = ConfigurableJointMotion.Locked;
        articulacionFisica.zMotion = ConfigurableJointMotion.Locked;

        articulacionFisica.angularXMotion = ConfigurableJointMotion.Locked;
        articulacionFisica.angularYMotion = ConfigurableJointMotion.Locked;
        articulacionFisica.angularZMotion = ConfigurableJointMotion.Locked;

        // Quitamos los muelles elásticos (ya no hacen falta porque al estar 'Locked' y sin colisión irá directo al sitio)
        JointDrive drive = new JointDrive();
        drive.positionSpring = 0f; 
        drive.positionDamper = 0f;   
        drive.maximumForce = 0f;
        
        articulacionFisica.xDrive = drive;
        articulacionFisica.yDrive = drive;
        articulacionFisica.zDrive = drive;
    }

    void SoltarObjeto()
    {
        if (articulacionFisica != null)
        {
            Destroy(articulacionFisica);
        }

        if (objetoAgarrado != null)
        {
            // 🌟 CLAVE: Volvemos a encender el colisionador del objeto para que choque contra las mesas al caer
            Collider col = objetoAgarrado.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }

            // Reseteamos por completo cualquier velocidad acumulada para evitar el efecto "lanzamiento" hacia arriba
            objetoAgarrado.linearVelocity = Vector3.zero;
            objetoAgarrado.angularVelocity = Vector3.zero;

            // Le devolvemos la gravedad del mundo para el aterrizaje
            objetoAgarrado.isKinematic = false; 
            objetoAgarrado.useGravity = true;

            // Iniciamos el temporizador para que se fije a la mesa cuando aterrice
            StartCoroutine(EsperarYFijarObjeto(objetoAgarrado));

            objetoAgarrado = null;
        }
    }

    // 🌟 RUTINA DE CONGELACIÓN AL TOCAR MESA
    IEnumerator EsperarYFijarObjeto(Rigidbody rb)
    {
        // Esperamos una fracción de segundo para asegurarnos de que se despegue de las manos y empiece a caer
        yield return new WaitForSeconds(0.2f);

        // Mientras el objeto se esté moviendo hacia abajo o rodando, esperamos
        // Analizamos su velocidad relativa (magnitud pequeña significa que ya impactó y se detuvo)
        while (rb != null && rb.linearVelocity.magnitude > 0.05f)
        {
            yield return null; // Espera al siguiente fotograma
        }

        // Cuando la velocidad es casi 0 (ha aterrizado en la mesa o el suelo del barco):
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // 🌟 SE SUEGLA AL BARCO DE NUEVO AUTOMÁTICAMENTE
        }
    }
}