using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using Minerva;
using Microsoft.MixedReality.Toolkit.SpatialAwareness.Processing;
//using Microsoft.MixedReality.Toolkit.SpatialObjectMeshObserver;

public class PaintManager : BaseInputHandler, IMixedRealityPointerHandler
{
    public static PaintManager Instance { get; private set; }


    public bool _colorMode = false;
    private bool _isInitialized = false;
    public GameObject _colorMenu;
    public SurfaceMeshesToPlanes _planeCreator;

    private float _colorRed_Value = 0.5f;
    private float _colorGreen_Value = 0.5f;
    private float _colorBlue_Value = 0.5f;
    //public Color _selectedColor;
    public Material _selectedMaterial;

    private float _textureScaling = 0.5f;

    public GameObject paletteColorPreview;
    public GameObject planeParentObject;
    public Transform planeParent;
    public GameObject colorModeInfo;

    public float MinPointDistance = .1f;
    public List<Transform> _wallPlanes;
    public List<Transform> _roomPlanes;

    private float _xScale;
    private float _yScale;

    public Collider[] closestWalls;
    public GameObject _selectedWall;
    [SerializeField] private LayerMask layermask;
    public Material _meshMaterial;
    public Material _transMeshMaterial;

    public IMixedRealitySpatialAwarenessMeshObserver spatialMeshObserver;
    public RoomMeshScanManager roomMeshScanManager;
    public List<MeshFilter> spatialMeshFilters;
    public GameObject _room;
    public Mesh roomMesh;

    public Transform _nullpunkt;

    public List<PlaneFinding.MeshData> meshData;

    private float elapsed = 0f;

    public List<Vector3> scanPoints;
    public List<float> distanceToPoints;
    private bool scanPointClose;
    [SerializeField] private float newPointDistance = 3.0f;

    public Material invisibleMat;


    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    protected override void Start()
    {
        paletteColorPreview.GetComponent<Renderer>().material.color = new Color(_colorRed_Value, _colorGreen_Value, _colorBlue_Value);

        base.IsFocusRequired = false;
        layermask |= (1 << 29);
        RegisterHandlers();
    }

    protected override void Update()
    {
        if (!_colorMenu.activeSelf) return;
        if ((elapsed += Time.deltaTime) < 6f) return;
        elapsed = 0f;

        CheckUpdatePlanes();
    }


    public void toggleColorMenu()
    {
        //_colorMenu.SetActive(!_colorMenu.activeSelf);
        //planeParentObject.SetActive(!planeParentObject.activeSelf);
        //toggleColorMode();
        spatialMeshObserver = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        Debug.Log(spatialMeshObserver);
        //spatialMeshObserver.Suspend(); // Beendet das Meshing
        //spatialMeshObserver.VisibleMaterial = invisibleMat;
        //spatialMeshObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
        //spatialMeshObserver.Resume();
        if (spatialMeshObserver == null)
        {
        Debug.LogError("Spatial Mesh Observer ist null!");
        }
        if (_isInitialized == false)
        {

            _isInitialized = true;



            _room = new GameObject("RoomMeshObject");


            _room.AddComponent<MeshRenderer>();

            _room.AddComponent<MeshFilter>();

            _room.GetComponent<MeshFilter>().mesh = roomMesh;
            _room.AddComponent<MeshCollider>();
            _room.GetComponent<MeshCollider>().convex = true;

            _room.GetComponent<Renderer>().material = _meshMaterial;
            _room.layer = 30;
            //_room.transform.localScale = new Vector3(1.03f, 1.03f, 1.03f);


            _planeCreator.DestroyPlanesMask = (SpatialAwarenessSurfaceTypes.Ceiling | SpatialAwarenessSurfaceTypes.Floor | SpatialAwarenessSurfaceTypes.Platform | SpatialAwarenessSurfaceTypes.Unknown);
            _planeCreator.MakePlanes();
            int planeCount = planeParent.childCount;

            scanPoints.Add(Camera.main.transform.position);


            //Adding the children of the planeParent game object to list

            for (int i = 0; i < planeCount; i++)
            {

                planeParent.GetChild(i).gameObject.layer = 29;
                //planeParent.GetChild(i).transform.position += (1.0f * planeParent.GetChild(i).transform.forward);
                //planeParent.GetChild(i).transform.localScale = new Vector3(1.02f, 1.02f, 1.02f);
                //planeParent.GetChild(i).position = Vector3.MoveTowards(planeParent.GetChild(i).transform.position, _nullpunkt.position, 0.5f);
            }
            //planeParent.localScale = new Vector3(0.95f, 0.95f, 0.95f);



        }
        _room.SetActive(true);
    }



    public void toggleColorMode()
    {
        _colorMode = !_colorMode;
        if (_colorMode)
        {
            //StartCoroutine(colorModePopup());
        }

    }

    public void OnSliderUpdatedRed(SliderEventData eventData)
    {
        //erhalte Farb-Wert vom Slider
        _colorRed_Value = eventData.NewValue;

        //Farb-Wert von Preview aktualisieren und im Scene Manger speichern
        paletteColorPreview.GetComponent<Renderer>().material.color = new Color(_colorRed_Value, _colorGreen_Value, _colorBlue_Value);
        //_selectedColor = new Color(_colorRed_Value, _colorGreen_Value, _colorBlue_Value);
    }

    public void OnSliderUpdatedGreen(SliderEventData eventData)
    {
        //erhalte Farb-Wert vom Slider
        _colorGreen_Value = eventData.NewValue;

        //Farb-Wert von Preview aktualisieren und im Scene Manger speichern
        paletteColorPreview.GetComponent<Renderer>().material.color = new Color(_colorRed_Value, _colorGreen_Value, _colorBlue_Value);
        //_selectedColor = new Color(_colorRed_Value, _colorGreen_Value, _colorBlue_Value);
    }

    public void OnSliderUpdatedBlue(SliderEventData eventData)
    {
        //erhalte Farb-Wert vom Slider
        _colorBlue_Value = eventData.NewValue;

        //Farb-Wert von Preview aktualisieren und im Scene Manger speichern
        paletteColorPreview.GetComponent<Renderer>().material.color = new Color(_colorRed_Value, _colorGreen_Value, _colorBlue_Value);
        //_selectedColor = new Color(_colorRed_Value, _colorGreen_Value, _colorBlue_Value);
    }

    public void updateColor(GameObject ColorPreset)
    {
        //setzt Farbe des Presets als aktuelle Farbe und aktualisiert Preview
        //_selectedColor = ColorPreset.GetComponent<Renderer>().material.color;
        //paletteColorPreview.GetComponent<Renderer>().material.color = _selectedColor;
    }

    public void updateMaterial(Material MaterialPreset)
    {
        //setzt Textur des Presets als aktuelle Textur und aktualisiert Preview
        _selectedMaterial = MaterialPreset;
        paletteColorPreview.GetComponent<Renderer>().material = _selectedMaterial;
        //paletteColorPreview.GetComponent<Renderer>().material.color = _selectedColor;
    }

    public void setTransparent(Material MaterialPreset)
    {
        //setzt Textur des Presets als aktuelle Textur und aktualisiert Preview
        _selectedMaterial = MaterialPreset;
        //_selectedColor = new Color(1, 1, 1, 0);
        paletteColorPreview.GetComponent<Renderer>().material = _selectedMaterial;
        paletteColorPreview.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.2f);
    }

    public void OnSliderUpdatedTexScale(SliderEventData eventData)
    {
        //erhalte Texturen-Scale-Wert vom Slider
        _textureScaling = 1.05f - eventData.NewValue;
    }

    public void resetPlanes()
    {

        int planeCount = planeParent.childCount;
        for (int i = 0; i < planeCount; i++)
        {

            planeParent.GetChild(i).GetComponent<Renderer>().material = _transMeshMaterial;
        }
    }

    public void UpdatePlanes()
    {
        scanPointClose = true;
        distanceToPoints.Clear();
        for (int i = 0; i < scanPoints.Count; i++)
        {
            distanceToPoints.Add(Vector3.Distance(Camera.main.transform.position, scanPoints[i]));
            if (distanceToPoints[i] < newPointDistance)
            {

                scanPointClose = false;
            }
        }

        if (scanPointClose)
        {
            int previousPlaneCount = planeParent.childCount;
            _planeCreator.MakePlanes();
            int planeCount = planeParent.childCount;


            scanPoints.Add(Camera.main.transform.position);

            //Adding the children of the planeParent game object to list

            for (int i = previousPlaneCount - 1; i < planeCount; i++)
            {

                planeParent.GetChild(i).gameObject.layer = 29;
                //planeParent.GetChild(i).position = Vector3.MoveTowards(planeParent.GetChild(i).transform.position, new Vector3(0.0f, 0.0f, 0.0f), 0.3f);
            }
            planeParent.localScale = new Vector3(0.95f, 0.95f, 0.95f);

        }
    }

    public void CheckUpdatePlanes()
    {
        scanPointClose = true;
        distanceToPoints.Clear();
        for (int i = 0; i < scanPoints.Count; i++)
        {
            distanceToPoints.Add(Vector3.Distance(Camera.main.transform.position, scanPoints[i]));
            if (distanceToPoints[i] < newPointDistance)
            {

                scanPointClose = false;
            }
        }

        if (scanPointClose)
        {
            StartCoroutine(AutoUpdatePlanes());

        }
    }

    IEnumerator AutoUpdatePlanes()
    {

        yield return new WaitForSeconds(4);
        int previousPlaneCount = planeParent.childCount;
        _planeCreator.MakePlanes();
        int planeCount = planeParent.childCount;


        scanPoints.Add(Camera.main.transform.position);

        //Adding the children of the planeParent game object to list

        for (int i = previousPlaneCount - 1; i < planeCount; i++)
        {

            planeParent.GetChild(i).gameObject.layer = 29;
            //planeParent.GetChild(i).position = Vector3.MoveTowards(planeParent.GetChild(i).transform.position, new Vector3(0.0f, 0.0f, 0.0f), 0.3f);
        }
        //planeParent.localScale = new Vector3(0.95f, 0.95f, 0.95f);
    }

    //Wand einfärben wenn Mal-Modus aktiv
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        if (_colorMode)
        {
            closestWalls = Physics.OverlapSphere(eventData.Pointer.BaseCursor.Position, 0.2f, layermask);
            if (closestWalls == null)
            {
                return;
            }
            _selectedWall = closestWalls[0].gameObject;

            _xScale = _selectedWall.transform.lossyScale.x;
            _yScale = _selectedWall.transform.lossyScale.y;
            _selectedMaterial.mainTextureScale = new Vector2(_xScale * _textureScaling * 4, _yScale * _textureScaling * 4);
            _selectedWall.GetComponent<Renderer>().material = _selectedMaterial;
            //_selectedWall.GetComponent<Renderer>().material.color = _selectedColor;
        }

    }


    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    { }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
    { }

    void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
    { }

    IEnumerator colorModePopup()
    {

        colorModeInfo.SetActive(true);
        //Wait for 2 seconds
        yield return new WaitForSeconds(2);

        colorModeInfo.SetActive(false);
    }


    protected override void RegisterHandlers()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    protected override void UnregisterHandlers()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
    }

    void OnApplicationQuit()
    {
        UnregisterHandlers();
    }


}
