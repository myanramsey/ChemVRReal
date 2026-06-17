using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialSelection : MonoBehaviour
{
    [Range(2,10)]
    public int numberofRadialParts; 
    public GameObject radialPartPrefab; 
    public Transform radialPartCanvas; 
    public float angleBetweenRadialParts; 

    public Transform handTransform;

    private List<GameObject> spawnedRadialParts = new List<GameObject>();
    private int currentSelectedRadialPartIndex = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        SpawnRadialPart();
        GetSelectedRadialPart();
    }

    void SpawnRadialPart(){

        foreach(GameObject radialPart in spawnedRadialParts){
            Destroy(radialPart); 
        }

        spawnedRadialParts.Clear();

        for(int i = 0; i < numberofRadialParts; i++ ){
            float angle = -i * 360f / numberofRadialParts - angleBetweenRadialParts/2f;
            Vector3 radialPartEulerAngle = new Vector3(0,0,angle); 

            GameObject spawnedRadialPart = Instantiate(radialPartPrefab,radialPartCanvas); 
            spawnedRadialPart.transform.position = radialPartCanvas.position; 
            spawnedRadialPart.transform.localEulerAngles = radialPartEulerAngle; 

            spawnedRadialPart.GetComponent<Image>().fillAmount = (1 / (float)numberofRadialParts - (angleBetweenRadialParts / 360f)); 

            spawnedRadialParts.Add(spawnedRadialPart);
        }
    }

    public void GetSelectedRadialPart(){
        Vector3 centerToHand = handTransform.position - radialPartCanvas.position;
        Vector3 centerToHandProjection = Vector3.ProjectOnPlane(centerToHand, radialPartCanvas.forward); 

        float angle = Vector3.SignedAngle(radialPartCanvas.up, centerToHandProjection, -radialPartCanvas.forward);
        if(angle < 0){
           angle += 360f;
        }
        currentSelectedRadialPartIndex = (int)(angle * numberofRadialParts / 360f);

        for (int i = 0; i < spawnedRadialParts.Count; i++){
            if(i == currentSelectedRadialPartIndex){
                spawnedRadialParts[i].GetComponent<Image>().color = Color.red; 
                spawnedRadialParts[i].transform.localScale = Vector3.one * 1.1f;
            } else {
                spawnedRadialParts[i].GetComponent<Image>().color = Color.white; 
                spawnedRadialParts[i].transform.localScale = Vector3.one * 1f;
            }
        } 


    }

}
