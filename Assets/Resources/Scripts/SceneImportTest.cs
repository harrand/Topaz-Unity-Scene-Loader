using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneImportTest : MonoBehaviour
{
    public String scene_path;

	void Start ()
    {
        SceneImporter importer = new SceneImporter(scene_path);
        importer.Retrieve();
	}
	
	void Update ()
    {
		
	}
}
