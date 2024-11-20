using System.Collections.Generic;


class Queue
{
    List<PathNode> queue;
    List<PathNode> visited;
    public Queue(GridPos startGp)
    {
        queue = new() { new(startGp, 0, null) };
        visited = new();
    }

    void BubbleUp(int index)
    {
        int parent = index;
        while ((parent = (parent - 1) / 2) > -1)
        {
            PathNode parentNode = queue[parent];
            if (parentNode.minCost <= queue[index].minCost)
            {
                break;
            }
            queue[parent] = queue[index];
            queue[index] = parentNode;
            index = parent;
        }
    }

    void BubbleDown(int index)
    {
        int parent = index;
        while ((parent = 1 + (parent * 2)) < queue.Count)
        {
            PathNode l = queue[parent];
            if (parent + 1 < queue.Count)
            {
                PathNode r = queue[parent + 1];
                if (l.minCost > r.minCost)
                {
                    parent++;
                    l = r;
                }
            }
            if (l.minCost > queue[index].minCost)
            {
                break;
            }

            queue[parent] = queue[index];
            queue[index] = l;
            index = parent;
        }
    }

    public bool Enqueue(PathNode pN)
    {
        int i;
        if ((i = visited.IndexOf(pN)) != -1)
        {
            if (visited[i].minCost > pN.minCost)
            {
                visited[i].minCost = pN.minCost;
                visited[i].previous = pN.previous;
                queue.Add(visited[i]);
            }
            else
            {
                return false;
            }
        }
        else
        {
            visited.Add(pN);
            queue.Add(pN);
        }
        BubbleUp(queue.Count - 1);
        return true;
    }

    public PathNode Dequeue()
    {
        if (queue.Count > 0)
        {
            PathNode p = queue[0];
            queue[0] = queue[^1];
            queue.RemoveAt(queue.Count - 1);
            BubbleDown(0);
            return p;
        }
        return null;
    }
}