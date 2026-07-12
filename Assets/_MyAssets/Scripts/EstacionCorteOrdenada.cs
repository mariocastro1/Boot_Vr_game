using UnityEngine;

public class EstacionCorteOrdenada : MonoBehaviour
{
    [Header("Referencias del Plato Activo")]
    private GameObject platoActual;
    private ControladorPlato controladorPlatoActual;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Si entra un plato y la estación está vacía, lo imantamos
        if (other.CompareTag("Plato") && platoActual == null)
        {
            platoActual = other.gameObject;
            controladorPlatoActual = platoActual.GetComponent<ControladorPlato>();

            // Imantación física al centro de la mesa
            Rigidbody rbPlato = platoActual.GetComponent<Rigidbody>();
            if (rbPlato != null) rbPlato.isKinematic = true;

            platoActual.transform.SetParent(transform);
            platoActual.transform.localPosition = new Vector3(0, 0.1f, 0); // Ajusta la altura según tu mesa
            platoActual.transform.localRotation = Quaternion.identity;
            return;
        }

        // 2. Si hay un plato puesto y el jugador suelta un ingrediente
        if (platoActual != null && controladorPlatoActual != null)
        {
            Rigidbody rbIng = other.GetComponent<Rigidbody>();
            // Solo procesamos el ingrediente si está suelto (no en la mano del jugador)
            if (rbIng != null && !rbIng.isKinematic)
            {
                // Le preguntamos al plato si este ingrediente es el que toca ahora mismo
                bool aceptado = controladorPlatoActual.IntentarAñadirIngrediente(other.gameObject.name);

                if (aceptado)
                {
                    // Si el plato lo acepta, borramos el objeto de la escena 
                    // porque su versión visual ya se ha encendido dentro del plato
                    Destroy(other.gameObject);
                }
            }
        }
    }

    public void LiberarEstacion()
    {
        platoActual = null;
        controladorPlatoActual = null;
    }
}