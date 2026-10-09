using System.IO;
using System.Collections.Generic;
using AutoVerse;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AutoVerseSceneBuilder
{
    private const string SceneFolder = "Assets/AutoVerse/Scenes";
    private static Font font;
    private static AutoVerseController controller;
    private static GameObject importedVehiclePrefab;
    private static GameObject importedSuvPrefab;
    private static GameObject importedThirdPrefab;

    [MenuItem("AutoVerse/Build All Scenes")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(SceneFolder);
        importedVehiclePrefab = FindPrefabByFileName("SportCar_1");
        if (!importedVehiclePrefab) importedVehiclePrefab = FindImportedVehiclePrefab(false);
        importedSuvPrefab = FindPrefabByFileName("Car_3_Baked_1");
        if (!importedSuvPrefab) importedSuvPrefab = importedVehiclePrefab;
        importedThirdPrefab = FindPrefabByFileName("Car_3_Baked_2");
        if (!importedThirdPrefab) importedThirdPrefab = FindPrefabByFileName("Car_3_Baked_3");
        if (!importedVehiclePrefab)
        {
            Debug.LogError("AutoVerse could not find the SportCar_1 vehicle prefab under Assets/SportCar/Prefabs.");
            return;
        }
        if (!importedThirdPrefab) importedThirdPrefab = importedSuvPrefab;
        string[] names = { "MainMenu", "MainShowroom", "SUVZone", "EVZone", "CustomizationStudio" };
        foreach (string name in names)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildScene(name);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/" + name + ".unity");
        }
        EditorBuildSettings.scenes = System.Array.ConvertAll(names, n => new EditorBuildSettingsScene(SceneFolder + "/" + n + ".unity", true));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(SceneFolder + "/MainMenu.unity");
        Debug.Log("AutoVerse created five scenes. Press Play to preview the project.");
    }

    private static void BuildScene(string sceneName)
    {
        bool menu = sceneName == "MainMenu";
        string title = sceneName switch
        {
            "MainMenu" => "AUTOVERSE",
            "MainShowroom" => "THE SHOWROOM",
            "SUVZone" => "SUV COLLECTION",
            "EVZone" => "ELECTRIC FUTURE",
            _ => "LUXURY CAR"
        };
        SetupCamera(menu);
        SetupLighting();
        BuildRoom(menu ? "MainShowroom" : sceneName);
        BuildCar(menu ? "MainShowroom" : sceneName);
        if (sceneName == "MainShowroom") BuildShowroomFleet();
        BuildUI(sceneName, title, menu);
    }

    private static void SetupCamera(bool menu)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f,0.055f,0.09f);
        cameraObject.transform.position = menu ? new Vector3(0,2.5f,-9) : new Vector3(0,3.15f,-11.5f);
        cameraObject.transform.rotation = Quaternion.Euler(10,0,0);
        camera.fieldOfView = menu ? 48 : 44;
        cameraObject.AddComponent<AudioListener>();
    }

    private static void SetupLighting()
    {
        GameObject lightObject = new GameObject("Showroom Key Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.color = new Color(0.82f,0.89f,1f);
        lightObject.transform.rotation = Quaternion.Euler(35,-28,0);
        light.shadows = LightShadows.None;
        RenderSettings.ambientLight = new Color(0.27f,0.29f,0.33f);
    }

    private static void BuildRoom(string sceneName)
    {
        Color zoneAccent = sceneName switch
        {
            "SUVZone" => new Color(1f,0.48f,0.12f),
            "EVZone" => new Color(0.12f,0.9f,0.62f),
            "CustomizationStudio" => new Color(0.68f,0.38f,1f),
            _ => new Color(0.84f,0.66f,0.38f)
        };
        Material floor = Mat(new Color(0.075f,0.09f,0.12f), 0.58f);
        Material wall = Mat(new Color(0.055f,0.067f,0.087f));
        Material panel = Mat(new Color(0.085f,0.1f,0.125f),0.42f);
        Material accent = Mat(zoneAccent, 0.3f);
        Cube("Showroom Floor", new Vector3(0,-0.28f,0), new Vector3(14,0.5f,14), floor);
        Cube("Back Wall", new Vector3(0,3.1f,6.2f), new Vector3(14,6.5f,0.35f), wall);
        Cube("Left Wall", new Vector3(-7,3.1f,0), new Vector3(0.35f,6.5f,13), wall);
        Cube("Right Wall", new Vector3(7,3.1f,0), new Vector3(0.35f,6.5f,13), wall);
        if (sceneName == "MainShowroom") BuildShowroomBrandSign(zoneAccent);
        for (int i=-1; i<=1; i+=2)
        {
            Cube("Wall Panel",new Vector3(i*4.55f,2.7f,5.98f),new Vector3(2.5f,4.8f,0.08f),panel);
            Cube("Wall Reveal",new Vector3(i*4.55f+1.25f,2.7f,5.91f),new Vector3(0.025f,4.4f,0.025f),accent);
        }
        Cube("Ceiling Cove Light Left",new Vector3(-4.7f,5.75f,0),new Vector3(3.5f,0.045f,7.5f),accent);
        Cube("Ceiling Cove Light Right",new Vector3(4.7f,5.75f,0),new Vector3(3.5f,0.045f,7.5f),accent);
        for (int i=-1; i<=1; i++)
        {
            GameObject lamp = new GameObject("Ceiling Spotlight"); lamp.transform.position=new Vector3(i*3.4f,5.5f,0);
            Light l=lamp.AddComponent<Light>(); l.type=LightType.Spot; l.spotAngle=72; l.range=12; l.intensity=3.2f; l.color=Color.Lerp(new Color(0.9f,0.9f,0.92f),zoneAccent,0.12f); l.shadows=LightShadows.None;
            lamp.transform.rotation=Quaternion.Euler(90,0,0);
        }
        if (sceneName == "MainShowroom")
        {
            for (int i=-1; i<=1; i++)
            {
                float x=i*4f;
                Cube("Showroom Display Platform",new Vector3(x,-0.01f,0),new Vector3(3.55f,0.14f,5.8f),Mat(new Color(0.055f,0.065f,0.082f),0.66f));
                Cube("Platform Edge Light",new Vector3(x,0.075f,-2.82f),new Vector3(2.9f,0.018f,0.035f),accent);
                GameObject floorWash=new GameObject("Vehicle Floor Wash"); floorWash.transform.position=new Vector3(x,2.7f,-0.35f);
                Light wash=floorWash.AddComponent<Light>(); wash.type=LightType.Spot; wash.spotAngle=54; wash.range=7; wash.intensity=2.2f; wash.color=Color.Lerp(Color.white,zoneAccent,0.12f); wash.shadows=LightShadows.None;
                floorWash.transform.rotation=Quaternion.Euler(28,0,0);
            }
        }
        if (sceneName == "EVZone")
        {
            Material green=Mat(new Color(0.05f,0.85f,0.55f));
            Cube("Charging Station",new Vector3(4,1.15f,1),new Vector3(0.8f,2.3f,0.55f),wall);
            Cube("Charging Display",new Vector3(4,1.5f,0.7f),new Vector3(0.5f,0.38f,0.04f),green);
            Cube("Charging Cable",new Vector3(3.5f,0.8f,0.2f),new Vector3(0.09f,1.4f,0.09f),green);
        }
        if (sceneName == "SUVZone") Cube("Display Platform",new Vector3(0,-0.01f,0),new Vector3(8,0.14f,5.8f),Mat(new Color(0.18f,0.22f,0.29f),0.55f));
    }

    private static void BuildCar(string sceneName)
    {
        if (importedVehiclePrefab)
        {
            GameObject selected=sceneName=="SUVZone"?importedSuvPrefab:sceneName=="EVZone"?importedThirdPrefab:importedVehiclePrefab;
            BuildImportedCar(sceneName,selected);
        }
    }

    private static GameObject FindImportedVehiclePrefab(bool preferSuv)
    {
        string bestPath=null;
        int bestScore=0;
        foreach (string typeFilter in new[]{"t:Prefab","t:Model"})
        foreach (string guid in AssetDatabase.FindAssets(typeFilter,new[]{"Assets"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            string normalized=path.Replace('_',' ').Replace('-',' ').ToLowerInvariant();
            string compact=normalized.Replace(" ","");
            string file=Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            bool vehiclePath=compact.Contains("sportcar") || normalized.Contains("vehicle") || normalized.Contains("automobile");
            bool isSuvAsset=normalized.Contains("suv") || normalized.Contains("off road") || normalized.Contains("offroad") || normalized.Contains("truck") || normalized.Contains("sport car free 3") || compact.Contains("sportcarfree3") || compact.Contains("sportcar3") || file.Contains("car_3_baked") || file.Contains("car_3_normal");
            if (!vehiclePath) continue;
            if (preferSuv!=isSuvAsset) continue;
            if (normalized.Contains("demo") || normalized.Contains("sample") || normalized.Contains("icon") || normalized.Contains("thumbnail")) continue;
            if (file.Contains("wheel") || file.Contains("tire") || file.Contains("rim") || file.Contains("texture") || file.Contains("material")) continue;
            if (file.Contains("low poly") || file.Contains("lowpoly") || file.Contains("lod")) continue;
            int score=100;
            if (typeFilter=="t:Prefab") score+=20;
            if (file.Contains("car") || file.Contains("vehicle") || file.Contains("sportcar")) score+=15;
            if (file.Contains("normal") || file.Contains("high")) score+=4;
            if (file.Contains("prefab") || file.Contains("model")) score+=5;
            if (file.Contains("sportcar 1") || file.Contains("sportcar_1")) score+=30;
            if (file.Contains("car_3_baked_1")) score+=30;
            if (file.Contains("baked")) score+=6;
            if (path.EndsWith(".fbx",System.StringComparison.OrdinalIgnoreCase) || path.EndsWith(".prefab",System.StringComparison.OrdinalIgnoreCase)) score+=3;
            if (score>bestScore) { bestScore=score; bestPath=path; }
        }
        if (string.IsNullOrEmpty(bestPath)) return null;
        GameObject asset=AssetDatabase.LoadAssetAtPath<GameObject>(bestPath);
        if (!asset || asset.GetComponentInChildren<Renderer>(true)==null)
        {
            Debug.LogError("The imported vehicle asset has no visible model. Check the car prefab/FBX and rebuild the scenes.");
            return null;
        }
        Debug.Log("AutoVerse vehicle model: " + bestPath);
        return asset;
    }

    private static void BuildImportedCar(string sceneName,GameObject prefab)
    {
        string model=sceneName=="SUVZone"?"TERRA X7":sceneName=="EVZone"?"VOLT E1":"AURORA GT";
        GameObject root=new GameObject("Car - " + model);
        GameObject visual=CreateNormalizedVehicle(prefab,root.transform,3.45f);
        if (!visual) { Object.DestroyImmediate(root); return; }
        root.transform.position=new Vector3(0,0.04f,0.2f);
        root.transform.rotation=Quaternion.Euler(0,24f,0);
        root.transform.localScale=Vector3.one*(sceneName=="SUVZone"?1.05f:sceneName=="EVZone"?0.98f:1f);

        BoxCollider collider=root.AddComponent<BoxCollider>(); collider.center=new Vector3(0,0.85f,0); collider.size=new Vector3(3.8f,1.7f,2.05f);
        Transform leftDoor=null,rightDoor=null;
        foreach (Transform part in visual.GetComponentsInChildren<Transform>(true))
        {
            string lower=part.name.ToLowerInvariant();
            if (!lower.Contains("door") || part==visual || part.parent!=visual && part.parent.name.ToLowerInvariant().Contains("door")) continue;
            if (!part.GetComponentInChildren<Renderer>()) continue;
            Bounds localDoorBounds=GetLocalRendererBounds(part,root.transform);
            float side=localDoorBounds.center.z;
            Transform hinge=CreateImportedDoorHinge(part,root.transform,localDoorBounds);
            if (lower.Contains("left") || lower.Contains("_l") || side<0 && leftDoor==null) { if (!leftDoor) leftDoor=hinge; }
            else if (lower.Contains("right") || lower.Contains("_r") || side>=0 && rightDoor==null) { if (!rightDoor) rightDoor=hinge; }
        }

        Light[] lamps=visual.GetComponentsInChildren<Light>(true);
        foreach (Light lamp in lamps) if (lamp) lamp.enabled=false;
        if (lamps.Length==0)
        {
            lamps=new Light[2];
            for (int side=-1;side<=1;side+=2)
            {
                GameObject lampObject=new GameObject("Headlight Beam"); lampObject.transform.SetParent(root.transform,false);
                lampObject.transform.localPosition=new Vector3(1.68f,0.68f,side*0.52f); lampObject.transform.localRotation=Quaternion.Euler(0,90,0);
                Light lamp=lampObject.AddComponent<Light>(); lamp.type=LightType.Spot; lamp.range=10; lamp.spotAngle=34; lamp.intensity=3; lamp.color=new Color(1f,0.9f,0.72f); lamp.enabled=false;
                lamps[side>0?0:1]=lamp;
            }
        }
        controller=new GameObject("AutoVerse Controller").AddComponent<AutoVerseController>();
        controller.car=root.transform; controller.leftDoor=leftDoor; controller.rightDoor=rightDoor; controller.headlights=lamps;
        BuildNameBoard(model,root.transform.position,root.transform.rotation,sceneName);
    }

    private static GameObject CreateNormalizedVehicle(GameObject prefab,Transform parent,float targetLength)
    {
        GameObject visual=Object.Instantiate(prefab,parent);
        visual.name=prefab.name + " Model";
        float wheelScale=prefab.name.IndexOf("SportCar_1",System.StringComparison.OrdinalIgnoreCase)>=0?0.48f:1f;
        ScaleWheelAssemblies(visual,wheelScale);
        Bounds bounds=GetWorldBounds(visual);
        if (bounds.size.sqrMagnitude<0.001f || bounds.size.x<0.001f && bounds.size.z<0.001f)
        {
            Object.DestroyImmediate(visual);
            Debug.LogError("Vehicle prefab has no visible mesh: " + prefab.name);
            return null;
        }
        visual.transform.localRotation=bounds.size.z>bounds.size.x?Quaternion.Euler(0,90,0):Quaternion.identity;
        bounds=GetWorldBounds(visual);
        float length=Mathf.Max(bounds.size.x,bounds.size.z);
        visual.transform.localScale=Vector3.one*(targetLength/Mathf.Max(length,0.01f));
        bounds=GetWorldBounds(visual);
        visual.transform.position+=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z);
        return visual;
    }

    private static void ScaleWheelAssemblies(GameObject vehicle,float factor)
    {
        foreach (Transform part in vehicle.GetComponentsInChildren<Transform>(true))
        {
            string name=part.name.ToLowerInvariant();
            bool wheel=name.Contains("wheel") || name.Contains("tire") || name.Contains("tyre");
            Renderer ownRenderer=part.GetComponent<Renderer>();
            if (!wheel && ownRenderer)
            {
                foreach (Material material in ownRenderer.sharedMaterials)
                {
                    if (!material) continue;
                    string materialName=material.name.ToLowerInvariant();
                    if (materialName.Contains("wheel") || materialName.Contains("tire") || materialName.Contains("tyre") || materialName.Contains("rubber") || materialName.Contains("rim")) { wheel=true; break; }
                }
            }
            if (!wheel || name=="wheels" || name=="tires" || name=="tyres") continue;
            Transform parent=part.parent;
            bool nested=false;
            while(parent && parent!=vehicle.transform)
            {
                string parentName=parent.name.ToLowerInvariant();
                if ((parentName.Contains("wheel") || parentName.Contains("tire") || parentName.Contains("tyre")) && parentName!="wheels" && parentName!="tires" && parentName!="tyres") { nested=true; break; }
                parent=parent.parent;
            }
            if (!nested) part.localScale*=factor;
        }
    }

    private static GameObject FindPrefabByFileName(string targetName)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            if (!string.Equals(Path.GetFileNameWithoutExtension(path),targetName,System.StringComparison.OrdinalIgnoreCase)) continue;
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab && prefab.GetComponentInChildren<Renderer>(true)) { Debug.Log("AutoVerse display vehicle: " + path); return prefab; }
        }
        return null;
    }

    private static Bounds GetWorldBounds(GameObject root)
    {
        Renderer[] renderers=root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length==0) return new Bounds(root.transform.position,Vector3.zero);
        Bounds bounds=renderers[0].bounds;
        for (int i=1;i<renderers.Length;i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static Bounds GetLocalRendererBounds(Transform part,Transform localSpace)
    {
        Renderer[] renderers=part.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length==0) return new Bounds(localSpace.InverseTransformPoint(part.position),Vector3.zero);
        Bounds world=renderers[0].bounds;
        for (int i=1;i<renderers.Length;i++) world.Encapsulate(renderers[i].bounds);
        Vector3 min=Vector3.one*float.PositiveInfinity,max=Vector3.one*float.NegativeInfinity;
        for(int ix=0;ix<2;ix++) for(int iy=0;iy<2;iy++) for(int iz=0;iz<2;iz++)
        {
            Vector3 corner=new Vector3(ix==0?world.min.x:world.max.x,iy==0?world.min.y:world.max.y,iz==0?world.min.z:world.max.z);
            Vector3 local=localSpace.InverseTransformPoint(corner); min=Vector3.Min(min,local); max=Vector3.Max(max,local);
        }
        return new Bounds((min+max)*0.5f,max-min);
    }

    private static Transform CreateImportedDoorHinge(Transform doorPart,Transform car,Bounds localBounds)
    {
        Transform hinge=new GameObject("Animated " + doorPart.name).transform;
        hinge.SetParent(car,false);
        hinge.localPosition=new Vector3(localBounds.max.x,localBounds.center.y,localBounds.center.z);
        doorPart.SetParent(hinge,true);
        return hinge;
    }

    private static void BuildPrimitiveCar(string sceneName)
    {
        string model = sceneName == "SUVZone" ? "TERRA X7" : sceneName == "EVZone" ? "VOLT E1" : "AURORA GT";
        GameObject root = new GameObject("Car - " + model);
        root.transform.position = new Vector3(0,0.04f,0.2f);
        root.transform.rotation = Quaternion.Euler(0,24f,0);
        root.transform.localScale = Vector3.one*(sceneName=="SUVZone"?1.16f:sceneName=="EVZone"?0.96f:1f);
        Color paintColor = sceneName == "SUVZone" ? new Color(0.22f,0.24f,0.23f) : sceneName == "EVZone" ? new Color(0.78f,0.8f,0.78f) : new Color(0.68f,0.62f,0.49f);
        Material paint = Mat(paintColor,0.72f);
        Material doorPaint = Mat(Color.Lerp(paintColor,Color.white,0.12f),0.62f);
        Material glass = Mat(new Color(0.055f,0.075f,0.095f),0.7f);
        Material tire = Mat(new Color(0.025f,0.03f,0.04f),0.8f);
        Material metal = Mat(new Color(0.58f,0.68f,0.78f),0.25f);
        Material trim = Mat(new Color(0.035f,0.045f,0.055f),0.55f);
        Renderer[] paintRenderers = new Renderer[6];
        GameObject body=Cube("Body",new Vector3(0,0.42f,0),new Vector3(3.4f,0.62f,1.55f),paint,root.transform); paintRenderers[0]=body.GetComponent<Renderer>();
        Cube("Cabin",new Vector3(-0.05f,1.0f,0),new Vector3(1.8f,0.72f,1.26f),trim,root.transform);
        GameObject frontGlass=Cube("Front Windshield",new Vector3(0.89f,1.08f,0),new Vector3(0.045f,0.5f,1.08f),glass,root.transform); frontGlass.transform.localRotation=Quaternion.Euler(0,0,20f);
        GameObject rearGlass=Cube("Rear Windshield",new Vector3(-0.99f,1.08f,0),new Vector3(0.045f,0.48f,1.08f),glass,root.transform); rearGlass.transform.localRotation=Quaternion.Euler(0,0,-20f);
        for (int side=-1;side<=1;side+=2)
        {
            Cube("Side Skirt",new Vector3(0,0.14f,side*0.78f),new Vector3(1.8f,0.1f,0.1f),trim,root.transform);
            Cube("Side Mirror",new Vector3(0.62f,1.0f,side*0.78f),new Vector3(0.24f,0.14f,0.2f),metal,root.transform);
            Cube("Rear Lamp",new Vector3(-1.71f,0.55f,side*0.5f),new Vector3(0.07f,0.17f,0.28f),Mat(new Color(0.72f,0.055f,0.045f)),root.transform);
            Cube("Front Fender Marker",new Vector3(1.28f,0.68f,side*0.785f),new Vector3(0.1f,0.055f,0.025f),Mat(new Color(1f,0.58f,0.18f)),root.transform);
            Cube("Door Shoulder Line",new Vector3(-0.04f,0.82f,side*0.79f),new Vector3(1.0f,0.025f,0.025f),metal,root.transform);
        }
        Cube("Roof",new Vector3(-0.05f,1.37f,0),new Vector3(1.38f,0.12f,1.25f),paint,root.transform);
        GameObject hood=Cube("Hood",new Vector3(1.0f,0.73f,0),new Vector3(1.15f,0.12f,1.47f),paint,root.transform); paintRenderers[1]=hood.GetComponent<Renderer>();
        GameObject trunk=Cube("Trunk",new Vector3(-1.25f,0.72f,0),new Vector3(0.75f,0.14f,1.45f),paint,root.transform); paintRenderers[2]=trunk.GetComponent<Renderer>();
        Transform leftDoor=CreateDoor(root.transform,"Left_Door",-1,doorPaint,metal);
        Transform rightDoor=CreateDoor(root.transform,"Right_Door",1,doorPaint,metal);
        foreach (int side in new[]{-1,1})
        {
            Transform door=side<0?leftDoor:rightDoor;
            LocalCube("Front Door Glass",door,new Vector3(-0.15f,0.46f,-side*0.13f),new Vector3(0.62f,0.34f,0.035f),glass);
            LocalCube("Rear Door Glass",door,new Vector3(-0.96f,0.46f,-side*0.13f),new Vector3(0.62f,0.34f,0.035f),glass);
            LocalCube("Window Divider",door,new Vector3(-0.55f,0.46f,-side*0.13f),new Vector3(0.035f,0.36f,0.045f),trim);
        }
        paintRenderers[3]=leftDoor.Find("Door Panel").GetComponent<Renderer>();
        paintRenderers[4]=rightDoor.Find("Door Panel").GetComponent<Renderer>();
        paintRenderers[5]=root.transform.Find("Roof").GetComponent<Renderer>();
        Cube("Front Bumper",new Vector3(1.73f,0.3f,0),new Vector3(0.16f,0.2f,1.42f),trim,root.transform);
        Cube("Rear Bumper",new Vector3(-1.73f,0.3f,0),new Vector3(0.16f,0.2f,1.42f),trim,root.transform);
        Cube("Front Grille",new Vector3(1.815f,0.46f,0),new Vector3(0.035f,0.24f,0.58f),trim,root.transform);
        for(int i=-1;i<=1;i++) Cube("Grille Slat",new Vector3(1.84f,0.39f+i*0.065f,0),new Vector3(0.02f,0.018f,0.48f),metal,root.transform);
        Cube("Front Lower Intake",new Vector3(1.79f,0.27f,0),new Vector3(0.05f,0.09f,0.72f),trim,root.transform);
        GameObject badge=GameObject.CreatePrimitive(PrimitiveType.Cylinder); badge.name="Front Emblem"; badge.transform.SetParent(root.transform,false); badge.transform.localPosition=new Vector3(1.86f,0.62f,0); badge.transform.localRotation=Quaternion.Euler(0,0,90); badge.transform.localScale=new Vector3(0.13f,0.035f,0.13f); badge.GetComponent<Renderer>().sharedMaterial=metal;
        Cube("Rear License Plate",new Vector3(-1.82f,0.48f,0),new Vector3(0.035f,0.17f,0.52f),metal,root.transform);
        for(int side=-1;side<=1;side+=2) for(int axle=-1;axle<=1;axle+=2)
        {
            GameObject wheel=new GameObject("Wheel " + (axle>0?"Front":"Rear") + (side>0?" Right":" Left")); wheel.transform.SetParent(root.transform,false);
            wheel.transform.localPosition=new Vector3(axle*1.08f,0.36f,side*0.79f); wheel.transform.localRotation=Quaternion.Euler(90,0,0);
            GameObject tireObject=GameObject.CreatePrimitive(PrimitiveType.Cylinder); tireObject.name="Rubber Tire"; tireObject.transform.SetParent(wheel.transform,false); tireObject.transform.localScale=new Vector3(0.72f,0.28f,0.72f); tireObject.GetComponent<Renderer>().sharedMaterial=tire;
            GameObject rim=GameObject.CreatePrimitive(PrimitiveType.Cylinder); rim.name="Alloy Rim"; rim.transform.SetParent(wheel.transform,false); rim.transform.localPosition=new Vector3(0,side*0.145f,0); rim.transform.localScale=new Vector3(0.48f,0.035f,0.48f); rim.GetComponent<Renderer>().sharedMaterial=metal;
            GameObject hub=GameObject.CreatePrimitive(PrimitiveType.Cylinder); hub.name="Wheel Center Cap"; hub.transform.SetParent(wheel.transform,false); hub.transform.localPosition=new Vector3(0,side*0.17f,0); hub.transform.localScale=new Vector3(0.14f,0.045f,0.14f); hub.GetComponent<Renderer>().sharedMaterial=trim;
            for(int spoke=0;spoke<5;spoke++)
            {
                GameObject spokeObject=GameObject.CreatePrimitive(PrimitiveType.Cube); spokeObject.name="Alloy Spoke"; spokeObject.transform.SetParent(wheel.transform,false);
                spokeObject.transform.localPosition=new Vector3(0,side*0.17f,0); spokeObject.transform.localRotation=Quaternion.Euler(0,spoke*72f,0);
                spokeObject.transform.localScale=new Vector3(0.055f,0.025f,0.34f); spokeObject.GetComponent<Renderer>().sharedMaterial=metal;
            }
        }
        Light[] lamps=new Light[2];
        for(int side=-1;side<=1;side+=2)
        {
            Cube("Headlamp",new Vector3(1.71f,0.57f,side*0.51f),new Vector3(0.08f,0.18f,0.34f),Mat(new Color(0.95f,0.88f,0.68f)),root.transform);
            GameObject lamp=new GameObject("Headlight Beam"); lamp.transform.SetParent(root.transform,false); lamp.transform.localPosition=new Vector3(1.8f,0.58f,side*0.5f); lamp.transform.localRotation=Quaternion.Euler(0,90,0);
            Light l=lamp.AddComponent<Light>(); l.type=LightType.Spot; l.range=12; l.spotAngle=38; l.intensity=5; l.color=new Color(1f,0.9f,0.72f); l.enabled=false; lamps[side==1?0:1]=l;
        }
        controller= new GameObject("AutoVerse Controller").AddComponent<AutoVerseController>();
        controller.car=root.transform; controller.leftDoor=leftDoor; controller.rightDoor=rightDoor; controller.headlights=lamps;
        BuildNameBoard(model,root.transform.position,root.transform.rotation,sceneName);
    }

    private static Transform CreateDoor(Transform car,string name,int side,Material paint,Material handleMaterial)
    {
        Transform pivot=new GameObject(name).transform;
        pivot.SetParent(car,false);
        pivot.localPosition=new Vector3(0.55f,0.62f,side*0.78f);
        LocalCube("Door Panel",pivot,new Vector3(-0.55f,0,0),new Vector3(1.05f,0.48f,0.07f),paint);
        LocalCube("Door Handle",pivot,new Vector3(-0.14f,0.08f,side*0.055f),new Vector3(0.2f,0.045f,0.025f),handleMaterial);
        LocalCube("Door Trim",pivot,new Vector3(-0.55f,-0.205f,side*0.052f),new Vector3(0.96f,0.018f,0.018f),handleMaterial);
        return pivot;
    }

    private static GameObject LocalCube(string name,Transform parent,Vector3 position,Vector3 scale,Material material)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=scale;
        go.GetComponent<Renderer>().sharedMaterial=material; return go;
    }

    private static void BuildNameBoard(string model,Vector3 carPosition,Quaternion carRotation,string zone)
    {
        Color accent=zone=="SUVZone"?new Color(1f,0.48f,0.12f):zone=="EVZone"?new Color(0.12f,0.9f,0.62f):zone=="CustomizationStudio"?new Color(0.68f,0.38f,1f):new Color(0.84f,0.66f,0.38f);
        Material boardMat=Mat(new Color(0.025f,0.032f,0.045f),0.65f);
        Material metal=Mat(new Color(0.45f,0.48f,0.52f),0.7f);
        // Keep the freestanding board outside the full 360-degree sweep of the car and its doors.
        Vector3 sidePosition=carPosition+carRotation*new Vector3(0,0,-3.7f);
        float x=sidePosition.x, z=sidePosition.z;
        Cube("Vehicle Nameplate",new Vector3(x,0.48f,z),new Vector3(1.22f,0.34f,0.06f),boardMat);
        Cube("Nameplate Accent",new Vector3(x,0.30f,z-0.035f),new Vector3(0.96f,0.018f,0.018f),Mat(accent,0.4f));
        Cube("Nameplate Side Stand",new Vector3(x,0.18f,z),new Vector3(0.045f,0.27f,0.05f),metal);
        GameObject boardLight=new GameObject("Nameplate Accent Light"); boardLight.transform.position=new Vector3(x,0.9f,z+0.1f);
        Light point=boardLight.AddComponent<Light>(); point.type=LightType.Point; point.range=1.5f; point.intensity=0.45f; point.color=accent; point.shadows=LightShadows.None;
        GameObject labelObject=new GameObject("Vehicle Nameplate Text",typeof(RectTransform),typeof(Canvas));
        Canvas canvas=labelObject.GetComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace;
        RectTransform rect=labelObject.GetComponent<RectTransform>(); rect.sizeDelta=new Vector2(156,44);
        labelObject.transform.position=new Vector3(x,0.49f,z-0.037f); labelObject.transform.rotation=Quaternion.Euler(0,180,0); labelObject.transform.localScale=new Vector3(-0.0075f,0.0075f,0.0075f);
        Text text=new GameObject("Model Name",typeof(RectTransform),typeof(Text)).GetComponent<Text>();
        text.transform.SetParent(labelObject.transform,false); RectTransform textRect=text.GetComponent<RectTransform>(); textRect.anchorMin=Vector2.zero; textRect.anchorMax=Vector2.one; textRect.offsetMin=new Vector2(4,2); textRect.offsetMax=new Vector2(-4,-2);
        text.font=Font(); text.text=model; text.fontSize=20; text.fontStyle=FontStyle.Bold; text.alignment=TextAnchor.MiddleCenter; text.color=new Color(0.96f,0.94f,0.89f); text.raycastTarget=false;
    }

    private static void BuildShowroomBrandSign(Color accent)
    {
        Material plaque=Mat(new Color(0.018f,0.024f,0.033f),0.7f);
        Material gold=Mat(accent,0.35f);
        float z=5.94f, y=4.65f;
        Cube("A1 Cars Back Wall Plaque",new Vector3(0,y,z),new Vector3(4.3f,1.25f,0.12f),plaque);
        Cube("A1 Sign Top Light",new Vector3(0,y+0.66f,z-0.08f),new Vector3(4.05f,0.035f,0.04f),gold);
        Cube("A1 Sign Bottom Light",new Vector3(0,y-0.66f,z-0.08f),new Vector3(4.05f,0.035f,0.04f),gold);
        Cube("A1 Sign Left Light",new Vector3(-2.17f,y,z-0.08f),new Vector3(0.035f,1.3f,0.04f),gold);
        Cube("A1 Sign Right Light",new Vector3(2.17f,y,z-0.08f),new Vector3(0.035f,1.3f,0.04f),gold);
        foreach (Vector2 corner in new[]{new Vector2(-1.92f,y+0.47f),new Vector2(1.92f,y+0.47f),new Vector2(-1.92f,y-0.47f),new Vector2(1.92f,y-0.47f)})
        {
            GameObject glow=new GameObject("A1 Sign Corner Light"); glow.transform.position=new Vector3(corner.x,corner.y,z-0.18f);
            Light light=glow.AddComponent<Light>(); light.type=LightType.Point; light.color=accent; light.range=1.6f; light.intensity=0.65f; light.shadows=LightShadows.None;
        }
        GameObject textObject=new GameObject("A1 Cars Sign Text",typeof(RectTransform),typeof(Canvas));
        textObject.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
        RectTransform canvasRect=textObject.GetComponent<RectTransform>(); canvasRect.sizeDelta=new Vector2(360,96);
        textObject.transform.position=new Vector3(0,y,z-0.075f); textObject.transform.rotation=Quaternion.Euler(0,180,0); textObject.transform.localScale=new Vector3(-0.01f,0.01f,0.01f);
        Text sign= new GameObject("A1 Cars",typeof(RectTransform),typeof(Text)).GetComponent<Text>();
        sign.transform.SetParent(textObject.transform,false); RectTransform signRect=sign.GetComponent<RectTransform>(); signRect.anchorMin=new Vector2(0,0.12f); signRect.anchorMax=new Vector2(1,0.9f); signRect.offsetMin=Vector2.zero; signRect.offsetMax=Vector2.zero;
        sign.font=Font(); sign.text="A1 CARS"; sign.fontSize=54; sign.fontStyle=FontStyle.Bold; sign.alignment=TextAnchor.MiddleCenter; sign.color=new Color(0.96f,0.9f,0.78f); sign.raycastTarget=false;
    }

    private static void BuildShowroomFleet()
    {
        CreateDisplayCar("VOLT E1",importedSuvPrefab,new Vector3(-4f,0.04f,0.2f));
        CreateDisplayCar("TERRA X7",importedThirdPrefab,new Vector3(4f,0.04f,0.2f));
    }

    private static void CreateDisplayCar(string model,GameObject prefab,Vector3 position)
    {
        if (!prefab) return;
        GameObject display=new GameObject("Car - " + model);
        display.transform.position=Vector3.zero;
        display.transform.rotation=Quaternion.identity;
        GameObject visual=CreateNormalizedVehicle(prefab,display.transform,3.2f);
        if (!visual) { Object.DestroyImmediate(display); return; }
        display.transform.position=position;
        display.transform.rotation=Quaternion.Euler(0,24f,0);
        BuildNameBoard(model,position,display.transform.rotation,"MainShowroom");
    }

    private static void BuildUI(string sceneName, string title, bool menu)
    {
        GameObject canvasObj=new GameObject("AutoVerse UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        Canvas canvas=canvasObj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=canvasObj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1600,900); scaler.matchWidthOrHeight=0.5f;
        if (!GameObject.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>()) new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.EventSystems.StandaloneInputModule));
        Panel(canvasObj.transform,"Top Bar",new Vector2(0,1),new Vector2(1,1),new Vector2(0,68),new Vector2(0,68),new Color(0.025f,0.04f,0.07f,0.93f));
        Label(canvasObj.transform,"Brand","AUTOVERSE  /  PRIVATE MOTOR COLLECTION",20,TextAnchor.MiddleLeft,new Vector2(0,1),new Vector2(0.54f,1),new Vector2(32,0),new Vector2(-4,0),new Color(0.91f,0.89f,0.83f));
        Color titleAccent=sceneName switch
        {
            "SUVZone" => new Color(1f,0.58f,0.2f),
            "EVZone" => new Color(0.2f,0.95f,0.68f),
            "CustomizationStudio" => new Color(0.76f,0.55f,1f),
            _ => new Color(0.88f,0.72f,0.46f)
        };
        Label(canvasObj.transform,"Scene Title",title,17,TextAnchor.MiddleRight,new Vector2(0.56f,1),new Vector2(1,1),new Vector2(0,0),new Vector2(-28,0),titleAccent);
        if (menu)
        {
            Panel(canvasObj.transform,"Hero Backing",new Vector2(0.2f,0.54f),new Vector2(0.8f,0.88f),Vector2.zero,Vector2.zero,new Color(0.025f,0.045f,0.075f,0.72f));
            Label(canvasObj.transform,"Hero Eyebrow","AUTOVERSE  /  PRIVATE MOTOR COLLECTION",13,TextAnchor.MiddleCenter,new Vector2(0.14f,0.84f),new Vector2(0.86f,0.88f),Vector2.zero,Vector2.zero,new Color(0.9f,0.73f,0.46f));
            Label(canvasObj.transform,"Hero Title","DISCOVER YOUR\nNEXT GREAT DRIVE.",48,TextAnchor.MiddleCenter,new Vector2(0.14f,0.66f),new Vector2(0.86f,0.83f),Vector2.zero,Vector2.zero,new Color(0.96f,0.95f,0.92f));
            Panel(canvasObj.transform,"Hero Accent Line",new Vector2(0.46f,0.64f),new Vector2(0.54f,0.645f),Vector2.zero,Vector2.zero,new Color(0.9f,0.72f,0.42f));
            Label(canvasObj.transform,"Hero Subtitle","A CURATED COLLECTION OF PERFORMANCE, ELECTRICITY AND DESIGN",14,TextAnchor.MiddleCenter,new Vector2(0.12f,0.57f),new Vector2(0.88f,0.63f),Vector2.zero,Vector2.zero,new Color(0.7f,0.76f,0.82f));
            Button(canvasObj.transform,"ENTER SHOWROOM","EXPLORE SHOWROOM",new Vector2(0.37f,0.45f),new Vector2(0.63f,0.52f),()=>controller.LoadScene("MainShowroom"),true);
            Label(canvasObj.transform,"Menu Hint","EXPLORE BY COLLECTION",13,TextAnchor.MiddleCenter,new Vector2(0.2f,0.34f),new Vector2(0.8f,0.4f),Vector2.zero,Vector2.zero,new Color(0.75f,0.7f,0.61f));
            Button(canvasObj.transform,"Go SHOWROOM","SHOWROOM",new Vector2(0.08f,0.25f),new Vector2(0.28f,0.32f),Vector2.zero,Vector2.zero,()=>controller.LoadScene("MainShowroom"),false);
            Button(canvasObj.transform,"Go SUV ZONE","SUV ZONE",new Vector2(0.31f,0.25f),new Vector2(0.49f,0.32f),Vector2.zero,Vector2.zero,()=>controller.LoadScene("SUVZone"),false);
            Button(canvasObj.transform,"Go EV ZONE","EV ZONE",new Vector2(0.51f,0.25f),new Vector2(0.69f,0.32f),Vector2.zero,Vector2.zero,()=>controller.LoadScene("EVZone"),false);
            Button(canvasObj.transform,"Go LUXURY CAR","LUXURY CAR",new Vector2(0.72f,0.25f),new Vector2(0.92f,0.32f),Vector2.zero,Vector2.zero,()=>controller.LoadScene("CustomizationStudio"),false);
        }
        else
        {
            Panel(canvasObj.transform,"Info Card",new Vector2(0,0),new Vector2(0,0),new Vector2(26,24),new Vector2(385,255),new Color(0.025f,0.04f,0.07f,0.91f));
            string model=sceneName=="SUVZone"?"TERRA X7":sceneName=="EVZone"?"VOLT E1":"AURORA GT";
            controller.modelText=Label(canvasObj.transform,"Model",model,26,TextAnchor.MiddleLeft,new Vector2(0,0),new Vector2(0,0),new Vector2(48,218),new Vector2(365,253),Color.white);
            string details=sceneName=="EVZone"?"ALL ELECTRIC  •  520 KM RANGE\nFast charge ready  /  0–100 in 4.8 sec":"PREMIUM PERFORMANCE\nInteractive model  /  Tap car parts to inspect";
            controller.specificationsText=Label(canvasObj.transform,"Specifications",details,15,TextAnchor.UpperLeft,new Vector2(0,0),new Vector2(0,0),new Vector2(48,165),new Vector2(368,218),new Color(0.64f,0.76f,0.87f));
            Label(canvasObj.transform,"Status","Ready. Select a control to explore.",14,TextAnchor.MiddleLeft,new Vector2(0,0),new Vector2(0,0),new Vector2(48,132),new Vector2(370,164),new Color(0.3f,0.84f,1f));
            controller.statusText=canvasObj.transform.Find("Status").GetComponent<Text>();
            float x=48;
            Button(canvasObj.transform,"Rotate Left","◀",new Vector2(0,0),new Vector2(0,0),new Vector2(x,76),new Vector2(x+58,122),controller.RotateLeft,false);
            Button(canvasObj.transform,"Pause Rotation","■",new Vector2(0,0),new Vector2(0,0),new Vector2(x+64,76),new Vector2(x+122,122),controller.StopRotation,false);
            Button(canvasObj.transform,"Rotate Right","▶",new Vector2(0,0),new Vector2(0,0),new Vector2(x+128,76),new Vector2(x+186,122),controller.RotateRight,false);
            Button(canvasObj.transform,"Door Control","DOORS",new Vector2(0,0),new Vector2(0,0),new Vector2(x,25),new Vector2(x+108,68),controller.ToggleDoors,false);
            Button(canvasObj.transform,"Headlight Control","LIGHTS",new Vector2(0,0),new Vector2(0,0),new Vector2(x+114,25),new Vector2(x+222,68),controller.ToggleLights,false);
            string[] names={"MainShowroom","SUVZone","EVZone","CustomizationStudio"}; string[] labels={"SHOWROOM","SUV","EV","LUXURY CAR"};
            for(int i=0;i<names.Length;i++)
                Button(canvasObj.transform,"Nav " + labels[i],labels[i],new Vector2(0,1),new Vector2(0,1),new Vector2(28+i*132,-126),new Vector2(151+i*132,-78),()=>controller.LoadScene(names[i]),false);
            Button(canvasObj.transform,"Home","HOME",new Vector2(1,1),new Vector2(1,1),new Vector2(-150,-126),new Vector2(-28,-78),()=>controller.LoadScene("MainMenu"),false);
            Label(canvasObj.transform,"Hint","Click the car to inspect parts  •  Use controls to rotate, open doors and switch lights",14,TextAnchor.MiddleCenter,new Vector2(0.28f,0),new Vector2(1,0),new Vector2(0,22),new Vector2(-20,58),new Color(0.7f,0.8f,0.9f));
        }
    }

    private static void SceneButton(Transform canvas,string label,string scene,float y)
    { Button(canvas,"Go " + label,label,new Vector2(0.5f,0),new Vector2(0.5f,0),new Vector2(-400,y*900),new Vector2(400,y*900+48),()=>controller.LoadScene(scene),false); }

    private static GameObject Panel(Transform parent,string name,Vector2 amin,Vector2 amax,Vector2 min,Vector2 max,Color color)
    {
        GameObject go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
        RectTransform rt=go.GetComponent<RectTransform>(); rt.anchorMin=amin; rt.anchorMax=amax; rt.offsetMin=min; rt.offsetMax=max;
        go.GetComponent<Image>().color=color; return go;
    }
    private static Text Label(Transform parent,string name,string value,int size,TextAnchor anchor,Vector2 amin,Vector2 amax,Vector2 min,Vector2 max,Color color)
    {
        GameObject go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
        RectTransform rt=go.GetComponent<RectTransform>(); rt.anchorMin=amin; rt.anchorMax=amax; rt.offsetMin=min; rt.offsetMax=max;
        Text text=go.GetComponent<Text>(); text.font=Font(); text.text=value; text.fontSize=size; text.alignment=anchor; text.color=color; text.horizontalOverflow=HorizontalWrapMode.Wrap; text.verticalOverflow=VerticalWrapMode.Overflow; return text;
    }
    private static Button Button(Transform parent,string name,string value,Vector2 amin,Vector2 amax,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action,bool primary)
    {
        GameObject go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button)); go.transform.SetParent(parent,false);
        RectTransform rt=go.GetComponent<RectTransform>(); rt.anchorMin=amin; rt.anchorMax=amax; rt.offsetMin=min; rt.offsetMax=max;
        Image image=go.GetComponent<Image>(); image.color=primary?new Color(0.67f,0.5f,0.27f,0.98f):new Color(0.055f,0.07f,0.095f,0.97f);
        Button button=go.GetComponent<Button>(); button.onClick.AddListener(action);
        Text label=Label(go.transform,"Label",value,primary?20:18,TextAnchor.MiddleCenter,Vector2.zero,Vector2.one,new Vector2(8,5),new Vector2(-8,-5),new Color(1f,0.99f,0.96f));
        label.fontStyle=FontStyle.Bold;
        label.resizeTextForBestFit=true; label.resizeTextMinSize=13; label.resizeTextMaxSize=primary?20:18;
        Outline outline=label.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(0.015f,0.02f,0.03f,0.85f); outline.effectDistance=new Vector2(1f,-1f);
        label.raycastTarget=false;
        return button;
    }
    private static Button Button(Transform parent,string name,string value,Vector2 anchorMin,Vector2 anchorMax,UnityEngine.Events.UnityAction action,bool primary)
    { return Button(parent,name,value,anchorMin,anchorMax,Vector2.zero,Vector2.zero,action,primary); }
    private static Material Mat(Color color,float metallic=0.1f)
    { Material m=new Material(Shader.Find("Standard")); m.color=color; if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic); if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",0.65f); return m; }
    private static GameObject Cube(string name,Vector3 pos,Vector3 scale,Material mat,Transform parent=null)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.position=pos; go.transform.localScale=scale; if(parent)go.transform.SetParent(parent,true); go.GetComponent<Renderer>().sharedMaterial=mat; return go;
    }
    private static Font Font()
    { if(font)return font; font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); return font; }
}






