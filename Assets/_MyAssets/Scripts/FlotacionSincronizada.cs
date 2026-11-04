using UnityEngine;
using LowPolyWater;

public class FlotacionSincronizada : MonoBehaviour
{
    [Header("Referencia del Mar")]
    public LowPolyWater.LowPolyWater scriptAgua;

    [Header("Ajuste de Altura")]
    public float desfaseAltura = 0f;

    
    private Quaternion rotacionInicial;

    void Start()
    {
        
        rotacionInicial = transform.rotation;
    }

void Update()
    {
        if (scriptAgua == null) return;

        Vector3 posBarco = transform.position;

        
        float distanciaCalculada = Vector3.Distance(new Vector3(posBarco.x, 0f, posBarco.z), scriptAgua.waveOriginPosition);
        distanciaCalculada = (distanciaCalculada % scriptAgua.waveLength) / scriptAgua.waveLength;

        float alturaOla = scriptAgua.waveHeight * Mathf.Sin(Time.time * Mathf.PI * 2.0f * scriptAgua.waveFrequency 
            + (Mathf.PI * 2.0f * distanciaCalculada));

       
        float yFinal = scriptAgua.transform.position.y - alturaOla + desfaseAltura;
        transform.position = new Vector3(posBarco.x, yFinal, posBarco.z);

        
        float balanceo = Mathf.Sin(Time.time * scriptAgua.waveFrequency * 3f) * (scriptAgua.waveHeight * 10f);
        Quaternion rotacionBalanceo = Quaternion.Euler(-balanceo * 0.3f, 0f, -balanceo);

        transform.rotation = rotacionInicial * rotacionBalanceo;
    }
}