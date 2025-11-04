public class BuildingSerialization
{/*
    private static HttpClient client = null;
    //const string connection = "http://localhost:301/";
    const string connection = "https://steamite-api.slavetraders.tech";
    static bool init = false;

    [Serializable]
    class BuildingSerialize
    {
        public int id;
        public string Name;
        public string Data;
    }
    public static void Init()
    {
        if (!init)
        {
            client = new();
            client.BaseAddress = new Uri(connection);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            init = true;
        }
    }

    [MenuItem("API/Serialize buildings", priority = 10)]
    public static async void Serialize()
    {
        Init();
        BuildingData data = AssetDatabase.LoadAssetAtPath<BuildingData>(
            "Assets/Game Data/Research && Building/Build Data.asset");
        await client.DeleteAsync("drop-buildings");
        int i = 1;
        foreach (var item in data.Categories)
        {
            foreach (var building in item.Objects)
            {
                BuildingSerialize serialize = new()
                {
                    id = i,
                    Name = building.building.objectName,
                    Data = JsonUtility.ToJson(building.building)
                };
                Debug.Log(Path.GetDirectoryName(AssetDatabase.GetAssetPath(building.building)));
                MultipartFormDataContent content = new();
                content.Add(new StringContent(JsonUtility.ToJson(serialize), Encoding.UTF8, "application/json"), "building");
                content.Add(new StreamContent(
                    File.OpenRead(
                        Path.Combine(
                            Path.GetDirectoryName(AssetDatabase.GetAssetPath(building.building)),
                            "texture.png"))),
                    "texture", $"{building.building.objectName}.png");
                await client.PostAsync("post-building", content);

                i++;
            }
        }

    }*/
}
