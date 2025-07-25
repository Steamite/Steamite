using ResearchUI;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class ResourceTester : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        CheckBuildings();
        CheckReseach();
        CheckMinables();
    }

    [MenuItem("Custom Editors/Check for none Resoucerces _F10")]
    public static void RequestCheck()
    {
        CheckBuildings();
        CheckReseach();
        CheckMinables();
    }


    private static void CheckMinables()
    {
        bool succes = true;
        MinableRes[] minableRes = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Holders/MapGen").Cast<MinableRes>().ToArray();
        foreach (var res in minableRes)
        {
            Resource resource = new();
            resource.types.Add(res.resource);
            if (CheckResource(resource, res.name, "Minable Res", "Minable Resouce yield"))
                succes = false;
        }

        if (!succes)
            throw new BuildFailedException($"Buildings have none resources assigned!");
    }

    static void CheckBuildings()
    {
        bool succes = true;
        BuildingData data = AssetDatabase.LoadAssetAtPath<BuildingData>("Assets/Game Data/Research && Building/Build Data.asset");
        for (int i = 0; i < data.Categories.Count; i++)
        {
            for (int j = 0; j < data.Categories[i].Objects.Count; j++)
            {
                Building building = data.Categories[i].Objects[j].building;
                if (building != null)
                {
                    if (CheckResource(building.Cost, building.objectName, data.Categories[i].Name, "Production cost"))
                        succes = false;

                    IResourceProduction res = building as IResourceProduction;
                    if (res != null)
                    {
                        if (CheckResource(res.ResourceYield, building.objectName, data.Categories[i].Name, "Production cost"))
                            succes = false;
                        if (CheckResource(res.ResourceYield, building.objectName, data.Categories[i].Name, "Production Yeild"))
                            succes = false;
                    }
                }
            }
        }

        if (!succes)
            throw new BuildFailedException($"Buildings have none resources assigned!");
    }

    static void CheckReseach()
    {
        bool succes = true;
        ResearchData data = AssetDatabase.LoadAssetAtPath<ResearchData>("Assets/Game Data/Research && Building/Research Data.asset");
        for (int i = 0; i < data.Categories.Count; i++)
        {
            for (int j = 0; j < data.Categories[i].Objects.Count; j++)
            {
                if (CheckResource(data.Categories[i].Objects[j].reseachCost, data.Categories[i].Objects[j].Name, data.Categories[i].Name, "Research cost"))
                    succes = false;
            }
        }
        if (!succes)
            throw new BuildFailedException($"Buildings have none resources assigned!");
    }

    static bool CheckResource(Resource testRes, string objectName, string categName, string problemName)
    {
        if (testRes.types.Contains(ResourceType.None))
        {
            /*if (handle == null)
                handle = EditorUtility.DisplayDialog("None resources detected", "Do you want to fail the build, or remove all NONE types?", , );*/
            Debug.LogError($"{objectName} in category {categName}! ({problemName})");
            return true;
        }
        return false;
    }
}