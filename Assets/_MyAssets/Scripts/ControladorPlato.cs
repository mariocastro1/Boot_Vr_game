using UnityEngine;

public class ControladorPlato : MonoBehaviour
{
    private GameObject panBajo;
    private GameObject carne;
    private GameObject queso;
    private GameObject tomate;
    private GameObject lechuga;
    private GameObject panArriba;

    // Control del orden: 
    // 0=Vacío, 1=PanBajo, 2=Carne, 3=Queso, 4=Tomate, 5=Lechuga, 6=PanArriba
    private int faseActual = 0; 

    void Start()
    {
        // Búsqueda automática dentro del propio plato
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

        // Ocultamos todo al empezar la partida
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

        // 1. PRIMER PAN: Si la hamburguesa está vacía (fase 0) y detecta "pan", activa la base
        if (faseActual == 0 && nombreMinusculas.Contains("pan"))
        {
            MostrarObjeto(panBajo);
            faseActual = 1;
            return true;
        }
        // 2. Esperamos la carne
        if (faseActual == 1 && nombreMinusculas.Contains("carne"))
        {
            MostrarObjeto(carne);
            faseActual = 2;
            return true;
        }
        // 3. Esperamos el queso
        if (faseActual == 2 && nombreMinusculas.Contains("queso"))
        {
            MostrarObjeto(queso);
            faseActual = 3;
            return true;
        }
        // 4. Esperamos el tomate
        if (faseActual == 3 && nombreMinusculas.Contains("tomate"))
        {
            MostrarObjeto(tomate);
            faseActual = 4;
            return true;
        }
        // 5. Esperamos la lechuga
        if (faseActual == 4 && nombreMinusculas.Contains("lechuga"))
        {
            MostrarObjeto(lechuga);
            faseActual = 5;
            return true;
        }
        // 6. SEGUNDO PAN: Si ya pusimos la lechuga (fase 5) y vuelves a tirar un "pan", activa la tapa
        if (faseActual == 5 && nombreMinusculas.Contains("pan"))
        {
            MostrarObjeto(panArriba);
            faseActual = 6;
            return true;
        }

        return false; 
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