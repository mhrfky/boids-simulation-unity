using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;



public class Spawner : MonoBehaviour
{
    [SerializeField] private Mesh boidMesh;
    [SerializeField] private Material boidMaterial;
    [SerializeField] private Mesh baitMesh;
    [SerializeField] private Material baitMaterial;
    void Start()
    {
        for (int i = 0; i < 1000; i++)
        {
            makeEntity(i);
        }
        makeBait();

    }
    private void makeBait()
    {
        Unity.Entities.EntityManager eM = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype bait = eM.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderBounds),
            typeof(bait),
            typeof(LocalToWorld));
        Entity entity = eM.CreateEntity(bait);
        eM.AddComponentData(entity, new Translation
        {
            Value = new float3(0, 0, 0)
        }
            );
        eM.SetSharedComponentData(entity, new RenderMesh
        {
            mesh = baitMesh,
            material = baitMaterial
        }
           );
    }
    
    private void makeEntity(int i)
    {
        Unity.Entities.EntityManager eM = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype boid = eM.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderBounds),
            typeof(Flock_settings),
            typeof(LocalToWorld));
        Entity entity = eM.CreateEntity(boid);
        eM.AddComponentData(entity, new Translation
        {
            Value = new float3(i%24 ,(int) i/4, 0)
        }
            ) ;
        eM.AddComponentData(entity, new Flock_settings
        {
            direction = new float3(0,0,0),
            flockHeading = new float3(0, 0, 0),
            flockCentre = new float3(0, 0, 0),
            separationHeading = new float3(0, 0, 0),
            velocity = new float3(0,0,0),
            index = i,
            numFlockmates = 0
}
            );
        eM.SetSharedComponentData(entity, new RenderMesh
        {
                mesh = boidMesh,
                material = boidMaterial
        }
            );
        eM.AddComponentData(entity, new Scale
        {
            Value = .1f
        }
            ) ;
    }
}
