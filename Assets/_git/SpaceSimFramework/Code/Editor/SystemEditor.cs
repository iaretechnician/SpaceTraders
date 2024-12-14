using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

namespace SpaceSimFramework
{
public class SystemEditor : EditorWindow
{

    [Range(0, 1)]
    private int _sectorFactionIndex;
    [Range(-10, 10)]
    private int _xMapPosition;
    [Range(-10, 10)]
    private int _yMapPosition;
    private string[] _factionList;

    private Flare _sun = null;
    private Material _skybox = null;
    private GameObject[] _stations, _jumpgates, _fields, _wrecks;

    private float _timer = 0;

    [MenuItem("Window/Sector Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SystemEditor));
        EditorSceneManager.OpenScene("Assets/SpaceSimFramework/Content/Scenes/SectorEditor.unity");
    }

    private void Awake()
    {
        FindObjectsForExport();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if(_timer < 0)
        {
            _timer = 1f;
            if(EditorSceneManager.GetActiveScene().name != "SectorEditor")
                EditorSceneManager.OpenScene("Assets/SpaceSimFramework/Content/Scenes/SectorEditor.unity");
            FindObjectsForExport();
        }
    }

    void OnGUI()
    {
        if (_factionList == null || _factionList.Length == 0)
        {
            // Init faction list
            _factionList = new string[ObjectFactory.Instance.Factions.Length];
            for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
                _factionList[i] = ObjectFactory.Instance.Factions[i].name;
        }

        /*
        * Clear data
        */
        GUILayout.Space(10);
        GUILayout.Label("EXPORT AND CLEAR SECTOR DATA", EditorStyles.centeredGreyMiniLabel);
        if (GUILayout.Button("Clear Sector"))
        {
            EditorSceneManager.OpenScene("Assets/SpaceSimFramework/Content/Scenes/SectorEditor.unity");
            ClearSector();
        }

        /*
        * Save sector data
        */
        GUILayout.Label("WARNING: When editing be careful not to mess up\nthe jumpgate target sectors!", EditorStyles.boldLabel);
        if (GUILayout.Button("Export to File"))
        {
            SaveSectorToFile();
        }

        /*
         * Sector loading
         */
        GUILayout.Space(10);
        GUILayout.Label("SECTOR LOADING", EditorStyles.centeredGreyMiniLabel);

        GUILayout.Label("WARNING: Loading a sector will clear the current scene!", EditorStyles.boldLabel);
        if (GUILayout.Button("Import from File"))
        {
            EditorSceneManager.OpenScene("Assets/SpaceSimFramework/Content/Scenes/SectorEditor.unity");
            LoadSectorFromFile();
        }

        /*
        * Sector data editing
        */
        GUILayout.Space(10);
        GUILayout.Label("SECTOR DATA OPTIONS", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Label("Select sector faction");
        _sectorFactionIndex = EditorGUILayout.Popup(_sectorFactionIndex, _factionList);
        _xMapPosition = EditorGUILayout.IntField("Sector X position on map e[-50, 50]", _xMapPosition);
        _yMapPosition = EditorGUILayout.IntField("Sector Y position on map e[-50, 50]", _yMapPosition);
        if (GUILayout.Button("Generate Random Sector"))
        {
            GenerateRandomSector.GenerateSectorAtPosition(new Vector2(_xMapPosition, _yMapPosition), Vector2.one*9999);
        }

        /*
        * Find and display export objects
        */
        GUILayout.Label("Sector objects found: ", EditorStyles.boldLabel);
        GUILayout.Label("Sun flare: " + _sun.name);
        GUILayout.Label("Skybox: " + _skybox.name);

        GUILayout.Space(50);

        foreach (var station in _stations)
        {
            if (station != null)
                GUILayout.Label("Station: " + station.name);
        }
        foreach (var gate in _jumpgates)
        {
            if (gate != null)
                GUILayout.Label("Jumpgate: " + gate.name);
        }
        foreach (var field in _fields)
        {
            if (field != null)
                GUILayout.Label("Asteroid Field: " + field.name);
        }
    }

    private void FindObjectsForExport()
    {
        // Get sun flare
        _sun = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare;
        // Get skybox
        _skybox = RenderSettings.skybox;

        // Get stations
        _stations = GameObject.FindGameObjectsWithTag("Station");
        // Get jumpgates
        _jumpgates = GameObject.FindGameObjectsWithTag("Jumpgate");
        // Get asteroid fields
        _fields = GameObject.FindGameObjectsWithTag("AsteroidField");
        // Get wrecks
        _wrecks = GameObject.FindGameObjectsWithTag("Wreck");
    }

    private void SaveSectorToFile()
    {
        string path = EditorUtility.SaveFilePanel(
             "Save sector to file",
             "",
             "UNNAMED_SECTOR_" + Time.timeSinceLevelLoad,
             "");
        if (path != null && path != "")
        {
            UpdateUniverseMap(Path.GetFileNameWithoutExtension(path));
            FindObjectsForExport();
            foreach (var station in _stations)
            {
                station.GetComponent<Station>().ID = "x" + position.x + "y" + position.y + "st" + GenerateRandomSector.RandomString(6);
            }
            foreach (var gate in _jumpgates)
            {
                gate.GetComponent<Jumpgate>().ID = "x" + position.x + "y" + position.y + "jg" + GenerateRandomSector.RandomString(6);
            }
            foreach (var field in _fields)
            {
                field.GetComponent<AsteroidField>().ID = "x" + position.x + "y" + position.y + "f" + GenerateRandomSector.RandomString(6);
            }
            var sectorModel = new SectorModel(_stations, _jumpgates, _fields, _wrecks, SectorNavigation.SectorSize);
            SectorSaver.SaveSectorToPath(sectorModel, path);
        }
    }

    private void LoadSectorFromFile()
    {
        string path = EditorUtility.OpenFilePanel("Import sector file", "Data/Sectors", "");
        string sectorName = Path.GetFileNameWithoutExtension(path);
        if (path != null && path != "")
        {
            ClearSector();
            SectorLoader.LoadSectorIntoScene(path);
            UpdateSectorFields(sectorName);
        }
    }

    private void ClearSector()
    {
        // Clear sector
        GameObject[] objects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].tag != "Sun" && objects[i].tag != "MainCamera")
            {
                GameObject.DestroyImmediate(objects[i]);
            }
        }
    }

    private void UpdateUniverseMap(string saveFilename)
    {
        // Load sectors
        Dictionary<SerializableVector2, SerializableUniverseSector> existingSectors = Universe.LoadUniverse();

        List<GameObject> jumpgates = new List<GameObject>(GameObject.FindGameObjectsWithTag("Jumpgate"));

        if (jumpgates.Count > 0)
            Universe.AddSector(new Vector2(_xMapPosition, _yMapPosition), jumpgates, saveFilename);
    }

    private void UpdateSectorFields(string sectorName = "")
    {
        foreach(var sector in Universe.Sectors.Values)
        {
            if(sector.Name == sectorName)
            {
                Faction owner = ObjectFactory.Instance.GetFactionFromName(sector.OwnerFaction);
                for (int i = 0; i < _factionList.Length; i++)
                {
                    if (_factionList[i] == owner.name)
                        _sectorFactionIndex = i;
                }
                _xMapPosition = (int)sector.SectorPosition.x;
                _yMapPosition = (int)sector.SectorPosition.y;
            }
        }
    }

}
}