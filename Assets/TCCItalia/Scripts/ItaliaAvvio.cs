using System.Linq;
using UnityEngine;
namespace Viaggio.Italia {
public sealed class ItaliaAvvio : MonoBehaviour {
    Transform parent;
    public bool interior;
    public BoxCollider pisoRua;
    [Header("Restaurante")]
    public Transform chefMarco;
    public Transform chegadaSofia;
    public Transform saidaRestaurante;
    GameObject Model(string name,string file,Vector3 foot,float dimension,string texture=null) {
        var prefab=Resources.Load<GameObject>("ItaliaPronta/"+file);
        var anchor=new GameObject(name); anchor.transform.SetParent(parent);
        var o=Instantiate(prefab); o.transform.SetParent(anchor.transform,false);
        var rs=o.GetComponentsInChildren<Renderer>(); var bounds=rs[0].bounds;
        foreach(var r in rs) bounds.Encapsulate(r.bounds);
        float scale=dimension/Mathf.Max(.001f,(file=="FerrariPremio"?Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z):bounds.size.y));
        o.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        anchor.transform.localScale=Vector3.one*scale; anchor.transform.position=foot;
        if(texture!=null){
            var tex=Resources.Load<Texture2D>("ItaliaPronta/"+texture);
            foreach(var r in rs)foreach(var mat in r.materials) {
                if(mat && tex){mat.mainTexture=tex;if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",tex);}
            }
        }
        return anchor;
    }
    static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
    Vector3 BridgeStart(GameObject[] roots, ItaliaFase stage){
        var pieces=roots.Where(o=>o.name=="bridge_02" || o.name.StartsWith("bridge_01_piece_02")).ToArray();
        var surfaces=new System.Collections.Generic.List<MeshCollider>();
        Bounds bounds=pieces[0].GetComponentInChildren<Renderer>().bounds;
        foreach(var piece in pieces){
            foreach(var filter in piece.GetComponentsInChildren<MeshFilter>()){
                if(!filter.sharedMesh)continue;
                var collider=filter.GetComponents<MeshCollider>().FirstOrDefault(c=>c.enabled && !c.isTrigger && c.sharedMesh==filter.sharedMesh);
                if(!collider){collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;}
                surfaces.Add(collider);
                bounds.Encapsulate(collider.bounds);
            }
        }
        Physics.SyncTransforms();
        stage.bridge=bounds;
        float inset=Mathf.Min(bounds.size.z*.15f,Mathf.Max(1f,stage.unit));
        float step=Mathf.Max(.1f,inset*.25f);
        for(float z=bounds.max.z-inset;z>=bounds.min.z;z-=step){
            var ray=new Ray(V(bounds.center.x,bounds.max.y+2,z),Vector3.down);
            float top=float.NegativeInfinity;
            foreach(var surface in surfaces){
                if(surface.Raycast(ray,out var hit,bounds.size.y+5) && hit.normal.y>.65f)
                    top=Mathf.Max(top,hit.point.y);
            }
            if(!float.IsNegativeInfinity(top))return V(bounds.center.x,top+.08f*stage.unit,z);
        }
        throw new System.InvalidOperationException("Não foi encontrada a superfície da ponte para posicionar Sofia.");
    }
    void CompleteCanal(GameObject[] roots){
        var scenery = new GameObject("Margens do canal").transform;
        scenery.SetParent(parent, false);
        var masonry = roots.First(o => o.name == "Cube").GetComponentInChildren<Renderer>().sharedMaterial;
        void Quay(string label, Vector3 low, Vector3 high){
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = label;
            block.transform.SetParent(scenery, false);
            block.transform.position = (low + high) * .5f;
            block.transform.localScale = high - low;
            block.GetComponent<Renderer>().sharedMaterial = masonry;
            block.GetComponent<Collider>().enabled = false;
        }
        GameObject Extend(string source, Vector3 offset){
            var original = roots.First(o => o.name == source);
            var copy = Instantiate(original, scenery, true);
            copy.transform.position += offset;
            foreach(var c in copy.GetComponentsInChildren<Collider>()) c.enabled = false;
            return copy;
        }
        var north = Extend("building_2", V(0,-13,81.6f));
        var east = Extend("building_5", V(118.2f,0,0));
        foreach(var house in roots.Where(o => o.name.StartsWith("building_") || o.name == "Modular European House Variant").Concat(new[]{north,east})){
            var renderers = house.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            Quay("Fundação - " + house.name, V(bounds.min.x,-8,bounds.min.z), V(bounds.max.x,bounds.min.y+.1f,bounds.max.z));
        }
        Quay("Cais da cabeceira", V(666,-8,579.6f), V(723,4,624));
        Extend("WaterBlock_50m", V(0,0,411.44f));
    }
    void Awake(){
        var scene=gameObject.scene;
        // As peças de rua ficam diretamente dentro dos grupos organizadores da cena.
        var roots = scene.GetRootGameObjects();
        var streetObjects = roots.SelectMany(o => o.name == "Cenário" || o.name == "Personagens"
            ? o.transform.Cast<Transform>().Select(t => t.gameObject)
            : new[] { o }).ToArray();
        parent=transform;
        var stage=gameObject.AddComponent<ItaliaFase>();stage.interior=interior;
            float floor=interior?.55f:(pisoRua ? pisoRua.bounds.max.y : 17.12f);
            Bounds entrance=new Bounds();
            Vector3 outward=Vector3.forward;
            if(!interior){
                var house=streetObjects.First(o=>o.name=="Modular European House Variant");
                var door=house.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Door" && t.GetComponentsInParent<Transform>(true).Any(p=>p.name=="Parts"));
                var meshes=door.GetComponentsInChildren<Renderer>();
                entrance=meshes[0].bounds;
                foreach(var mesh in meshes)entrance.Encapsulate(mesh.bounds);
                // Face the street on the side of the house occupied by its real door.
                var street=Vector3.ProjectOnPlane(V(682,floor,405)-entrance.center,Vector3.up);
                outward=entrance.size.x>entrance.size.z?Vector3.forward:Vector3.right;
                if(Vector3.Dot(outward,street)<0)outward=-outward;
                stage.unit=Mathf.Max(.1f,entrance.size.y*.78f/1.75f);
                stage.outward=outward;
            }
            GameObject floorObj=null;
            if(interior || !pisoRua){
            floorObj=new GameObject("Colisão do percurso");
            floorObj.transform.SetParent(parent);
            floorObj.transform.position=interior?V(0,floor-.15f,0):V(717,floor-.15f,405);
            floorObj.AddComponent<BoxCollider>().size=interior?V(15,.3f,15):V(98,.3f,22);
            }
            if(interior){
                // A área jogável acompanha o piso que sustenta a Sofia.
                // Não usar coordenadas rígidas que cortam o salão ao meio.
                var building = roots.SelectMany(o => o.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "Dinner_building");
                if(building){
                    var surfaces = building.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.sharedMaterials.Any(m => m && m.name.ToLowerInvariant().Contains("floor"))).ToArray();
                    if(surfaces.Length > 0){
                        var bounds = surfaces[0].bounds;
                        foreach(var surface in surfaces) bounds.Encapsulate(surface.bounds);
                        floorObj.transform.position = V(bounds.center.x,floor-.15f,bounds.center.z);
                        floorObj.GetComponent<BoxCollider>().size = V(bounds.size.x,.3f,bounds.size.z);
                    }
                    foreach(var c in building.GetComponentsInChildren<Collider>(true)){
                        if(!c.isTrigger) c.enabled = false;
                    }
                }
                Physics.SyncTransforms();
                stage.restaurantWalkArea = floorObj.GetComponent<BoxCollider>().bounds;
                // Mesas e cadeiras mantêm colisão; objetos decorativos pequenos não
                // devem funcionar como obstáculos invisíveis no caminho.
                // Rebuild only physical obstacles from visible furniture geometry.
                var props = roots.SelectMany(o => o.GetComponentsInChildren<Collider>(true)).ToArray();
                foreach(var c in props){
                    if(!c || c.isTrigger || c.gameObject == floorObj) continue;
                    c.enabled = false;
                }
                foreach(var r in roots.SelectMany(o => o.GetComponentsInChildren<MeshRenderer>())){
                    if(!r.enabled) continue;
                    var n=r.name.ToLowerInvariant();
                    if(!(n.Contains("table") || n.Contains("chair") || n.Contains("counter") || n.Contains("sofa") || n.Contains("stool"))) continue;
                    var b=r.bounds;
                    if(b.size.x<.15f || b.size.z<.15f || b.max.y<floor+.2f || b.min.y>floor+1.3f) continue;
                    var obstacle=new GameObject("Furniture collision - "+r.name);
                    obstacle.transform.SetParent(parent);
                    obstacle.transform.position=b.center;
                    obstacle.AddComponent<BoxCollider>().size=b.size;
                }
            }
            if(interior){
                var walls = new System.Collections.Generic.List<Collider>();
                var building = roots.SelectMany(o => o.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "Dinner_building");
                if(building){
                    foreach(var mesh in building.GetComponentsInChildren<MeshFilter>()){
                        if(!mesh.sharedMesh) continue;
                        var wall = mesh.gameObject.AddComponent<MeshCollider>();
                        wall.sharedMesh=mesh.sharedMesh;

                        walls.Add(wall);
                    }
                }
                stage.restaurantCameraWalls=walls.ToArray();
                stage.npc=chefMarco;
                if(!stage.npc){
                    stage.npc=Model("Chef Marco","chef",V(6.2f,.40f,.65f),1.85f,"chef.001").transform;
                    stage.npc.rotation=Quaternion.Euler(0,-105,0);
                }
            }else{
                if(!transform.Find("Margens do canal")) CompleteCanal(streetObjects);
                var guide=streetObjects.First(o=>o.name=="guiaturistico");
                stage.npc=new GameObject("Interação com o guia").transform;stage.npc.SetParent(parent);stage.npc.position=V(682,floor,405);
                var rs=guide.GetComponentsInChildren<Renderer>();
                var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);
                guide.transform.localScale*=1.85f*stage.unit/Mathf.Max(.01f,bounds.size.y);
                bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);
                guide.transform.position+=V(682,floor,405)-V(bounds.center.x,bounds.min.y,bounds.center.z);
            }
            if(interior && saidaRestaurante)stage.door=saidaRestaurante;
            else {
                stage.door=new GameObject(interior?"Saída":"Entrada da pizzaria").transform;
                stage.door.SetParent(parent);
                if(interior)stage.door.position=V(-4,floor,-1);
            }
            if(!interior){
                float thickness=Mathf.Abs(outward.x)*entrance.extents.x+Mathf.Abs(outward.z)*entrance.extents.z;
                Vector3 center=V(entrance.center.x,floor,entrance.center.z);
                stage.door.position=center+outward*(thickness+.55f*stage.unit);
                if(pisoRua) stage.area=pisoRua.bounds;
                else {
                var area=new Bounds(V(717,floor,405),V(110,.3f,22));
                area.Encapsulate(stage.door.position+outward*(3*stage.unit));
                area.Encapsulate(stage.door.position);
                area.Expand(V(4*stage.unit,0,4*stage.unit));
                floorObj.transform.position=V(area.center.x,floor-.15f,area.center.z);
                floorObj.GetComponent<BoxCollider>().size=V(area.size.x,.3f,area.size.z);
                stage.area=area;
                }
            }
            stage.player=new GameObject("Sofia").transform;stage.player.tag="Player";stage.player.position=interior?(chegadaSofia?chegadaSofia.position:V(-2,floor+.05f,-1)):BridgeStart(streetObjects,stage);
            var sofia = Instantiate(Resources.Load<GameObject>("ItaliaPronta/SofiaAnimada"), stage.player, false);
            sofia.transform.localScale = Vector3.one * stage.unit;
            stage.animator = sofia.GetComponentInChildren<Animator>();
            stage.animator.runtimeAnimatorController=Resources.Load<RuntimeAnimatorController>("ItaliaPronta/SofiaRestaurante");
            var cc=stage.player.gameObject.AddComponent<CharacterController>(); cc.height=1.75f*stage.unit;cc.radius=.28f*stage.unit;cc.center=V(0,.89f*stage.unit,0);cc.stepOffset=.3f*stage.unit; cc.skinWidth=.025f*stage.unit; cc.minMoveDistance=0; cc.slopeLimit=50; foreach(var wall in stage.restaurantCameraWalls) Physics.IgnoreCollision(cc,wall,true);
            stage.cameraFase=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Camera>()).First(c=>c.CompareTag("MainCamera"));
            if(interior)stage.cameraFase.transform.rotation=Quaternion.Euler(28,17,0);
            else {
                var quay = streetObjects.FirstOrDefault(o => o.name == "Cube");
                var surface = quay ? quay.GetComponentInChildren<Renderer>() : null;
                stage.SetupStreetCamera(surface ? surface.bounds : stage.area);
            }
            if(interior)stage.cameraFase.fieldOfView=43;
            stage.cameraFase.nearClipPlane=.05f;
            stage.cameraFase.transform.position=stage.CameraPosition();
            {stage.reward=Model("Miniatura Ferrari F40","FerrariPremio",V(0,0,0),2,"f40").transform; stage.reward.gameObject.SetActive(false);}
    }
}}
