using Assets.Scripts.Systems.Human.JobFind;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace JobFind
{
    public class JobOfferLinker : MonoBehaviour, IAfterLoad, IDigManager
    {
        NativeList<int> digJob;
        NativeList<uint> digRegions;
        NativeList<int> impossibleToDig;
        NativeList<Entity> buildJob;

        NativeList<int> toDigMarks;
        NativeList<DigAssign> assignedJobs;
        NativeList<int> positionsToCancel;

        /*public void BeforeInit()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var ent = manager.CreateEntity();

            digJob = new NativeList<int>(50, Allocator.Persistent);
            buildJob = new NativeList<Entity>(20, Allocator.Persistent);
            toDigMarks = new NativeList<int>(10, Allocator.Persistent);
            manager.SetComponentData(ent, new JobOffers()
            {
                digJobs = digJob,
                buildingJobs = buildJob
            });
        }*/

        private void OnDestroy()
        {
            if (digJob.IsCreated)
                digJob.Dispose();
            if (digRegions.IsCreated)
                digRegions.Dispose();
            if (impossibleToDig.IsCreated)
                impossibleToDig.Dispose();

            if (buildJob.IsCreated)
                buildJob.Dispose();

            if(toDigMarks.IsCreated)
                toDigMarks.Dispose();

            if (assignedJobs.IsCreated)
                assignedJobs.Dispose();

            if (positionsToCancel.IsCreated)
                positionsToCancel.Dispose();
        }


        public void AddJobs(NativeArray<int> ints)
        {
            foreach (var item in ints)
            {
                if(!digJob.Contains(item))
                    digJob.Add(item);
            }
        }
        public void RemoveJobs(NativeArray<int> ints)
        {
            foreach (var item in ints)
            {
                int index = digJob.IndexOf(item);
                if (index != -1)
                    digJob.RemoveAt(index);
            }
        }

        public void ResetMark()
        {
            toDigMarks.Clear();
        }

        public void MarkTile(int mark)
        {
            toDigMarks.Add(mark);
        }

        public void ApplyMark(bool add)
        {
            if(add)
                digJob.AddRange(toDigMarks);
            else
            {
                positionsToCancel.AddRange(toDigMarks);
            }
            toDigMarks.Clear();
        }

        public void AfterLoad()
        {
            instance = this;

            EntityManager  manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = manager.CreateEntity();

            digJob = new NativeList<int>(50, Allocator.Persistent);
            digRegions = new NativeList<uint>(50, Allocator.Persistent);
            impossibleToDig = new NativeList<int>(50, Allocator.Persistent);
            buildJob = new NativeList<Entity>(20, Allocator.Persistent);
            toDigMarks = new NativeList<int>(10, Allocator.Persistent);

            assignedJobs = new NativeList<DigAssign>(10, Allocator.Persistent);
            positionsToCancel = new NativeList<int>(10, Allocator.Persistent);


            manager.AddComponentData(entity, new JobOffersComponent()
            {
                digJobs = digJob,
                digRegions = digRegions,
                impossibleToDig = impossibleToDig,
                assignedJobs = assignedJobs,
                buildingJobs = buildJob
            });
            manager.AddComponentData(entity, new JobCancelComponent()
            {
                positionsToCancel = positionsToCancel
            });
        }

        public static JobOfferLinker instance;

        public bool GetAssignedHuman(int globalPosition, out Entity assigned) 
        {
            foreach (var item in assignedJobs)
            {
                if(item.position == globalPosition)
                {
                    assigned = item.assignHuman;
                    return true;
                }
            }

            assigned = default;
            return false;
        }
    }
}
