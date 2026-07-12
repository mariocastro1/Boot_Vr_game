using UnityEngine;

public class ControladorPlato : MonoBehaviour
{
    private GameObject panBajo;
    private GameObject carne;
    private GameObject queso;
    private GameObject tomate;
    private GameObject lechuga;
    private GameObject panArriba;

    private bool yaTieneBase = false; 

    void Start()
    {
        // Búsqueda automática dentro de tu objeto "completa"
        Transform completa = transform.Find("completa");

        if (completa != null)
        {
            Transform tPanBajo = completa.Find("pan_bajo");
            Transform tCarne = completa.Find("carne");
            Transform tQueso = completa.Find("queso");
            Transform tTomate = completa.Find("tomate");
            Transform tLechuga = completa.Find("lechuga");
            Transform tPanArriba = completa.Find("pan_arriba");

            if (tPanBajo != null) panBajo = tPanBajo.gameObject;
            if (tCarne != null) carne = tCarne.gameObject;
            if (tQueso != null) queso = tQueso.gameObject;
            if (tTomate != null) tomate = tTomate.gameObject;
            if (tLechuga != null) lechuga = tLechuga.gameObject;
            if (tPanArriba != null) panArriba = tPanArriba.gameObject;
        }

        // Ocultamos todo al empezar el juego
        OcultarObjeto(panBajo);
        OcultarObjeto(carne);
        OcultarObjeto(queso);
        OcultarObjeto(tomate);
        OcultarObjeto(lechuga);
        OcultarObjeto(panArriba);
    }

    public bool IntentarAñadirIngrediente(string nombreObjeto)
    {
        string nombreMinusculas = nombreObjeto.ToLower();

        // --- CARNE ---
        if (nombreMinusculas.Contains("carne"))
        {
            if (carne != null && !carne.activeSelf)
            {
                MostrarObjeto(carne);
            }
            return true; // Se destruye el clon de la mesa siempre (esté puesto o no)
        }

        // --- QUESO ---
        if (nombreMinusculas.Contains("queso"))
        {
            if (queso != null && !queso.activeSelf)
            {
                MostrarObjeto(queso);
            }
            return true; // Se destruye siempre
        }

        // --- TOMATE ---
        if (nombreMinusculas.Contains("tomate"))
        {
            if (tomate != null && !tomate.activeSelf)
            {
                MostrarObjeto(tomate);
            }
            return true; // Se destruye siempre
        }

        // --- LECHUGA ---
        if (nombreMinusculas.Contains("lechuga"))
        {
            if (lechuga != null && !lechuga.activeSelf)
            {
                MostrarObjeto(lechuga);
            }
            return true; // Se destruye siempre
        }

        // --- PANES (LÓGICA ESPECIAL) ---
        if (nombreMinusculas.Contains("pan"))
        {
            // Si no hay base, activamos la base
            if (!yaTieneBase && panBajo != null)
            {
                if (!panBajo.activeSelf) MostrarObjeto(panBajo);
                yaTieneBase = true;
                return true;
            }
            // Si ya hay base pero no hay tapa, activamos la tapa
            if (yaTieneBase && panArriba != null)
            {
                if (!panArriba.activeSelf) MostrarObjeto(panArriba);
                return true;
            }
            
            // Si ya tiene base y tapa, y tiran un tercer pan, lo destruimos para no ensuciar
            return true; 
        }

        return false; // Si tiran algo que no es un ingrediente (ej. un cuchillo), no lo destruye
    }

    void OcultarObjeto(GameObject obj)
    {
        if (obj != null) obj.SetActive(false);
    }

    void MostrarObjeto(GameObject obj)
    {
        if (obj != null) obj.SetActive(true);
    }
}