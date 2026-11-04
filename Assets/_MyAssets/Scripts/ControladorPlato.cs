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

        
        if (nombreMinusculas.Contains("carne"))
        {
            if (carne != null && !carne.activeSelf)
            {
                MostrarObjeto(carne);
            }
            return true; 
        }

        
        if (nombreMinusculas.Contains("queso"))
        {
            if (queso != null && !queso.activeSelf)
            {
                MostrarObjeto(queso);
            }
            return true; 
        }

        
        if (nombreMinusculas.Contains("tomate"))
        {
            if (tomate != null && !tomate.activeSelf)
            {
                MostrarObjeto(tomate);
            }
            return true; 
        }

        
        if (nombreMinusculas.Contains("lechuga"))
        {
            if (lechuga != null && !lechuga.activeSelf)
            {
                MostrarObjeto(lechuga);
            }
            return true; 
        }

        
        if (nombreMinusculas.Contains("pan"))
        {
            
            if (!yaTieneBase && panBajo != null)
            {
                if (!panBajo.activeSelf) MostrarObjeto(panBajo);
                yaTieneBase = true;
                return true;
            }
            
            if (yaTieneBase && panArriba != null)
            {
                if (!panArriba.activeSelf) MostrarObjeto(panArriba);
                return true;
            }
            
            
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