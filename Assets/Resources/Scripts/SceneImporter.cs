using System;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

class Utility
{
    public static XmlNode FirstChildElement(XmlNode parent, String name)
    {
        Debug.Log("parent = " + parent);
        foreach (XmlNode node in parent.ChildNodes)
        {
            if (node.Name == name)
                return node;
        }
        return null;
    }
}

public class SceneImporter
{
    public const String scene_element_name = "scene";
    public const String object_element_name = "object";
    public const String mesh_element_name = "mesh";
    public const String element_label_name = "name";
    public const String element_label_path = "path";
    public const String texture_element_name = "texture";
    public const String position_element_name = "position";
    public const String rotation_element_name = "rotation";
    public const String scale_element_name = "scale";
    public const String node_element_name = "node";

    private XmlDocument scene_file;
    private List<TextBasedObject> imported_objects;

    public SceneImporter(String filename)
    {
        scene_file = new XmlDocument();
        this.imported_objects = new List<TextBasedObject>();
        bool error = false;
        try
        {
            Debug.Log("Scene Path = " + filename);
            scene_file.Load(filename);
        }
        catch (XmlException xmle) { error = true; Console.WriteLine("Scene Importer XML Exception: " + xmle.Message); }
        if (!error)
            this.Import();
    }

    public Scene Retrieve()
    {
        Scene scene = SceneManager.CreateScene("Imported Scene");
        // Populate Scene as necessary.
        SceneManager.SetActiveScene(scene);
        GameObject root = new GameObject();
        root.name = "Scene Root Object";
        Debug.Log("Number of Imported Objects = " + this.imported_objects.Count);
        foreach(TextBasedObject tbo in this.imported_objects)
        {
            GameObject obj = (GameObject.Instantiate(Resources.Load(tbo.mesh_link)) as GameObject).transform.GetChild(0).gameObject;
            NodeName node = obj.AddComponent<NodeName>();
            obj.transform.SetParent(root.transform);
            MeshRenderer mesh_renderer = obj.GetComponent<MeshRenderer>();
            mesh_renderer.material.mainTexture = GameObject.Instantiate(Resources.Load(tbo.texture_link) as Texture);
            obj.transform.localPosition = tbo.transform.position;
            obj.transform.localRotation = Quaternion.Euler(tbo.transform.rotation);
            obj.transform.localScale = tbo.transform.scale;
            node.node_name = tbo.node_name;
            // Make them possible occluders.
            GameObjectUtility.SetStaticEditorFlags(obj, (StaticEditorFlags.OccluderStatic));
        }
        return scene;
    }

    class SimpleTransform
    {
        public Vector3 position, rotation, scale;
        public SimpleTransform(Vector3 position, Vector3 rotation, Vector3 scale) { this.position = position; this.rotation = rotation; this.scale = scale; }
    }

    class TextBasedObject
    {
        public String mesh_name;
        public String mesh_link;
        public String texture_name;
        public String texture_link;
        public String node_name;
        public SimpleTransform transform;
        public TextBasedObject(String mesh_name, String mesh_link, String texture_name, String texture_link, String node_name, SimpleTransform transform) { this.mesh_name = mesh_name; this.mesh_link = mesh_link; this.texture_name = texture_name; this.texture_link = texture_link; this.node_name = node_name; this.transform = transform; }
        public TextBasedObject(XmlNode node)
        {
            this.mesh_name = Utility.FirstChildElement(Utility.FirstChildElement(node, SceneImporter.mesh_element_name), SceneImporter.element_label_name).InnerText;
            this.mesh_link = Utility.FirstChildElement(Utility.FirstChildElement(node, SceneImporter.mesh_element_name), SceneImporter.element_label_path).InnerText;
            this.texture_name = Utility.FirstChildElement(Utility.FirstChildElement(node, SceneImporter.texture_element_name), SceneImporter.element_label_name).InnerText;
            this.texture_link = Utility.FirstChildElement(Utility.FirstChildElement(node, SceneImporter.texture_element_name), SceneImporter.element_label_path).InnerText;
            this.node_name = Utility.FirstChildElement(node, SceneImporter.node_element_name).InnerText;
            this.transform = new SimpleTransform(Vector3.zero, Vector3.zero, Vector3.zero);
            XmlNode element = Utility.FirstChildElement(node, SceneImporter.position_element_name);
            transform.position.x = float.Parse(Utility.FirstChildElement(element, "x").InnerText);
            transform.position.y = float.Parse(Utility.FirstChildElement(element, "y").InnerText);
            transform.position.z = float.Parse(Utility.FirstChildElement(element, "z").InnerText);

            element = Utility.FirstChildElement(node, SceneImporter.rotation_element_name);
            transform.rotation.x = float.Parse(Utility.FirstChildElement(element, "x").InnerText);
            transform.rotation.y = float.Parse(Utility.FirstChildElement(element, "y").InnerText);
            transform.rotation.z = float.Parse(Utility.FirstChildElement(element, "z").InnerText);

            element = Utility.FirstChildElement(node, SceneImporter.scale_element_name);
            transform.scale.x = float.Parse(Utility.FirstChildElement(element, "x").InnerText);
            transform.scale.y = float.Parse(Utility.FirstChildElement(element, "y").InnerText);
            transform.scale.z = float.Parse(Utility.FirstChildElement(element, "z").InnerText);
        }
    }

    private void Import()
    {
        this.imported_objects.Clear();
        XmlNode scene_node = null;
        foreach (XmlNode node in this.scene_file.DocumentElement.SelectNodes("/" + SceneImporter.scene_element_name))
            scene_node = node;
        Debug.Log("scene node = " + scene_node);
        uint child_counter = 0;
        Func<String> get_current_name = () => SceneImporter.object_element_name + child_counter;
        XmlNode object_node = null;
        do
        {
            object_node = Utility.FirstChildElement(scene_node, get_current_name());
            Debug.Log("child of node " + scene_node.Name + " of name " + get_current_name() + " = " + object_node);
            if (object_node != null)
            {
                this.imported_objects.Add(new TextBasedObject(object_node));
                child_counter++;
            }
        } while (object_node != null);
    }
}
