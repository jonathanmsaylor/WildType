using UnityEngine;
namespace WildType
{
    // Cosmetic sampler of the existing TravelLimit function, never a movement modifier.
    public sealed class BiomeEntryReadout
    {
        object run;CreatureId controlled;Habitat accepted,candidate;Vector3 baseline,origin;
        Phenotype snapshot;float dwell,cooldown,window,age;bool sampling,shown;
        public string Text {get;private set;}="";
        public float Alpha=>!shown?0:Mathf.Min(Mathf.Clamp01(age/.35f),Mathf.Clamp01((7-age)/1f));
        public void Clear(){run=null;controlled=default;Text="";shown=sampling=false;age=cooldown=dwell=window=0;}
        public void Tick(object identity,CreatureId id,Phenotype stats,Vector3 point,bool paused,float dt)
        {
            if(!id.IsValid){Clear();return;}
            if(run!=identity||controlled!=id){Clear();run=identity;controlled=id;accepted=candidate=EcologyRules.Region(point);baseline=point;return;}
            if(paused)return;
            dt=Mathf.Max(0,dt);cooldown=Mathf.Max(0,cooldown-dt);
            if(shown){age+=dt;if(age>=7){shown=sampling=false;Text="";}}
            var region=EcologyRules.Region(point);
            bool settled=region==Habitat.Woodland?point.x<=-36:region==Habitat.Dry?point.x>=36:point.x>=-34&&point.x<=34;
            if(region!=accepted&&settled){if(candidate!=region){candidate=region;dwell=0;}dwell+=dt;
                if(dwell>=.6f){accepted=region;dwell=0;origin=baseline;snapshot=stats;window=0;sampling=cooldown<=0;shown=false;Text="";}}
            else {candidate=accepted;dwell=0;}
            if(region==accepted&&(point.x<=-43||point.x>=-35))baseline=point;
            if(!sampling)return;window+=dt;if(window>8){sampling=false;return;}
            string changes=Changes(snapshot,origin,point);
            if(changes.Length==0)return;
            if(!shown){shown=true;age=0;cooldown=10;}
            Text="Entering "+EcologyRules.Name(accepted)+"\n"+changes+"\nAdult limits · brush blends gradually";
        }
        public static string Changes(Phenotype stats,Vector3 from,Vector3 to)
        {
            string result="";
            for(int i=0;i<2;i++){float a=EcologyRules.TravelLimit(stats,from,i==1),b=EcologyRules.TravelLimit(stats,to,i==1),difference=b-a;
                if(Mathf.Abs(difference)<.05f)continue;
                if(result.Length>0)result+="\n";
                string format=Mathf.Round(a*10)==Mathf.Round(b*10)?"0.00":"0.0";
                result+=(i==0?"Walk":"Sprint")+" limit: "+a.ToString(format,System.Globalization.CultureInfo.InvariantCulture)+" → "+b.ToString(format,System.Globalization.CultureInfo.InvariantCulture)+" m/s ("+(difference>0?"+":"")+difference.ToString(format,System.Globalization.CultureInfo.InvariantCulture)+")";}
            return result;
        }
    }
}
