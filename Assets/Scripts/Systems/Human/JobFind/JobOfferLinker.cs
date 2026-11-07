using Assets.Scripts.Systems.Human.JobFind;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace JobFind
{
    class JobOfferLinker : MonoBehaviour, IAfterLoad, IDigManager
    {
        NativeList<int> digJob;
        NativeList<Entity> buildJob;

        NativeList<int> toDigMarks;

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

            if (buildJob.IsCreated)
                buildJob.Dispose();

            if(toDigMarks.IsCreated)
                toDigMarks.Dispose();
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
                foreach (int mark in toDigMarks)
                {
                    int index = digJob.IndexOf(mark);
                    if (index > -1)
                        digJob.RemoveAt(index);
                }
            }
            toDigMarks.Clear();
        }

        public void AfterLoad()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var ent = manager.CreateEntity();

            digJob = new NativeList<int>(50, Allocator.Persistent);
            buildJob = new NativeList<Entity>(20, Allocator.Persistent);
            toDigMarks = new NativeList<int>(10, Allocator.Persistent);
            manager.AddComponentData(ent, new JobOfferComponent()
            {
                digJobs = digJob,
                buildingJobs = buildJob
            });
        }
    }
}
