using System.Collections.Generic;
using UnityEngine;
namespace WildType
{
    // UI-only projection: one vertex per stable ID, including recorded co-parents/ancestors.
    // Coordinates never depend on names or an inferred death. All work is bounded by the archive.
    public sealed class FamilyTreeLayout
    {
        public const float CardWidth=326, CardHeight=148, Column=374, Row=236, Margin=24;
        public readonly List<CreatureLineageRecord> Records=new List<CreatureLineageRecord>();
        public readonly Dictionary<CreatureId,Vector2> Positions=new Dictionary<CreatureId,Vector2>();
        public readonly List<(CreatureId parent,CreatureId child)> Links=new List<(CreatureId,CreatureId)>();
        public Vector2 Size {get;private set;}
        public void Build(LineageArchive archive,IEnumerable<CreatureLineageRecord> family)
        {
            Records.Clear();Positions.Clear();Links.Clear();
            var included=new HashSet<CreatureId>();var pending=new Stack<CreatureId>();
            foreach(var r in family)pending.Push(r.CreatureId);
            while(pending.Count>0){var id=pending.Pop();var r=archive.Get(id);if(r==null||!included.Add(id))continue;
                Records.Add(r);pending.Push(r.FirstParentId);pending.Push(r.SecondParentId);}
            Records.Sort((a,b)=>{int g=a.Generation.CompareTo(b.Generation);return g!=0?g:a.CreatureId.CompareTo(b.CreatureId);});
            int generation=-1,column=0,maxColumns=1;
            foreach(var r in Records){if(generation!=r.Generation){generation=r.Generation;column=0;}
                Positions[r.CreatureId]=new Vector2(Margin+column++*Column,Margin+r.Generation*Row);maxColumns=Mathf.Max(maxColumns,column);
                foreach(var parent in new[]{r.FirstParentId,r.SecondParentId})if(included.Contains(parent))Links.Add((parent,r.CreatureId));}
            Size=new Vector2(Mathf.Max(1120,Margin*2+(maxColumns-1)*Column+CardWidth),Mathf.Max(410,Margin*2+Mathf.Max(0,generation)*Row+CardHeight));
        }
        public Rect Bounds(CreatureId id)=>new Rect(Positions[id],new Vector2(CardWidth,CardHeight));
        public CreatureId Move(CreatureId id,int dx,int dy)
        {
            if(!Positions.TryGetValue(id,out var at))return id;
            CreatureId best=id;float score=float.MaxValue;
            foreach(var r in Records){var p=Positions[r.CreatureId];float primary=dy!=0?(p.y-at.y)*dy:(p.x-at.x)*dx;
                if(primary<1|| (dy==0&&Mathf.Abs(p.y-at.y)>1))continue;
                float cost=primary*1000+(dy!=0?Mathf.Abs(p.x-at.x):0);
                if(cost<score){score=cost;best=r.CreatureId;}}
            return best;
        }
    }
}
