using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
public struct Flock_settings : IComponentData
{
   
    public float3 direction;
    public float3 velocity;
    public int index;
    public float3 flockHeading;             
    public float3 flockCentre;
    public float3 separationHeading;
    public int numFlockmates;



}
