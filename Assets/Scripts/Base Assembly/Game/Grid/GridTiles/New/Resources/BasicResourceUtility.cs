using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Base_Assembly.Game.Grid.GridTiles.New.Resources
{
    public static class BasicResourceUtility
    {
        public static void Add(byte[] destinationTypes, short[] destinationValues, byte[] toAddTypes, short[] toAddValues)
        {
            for (int i = 0; i < toAddTypes.Length; i++)
            {
                for (int j = 0; j < destinationTypes.Length; j++)
                {
                    destinationValues[j] += (short)math.select(0, toAddValues[i], destinationTypes[j] == toAddTypes[i]);
                }
            }
        }
        
        /*

        protected virtual int Remove(ResourceType type, int change)
        {
            int i = types.IndexOf(type);
            if (i == -1)
            {
                Debug.LogWarning($"{type} not present in this res: {this}");
                return 0;
            }
            else
                ammounts[i] -= change;
            return change;
        }

        public void Manage(Resource resAmmount, bool add, float mod = 1, bool removeEmpty = false)
        {
            for (int i = resAmmount.types.Count - 1; i > -1; i--)
                ManageSimple(resAmmount.types[i], resAmmount.ammounts[i], add, mod, removeEmpty);
        }

        public void ManageSimple(ResourceType type, float ammount, bool add, float mod = 1, bool removeEmpty = false)
        {
            if (add)
                Add(type, Mathf.RoundToInt(ammount * mod));
            else
            {
                Remove(type, Mathf.RoundToInt(ammount * mod));
                if (removeEmpty)
                {
                    int i = types.IndexOf(type);
                    if (i == -1)
                    {
                        Debug.LogWarning("No resource to remove(not present)");
                        return;
                    }
                    if (ammounts[i] <= 0)
                    {
                        if (ammount < 0)
                            Debug.LogError("removed too much");
                        types.RemoveAt(i);
                        ammounts.RemoveAt(i);
                    }
                }
            }
        }

        public void TakeResource(Resource toTransfer)
        {
            Resource ret = toTransfer - Diff(toTransfer);

            // check if present
            Manage(ret, false);
            toTransfer.Manage(ret, false, removeEmpty: true);
        }

        public Resource GetResOfAmmount(Resource ret, int _ammount, bool remove)
        {
            for (int i = types.Count - 1; i > -1; i--)
            {
                int x = (ammounts[i] < _ammount)
                    ? ammounts[i]
                    : _ammount;
                ret.ManageSimple(types[i], x, true);
                if (remove)
                    ManageSimple(types[i], x, false);
                _ammount -= x;
                if (_ammount <= 0)
                    break;
            }
            return ret;
        }

        protected void BaseDiff(Resource ret, Resource cost)
        {
            for (int i = 0; i < cost.types.Count; i++)
            {
                int j = types.IndexOf(cost.types[i]);
                if (j > -1)
                {
                    int x = cost.ammounts[i] - ammounts[j];
                    if (x > 0)
                    {
                        ret.types.Add(cost.types[i]);
                        ret.ammounts.Add(x);
                    }
                }
                else
                {
                    ret.types.Add(cost.types[i]);
                    ret.ammounts.Add(cost.ammounts[i]);
                }
            }
        }

        public Resource Diff(Resource cost)
        {
            Resource diff = new();
            BaseDiff(diff, cost);
            return diff;
        }

        public Resource Diff(Resource bonusInventory, Resource cost)
        {
            Resource resource = new(this);
            resource.Manage(bonusInventory, true);
            Resource diff = resource.Diff(cost);
            return diff;
        }

        public void RemoveEmpty()
        {
            for (int i = types.Count - 1; i > -1; i--)
            {
                if (ammounts[i] == 0)
                {
                    types.RemoveAt(i);
                    ammounts.RemoveAt(i);
                }
            }
        }


        public int Sum() => ammounts.Sum();

        public Resource FilterByType(List<ResourceWrapper> objects, out int activeElem)
        {
            Resource resource = new();
            activeElem = 0;
            for (int i = 0; i < objects.Count; i++)
            {
                ResourceType type = objects[i].data;
                int j = types.FindIndex(q => q == type);

                resource.Add(type, j > -1 ? ammounts[j] : 0);
                activeElem++;
            }
            return resource;
        }

        public static Resource operator -(Resource a, Resource b)
        {
            Resource res = new(a);
            res.Manage(b, false, removeEmpty: true);
            return res;
        }

        public void Clear()
        {
            ammounts.Clear();
            types.Clear();
        }
*/
    }
}
