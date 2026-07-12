using UnityEngine;

public class EstacionCorteOrdenada : MonoBehaviour
{
    [Header("Referencias del Plato Activo")]
    private GameObject platoActual;
    private ControladorPlato controladorPlatoActual;

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Plato") && platoActual == null)
        {
            platoActual = other.gameObject;
            controladorPlatoActual = platoActual.GetComponent<ControladorPlato>();

            
            Rigidbody rbPlato = platoActual.GetComponent<Rigidbody>();
            if (rbPlato != null) rbPlato.isKinematic = true;

            
            platoActual.transform.SetParent(transform);
            
            
            
            platoActual.transform.localPosition = new Vector3(0, 0, 0); 
            platoActual.transform.localRotation = Quaternion.identity;
            return;
        }

        
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