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

            // Lo hacemos hijo de la tabla directamente
            platoActual.transform.SetParent(transform);
            
            // 🌟 SOLUCIÓN DIRECTA: Lo centramos en X y Z, pero le restamos altura en Y.
            // Si ves que se hunde demasiado o se queda un poco alto, cambia el -0.15f por -0.2f o -0.1f
            platoActual.transform.localPosition = new Vector3(0, 0, 0); 
            platoActual.transform.localRotation = Quaternion.identity;
            return;
        }

        // 2. Si hay un plato puesto y el jugador suelta un ingrediente
        if (platoActual != null && controladorPlatoActual != null)
        {
            Rigidbody rbIng = other.GetComponent<Rigidbody>();
            if (rbIng != null && !rbIng.isKinematic)
            {
                bool aceptado = controladorPlatoActual.IntentarAñadirIngrediente(other.gameObject.name);

                if (aceptado)
                {
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