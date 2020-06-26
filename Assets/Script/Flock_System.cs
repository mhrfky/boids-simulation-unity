using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

using UnityEngine;
using Unity.Collections;
using System;

public class Flock_System : SystemBase
{

    private EntityQuery m_Group;
    private EntityQuery baitQuery;
    public const int threshold = 20;

    public const float minSpeed = .18f;
    public const float maxSpeed = .24f;
    public const float perceptionRadius =5f;
    public const float avoidanceRadius = 4f;
    public const float maxSteerForce = 3;

    public const float alignWeight = 1f;
    public const float cohesionWeight = 2f;                                                      
    public const float seperateWeight = 2f;
    public const float diameter = 40;

    public const float targetWeight = 1;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public const float boundsRadius = .27f;
    public const float avoidCollisionWeight = 100;
    public const float checkRadius = 5;
    public bool target = false;

    protected override void OnCreate()
    {
        m_Group = GetEntityQuery(typeof(Flock_settings), typeof(Translation));
        baitQuery = GetEntityQuery(typeof(bait), typeof(Translation));
    }

    protected override void OnUpdate()
    {

        
        var baitpos = baitQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        var poss = m_Group.ToComponentDataArray<Translation>(Allocator.TempJob);
        var moves = m_Group.ToComponentDataArray<Flock_settings>(Allocator.TempJob);
        JobHandle update = Entities.ForEach((ref Flock_settings boid, ref Translation pos, ref Rotation rot) =>
        {
            float speed = boid.velocity.x * boid.velocity.x + boid.velocity.y * boid.velocity.y + boid.velocity.z * boid.velocity.z;
            speed = math.sqrt(speed);
            float3 posToBe = pos.Value;
            posToBe += boid.velocity;
            boid.separationHeading = new float3(0,0,0);
            boid.flockHeading = new float3(0, 0, 0);
            boid.numFlockmates = 0;
            boid.flockCentre = new float3(0, 0, 0);
            pos.Value = posToBe;

            rot.Value = quaternion.LookRotation(new float3(boid.velocity.y, -boid.velocity.x, boid.velocity.z) / speed, math.up()) ; 
        }).Schedule(this.Dependency);
        update.Complete();


        JobHandle compute =  Entities.ForEach((ref Flock_settings boid, ref Translation pos) =>
        {
            
            for (int i = 0; i < poss.Length; i++)
            {
                float3 offset = poss[i].Value - pos.Value;
                float distsqr = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;
                if (perceptionRadius* perceptionRadius > distsqr)
                {
                    boid.numFlockmates++;
                    boid.flockCentre = boid.flockCentre + poss[i].Value;
                    boid.flockHeading += moves[i].velocity;
                    if(distsqr < avoidanceRadius && boid.index != i)
                    {
                        if(distsqr == 0)
                        {
                            boid.separationHeading += new float3(1, 1, 1);
                            boid.direction = new float3(6, 6, 6);
                        }
                        else    boid.separationHeading -= new float3(offset.x / distsqr, offset.y / distsqr, offset.z / distsqr);
                        
                    }
                }
            }
        }).Schedule(this.Dependency);
        compute.Complete();


        float deltaTime = Time.DeltaTime;
        JobHandle acc = Entities.ForEach((ref Flock_settings boid, ref Translation pos) =>
        {
            var Acceleration = float3.zero;

            Func<float3, float3, float3> steerTowards = (float3 x, float3 y) =>
             {
                 Vector3 velocity = new Vector3(y.x, y.y, y.z);
                 Vector3 vector = new Vector3(x.x, x.y, x.z);
                 Vector3 v = vector.normalized * maxSpeed - velocity;
                 Vector3 re = Vector3.ClampMagnitude(v, maxSteerForce);
                 return new float3(re.x, re.y, re.z);
             };
            float distsqr = pos.Value.x * pos.Value.x + pos.Value.y * pos.Value.y + pos.Value.z * pos.Value.z;
            if (distsqr > diameter * diameter)
            {
                Acceleration += steerTowards(-pos.Value, boid.velocity) * avoidCollisionWeight;
            }
            
            //float3 offsetToBait = baitpos[0].Value - pos.Value;
            //Acceleration += steerTowards(offsetToBait, boid.velocity) * targetWeight;
            

            if (boid.numFlockmates != 1) {
                boid.flockCentre = boid.flockCentre / (boid.numFlockmates);

                float3 offsetToFlockmatesCentre = boid.flockCentre - pos.Value;

                var alignmentForce = steerTowards(boid.flockHeading,boid.velocity) * alignWeight;
                var cohesionForce = steerTowards(offsetToFlockmatesCentre,boid.velocity) * cohesionWeight;
                var seperationForce = steerTowards(boid.separationHeading,boid.velocity) * seperateWeight;

                Acceleration += alignmentForce;
                Acceleration += cohesionForce;
                Acceleration += seperationForce;


                boid.velocity += Acceleration * deltaTime;
                float speed = boid.velocity.x * boid.velocity.x + boid.velocity.y * boid.velocity.y + boid.velocity.z * boid.velocity.z;
                speed = math.sqrt(speed);
                Vector3 dir = boid.velocity / speed;
                speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
                boid.velocity = dir * speed;
            }




        }).Schedule(this.Dependency);
        acc.Complete();



        Vector2 temp = UnityEngine.Random.insideUnitCircle * diameter;
        JobHandle a = Entities.ForEach((ref bait b, ref Translation pos) =>
        {
            int count = 0;
            float3 sup = b.sup;
            for (int i = 0; i < poss.Length; i++)
            {
                float3 offset = poss[i].Value - pos.Value;
                float distsqr = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;
                if (distsqr < checkRadius * checkRadius) count++;
            }

            if (count > threshold)
            {
                
                pos.Value = new float3(temp.x, temp.y, 0);
            }
        }).Schedule(this.Dependency);
        a.Complete();
        baitpos.Dispose();
        poss.Dispose();
        moves.Dispose();

    }
}


