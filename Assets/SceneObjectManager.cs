using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObjectManager : MonoBehaviour
{
    public static SceneObjectManager Instance { get; private set;}

    [SerializeField] private GameObject meshScanUI;
    [SerializeField] private GameObject originUI;
    void Awake() {
        if (Instance != null && Instance != this){

        Destroy(gameObject);
        }
        else {

        Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void ShowMeshScanUI() => meshScanUI.SetActive(true);
    public virtual void HideMeshScanUI() => meshScanUI.SetActive(false);
    
    public virtual void ShowOriginUI() => originUI.SetActive(true);
    
    public virtual void HideOriginUI() => originUI.SetActive(false);


}
