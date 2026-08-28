using System;
using UnityEngine;

namespace FFTestMod;

public class CustomFactoryTransition : MonoBehaviour {
    public static CustomFactoryTransition INSTANCE;
    
    private RenderTexture innerRT;
    public const int INNER_LAYER = 1;
    private Camera outerCamera;
    private Camera innerCamera;
    private GameObject portal;
    private Mesh portalMesh;
    private Material portalMaterial;
    private MeshFilter portalMeshFilter;
    private MeshRenderer portalMeshRenderer;
    // private Stage
    
    private void Awake() {
        INSTANCE = this;
        
        portalMesh = new Mesh();
        const float PORTAL_SIZE = 4.5f;
        portalMesh.vertices = [
            new(-PORTAL_SIZE, 0f, -PORTAL_SIZE),
            new(-PORTAL_SIZE, 0f, +PORTAL_SIZE),
            new(+PORTAL_SIZE, 0f, +PORTAL_SIZE),
            new(+PORTAL_SIZE, 0f, -PORTAL_SIZE)
        ];
        portalMesh.triangles = [0, 1, 2, 0, 2, 3];
        portalMesh.RecalculateBounds();
        portalMesh.RecalculateNormals();
        
        portal = new GameObject("Portal");
        portal.layer = 0;
        portalMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        portalMeshFilter = portal.AddComponent<MeshFilter>();
        portalMeshFilter.mesh = portalMesh;
        portalMeshRenderer = portal.AddComponent<MeshRenderer>();
        portalMeshRenderer.material = portalMaterial;
        
        updateRT();
    }

    private void Update() {
        updateRT();
        
        
        innerCamera.targetTexture = innerRT;
    }

    private void updateRT() {
        if (!innerRT || innerRT.width != Screen.width || innerRT.height != Screen.height) {
            if (innerRT) innerRT.Release();
            innerRT = new RenderTexture(Screen.width, Screen.height, 24);
            innerRT.Create();
            portalMaterial.SetTexture("_BaseMap", innerRT);
        }
    }
}